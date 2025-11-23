import { inject } from '@angular/core';
import { CanActivateFn, Router, UrlTree } from '@angular/router';
import { AuthStore } from './auth.store';

export const AuthGuard: CanActivateFn = (): boolean | UrlTree => {
  const auth = inject(AuthStore);
  const router = inject(Router);
  auth.hydrate();
  return auth.isAuthenticated() ? true : router.createUrlTree(['/login']);
};
