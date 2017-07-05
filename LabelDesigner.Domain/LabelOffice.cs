using Akka.Actor;
using Wki.EventSourcing.Actors;

namespace LabelDesigner.Domain
{
    public class LabelOffice : OfficeActor<LabelActor, int>
    {
        public LabelOffice(IActorRef eventStore) : base(eventStore)
        {
        }

        // if we have to handle special cases...
        //protected override void HandleMessage(object message)
        //{
        //    base.HandleMessage(message);

        //    // do our own.
        //}
    }
}
