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

        // remember last contact with an actor
        private Dictionary<string, DateTime> LastContact;

        // maintain a complete state (for knowledge and statistics)
        private OfficeActorState officeActorState;

        protected OfficeActor(IActorRef eventStore)
        {
            // initialize
            this.eventStore = eventStore;
            LastContact = new Dictionary<string, DateTime>();
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
            Receive<GetSize>(_ => Sender.Tell(LastContact.Count));
            Receive<GetActors>(_ => Sender.Tell(String.Join("|", LastContact.Keys.Select(k => k))));
            Receive<CheckInactiveActors>(_ => CheckInactiveActors());

            // diagnostic messages for monitoring
            Receive<GetState>(_ => Sender.Tell(officeActorState));

            // all commands will pass thru Unhandled()...
        }

        protected override void Unhandled(object message)
        {
            var command = message as DispatchableCommand<TIndex>;
            if (command != null)
            {
                officeActorState.NrCommandsForwarded++;

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
            foreach (var actorName in LastContact.Keys)
            {
                var child = Context.Child(actorName);
                if (child == Nobody.Instance)
                {
                    Context.System.Log.Debug("Office {0}: removing dead Child {1}", Self.Path.Name, actorName);

                    officeActorState.NrActorsRemoved++;
                    officeActorState.LastActorRemovedAt = SystemTime.Now;

                    LastContact.Remove(actorName);
                }
                else if (LastContact[actorName] < SystemTime.Now - MaxActorIdleTimeSpan)
                {
                    Context.System.Log.Debug("Office {0}: removing inactive Child {1}", Self.Path.Name, actorName);

                    officeActorState.NrActorsRemoved++;
                    officeActorState.LastActorRemovedAt = SystemTime.Now;

                    Context.Stop(child);
                    LastContact.Remove(actorName);
                }
            }
        }

        private void ForwardToDestinationActor(DispatchableCommand<TIndex> cmd)
        {
            var destination = CreateOrLoadChild(cmd.Id);
            LastContact[destination.Path.Name] = SystemTime.Now;

            destination.Forward(cmd);
        }

        private IActorRef CreateOrLoadChild(TIndex id)
        {
            var type = typeof(TActor);
            var name = id.ToString();

            var child = Context.Child(name);
            if (child != Nobody.Instance)
                return child;

            Context.System.Log.Debug("Office {0}: creating Child {1}", Self.Path.Name, name);

            officeActorState.NrActorsRemoved++;
            officeActorState.LastActorRemovedAt = SystemTime.Now;

            return Context.ActorOf(Props.Create(type, eventStore, id), name);
        }
    }
}