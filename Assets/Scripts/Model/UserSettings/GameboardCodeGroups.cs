using Google.Protobuf;
using LitJson;
using System;
using System.Collections.Generic;

public class GameboardCodeGroups : UserSettingBase
{
    private class RobotCodeGroups : Gameboard.RobotCodeGroups
    {
        private readonly GameboardCodeGroups m_parent;

        public RobotCodeGroups(GameboardCodeGroups parent)
        {
            m_parent = parent;
        }

        protected override void OnChanged()
        {
            base.OnChanged();

            // TODO: upload queue
            m_parent.Upload();
        }

        public JsonData ToJson()
        {
            var data = new JsonData();
            foreach (var groupData in Serialize())
            {
                data.Add(Convert.ToBase64String(groupData.ToByteArray()));
            }
            return data;
        }

        public void FromJson(JsonData data)
        {
            var groups = new List<Save_GameboardCodeGroup>();
            for (int i = 0; i < data.Count; ++i)
            {
                var encodedGroup = (string)data[i];
                groups.Add(Save_GameboardCodeGroup.Parser.ParseFrom(Convert.FromBase64String(encodedGroup)));
            }
            base.Deserialize(groups);
        }
    }

    private readonly string m_key;
    private RobotCodeGroups[] m_codeGroups = new RobotCodeGroups[(int)ScriptLanguage.Num];

    public GameboardCodeGroups(string key)
    {
        m_key = key;
        Reset();
    }

    // readonly
    public Gameboard.RobotCodeGroups[] codeGroups
    {
        get { return m_codeGroups; }
    }

    public override void FromJson(JsonData root)
    {
        for (int i = 0; i < root.Count; ++i)
        {
            var groupsData = root[i];
            m_codeGroups[i].FromJson(groupsData);
        }
    }

    public override JsonData ToJson()
    {
        var root = new JsonData();
        for (int i = 0; i < m_codeGroups.Length; ++i)
        {
            root.Add(m_codeGroups[i].ToJson());
        }
        return root;
    }

    public override void Reset()
    {
        m_codeGroups = new RobotCodeGroups[(int)ScriptLanguage.Num];
        for (int i = 0; i < m_codeGroups.Length; ++i)
        {
            m_codeGroups[i] = new RobotCodeGroups(this);
        }
    }

    public override string key
    {
        get { return m_key; }
    }
}
