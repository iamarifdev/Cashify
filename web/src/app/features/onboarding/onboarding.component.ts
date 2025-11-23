import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { environment } from '../../../environments/environment';
import { AuthStore } from '../../core/auth/auth.store';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';

@Component({
  standalone: true,
  selector: 'app-onboarding',
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule, MatInputModule, MatCheckboxModule, MatButtonModule],
  styleUrls: ['./onboarding.component.scss'],
  template: `
    <div class="onboarding">
      <header class="brand-bar">
        <div class="brand">
          <div class="brand-mark">C</div>
          <span>{{ appName }}</span>
        </div>
        <div class="avatar">CB</div>
      </header>

      <mat-card class="onboarding-card">
        <div class="card-content">
          <div class="intro">
            <p class="eyebrow">Welcome to {{ appName }}</p>
            <h1>Add your details</h1>
          </div>

          <form class="form" [formGroup]="form" (ngSubmit)="submit()">
            <mat-form-field appearance="outline" class="field">
              <mat-label>Your Full Name</mat-label>
              <input matInput placeholder="Your Full Name" formControlName="fullName" />
            </mat-form-field>

            <mat-form-field appearance="outline" class="field">
              <mat-label>Business Name</mat-label>
              <input matInput placeholder="Business Name" formControlName="businessName" [disabled]="skipBusiness" />
            </mat-form-field>

            <mat-checkbox color="primary" [checked]="skipBusiness" (change)="toggleSkip()">I don't own a business. Skip this.</mat-checkbox>

            <button mat-flat-button color="primary" class="submit" type="submit" [disabled]="form.invalid || submitting">
              {{ submitting ? 'Saving...' : 'Get Started' }}
            </button>
            <p class="hint">
              If left empty, we'll start with "{{ form.value.fullName || 'Your' }}'s Business".
            </p>
            <p *ngIf="error" class="error">{{ error }}</p>
          </form>
        </div>
      </mat-card>
    </div>
  `
})
export class OnboardingComponent {
  readonly appName = environment.appName;
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthStore);

  skipBusiness = false;
  submitting = false;
  error?: string;

  form = this.fb.group({
    fullName: ['', Validators.required],
    businessName: ['']
  });

  toggleSkip() {
    this.skipBusiness = !this.skipBusiness;
    if (this.skipBusiness) {
      this.form.get('businessName')?.disable();
    } else {
      this.form.get('businessName')?.enable();
    }
  }

  async submit() {
    this.error = undefined;
    if (this.form.invalid) return;
    const { fullName, businessName } = this.form.value;
    this.submitting = true;
    try {
      const nameToSend = this.skipBusiness ? undefined : businessName?.trim();
      await this.auth.completeOnboarding(fullName!, nameToSend);
    } catch (err: any) {
      this.error = err?.message ?? 'Failed to save';
    } finally {
      this.submitting = false;
    }
  }

  ngOnInit() {
    const profile = this.auth.profile();
    if (profile?.name) {
      this.form.patchValue({ fullName: profile.name });
    }
  }
}
