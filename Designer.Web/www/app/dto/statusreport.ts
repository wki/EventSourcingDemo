import { ActorStatus } from './actorstatus';
import { EventStoreState } from './eventstorestate';

export class StatusReport {
    constructor(
        public actors: ActorStatus[],
        public eventStoreState: EventStoreState,
    ) {}
}
