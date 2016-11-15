"use strict";
var OfficeActorChildState = (function () {
    function OfficeActorChildState(startedAt, lastStatusQuerySentAt, lastStatusReceivedAt, status, nrCommandsForwarded, lastCommandForwardedAt) {
        this.startedAt = startedAt;
        this.lastStatusQuerySentAt = lastStatusQuerySentAt;
        this.lastStatusReceivedAt = lastStatusReceivedAt;
        this.status = status;
        this.nrCommandsForwarded = nrCommandsForwarded;
        this.lastCommandForwardedAt = lastCommandForwardedAt;
    }
    return OfficeActorChildState;
}());
exports.OfficeActorChildState = OfficeActorChildState;
//# sourceMappingURL=officeactorchildstate.js.map