import { Component, ViewChild, ElementRef } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  @ViewChild('navbarToggler') navbarToggler!: ElementRef;
  @ViewChild('navbarDropdown') navbarDropdown!: ElementRef;
  public title = 'Webhooks Server';

  constructor(private router: Router) {
    router.events.subscribe(val => {
      if (val instanceof NavigationEnd) {
        // TODO: It feels dirty to directly manipulate the dom this way.
        this.navbarToggler?.nativeElement.classList.add('collapsed');
        this.navbarToggler?.nativeElement.setAttribute('aria-expanded', false);
        this.navbarDropdown?.nativeElement.classList.remove('show');
      }
    })
  }
}
