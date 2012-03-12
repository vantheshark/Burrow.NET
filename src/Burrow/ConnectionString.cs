using System;
using System.Collections.Generic;

namespace Burrow
{
    /// <summary>
    /// Parses a connection string for the values required to connect to a RabbitMQ broker instance.
    /// 
    /// Connection string should look something like this:
    /// host=192.168.1.1;virtualHost=MyVirtualHost;username=MyUsername;password=MyPassword
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
        }

        public string Host
        {
            get { return GetValue("host", "localhost"); }
        }

        public string VirtualHost
        {
            get { return GetValue("virtualHost", "/"); }
        }

        public string UserName
        {
            get { return GetValue("username", "guest"); }
        }

        public string Password
        {
            get { return GetValue("password", "guest"); }
        }

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
            return _parametersDictionary.ContainsKey(key)
                       ? _parametersDictionary[key]
                       : defaultValue;
        }
    }
}
