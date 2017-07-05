using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using Wki.EventSourcing.Protocol.LifeCycle;
using Wki.EventSourcing.Util;
using static Wki.EventSourcing.Util.Constant;

namespace Wki.EventSourcing.Actors
{
    public abstract class OfficeActor<TClerk, TIndex> : UntypedActor
    {
        #region private classes
        private class ClerkStatus
        {
            public IActorRef Actor;
            public DateTime LatestCommand;
            public DateTime LatestKeepalive;

            public ClerkStatus(IActorRef actor)
            {
                Actor = actor;
                LatestKeepalive = SystemTime.Now;
                LatestCommand = SystemTime.Now;
            }

            public bool LastContactBefore(DateTime before) =>
                LatestKeepalive < before || LatestCommand < before;

        }
        #endregion

        private IActorRef eventStore;

        private Dictionary<string, ClerkStatus> clerks;

        public OfficeActor(IActorRef eventStore)
        {
            this.eventStore = eventStore;
            clerks = new Dictionary<string, ClerkStatus>();

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
            string name;

            switch (message)
            {
                case ICommand<TIndex> cmd:
                    name = cmd.Id.ToString();
                    if (!clerks.ContainsKey(name))
                    {
                        var clerk = Context.ActorOf(Props.Create(typeof(TClerk), eventStore, cmd.Id), name);
                        clerks[name] = new ClerkStatus(clerk);
                    }
                    clerks[name].LatestCommand = SystemTime.Now;
                    clerks[name].Actor.Forward(message);
                    break;

                case StillAlive _:
                    name = Sender.Path.Name;
                    if (clerks.ContainsKey(name))
                        clerks[name].LatestKeepalive = SystemTime.Now;
                    break;

                case Passivate _:
                    name = Sender.Path.Name;
                    if (clerks.ContainsKey(name))
                    {
                        clerks.Remove(name);
                        Sender.Tell(PoisonPill.Instance);
                    }
                    break;

                case RemoveInactiveActors _:
                    var oldestAllowedTime = SystemTime.Now - DeadActorRemoveTimeSpan;
                    clerks.Keys
                        .Where(c => clerks[c].LastContactBefore(oldestAllowedTime))
                        .ToList()
                        .ForEach(c =>
                        {
                            clerks[c].Actor.Tell(PoisonPill.Instance);
                            clerks.Remove(c);
                        });
                    break;

                default:
                    HandleMessage(message);
                    break;
            }
        }

        /// <summary>
        /// an overloadable handler for all not handed messages
        /// </summary>
        /// <param name="message"></param>
        protected virtual void HandleMessage(object message) { }
    }
}
