using System;

namespace Robomation
{
    /**
     * @author akaii@kw.ac.kr (Kwang-Hyun Park)
     * 
     * ported by alex@g3games.cn
     */
    public abstract class DeviceImpl : NamedElementImpl, Device
    {
        
    	private readonly int mUid;
    	protected readonly int mDataSize;
    	protected readonly object mInitialValue;
    	// private bool mEvent;
    	private bool mFired;

        protected DeviceImpl(int uid, string name, int dataSize, object initialValue)
            : base(name)
        {
            mUid = uid;
            mDataSize = dataSize;
            mInitialValue = initialValue;
        }

        public int getId()
        {
            return mUid;
        }

        public abstract DeviceType getDeviceType();

        public abstract DataType getDataType();

        public int getDataSize()
        {
            return mDataSize;
        }

        public virtual int read()
        {
            return 0;
        }

        public virtual int read(int index)
        {
            return 0;
        }

        public virtual int read(int[] data)
        {
            return 0;
        }

        public virtual float readFloat()
        {
            return 0.0f;
        }

        public virtual float readFloat(int index)
        {
            return 0.0f;
        }

        public virtual int readFloat(float[] data)
        {
            return 0;
        }

        public virtual string readString()
        {
            return "";
        }

        public virtual string readString(int index)
        {
            return "";
        }

        public virtual int readString(string[] data)
        {
            return 0;
        }

        public virtual bool write(int data)
        {
            throw new InvalidOperationException();
        }

        public virtual bool write(int index, int data)
        {
            throw new InvalidOperationException();
        }

        public virtual int write(int[] data)
        {
            throw new InvalidOperationException();
        }

        public virtual bool writeFloat(float data)
        {
            throw new InvalidOperationException();
        }

        public virtual bool writeFloat(int index, float data)
        {
            throw new InvalidOperationException();
        }

        public virtual int writeFloat(float[] data)
        {
            throw new InvalidOperationException();
        }

        public virtual bool writeString(string data)
        {
            throw new InvalidOperationException();
        }

        public virtual bool writeString(int index, string data)
        {
            throw new InvalidOperationException();
        }

        public virtual int writeString(string[] data)
        {
            throw new InvalidOperationException();
        }

    	public virtual bool put(int data)
    	{
    		throw new InvalidOperationException();
    	}
    	
    	public virtual bool put(int index, int data)
    	{
    		throw new InvalidOperationException();
    	}
    	
    	public virtual int put(int[] data)
    	{
    		throw new InvalidOperationException();
    	}
    	
    	public virtual bool putFloat(float data)
    	{
    		throw new InvalidOperationException();
    	}
    	
    	public virtual bool putFloat(int index, float data)
    	{
    		throw new InvalidOperationException();
    	}
    	
    	public virtual int putFloat(float[] data)
    	{
    		throw new InvalidOperationException();
    	}
    	
    	public virtual bool putString(string data)
    	{
    		throw new InvalidOperationException();
    	}
    	
    	public virtual bool putString(int index, string data)
    	{
    		throw new InvalidOperationException();
    	}
    	
    	public virtual int putString(string[] data)
    	{
    		throw new InvalidOperationException();
    	}

        public virtual void reset()
        {
        }

        public void fire()
        {
            mFired = true;
        }

        public bool isFired()
        {
            return mFired;
        }

        public bool isWritten()
        {
            return isFired();
        }

        public void updateState()
        {
            mFired = false;
        }
    }

}
