using LitJson;
using System;

public class UISortSettingDefault
{
    public int key;
    public bool asc = true;
}

public class UISortSetting : UserSettingBase
{
    private readonly string m_key;

    private readonly UISortSettingDefault m_defaultSetting;

    public UISortSetting(string key, UISortSettingDefault defaultSetting)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("key");
        }
        if (defaultSetting == null)
        {
            throw new ArgumentNullException("defaultSetting");
        }

        m_key = key;
        m_defaultSetting = defaultSetting;

        Reset();
    }

    public void SetSortCriterion(int sortKey, bool ascending)
    {
        if (this.sortKey != sortKey || this.ascending != ascending)
        {
            this.sortKey = sortKey;
            this.ascending = ascending;

            Upload();
        }
    }

    public int sortKey { get; set; }

    public bool ascending { get; set; }

    public override void FromJson(JsonData data)
    {
        var sortData = (int)data;
        sortKey = (sortData >> 1) & 0x7FFFFFFF;
        ascending = (sortData & 1) != 0;
    }

    public override JsonData ToJson()
    {
        return new JsonData((sortKey << 1) | (ascending ? 1 : 0));
    }

    public override void Reset()
    {
        sortKey = m_defaultSetting.key;
        ascending = m_defaultSetting.asc;
    }

    public override string key
    {
        get { return m_key; }
    }

    public static UserSettingFactoryDelegate GetFactory(UISortSettingDefault defaultSetting)
    {
        return key => new UISortSetting(key, defaultSetting);
    }
}
