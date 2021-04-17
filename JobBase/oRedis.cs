
using System;
using System.Collections.Generic;

public enum REDIS_ACTION
{
    STORE_DELETE,
    ITEM_UPDATE,
    ITEM_DELETE
}

public class oRedis
{
    public string NotifyId { set; get; }
    public long TimeCreated { set; get; }

    public int Index { set; get; }
    public int MaxItem { set; get; }

    public REDIS_ACTION Type { set; get; }

    public string KeyStore { set; get; }
    public string KeyItem { set; get; }
    public Dictionary<string, object> Data { set; get; }

    public oRedis(string notifyId, REDIS_ACTION type, string storeKey, string keyItem = "", Dictionary<string, object> data = null, int index = 0, int maxItem = 0)
    {
        if (data == null) data = new Dictionary<string, object>() { };
        this.NotifyId = notifyId;
        this.Type = type;
        this.KeyStore = storeKey;
        this.KeyItem = keyItem;
        this.Data = data;
        this.Index = index;
        this.MaxItem = maxItem;
        this.TimeCreated = long.Parse(DateTime.Now.ToString("yyMMddHHmmss"));
    }

    public oRedis(REDIS_ACTION type, string storeKey, string keyItem = "", Dictionary<string, object> data = null, int index = 0, int maxItem = 0)
    {
        if (data == null) data = new Dictionary<string, object>() { };
        this.NotifyId = string.Empty;
        this.Type = type;
        this.KeyStore = storeKey;
        this.KeyItem = keyItem;
        this.Data = data;
        this.Index = index;
        this.MaxItem = maxItem;
        this.TimeCreated = long.Parse(DateTime.Now.ToString("yyMMddHHmmss"));
    }
}
