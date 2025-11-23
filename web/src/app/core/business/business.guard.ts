import { inject } from '@angular/core';
import { CanActivateFn, Router, UrlTree } from '@angular/router';
import { BusinessesApi } from '../api/businesses.api';
import { BusinessContextService } from './business-context.service';
import { AuthStore } from '../auth/auth.store';

const loadBusinesses = async (businessContext: BusinessContextService, businessesApi: BusinessesApi) => {
  let list = businessContext.businesses();
  if (!list.length) {
    try {
      list = (await businessesApi.list()) ?? [];
      businessContext.setBusinesses(list);
    } catch {
      list = [];
    }
  }
  return list;
};

export const HasBusinessGuard: CanActivateFn = async (): Promise<boolean | UrlTree> => {
  const auth = inject(AuthStore);
  const router = inject(Router);
  const businessContext = inject(BusinessContextService);
  const businessesApi = inject(BusinessesApi);

  auth.hydrate();
  if (!auth.isAuthenticated()) {
    return router.createUrlTree(['/login']);
  }

  const businesses = await loadBusinesses(businessContext, businessesApi);
  if (businesses.length) {
    return true;
  }
  return router.createUrlTree(['/onboarding']);
};

export const OnboardingGuard: CanActivateFn = async (): Promise<boolean | UrlTree> => {
  const auth = inject(AuthStore);
  const router = inject(Router);
  const businessContext = inject(BusinessContextService);
  const businessesApi = inject(BusinessesApi);

  auth.hydrate();
  if (!auth.isAuthenticated()) {
    return router.createUrlTree(['/login']);
  }

  const businesses = await loadBusinesses(businessContext, businessesApi);
  if (businesses.length) {
    return router.createUrlTree(['/']);
  }
  return true;
};
