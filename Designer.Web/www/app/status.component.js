"use strict";
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
var http_1 = require('@angular/http');
var Rx_1 = require('rxjs/Rx');
var statusreport_1 = require('./dto/statusreport');
var StatusComponent = (function () {
    function StatusComponent(http) {
        this.http = http;
        this.statusReport = new statusreport_1.StatusReport([], null);
        this.loadStatusReport();
    }
    StatusComponent.prototype.ngOnInit = function () {
        var that = this;
        this.timer = Rx_1.Observable.timer(2000, 2000);
        this.subscription = this.timer.subscribe(function (t) { console.log("tick"); that.loadStatusReport(); });
    };
    StatusComponent.prototype.ngOnDestroy = function () {
        this.subscription.unsubscribe();
    };
    StatusComponent.prototype.loadStatusReport = function () {
        var _this = this;
        this.http.get("http://localhost:9000/api/status")
            .subscribe(function (r) { return _this.statusReport = r.json(); });
    };
    StatusComponent = __decorate([
        core_1.Component({
            templateUrl: 'templates/status.html',
        }), 
        __metadata('design:paramtypes', [http_1.Http])
    ], StatusComponent);
    return StatusComponent;
}());
exports.StatusComponent = StatusComponent;
//# sourceMappingURL=status.component.js.map