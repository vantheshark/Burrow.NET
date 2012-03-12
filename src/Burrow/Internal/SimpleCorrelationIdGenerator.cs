using System;

namespace Burrow.Internal
{
    internal class SimpleCorrelationIdGenerator : ICorrelationIdGenerator
    {
        public string GenerateCorrelationId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
