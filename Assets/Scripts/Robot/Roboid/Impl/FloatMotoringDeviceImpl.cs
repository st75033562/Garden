namespace Robomation
{
    /**
     * @author akaii@kw.ac.kr (Kwang-Hyun Park)
     * 
     * ported by alex@g3games.cn
     */
    internal abstract class FloatMotoringDeviceImpl : FloatDeviceImpl
    {
    	public FloatMotoringDeviceImpl(int uid, string name, int dataSize, object initialValue, float minValue, float maxValue)
    		: base(uid, name, dataSize, initialValue, minValue, maxValue)
    	{
    	}
    	
    	public override bool write(int data)
    	{
            return put(data);
    	}

    	public override bool write(int index, int data)
    	{
            return put(index, data);
    	}

    	public override int write(int[] data)
    	{
            return put(data);
    	}

    	public override bool writeFloat(float data)
    	{
            return putFloat(data);
    	}

    	public override bool writeFloat(int index, float data)
    	{
            return putFloat(index, data);
    	}

    	public override int writeFloat(float[] data)
    	{
            return putFloat(data);
    	}
    }
}
