﻿namespace Robomation
{
    /**
     * @author akaii@kw.ac.kr (Kwang-Hyun Park)
     * 
     * ported by alex@g3games.cn
     */
    internal class IntEffectorImpl : IntMotoringDeviceImpl
    {
        public IntEffectorImpl(int uid, string name, int dataSize, object initialValue, int minValue, int maxValue)
            : base(uid, name, dataSize, initialValue, minValue, maxValue)
        {
        }

        public override DeviceType getDeviceType()
        {
            return DeviceType.EFFECTOR;
        }
    }
}
