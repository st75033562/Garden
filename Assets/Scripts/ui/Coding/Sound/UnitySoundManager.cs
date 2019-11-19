using AssetBundles;
using DataAccess;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitySound : ISound
{
    private readonly UnitySoundManager m_manager;
    private readonly AudioSource m_source;
    private UnitySoundClip m_soundClip;
    private bool m_mute;
    private bool m_isPlaying;
    private bool m_isPaused;
    private bool m_isLoadingClip;

    public UnitySound(UnitySoundManager manager, AudioSource source)
    {
        if (manager == null)
        {
            throw new ArgumentNullException("manager");
        }

        if (source == null)
        {
            throw new ArgumentNullException("source");
        }

        m_manager = manager;
        m_source = source;
    }

    public void Play()
    {
        if (m_soundClip == null)
        {
            Debug.Log("sound clip not set");
            return;
        }

        if (!m_isPlaying)
        {
            m_isPlaying = true;
            m_isLoadingClip = true;
            m_manager.OnPlay(this);
        }
    }

    public void Stop()
    {
        if (m_isPlaying)
        {
            ResetPlayStates();
            m_isLoadingClip = false;
            m_source.Stop();
            m_manager.OnStop(this);
        }
    }

    private void ResetPlayStates()
    {
        m_isPlaying = false;
        m_isPaused = false;
    }

    public AudioSource audioSource
    {
        get { return m_source; }
    }

    public void Release()
    {
        m_manager.Release(this);
    }

    public UnitySoundClip clip
    {
        get { return m_soundClip; }
        set
        {
            Stop();
            m_soundClip = value;
        }
    }

    public bool isPlaying
    {
        get { return m_isPlaying; }
    }

    public bool isPaused
    {
        get { return m_isPaused || m_manager.isPaused; }
        set
        {
            if (m_isPlaying)
            {
                m_isPaused = value;
                UpdatePause();
            }
        }
    }

    internal void UpdatePause()
    {
        if (isPaused)
        {
            m_source.Pause();
        }
        else
        {
            m_source.UnPause();
        }
    }

    public bool mute
    {
        get { return m_mute || m_manager.mute; }
        set
        {
            m_mute = value;
            UpdateMute();
        }
    }

    internal void UpdateMute()
    {
        m_source.mute = mute;
    }

    public bool Update()
    {
        if (m_isPlaying)
        {
            if (m_isLoadingClip)
            {
                if (m_soundClip.isReady)
                {
                    audioSource.clip = m_soundClip.clip;
                    audioSource.volume = m_soundClip.volume;
                    audioSource.Play();
                    if (isPaused)
                    {
                        audioSource.Pause();
                    }
                    m_isLoadingClip = false;

                    //Debug.Log("playing " + audioSource.clip.name);
                }
                else if (m_soundClip.isError)
                {
                    ResetPlayStates();
                    m_isLoadingClip = false;
                }
            }
            else if (!audioSource.isPlaying && !isPaused)
            {
                ResetPlayStates();
                //Debug.Log("stopped " + audioSource.clip.name);
            }
        }

        return m_isPlaying;
    }
}

public class UnitySoundClip
{
    private AssetBundleLoadAssetOperation m_loadOp;
    private AudioClip m_clip;
    private SoundAssetData m_assetData;

    public UnitySoundClip(SoundAssetData assetData)
    {
        if (assetData == null)
        {
            throw new ArgumentNullException("assetData");
        }
        m_assetData = assetData;
        m_loadOp = AssetBundleManager.LoadAssetAsync(assetData.bundleName, assetData.assetName, typeof(AudioClip));
    }

    public bool isReady
    {
        get { return m_clip != null; }
    }

    public bool isError
    {
        get;
        private set;
    }

    public bool isLoading
    {
        get { return !isReady && !isError; }
    }

    public AudioClip clip
    {
        get { return m_clip; }
    }

    public bool Update()
    {
        if (m_loadOp != null)
        {
            if (m_loadOp.IsDone())
            {
                m_clip = m_loadOp.GetAsset<AudioClip>();
                isError = m_clip == null;
                if (isError)
                {
                    Debug.LogErrorFormat("failed to load {0}, {1}", m_assetData.bundleName, m_assetData.assetName);
                    AssetBundleManager.UnloadAssetBundle(m_assetData.bundleName);
                }
                m_loadOp = null;
                return false;
            }

            return true;
        }

        return false;
    }

    public void Dispose()
    {
        if (m_loadOp != null)
        {
            m_loadOp.Dispose();
            m_loadOp = null;
        }

        if (m_clip)
        {
            AssetBundleManager.UnloadAssetBundle(m_assetData.bundleName);
            m_clip = null;
        }
    }

    public float volume {
        get {
            return m_assetData.volume;
        }
    }
}

public class UnitySoundManager : IDisposable
{
    private GameObject m_audioSourceGo;
    private readonly Stack<UnitySound> m_freeSounds = new Stack<UnitySound>();
    private readonly List<UnitySound> m_inUseSounds = new List<UnitySound>();
    private readonly Dictionary<SoundAssetData, UnitySoundClip> m_loadedClips =
        new Dictionary<SoundAssetData, UnitySoundClip>();

    private readonly List<UnitySoundClip> m_loadingClips = new List<UnitySoundClip>();
    private readonly List<UnitySound> m_updatingSounds = new List<UnitySound>();
    private readonly List<UnitySound> m_oneShotSounds = new List<UnitySound>();

    private bool m_mute;
    private bool m_isPaused;
    private int m_maxNumSounds = int.MaxValue;

    public UnitySoundManager()
    {
        m_audioSourceGo = new GameObject("AudioSouce Pool");
    }

    public int maxNumSounds
    {
        get { return m_maxNumSounds; }
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("value");
            }
            m_maxNumSounds = value;
        }
    }

    public void Play(SoundAssetData asset)
    {
        var sound = Create(asset);
        if (sound != null)
        {
            sound.Play();
            m_oneShotSounds.Add((UnitySound)sound);
        }
    }

    /// <summary>
    /// create a sound from a bundle asset.
    /// </summary>
    /// <remarks>
    /// If the number of created sounds already reaches the maximum allowed number, null is returned
    /// </remarks>
    public ISound Create(SoundAssetData asset)
    {
        if (asset == null)
        {
            throw new ArgumentNullException("asset");
        }

        if (m_inUseSounds.Count >= m_maxNumSounds)
        {
            //Debug.LogError("cannot create a new sound, too many sounds");
            return null;
        }

        UnitySoundClip clip;
        if (!m_loadedClips.TryGetValue(asset, out clip))
        {
            clip = new UnitySoundClip(asset);
            m_loadingClips.Add(clip);
            m_loadedClips.Add(asset, clip);
        }

        UnitySound sound;
        if (m_freeSounds.Count > 0)
        {
            sound = m_freeSounds.Pop();
        }
        else
        {
            var source = m_audioSourceGo.AddComponent<AudioSource>();
            sound = new UnitySound(this, source);
        }
        sound.clip = clip;
        sound.mute = false;
        m_inUseSounds.Add(sound);
        return sound;
    }

    public void Release(UnitySound sound)
    {
        if (sound == null)
        {
            throw new ArgumentNullException("sound");
        }

        sound.clip = null;
        m_inUseSounds.Remove(sound);
        RecycleSound(sound);
    }

    private void RecycleSound(UnitySound sound)
    {
        if (m_freeSounds.Count < m_maxNumSounds)
        {
            m_freeSounds.Push(sound);
        }
        else
        {
            UnityEngine.Object.Destroy(sound.audioSource);
        }
    }

    internal void OnPlay(UnitySound sound)
    {
        if (sound == null)
        {
            throw new ArgumentNullException("sound");
        }
        m_updatingSounds.Add(sound);
    }

    internal void OnStop(UnitySound sound)
    {
        if (sound == null)
        {
            throw new ArgumentNullException("sound");
        }
        m_updatingSounds.Remove(sound);
    }

    public void Dispose()
    {
        if (m_audioSourceGo)
        {
            GameObject.Destroy(m_audioSourceGo);
            m_audioSourceGo = null;
        }

        m_freeSounds.Clear();
        m_inUseSounds.Clear();

        foreach (var clip in m_loadedClips.Values)
        {
            clip.Dispose();
        }
        m_loadedClips.Clear();

        m_loadingClips.Clear();
        m_updatingSounds.Clear();
        m_oneShotSounds.Clear();
    }

    public void StopAll()
    {
        foreach (var sound in m_inUseSounds)
        {
            sound.Stop();
        }
        m_oneShotSounds.Clear();
    }

    public void ReleaseAll()
    {
        foreach (var sound in m_inUseSounds)
        {
            sound.clip = null;
            RecycleSound(sound);
        }
        m_inUseSounds.Clear();
        m_updatingSounds.Clear();
        m_oneShotSounds.Clear();
    }

    public bool mute
    {
        get { return m_mute; }
        set
        {
            m_mute = value;
            foreach (var sound in m_inUseSounds)
            {
                sound.UpdateMute();
            }
        }
    }

    public bool isPaused
    {
        get { return m_isPaused; }
        set
        {
            m_isPaused = value;
            foreach (var sound in m_inUseSounds)
            {
                sound.UpdatePause();
            }
        }
    }

    public void Update()
    {
        for (int i = 0; i < m_loadingClips.Count; ++i)
        {
            var clip = m_loadingClips[i];
            if (!clip.Update())
            {
                m_loadingClips.RemoveAt(i);
                --i;
            }
        }

        for (int i = 0; i < m_updatingSounds.Count; ++i)
        {
            var sound = m_updatingSounds[i];
            if (!sound.Update())
            {
                m_updatingSounds.RemoveAt(i);
                --i;
            }
        }

        for (int i = 0; i < m_oneShotSounds.Count; ++i)
        {
            var sound = m_oneShotSounds[i];
            if (!sound.isPlaying)
            {
                sound.Release();
                m_oneShotSounds.RemoveAt(i);
                --i;
            }
        }
    }
}
