import { computed, inject, Injectable, signal } from '@angular/core';
import { ApiService } from '../api/api.service';
import { BusinessesApi } from '../api/businesses.api';
import { Router } from '@angular/router';

type Profile = { id: string; email: string; name: string; picture?: string };
type GoogleLoginResponse = { token: string; userId: string; email: string; name: string; photoUrl?: string; hasBusinesses: boolean };
type AuthState = { token?: string; googleIdToken?: string; profile?: Profile; loading: boolean; error?: string };

@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly api = inject(ApiService);
  private readonly businessesApi = inject(BusinessesApi);
  private readonly router = inject(Router);
  private readonly state = signal<AuthState>({ loading: false });

  readonly token = computed(() => this.state().token);
  readonly googleIdToken = computed(() => this.state().googleIdToken);
  readonly headerToken = computed(() => this.state().googleIdToken ?? this.state().token);
  readonly profile = computed(() => this.state().profile);
  readonly loading = computed(() => this.state().loading);
  readonly error = computed(() => this.state().error);
  readonly isAuthenticated = computed(() => !!(this.state().token || this.state().googleIdToken));

  hydrate() {
    const token = localStorage.getItem('auth_token') ?? undefined;
    const googleIdToken = localStorage.getItem('auth_google_id_token') ?? undefined;
    const profileJson = localStorage.getItem('auth_profile');
    const profile = profileJson ? (JSON.parse(profileJson) as Profile) : undefined;
    if (token || googleIdToken) this.state.update((s) => ({ ...s, token, googleIdToken, profile }));
  }

  async login(idToken: string) {
    this.state.update((s) => ({ ...s, loading: true, error: undefined }));
    try {
      const res = await this.api.post<GoogleLoginResponse>('/auth/google', { idToken }).toPromise();
      if (res) {
        this.state.set({
          token: res.token,
          googleIdToken: idToken,
          profile: { id: res.userId, email: res.email, name: res.name, picture: res.photoUrl },
          loading: false
        });
        localStorage.setItem('auth_token', res.token);
        localStorage.setItem('auth_google_id_token', idToken);
        localStorage.setItem('auth_profile', JSON.stringify({ id: res.userId, email: res.email, name: res.name, picture: res.photoUrl }));
        await this.router.navigate([res.hasBusinesses ? '/' : '/onboarding']);
      } else {
        this.state.update((s) => ({ ...s, loading: false, error: 'Login failed' }));
      }
    } catch (err: any) {
      this.state.update((s) => ({ ...s, loading: false, error: err?.message ?? 'Login failed' }));
    }
  }

  logout() {
    localStorage.removeItem('auth_token');
    localStorage.removeItem('auth_google_id_token');
    localStorage.removeItem('auth_profile');
    this.state.set({ loading: false });
  }

  async completeOnboarding(fullName: string, businessName?: string) {
    this.state.update((s) => ({ ...s, loading: true, error: undefined }));
    try {
      await this.api.post<{ businessId: string; businessName: string }>('/auth/onboarding', { fullName, businessName }).toPromise();
      this.state.update((s) => ({
        ...s,
        loading: false,
        profile: this.state().profile ? { ...this.state().profile!, name: fullName } : undefined
      }));
      localStorage.setItem('auth_profile', JSON.stringify(this.state().profile));
      await this.router.navigate(['/']);
    } catch (err: any) {
      this.state.update((s) => ({ ...s, loading: false, error: err?.message ?? 'Failed to complete onboarding' }));
      throw err;
    }
  }
}
