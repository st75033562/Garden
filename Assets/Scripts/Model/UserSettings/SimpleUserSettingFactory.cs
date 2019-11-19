using System;
using System.Collections.Generic;

public delegate UserSettingBase UserSettingFactoryDelegate(string key);

public class SimpleUserSettingFactory : IUserSettingFactory
{
    private const char PrefixSep = '.';

    private readonly Dictionary<string, UserSettingFactoryDelegate> m_factories
        = new Dictionary<string, UserSettingFactoryDelegate>();

    public void Register(string prefix, UserSettingFactoryDelegate factory)
    {
        if (string.IsNullOrEmpty(prefix))
        {
            throw new ArgumentException("prefix");
        }
        if (prefix.IndexOf(PrefixSep) != -1)
        {
            throw new ArgumentException("prefix should not contain " + PrefixSep);
        }
        if (factory == null)
        {
            throw new ArgumentNullException("factory");
        }
        m_factories.Add(prefix, factory);
    }

    public UserSettingBase Create(string key)
    {
        string prefix = key;
        int dotIndex = key.IndexOf(PrefixSep);
        if (dotIndex != -1)
        {
            prefix = key.Substring(0, dotIndex);
        }

        UserSettingFactoryDelegate factory;
        m_factories.TryGetValue(prefix, out factory);
        return factory != null ? factory(key) : null;
    }
}
