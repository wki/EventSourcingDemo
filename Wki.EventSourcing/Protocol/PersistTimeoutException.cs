using System;

namespace Wki.EventSourcing.Protocol
{
    class PersistTimeoutException : Exception
    {
        public PersistTimeoutException(string message) : base(message) { }
    }
}
