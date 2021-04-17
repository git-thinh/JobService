using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CSRedis
{
    /// <summary>
    /// Common properties of the RedisClient
    /// </summary>
    public interface IRedisClient :IDisposable
    {
        /// <summary>
        /// Occurs when a subscription message is received
        /// </summary>
        event EventHandler<RedisSubscriptionReceivedEventArgs> SubscriptionReceived;

        /// <summary>
        /// Occurs when a subscription channel is added or removed
        /// </summary>
        event EventHandler<RedisSubscriptionChangedEventArgs> SubscriptionChanged;

        /// <summary>
        /// Occurs when a transaction command is acknowledged by the server
        /// </summary>
        event EventHandler<RedisTransactionQueuedEventArgs> TransactionQueued;

        /// <summary>
        /// Occurs when a monitor message is received
        /// </summary>
        event EventHandler<RedisMonitorEventArgs> MonitorReceived;

        /// <summary>
        /// Occurs when the connection has sucessfully reconnected
        /// </summary>
        event EventHandler Connected;


        /// <summary>
        /// Get the Redis server hostname
        /// </summary>
        string Host { get; }

        /// <summary>
        /// Get the Redis server port
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Get a value indicating whether the Redis client is connected to the server
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Get or set the string encoding used to communicate with the server
        /// </summary>
        Encoding Encoding { get; set; }

        /// <summary>
        /// Get or set the connection read timeout (milliseconds)
        /// </summary>
        int ReceiveTimeout { get; set; }

        /// <summary>
        /// Get or set the connection send timeout (milliseconds)
        /// </summary>
        int SendTimeout { get; set; }

        /// <summary>
        /// Get or set the number of times to attempt a reconnect after a connection fails
        /// </summary>
        int ReconnectAttempts { get; set; }

        /// <summary>
        /// Get or set the amount of time (milliseconds) to wait between reconnect attempts
        /// </summary>
        int ReconnectWait { get; set; }



        /// <summary>
        /// Set the string value of a key
        /// </summary>
        /// <param name="key">Key to modify</param>
        /// <param name="value">Value to set</param>
        /// <returns>Status code</returns>
        string Set(string key, object value);




        /// <summary>
        /// Set the string value of a key with atomic expiration and existence condition
        /// </summary>
        /// <param name="key">Key to modify</param>
        /// <param name="value">Value to set</param>
        /// <param name="expiration">Set expiration to nearest millisecond</param>
        /// <param name="condition">Set key if existence condition</param>
        /// <returns>Status code, or null if condition not met</returns>
        string Set(string key, object value, TimeSpan expiration, RedisExistence? condition = null);




        /// <summary>
        /// Set the string value of a key with atomic expiration and existence condition
        /// </summary>
        /// <param name="key">Key to modify</param>
        /// <param name="value">Value to set</param>
        /// <param name="expirationSeconds">Set expiration to nearest second</param>
        /// <param name="condition">Set key if existence condition</param>
        /// <returns>Status code, or null if condition not met</returns>
        string Set(string key, object value, int? expirationSeconds = null, RedisExistence? condition = null);




        /// <summary>
        /// Set the string value of a key with atomic expiration and existence condition
        /// </summary>
        /// <param name="key">Key to modify</param>
        /// <param name="value">Value to set</param>
        /// <param name="expirationMilliseconds">Set expiration to nearest millisecond</param>
        /// <param name="condition">Set key if existence condition</param>
        /// <returns>Status code, or null if condition not met</returns>
        string Set(string key, object value, long? expirationMilliseconds = null, RedisExistence? condition = null);





        /// <summary>
        /// Get the value of a key
        /// </summary>
        /// <param name="key">Key to lookup</param>
        /// <returns>Value of key</returns>
        string Get(string key);

        /// <summary>
        /// Stream a BULK reply from the server using default buffer size
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="destination">Destination stream</param>
        /// <param name="func">Client command to execute (BULK reply only)</param>
        void StreamTo<T>(Stream destination, Func<IRedisClientSync, T> func);

        /// <summary>
        /// Stream a BULK reply from the server
        /// </summary>
        /// <typeparam name="T">Response type</typeparam>
        /// <param name="destination">Destination stream</param>
        /// <param name="bufferSize">Size of buffer used to write server response</param>
        /// <param name="func">Client command to execute (BULK reply only)</param>
        void StreamTo<T>(Stream destination, int bufferSize, Func<IRedisClientSync, T> func);
    }
}
