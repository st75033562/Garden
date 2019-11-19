namespace Robomation
{
    /**
     * @author akaii@kw.ac.kr (Kwang-Hyun Park)
     * 
     * ported by alex@g3games.cn
     */
    public interface Device : NamedElement
    {
    	int getId();
    	DeviceType getDeviceType();
    	DataType getDataType();
    	int getDataSize();
    	// bool e();
    	int read();
    	int read(int index);
    	int read(int[] data);
    	float readFloat();
    	float readFloat(int index);
    	int readFloat(float[] data);
    	string readString();
    	string readString(int index);
    	int readString(string[] data);
    	bool write(int data);
    	bool write(int index, int data);
    	int write(int[] data);
    	bool writeFloat(float data);
    	bool writeFloat(int index, float data);
    	int writeFloat(float[] data);
    	bool writeString(string data);
    	bool writeString(int index, string data);
    	int writeString(string[] data);
        void reset();
        bool isWritten();
        void updateState();
    }

}
