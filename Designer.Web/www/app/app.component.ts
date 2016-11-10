import { Component, OnInit, OnDestroy } from '@angular/core';
import { ViewUpdaterService } from './view/viewupdater.service';
import { View } from './view/view';

@Component({
    selector: 'my-app',
    template: `<div class="jumbotron">
    <div class="container">
    <h1>My Next Angular App</h1>
    </div>
    </div>`,
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
