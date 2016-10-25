using System;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.HangtagCreation.Actors
{
    public class Hangtag : DurableActor<int>
    {
        public Hangtag(int id) : base(id)
        {
            // TODO: Use Cases umsetzen
        }
    }
}
