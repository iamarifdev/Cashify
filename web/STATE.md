# State Management with NgRx SignalStore

## Why SignalStore
- Simpler mental model than reducers/selectors while still leveraging NgRx patterns.
- Built-in support for effects (`rxMethod`) and derived data (`computed`).
- Excellent fit for optimistic UI and cache-like stores per feature.

## Recommended Setup
- Install NgRx Signals packages after scaffolding:
  ```bash
  npm install @ngrx/signals @ngrx/effects
  ```
- Organize stores by feature under `src/app/state/{feature}` or within each feature folder for co-location.
- Keep DTOs and API clients typed; map API responses to store models.

## Example: Auth Store
```ts
import { signalStore, withState, withMethods, patchState } from '@ngrx/signals';
import { computed, inject } from '@angular/core';
import { AuthApi } from '../../core/api/auth.api';

type AuthState = { token?: string; profile?: { id: string; email: string; name: string; picture?: string }; loading: boolean; error?: string };

export const AuthStore = signalStore(
  { providedIn: 'root' },
  withState<AuthState>({ loading: false }),
  withMethods((store) => {
    const api = inject(AuthApi);
    const setToken = (token?: string) => patchState(store, { token });

    return {
      login: async (idToken: string) => {
        patchState(store, { loading: true, error: undefined });
        try {
          const res = await api.googleSignIn(idToken);
          patchState(store, { token: res.token, profile: { id: res.userId, email: res.email, name: res.name, picture: res.photoUrl }, loading: false });
        } catch (err: any) {
          patchState(store, { error: err.message ?? 'Login failed', loading: false, token: undefined });
        }
      },
      logout: () => patchState(store, { token: undefined, profile: undefined })
    };
  }),
  withMethods((store) => ({
    isAuthenticated: computed(() => !!store.token())
  }))
);
```

## Example: Transactions Store with Optimistic Create
```ts
import { signalStore, withState, withMethods, patchState } from '@ngrx/signals';
import { computed, inject } from '@angular/core';
import { TransactionsApi } from '../../core/api/transactions.api';
import { v4 as uuid } from 'uuid';

type Tx = { id: string; amount: number; type: string; description?: string; transactionDate: string; version: number };
type TxState = { items: Tx[]; loading: boolean; error?: string };

export const TransactionsStore = signalStore(
  { providedIn: 'root' },
  withState<TxState>({ items: [], loading: false }),
  withMethods((store) => {
    const api = inject(TransactionsApi);

    return {
      load: async (businessId: string, cashbookId: string) => {
        patchState(store, { loading: true, error: undefined });
        try {
          const items = await api.list(businessId, cashbookId);
          patchState(store, { items, loading: false });
        } catch (err: any) {
          patchState(store, { error: err.message ?? 'Failed to load', loading: false });
        }
      },
      createOptimistic: async (businessId: string, cashbookId: string, payload: Omit<Tx, 'id' | 'version'>) => {
        const tempId = uuid();
        const optimistic: Tx = { id: tempId, version: 0, ...payload };
        patchState(store, (s) => ({ items: [optimistic, ...s.items] }));
        try {
          const res = await api.create(businessId, cashbookId, payload);
          patchState(store, (s) => ({
            items: s.items.map((t) => (t.id === tempId ? { ...t, id: res.id, version: 1 } : t))
          }));
        } catch (err: any) {
          patchState(store, (s) => ({ items: s.items.filter((t) => t.id !== tempId), error: err.message ?? 'Create failed' }));
        }
      }
    };
  }),
  withMethods((store) => ({
    totalIncome: computed(() => store.items().filter((t) => t.type === 'income').reduce((sum, t) => sum + t.amount, 0)),
    totalExpense: computed(() => store.items().filter((t) => t.type === 'expense').reduce((sum, t) => sum + t.amount, 0))
  }))
);
```

## API Client Sketch
```ts
import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class TransactionsApi {
  private http = inject(HttpClient);
  private base = environment.apiBaseUrl;

  list(businessId: string, cashbookId: string) {
    return this.http.get<Tx[]>(`${this.base}/businesses/${businessId}/cashbooks/${cashbookId}/transactions`).toPromise();
  }

  create(businessId: string, cashbookId: string, payload: any) {
    return this.http.post<{ id: string }>(`${this.base}/businesses/${businessId}/cashbooks/${cashbookId}/transactions`, payload).toPromise();
  }
}
```

## Tips
- Use feature-specific SignalStores to keep logic close to UI; avoid one massive root store.
- Keep derived view models as `computed` signals; components subscribe via the `| async`-like `signal()` usage.
- Handle errors in stores and expose a `status` signal (`idle` | `loading` | `error`).
- Align with API contracts: reuse DTOs and enums from backend spec for consistency.
