using System;

namespace Robomation
{
    /**
     * @author akaii@kw.ac.kr (Kwang-Hyun Park)
     * 
     * ported by alex@g3games.cn
     */
    internal abstract class FloatDeviceImpl : DeviceImpl
    {
        // private readonly object mReadLock = new object();
    	private float[] mReadData;
        private readonly float mMinValue;
        private readonly float mMaxValue;

    	public FloatDeviceImpl(int uid, string name, int dataSize, object initialValue, float minValue, float maxValue)
            : base(uid, name, dataSize, initialValue)
    	{
    		if(dataSize < 0) return;
    		
    		mReadData = new float[dataSize];
            mMinValue = minValue;
            mMaxValue = maxValue;
            reset();
    	}
        
    	void fill(float[] data, object value, int start, int end)
    	{
    		if(value is float)
    		{
    			float v = (float)value;
    			for(int i = start; i < end; ++i)
    				data[i] = v;
    		}
    		else if(value is int)
    		{
    			float v = (int)value;
    			for(int i = start; i < end; ++i)
    				data[i] = v;
    		}
    		else if(value is float[])
    		{
    			float[] v = (float[])value;
    			int len = Math.Min(end, v.Length);
    			if(start >= len)
    			{
    				for(int i = start; i < end; ++i)
    					data[i] = 0.0f;
    			}
    			else
    			{
    				Array.Copy(v, start, data, start, len - start);
    				for(int i = len; i < end; ++i)
    					data[i] = 0.0f;
    			}
    		}
    		else if(value is int[])
    		{
    			int[] v = (int[])value;
    			int len = Math.Min(end, v.Length);
    			if(start >= len)
    			{
    				for(int i = start; i < end; ++i)
    					data[i] = 0.0f;
    			}
    			else
    			{
    				for(int i = start; i < len; ++i)
    					data[i] = v[i];
    				for(int i = len; i < end; ++i)
    					data[i] = 0.0f;
    			}
    		}
    	}

    	public override DataType getDataType()
    	{
    		return DataType.FLOAT;
    	}

    	public override int read()
    	{
    		return (int)readFloat();
    	}

    	public override int read(int index)
    	{
    		return (int)readFloat(index);
    	}

    	public override int read(int[] data)
    	{
    		if(data == null) return 0;
    		int datalen = data.Length;
    		if(datalen <= 0) return 0;
    		
    		int len = 0;
    		// lock(mReadLock)
    		{
    			if(mReadData == null) return 0;
    			float[] readData = mReadData;
    			len = Math.Min(readData.Length, datalen);
    			for(int i = 0; i < len; ++i)
    				data[i] = (int)readData[i];
        		for(int i = len; i < datalen; ++i)
        			data[i] = 0;
    		}
    		return len;
    	}

    	public override float readFloat()
    	{
    		// lock(mReadLock)
    		{
    			if(mReadData == null || mReadData.Length <= 0) return 0.0f;
    			return mReadData[0];
    		}
    	}

    	public override float readFloat(int index)
    	{
    		if(index < 0) return 0.0f;
    		// lock(mReadLock)
    		{
    			if(mReadData == null || index >= mReadData.Length) return 0.0f;
    			return mReadData[index];
    		}
    	}

    	public override int readFloat(float[] data)
    	{
    		if(data == null) return 0;
    		int datalen = data.Length;
    		if(datalen <= 0) return 0;
    		
    		int len = 0;
    		// lock(mReadLock)
    		{
    			if(mReadData == null) return 0;
    			len = Math.Min(mReadData.Length, datalen);
    			Array.Copy(mReadData, 0, data, 0, len);
        		for(int i = len; i < datalen; ++i)
        			data[i] = 0.0f;
    		}
    		return len;
    	}
    	
    	public override bool put(int data)
    	{
    		return putFloat((float)data);
    	}
    	
    	public override bool put(int index, int data)
    	{
    		return putFloat(index, (float)data);
    	}
    	
    	public override int put(int[] data)
    	{
    		if(data == null) return 0;
    		int datalen = data.Length;
    		if(datalen <= 0) return 0;

            int len;
    		// lock(mReadLock)
    		{
    			if(mDataSize < 0)
    			{
    				if(mReadData == null || mReadData.Length != datalen)
    					mReadData = new float[datalen];
    			}
    			float[] readData = mReadData;
    			if(readData == null) return 0;
    			int thislen = readData.Length;
    			if(thislen <= 0) return 0;
    			len = Math.Min(thislen, datalen);
    			for(int i = 0; i < len; ++i)
    				readData[i] = Utils.clamp(data[i], mMinValue, mMaxValue);
    			for(int i = len; i < thislen; ++i)
    				readData[i] = 0.0f;
                fire();
    		}
    		return len;
    	}
    	
    	public override bool putFloat(float data)
    	{
    		// lock(mReadLock)
    		{
    			if(mReadData == null || mReadData.Length <= 0)
    			{
    				if(mDataSize < 0)
    					mReadData = new float[1];
    				else
    					return false;
    			}
    			mReadData[0] = Utils.clamp(data, mMinValue, mMaxValue);
                fire();
    		}
    		return true;
    	}

    	public override bool putFloat(int index, float data)
    	{
    		if(index < 0) return false;
    		// lock(mReadLock)
    		{
    			if(mReadData == null)
    			{
    				if(mDataSize < 0)
    				{
    					mReadData = new float[index + 1];
    					fill(mReadData, mInitialValue, 0, index);
    				}
    				else
    					return false;
    			}
    			else if(index >= mReadData.Length)
    			{
    				if(mDataSize < 0)
    				{
    					float[] newData = new float[index + 1];
    					int len = mReadData.Length;
    					Utils.copyClamped(mReadData, 0, newData, 0, len, mMinValue, mMaxValue);
    					fill(newData, mInitialValue, len, index);
    					mReadData = newData;
    				}
    				else
    					return false;
    			}
    			mReadData[index] = data;
                fire();
    		}
    		return true;
    	}

    	public override int putFloat(float[] data)
    	{
    		if(data == null) return 0;
    		int datalen = data.Length;
    		if(datalen <= 0) return 0;

            int len = 0;
    		// lock(mReadLock)
    		{
    			if(mDataSize < 0)
    			{
    				if(mReadData == null || mReadData.Length != datalen)
    					mReadData = new float[datalen];
    			}
    			float[] readData = mReadData;
    			if(readData == null) return 0;
    			int thislen = readData.Length;
    			if(thislen <= 0) return 0;
    			len = Math.Min(thislen, datalen);
    			Utils.copyClamped(data, 0, readData, 0, len, mMinValue, mMaxValue);
    			for(int i = len; i < thislen; ++i)
    				readData[i] = 0.0f;
                fire();
    		}
    		return 0;
    	}

        public override void reset()
        {
            if (mReadData != null)
            {
                fill(mReadData, mInitialValue, 0, mDataSize);
            }
        }
    }
}
