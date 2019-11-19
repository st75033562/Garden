using LitJson;
using System;
using System.Collections.Generic;
using System.Linq;

public class VersionDatabase
{
    private List<Pair<Version, int>> m_versions;

    public VersionDatabase(string data)
    {
        var jsonData = JsonMapper.ToObject(data);
        m_versions = jsonData.Keys.Select(x => {
            Version v;
            if (x == "*")
            {
                // * for latest version
                v = new Version(int.MaxValue, int.MaxValue);
            }
            else
            {
                v = new Version(x);
            }
            return Pair.Of(v, (int)jsonData[x]);
        }).OrderBy(x => x.first).ToList();
    }

    /// <summary>
    /// get the version number for the specified version
    /// </summary>
    public int GetVersionNumber(string current)
    {
        var curVersion = new Version(current);
        return m_versions.First(x => curVersion <= x.first).second;
    }
}
