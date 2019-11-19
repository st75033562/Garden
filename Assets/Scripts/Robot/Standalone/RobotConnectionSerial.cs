#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
//#define DEBUG

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using System.Runtime.InteropServices;
using System.IO;

namespace Robomation.Standalone
{
    // connection to robots using a USB dongle
    // TODO: take into account the enabled state of robot
    class RobotConnectionSerial : IRobotConnection
    {
        private static readonly A8.Logger s_logger = A8.Logger.GetLogger<RobotConnectionSerial>();

        private readonly RobotManager m_manager;
        private bool m_scanning;

        /// <summary>
        /// represent a physical connection, i.e. a dongle
        /// one dongle can only connect to one robot
        /// </summary>
        private class Connection : IDisposable
        {
            public volatile bool active; // active for processing in main thread
            // the threshold when the connection is considered lost
            private const float ReconnectThreshold = 1.0f;

            private CRobot m_robotHandle;
            private Robot m_robot;

            // can be null, in case we're creating a new one
            public CRobot robotHandle
            {
                get { return m_robotHandle; }
                set
                {
                    if (m_robotHandle != null)
                    {
                        m_robotHandle.onSensoryDataUpdated = null;
                    }
                    m_robotHandle = value;
                    if (value != null)
                    {
                        value.onSensoryDataUpdated = onSensoryData;
                    }
                }
            }

            public float timeSinceDisconnected = -1;
            // non null if connection has been made
            public Robot robot
            {
                get { return m_robot; }
                set
                {
                    m_robot = value;
                    if (value != null)
                    {
                        lock (sensoryBuffer)
                        {
                            Array.Clear(sensoryBuffer, 0, sensoryBuffer.Length);
                        }
                    }
                }
            }

            public readonly byte[] sensoryBuffer = new byte[CRobot.PacketSize];

            public bool isConnectionLost { get { return timeSinceDisconnected >= ReconnectThreshold; } }

            public int id; // connection id

            private static int s_nextId;

            public Connection()
            {
                id = s_nextId++;
            }

            public void update()
            {
                bool wasConnected = robotHandle.connected;
                robotHandle.updateState();

                if (robotHandle.connected && !wasConnected)
                {
                    resetDisconnectionTimer();
                }
                else if (!robotHandle.connected && wasConnected)
                {
                    startDisconnectionTimer();
                }

                if (timeSinceDisconnected >= 0)
                {
                    timeSinceDisconnected += Time.unscaledDeltaTime;
                }
            }

            public void startDisconnectionTimer()
            {
                timeSinceDisconnected = 0;
            }

            public void resetDisconnectionTimer()
            {
                timeSinceDisconnected = -1;
            }

            public void unlinkRobot()
            {
                if (robot != null)
                {
                    s_logger.Log("unlink robot {0}, conn {1}", robot.uniqueId, id);

                    var curRobot = robot;
                    robot = null;
                    curRobot.setConnectionState(ConnectionState.Disconnected);
                }
            }

            private void onSensoryData(byte[] buffer)
            {
                var robot = this.robot;
                if (robot != null && buffer[0] == robot.sensoryPacketType)
                {
                    lock (sensoryBuffer)
                    {
                        Array.Copy(buffer, sensoryBuffer, buffer.Length);
                    }
                }
            }

            public void sendReceiveData()
            {
                if (robot != null && robot.getConnectionState() == ConnectionState.Connected)
                {
                    lock (sensoryBuffer)
                    {
                        robot.decodeSensoryData(sensoryBuffer);
                    }
                    if (robotHandle.isMotoringDataSent)
                    {
                        robotHandle.writeMotoringData(robot.encodeMotoringData());
                        robot.updateDeviceStates();
                    }
                }
            }

            public void Dispose()
            {
                if (robotHandle != null)
                {
                    var handle = robotHandle;
                    robotHandle = null;
                    handle.Dispose();
                }
            }
        }

        private readonly TaskQueue m_taskQueue = new TaskQueue(1);
        private readonly List<Connection> m_connections = new List<Connection>();
        private readonly List<DongleNotification> m_notifications = new List<DongleNotification>();
        private GCHandle m_logCallbackHandle;

        static RobotConnectionSerial()
        {
            RobotApi.Register((int)RobotType.Hamster, Hamster.NAME, Hamster.PRODUCT_ID, Hamster.SENSORY_PACKET_TYPE);
            RobotApi.Register((int)RobotType.CheeseStick, CheeseStick.NAME, CheeseStick.PRODUCT_ID, 
                CheeseStick.SENSORY_PACKET_TYPE);
        }

        static void RobotLogCallback(string log)
        {
            s_logger.Log("<color=red>Roboid API</color> {0}", log);
        }

        public RobotConnectionSerial(RobotManager manager)
        {
            m_manager = manager;
        }

        public void initialize(Action<bool> onInitialized)
        {
            Dongle.Init();

            m_logCallbackHandle = GCHandle.Alloc((RobotApi.LogCallback)RobotLogCallback);
            RobotApi.SetLogCallback((RobotApi.LogCallback)m_logCallbackHandle.Target);

            // creating a robot can take a long time, move creation to another thread
            m_taskQueue.add(() => {
                foreach (var port in RobotApi.GetPorts())
                {
                    try
                    {
                        m_connections.Add(new Connection {
                            active = true,
                            robotHandle = new CRobot(port)
                        });
                    }
                    catch (ConnectionNotAvailableException)
                    {
                        s_logger.LogError("port {0} not available", port);
                    }
                }

                CallbackQueue.instance.Enqueue(() => {
                    onInitialized(true);
                });
            });
        }

        public void uninitialize()
        {
#if UNITY_EDITOR
            // underlying thread pool might have been destroyed by unity, so wait for a short time,
            // otherwise we can be deadlock
            if (!m_taskQueue.wait(1000))
            {
                s_logger.Log("timed out when waiting for all tasks to be finished");
            }
#else
            m_taskQueue.wait();
#endif

            foreach (var conn in m_connections)
            {
                conn.Dispose();
            }
            m_connections.Clear();
            m_logCallbackHandle.Free();
            Dongle.Uninit();
        }

        public void reset(Action<bool> onReset)
        {
            throw new NotImplementedException();
        }

        public RobotManager.Error startScan()
        {
            m_scanning = true;
            return RobotManager.Error.None;
        }

        public void stopScan()
        {
            m_scanning = false;
        }

        public bool scanning
        {
            get { return m_scanning; }
        }

        public void connect(Robot robot, bool reconnect)
        {
            var conn = findConnection(robot);
            if (conn != null)
            {
                m_manager.didConnectRobot(robot);
            }
        }

        public void disconnect(Robot robot)
        {
            var conn = findConnection(robot);
            if (conn != null)
            {
                conn.unlinkRobot();
            }
            m_manager.didDisconnectRobot(robot);
        }

        public bool enable()
        {
            // make async
            m_manager.StartCoroutine(enableImpl());
            return true;
        }

        private IEnumerator enableImpl()
        {
            yield return null;
            m_manager.connectionEnabled(true);
        }

        public bool enabled
        {
            get { return true; }
        }

        private Connection findConnection(Robot robot)
        {
            return m_connections.Find(x => x.robot == robot);
        }

        public void onRobotRemoved(Robot robot)
        {
            var conn = findConnection(robot);
            if (conn != null)
            {
                conn.unlinkRobot();
                s_logger.Log("robot: {0} was removed, conn: {1}", robot.uniqueId, conn.id);
            }
        }

        public void update()
        {
            updateConnections();
            checkForDongleNotifications();
        }

        private void updateConnections()
        {
            for (int i = 0; i < m_connections.Count; ++i)
            {
                var conn = m_connections[i];
                if (!conn.active)
                {
                    continue;
                }

                // remove invalid connection
                if (conn.robotHandle == null)
                {
                    m_connections.RemoveAt(i);
                    --i;
                }

                conn.update();

                if (m_scanning)
                {
                    checkNewRobot(conn);
                }

                checkConnectionState(conn);
                conn.sendReceiveData();
            }
        }

        private void checkForDongleNotifications()
        {
            Dongle.GetNotifications(m_notifications);

            // consolidate notifications
            for (int i = 0; i < m_notifications.Count; ++i)
            {
                if (m_notifications[i].type == DongleNotificationType.Insertion)
                {
                    var removeNotifIndex = m_notifications.FindIndex(
                        i + 1,
                        x => x.portName == m_notifications[i].portName &&
                             x.type == DongleNotificationType.Removal);
                    if (removeNotifIndex != -1)
                    {
                        m_notifications.RemoveAt(removeNotifIndex);
                        m_notifications.RemoveAt(i);
                        --i;
                    }
                }
            }

            foreach (var notif in m_notifications)
            {
                var conn = m_connections.Find(x => x.robotHandle.portName == notif.portName);
                if (notif.type == DongleNotificationType.Insertion)
                {
                    // reset existing connection if any
                    if (conn != null)
                    {
                        resetConnection(conn);
                    }
                    else
                    {
                        var newConn = new Connection();
                        m_connections.Add(newConn);
                        resetRobotHandle(newConn, notif.portName, () => {
                            s_logger.Log("dongle {0} inserted", notif.portName);
                        });
                    }
                }
                else if (conn != null)
                {
                    s_logger.Log("dongle {0} removed", conn.robotHandle.portName);

                    disconnectRobot(conn);
                    conn.robotHandle.Dispose();
                    m_connections.Remove(conn);
                }
            }
            m_notifications.Clear();
        }

        private void checkNewRobot(Connection conn)
        {
            if (conn.robotHandle.connected)
            {
                string address = conn.robotHandle.address;

                // robot still connected
                if (conn.robot != null && address == conn.robot.uniqueId)
                {
                    return;
                }

                if (conn.robot != null)
                {
                    // robot changed for the same dongle, clear the old link if any
                    Assert.IsNull(m_connections.FirstOrDefault(x => x.robot == conn.robot), "duplicate robot");

                    var oldRobot = conn.robot;
                    conn.unlinkRobot();
                    m_manager.didDisconnectRobot(oldRobot);
                }

                if (!linkIdleRobot(conn))
                {
                    addNewRobot(conn);
                }
            }
        }

        private void addNewRobot(Connection conn)
        {
            switch (conn.robotHandle.type)
            {
            case CRobotType.Hamster:
                conn.robot = new HamsterRobot(m_manager, conn.robotHandle.address, true);
                break;

            case CRobotType.CheeseStick:
                conn.robot = new CheeseStickRobot(m_manager, conn.robotHandle.address, true);
                break;

            default:
                s_logger.LogError("unknown robot type");
                return;
            }

            m_manager.didDiscoverRobot(conn.robot);

            s_logger.Log("found a new robot: {0}, conn: {1}", conn.robot.uniqueId, conn.id);
        }

        private void checkConnectionState(Connection conn)
        {
            if (conn.isConnectionLost && conn.robot != null)
            {
                disconnectRobot(conn);
            }
            else if (conn.robotHandle.connected && conn.robot == null)
            {
                linkIdleRobot(conn);
            }
        }

        private void resetConnection(Connection conn)
        {
            var oldRobot = conn.robot;
            disconnectRobot(conn);

            //s_logger.Log("connection {0} lost, robot: {1}, reconnecting", conn.id, conn.hamster.address);

            // recreate the robot so that we can connect to
            resetRobotHandle(conn, conn.robotHandle.portName, () => {
                s_logger.Log("reset hamster for robot: {0}, conn: {1}",
                    oldRobot != null ? oldRobot.uniqueId : conn.robotHandle.address, conn.id);
            });
        }

        private void disconnectRobot(Connection conn)
        {
            var oldRobot = conn.robot;
            if (conn.robot != null)
            {
                conn.unlinkRobot();
                m_manager.didDisconnectRobot(oldRobot);
            }
        }

        private void resetRobotHandle(Connection conn, string portName, Action callback)
        {
            conn.active = false;

            m_taskQueue.add(() => {
                conn.resetDisconnectionTimer();
                if (conn.robotHandle != null)
                {
                    conn.robotHandle.Dispose();
                    conn.robotHandle = null;
                }

                try
                {
                    conn.robotHandle = new CRobot(portName);
                    if (callback != null)
                    {
                        callback();
                    }
                }
                catch (ConnectionNotAvailableException)
                {
                    s_logger.LogError("unable to connect to " + conn.robotHandle.portName);
                }
                finally
                {
                    conn.active = true;
                }
            });
        }

        private bool linkIdleRobot(Connection conn)
        {
            // relink if the robot was connected 
            var robot = m_manager.find(conn.robotHandle.address);
            if (robot != null)
            {
                conn.robot = robot;
                m_manager.didConnectRobot(conn.robot);

                s_logger.Log("re-established link to robot {0}, conn: {1}", robot.uniqueId, conn.id);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

#endif
