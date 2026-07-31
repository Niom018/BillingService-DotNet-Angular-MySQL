import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest } from '../models/auth.model';

const STORAGE_KEY = 'billing_auth';

interface StoredAuth {
  token: string;
  email: string;
  fullName: string;
  roles: string[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private authState = signal<StoredAuth | null>(this.loadFromStorage());

  isAuthenticated = computed(() => !!this.authState());
  currentUser = computed(() => this.authState());
  roles = computed(() => this.authState()?.roles ?? []);

  constructor(private http: HttpClient) {}

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/Auth/login`, request)
      .pipe(tap((res) => this.persist(res)));
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/Auth/register`, request)
      .pipe(tap((res) => this.persist(res)));
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.authState.set(null);
  }

  getToken(): string | null {
    return this.authState()?.token ?? null;
  }

  hasRole(...allowed: string[]): boolean {
    const current = this.roles();
    return allowed.some((r) => current.includes(r));
  }

  private persist(res: AuthResponse): void {
    const stored: StoredAuth = {
      token: res.token,
      email: res.email,
      fullName: res.fullName,
      roles: res.roles
    };
    localStorage.setItem(STORAGE_KEY, JSON.stringify(stored));
    this.authState.set(stored);
  }

  private loadFromStorage(): StoredAuth | null {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? JSON.parse(raw) : null;
  }
}
