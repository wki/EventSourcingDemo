import { Component, OnInit, OnDestroy } from '@angular/core'; 
import { ViewUpdaterService } from './viewupdater.service';

@Component({})
/** Base class of all views, handles communication with ViewUpdater */
export class View implements OnInit, OnDestroy {
    constructor (private viewUpdaterService: ViewUpdaterService, private name: string) {
    }

    /** must be called when view is shown */
    protected show(): void {
        this.viewUpdaterService.showView(this, name);
    }

    /** must be called when view is hidden */
    protected hide(): void {
        this.viewUpdaterService.hideView(this);
    }

    ngOnInit() {
        console.log('view: onInit');
    }

    ngOnDestroy() {
        console.log('view: onDestroy');
    }
}