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
var ActorStatus = (function () {
    function ActorStatus(path, status, lastSeen, events) {
        this.path = path;
        this.status = status;
        this.lastSeen = lastSeen;
        this.events = events;
    }
    return ActorStatus;
}());
var StatusReport = (function () {
    function StatusReport(startedAt, loadDurationMilliseconds, status, eventStoreSize, actors) {
        this.startedAt = startedAt;
        this.loadDurationMilliseconds = loadDurationMilliseconds;
        this.status = status;
        this.eventStoreSize = eventStoreSize;
        this.actors = actors;
    }
    return StatusReport;
}());
var StatusComponent = (function () {
    function StatusComponent(http) {
        this.http = http;
        this.statusReport = new StatusReport(null, 0, "Init", 0, []);
        this.loadStatusReport();
    }
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