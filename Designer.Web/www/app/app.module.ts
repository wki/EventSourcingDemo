import { NgModule }      from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';

import { AppComponent }  from './app.component';
import { WelcomeComponent } from './welcome.component';
import { RegisterComponent } from './register.component';
import { ProductsComponent } from './products.component';

@NgModule({
  imports: [ 
    BrowserModule,
    FormsModule,
    HttpModule,
    RouterModule.forRoot([
      { path: 'welcome', component: WelcomeComponent },
      { path: 'register', component: RegisterComponent },
      { path: 'products', component: ProductsComponent },
      // { path: 'bla/:id', component: BlaComponent },
      { path: '', redirectTo: 'welcome', pathMatch: 'full' },
      // { path: '**', component: PageNotFoundComponent },
    ]) 
  ],
  declarations: [
    AppComponent,
    WelcomeComponent,
    RegisterComponent,
    ProductsComponent,
  ],
  bootstrap:    [ AppComponent ]
})
export class AppModule { }
