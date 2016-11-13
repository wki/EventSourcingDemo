import { NgModule }      from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';

import { AppComponent }  from './app.component';
import { WelcomeComponent } from './welcome.component';
import { RegisterComponent } from './register.component';
import { PersonListComponent } from './personlist.component';
import { ProductsComponent } from './products.component';
import { StatusComponent } from './status.component';

@NgModule({
  imports: [ 
    BrowserModule,
    FormsModule,
    HttpModule,
    RouterModule.forRoot([
      { path: 'welcome', component: WelcomeComponent },
      { path: 'register', component: RegisterComponent },
      { path: 'personlist', component: PersonListComponent },
      { path: 'products', component: ProductsComponent },
      { path: 'status', component: StatusComponent },
      // { path: 'bla/:id', component: BlaComponent },
      { path: '', redirectTo: 'welcome', pathMatch: 'full' },
      // { path: '**', component: PageNotFoundComponent },
    ]) 
  ],
  declarations: [
    AppComponent,
    WelcomeComponent,
    RegisterComponent,
    PersonListComponent,
    ProductsComponent,
    StatusComponent,
  ],
  bootstrap:    [ AppComponent ]
})
export class AppModule { }
