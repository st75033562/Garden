using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Robomation
{
    public class RobotManager : Singleton<RobotManager>, IRobotManager
    {
        private static readonly A8.Logger s_logger = A8.Logger.GetLogger<RobotManager>();

        [Serializable]
        private class RobotData
        {
            public string name;
            public string uuid;
            //public bool enabled = true;
        }

        public event Action<bool>         onInitialized;

        // robot was discovered
        public event Action<Robot> onRobotDiscovered;

        public event Action<Robot> onRobotStateChanged;

        public event Action<Robot> onRobotRemoved;

        public event Action<bool>  onConnectionEnabled;
        public event Action<bool>  onReset;

        public enum State
        {
            Invalid,
            Initializing,
            Initialized,
            Resetting
        }

        public enum Error
        {
            None,
            NotReady,     // when resetting or not initialized
            NoConnection, // when connection is not enabled
            Failed
        }

        private const string robotSaveData = "robots.dat";

        private readonly List<Robot> mRobots = new List<Robot>();
		private List<RobotData> mLocalRobotData = new List<RobotData>();

        private bool mRobotDataDirty;

        private State mState = State.Invalid;
        private readonly IRobotConnection mConnection;

        private RobotManager()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            mConnection = new Standalone.RobotConnectionSerial(this);
#elif UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX
            mConnection = new BLE.RobotConnectionBLE(this);
#else
            mConnection = new RobotConnectionNull();
#endif
        }

        // initialize the manager
        // true if initialized successfully, false if any error occurred
        public void initialize()
        {
            if (mState != State.Invalid)
            {
                throw new InvalidOperationException("Already initialized");
            }

            loadRobots();

            mState = State.Initializing;
            mConnection.initialize(success => {
                if (mState == State.Initializing)
                {
                    if (success)
                    {
                        mState = State.Initialized;
                        if (onInitialized != null)
                        {
                            onInitialized(true);
                        }
                    }
                    else
                    {
                        mState = State.Invalid;
                        if (onInitialized != null)
                        {
                            onInitialized(false);
                        }
                    }
                }
                else
                {
                    s_logger.Log("not initializing");
                }
            });
        }

        public void uninitialize()
        {
            if (mState != State.Invalid)
            {
                StopAllCoroutines();
                mConnection.uninitialize();
                mRobots.Clear();
                mState = State.Invalid;
            }
        }

        public bool enableConnection()
        {
            return mConnection.enable();
        }

        public bool isConnectionEnabled
        {
            get { return mConnection.enabled; }
        }

        internal void connectionEnabled(bool enabled)
        {
            if (onConnectionEnabled != null)
            {
                onConnectionEnabled(enabled);
            }
        }

        public State state
        {
            get { return mState; }
        }

        public IEnumerable<Robot> robots
        {
            get { return mRobots; }
        }

        // return null if index is invalid
        public Robot get(int index)
        {
            if (index >= 0 && index < mRobots.Count)
            {
                return mRobots[index];
            }
            return null;
        }


        #region IRobotManager

        public int robotCount
        {
            get { return mRobots.Count; }
        }


        IRobot IRobotManager.get(int index)
        {
            return get(index);
        }

        public IEnumerator<IRobot> GetEnumerator()
        {
            return mRobots.Cast<IRobot>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public Robot find(string uniqueId)
        {
            return mRobots.Find(x => x.uniqueId == uniqueId);
        }

        internal void didDiscoverRobot(Robot robot)
        {
            if (find(robot.uniqueId) != null)
            {
                s_logger.Log("robot already discovered: " + robot.uniqueId);
                return;
            }

			for (int i = 0; i < mLocalRobotData.Count; ++i)
			{
				if (mLocalRobotData[i].uuid == robot.uniqueId)
				{
					robot.setName(mLocalRobotData[i].name);
					break;
				}
			}
            mRobots.Add(robot);
            robot.onConnectionStateChanged += onRobotConnectionStateChanged;
            robot.connect();

            if (onRobotDiscovered != null)
            {
                onRobotDiscovered(robot);
            }
        }

        private void onRobotConnectionStateChanged(Robot robot, ConnectionState state)
        {
            if (onRobotStateChanged != null)
            {
                onRobotStateChanged(robot);
            }
        }

        // remove the robot from the manager
        // if the robot is connected, it will then be disconnected
        public void remove(Robot robot)
        {
            if (robot == null)
            {
                throw new ArgumentNullException();
            }

            int index = mRobots.IndexOf(robot);
            if (index == -1)
            {
                throw new ArgumentException("invalid robot");
            }

            mRobots.RemoveAt(index);
            robot.disconnect();

            mConnection.onRobotRemoved(robot);
            if (onRobotRemoved != null)
            {
                onRobotRemoved(robot);
            }
        }

        // scan for new robots, stops when a new device is found.
        // the new device will be connected automatically
        // return true if scan is started
        public Error startScan()
        {
            if (mState == State.Initialized)
            {
                return mConnection.startScan();
            }
            else
            {
                s_logger.LogError("cannot start scan: " + mState);
                return Error.NotReady;
            }
        }

        public void stopScan()
        {
            if (mState == State.Initialized)
            {
                //s_logger.Log("stop scanning");
                mConnection.stopScan();
            }
            else
            {
                s_logger.LogError("stop scan failed: " + mState);
            }
        }

        public bool scanning
        {
            get { return mConnection.scanning; }
        }

        /// <summary>
        /// reset the system, can only be called after being initialized
        /// after resetting, 
        ///    1. robots are removed
        ///    2. scanning is stopped
        /// </summary>
        public void reset()
        {
            if (mState < State.Initialized)
            {
                s_logger.LogError("not initialized");
                return;
            }

            if (mState == State.Resetting)
            {
                s_logger.Log("already resetting");
                return;
            }

            mState = State.Resetting;
            mRobots.Clear();
            stopScan();

            mConnection.reset(success => {
                if (mState == State.Resetting)
                {
                    mState = State.Initialized;
                }
                if (onReset != null)
                {
                    onReset(success);
                }
            });
        }

        // request connecting to the robot
        internal void connectRobot(Robot robot, bool reconnect = false)
        {
            if (robot == null)
            {
                throw new ArgumentNullException();
            }

            if (!mRobots.Contains(robot))
            {
                throw new InvalidOperationException("invalid robot");
            }

            if (mState < State.Initialized)
            {
                return;
            }

            if (robot.getConnectionState() == ConnectionState.Disconnected)
            {
                robot.setConnectionState(ConnectionState.Connecting);
                mConnection.connect(robot, reconnect);
            }
            else
            {
                s_logger.LogError("cannot connect to robot {0}, state {1}", robot.uniqueId, robot.getConnectionState());
            }
        }

        internal void didConnectRobot(Robot robot)
        {
            robot.setConnectionState(ConnectionState.Connected);

            if (find(robot.uniqueId) != null)
            {
                if (!robot.isEnabled)
                {
                    robot.disconnect();
                }
            }
            else
            {
                //s_logger.LogError("invalid robot: " + robot.uniqueId);
            }
        }

        internal void disconnectRobot(Robot robot)
        {
            if (robot == null)
            {
                throw new ArgumentNullException();
            }

            if (mState < State.Initialized)
            {
                return;
            }

            if (robot.getConnectionState() <= ConnectionState.Connected)
            {
                robot.setConnectionState(ConnectionState.Disconnecting);
                mConnection.disconnect(robot);
            }
            else
            {
                //s_logger.LogError("cannot disconnect from robot {0}, state {1}", robot.uniqueId, robot.getConnectionState());
            }
        }

        internal void didDisconnectRobot(Robot robot)
        {
            robot.setConnectionState(ConnectionState.Disconnected);
            if (find(robot.uniqueId) != null)
            {
                tryReconnect(robot);
            }
        }

        internal void tryReconnect(Robot robot)
        {
            if (robot.isEnabled && robot.autoReconnect &&
                robot.getConnectionState() == ConnectionState.Disconnected)
            {
                connectRobot(robot, true);
            }
        }

        // save all internal states
        public void save()
        {
            saveRobots();
        }

        internal void setChanged(Robot robot)
        {
            mRobotDataDirty = true;
        }

        private string saveDataPath
        {
            get { return Application.persistentDataPath + "/" + robotSaveData; }
        }

        private void saveRobots()
        {
            if (!mRobotDataDirty)
            {
                return;
            }

            try
            {
				FileUtils.createParentDirectory(saveDataPath);
                File.WriteAllText(saveDataPath, ListSerializer<RobotData>.Serialize(mLocalRobotData));

                mRobotDataDirty = false;

                s_logger.Log("saved robot data: " + saveDataPath);
            }
            catch (IOException e)
            {
                s_logger.LogError("error saving robot data {0}: {1}", saveDataPath, e.Message);
            }
            catch (Exception e)
            {
                s_logger.LogException(e);
            }
        }

        private void loadRobots()
        {
            try
            {
                if (File.Exists(saveDataPath))
                {
                    mLocalRobotData = ListSerializer<RobotData>.Deserialize(File.ReadAllText(saveDataPath));
                }
                else
                {
                    //s_logger.Log("save data not found at: " + saveDataPath);
                }
            }
            catch (IOException e)
            {
                s_logger.LogError("error reading {0}: {1}", saveDataPath, e.Message);
            }
            catch (Exception e)
            {
                s_logger.LogException(e);
            }
        }

		public void removeAllRobots()
		{
			for(int i = mRobots.Count - 1; i >= 0; --i)
			{
                remove(mRobots[i]);
			}
		}

        public void changeRobotName(string uuid, string name)
        {
            var robot = find(uuid);
            if (robot != null)
            {
                robot.setName(name);
            }
        }

		internal void changeRobotName(Robot robot, string name)
		{
            var robotData = mLocalRobotData.Find(x => x.uuid == robot.uniqueId);
			if (robotData != null)
			{
                robotData.name = name;
			}
            else
            {
				RobotData newData = new RobotData();
				newData.uuid = robot.uniqueId;
				newData.name = name;
				mLocalRobotData.Add(newData);
            }

			mRobotDataDirty = true;

			saveRobots();
        }

        public object connection
        {
            get { return mConnection; }
        }

        void Update()
        {
            if (state >= State.Initialized)
            {
                mConnection.update();
            }
        }

        void OnDestroy()
        {
            uninitialize();
        }
    }
}