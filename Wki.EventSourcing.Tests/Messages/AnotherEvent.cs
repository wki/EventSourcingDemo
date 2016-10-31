using System;
using Wki.EventSourcing.Messages;

namespace Wki.EventSourcing.Tests.Messages
{
    /// <summary>
    /// Event with only a default constructor
    /// </summary>
    public class AnotherEvent : Event
    {
        public string MyText { get; private set; }

        public AnotherEvent(string myText)
        {
            MyText = myText;
        }
    }
}
