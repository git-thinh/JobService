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

        #region Connection
        /// <summary>
        /// Authenticate to the server
        /// </summary>
        /// <param name="password">Redis server password</param>
        /// <returns>Status message</returns>
        string Auth(string password);

        /// <summary>
        /// Echo the given string
        /// </summary>
        /// <param name="message">Message to echo</param>
        /// <returns>Message</returns>
        string Echo(string message);

        /// <summary>
        /// Ping the server
        /// </summary>
        /// <returns>Status message</returns>
        string Ping();

        /// <summary>
        /// Close the connection
        /// </summary>
        /// <returns>Status message</returns>
        string Quit();

        /// <summary>
        /// Change the selected database for the current connection
        /// </summary>
        /// <param name="index">Zero-based database index</param>
        /// <returns>Status message</returns>
        string Select(int index);

        #endregion

        #region Sets



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
        /// Add one or more members to a set
        /// </summary>
        /// <param name="key">Set key</param>
        /// <param name="members">Members to add to set</param>
        /// <returns>Number of elements added to set</returns>
        long SAdd(string key, params object[] members);

        /// <summary>
        /// Get the number of members in a set
        /// </summary>
        /// <param name="key">Set key</param>
        /// <returns>Number of elements in set</returns>
        long SCard(string key);

        /// <summary>
        /// Subtract multiple sets
        /// </summary>
        /// <param name="keys">Set keys to subtract</param>
        /// <returns>Array of elements in resulting set</returns>
        string[] SDiff(params string[] keys);

        /// <summary>
        /// Subtract multiple sets and store the resulting set in a key
        /// </summary>
        /// <param name="destination">Destination key</param>
        /// <param name="keys">Set keys to subtract</param>
        /// <returns>Number of elements in the resulting set</returns>
        long SDiffStore(string destination, params string[] keys);


        /// <summary>
        /// Intersect multiple sets
        /// </summary>
        /// <param name="keys">Set keys to intersect</param>
        /// <returns>Array of elements in resulting set</returns>
        string[] SInter(params string[] keys);




        /// <summary>
        /// Intersect multiple sets and store the resulting set in a key
        /// </summary>
        /// <param name="destination">Destination key</param>
        /// <param name="keys">Set keys to intersect</param>
        /// <returns>Number of elements in resulting set</returns>
        long SInterStore(string destination, params string[] keys);




        /// <summary>
        /// Determine if a given value is a member of a set
        /// </summary>
        /// <param name="key">Set key</param>
        /// <param name="member">Member to lookup</param>
        /// <returns>True if member exists in set</returns>
        bool SIsMember(string key, object member);




        /// <summary>
        /// Get all the members in a set
        /// </summary>
        /// <param name="key">Set key</param>
        /// <returns>All elements in the set</returns>
        string[] SMembers(string key);




        /// <summary>
        /// Move a member from one set to another
        /// </summary>
        /// <param name="source">Source key</param>
        /// <param name="destination">Destination key</param>
        /// <param name="member">Member to move</param>
        /// <returns>True if element was moved</returns>
        bool SMove(string source, string destination, object member);




        /// <summary>
        /// Remove and
        /// </summary>
        /// <param name="key">Set key</param>
        /// <returns>The removed element</returns>
        string SPop(string key);




        /// <summary>
        /// Get a random member from a set
        /// </summary>
        /// <param name="key">Set key</param>
        /// <returns>One random element from set</returns>
        string SRandMember(string key);




        /// <summary>
        /// Get one or more random members from a set
        /// </summary>
        /// <param name="key">Set key</param>
        /// <param name="count">Number of elements to return</param>
        /// <returns>One or more random elements from set</returns>
        string[] SRandMember(string key, long count);




        /// <summary>
        /// Remove one or more members from a set
        /// </summary>
        /// <param name="key">Set key</param>
        /// <param name="members">Set members to remove</param>
        /// <returns>Number of elements removed from set</returns>
        long SRem(string key, params object[] members);




        /// <summary>
        /// Add multiple sets
        /// </summary>
        /// <param name="keys">Set keys to union</param>
        /// <returns>Array of elements in resulting set</returns>
        string[] SUnion(params string[] keys);




        /// <summary>
        /// Add multiple sets and store the resulting set in a key
        /// </summary>
        /// <param name="destination">Destination key</param>
        /// <param name="keys">Set keys to union</param>
        /// <returns>Number of elements in resulting set</returns>
        long SUnionStore(string destination, params string[] keys);




        /// <summary>
        /// Iterate the elements of a set field
        /// </summary>
        /// <param name="key">Set key</param>
        /// <param name="cursor">The cursor returned by the server in the previous call, or 0 if this is the first call</param>
        /// <param name="pattern">Glob-style pattern to filter returned elements</param>
        /// <param name="count">Maximum number of elements to return</param>
        /// <returns>Updated cursor and result set</returns>
        RedisScan<string> SScan(string key, long cursor, string pattern = null, long? count = null);



        #endregion



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
