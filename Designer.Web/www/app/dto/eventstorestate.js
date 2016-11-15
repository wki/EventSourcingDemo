"use strict";
var EventStoreState = (function () {
    function EventStoreState(status, statusChangedAt, startedAt, loadDuration, nrEventsLoaded, nrStashedCommands, nrActorsRestored, nrStillAliveReceived, nrSubscribers, lastEventPersistedAt, nrEventsPersisted, nrEventsTotal) {
        this.status = status;
        this.statusChangedAt = statusChangedAt;
        this.startedAt = startedAt;
        this.loadDuration = loadDuration;
        this.nrEventsLoaded = nrEventsLoaded;
        this.nrStashedCommands = nrStashedCommands;
        this.nrActorsRestored = nrActorsRestored;
        this.nrStillAliveReceived = nrStillAliveReceived;
        this.nrSubscribers = nrSubscribers;
        this.lastEventPersistedAt = lastEventPersistedAt;
        this.nrEventsPersisted = nrEventsPersisted;
        this.nrEventsTotal = nrEventsTotal;
    }
    return EventStoreState;
}());
exports.EventStoreState = EventStoreState;
//# sourceMappingURL=eventstorestate.js.map