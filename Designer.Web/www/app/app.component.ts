import { Component, OnInit, OnDestroy } from '@angular/core';
import { ViewUpdaterService } from './view/viewupdater.service';
import { View } from './view/view';

@Component({
    selector: 'my-app',
    templateUrl: 'templates/app.html',
    providers: [ViewUpdaterService]
})
export class AppComponent extends View implements OnInit, OnDestroy {
    constructor(viewUpdaterService: ViewUpdaterService) {
        super(viewUpdaterService, 'app');

        console.log(`Received viewupdater: ${viewUpdaterService}`);
    }

    ngOnInit() {
        super.ngOnInit();
        console.log('app: onInit');
        this.show();
    }

    ngOnDestroy() {
        console.log('app: onDestroy');
        super.ngOnDestroy();
        this.hide();
    }
}
