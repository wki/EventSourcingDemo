import { Component, OnInit, OnDestroy } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable, Subscription } from 'rxjs/Rx';
import { StatusReport } from './dto/statusreport';

@Component({
    templateUrl: 'templates/status.html',
})
export class StatusComponent implements OnInit, OnDestroy {
    statusReport: StatusReport;
    timer: Observable<number>;
    subscription: Subscription;

    constructor(private http: Http) {
        this.statusReport = new StatusReport([], null);
        this.loadStatusReport();
    }

    ngOnInit() {
        let that = this;
        this.timer = Observable.timer(2000,2000);
        this.subscription = this.timer.subscribe(t => { console.log("tick"); that.loadStatusReport() } );
    }

    ngOnDestroy() {
        this.subscription.unsubscribe();
    }

    loadStatusReport(): void {
        this.http.get("http://localhost:9000/api/status")
            .subscribe(r => this.statusReport = <StatusReport>r.json());
    }
}