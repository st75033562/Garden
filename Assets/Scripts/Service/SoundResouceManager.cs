using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

public class Sound
{
    public enum Type
    {
        Embedded,
        External
    }

    private enum State
    {
        None,
        Loaded,
        Loading
    }

    private SoundResourceManager m_soundManager;
    private State m_state;

    internal Sound(SoundResourceManager manager, string name, Type type)
    {
        m_soundManager = manager;
        this.name = name;
        this.type = type;
    }

    internal Sound(SoundResourceManager manager, string name, AudioClip clip)
        : this(manager, name, Type.External)
    {
        this.clip = clip;
        m_state = State.Loaded;
    }

    public string name
    {
        get;
        private set;
    }

    public Type type
    {
        get;
        private set;
    }

    public AudioClip clip
    {
        get;
        private set;
    }

    // to get an audio clip, steps are
    //    1. call load and yield
    //    2. get the clip from object
    public CustomYieldInstruction load()
    {
        if (m_state == State.Loaded)
        {
            return null;
        }

        if (m_state == State.None)
        {
            if (type == Type.Embedded)
            {
                clip = m_soundManager.loadFromResource(name);
                m_state = State.Loaded;
                return null;
            }
            else
            {
                m_soundManager.loadFromFile(this);
                m_state = State.Loading;
            }
        }
        return new WaitUntil(() => m_state == State.Loaded);
    }

    internal void setLoaded(AudioClip clip)
    {
        m_state = State.Loaded;
        this.clip = clip;
    }

    internal void dispose()
    {
        if (clip && type == Type.External)
        {
            Object.Destroy(clip);
        }
        clip = null;
        m_state = State.None;
    }
}

public class SoundResourceManager : MonoBehaviour, IService
{
    public class SavingRequest : CustomYieldInstruction
    {
        private bool m_done;
        private bool m_error;

        public bool error
        {
            get { return m_error; }
        }

        internal void setDone(bool error)
        {
            m_error = error;
            m_done = true;
        }
        
        public override bool keepWaiting
        {
            get { return !m_done; }
        }
    }

    private struct SoundHeader
    {
        public const int size = 5;

        public int frequency;
        public int channels;

        public SoundHeader(int f, int c)
        {
            frequency = f;
            channels = c;
        }

        public void save(byte[] data)
        {
            byte[] freqData = BitConverter.GetBytes(frequency);
            Array.Copy(freqData, 0, data, 0, freqData.Length);
            data[sizeof(int)] = (byte)channels;
        }

        public void load(byte[] data)
        {
            frequency = BitConverter.ToInt32(data, 0);
            channels = data[sizeof(int)];
        }
    }

    private const string SoundFolder = "Sound";

    private List<Sound> m_sounds = new List<Sound>();
    private FileManager m_fileManager;

    public void init()
    {
        // TODO: add the basic audios
        m_fileManager = ServiceLocator.get<FileManager>();
        loadSoundFiles();
    }

    private void loadSoundFiles()
    {
        if (Directory.Exists(SoundFolder))
        {
            foreach (var f in Directory.GetFiles(SoundFolder))
            {
                string name = Path.GetFileName(f);
                m_sounds.Add(new Sound(this, name, Sound.Type.External));
            }
        }
    }

    // gracefully shutdown the manager, wait for all asynchronous operations to complete
    public Coroutine shutdown()
    {
        return StartCoroutine(shutdownImpl());
    }

    // reset the manager, cancel all pending operations
    public void reset()
    {
        m_fileManager.reset();
        clearSounds();
    }

    private IEnumerator shutdownImpl()
    {
        // TODO: wait until all requests completes
        clearSounds();
        yield break;
    }

    private void clearSounds()
    {
        for (int i = 0; i < m_sounds.Count; ++i)
        {
            m_sounds[i].dispose();
        }
        m_sounds.Clear();
    }

    public Sound getSound(string name)
    {
        return m_sounds.Find(x => name == x.name);
    }

    // save the recording
    // if save succeeds, a sound object will be added to the manager, otherwise no-op
    // in case of failure, caller is responsible for destroying the clip
    public SavingRequest saveRecording(string name, AudioClip clip)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("empty name");
        }

        if (getSound(name) != null)
        {
            throw new InvalidOperationException("duplicate sound");
        }

        if (clip == null)
        {
            throw new ArgumentNullException("clip");
        }

        byte[] data = new byte[SoundHeader.size + clip.samples * sizeof(float)];
        float[] audioSamples = new float[clip.samples];
        clip.GetData(audioSamples, 0);

        SoundHeader header = new SoundHeader(clip.frequency, clip.channels);
        header.save(data);
        Buffer.BlockCopy(audioSamples, 0, data, SoundHeader.size, audioSamples.Length * sizeof(float));

        Sound sound = new Sound(this, name, clip);
        var request = new SavingRequest();

        m_fileManager.saveAsync(getFilePath(name), data, null, (error) =>
        {
            request.setDone(error != null);
            if (error == null)
            {
                m_sounds.Add(sound);
            }
            else
            {
                Debug.LogErrorFormat("failed to save {0}", sound.name);
            }
        });

        return request;
    }

    public string getFilePath(string name)
    {
        return "Sounds/" + name;
    }

    internal void loadFromFile(Sound sound)
    {
        m_fileManager.loadDataAsync(getFilePath(sound.name), (data, error) =>
        {
            AudioClip clip = null;
            if (data != null)
            {
                if (data.Length >= SoundHeader.size)
                {
                    SoundHeader header = new SoundHeader();
                    header.load(data);

                    // round down to multiple of float size
                    int audioDataSize = (data.Length - SoundHeader.size) & ~(sizeof(float) - 1);
                    var audioSamples = new float[audioDataSize / sizeof(float)];
                    Buffer.BlockCopy(data, SoundHeader.size, audioSamples, 0, audioDataSize);

                    // non-streamed clip, otherwise we need to implement callback
                    clip = AudioClip.Create(sound.name, audioSamples.Length, header.channels, header.frequency, false);
                    clip.SetData(audioSamples, 0);
                }
                else
                {
                    Debug.LogErrorFormat("invalid sound data size: {0}, {1} bytes", sound.name, data.Length);
                }
            }
            else
            {
                Debug.LogErrorFormat("failed to load {0}", sound.name);
            }
            sound.setLoaded(clip);
        });
    }

    internal AudioClip loadFromResource(string name)
    {
        return Resources.Load<AudioClip>(getFilePath(name));
    }

}
