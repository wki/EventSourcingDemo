using System;

namespace Wki.EventSourcing.Protocol
{
    class PersistException: Exception
    {
        public PersistException(string message): base(message)
        {
        }
    }
}
