import { BrowserModule } from '@angular/platform-browser';
import { NgModule, InjectionToken } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HttpClient  } from '@angular/common/http';
import { HttpModule} from '@angular/http';
import { RouterModule, ActivatedRouteSnapshot } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { CounterComponent } from './counter/counter.component';
import { FetchDataComponent } from './fetch-data/fetch-data.component';
import { CustosComponent } from './home/custos/custos.component';


const externalUrlProvider = new InjectionToken('externalUrlRedirectResolver');


@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    CounterComponent,
    FetchDataComponent,
    CustosComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    HttpModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: HomeComponent, pathMatch: 'full' },
      { path: 'counter', component: CounterComponent },
      { path: 'fetch-data', component: FetchDataComponent },
      { path: 'custos', component: CustosComponent},
      { path: 'externalRedirect', resolve: { url: externalUrlProvider}, component: HomeComponent},

    ])
  ],
  providers: [{
    provide: externalUrlProvider,
    useValue: (route: ActivatedRouteSnapshot) => {
        const externalUrl = route.paramMap.get('externalUrl');
        window.open(externalUrl, '_self');
    },
  }],
  bootstrap: [AppComponent]
})
export class AppModule { }
