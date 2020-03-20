import { Injectable, Inject, AfterViewInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AccessToken } from './access-token';
import { tap, map, catchError } from 'rxjs/operators';
import { Observable } from 'rxjs/Observable';
import { DadosUser } from './dados-user';

@Injectable()
export class HomeService  {
  



  baseUrl = 'http://localhost:3000/';

  oAuthURL: string;

  constructor(private http: HttpClient) {
   }

  getPublicTokenAsync(){
    return this.http.get<AccessToken>(this.baseUrl + 'api/forge/oauth/token')
   }

   getOAuthURL(): Observable<string>{
    return this.http.get(this.baseUrl + 'api/forge/oauth/url', {responseType: 'text'});
   }

   getUserProfileAsync(){
    return this.http.get<DadosUser>(this.baseUrl + 'api/forge/user/profile')
   }


}
