"use strict";
var ActorStatus = (function () {
    function ActorStatus(path, status, lastSeen, events) {
        this.path = path;
        this.status = status;
        this.lastSeen = lastSeen;
        this.events = events;
    }
    return ActorStatus;
}());
exports.ActorStatus = ActorStatus;
//# sourceMappingURL=actorstatus.js.map