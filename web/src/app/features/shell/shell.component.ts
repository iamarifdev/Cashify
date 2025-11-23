import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatSelectModule } from '@angular/material/select';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterModule } from '@angular/router';

import { environment } from '../../../environments/environment';
import { BusinessesApi, BusinessSummary } from '../../core/api/businesses.api';
import { AuthStore } from '../../core/auth/auth.store';
import { BusinessContextService } from '../../core/business/business-context.service';

@Component({
  standalone: true,
  selector: 'app-shell',
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatSelectModule,
    MatDividerModule,
  ],
  styleUrls: ['./shell.component.scss'],
  template: `
    <div class="shell">
      <aside class="side-nav">
        <div class="brand">
          <div class="brand-mark">C</div>
          <span>{{ appName }}</span>
        </div>
        <nav class="nav-group">
          <p class="nav-title">Book Keeping</p>
          <a routerLink="/" routerLinkActive="active">Cashbooks</a>
        </nav>
        <nav class="nav-group">
          <p class="nav-title">Settings</p>
          <a routerLink="/activity" routerLinkActive="active">Activity</a>
          <a routerLink="/reports" routerLinkActive="active">Reports</a>
        </nav>
        <nav class="nav-group">
          <p class="nav-title">Others</p>
          <a routerLink="/transactions" routerLinkActive="active">Transactions</a>
        </nav>
      </aside>

      <div class="main">
        <mat-toolbar color="primary" class="topbar">
          <div class="business-select">
            <mat-select
              placeholder="Select business"
              [(value)]="selectedBusinessId"
              (valueChange)="onBusinessChange($event)"
            >
              <mat-option *ngFor="let biz of businesses" [value]="biz.id">{{
                biz.name
              }}</mat-option>
            </mat-select>
          </div>
          <span class="spacer"></span>
          <button mat-stroked-button color="primary" class="download-btn" routerLink="/reports">
            Download App
          </button>
          <button mat-icon-button [matMenuTriggerFor]="userMenu">
            <div class="avatar">
              {{ profileInitials }}
            </div>
          </button>
          <mat-menu #userMenu="matMenu">
            <div class="menu-header">
              <p class="name">{{ auth.profile()?.name }}</p>
              <p class="email">{{ auth.profile()?.email }}</p>
            </div>
            <mat-divider></mat-divider>
            <button mat-menu-item (click)="logout()">Logout</button>
          </mat-menu>
        </mat-toolbar>

        <main class="main-content">
          <router-outlet />
        </main>
      </div>
    </div>
  `,
})
export class ShellComponent implements OnInit {
  readonly appName = environment.appName;
  readonly auth = inject(AuthStore);
  private readonly businessesApi = inject(BusinessesApi);
  private readonly businessContext = inject(BusinessContextService);

  businesses: BusinessSummary[] = [];
  selectedBusinessId?: string;

  get profileInitials() {
    const name = this.auth.profile()?.name ?? 'U';
    const initials = name
      .split(' ')
      .filter((x) => x)
      .slice(0, 2)
      .map((x) => x[0].toUpperCase())
      .join('');
    return initials || 'U';
  }

  ngOnInit() {
    this.initializeBusinesses();
  }

  private async initializeBusinesses() {
    try {
      this.businesses = (await this.businessesApi.list()) ?? [];
      this.businessContext.setBusinesses(this.businesses);
      this.selectedBusinessId = this.businessContext.selectedBusinessId();
    } catch {
      this.businesses = [];
      this.selectedBusinessId = undefined;
    }
  }

  onBusinessChange(value: string) {
    this.selectedBusinessId = value;
    this.businessContext.setSelectedBusinessId(value);
  }

  logout() {
    this.auth.logout();
  }
}
