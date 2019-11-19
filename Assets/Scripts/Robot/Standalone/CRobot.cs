#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Robomation.Standalone
{
    public enum CRobotType
    {
        Unknown = -1,
        Hamster,
        CheeseStick
    }

    public class ConnectionNotAvailableException : Exception { }

    // a simple wrapper around the c hamster object
    public class CRobot : IDisposable
    {
        private const string InvalidAddress = "000000000000";
        private const int AddressLength = 12;

        public const int PacketSize = RobotApi.PacketSize;

        private IntPtr m_handle;
        private StringBuilder m_addressBuf = new StringBuilder();
        private RobotSensoryDataCallback m_onSensoryDataUpdated;

        public CRobot()
        {
            m_handle = RobotApi.Create();
            if (m_handle == IntPtr.Zero)
            {
                throw new ConnectionNotAvailableException();
            }
            type = CRobotType.Unknown;
        }

        public CRobot(string portName)
        {
            if (string.IsNullOrEmpty(portName))
            {
                throw new ArgumentException("portName");
            }

            this.portName = portName;
            m_handle = RobotApi.CreatePort(portName);
            if (m_handle == IntPtr.Zero)
            {
                throw new ConnectionNotAvailableException();
            }
            type = CRobotType.Unknown;
        }

        ~CRobot()
        {
            Dispose(false);
        }

        // update cached state
        public void updateState()
        {
            ThrowIfDisposed();

            m_addressBuf.EnsureCapacity(AddressLength);
            // clear the string builder, otherwise the content won't be updated
            m_addressBuf.Length = 0;
            RobotApi.GetAddress(m_handle, m_addressBuf, m_addressBuf.Capacity);
            address = m_addressBuf.ToString();

            connected = RobotApi.IsConnected(m_handle) != 0;

            isMotoringDataSent = RobotApi.IsMotoringDataSent(m_handle) != 0;
            type = (CRobotType)RobotApi.GetType(m_handle);
        }

        public CRobotType type
        {
            get;
            private set;
        }

        public string address
        {
            get;
            private set;
        }

        public bool connected
        {
            get;
            private set;
        }

        public string portName
        {
            get;
            private set;
        }

        public RobotSensoryDataCallback onSensoryDataUpdated
        {
            get { return m_onSensoryDataUpdated; }
            set
            {
                ThrowIfDisposed();
                m_onSensoryDataUpdated = value;
                RobotApi.SetSensoryDataCallback(m_handle, value);
            }
        }

        public bool isMotoringDataSent
        {
            get;
            private set;
        }

        public void writeMotoringData(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            if (data.Length != PacketSize)
            {
                throw new ArgumentException("data");
            }

            ThrowIfDisposed();
            RobotApi.SetMotoringData(m_handle, data);
        }

        private void ThrowIfDisposed()
        {
            if (m_handle == IntPtr.Zero)
            {
                throw new ObjectDisposedException("hamster");
            }
        }


        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (m_handle != IntPtr.Zero)
            {
                m_onSensoryDataUpdated = null;
                RobotApi.Dispose(m_handle);
                m_handle = IntPtr.Zero;
            }
        }

        #endregion
    }
}

#endif
