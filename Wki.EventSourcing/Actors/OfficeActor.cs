using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Wki.EventSourcing.Messages;
using Wki.EventSourcing.Util;
using static Wki.EventSourcing.Util.Constant;

namespace Wki.EventSourcing.Actors
{
    ///<summary>
    /// Base class for an office actor
    ///</summary>
    ///<description>
    /// An office actor is something like a router. All messages meant for
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
        private OfficeActorState officeActorState;

        protected OfficeActor(IActorRef eventStore)
        {
            // initialize
            this.eventStore = eventStore;
            officeActorState = new OfficeActorState();

            // periodically check for inactive children
            Context.System.Scheduler
                   .ScheduleTellRepeatedly(
                       initialDelay: IdleActorPollTimeSpan,
                       interval: IdleActorPollTimeSpan,
                       receiver: Self,
                       message: new CheckInactiveActors(),
                       sender: Self
                   );

            // diagnostic messages for testing
            Receive<GetSize>(_ => Sender.Tell(officeActorState.ChildActorStates.Count));
            Receive<GetActors>(_ => Sender.Tell(String.Join("|", officeActorState.ChildActorStates.Keys.Select(k => k))));
            Receive<CheckInactiveActors>(_ => CheckInactiveActors());

            // diagnostic messages for monitoring
            Receive<GetState>(_ => Sender.Tell(officeActorState));
            Receive<DurableActorState>(d => UpdateChild(d));

            // all commands will pass thru Unhandled()...
        }

        protected override void Unhandled(object message)
        {
            var command = message as DispatchableCommand<TIndex>;
            if (command != null)
            {
                officeActorState.NrCommandsForwarded++;
                officeActorState.LastCommandForwardedAt = SystemTime.Now;

                ForwardToDestinationActor(command);
            }
            else
            {
                officeActorState.NrUnhandledMessages++;

                Context.System.Log.Warning("Received {0} -- ignoring", message);
            }
        }

        private void CheckInactiveActors()
        {
            officeActorState.NrActorChecks++;
            officeActorState.LastActorCheckAt = SystemTime.Now;

            foreach (var actorName in officeActorState.ChildActorStates.Keys.ToList())
            {
                var child = Context.Child(actorName);
                var childActorState = officeActorState.ChildActorStates[actorName];
                if (child == Nobody.Instance)
                {
                    // Aktor nicht mehr vorhanden. weg damit
                    Context.System.Log.Info("Office {0}: removing dead Child {1}", Self.Path.Name, actorName);

                    officeActorState.RemoveChildActor(actorName);
                }
                else if (childActorState.LastCommandForwardedAt < SystemTime.Now - MaxActorIdleTimeSpan)
                {
                    // Aktor zu lange inaktiv. auch weg.
                    Context.System.Log.Info("Office {0}: removing inactive Child {1}", Self.Path.Name, actorName);

                    Context.Stop(child);
                    officeActorState.RemoveChildActor(actorName);
                }
                else
                {
                    // Aktuellen Aktor-Status erfragen
                    childActorState.LastStatusQuerySentAt = SystemTime.Now;
                    child.Tell(new GetState());
                }
            }
        }

        private void UpdateChild(DurableActorState durableActorState)
        {
            var actorName = Sender.Path.Name;

            if (officeActorState.ChildActorStates.ContainsKey(actorName))
            {
                var childActorState = officeActorState.ChildActorStates[actorName];
                childActorState.LastStatusReceivedAt = SystemTime.Now;
                childActorState.Status = durableActorState.Status;
            }
            else
                officeActorState.NrActorsMissed++;
        }

        private void ForwardToDestinationActor(DispatchableCommand<TIndex> cmd)
        {
            var destination = CreateOrLoadChild(cmd.Id);
            var actorName = destination.Path.Name;

            if (officeActorState.ChildActorStates.ContainsKey(actorName))
            {
                var childActorState = officeActorState.ChildActorStates[actorName];
                childActorState.LastCommandForwardedAt = SystemTime.Now;
                childActorState.NrCommandsForwarded++;

                destination.Forward(cmd);
            }
            else
                officeActorState.NrActorsMissed++;
        }

        private IActorRef CreateOrLoadChild(TIndex id)
        {
            var type = typeof(TActor);
            var name = id.ToString();

            var child = Context.Child(name);
            if (child != Nobody.Instance)
                return child;

            Context.System.Log.Info("Office {0}: creating Child {1}", Self.Path.Name, name);

            officeActorState.NrActorsLoaded++;
            officeActorState.LastActorLoadedAt = SystemTime.Now;

            return Context.ActorOf(Props.Create(type, eventStore, id), name);
        }
    }
}