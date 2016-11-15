export class EventStoreState {
    constructor(
        public status: string,
        public statusChangedAt: Date,
        public startedAt: Date,
        public loadDuration: string,
        public nrEventsLoaded: number,
        public nrStashedCommands: number,
        public nrActorsRestored: number,
        public nrStillAliveReceived: number,
        public nrSubscribers: number,
        public lastEventPersistedAt: Date,
        public nrEventsPersisted: number,
        public nrEventsTotal: number,
    ) {}
}
