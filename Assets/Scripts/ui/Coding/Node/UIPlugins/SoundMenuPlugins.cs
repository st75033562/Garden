using System;
using System.Linq;
using UnityEngine;

public class SoundMenuPlugins : DownMenuPlugins
{
    private const string MenuItemNoSound = "sound_menu_no_sound";

    private class SaveData
    {
        public int assetId;
        // public string resourceId;
    }

    private SoundClipData m_curClipData;

    protected override void Start()
    {
        base.Start();
        if (m_curClipData == null)
        {
            ResetSelection();
        }

        if (CodeContext != null)
        {
            CodeContext.soundClipDataSource.clipsAdded.AddListener(OnClipsAdded);
            CodeContext.soundClipDataSource.clipsCleared.AddListener(ResetSelection);
        }
    }

    protected void OnDestroy()
    {
        if (CodeContext != null)
        {
            CodeContext.soundClipDataSource.clipsAdded.RemoveListener(OnClipsAdded);
            CodeContext.soundClipDataSource.clipsCleared.RemoveListener(ResetSelection);
        }
    }

    private void OnClipsAdded()
    {
        if (m_curClipData == null)
        {
            ResetSelection();
        }
    }

    public override void ResetSelection()
    {
        SoundClipData newClipData = null;
        if (CodeContext != null)
        {
            if (CodeContext.soundClipDataSource.clips.Count > 0)
            {
                newClipData = CodeContext.soundClipDataSource.clips[0];
            }
        }
        if (newClipData != m_curClipData)
        {
            if (newClipData != null)
            {
                ChangePluginsText(newClipData.bundleClip.localizedName);
            }
            else
            {
                ChangePluginsText(MenuItemNoSound);
            }
            m_curClipData = newClipData;
        }
    }

    public SoundClipData clip
    {
        get { return m_curClipData; }
    }

    protected override void OnInput(string str)
    {
        var index = int.Parse(str);
        m_curClipData = CodeContext.soundClipDataSource.clips[index];
        base.OnInput(m_curClipData.bundleClip.localizedName);
    }

    public override void Clicked()
    {
        SetMenuItems(CodeContext.soundClipDataSource.clips.Select((x, i) => {
            string name;
            if (x.bundleClip != null)
            {
                name = x.bundleClip.localizedName;
            }
            else
            {
                throw new NotImplementedException();
            }

            return new UIMenuItem(name, i.ToString());
        }));
        base.Clicked();
    }

    public override Save_PluginsData GetPluginSaveData()
    {
        Save_PluginsData tSaveData = new Save_PluginsData();
        tSaveData.PluginId = PluginID;

        if (m_curClipData != null)
        {
            var menuSaveData = new SaveData();
            if (m_curClipData.bundleClip != null)
            {
                menuSaveData.assetId = m_curClipData.bundleClip.id;
            }
            else
            {
                throw new NotImplementedException();
            }
            tSaveData.PluginTextValue = JsonUtility.ToJson(menuSaveData);
        }

        return tSaveData;
    }

    public override void LoadPluginSaveData(Save_PluginsData save)
    {
        m_curClipData = null;
        if (save.PluginTextValue != "")
        {
            var menuSaveData = JsonUtility.FromJson<SaveData>(save.PluginTextValue);
            if (menuSaveData.assetId != 0)
            {
                m_curClipData = CodeContext.soundClipDataSource.GetClip(menuSaveData.assetId);
            }
            else
            {
                throw new NotImplementedException();
            }

            if (m_curClipData == null)
            {
                ResetSelection();
            }
            else
            {
                SetPluginsText(m_curClipData.bundleClip.localizedName);
            }
        }
        else
        {
            SetPluginsText(MenuItemNoSound);
        }
    }

    public override void PostClone(NodePluginsBase other)
    {
        base.PostClone(other);

        m_curClipData = (other as SoundMenuPlugins).m_curClipData;
    }
}
