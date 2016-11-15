import { Component, OnInit, OnDestroy } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable, Subscription } from 'rxjs/Rx';
import { OfficeActorState } from './dto/officeactorstate';

@Component({
    templateUrl: 'templates/officeactorstate.html',
})
export class PersonOfficeStateComponent implements OnInit, OnDestroy {
    state: OfficeActorState;
    timer: Observable<number>;
    subscription: Subscription;

    constructor(private http: Http) {
        this.state = null;
        this.loadState();
    }

    ngOnInit() {
        let that = this;
        this.timer = Observable.timer(2000,2000);
        this.subscription = this.timer.subscribe(t => { console.log("tick"); that.loadState() } );
    }

    ngOnDestroy() {
        this.subscription.unsubscribe();
    }

    loadState(): void {
        this.http.get("http://localhost:9000/api/status/person")
            .subscribe(r => this.state = <OfficeActorState>r.json());
    }
}