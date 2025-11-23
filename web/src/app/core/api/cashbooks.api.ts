import { inject, Injectable } from '@angular/core';
import { ApiService } from './api.service';

export type CashbookSummary = { id: string; name: string; currency: string };

@Injectable({ providedIn: 'root' })
export class CashbooksApi {
  private api = inject(ApiService);

  list(businessId: string) {
    return this.api.get<CashbookSummary[]>(`/businesses/${businessId}/cashbooks`).toPromise();
  }
}
