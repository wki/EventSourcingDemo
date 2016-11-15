"use strict";
var ActorInfo = (function () {
    function ActorInfo(name, state) {
        this.name = name;
        this.state = state;
    }
    return ActorInfo;
}());
var OfficeActorState = (function () {
    function OfficeActorState(startedAt, nrActorsLoaded, lastActorLoadedAt, nrActorsRemoved, lastActorRemovedAt, nrActorChecks, lastActorCheckAt, childActorStates, // Dictionary<string, OfficeActorChildState>;
        nrActorsMissed, nrCommandsForwarded, nrUnhandledMessages, lastCommandForwardedAt) {
        this.startedAt = startedAt;
        this.nrActorsLoaded = nrActorsLoaded;
        this.lastActorLoadedAt = lastActorLoadedAt;
        this.nrActorsRemoved = nrActorsRemoved;
        this.lastActorRemovedAt = lastActorRemovedAt;
        this.nrActorChecks = nrActorChecks;
        this.lastActorCheckAt = lastActorCheckAt;
        this.nrActorsMissed = nrActorsMissed;
        this.nrCommandsForwarded = nrCommandsForwarded;
        this.nrUnhandledMessages = nrUnhandledMessages;
        this.lastCommandForwardedAt = lastCommandForwardedAt;
        this.actors = [];
        for (var name_1 in childActorStates) {
            this.actors.push(new ActorInfo(name_1, childActorStates[name_1]));
        }
    }
    return OfficeActorState;
}());
exports.OfficeActorState = OfficeActorState;
//# sourceMappingURL=officeactorstate.js.map