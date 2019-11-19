using LitJson;
using System.Collections.Generic;

public class GameboardHistory : UserSettingBase
{
    private Dictionary<string, string> m_lastUsedGameboards = new Dictionary<string, string>();
    
    public string GetLastOpenedGameboard(string projectName)
    {
        string gameboardName;
        m_lastUsedGameboards.TryGetValue(projectName, out gameboardName);
        return gameboardName;
    }

    public void SetLastOpenedGameboard(string projectName, string gameboardName)
    {
        m_lastUsedGameboards[projectName] = gameboardName;
    }

    public void UnsetLastOpenedGameboard(string projectName)
    {
        m_lastUsedGameboards.Remove(projectName);
    }

    public override void FromJson(JsonData data)
    {
        m_lastUsedGameboards.Clear();
        foreach (var key in data.Keys)
        {
            m_lastUsedGameboards[key] = (string)data[key];
        }
    }

    public override JsonData ToJson()
    {
        return JsonMapperUtils.ToJson(m_lastUsedGameboards);
    }

    public override void Reset()
    {
        m_lastUsedGameboards.Clear();
    }

    public override string key
    {
        get { return "lastOpenedGameboards"; }
    }
}
