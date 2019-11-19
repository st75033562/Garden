using UnityEngine;

public class BinaryWriter
{
	const int m_OneBuff = 1024;
	byte[] m_Buff;
	int m_BuffPos;

	public BinaryWriter()
	{
		m_Buff = new byte[m_OneBuff];
		m_BuffPos = 0;
	}

	public int Packbool(bool data)
	{
		int curDatalen = 1;
		NeedMoreBuff(m_BuffPos + curDatalen);
		byte[] mBuff = System.BitConverter.GetBytes(data);
		return Pack(mBuff, curDatalen);
	}

	public int Pack8bit(byte data)
	{
		NeedMoreBuff(m_BuffPos + 1);
		int mStartPos = m_BuffPos;
		m_Buff[m_BuffPos] = data;
		m_BuffPos += 1;
		return mStartPos;
	}

	public int Pack16bit(short data)
	{
		int curDatalen = 2;
		NeedMoreBuff(m_BuffPos + curDatalen);
		byte[] mBuff = System.BitConverter.GetBytes(data);
		return Pack(mBuff, curDatalen);
	}
	public int Pack16bit(ushort data)
	{
		int curDatalen = 2;
		NeedMoreBuff(m_BuffPos + curDatalen);
		byte[] mBuff = System.BitConverter.GetBytes(data);
		return Pack(mBuff, curDatalen);
	}
	public int Pack32bit(int data)
	{
		int curDatalen = 4;
		NeedMoreBuff(m_BuffPos + curDatalen);
		byte[] mBuff = System.BitConverter.GetBytes(data);
		return Pack(mBuff, curDatalen);
	}
	public int Pack32bit(uint data)
	{
		int curDatalen = 4;
		NeedMoreBuff(m_BuffPos + curDatalen);
		byte[] mBuff = System.BitConverter.GetBytes(data);
		return Pack(mBuff, curDatalen);
	}
	public int Pack32bit(float data)
	{
		int curDatalen = 4;
		NeedMoreBuff(m_BuffPos + curDatalen);
		byte[] mBuff = System.BitConverter.GetBytes(data);
		return Pack(mBuff, curDatalen);
	}
	public int Pack64bit(double data)
	{
		int curDatalen = 8;
		NeedMoreBuff(m_BuffPos + curDatalen);
		byte[] mBuff = System.BitConverter.GetBytes(data);
		return Pack(mBuff, curDatalen);
	}
	public int Pack64bit(long data)
	{
		int curDatalen = 8;
		NeedMoreBuff(m_BuffPos + curDatalen);
		byte[] mBuff = System.BitConverter.GetBytes(data);
		return Pack(mBuff, curDatalen);
	}

	public void PackString(string data)
	{
		byte[] tBuff = System.Text.Encoding.UTF8.GetBytes(data);
		NeedMoreBuff(m_BuffPos + tBuff.Length);
		Pack32bit(tBuff.Length);
		Pack(tBuff, tBuff.Length);
	}

	//public int PackString(string data, int len)
	//{
	//	int curDatalen = len;
	//	if (m_BuffPos + curDatalen > m_Buff.Length)
	//	{
	//		NeedMoreBuff();
	//	}
	//	byte[] mBuff = System.Text.Encoding.UTF8.GetBytes(data);
	//	return Pack(mBuff, curDatalen);
	//}

	public int PackByte(byte[] data, int offset, int len)
	{
		NeedMoreBuff(m_BuffPos + len);
		int mStartPos = m_BuffPos;
		System.Buffer.BlockCopy(data, offset, m_Buff, m_BuffPos, len);
		m_BuffPos += len;
		return mStartPos;
	}

	int Pack(byte[] data, int len)
	{
		int mStartPos = m_BuffPos;
        System.Buffer.BlockCopy(data, 0, m_Buff, m_BuffPos, len);
		m_BuffPos += len;
		return mStartPos;
	}

	public byte[] GetByte()
	{
		byte[] mVaild = new byte[m_BuffPos];
		System.Buffer.BlockCopy(m_Buff, 0, mVaild, 0, m_BuffPos);
		return mVaild;
	}

	public int GetByteLen()
	{
		return m_BuffPos;
	}

	void NeedMoreBuff(int curlen)
	{
		if(curlen > m_Buff.Length)
		{
			int tNeedBuff = (curlen + m_OneBuff - 1) * m_OneBuff;
			byte[] mCurData = new byte[m_Buff.Length];
			System.Buffer.BlockCopy(m_Buff, 0, mCurData, 0, m_Buff.Length);

			m_Buff = new byte[tNeedBuff];
			System.Buffer.BlockCopy(mCurData, 0, m_Buff, 0, mCurData.Length);
		}
	}

	public void CoverBuff(byte[] data, int len, int pos)
	{
		if(pos + len > m_BuffPos)
		{
			Debug.LogError("BinaryWriter's CoverBuff() error");
			return;
		}

		System.Buffer.BlockCopy(data, 0, m_Buff, pos, len);
	}
}