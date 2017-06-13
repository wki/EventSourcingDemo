﻿using Akka.Actor;
using Wki.EventSourcing.Protocol.LiveCycle;
using static Wki.EventSourcing.Util.Constant;

namespace Wki.EventSourcing.Actors
{
    public abstract class OfficeActor<TClerk, TIndex>: UntypedActor
    {
        public IActorRef EventStore;

        public OfficeActor(IActorRef eventStore)
        {
            EventStore = eventStore;

            Context.System.Scheduler
               .ScheduleTellRepeatedly(
                   initialDelay: DeadActorRemoveTimeSpan,
                   interval: DeadActorRemoveTimeSpan,
                   receiver: Self,
                   message: RemoveInactiveActors.Instance,
                   sender: Self
                );
        }

        protected override void OnReceive(object message)
        {
            // TODO: wie bauen wir ein Coordination Office?
            //       evtl. Receptor -> Message an Office?
            switch(message)
            {
                case ICommand<TIndex> cmd:
                    // TODO: find or create matching office actor
                    break;

                case StillAlive _:
                    // TODO: update statistics
                    break;

                case Passivate _:
                    // TODO: passivate
                    break;

                case RemoveInactiveActors _:
                    // TODO: remove them
                    break;

                default:
                    break;
            }
        }
    }
}
