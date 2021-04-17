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

namespace JobWeb
{
    public class Redis
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
                    var m_db = RedisStore.RedisCache;
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

                        if (qs.Count == 0) m_signal.WaitOne();

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

                            IBatch batch = m_db.CreateBatch();
                            updateBatch(batch, arr);

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
                    var redis = RedisStore.Connection;
                    app.RedisSubscriber = redis.GetSubscriber();
                    app.RedisSubscriber.Subscribe("MESSAGE", OnMessage);
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
            if (m_app != null && m_app.RedisSubscriber != null)
                m_app.RedisSubscriber.Publish("MESSAGE", message);
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

        static void updateBatch(IBatch batch, Dictionary<string, object>[] arr)
        {
            if (arr == null || arr.Length == 0) return;
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
                    object _result = dic.Get<object>("_result", null);

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
                        if (it.ContainsKey("_result")) it.Remove("_result");
                        m_app.RedisPublish(JsonConvert.SerializeObject(it));
                    }
                }
            }
        }
    }

    public class RedisStore
    {
        private static readonly Lazy<ConnectionMultiplexer> LazyConnection;

        static RedisStore()
        {
            string redisImageConnectStr = ConfigurationManager.AppSettings["REDIS_CONNECT"];
            LazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(redisImageConnectStr));
        }

        public static ConnectionMultiplexer Connection => LazyConnection.Value;

        public static IDatabase RedisCache => Connection.GetDatabase();
    }

}