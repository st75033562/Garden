using System;

namespace Robomation
{

    /**
     * @author akaii@kw.ac.kr (Kwang-Hyun Park)
     * 
     * ported by alex@g3games.cn
     */
    internal abstract class IntDeviceImpl : DeviceImpl
    {
        // private readonly object mReadLock = new object();
    	private int[] mReadData;
        private readonly int mMinValue;
        private readonly int mMaxValue;
    	
    	public IntDeviceImpl(int uid, string name, int dataSize, object initialValue, int minValue, int maxValue)
            : base(uid, name, dataSize, initialValue)
    	{
    		if(dataSize < 0) return;
    		
    		mReadData = new int[dataSize];
            mMinValue = minValue;
            mMaxValue = maxValue;
            reset();
    	}
    	
    	void fill(int[] data, object value, int start, int end)
    	{
    		if (value is int)
    		{
                int v = (int)value;
    			for(int i = start; i < end; ++i)
    				data[i] = v;
    		}
    		else if(value is float)
    		{
                int v = (int)(float)value;
    			for(int i = start; i < end; ++i)
    				data[i] = v;
    		}
    		else if(value is int[])
    		{
    			int[] v = (int[])value;
    			int len = Math.Min(end, v.Length);
    			if(start >= len)
    			{
    				for(int i = start; i < end; ++i)
    					data[i] = 0;
    			}
    			else
    			{
    				Array.Copy(v, start, data, start, len - start);
    				for(int i = len; i < end; ++i)
    					data[i] = 0;
    			}
    		}
    		else if(value is float[])
    		{
    			float[] v = (float[])value;
    			int len = Math.Min(end, v.Length);
    			if(start >= len)
    			{
    				for(int i = start; i < end; ++i)
    					data[i] = 0;
    			}
    			else
    			{
    				for(int i = start; i < len; ++i)
    					data[i] = (int)v[i];
    				for(int i = len; i < end; ++i)
    					data[i] = 0;
    			}
    		}
    	}
    	
    	public override DataType getDataType()
    	{
    		return DataType.INTEGER;
    	}
    	
    	public override int read()
        {
            if (mReadData == null || mReadData.Length <= 0) return 0;
            // lock (mReadLock)
            {
                return mReadData[0];
            }
        }

    	public override int read(int index)
        {
            if (index < 0) return 0;
            if (mReadData == null || index >= mReadData.Length) return 0;
            // lock (mReadLock)
            {
                return mReadData[index];
            }
        }

    	public override int read(int[] data)
    	{
    		if(data == null) return 0;
    		int datalen = data.Length;
    		if(datalen <= 0) return 0;
    		
    		int len = 0;
            // lock (mReadLock)
            {
        		if(mReadData == null) return 0;
        		len = Math.Min(mReadData.Length, datalen);
                Array.Copy(mReadData, 0, data, 0, len);
                for (int i = len; i < datalen; ++i)
                    data[i] = 0;
            }
    		return len;
    	}

    	public override float readFloat()
    	{
    		return read();
    	}

    	public override float readFloat(int index)
    	{
    		return read(index);
    	}

    	public override int readFloat(float[] data)
    	{
    		if(data == null) return 0;
    		int datalen = data.Length;
    		if(datalen <= 0) return 0;
    		
    		// lock(mReadLock)
    		{
        		int len = 0;
    			if(mReadData == null) return 0;
    			int[] readData = mReadData;
    			len = Math.Min(readData.Length, datalen);
    			for(int i = 0; i < len; ++i)
    				data[i] = readData[i];
        		for(int i = len; i < datalen; ++i)
        			data[i] = 0.0f;
        		return len;
    		}
    	}

        public override bool put(int data)
        {
            //lock (mReadLock)
            {
                if(mReadData == null || mReadData.Length <= 0)
                {
                    if(mDataSize < 0)
                        mReadData = new int[1];
                    else
                        return false;
                }
                mReadData[0] = Utils.clamp(data, mMinValue, mMaxValue);
                fire();
            }
            return true;
        }
        
        public override bool put(int index, int data)
        {
            if(index < 0) return false;

            //lock(mReadLock)
            {
                if(mReadData == null)
                {
                    if(mDataSize < 0)
                    {
                        mReadData = new int[index + 1];
                        fill(mReadData, mInitialValue, 0, index);
                    }
                    else
                        return false;
                }
                else if(index >= mReadData.Length)
                {
                    if(mDataSize < 0)
                    {
                        int[] newData = new int[index + 1];
                        int len = mReadData.Length;
                        Array.Copy(mReadData, 0, newData, 0, len);
                        fill(newData, mInitialValue, len, index);
                        mReadData = newData;
                    }
                    else
                        return false;
                }
                mReadData[index] = Utils.clamp(data, mMinValue, mMaxValue);
                fire();
            }
            return true;
        }
        
        public override int put(int[] data)
        {
            if(data == null) return 0;
            int datalen = data.Length;
            if(datalen <= 0) return 0;

            int len = 0;
            //lock (mReadLock)
            {
                if(mDataSize < 0)
                {
                    if(mReadData == null || mReadData.Length != datalen)
                        mReadData = new int[datalen];
                }
                int[] readData = mReadData;
                if(readData == null) return 0;
                int thislen = readData.Length;
                if(thislen <= 0) return 0;
                len = Math.Min(thislen, datalen);
                Utils.copyClamped(data, 0, readData, 0, len, mMinValue, mMaxValue);
                for(int i = len; i < thislen; ++i)
                    readData[i] = 0;
                fire();
            }
            return len;
        }
        
        public override bool putFloat(float data)
        {
            return put((int)data);
        }

        public override bool putFloat(int index, float data)
        {
            return put(index, (int)data);
        }

        public override int putFloat(float[] data)
        {
            if(data == null) return 0;
            int datalen = data.Length;
            if(datalen <= 0) return 0;

            int len = 0;
            // lock (mReadLock)
            {
                if(mDataSize < 0)
                {
                    if(mReadData == null || mReadData.Length != datalen)
                        mReadData = new int[datalen];
                }
                int[] readData = mReadData;
                if(readData == null) return 0;
                int thislen = readData.Length;
                if(thislen <= 0) return 0;
                len = Math.Min(thislen, datalen);
                for(int i = 0; i < len; ++i)
                    readData[i] = Utils.clamp((int)data[i], mMinValue, mMaxValue);
                for(int i = len; i < thislen; ++i)
                    readData[i] = 0;
                fire();
            }
            return len;
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
