using System.Linq;
using Akka.Actor;
using Wki.EventSourcing.Messages;
using Wki.EventSourcing.Util;
using static Wki.EventSourcing.Util.Constant;
using Wki.EventSourcing.Protocol.Statistics;
using Wki.EventSourcing.Protocol.LiveCycle;
using System.Collections.Generic;

namespace Wki.EventSourcing.Actors
{
    ///<summary>
    /// Base class for an office actor
    ///</summary>
    ///<description>
    /// An office actor is something like a router. All commands meant for
    /// a given Aggregate root or view with a persistence ID are first sent
    /// to the office. The office creates a not existing clerk and
    /// forwards the commands to it. 
    /// Clerks not reporting alive or asking for passivation are removed.
    /// Clerks dying during restore are blocked forever.
    ///</description>
    ///<example>
    /// // Office handles Articles with Index type int
    /// public class ArticleOffice : OfficeActor&lt;ArticleActor, int&gt;
    /// {
    ///     public ArticleOffice(IActorRef eventStore) : base(eventStore)
    ///     {
    ///         // handle some messages inside the office.
    ///         // all unhandled commands of Type DispatchableCommand<>
    ///         // will be forwarded to a clerk
    ///         Receive<SomeMessage>(...);
    ///     }
    /// }
    ///</example>
    public abstract class OfficeActor<TActor, TIndex> : ReceiveActor
    {
        // we need to know the eventStore to forward it to our clerks
        private readonly IActorRef eventStore;

        // our clerks with some state information for each of them 
        private Dictionary<string, ClerkState> clerks;
        
        private OfficeActorStatistics statistics;

        protected OfficeActor(IActorRef eventStore)
        {
            // initialize
            this.eventStore = eventStore;
            clerks = new Dictionary<string, ClerkState>();
            statistics = new OfficeActorStatistics();

            // periodically check for dead clerks
            Context.System.Scheduler
                   .ScheduleTellRepeatedly(
                       initialDelay: DeadActorRemoveTimeSpan,
                       interval: DeadActorRemoveTimeSpan,
                       receiver: Self,
                       message: new RemoveInactiveActors(),
                       sender: Self
                   );

            // Alive / Graceful passivation
            Receive<StillAlive>(_ => StillAlive());
            Receive<Passivate>(_ => Passivate());
            Receive<DiedDuringRestore>(_ => Lockout());

            // timer
            Receive<RemoveInactiveActors>(_ => RemoveDeadActors());

            // statistics
            Receive<GetStatistics>(_ => Sender.Tell(statistics));

            // all commands will pass thru Unhandled() if not handled by derived class...
        }

        // commands are not handled in the derived office actor
        // so they appear here and are forwarded to the clerk
        // if they can be identified as commands.
        protected override void Unhandled(object message)
        {
            var command = message as DispatchableCommand<TIndex>;
            if (command != null)
            {
                statistics.ForwardedCommand();

                ForwardToClerk(command);
            }
            else
            {
                statistics.UnhandledMessage();

                Context.System.Log.Warning("Office {0}: Received {1} -- ignoring", Self.Path.Name, message);
            }
        }

        // a clerk just told he is alive
        private void StillAlive()
        {
            var name = Sender.Path.Name;
            if (clerks.ContainsKey(name))
                clerks[name].StillAlive();
            else
                Context.System.Log.Warning("Office {0}: Received 'StillAlive' from unknown actor '{1}' - ignored", Self.Path.Name, Sender.Path);
        }
            
        // a clerk wants to get removed
        private void Passivate()
        {
            // FIXME: did we recently forward a message to this clerk?

            var name = Sender.Path.Name;
            if (clerks.ContainsKey(name))
            {
                Context.System.Log.Info("Office {0}: removed clerk {1}", Self.Path.Name, name);
                Context.Stop(Sender);
                clerks.Remove(name);
                statistics.ActorRemoved();
            }
            else
                Context.System.Log.Warning("Office {0}: Received 'Passivate' from unknown actor '{1}' - ignored", Self.Path.Name, Sender.Path);
        }

        // a clerk died during restore and cannot be reused any more
        private void Lockout()
        {
            var name = Sender.Path.Name;
            if (clerks.ContainsKey(name))
            {
                Context.System.Log.Error("Office {0}: clerk '{1}' died during restore - locking out", Self.Path.Name, Sender.Path);
                clerks[name].ChangeStatus(ClerkStatus.DiedDuringRestore);
            }
            else
                Context.System.Log.Warning("Office {0}: Received 'DiedDuringRestore' from unknown actor '{1}' - ignored", Self.Path.Name, Sender.Path);
        }

        // periodically remove clerks not reported themselves alive
        // should never happen...
        private void RemoveDeadActors()
        {
            foreach (var clerkState in clerks.Values.Where(c => c.IsDead()).ToList())
            {
                var clerk = clerkState.Clerk;
                var name = clerk.Path.Name;
                Context.System.Log.Info("Office {0}: removing dead clerk '{1}'", Self.Path.Name, name);

                Context.Stop(clerk);
                clerks.Remove(name);
            }
        }

        private void ForwardToClerk(DispatchableCommand<TIndex> cmd)
        {
            var destination = LookupOrCreateClerk(cmd.Id);
            if (destination == null)
            {
                statistics.DiscardedCommand();
                Context.System.Log.Warning("Office {0}: discarding command {1}", Self.Path.Name, cmd);
                Sender.Tell(Reply.Error("message '{0}' discarded: clerk {1} died during restore", cmd, cmd.Id));
            }
            else
            {
                statistics.ForwardedCommand();
                destination.Forward(cmd);
            }
        }

        private IActorRef LookupOrCreateClerk(TIndex id)
        {
            var name = id.ToString();
            if (clerks.ContainsKey(name))
            {
                // we know this clerk. Ensure he did not die during restore
                var clerk = clerks[name];
                if (clerk.IsOperating())
                    return clerk.Clerk;
                else
                    return null;
            }
            else
            {
                Context.System.Log.Info("Office {0}: creating clerk {1}", Self.Path.Name, name);

                var clerk = Context.ActorOf(Props.Create(typeof(TActor), eventStore, id), name);
                clerks.Add(name, new ClerkState(clerk));

                return clerk;
            }
        }
    }
}