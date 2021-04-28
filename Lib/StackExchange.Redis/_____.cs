using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackExchange.Redis
{
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
}
