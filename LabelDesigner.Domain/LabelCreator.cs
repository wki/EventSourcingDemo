using System;
using Akka.Actor;
using Wki.EventSourcing;
using Wki.EventSourcing.Actors;

namespace LabelDesigner.Domain
{
    /// <summary>
    /// Create or clone new Labels by assigning increasing Ids
    /// </summary>
    /// <remarks>
    /// handles Label.Create and Label.Clone commands
    /// creates Label.Created and Label.Cloned events with next available Id
    /// </remarks>
    public class LabelCreator : DurableActor
    {
        private int _lastPersistedId;
        private int lastPersistedId
        {
            get => _lastPersistedId;
            set
            {
                _lastPersistedId = value;
                if (nextUsableId < _lastPersistedId + 1)
                    nextUsableId = _lastPersistedId + 1;
            }
        }
        private int nextUsableId;

        public LabelCreator(IActorRef eventStore) : base(eventStore)
        {
            nextUsableId = 1;
            lastPersistedId = 0;
        }

        protected override EventFilter BuildEventFilter() =>
            WantEvents
                .Allow<Label.Created>()
                .Allow<Label.Cloned>();

        protected override void HandleCommand(ICommand command)
        {
            // must fake persistence Id to ensure LabelActor will receive 
            // the persisted events created below
            var usePersistenceId = $"Label-{nextUsableId}";

            switch (command)
            {
                case Label.Create _:
                    Persist(new Label.Created(nextUsableId), usePersistenceId);
                    nextUsableId++;
                    break;

                case Label.Clone clone:
                    Persist(new Label.Cloned(nextUsableId), usePersistenceId);
                    nextUsableId++;
                    break;
            }
        }

        protected override void ApplyEvent(IEvent @event)
        {
            switch(@event)
            {
                case Label.Created created:
                    lastPersistedId = created.Id;
                    break;

                case Label.Cloned cloned:
                    lastPersistedId = cloned.Id;
                    break;
            }
        }
    }
}