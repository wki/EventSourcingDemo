import { OfficeActorChildState } from './officeactorchildstate';

class ActorInfo {
    constructor(
        public name: string,
        public state: OfficeActorChildState,
    ) {}
}

export class OfficeActorState {
    public actors: ActorInfo [];

    constructor(
        public startedAt: Date,

        public nrActorsLoaded: number,
        public lastActorLoadedAt: Date,
        public nrActorsRemoved: number,
        public lastActorRemovedAt: Date,

        public nrActorChecks: number,
        public lastActorCheckAt: Date,

        public childActorStates: any,

        public nrActorsMissed: number,
        public nrCommandsForwarded: number,
        public nrUnhandledMessages: number,
        public lastCommandForwardedAt: Date,
    ) {
        this.actors = [];

        for (let name in childActorStates) {
            this.actors.push(new ActorInfo(name, childActorStates[name]));
        }
    }
}