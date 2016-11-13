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
var RegisterState;
(function (RegisterState) {
    RegisterState[RegisterState["Register"] = 0] = "Register";
    RegisterState[RegisterState["Saving"] = 1] = "Saving";
    RegisterState[RegisterState["Saved"] = 2] = "Saved";
    RegisterState[RegisterState["Failed"] = 3] = "Failed";
})(RegisterState || (RegisterState = {}));
var RegisterComponent = (function () {
    function RegisterComponent(http) {
        this.http = http;
        this.state = RegisterState.Register;
        this.email = undefined;
        this.fullname = undefined;
        this.message = '';
    }
    RegisterComponent.prototype.saveRegistration = function () {
        var _this = this;
        console.log("save registration email=" + this.email + ", fullname=" + this.fullname);
        this.state = RegisterState.Saving;
        var headers = new http_1.Headers();
        headers.append("Content-Type", 'application/json');
        this.http
            .post('http://localhost:9000/api/person/register', JSON.stringify({ email: this.email, fullname: this.fullname }), { headers: headers })
            .subscribe(function (_) { return _this.savedRegistration(); }, function (e) { return _this.failedSaving(e); });
    };
    RegisterComponent.prototype.savedRegistration = function () {
        console.log('saved registration');
        this.state = RegisterState.Saved;
    };
    RegisterComponent.prototype.failedSaving = function (e) {
        console.log('failed saving registration');
        this.state = RegisterState.Failed;
        this.message = e;
    };
    RegisterComponent.prototype.showRegisterForm = function () {
        return this.state == RegisterState.Register;
    };
    RegisterComponent.prototype.showSavingMessage = function () {
        return this.state == RegisterState.Saving;
    };
    RegisterComponent.prototype.showSavedMessage = function () {
        return this.state == RegisterState.Saved;
    };
    RegisterComponent.prototype.showFailedMessage = function () {
        return this.state == RegisterState.Failed;
    };
    RegisterComponent.prototype.registerDataInvalid = function () {
        if (!this.email || this.email.length < 5)
            return true;
        if (!this.fullname || this.fullname.length < 3)
            return true;
        return false;
    };
    RegisterComponent = __decorate([
        core_1.Component({
            templateUrl: 'templates/register.html',
        }), 
        __metadata('design:paramtypes', [http_1.Http])
    ], RegisterComponent);
    return RegisterComponent;
}());
exports.RegisterComponent = RegisterComponent;
//# sourceMappingURL=register.component.js.map