using System;
using System.Collections.Generic;

namespace Burrow
{
    /// <summary>
    /// Parses a connection string for the values required to connect to a RabbitMQ broker instance.
    /// 
    /// Connection string should look something like this:
    /// host=192.168.1.1;port=5672;virtualHost=MyVirtualHost;username=MyUsername;password=MyPassword
    /// </summary>
    public class ConnectionString
    {
        private readonly IDictionary<string, string> _parametersDictionary = new Dictionary<string, string>();

        /// <summary>
        /// Create a ConnectionString object by a string value
        /// </summary>
        /// <param name="connectionStringValue"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public ConnectionString(string connectionStringValue)
        {
            if (connectionStringValue == null)
            {
                throw new ArgumentNullException(nameof(connectionStringValue));
            }

            var keyValuePairs = connectionStringValue.Split(';');
            foreach (var keyValuePair in keyValuePairs)
            {
                if (string.IsNullOrWhiteSpace(keyValuePair)) continue;

                var keyValueParts = keyValuePair.Split('=');
                if (keyValueParts.Length != 2)
                {
                    throw new Exception($"Invalid connection string element: '{keyValuePair}' should be 'key=value'");
                }

                _parametersDictionary.Add(keyValueParts[0].ToLower(), keyValueParts[1]);
            }

            Port = int.Parse(GetValue("port", "5672"));
            Host = GetValue("host", "localhost");
            VirtualHost = GetValue("virtualHost", "/");
            UserName = GetValue("username", "guest");
            Password = GetValue("password", "guest");

            if (Host.Contains(":"))
            {
                var index = Host.IndexOf(":", StringComparison.Ordinal);
                Port = int.Parse(Host.Substring(index + 1));
                Host = Host.Substring(0, index);
            }
        }
        /// <summary>
        /// The rabbitMQ port, default is 5672
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// The rabbitMQ host, default is localhost
        /// </summary>
        public string Host { get; private set; }


        /// <summary>
        /// The rabbitMQ virtual host, default is /
        /// </summary>
        public string VirtualHost { get; private set; }

        /// <summary>
        /// The rabbitMQ username, default is guest
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// The rabbitMQ password, default is guest
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// Get value of a key from the connection string
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var lowerKey = key.ToLower();
            if (!_parametersDictionary.ContainsKey(lowerKey))
            {
                throw new Exception($"No value with key '{key}' exists");
            }
            return _parametersDictionary[lowerKey];
        }

        /// <summary>
        /// Get value of a key from the connection string, return default value if the key is not set or the key value is null or empty
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetValue(string key, string defaultValue)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            var lowerKey = key.ToLower();
            return _parametersDictionary.ContainsKey(lowerKey) && !string.IsNullOrEmpty(_parametersDictionary[lowerKey])
                ? _parametersDictionary[lowerKey]
                : defaultValue;
        }
    }
}
