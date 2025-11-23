import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthStore } from '../../core/auth/auth.store';
import { environment } from '../../../environments/environment';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';

@Component({
  standalone: true,
  selector: 'app-login',
  imports: [CommonModule, MatButtonModule, MatCardModule],
  styleUrls: ['./login.component.scss'],
  template: `
    <div class="login-layout">
      <div class="hero">
        <div class="hero-content">
          <div class="brand">
            <div class="brand-mark">C</div>
            <span>{{ appName }}</span>
          </div>
          <h2>Add Team, Assign Roles, Manage Finances</h2>
          <div class="hero-illustration"></div>
        </div>
      </div>

      <div class="form-area">
        <div class="form-card">
          <div class="brand center">
            <div class="brand-mark">C</div>
            <span>{{ appName }}</span>
          </div>
          <h1>Log In / Create Account</h1>

          <mat-card class="cta-card">
            <p class="subtitle">Choose one option to continue</p>
            <button mat-stroked-button color="primary" class="google-btn" (click)="onGoogleSignIn()">
              <span class="google-icon" aria-hidden="true">
                <svg viewBox="0 0 533.5 544.3">
                  <path fill="#4285f4" d="M533.5 278.4c0-17.4-1.5-34.1-4.3-50.2H272v95h146.9c-6.3 34-25.3 62.8-54 82v68h87.2c51-46.9 81.4-116.1 81.4-194.8z" />
                  <path fill="#34a853" d="M272 544.3c73.4 0 135-24.2 180-65.7l-87.2-68c-24.2 16.2-55.2 25.8-92.8 25.8-71 0-131.2-47.9-152.8-112.5h-90v70.6c45.7 90.6 140 149.8 242.8 149.8z" />
                  <path fill="#fbbc04" d="M119.2 323.9c-10.4-30-10.4-62.4 0-92.4v-70.6h-90c-36.7 71.3-36.7 155.7 0 227z" />
                  <path fill="#ea4335" d="M272 106.1c38.8-.6 75.9 13.8 104.3 40.5l78-78C409.7 24.7 342.7-.6 272 0 169.2 0 74.9 59.2 29.2 149.9l90 70.6C140.8 154 201 106.1 272 106.1z" />
                </svg>
              </span>
              <span>Continue with Google</span>
            </button>
            <p class="terms">
              By continuing, you accept our
              <a>Terms of Service</a> and <a>Privacy Policy</a>.
            </p>
          </mat-card>

          <p *ngIf="store.loading()" class="status">Signing in...</p>
          <p *ngIf="store.error()" class="status error">{{ store.error() }}</p>
        </div>
      </div>
    </div>
  `
})
export class LoginComponent implements OnInit {
  readonly store = inject(AuthStore);
  readonly appName = environment.appName;
  error?: string;

  async onGoogleSignIn() {
    const nonce = this.generateNonce();
    sessionStorage.setItem('google_nonce', nonce);
    const authUrl =
      'https://accounts.google.com/o/oauth2/v2/auth?' +
      new URLSearchParams({
        client_id: environment.googleClientId,
        redirect_uri: globalThis.location.origin + '/login',
        response_type: 'token id_token',
        scope: 'openid email profile',
        include_granted_scopes: 'true',
        prompt: 'select_account',
        nonce
      }).toString();
    globalThis.location.href = authUrl;
  }

  ngOnInit() {
    const hash = globalThis.location.hash.startsWith('#') ? globalThis.location.hash.substring(1) : '';
    if (hash) {
      const params = new URLSearchParams(hash);
      const idToken = params.get('id_token');
      const error = params.get('error');
      const errorDescription = params.get('error_description');
      if (idToken) {
        globalThis.history.replaceState({}, document.title, globalThis.location.pathname);
        this.store.login(idToken);
      } else if (error) {
        this.error = errorDescription ?? error;
      }
    }
  }

  private generateNonce() {
    const array = new Uint8Array(16);
    crypto.getRandomValues(array);
    return Array.from(array, (b) => ('00' + b.toString(16)).slice(-2)).join('');
  }
}
