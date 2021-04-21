using Autofac;
using System.Collections.Generic;
using System.Net.Http;

public interface IApp
{
    string PATH_ROOT { get; }
    string PATH_WWW { get; }

    //void SetCacheFile(string keyStore, NameValueEntry[])



    IContainer AutofacContainer { set; get; }
    bool ApiCheckLogin(HttpRequestMessage request);

    void EventSend(string text);
    void EventRegister(object client);

    void RedisClearDB();
    void RedisSaveFile();
    string[] RedisSearchKeys(string keyContainText);
    void RedisPublish(string message);
    void RedisUpdate(string storeKey, string itemKey, Dictionary<string, object> data);
    void RedisUpdate(string storeKey, string itemKey, byte[] data);
    void RedisUpdate(string storeKey, string itemKey, string data);
    void RedisDelete(string storeKey, string itemKey);
    void RedisDeleteAll(string storeKey);
}