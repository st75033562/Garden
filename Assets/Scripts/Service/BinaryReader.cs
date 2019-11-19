using UnityEngine;


public class BinaryReader
{
	byte[] m_Buff;
	int m_BuffPos;
	int m_BuffLen;

	public BinaryReader()
	{
	}

	public BinaryReader(byte[] mBuff, int len)
	{
		UnPack(mBuff, 0, len);
	}

	public BinaryReader(byte[] mBuff, int offset, int len)
	{
		UnPack(mBuff, offset, len);
	}

	public void UnPack(byte[] mBuff, int len)
	{
		UnPack(mBuff, 0, len);
    }

	public void UnPack(byte[] mBuff, int offset, int len)
	{
		m_Buff = new byte[len];
		System.Buffer.BlockCopy(mBuff, offset, m_Buff, 0, len);
		m_BuffLen = len;
		m_BuffPos = 0;
	}

	public bool Readbool()
	{
		if (m_BuffPos + 1 > m_BuffLen)
		{
			ScreenDebug.ScreenPrint("Readbool() longer lager than Max buff len");
			return false;
		}
		bool data = System.BitConverter.ToBoolean(m_Buff, m_BuffPos);
		++m_BuffPos;
		return data;
	}

	public short ReadShort()
	{
		if (m_BuffPos + 2 > m_BuffLen)
		{
			ScreenDebug.ScreenPrint("ReadShort() longer lager than Max buff len");
			return 0;
		}
		short data = System.BitConverter.ToInt16(m_Buff, m_BuffPos);
		m_BuffPos += 2;
		return data;
	}

	public ushort ReadUShort()
	{
		if (m_BuffPos + 2 > m_BuffLen)
		{
			ScreenDebug.ScreenPrint("ReadUShortbit() longer lager than Max buff len");
			return 0;
		}
		ushort data = System.BitConverter.ToUInt16(m_Buff, m_BuffPos);
		m_BuffPos += 2;
		return data;
	}

	public int ReadInt()
	{
		if (m_BuffPos + 4 > m_BuffLen)
		{
			ScreenDebug.ScreenPrint("ReadInt() longer lager than Max buff len");
			return 0;
		}
		int data = System.BitConverter.ToInt32(m_Buff, m_BuffPos);
		m_BuffPos += 4;
		return data;
	}

	public uint ReadUInt()
	{
		if (m_BuffPos + 4 > m_BuffLen)
		{
			ScreenDebug.ScreenPrint("ReadUInt() longer lager than Max buff len");
			return 0;
		}
		uint data = System.BitConverter.ToUInt32(m_Buff, m_BuffPos);
		m_BuffPos += 4;
		return data;
	}

	public float ReadFloat()
	{
		if (m_BuffPos + 4 > m_BuffLen)
		{
			ScreenDebug.ScreenPrint("ReadFloat() longer lager than Max buff len");
			return 0.0f;
		}
		float data = System.BitConverter.ToSingle(m_Buff, m_BuffPos);
		m_BuffPos += 4;
		return data;
	}

	public double ReadDouble()
	{
		if (m_BuffPos + 8 > m_BuffLen)
		{
			ScreenDebug.ScreenPrint("ReadDouble() longer lager than Max buff len");
			return 0.0f;
		}
		double data = System.BitConverter.ToDouble(m_Buff, m_BuffPos);
		m_BuffPos += 8;
		return data;
	}

	public long ReadLong()
	{
		if (m_BuffPos + 8 > m_BuffLen)
		{
			ScreenDebug.ScreenPrint("ReadLong() longer lager than Max buff len");
			return 0;
		}
		long data = System.BitConverter.ToInt64(m_Buff, m_BuffPos);
		m_BuffPos += 8;
		return data;
	}

	public ulong ReadULong()
	{
		if (m_BuffPos + 8 > m_BuffLen)
		{
			ScreenDebug.ScreenPrint("ReadULong() longer lager than Max buff len");
			return 0;
		}
		ulong data = System.BitConverter.ToUInt64(m_Buff, m_BuffPos);
		m_BuffPos += 8;
		return data;
	}

	public string ReadString()
	{
		int tLen = ReadInt();
		if(0 == tLen)
		{
			return "";
		}
		if (m_BuffPos + tLen > m_BuffLen)
		{
			ScreenDebug.ScreenPrint("ReadString() longer lager than Max buff len");
			return "";
		}
		string data = System.Text.Encoding.UTF8.GetString(m_Buff, m_BuffPos, tLen);
		m_BuffPos += tLen;
		return data;
	}

	public byte[] ReadByte(int len)
	{
		byte[] mCur = null;
		if (m_BuffPos + len > m_BuffLen)
		{
			ScreenDebug.ScreenPrint("ReadByte(ushort len) longer lager than Max buff len");
			return mCur;
		}
		mCur = new byte[len];
		System.Buffer.BlockCopy(m_Buff, m_BuffPos, mCur, 0, len);
		m_BuffPos += len;
		return mCur;
	}

	public byte ReadByte()
	{
		if (m_BuffPos + 1 > m_BuffLen)
		{
			ScreenDebug.ScreenPrint("ReadByte() longer lager than Max buff len");
			return 0;
		}
		byte mCur = m_Buff[m_BuffPos];
		++m_BuffPos;
		return mCur;
	}

	public int GetDataLen()
	{
		return m_BuffLen;
	}

	public bool JumpTo(int pos)
	{
		if (pos > m_BuffLen)
		{
			ScreenDebug.ScreenPrint("JumpTo() longer lager than Max buff len");
			return false;
		}
		m_BuffPos = pos;
        return true;
	}
}