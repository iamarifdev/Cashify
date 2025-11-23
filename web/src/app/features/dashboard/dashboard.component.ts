import { Component, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BusinessesApi, BusinessSummary } from '../../core/api/businesses.api';
import { CashbooksApi, CashbookSummary } from '../../core/api/cashbooks.api';
import { FormsModule } from '@angular/forms';
import { BusinessContextService } from '../../core/business/business-context.service';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';

@Component({
  standalone: true,
  selector: 'app-dashboard',
  imports: [CommonModule, FormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatIconModule],
  styleUrls: ['./dashboard.component.scss'],
  template: `
    <div class="dashboard">
      <mat-card class="role-banner" *ngIf="selectedBusiness">
        <div class="role-chip"></div>
        <div class="role-text">
          <span class="label">Your Role:</span>
          <span class="value">Owner</span>
          <a class="link" href="#">View</a>
        </div>
      </mat-card>

      <div class="filters">
        <mat-form-field appearance="outline" class="filter-field">
          <mat-label>Search by book name</mat-label>
          <input matInput [(ngModel)]="searchTerm" (ngModelChange)="filter()" placeholder="Search..." />
        </mat-form-field>
        <mat-form-field appearance="outline" class="filter-field narrow">
          <mat-label>Sort By</mat-label>
          <mat-select [(value)]="sortBy" (valueChange)="filter()">
            <mat-option value="updated">Last Updated</mat-option>
            <mat-option value="name">Name</mat-option>
          </mat-select>
        </mat-form-field>
      </div>

      <div *ngIf="error" class="alert error">{{ error }}</div>
      <div *ngIf="loading" class="alert muted">Loading cashbooks...</div>

      <div class="cashbook-list">
        <mat-card *ngFor="let cb of filteredCashbooks" class="cashbook-card">
          <div class="avatar">
            <mat-icon>folder</mat-icon>
          </div>
          <div class="details">
            <div class="name">{{ cb.name }}</div>
            <div class="meta">Updated recently • Currency: {{ cb.currency }}</div>
          </div>
          <div class="balance">
            <div class="amount">0</div>
            <div class="caption">Balance</div>
          </div>
        </mat-card>
      </div>

      <div *ngIf="!loading && !filteredCashbooks.length" class="empty">
        <p class="title">No cashbooks yet</p>
        <p class="subtitle">Create your first cashbook to start tracking transactions.</p>
      </div>
    </div>
  `
})
export class DashboardComponent {
  private readonly businessesApi = inject(BusinessesApi);
  private readonly cashbooksApi = inject(CashbooksApi);
  private readonly businessContext = inject(BusinessContextService);

  businesses: BusinessSummary[] = [];
  selectedBusiness?: BusinessSummary;
  cashbooks: CashbookSummary[] = [];
  filteredCashbooks: CashbookSummary[] = [];
  searchTerm = '';
  sortBy: 'updated' | 'name' = 'updated';
  loading = false;
  error?: string;

  async ngOnInit() {
    let businesses = this.businessContext.businesses();
    if (!businesses.length) {
      businesses = (await this.businessesApi.list()) ?? [];
      this.businessContext.setBusinesses(businesses);
    }
    this.businesses = businesses;
    this.selectedBusiness = this.businesses.find((x) => x.id === this.businessContext.selectedBusinessId()) ?? this.businesses[0];

    effect(() => {
      const bizId = this.businessContext.selectedBusinessId();
      const match = this.businesses.find((x) => x.id === bizId);
      this.selectedBusiness = match;
      if (bizId) {
        this.loadCashbooks(bizId);
      } else {
        this.cashbooks = [];
        this.filteredCashbooks = [];
      }
    });
  }

  async loadCashbooks(businessId: string) {
    this.loading = true;
    this.error = undefined;
    try {
      this.cashbooks = (await this.cashbooksApi.list(businessId)) ?? [];
      this.filter();
    } catch (err: any) {
      this.error = err?.message ?? 'Failed to load cashbooks';
      this.cashbooks = [];
      this.filteredCashbooks = [];
    } finally {
      this.loading = false;
    }
  }

  filter() {
    const term = this.searchTerm.toLowerCase();
    this.filteredCashbooks = this.cashbooks
      .filter((cb) => cb.name.toLowerCase().includes(term))
      .sort((a, b) => {
        if (this.sortBy === 'name') return a.name.localeCompare(b.name);
        return a.name.localeCompare(b.name);
      });
  }
}
