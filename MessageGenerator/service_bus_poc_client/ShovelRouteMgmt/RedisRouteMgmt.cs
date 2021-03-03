using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShovelRouteMgmt
{
    internal static class RedisRouteMgmt
    {
        private static ConnectionMultiplexer redis;
        private static IDatabase db;
        private static string connectionString;
        static RedisRouteMgmt()
        {
            connectionString = Environment.GetEnvironmentVariable("RedisConnectionString");

            redis = ConnectionMultiplexer.Connect(connectionString);

            db = lazyConnection.Value.GetDatabase();
        }

        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            // string cacheConnection = Configuration[SecretName];
            return ConnectionMultiplexer.Connect(connectionString);
        });

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

        internal static bool AddRoute(string RouteAllowedKey)
        {
            try
            {
                return db.StringSet(RouteAllowedKey, "Y");
            }
            catch (Exception addException)
            {
                Console.WriteLine(addException.Message);
                return false;
            }
        }

        internal static bool DeleteRoute(string RouteAllowedKey)
        {
            try
            {
                return db.KeyDelete(RouteAllowedKey);
            }
            catch (Exception deleteException)
            {
                Console.WriteLine(deleteException.Message);
                return false;
            }
        }
    }
}
