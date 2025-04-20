import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { HIGHLIGHT_OPTIONS } from 'ngx-highlightjs';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

@NgModule({
  imports: [ BrowserModule, AppRoutingModule ],
  providers: [
    provideHttpClient(withInterceptorsFromDi()),
    {
      provide: HIGHLIGHT_OPTIONS,
      useValue: {
        coreLibraryLoader: () => import('highlight.js/lib/core'),
        lineNumbersLoader: () => import('ngx-highlightjs/line-numbers'),
        languages: { csharp: () => import('highlight.js/lib/languages/csharp') }
      }
    }],
  bootstrap: [AppComponent]
})
export class AppModule { }
