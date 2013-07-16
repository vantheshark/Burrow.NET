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

        public ConnectionString(string connectionStringValue)
        {
            if (connectionStringValue == null)
            {
                throw new ArgumentNullException("connectionStringValue");
            }

            var keyValuePairs = connectionStringValue.Split(';');
            foreach (var keyValuePair in keyValuePairs)
            {
                if (string.IsNullOrWhiteSpace(keyValuePair)) continue;

                var keyValueParts = keyValuePair.Split('=');
                if (keyValueParts.Length != 2)
                {
                    throw new Exception(string.Format("Invalid connection string element: '{0}' should be 'key=value'", keyValuePair));
                }

                _parametersDictionary.Add(keyValueParts[0], keyValueParts[1]);
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

        public int Port { get; private set; }

        public string Host { get; private set; }

        public string VirtualHost { get; private set; }

        public string UserName { get; private set; }

        public string Password { get; private set; }

        public string GetValue(string key)
        {
            if (!_parametersDictionary.ContainsKey(key))
            {
                throw new Exception(string.Format("No value with key '{0}' exists", key));
            }
            return _parametersDictionary[key];
        }

        public string GetValue(string key, string defaultValue)
        {
            return _parametersDictionary.ContainsKey(key) && !string.IsNullOrEmpty(_parametersDictionary[key])
                ? _parametersDictionary[key]
                : defaultValue;
        }
    }
}
