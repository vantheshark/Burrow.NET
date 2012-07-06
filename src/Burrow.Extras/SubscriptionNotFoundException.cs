using System;
using System.Diagnostics.CodeAnalysis;

namespace Burrow.Extras
{
    [ExcludeFromCodeCoverage]
    public class SubscriptionNotFoundException : Exception
    {
        public string Name { get; private set; }
        private readonly string _message;

        public SubscriptionNotFoundException(string name)
        {
            Name = name;
        }

        public SubscriptionNotFoundException(string name, string message)
        {
            Name = name;
            _message = message;
        }

        public override string Message
        {
            get
            {
                return string.IsNullOrEmpty(_message)
                    ? string.Format("Subscription {0} not found", Name)
                    : _message;

            }
        }
    }
}
