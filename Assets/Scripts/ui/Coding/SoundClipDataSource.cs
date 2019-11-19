using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class SoundClipData
{
    // if a bundle clip, this is not null
    public readonly SoundAssetData bundleClip;

    public SoundClipData(SoundAssetData bundleClip)
    {
        if (bundleClip == null)
        {
            throw new ArgumentNullException("bundleClip");
        }
        this.bundleClip = bundleClip;
    }
}

public class SoundClipDataSource
{
    public readonly UnityEvent0 clipsAdded = new UnityEvent0();
    public readonly UnityEvent0 clipsCleared = new UnityEvent0();

    private readonly List<SoundClipData> m_clips = new List<SoundClipData>();

    public void AddClips(IEnumerable<SoundClipData> clips)
    {
        if (clips == null)
        {
            throw new ArgumentNullException("clips");
        }

        m_clips.AddRange(clips);
        clipsAdded.Invoke();
    }

    public IList<SoundClipData> clips
    {
        get { return m_clips; }
    }

    public SoundClipData GetClip(int clipAssetId)
    {
        return m_clips.Find(x => x.bundleClip != null && x.bundleClip.id == clipAssetId);
    }

    public void Clear()
    {
        m_clips.Clear();
        clipsCleared.Invoke();
    }
}
