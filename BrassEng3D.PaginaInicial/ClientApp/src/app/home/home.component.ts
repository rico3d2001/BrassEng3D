import { Component, AfterViewInit, OnInit } from '@angular/core';
import { HomeService } from './home.service';
import { Router } from '@angular/router';



@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  providers: [HomeService]
})
export class HomeComponent implements OnInit, AfterViewInit{
  

  LOGO = "assets/img/logo.png";
  oAuthURL: string;

  constructor(private homeService: HomeService, private router: Router){
      
  }

  autodeskSigninButton(){
    this.router.navigate(['/externalRedirect', { externalUrl: this.oAuthURL }]);
  }

  ngOnInit(): void {
    this.homeService.getPublicTokenAsync()
    .subscribe((t) => {
      console.log('OnInit: ' + t);
    },
    err => {
      console.error('OnInit: ' + err);
    });
  }

  ngAfterViewInit(){
    this.homeService.getOAuthURL().subscribe((u) => {
      this.oAuthURL = u;
      console.log(this.oAuthURL);
     });
  }


  
}
