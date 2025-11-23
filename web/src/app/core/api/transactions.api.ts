import { inject, Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Tx } from '../../state/transactions.store';

@Injectable({ providedIn: 'root' })
export class TransactionsApi {
  private api = inject(ApiService);

  list(businessId: string, cashbookId: string) {
    return this.api.get<Tx[]>(`/businesses/${businessId}/cashbooks/${cashbookId}/transactions`).toPromise();
  }

  create(businessId: string, cashbookId: string, payload: Partial<Tx>) {
    return this.api.post<{ id: string }>(`/businesses/${businessId}/cashbooks/${cashbookId}/transactions`, payload).toPromise();
  }
}
