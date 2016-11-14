import { Component, OnInit, OnDestroy } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable } from 'rxjs/Rx';

class ActorStatus {
    constructor(
        public path: string, 
        public status: string, 
        public lastSeen: Date, 
        public events: string[] 
    ) {}
}

class EventStoreState {
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

class StatusReport {
    constructor(
        public actors: ActorStatus[],
        public eventStoreState: EventStoreState,
    ) {}
}

@Component({
    templateUrl: 'templates/status.html',
})
export class StatusComponent implements OnInit, OnDestroy {
    statusReport: StatusReport;
    timer: Observable<number>;

    constructor(private http: Http) {
        this.statusReport = new StatusReport([], null);
        this.loadStatusReport();
    }

    ngOnInit() {
        let that = this;
        this.timer = Observable.timer(2000,2000);
        this.timer.subscribe(t => { console.log("tick"); that.loadStatusReport() } );
    }

    ngOnDestroy() {
        // this.subscription.unsubscribe();
    }

    loadStatusReport(): void {
        this.http.get("http://localhost:9000/api/status")
            .subscribe(r => this.statusReport = <StatusReport>r.json());
    }
}