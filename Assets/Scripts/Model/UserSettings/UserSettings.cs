using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


public abstract class UserSettingBase
{
    private UserSettings m_manager;

    public UserSettings manager
    {
        get { return m_manager; }
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }
            m_manager = value;
        }
    }

    public void Upload()
    {
        manager.Upload(this);
    }

    public abstract void FromJson(JsonData data);

    public abstract JsonData ToJson();

    public abstract void Reset();

    public abstract string key { get; }
}

public interface IUserSettingFactory
{
    UserSettingBase Create(string key);
}

public class UserSettings
{
    private readonly SocketManager m_manager;
    private readonly Dictionary<string, UserSettingBase> m_userSettings = new Dictionary<string, UserSettingBase>();
    private readonly IUserSettingFactory m_factory;

    public UserSettings(SocketManager manager, IUserSettingFactory factory)
    {
        if (manager == null)
        {
            throw new ArgumentNullException("manager");
        }
        if (factory == null)
        {
            throw new ArgumentNullException("factory");
        }
        m_manager = manager;
        m_factory = factory;
    }

    public UserSettingBase Get(string key, bool create = false)
    {
        UserSettingBase setting;
        if (!m_userSettings.TryGetValue(key, out setting) && create)
        {
            setting = TryCreate(key);
        }
        return setting;
    }

    public IEnumerator SyncAllSettings()
    {
        var requestData = new CMD_Get_User_Setting_r_Parameters();
        var request = m_manager.sendAsync(Command_ID.CmdGetUserSettingR, requestData);
        yield return request;
        if (request.result == Command_Result.CmdNoError)
        {
            m_userSettings.Clear();

            try
            {
                var userSettingData = CMD_Get_User_Setting_a_Parameters.Parser.ParseFrom(request.response);
                if (!string.IsNullOrEmpty(userSettingData.JsonString))
                {
                    var jsonObj = JsonMapper.ToObject(userSettingData.JsonString);
                    foreach (var key in jsonObj.Keys)
                    {
                        var setting = TryCreate(key);
                        if (setting != null)
                        {
                            setting.FromJson(jsonObj[key]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogException(e);
            }
        }
        else
        {
            Debug.Log("get user setting failed: " + request.result);
        }
    }

    private UserSettingBase TryCreate(string key)
    {
        var setting = m_factory.Create(key);
        if (setting != null)
        {
            setting.manager = this;
            m_userSettings.Add(key, setting);
        }
        else
        {
            Debug.LogError("unrecognized key: " + key);
        }
        return setting;
    }

    public void Upload(UserSettingBase setting)
    {
        Assert.IsNotNull(Get(setting.key));

        var data = new JsonData();
        data[setting.key] = setting.ToJson();

        var requestData = new CMD_Update_User_Setting_r_Parameters();
        requestData.JsonString = data.ToJson();
        m_manager.send(Command_ID.CmdUpdateUserSettingR, requestData, (res, response) => {
            if (res == Command_Result.CmdNoError)
            {
                Debug.Log("updated user setting: " + setting.key);
            }
            else
            {
                Debug.LogErrorFormat("failed to update user setting: {0}, {1}", setting.key, res);
            }
        });
    }

    public void Reset()
    {
        m_userSettings.Clear();
    }
}
