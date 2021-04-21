using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Configuration;

namespace Test
{
    public class Global : System.Web.HttpApplication
    {
        static IDatabase mdb_read;
        static IDatabase mdb_write;
        static ISubscriber m_subscriber;
        static ConcurrentQueue<Dictionary<string, object>> m_queue;
        public static void UpdateRedis(Dictionary<string, object> dic) {
            m_queue.Enqueue(dic);
            mdb_read.Publish("QUEUE", true);
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            m_queue = new ConcurrentQueue<Dictionary<string, object>>() { };

            var conn = RedisRead.Connection;
            mdb_read = conn.GetDatabase();
            m_subscriber = conn.GetSubscriber();
            m_subscriber.Subscribe("QUEUE", OnQueue);
            m_subscriber.Subscribe("MESSAGE", OnMessage);

            mdb_write = RedisWrite.Db;
        }

        private void OnQueue(RedisChannel channel, RedisValue message)
        {
            if (m_queue.Count > 0) { 
            
            }
        }

        private void OnMessage(RedisChannel channel, RedisValue message)
        {

        }


        protected void Application_BeginRequest(object sender, EventArgs e)
        {
        }
    }
}