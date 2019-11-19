using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Robomation.BLE
{
    public class RobotConnectionBLE : IRobotConnection, IBLEConnection
    {
        private readonly A8.Logger s_logger = A8.Logger.GetLogger<RobotConnectionBLE>();

        // the minimum required RSSI for a robot to be discovered
        private const int defaultMinRSSI = -45;

        private const float rssiSmoothFactor = 0.5f;
        private const float rssiInitialValue = -80;

        private const string rssiKey = "ble_min_rssi";

        private RequestQueue mRequestQueue;

        private struct DelayedConnection
        {
            public Robot robot;
            public Coroutine coroutine;
        }

        private const float BaseConnectionDelay = 1.0f;
        private const float MaxConnectionDelay = 4.0f;
        private const float ResetTimer = 15.0f;
        private const int MaxDisconnectionTryCount = 1;
        private const int MaxReconnectionCount = 2;

        private readonly List<DelayedConnection> mDelayedConnections = new List<DelayedConnection>();

        private bool mRetrievedPeripherals;
        private bool mScanning;
        private bool mPoweredOn;

        private readonly RobotManager mManager;
        private Action<bool> mOnInitialized;

        private Action<bool> mOnReset;
        private Coroutine mResetTimer;
        private readonly Dictionary<string, ExponentialFilter> mRssiFilters 
            = new Dictionary<string, ExponentialFilter>();
        private readonly Dictionary<Robot, RobotConnectionState> mConnectionStates 
            = new Dictionary<Robot, RobotConnectionState>();

        private int mMinRSSI;

        public RobotConnectionBLE(RobotManager manager)
        {
            mManager = manager;
            mRequestQueue = new RequestQueue();

#if CVAR
            CmdServer.Register("bledq", new DumpQueueCommand(this));
#endif
        }

        // true if initialized successfully, false if BLE is not supported
        public void initialize(Action<bool> onInitialized)
        {
            mMinRSSI = PlayerPrefs.GetInt(rssiKey, defaultMinRSSI);

            mOnInitialized = onInitialized;
            BluetoothLEHardwareInterface.Initialize(true, false,
                () =>
                {
                    s_logger.Log("BLE initialized");
                    onInitialized(true);
                    mOnInitialized = null;
                },
                onBluetoothError,
                onPowerStateChanged);
        }

        private void onBluetoothError(string error, string[] extraInfo)
        {
            s_logger.LogError("BLE error: {0}, {1}", error, string.Join(" ", extraInfo));

            if (error.StartsWith(BluetoothDeviceScript.ErrorBLENotAvailable))
            {
                mOnInitialized(false);
                mOnInitialized = null;
            }
            else if (error.StartsWith(BluetoothDeviceScript.ErrorWriteCharacteristic))
            {
                checkedAck(mRequestQueue, extraInfo[0], RequestType.WriteMotoring);
            }
            else if (error.StartsWith(BluetoothDeviceScript.ErrorUpdateNotificationState))
            {
                checkedAck(mRequestQueue, extraInfo[0], RequestType.Subscribe);
            }
            else if (error.StartsWith(BluetoothDeviceScript.ErrorServiceDiscovery))
            {
                checkedAck(mRequestQueue, extraInfo[0], RequestType.DiscoverService);
            }
        }

        private void onPowerStateChanged(bool on)
        {
            s_logger.Log("power state changed: " + on);

            mPoweredOn = on;
            mManager.connectionEnabled(on);

            if (on)
            {
                if (!mRetrievedPeripherals)
                {
                    mRetrievedPeripherals = true;
                    var identifiers = mManager.robots.Select(x => x.uniqueId).ToArray();
                    if (identifiers.Length > 0)
                    {
                        BluetoothLEHardwareInterface.RetrievePeripheralsWithIdentifiers(identifiers, onRetrievedPeripherals);
                    }
                }

                if (mScanning)
                {
                    startScan();
                }

                if (mOnReset != null)
                {
                    clearResetTimer();
                    mOnReset(true);
                    mOnReset = null;
                }
            }
            else
            {
                mRequestQueue.reset();
                removeDelayedConnections(null);

                foreach (var robot in mManager.robots)
                {
                    mConnectionStates[robot].reset();
                    mManager.didDisconnectRobot(robot);
                }

                if (mOnReset != null)
                {
                    if (!enable())
                    {
                        s_logger.LogError("failed to restart ble");

                        clearResetTimer();
                        mOnReset(false);
                        mOnReset = null;
                    }
                }

                stopScan();
            }
        }

        private void onRetrievedPeripherals()
        {
            foreach (var robot in mManager.robots)
            {
                robot.connect();
            }
        }

        // only valid on Android
        public bool enable()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                return BluetoothLEHardwareInterface.EnableBluetooth(true);
            }

            // on ios we cannot on/off BLE programmatically, this is not considered an error
            return true;
        }

        public bool enabled
        {
            get { return mPoweredOn; }
        }

        public void uninitialize()
        {
            BluetoothLEHardwareInterface.DeInitialize(delegate { });
            mRetrievedPeripherals = false;
            mScanning = false;
            mPoweredOn = false;
            mOnInitialized = null;
            mOnReset = null;
            mResetTimer = null;
            mRequestQueue.reset();
            mDelayedConnections.Clear();
            mRssiFilters.Clear();
        }

        public void reset(Action<bool> onReset)
        {
            mOnReset = onReset;

            mRequestQueue.reset();
            removeDelayedConnections(null);
            mRssiFilters.Clear();

            if (mPoweredOn)
            {
                if (BluetoothLEHardwareInterface.IsEnabled())
                {
                    if (BluetoothLEHardwareInterface.EnableBluetooth(false))
                    {
                        s_logger.Log("disabling ble");
                        mResetTimer = mManager.StartCoroutine(resetTimer());
                    }
                    else
                    {
                        s_logger.LogError("failed to disable ble");
                        onReset(false);
                        mOnReset = null;
                    }
                }
                else
                {
                    onPowerStateChanged(false);
                }
            }
            else if (!enable())
            {
                s_logger.LogError("failed to disable ble");
                onReset(false);
                mOnReset = null;
            }
        }

        IEnumerator resetTimer()
        {
            yield return new WaitForSecondsRealtime(ResetTimer);

            s_logger.LogError("reset timed out");
            if (mOnReset != null)
            {
                mOnReset(false);
                mOnReset = null;
            }
        }

        private void clearResetTimer()
        {
            if (mResetTimer != null)
            {
                mManager.StopCoroutine(mResetTimer);
                mResetTimer = null;
            }
        }

        // scan for new robots, stops when a new device is found.
        // the new device will be connected automatically
        public RobotManager.Error startScan()
        {
            if (!mPoweredOn)
            {
                return RobotManager.Error.NoConnection;
            }

            if (!mScanning)
            {
                s_logger.Log("start scanning");
                bool success = BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(
                    new string[] { BLERobotConfig.AdvertisingServiceUUID },
                    null, onDiscoveredPeripheral, true);
                if (!success)
                {
                    return RobotManager.Error.Failed;
                }
                mScanning = true;
            }

            return RobotManager.Error.None;
        }

        private void onDiscoveredPeripheral(string identifier, string localName, int rssi, byte[] data)
        {
            //s_logger.Log("id: {0}, rssi: {1}", identifier, rssi);

            // there is a small delay before scanning is stopped after calling stopScan
            if (!mScanning) { return; }

            var robot = mManager.find(identifier);
            if (robot != null)
            {
                mManager.tryReconnect(robot);
            }
            else if (rssi >= minRSSI && rssi < 0)
            {
                ExponentialFilter rssiFilter;
                if (!mRssiFilters.TryGetValue(identifier, out rssiFilter))
                {
                    rssiFilter = new ExponentialFilter {
                        smoothFactor = rssiSmoothFactor,
                        value = rssiInitialValue
                    };
                    mRssiFilters.Add(identifier, rssiFilter);
                }
                rssiFilter.addSample(rssi);
                //s_logger.Log("filtered rssi: " + rssiFilter.value);
                if (rssiFilter.value >= minRSSI && !mRequestQueue.hasRequests(identifier))
                {
                    var newRobot = createRobot(localName, identifier);
                    if (newRobot != null)
                    {
                        mManager.didDiscoverRobot(newRobot);
                        mRssiFilters.Remove(identifier);
                    }
                }
            }
        }

        private Robot createRobot(string localName, string identifier)
        {
            Robot robot;
            switch (localName)
            {
            case Hamster.NAME:
                robot = new HamsterRobot(mManager, identifier, true);
                break;

            case CheeseStick.NAME:
                robot = new CheeseStickRobot(mManager, identifier, true);
                break;

            default:
                s_logger.LogWarning("unrecognized robot type {0}", localName);
                return null;
            }

            mConnectionStates.Add(robot, new RobotConnectionState());
            return robot;
        }

        public void stopScan()
        {
            mScanning = false;
            BluetoothLEHardwareInterface.StopScan();
            mRssiFilters.Clear();
            s_logger.Log("stop scanning");
        }

        public bool scanning
        {
            get { return mScanning; }
        }

        // request connecting to the robot
        public void connect(Robot robot, bool reconnet)
        {
            var connectionState = mConnectionStates[robot];
            if (reconnet)
            {
                ++connectionState.reconnectionCount;
            }
            else
            {
                connectionState.reconnectionCount = 0;
            }

            if (connectionState.nextConnectionDelay > 0)
            {
                var coroutine = mManager.StartCoroutine(delayedConnect(robot));
                mDelayedConnections.Add(new DelayedConnection {
                    robot = robot,
                    coroutine = coroutine
                });
            }
            else
            {
                connectionState.nextConnectionDelay = BaseConnectionDelay;
                enqueueRequest(mRequestQueue, new Request {
                    robotId = robot.uniqueId,
                    type = RequestType.Connection
                });
            }
        }

        private IEnumerator delayedConnect(Robot robot)
        {
            var connectionState = mConnectionStates[robot];
            s_logger.Log("delay connection, time: {0}, robot: {1}", connectionState.nextConnectionDelay, robot.uniqueId);

            yield return new WaitForSecondsRealtime(connectionState.nextConnectionDelay);

            connectionState.nextConnectionDelay = Mathf.Clamp(
                connectionState.nextConnectionDelay * 2, 
                BaseConnectionDelay, MaxConnectionDelay);

            int index = mDelayedConnections.FindIndex(x => x.robot == robot);
            mDelayedConnections.RemoveAt(index);

            enqueueRequest(mRequestQueue, new Request {
                robotId = robot.uniqueId,
                type = RequestType.Connection
            });
        }

        private void removeDelayedConnections(string identifier)
        {
            if (identifier != null)
            {
                for (int i = 0; i < mDelayedConnections.Count; ++i)
                {
                    if (mDelayedConnections[i].robot.uniqueId == identifier)
                    {
                        mManager.StopCoroutine(mDelayedConnections[i].coroutine);
                        mDelayedConnections.RemoveAt(i);
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < mDelayedConnections.Count; ++i)
                {
                    mManager.StopCoroutine(mDelayedConnections[i].coroutine);
                }
                mDelayedConnections.Clear();
            }
        }

        private void onConnectedToPeripheral(string identifier)
        {
            if (mRequestQueue.count > 0)
            {
                if (checkedAck(mRequestQueue, identifier, RequestType.Connection))
                {
                    var robot = mManager.find(identifier);
                    if (robot != null)
                    {
                        s_logger.Log("connected to " + identifier);
                        mConnectionStates[robot].reconnectionCount = 0;

                        // service discovering is started automatically, so insert the request for checking timeout
                        var request = new Request {
                            robotId = robot.uniqueId,
                            type = RequestType.DiscoverService,
                        };
                        mRequestQueue.prepend(request);
                        mRequestQueue.busy = true;

                        s_logger.Log(mRequestQueue);

                        if (robot.getConnectionState() == ConnectionState.Connecting)
                        {
                            mManager.didConnectRobot(robot);
                        }
                        else
                        {
                            Assert.IsNotNull(mRequestQueue.find(identifier, RequestType.Disconnection),
                                string.Format("connected to robot {0} state {1} with no Disconnection request", identifier, robot.getConnectionState()));
                        }
                    }
                    else
                    {
                        s_logger.LogError("connected to invalid robot {0}, start disconnection", identifier);
                        disconnectInternal(identifier);
                    }
                }
            }
            else
            {
                s_logger.LogError("got connection response, but connection queue is empty");
            }
        }

        private void onDiscoveredService(string identifier, string service)
        {
            s_logger.Log("discovered service: " + service);

            // no checked ack, since there're several services and we only have one request
            ackRequest(mRequestQueue, identifier, RequestType.DiscoverService);

            var robot = mManager.find(identifier);
            if (robot != null)
            {
                mConnectionStates[robot].nextConnectionDelay = 0;
            }
            else
            {
                s_logger.LogError("discovered service for invalid robot {0}, start disconnection", identifier);
                disconnectInternal(identifier);
            }
        }

        private void onDiscoveredCharacteristic(string identifier, string service, string characteristic)
        {
            var robot = mManager.find(identifier);
            if (robot != null)
            {
                // TODO: check if this will cause bug
                if (!robot.isEnabled)
                {
                    robot.disconnect();
                    return;
                }

                if (characteristic.Equals(BLERobotConfig.TxRxCharacteristicUUID, StringComparison.InvariantCultureIgnoreCase))
                {
#if UNITY_ANDROID
                    enqueueRequest(mRequestQueue, new Request {
                        robotId = robot.uniqueId,
                        type = RequestType.Subscribe
                    });
#else
                    subscribe(robot);
#endif
                    mConnectionStates[robot].canSendMotoringPacket = true;
                }
            }
            else
            {
                s_logger.LogError("discovered characteristic for invalid robot {0}, start disconnection", identifier);
                disconnectInternal(identifier);
            }
        }

        private void onFailedToConnectPeripheral(string identifier)
        {
            var robot = mManager.find(identifier);
            if (robot != null)
            {
                s_logger.LogError("Failed to connect to {0}, retry", identifier);
                mManager.didDisconnectRobot(robot);
            }
            else
            {
                s_logger.LogError("failed to connect to invalid robot: " + identifier);
            }
        }

        public void enqueueMotoringRequest(Robot robot, byte[] data)
        {
            if (robot == null)
            {
                throw new ArgumentNullException("robot");
            }

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

#if UNITY_ANDROID
            enqueueRequest(mRequestQueue, new Request {
                robotId = robot.uniqueId,
                type = RequestType.WriteMotoring,
                writeData = data
            });
#else
            sendPacket(robot, data);
#endif
        }

        public void disconnect(Robot robot)
        {
            // avoid queuing motoring packets
            mConnectionStates[robot].canSendMotoringPacket = false;

            disconnectInternal(robot.uniqueId);
            robot.setConnectionState(ConnectionState.Disconnecting);
        }

        private void disconnectInternal(string robotId)
        {
            removeDelayedConnections(robotId);

            mRequestQueue.removeAllRequests(robotId, true);

            if (!isDisconnecting(robotId))
            {
                enqueueRequest(mRequestQueue, new Request {
                    robotId = robotId,
                    type = RequestType.Disconnection
                });
            }
        }

        private void onDisconnectedFromPeripheral(string identifier)
        {
            s_logger.Log("disconnected " + identifier);

            mRequestQueue.removeAllRequests(identifier);
            removeDelayedConnections(identifier);

            var robot = mManager.find(identifier);
            if (robot != null)
            {
                var connectionState = mConnectionStates[robot];
                // do not reset connection delay
                connectionState.canSendMotoringPacket = false;
                connectionState.disconnectionTryCount = 0;

                if (connectionState.reconnectionCount >= MaxReconnectionCount)
                {
                    robot.autoReconnect = false;
                    s_logger.Log("disable reconnect for robot: " + robot.uniqueId);
                }
                mManager.didDisconnectRobot(robot);
            }
            else
            {
                // TODO: update the state of the removed robot
            }
        }

        public void onRobotRemoved(Robot robot)
        {
            mConnectionStates.Remove(robot);
        }

        public void update()
        {
            // send motoring data for connected robot if no pending request
            foreach (var robot in mManager.robots)
            {
                if (mConnectionStates[robot].canSendMotoringPacket)
                {
#if UNITY_ANDROID
                    if (mRequestQueue.find(robot.uniqueId, RequestType.WriteMotoring) == null)
                    {
                        enqueueRequest(mRequestQueue, new Request {
                            robotId = robot.uniqueId,
                            type = RequestType.WriteMotoring,
                            writeData = robot.encodeMotoringData()
                        });
                        robot.updateDeviceStates();
                    }
#else
                    sendPacket(robot, robot.encodeMotoringData());
                    robot.updateDeviceStates();
#endif
                }
            }

            updateRequestQueue();
        }

        private void updateRequestQueue()
        {
            if (mRequestQueue.updateTimer())
            {
                var request = mRequestQueue.first;
                s_logger.LogError("timed out, request: {0}, robot: {1}", request.type, request.robotId);

                mRequestQueue.dequeueRequest();

                switch (request.type)
                {
                case RequestType.Connection:
                    s_logger.LogError("force disconnection, robot {0}", request.robotId);
                    onDisconnectedFromPeripheral(request.robotId);
                    break;

                case RequestType.DiscoverService:
                case RequestType.Subscribe:
                case RequestType.WriteMotoring:
                {
                    var robot = mManager.find(request.robotId);
                    if (robot != null)
                    {
                        disconnect(robot);
                    }
                    else
                    {
                        disconnectInternal(request.robotId);
                    }
                    break;
                }

                case RequestType.Disconnection:
                {
                    var robot = mManager.find(request.robotId);
                    if (robot != null && mConnectionStates[robot].disconnectionTryCount++ < MaxDisconnectionTryCount)
                    {
                        disconnect(robot);
                    }
                    else
                    {
                        s_logger.LogError("force disconnection, robot {0}", request.robotId);
                        onDisconnectedFromPeripheral(request.robotId);
                    }
                    break;
                }

                default:
                    s_logger.LogError("not handled request type: " + request.type);
                    break;
                }
            }

            if (!mRequestQueue.busy)
            {
                executeNextRequest(mRequestQueue);
            }
        }

        private bool isDisconnecting(string robotId)
        {
            return mRequestQueue.find(robotId, RequestType.Disconnection) != null;
        }

        private void executeNextRequest(RequestQueue queue)
        {
            if (queue.count > 0 && !queue.busy)
            {
                if (mPoweredOn)
                {
                    var request = queue.first;
                    if (request.type <= RequestType.Subscribe)
                    {
                        s_logger.Log("execute {0}, robot: {1}", request.type, request.robotId);
                    }

                    queue.busy = true;
                    executeRequest(request);
                }
                else
                {
                    s_logger.Log("execute when powered off");
                    s_logger.Log(queue);
                }
            }
            else if (!queue.busy)
            {
                //s_logger.Log("execute empty queue: " + queue.name);
            }
            else
            {
                s_logger.Log("execute busy queue");
                s_logger.Log(queue);
            }
        }

        private void enqueueRequest(RequestQueue queue, Request request)
        {
            if (request.type <= RequestType.Subscribe)
            {
                s_logger.Log("enqueue {0}, robot: {1}", request.type, request.robotId);
            }

            queue.append(request);
        }

        private bool ackRequest(RequestQueue queue, string id, RequestType type)
        {
            if (queue.count > 0)
            {
                var request = queue.first;

                if (request.type == type && id == request.robotId)
                {
                    queue.dequeueRequest();
                    return true;
                }
                else
                {
                    s_logger.LogError("remove failed, expect: id {0}, type: {1}, got: {2}, type: {3}",
                        id, type, request.robotId, request.type);
                }
            }
            else
            {
                s_logger.Log("ack empty queue, id {0}, type: {1}", id, type);
            }
            return false;
        }

        private bool checkedAck(RequestQueue queue, string id, RequestType type)
        {
            if (!ackRequest(queue, id, type))
            {
                var robot = mManager.find(id);
                if (robot != null)
                {
                    disconnect(robot);
                }
                else
                {
                    disconnectInternal(id);
                }
                return false;
            }
            return true;
        }

        private void executeRequest(Request request)
        {
            var robot = mManager.find(request.robotId);
            if (robot == null && request.type != RequestType.Disconnection)
            {
                s_logger.LogError("invalid robot {0} while executing request {1}", request.robotId, request.type);
                return;
            }

            switch (request.type)
            {
            case RequestType.Connection:
                BluetoothLEHardwareInterface.ConnectToPeripheral(
                    robot.uniqueId,
                    onConnectedToPeripheral,
                    onDiscoveredService,
                    onDiscoveredCharacteristic,
                    onDisconnectedFromPeripheral);
                break;

            case RequestType.Disconnection:
                if (robot != null)
                {
                    mConnectionStates[robot].canSendMotoringPacket = false;
                }
                BluetoothLEHardwareInterface.DisconnectPeripheral(request.robotId, onDisconnectedFromPeripheral);
                break;

            case RequestType.Subscribe:
                subscribe(robot);
                break;

            case RequestType.WriteMotoring:
                sendPacket(robot, request.writeData);
                break;

            default:
                throw new ArgumentException();
            }
        }

        private void sendPacket(Robot robot, byte[] data)
        {
            // even with `write without response', callback will still be called on
            // android, see https://stackoverflow.com/a/43744888/381646
            BluetoothLEHardwareInterface.WriteCharacteristicWithDeviceAddress(
                robot.uniqueId,
                BLERobotConfig.SensorServiceUUID,
                BLERobotConfig.TxRxCharacteristicUUID,
                data, data.Length, 
                false,
                onWriteCharacteristic);
        }

        private void subscribe(Robot robot)
        {
            BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(
                robot.uniqueId, BLERobotConfig.SensorServiceUUID, BLERobotConfig.TxRxCharacteristicUUID,
#if UNITY_ANDROID
                onSubscribedCharacteristic,
#else
                null,
#endif
                (id, characteristicId, data) => {
                    if (data[0] == robot.sensoryPacketType)
                    {
                        robot.decodeSensoryData(data);
                    }
                });
        }

        private void onSubscribedCharacteristic(string id, string characteristic)
        {
            checkedAck(mRequestQueue, id, RequestType.Subscribe);
        }

        private void onWriteCharacteristic(string id, string characteristic)
        {
            checkedAck(mRequestQueue, id, RequestType.WriteMotoring);
        }

        // debugging
        public string dumpRequestQueue()
        {
            return mRequestQueue.ToString();
        }

        public int minRSSI
        {
            get { return mMinRSSI; }
            set
            {
                mMinRSSI = Mathf.Min(-1, value);
                PlayerPrefs.SetInt(rssiKey, mMinRSSI);
            }
        }
    }
}
