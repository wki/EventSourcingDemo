using System;
using System.Collections.Generic;
using Akka.Actor;
using Wki.EventSourcing.Messages;

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
    ///     public ArticleOffice()
    ///     {
    ///         // set idle time (if different from 60 seconds)
    ///         IdleTime = TimeSpan.
    /// 
    ///         // set type of actor to forward
    ///         ActorType = typeof(ArticleActor);
    ///     }
    /// }
    ///</example>
    public class OfficeActor<TActor, TIndex> : ReceiveActor
    {
        private class CheckInactiveActors { }

        // Idle time and its default value
        private const int DefaultIdleSeconds = 60;
        protected TimeSpan IdleTime { get; set; }

        // remember last contact with an actor
        private Dictionary<string, DateTime> LastContact;

        public OfficeActor()
        {
            // initialize
            IdleTime = TimeSpan.FromSeconds(DefaultIdleSeconds);
            LastContact = new Dictionary<string, DateTime>();

            // periodically check for inactive children
            Context.System.Scheduler
                   .ScheduleTellRepeatedly(
                       initialDelay: IdleTime,
                       interval: IdleTime,
                       receiver: Self,
                       message: new CheckInactiveActors(),
                       sender: Self
                   );
            Receive<CheckInactiveActors>(_ => RemoveInactiveActors());

            // OBSOLETE -- use unhandled instead to give child class a chance for special behavior
            // forward all commands
            //Receive<DispatchableCommand>(c => ForwardToDestinationActor(c));
        }

        protected override void Unhandled(object message)
        {
            var command = message as DispatchableCommand<TIndex>;
            if (command != null)
                ForwardToDestinationActor(command);
            else
                Context.System.Log.Warning("Received {0} -- ignoring", message);
        }

        private void RemoveInactiveActors()
        {
            foreach (var actorName in LastContact.Keys)
            {
                var child = Context.Child(actorName);
                if (child == Nobody.Instance)
                {
                    Context.System.Log.Debug("Office {0}: removing dead Child {1}", Self.Path.Name, actorName);
                    LastContact.Remove(actorName);
                }
                else if (LastContact[actorName] < DateTime.Now - IdleTime)
                {
                    Context.System.Log.Debug("Office {0}: removing inactive Child {1}", Self.Path.Name, actorName);
                    Context.Stop(child);
                    LastContact.Remove(actorName);
                }
            }
        }

        private void ForwardToDestinationActor(DispatchableCommand<TIndex> cmd)
        {
            var destination = CreateOrLoadChild(cmd.Id);
            LastContact[destination.Path.Name] = DateTime.Now;

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

            return Context.ActorOf(Props.Create(type, id), name);
        }
    }
}