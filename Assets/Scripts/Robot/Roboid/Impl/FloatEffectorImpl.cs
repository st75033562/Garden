namespace Robomation
{
    /**
     * @author akaii@kw.ac.kr (Kwang-Hyun Park)
     * 
     * ported by alex@g3games.cn
     */
    internal class FloatEffectorImpl : FloatMotoringDeviceImpl
    {
        public FloatEffectorImpl(int uid, string name, int dataSize, object initialValue, float minValue, float maxValue)
            : base(uid, name, dataSize, initialValue, minValue, maxValue)
        {
        }

        public override DeviceType getDeviceType()
        {
            return DeviceType.EFFECTOR;
        }
    }
}
