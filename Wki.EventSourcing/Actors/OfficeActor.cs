using System.Linq;
using Akka.Actor;
using Wki.EventSourcing.Messages;
using Wki.EventSourcing.Util;
using static Wki.EventSourcing.Util.Constant;
using Wki.EventSourcing.Protocol.Statistics;
using Wki.EventSourcing.Protocol.LiveCycle;

namespace Wki.EventSourcing.Actors
{
    ///<summary>
    /// Base class for an office actor
    ///</summary>
    ///<description>
    /// An office actor is something like a router. All commands meant for
    /// a given Aggregate root or view with a persistence ID are first sent
    /// to the office. The office creates a not existing destination and
    /// forwards the commands to it. After some idle time, the destination
    /// actor is removed again
    ///</description>
    ///<example>
    /// public class ArticleOffice : OfficeActor&lt;ArticleActor, int&gt;
    /// {
    ///     public ArticleOffice(IActorRef eventStore) : base(eventStore)
    ///     {
    ///         // set idle time (if different from 60 seconds)
    ///         IdleTime = TimeSpan.
    /// 
    ///         // handle some commands in the office.
    ///         // all unhandled will be forwarded to a child actor
    ///         Receive<SomeMessage>(...);
    ///     }
    /// }
    ///</example>
    public abstract class OfficeActor<TActor, TIndex> : ReceiveActor
    {
        // we need to know the eventStore to forward it to our children
        private readonly IActorRef eventStore;

        // maintain a complete state (for knowledge and statistics)
        private OfficeActorStatistics officeActorStatistics;

        protected OfficeActor(IActorRef eventStore)
        {
            // initialize
            this.eventStore = eventStore;
            officeActorStatistics = new OfficeActorStatistics();

            // periodically check for inactive children
            Context.System.Scheduler
                   .ScheduleTellRepeatedly(
                       initialDelay: IdleActorPollTimeSpan,
                       interval: IdleActorPollTimeSpan,
                       receiver: Self,
                       message: new RemoveInactiveActors(),
                       sender: Self
                   );

            // Alive / Graceful passivation
            Receive<StillAlive>(_ => StillAlive());
            Receive<Passivate>(_ => Passivate());
            Receive<DiedDuringRestore>(_ => Lockout());
            Receive<RemoveInactiveActors>(_ => RemoveInactiveActors());

            // diagnostic messages for monitoring
            Receive<GetStatistics>(_ => Sender.Tell(officeActorStatistics));
            Receive<DurableActorStatistics>(d => UpdateChild(d));

            // all commands will pass thru Unhandled()...
        }

        // handle all commands sent to the actor by forwarding
        protected override void Unhandled(object message)
        {
            var command = message as DispatchableCommand<TIndex>;
            if (command != null)
            {
                officeActorStatistics.ForwardedCommand();

                ForwardToDestinationActor(command);
            }
            else
            {
                officeActorStatistics.UnhandledMessage();

                Context.System.Log.Warning("Received {0} -- ignoring", message);
            }
        }

        private void StillAlive() =>
            officeActorStatistics.ChildStillAlive(Sender.Path.Name);

        private void Passivate()
        {
            var name = Sender.Path.Name;
            if (officeActorStatistics.ContainsChild(name))
            {
                Context.System.Log.Info("Office {0}: removed child {1}", Self.Path.Name, name);
                Context.Stop(Sender);
                officeActorStatistics.RemoveChildActor(name);
            }
        }

        private void Lockout()
        {
            // TODO: lock an actor for further requests.
        }

        private void RemoveInactiveActors()
        {
            officeActorStatistics.InactiveActorCheck();

            foreach (var actorName in officeActorStatistics.ChildActorNames())
            {
                var child = Context.Child(actorName);
                var childActorState = officeActorStatistics.ChildActorStates[actorName];

                Context.System.Log.Info(
                    "Office {0}: child {1} - last cmd {2:HH:mm:ss}, oldest allowed {3:HH:mm:ss}",
                    Self.Path.Name,
                    actorName,
                    childActorState.LastCommandForwardedAt,
                    SystemTime.Now - MaxActorIdleTimeSpan
                );

                if (child == Nobody.Instance)
                {
                    // Aktor nicht mehr vorhanden. weg damit
                    Context.System.Log.Info("Office {0}: removing dead Child {1}", Self.Path.Name, actorName);

                    officeActorStatistics.RemoveChildActor(actorName);
                }
                else if (childActorState.LastCommandForwardedAt < SystemTime.Now - MaxActorIdleTimeSpan)
                {
                    // Aktor zu lange inaktiv. auch weg.
                    Context.System.Log.Info("Office {0}: removing inactive Child {1}", Self.Path.Name, actorName);

                    Context.Stop(child);
                    officeActorStatistics.RemoveChildActor(actorName);
                }
                else
                {
                    // Aktuellen Aktor-Status erfragen
                    childActorState.LastStatusQuerySentAt = SystemTime.Now;
                    child.Tell(new GetStatistics());
                }
            }
        }

        // a durable actor has answered to GetState
        private void UpdateChild(DurableActorStatistics durableActorStatistics)
        {
            var actorName = Sender.Path.Name;

            if (officeActorStatistics.ContainsChild(actorName))
            {
                var childActorState = officeActorStatistics.ChildActorStates[actorName];
                childActorState.LastStatusReceivedAt = SystemTime.Now;
                childActorState.Status = durableActorStatistics.Status;
            }
            else
                officeActorStatistics.NrActorsMissed++;
        }

        private void ForwardToDestinationActor(DispatchableCommand<TIndex> cmd)
        {
            var destination = LookupOrCreateChild(cmd.Id);
            var actorName = destination.Path.Name;

            if (officeActorStatistics.ChildActorStates.ContainsKey(actorName))
            {
                var childActorState = officeActorStatistics.ChildActorStates[actorName];
                childActorState.LastCommandForwardedAt = SystemTime.Now;
                childActorState.NrCommandsForwarded++;

                destination.Forward(cmd);
            }
            else
                officeActorStatistics.NrActorsMissed++;
        }

        private IActorRef LookupOrCreateChild(TIndex id)
        {
            var type = typeof(TActor);
            var name = id.ToString();

            var child = Context.Child(name);
            if (child != Nobody.Instance)
                return child;

            Context.System.Log.Info("Office {0}: creating Child {1}", Self.Path.Name, name);

            officeActorStatistics.AddChildActor(name);

            return Context.ActorOf(Props.Create(type, eventStore, id), name);
        }
    }
}