using UnityEngine;
using System.Collections;

public class PhoneSensorBlock : InsertBlock
{
	DownMenuPlugins m_Menu;

	protected override void Start()
	{
		base.Start();
		
		m_Menu = GetComponentInChildren<DownMenuPlugins>();
	}

	public override IEnumerator GetNodeReturnValue(ThreadContext context, ValueWrapper<string> retValue)
	{
		float mA = 0;
		retValue.value = Calculate(m_Menu.GetMenuValue(), mA);
        yield break;
	}

	public string Calculate(string method, float val)
	{
		Vector3 mAcc = Input.acceleration * 9.81f;
		if (method.Equals("phone_sensor_x_acceleration"))
		{
			return mAcc.x.ToString("F6");
		}
		else if (method.Equals("phone_sensor_y_acceleration"))
		{
			return mAcc.y.ToString("F6");
		}
		else if (method.Equals("phone_sensor_z_acceleration"))
		{
			return mAcc.z.ToString("F6");
		}
		else if (method.Equals("phone_sensor_x_angular_speed"))
		{
			return Input.gyro.rotationRate.x.ToString("F6");
		}
		else if (method.Equals("phone_sensor_y_angular_speed"))
		{
			return Input.gyro.rotationRate.y.ToString("F6");
		}
		else if (method.Equals("phone_sensor_z_angular_speed"))
		{
			return Input.gyro.rotationRate.z.ToString("F6");
		}
		return "0";
	}
}
