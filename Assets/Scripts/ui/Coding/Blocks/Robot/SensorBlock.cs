using Robomation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorBlock : InsertBlock
{
	SensorPlugins m_Plugins;

	protected override void Start()
	{
		base.Start();

		m_Plugins = GetComponentInChildren<SensorPlugins>();
	}

	public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
	{
		var slotValues = new List<string>();
		yield return Node.GetSlotValues(context, slotValues);
		int index = 0;
		int.TryParse(slotValues[0], out index);
		float sensorValue = 0;
        var robot = CodeContext.robotManager.get(index);
        if (robot != null)
		{
			string sensorType = m_Plugins.GetMenuValue();
			if("sensor_leftProximity" == sensorType)
			{
				sensorValue = robot.read(Hamster.LEFT_PROXIMITY);
            }
			else if("sensor_rightProximity" == sensorType)
			{
				sensorValue = robot.read(Hamster.RIGHT_PROXIMITY);
			}
			else if ("sensor_leftFloor" == sensorType)
			{
				sensorValue = robot.read(Hamster.LEFT_FLOOR);
			}
			else if ("sensor_rightFloor" == sensorType)
			{
				sensorValue = robot.read(Hamster.RIGHT_FLOOR);
			}
			else if ("sensor_xAcceleration" == sensorType)
			{
				sensorValue = robot.read(Hamster.ACCELERATION, Hamster.ACCEL_X);
			}
			else if ("sensor_yAcceleration" == sensorType)
			{
				sensorValue = robot.read(Hamster.ACCELERATION, Hamster.ACCEL_Y);
			}
			else if ("sensor_zAcceleration" == sensorType)
			{
				sensorValue = robot.read(Hamster.ACCELERATION, Hamster.ACCEL_Z);
			}
			else if ("sensor_light" == sensorType)
			{
				sensorValue = robot.read(Hamster.LIGHT);
			}
			else if ("sensor_temperature" == sensorType)
			{
				sensorValue = robot.read(Hamster.TEMPERATURE);
			}
			else if ("sensor_singnalStrength" == sensorType)
			{
				sensorValue = robot.read(Hamster.SIGNAL_STRENGTH);
			}
			else if ("sensor_inputA" == sensorType)
			{
				sensorValue = robot.read(Hamster.INPUT_A);
			}
			else if ("sensor_inputB" == sensorType)
			{
				sensorValue = robot.read(Hamster.INPUT_B);
			}
            else if ("sensor_leftSpeed" == sensorType)
            {
                sensorValue = robot.readFloat(Hamster.LEFT_WHEEL_SPEED);
            }
            else if ("sensor_rightSpeed" == sensorType)
            {
                sensorValue = robot.readFloat(Hamster.RIGHT_WHEEL_SPEED);
            }
            else if ("sensor_leftWheelValue" == sensorType)
            {
                sensorValue = robot.read(Hamster.LEFT_WHEEL);
            }
            else if ("sensor_rightWheelValue" == sensorType)
            {
                sensorValue = robot.read(Hamster.RIGHT_WHEEL);
                
            }
		}
		retValue.value = sensorValue.ToString();
	}
}
