import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  const token = localStorage.getItem('token'); // Check if user is logged in

  if (token) {
    router.navigate(['/']); // Redirect logged-in users to home
    return false; // Prevent access to login
  }

  return true; // Allow access if not logged in
};