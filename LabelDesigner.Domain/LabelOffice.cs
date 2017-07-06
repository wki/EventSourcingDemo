using Akka.Actor;
using Wki.EventSourcing.Actors;

namespace LabelDesigner.Domain
{
    /// <summary>
    /// Central hub for all label-related commands
    /// </summary>
    public class LabelOffice : OfficeActor<LabelActor, int>
    {
        private IActorRef labelCreator;

        public LabelOffice(IActorRef eventStore) : base(eventStore)
        {
            labelCreator = Context.ActorOf(Props.Create<LabelCreator>(eventStore), "creator");
        }

        protected override void HandleMessage(object message)
        {
            switch(message)
            {
                case Label.Create create:
                    labelCreator.Forward(create);
                    break;

                case Label.Clone clone:
                    labelCreator.Forward(clone);
                    break;
            }
        }
    }
}
