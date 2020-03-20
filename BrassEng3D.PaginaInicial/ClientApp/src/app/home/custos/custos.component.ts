import { Component, OnInit } from '@angular/core';
import { HomeService } from '../home.service';


@Component({
  selector: 'app-custos',
  templateUrl: './custos.component.html',
  styleUrls: ['./custos.component.css'],
  providers: [HomeService]
})
export class CustosComponent implements OnInit {

  nome: string;
  figura: string;

  constructor(private homeService: HomeService) {
    this.homeService.getUserProfileAsync()
    .subscribe((u) => {
         this.nome = u.name;
         this.figura = u.picture;
         console.log(this.figura);
    });
   }

  ngOnInit() {
    
  }

}
