using System;
using Wki.EventSourcing.Protocol.Subscription;

namespace Wki.EventSourcing.Protocol.EventStore
{
    /// <summary>
    /// Command to EventStore: start restoring a given durable actor
    /// </summary>
    public class StartRestore { }
}
