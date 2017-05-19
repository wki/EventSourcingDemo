using System;
using Akka.Actor;
using Wki.EventSourcing.Infrastructure;
using static Wki.EventSourcing.Util.Constant;

namespace Wki.EventSourcing.Actors
{
    public abstract class OfficeActor<TAggregate, TIndex>: UntypedActor
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
                   message: new RemoveInactiveActors(),
                   sender: Self
                );

        }

        protected override void OnReceive(object message)
        {
            switch(message)
            {
                case DispatchableCommand<TIndex> cmd:
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
            }
        }
    }
}
