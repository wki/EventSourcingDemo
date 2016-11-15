export class OfficeActorChildState {
    constructor(
        public startedAt: Date,
        public lastStatusQuerySentAt: Date,
        public lastStatusReceivedAt: Date,
        public status: number,
        public nrCommandsForwarded: number,
        public lastCommandForwardedAt: Date,
    ) {}
}
