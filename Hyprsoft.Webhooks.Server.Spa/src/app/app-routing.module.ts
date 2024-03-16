import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { HealthComponent } from './health/health.component';
import { SubscriptionsComponent } from './subscriptions/subscriptions.component';
import { DocsComponent } from './docs/docs.component';

const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'health', component: HealthComponent },
  { path: 'subscriptions', component: SubscriptionsComponent },
  { path: 'docs', component: DocsComponent },
  { path: '**', redirectTo: '/' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
