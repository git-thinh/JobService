﻿using Autofac;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Net.Http;

public interface IApp
{
    ISubscriber RedisSubscriber { set; get; }
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