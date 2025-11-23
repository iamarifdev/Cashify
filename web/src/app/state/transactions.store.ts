import { computed, inject, Injectable, signal } from '@angular/core';
import { TransactionsApi } from '../core/api/transactions.api';
import { v4 as uuid } from 'uuid';

export type Tx = {
  id: string;
  amount: number;
  type: string;
  description?: string;
  transactionDate: string;
  version: number;
};

type TxState = { items: Tx[]; loading: boolean; error?: string };

@Injectable({ providedIn: 'root' })
export class TransactionsStore {
  private readonly api = inject(TransactionsApi);
  private readonly state = signal<TxState>({ items: [], loading: false });

  readonly items = computed(() => this.state().items);
  readonly loading = computed(() => this.state().loading);
  readonly error = computed(() => this.state().error);

  async load(businessId: string, cashbookId: string) {
    this.state.update((s) => ({ ...s, loading: true, error: undefined }));
    try {
      const items = (await this.api.list(businessId, cashbookId)) ?? [];
      this.state.set({ items, loading: false });
    } catch (err: any) {
      this.state.update((s) => ({ ...s, loading: false, error: err?.message ?? 'Failed to load' }));
    }
  }

  async createOptimistic(businessId: string, cashbookId: string, payload: Omit<Tx, 'id' | 'version'>) {
    const tempId = uuid();
    const optimistic: Tx = { id: tempId, version: 0, ...payload };
    this.state.update((s) => ({ ...s, items: [optimistic, ...s.items] }));
    try {
      const res = await this.api.create(businessId, cashbookId, payload);
      if (res) {
        this.state.update((s) => ({
          ...s,
          items: s.items.map((t) => (t.id === tempId ? { ...t, id: res.id, version: 1 } : t))
        }));
      }
    } catch (err: any) {
      this.state.update((s) => ({
        ...s,
        items: s.items.filter((t) => t.id !== tempId),
        error: err?.message ?? 'Create failed'
      }));
    }
  }

  readonly incomeTotal = computed(() =>
    this.items().filter((t) => t.type === 'income').reduce((sum, t) => sum + t.amount, 0)
  );
  readonly expenseTotal = computed(() =>
    this.items().filter((t) => t.type === 'expense').reduce((sum, t) => sum + t.amount, 0)
  );
}
