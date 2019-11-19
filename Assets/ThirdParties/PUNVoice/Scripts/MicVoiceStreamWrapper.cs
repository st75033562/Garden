using ExitGames.Client.Photon.Voice;

public class MicVoiceStreamWrapper : IAudioStream
{
    private MicVoiceStream m_voiceStream;

    public MicVoiceStreamWrapper(string device, int suggestedFrequency)
    {
        m_voiceStream = new MicVoiceStream(device, suggestedFrequency);
    }

    public int SamplingRate { get { return this.m_voiceStream.samplingRate; } }
    public int Channels { get { return this.m_voiceStream.channels; } }

    public bool GetData(float[] buffer)
    {
        return m_voiceStream.Read(buffer);
    }

    public void Close()
    {
        m_voiceStream.Close();
    }
}
