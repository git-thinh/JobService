using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Test
{
    public class RedisStatic
    {
        static IApp m_app;
        static bool m_connected;
        static byte m_dequeue_trying = 0;
        static AutoResetEvent m_signal = new AutoResetEvent(false);
        static AutoResetEvent m_pubsub = new AutoResetEvent(false);
        static int m_size = 1;
        static ConcurrentQueue<Dictionary<string, object>> m_queue = new ConcurrentQueue<Dictionary<string, object>>() { };

        public static void initInstance(IApp app_)
        {
            m_app = app_;
            try
            {
                new Thread(new ParameterizedThreadStart((ms) =>
                {
                    var m_db = RedisWrite.Db;
                    m_connected = true;

                    var tup = (Tuple<IApp, ConcurrentQueue<Dictionary<string, object>>>)ms;
                    var qs = tup.Item2;

                    while (true)
                    {
                        if (m_dequeue_trying == 1)
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        if (qs.Count == 0)
                        {
                            m_signal.WaitOne();
                        }

                        if (m_connected)
                        {
                            m_dequeue_trying = 1;
                            int count = qs.Count;
                            if (count > m_size) count = m_size;
                            Dictionary<string, object>[] arr = new Dictionary<string, object>[count];
                            for (int i = 0; i < count; i++) arr[i] = new Dictionary<string, object>() { };
                            int k = 0;
                            while (k < count)
                            {
                                qs.TryDequeue(out arr[k]);
                                k++;
                            }

                            updateBatch(m_db, arr);
                            
                            //app_.RedisSaveFile();

                            m_dequeue_trying = 0;
                        }
                        Thread.Sleep(5);
                    }//end while
                })).Start(new Tuple<IApp, ConcurrentQueue<Dictionary<string, object>>>(app_, m_queue));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void initPubSub(IApp app_)
        {
            try
            {
                new Thread(new ParameterizedThreadStart((ms) =>
                {
                    var app = (IApp)ms;
                    var redis = RedisRead.Connection;
                    //app.RedisSubscriber = redis.GetSubscriber();
                    //app.RedisSubscriber.Subscribe("MESSAGE", OnMessage);
                    m_pubsub.WaitOne();
                })).Start(app_);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Publish(string message)
        {
            //if (m_app != null && m_app.RedisSubscriber != null)
            //    m_app.RedisSubscriber.Publish("MESSAGE", message);
        }

        public static void Update(string storeKey, string itemKey, byte[] data)
        {
            var dic = new Dictionary<string, object>() { };
            dic.Add("_rd_action", "ITEM_UPDATE");
            dic.Add("_rd_key_store", storeKey);
            dic.Add("_rd_key_item", itemKey);
            dic.Add("_data", data);
            m_queue.Enqueue(dic);
            signalSet();
        }

        public static void Update(string storeKey, string itemKey, string data)
        {
            var dic = new Dictionary<string, object>() { };
            dic.Add("_rd_action", "ITEM_UPDATE");
            dic.Add("_rd_key_store", storeKey);
            dic.Add("_rd_key_item", itemKey);
            dic.Add("_data", data);
            m_queue.Enqueue(dic);
            signalSet();
        }

        public static void Update(string storeKey, string itemKey, Dictionary<string, object> data)
        {
            if (data == null) data = new Dictionary<string, object>() { };
            data.Add("_rd_action", "ITEM_UPDATE");
            data.Add("_rd_key_store", storeKey);
            data.Add("_rd_key_item", itemKey);
            m_queue.Enqueue(data);
            signalSet();
        }

        public static void Delete(string storeKey, string itemKey)
        {
            var data = new Dictionary<string, object>() { };
            data.Add("_rd_action", "ITEM_DELETE");
            data.Add("_rd_key_store", storeKey);
            data.Add("_rd_key_item", itemKey);
            m_queue.Enqueue(data);
            signalSet();
        }

        public static void DeleteAll(string storeKey)
        {
            var data = new Dictionary<string, object>() { };
            data.Add("_rd_action", "STORE_DELETE");
            data.Add("_rd_key_store", storeKey);
            data.Add("_rd_key_item", string.Empty);
            m_queue.Enqueue(data);
            signalSet();
        }

        public static void RedisClearDB()
        {
            RedisWrite.Server.FlushDatabase();
        }

        public static void RedisSaveFile()
        {
            RedisWrite.Server.SaveAsync(SaveType.BackgroundSave);
        }

        public static string[] RedisSearchKeys(string keyContainText)
        {
            return RedisRead.Server.Keys(pattern: "*" + keyContainText + "*").Select(x => (string)x).ToArray();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////

        static void signalSet()
        {
            if (m_dequeue_trying == 0) m_signal.Set();
        }

        static void OnMessage(RedisChannel channel, RedisValue value)
        {
            string json = (string)value;
            Dictionary<string, object> dic;
            if (!string.IsNullOrWhiteSpace(json))
            {
                try { dic = JsonConvert.DeserializeObject<Dictionary<string, object>>(json); }
                catch { }
            }
        }

        static void updateBatch(IDatabase db, Dictionary<string, object>[] arr)
        {
            if (arr == null || arr.Length == 0) return;

            IBatch batch = db.CreateBatch();

            var list = new List<Task<bool>>();
            var indexs = new List<int>();

            for (int i = 0; i < arr.Length; i++)
            {
                Dictionary<string, object> dic = arr[i];
                if (dic != null && dic.Count > 0)
                {
                    Task<bool> wait = null;
                    string rd_action = dic.Get("_rd_action"),
                        key_store = dic.Get("_rd_key_store"),
                        key_item = dic.Get("_rd_key_item");
                    object _result = dic.Get<object>("_data", null);

                    switch (rd_action)
                    {
                        case "ITEM_UPDATE":
                            if (!string.IsNullOrWhiteSpace(key_store)
                                && !string.IsNullOrWhiteSpace(key_item))
                            {
                                if (_result == null)
                                    wait = batch.HashSetAsync(key_store, key_item, string.Empty);
                                else
                                {
                                    string type = _result.GetType().Name;
                                    switch (type)
                                    {
                                        case "Byte[]":
                                            wait = batch.HashSetAsync(key_store, key_item, (Byte[])_result);
                                            break;
                                        case "String":
                                            wait = batch.HashSetAsync(key_store, key_item, (string)_result);
                                            break;
                                    }
                                }
                            }
                            break;
                        case "ITEM_DELETE":
                            if (!string.IsNullOrWhiteSpace(key_store)
                                && !string.IsNullOrWhiteSpace(key_item))
                                wait = batch.HashDeleteAsync(key_store, key_item);
                            break;
                        case "STORE_DELETE":
                            if (!string.IsNullOrWhiteSpace(key_store))
                                wait = batch.KeyDeleteAsync(key_store);
                            break;
                    }
                    if (wait != null)
                    {
                        indexs.Add(i);
                        list.Add(wait);
                    }

                    //{
                    //    try
                    //    {
                    //        var ok = m_db.Wait(wait);
                    //        bool redis_publish = dic.Get<bool>("_rd_publish", false);
                    //        if (redis_publish)
                    //        {
                    //            dic.Add("_rd_ok", ok);
                    //            if (dic.ContainsKey("_result")) dic.Remove("_result");
                    //            m_app.RedisPublish(JsonConvert.SerializeObject(dic));
                    //        }
                    //    }
                    //    catch
                    //    {
                    //        m_queue.Enqueue(dic);
                    //    }
                    //    Thread.Sleep(10);
                    //}
                }
            }

            if (list.Count > 0)
            {
                batch.Execute();
                var oks = Task.WhenAll(list.ToArray()).GetAwaiter().GetResult();
                for (int i = 0; i < oks.Length; i++)
                {
                    int ix = indexs[i];
                    var it = arr[ix];
                    bool redis_publish = it.Get<bool>("_rd_publish", false);
                    if (redis_publish)
                    {
                        it.Add("_rd_ok", oks[i]);
                        if (it.ContainsKey("_data")) it.Remove("_data");
                        m_app.RedisPublish(JsonConvert.SerializeObject(it));
                    }
                }
            }
        }
    }

    public class RedisWrite
    {
        private static readonly Lazy<ConnectionMultiplexer> LazyConnection;
        private static readonly string redisConnectStr;

        static RedisWrite()
        {
            redisConnectStr = ConfigurationManager.AppSettings["REDIS_WRITE"];
            LazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(redisConnectStr));
        }

        public static ConnectionMultiplexer Connection => LazyConnection.Value;
        public static IServer Server => LazyConnection.Value.GetServer(redisConnectStr.Split(',')[0]);
        public static IDatabase Db => Connection.GetDatabase();
    }

    public class RedisRead
    {
        private static readonly Lazy<ConnectionMultiplexer> LazyConnection;
        private static readonly string redisConnectStr;

        static RedisRead()
        {
            redisConnectStr = ConfigurationManager.AppSettings["REDIS_READ"];
            LazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(redisConnectStr));
        }

        public static ConnectionMultiplexer Connection => LazyConnection.Value;
        public static IServer Server => LazyConnection.Value.GetServer(redisConnectStr.Split(',')[0]);
        public static IDatabase Db => Connection.GetDatabase();
    }
}