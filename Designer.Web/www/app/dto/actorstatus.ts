export class ActorStatus {
    constructor(
        public path: string, 
        public status: string, 
        public lastSeen: Date, 
        public events: string[] 
    ) {}
}
