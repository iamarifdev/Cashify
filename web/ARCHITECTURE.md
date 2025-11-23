# Cashify Web Architecture (Angular 21 + Material, SCSS)

## Project Layout
- `src/app/core`: global services (auth, api clients), interceptors (JWT, error), guards, layout shell, state bootstrap.
- `src/app/shared`: dumb/presentational components, pipes, directives, form controls, typography/spacing utilities.
- `src/app/features`: vertical slices (auth, businesses, cashbooks, transactions, lookups, reports, activity).
- `src/app/state`: NgRx stores (feature-level, plus root meta-reducers), selectors, effects, models.
- `src/styles`: Global SCSS and Material theming tokens.
- `environments`: `environment.ts` & `.prod.ts` with `apiBaseUrl`, `googleClientId`.

## Core Practices
- **Routing**: Feature modules with lazy routes; guard protected areas with an `AuthGuard` that checks JWT presence/expiry.
- **State**: Use NgRx SignalStore (NgRx Signals) for business/cashbook/transactions/lookups/auth; derive view models via computed signals; use optimistic updates for inline lookup creation.
- **Data Access**: Centralized `ApiService` wrapping `HttpClient` with base URL + typed methods; feature services compose it.
- **Interceptors**: `AuthInterceptor` injects JWT; `HttpErrorInterceptor` handles 401/403 (logout), 429/500 (toasts); `LoaderInterceptor` optional for UX.
- **Theming**: Angular Material theming with custom Indigo palette; global SCSS for layout/spacing; keep palette tokens in `styles.scss`.
- **Forms**: Reactive Forms with strongly typed `FormGroup`; shared validators; Material form field styles with consistent spacing via SCSS utilities.
- **Accessibility**: Use Material components for built-in a11y; ensure focus states, aria labels, and responsive touch targets (min 44px).

## Authentication (Google OAuth)
- Use Google One Tap or button; on success, send `idToken` to `/auth/google`, store returned JWT (memory + refresh on tab reload via storage).
- Persist in `localStorage` or `IndexedDB` with encryption if desired; auto-logout on token expiry; keep Google profile in NgRx `auth` slice.

## Feature Notes
- **Businesses/Cashbooks**: list/create dialogs; member management modals; enforce membership via guards + API 403 handling.
- **Transactions**: table/card view with filters; add/edit dialogs; inline create category/contact/payment method with optimistic add; show version history drawer from `/changes`.
- **Reports**: charts (e.g., NGX-Charts/Material cards) for summary/balance; export button hits `/reports/export` (CSV).
- **Activity**: timeline feed from `/activity` with actor + action + timestamp; paginate with `limit/offset`.

## Responsive & Mobile-First
- Use CSS grid/flex with SCSS utilities; avoid fixed widths.
- Navigation: top app bar + responsive drawer; collapse filters into accordions on mobile.
- Typography scale via SCSS variables and Material typography config; keep touch targets ≥44px.

## Example Snippets
**Auth interceptor**
```ts
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AuthService).token();
  if (!token) return next(req);
  return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
```
**Feature selector (transactions)**
```ts
export const selectTransactionsVm = createSelector(
  selectTransactionsState,
  selectLookupsState,
  (tx, lookups) => tx.items.map(t => ({
    ...t,
    category: lookups.categories[t.categoryId ?? ''],
    contact: lookups.contacts[t.contactId ?? ''],
  }))
);
```

## Testing/Tooling
- Unit: Jest or Karma defaults; prefer Jest for speed.
- E2E: Cypress/Playwright for auth + core flows.
- Lint/format: `ng lint`, `eslint`, `prettier`; enforce via Husky if desired.
- CI: `npm run lint && npm run test && npm run build` with envs for API URL/Google Client ID.
