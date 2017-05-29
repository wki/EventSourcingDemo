using System;
using Akka.Actor;
using Wki.EventSourcing.Protocol;

namespace Wki.EventSourcing.Persistence
{
    public class EventStore : UntypedActor, IWithUnboundedStash
    {
        public IStash Stash { get; set; }
        public IActorRef Journal;
        public Subscriptions Subscriptions = new Subscriptions();
        public EventCache EventCache = new EventCache();

        public EventStore(IActorRef journal)
        {
            Journal = journal;

            // TODO: wir sollten blockweise zu 1000 Stück laden
            Journal.Tell(LoadNextEvents.FromBeginning);
            Become(Loading);
        }

        protected override void OnReceive(object message)
        {
            throw new NotImplementedException();
        }

        private void Loading(object message)
        {
            switch(message)
            {
                case EventRecord e:
                    EventCache.Append(e);
                    break;

                case End _:
                    Become(OnReceive);
                    break;
            }
        }
    }
}
