"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var core_1 = require('@angular/core');
var viewupdater_service_1 = require('./view/viewupdater.service');
var view_1 = require('./view/view');
var AppComponent = (function (_super) {
    __extends(AppComponent, _super);
    function AppComponent(viewUpdaterService) {
        _super.call(this, viewUpdaterService, 'app');
        console.log("Received viewupdater: " + viewUpdaterService);
    }
    AppComponent.prototype.ngOnInit = function () {
        _super.prototype.ngOnInit.call(this);
        console.log('app: onInit');
        this.show();
    };
    AppComponent.prototype.ngOnDestroy = function () {
        console.log('app: onDestroy');
        _super.prototype.ngOnDestroy.call(this);
        this.hide();
    };
    AppComponent = __decorate([
        core_1.Component({
            selector: 'my-app',
            templateUrl: 'templates/app.html',
            providers: [viewupdater_service_1.ViewUpdaterService]
        }), 
        __metadata('design:paramtypes', [viewupdater_service_1.ViewUpdaterService])
    ], AppComponent);
    return AppComponent;
}(view_1.View));
exports.AppComponent = AppComponent;
//# sourceMappingURL=app.component.js.map