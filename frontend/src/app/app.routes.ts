import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { SignupComponent } from './components/signup/signup.component';
import { HomeComponent } from './components/home/home.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { authGuard } from './guards/login/auth.guard';

export const routes: Routes = [
    { path: '', component: HomeComponent},
    { path: 'login', component: LoginComponent, canActivate: [authGuard]},
    { path: 'signup', component: SignupComponent, canActivate: [authGuard]},
    { path: 'dashboard', component: DashboardComponent},
    { path: '**', redirectTo: '' } // Redirect unknown routes to Home
];