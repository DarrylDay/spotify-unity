using System;
using UnityEngine;

public abstract class PlayerPrefValue<T>
{
    public readonly string Key;
    public readonly T DefaultValue;

    public PlayerPrefValue(string key, T defaultValue)
    {
        Key = key;
        DefaultValue = defaultValue;
    }

    public abstract T Read();

    public abstract void Write(T value);

    public void Delete()
    {
        PlayerPrefs.DeleteKey(Key);
    }
}

public class PlayerPrefString : PlayerPrefValue<string>
{
    public PlayerPrefString(string key, string defaultValue = default) : base(key, defaultValue) { }

    public override string Read()
    {
        return PlayerPrefs.GetString(Key, DefaultValue);
    }

    public override void Write(string value)
    {
        PlayerPrefs.SetString(Key, value);
        PlayerPrefs.Save();
    }
}

