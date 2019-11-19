#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

using UnityEngine;
using System.Collections;

namespace Robomation.Standalone.Tests
{
    public class TestConnection : MonoBehaviour
    {
        IEnumerator Start()
        {
            const float TimeOut = 10.0f;

            Debug.Log("connecting to roboid");

            using (var robot = new CRobot())
            {
                var motoringPacket = HamsterEffectorPacket.Create();
                motoringPacket.command = HamsterEffectorPacket.CommandMotoring;

                float remainingTime = TimeOut;
                while (!robot.connected && remainingTime >= 0)
                {
                    robot.updateState();
                    yield return new WaitForSeconds(0.1f);
                    remainingTime -= 0.1f;
                }

                if (robot.connected)
                {
                    Debug.LogFormat("connected {0}", robot.address);

                    motoringPacket.leftWheel = motoringPacket.rightWheel = 30;
                    robot.writeMotoringData(motoringPacket.data);

                    yield return new WaitForSeconds(10);
                }
                else
                {
                    Debug.LogFormat("connection timed out");
                }
            }
        }
    }
}

#endif
