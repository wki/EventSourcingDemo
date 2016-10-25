using System;
using System.Collections.Generic;
using Designer.Domain.HangtagCreation.Messages;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.Rendering.Actors
{
    public class RenderObserver : DurableActor<int?>
    {
        // hangtag id => start time
        private Dictionary<int, DateTime> RenderingRequestedAt;

        public RenderObserver()
        {
            // Id is set to null by default. So we receive events for all actors

            // Lauschen auf PDF Erzeugung starten/beenden
            Recover<RenderingRequested>(r => RenderingRequested(r));
            Recover<RenderingCompleted>(r => RenderingCompleted(r));

            // außerhalb der Recovery:
            // für alle noch eingetragenen: Render-Kind starten
            // Render-Kind erzeugt PDF und meldet an diesen Observer zurück.
        }

        private void RenderingRequested(RenderingRequested renderingRequested)
        {
            RenderingRequestedAt[renderingRequested.Id] = renderingRequested.OccuredOn;
        }

        private void RenderingCompleted(RenderingCompleted renderingCompleted)
        {
            RenderingRequestedAt.Remove(renderingCompleted.Id);
        }
    }
}
