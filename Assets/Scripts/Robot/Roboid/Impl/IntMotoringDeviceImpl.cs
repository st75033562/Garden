namespace Robomation
{
    /**
     * @author akaii@kw.ac.kr (Kwang-Hyun Park)
     * 
     * ported by alex@g3games.cn
     */
    internal abstract class IntMotoringDeviceImpl : IntDeviceImpl
    {

    	public IntMotoringDeviceImpl(int uid, string name, int dataSize, object initialValue, int minValue, int maxValue)
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
    		return write((int)data);
    	}

    	public override bool writeFloat(int index, float data)
    	{
    		return write(index, (int)data);
    	}

    	public override int writeFloat(float[] data)
    	{
            return putFloat(data);
    	}
    }
}
