import { Injectable, signal } from '@angular/core';
import { BusinessSummary } from '../api/businesses.api';

@Injectable({ providedIn: 'root' })
export class BusinessContextService {
  private readonly businessesSignal = signal<BusinessSummary[]>([]);
  private readonly selectedBusinessIdSignal = signal<string | undefined>(undefined);

  businesses() {
    return this.businessesSignal();
  }

  selectedBusinessId() {
    return this.selectedBusinessIdSignal();
  }

  setBusinesses(list: BusinessSummary[]) {
    this.businessesSignal.set(list);
    if (list.length && !this.selectedBusinessIdSignal()) {
      this.selectedBusinessIdSignal.set(list[0].id);
    }
  }

  setSelectedBusinessId(id?: string) {
    this.selectedBusinessIdSignal.set(id);
  }
}
