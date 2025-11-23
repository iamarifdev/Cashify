# Cashify Web Setup (Angular 21 + Material + NgRx SignalStore)

## 1) Scaffold the workspace
```bash
cd web
npm create @angular@latest cashify-web -- --routing --style=scss --standalone
cd cashify-web
```

## 2) Install dependencies
```bash
npm install @angular/material @angular/cdk @angular/animations
npm install @ngrx/signals @ngrx/signals-store @ngrx/effects
```

## 4) Drop-in the provided templates
Copy the contents of `web/templates/` into `cashify-web/src/` (merge with existing files):
- `src/environments/environment.ts` / `.prod.ts`
- `src/app/core` (auth store/guard/interceptors, api service)
- `src/app/features` (auth/login component, shell dashboard stubs)
- `src/app/shared` (ui atoms)
- `src/app/app.routes.ts` and `app.config.ts`

## 5) Register providers
- In `src/app/app.config.ts`, add interceptors and animations:
```ts
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authInterceptor } from './core/auth/auth.interceptor';
import { errorInterceptor } from './core/auth/error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
    provideAnimationsAsync(),
    // existing providers...
  ]
};
```

## 5) Run
```bash
npm start
```

## Notes
- Auth flow expects `/auth/google` API. Set `apiBaseUrl` and `googleClientId` in `environment.ts`.
- State uses NgRx SignalStore; see `core/auth/auth.store.ts` and `state/transactions.store.ts` for patterns.
