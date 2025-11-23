import { inject, Injectable } from '@angular/core';
import { ApiService } from './api.service';

export type BusinessSummary = { id: string; name: string; role: string };

@Injectable({ providedIn: 'root' })
export class BusinessesApi {
  private api = inject(ApiService);

  list() {
    return this.api.get<BusinessSummary[]>('/businesses').toPromise();
  }

  create(name: string) {
    return this.api.post<{ id: string; name: string }>('/businesses', { name }).toPromise();
  }
}
