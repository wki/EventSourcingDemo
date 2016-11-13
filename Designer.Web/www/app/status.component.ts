import { Component } from '@angular/core';
import { Http, Response } from '@angular/http';

class ActorStatus {
    constructor(
        public path: string, 
        public status: string, 
        public lastSeen: Date, 
        public events: string[] 
    ) {}
}
class StatusReport {
    constructor(
        public startedAt: Date,
        public loadDurationMilliseconds: number,
        public status: string,
        public eventStoreSize: number,
        public actors: ActorStatus[],
    ) {}
}

@Component({
    templateUrl: 'templates/status.html',
})
export class StatusComponent {
    statusReport: StatusReport;

    constructor(private http: Http) {
        this.statusReport = new StatusReport(null, 0, "Init", 0, []);
        this.loadStatusReport();
    }

    loadStatusReport(): void {
        this.http.get("http://localhost:9000/api/status")
            .subscribe(r => this.statusReport = <StatusReport>r.json());
    }
}