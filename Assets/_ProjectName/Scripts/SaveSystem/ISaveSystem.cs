using System;

public interface ISaveSystem
{
    public void Save<T>(string key, T data);
    public T Load<T>(string key);
    public bool HasKey(string key);
    public void Delete(string key);
}
