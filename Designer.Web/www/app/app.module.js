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
var platform_browser_1 = require('@angular/platform-browser');
var forms_1 = require('@angular/forms');
var http_1 = require('@angular/http');
var router_1 = require('@angular/router');
var app_component_1 = require('./app.component');
var welcome_component_1 = require('./welcome.component');
var register_component_1 = require('./register.component');
var personlist_component_1 = require('./personlist.component');
var persondetail_component_1 = require('./persondetail.component');
var products_component_1 = require('./products.component');
var status_component_1 = require('./status.component');
var personofficestate_component_1 = require('./personofficestate.component');
var AppModule = (function () {
    function AppModule() {
    }
    AppModule = __decorate([
        core_1.NgModule({
            imports: [
                platform_browser_1.BrowserModule,
                forms_1.FormsModule,
                http_1.HttpModule,
                router_1.RouterModule.forRoot([
                    { path: 'welcome', component: welcome_component_1.WelcomeComponent },
                    { path: 'register', component: register_component_1.RegisterComponent },
                    { path: 'personlist', component: personlist_component_1.PersonListComponent },
                    { path: 'person/:id', component: persondetail_component_1.PersonDetailComponent },
                    { path: 'products', component: products_component_1.ProductsComponent },
                    { path: 'status', component: status_component_1.StatusComponent },
                    { path: 'personstatus', component: personofficestate_component_1.PersonOfficeStateComponent },
                    // { path: 'bla/:id', component: BlaComponent },
                    { path: '', redirectTo: 'welcome', pathMatch: 'full' },
                ])
            ],
            declarations: [
                app_component_1.AppComponent,
                welcome_component_1.WelcomeComponent,
                register_component_1.RegisterComponent,
                personlist_component_1.PersonListComponent,
                persondetail_component_1.PersonDetailComponent,
                products_component_1.ProductsComponent,
                status_component_1.StatusComponent,
                personofficestate_component_1.PersonOfficeStateComponent,
            ],
            bootstrap: [app_component_1.AppComponent]
        }), 
        __metadata('design:paramtypes', [])
    ], AppModule);
    return AppModule;
}());
exports.AppModule = AppModule;
//# sourceMappingURL=app.module.js.map