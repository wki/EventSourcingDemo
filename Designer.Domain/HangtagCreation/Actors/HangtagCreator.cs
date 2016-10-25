using System;
using Designer.Domain.HangtagCreation.Messages;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.HangtagCreation.Actors
{
    public class HangtagCreator : DurableActor<Hangtag>
    {
        // after persisting this id is updated
        private int lastPersistedId;

        // id to be used for next registration to avoid race conditions
        private int nextUsableId;

        public HangtagCreator()
        {
            lastPersistedId = 0;
            nextUsableId = 1;

            Command<CreateHangtag>(h => CreateHangtag(h));
            Command<CloneHangtag>(h => CloneHangtag(h));

            Recover<HangtagCreated>(h => HangtagCreated(h));
            Recover<HangtagCloned>(h => HangtagCloned(h));
        }

        private void CreateHangtag(CreateHangtag createHangtag)
        {
            Persist(new HangtagCreated(nextUsableId++));
        }

        private void CloneHangtag(CloneHangtag cloneHangtag)
        {
            Persist(new HangtagCloned(nextUsableId++));
        }

        private void HangtagCreated(HangtagCreated hangtagCreated)
        {
            lastPersistedId = hangtagCreated.Id;

            if (nextUsableId < lastPersistedId + 1)
                nextUsableId = lastPersistedId + 1;
        }

        private void HangtagCloned(HangtagCloned hangtagCloned)
        {
            lastPersistedId = hangtagCloned.Id;

            if (nextUsableId < lastPersistedId + 1)
                nextUsableId = lastPersistedId + 1;
        }
   }
}
