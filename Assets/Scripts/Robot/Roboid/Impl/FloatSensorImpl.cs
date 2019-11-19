using System;

namespace Robomation
{
    /**
     * @author akaii@kw.ac.kr (Kwang-Hyun Park)
     * 
     * ported by alex@g3games.cn
     */
    internal class FloatSensorImpl : FloatDeviceImpl
    {
    	public FloatSensorImpl(int uid, string name, int dataSize, object initialValue, float min, float max)
    		: base(uid, name, dataSize, initialValue, min, max)
    	{
    	}
    	
    	public override DeviceType getDeviceType()
    	{
    		return DeviceType.SENSOR;
    	}
    }
}
