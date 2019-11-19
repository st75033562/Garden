using Gameboard.Script;
using Google.Protobuf;
using Networking;
using Robomation;
using System;
using UnityEngine;

namespace Gameboard
{
    class RobotSensorNotifier
    {
        private HamsterSensorPacket m_packet = HamsterSensorPacket.Create();
        private readonly SensorNotification m_notification = new SensorNotification();
        private int m_maxRobotNum;

        public ClientConnection connection { get; set; }

        public IRobotManager robotManager { get; set; } 

        /// <summary>
        /// maximum number of robots to notify
        /// </summary>
        public void SetMaxRobotNum(int num)
        {
            if (num < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            m_maxRobotNum = num;
        }

        public void Reset()
        {
            m_maxRobotNum = 0;
            connection = null;
        }

        public void Notify()
        {
            if (connection != null && robotManager != null)
            {
                int num = Mathf.Min(robotManager.robotCount, m_maxRobotNum);
                for (int i = 0; i < num; ++i)
                {
                    var robot = robotManager.get(i);
                    ReadRobotSensorData(robot);
                    m_notification.RobotIndex = i;
                    m_notification.Data = ByteString.CopyFrom(m_packet.data);
                    connection.Send(CommandId.CmdSensorNotification, m_notification);
                }
            }
        }

        private void ReadRobotSensorData(IRobot robot)
        {
            m_packet.leftProximity = robot.read(Hamster.LEFT_PROXIMITY);
            m_packet.rightProximity = robot.read(Hamster.RIGHT_PROXIMITY);
            m_packet.leftFloor = robot.read(Hamster.LEFT_FLOOR);
            m_packet.rightFloor = robot.read(Hamster.RIGHT_FLOOR);
        }
    }
}
