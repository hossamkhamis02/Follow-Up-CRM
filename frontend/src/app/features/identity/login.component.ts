import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FormControl, FormGroup, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { ApiErrorResponse } from '../../core/models/shared';

type LoginForm = FormGroup<{
  email: FormControl<string>;
  password: FormControl<string>;
}>;

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatProgressSpinnerModule,
    ReactiveFormsModule,
    RouterModule,
  ],
  template: `
    <main class="login-page">
      <section class="login-panel" aria-labelledby="login-title">
        <div class="login-intro">
          <div class="login-mark" aria-hidden="true">
            <mat-icon>rocket_launch</mat-icon>
          </div>
          <p class="login-kicker">FollowUp CRM</p>
          <h1 id="login-title">Sign in to your workspace</h1>
          <p class="login-copy">Manage customers and follow-ups from one focused CRM dashboard.</p>
        </div>

        <mat-card class="login-card" appearance="outlined">
        <mat-card-header>
          <mat-card-title>Welcome back</mat-card-title>
          <mat-card-subtitle>Use your email or username to continue.</mat-card-subtitle>
        </mat-card-header>
        <mat-card-content>
          <form [formGroup]="loginForm" class="login-form" (ngSubmit)="login()">
            <mat-form-field appearance="outline">
              <mat-label>Email or username</mat-label>
              <input matInput formControlName="email" autocomplete="username" type="text">
              @if (loginForm.controls.email.hasError('required') && loginForm.controls.email.touched) {
                <mat-error>Email or username is required.</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>Password</mat-label>
              <input matInput formControlName="password" autocomplete="current-password" type="password">
              @if (loginForm.controls.password.hasError('required') && loginForm.controls.password.touched) {
                <mat-error>Password is required.</mat-error>
              }
            </mat-form-field>

            @if (errorMessage) {
              <p class="login-error">{{ errorMessage }}</p>
            }

            <button mat-raised-button color="primary" class="login-submit" [disabled]="loginForm.invalid || isSubmitting">
              @if (isSubmitting) {
                <mat-spinner diameter="18"></mat-spinner>
                <span>Signing in</span>
              } @else {
                <span>Sign in</span>
              }
            </button>
          </form>
        </mat-card-content>
        <mat-card-actions align="end">
          <a mat-button routerLink="/identity/register">Create account</a>
        </mat-card-actions>
      </mat-card>
      </section>
    </main>
  `,
  styles: [`
    .login-page {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 100vh;
      padding: 32px 16px;
      background:
        linear-gradient(135deg, rgba(25, 118, 210, 0.08), rgba(0, 150, 136, 0.07)),
        #f7f9fc;
    }

    .login-panel {
      display: grid;
      grid-template-columns: minmax(0, 1fr) minmax(320px, 420px);
      align-items: center;
      gap: 48px;
      width: min(100%, 960px);
    }

    .login-intro {
      color: #172033;
    }

    .login-mark {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 48px;
      height: 48px;
      margin-bottom: 20px;
      border-radius: 8px;
      color: #ffffff;
      background: #1976d2;
    }

    .login-mark mat-icon {
      width: 28px;
      height: 28px;
      font-size: 28px;
    }

    .login-kicker {
      margin: 0 0 8px;
      color: #1976d2;
      font-size: 13px;
      font-weight: 700;
      text-transform: uppercase;
    }

    h1 {
      max-width: 520px;
      margin: 0;
      font-size: 40px;
      line-height: 1.1;
      font-weight: 700;
    }

    .login-copy {
      max-width: 460px;
      margin: 16px 0 0;
      color: #526070;
      font-size: 16px;
      line-height: 1.6;
    }

    .login-card {
      width: 100%;
      border-radius: 8px;
      box-shadow: 0 16px 40px rgba(23, 32, 51, 0.12);
    }

    .login-form {
      display: flex;
      flex-direction: column;
      gap: 12px;
      margin-top: 16px;
    }

    .login-submit {
      width: 100%;
      margin-top: 8px;
      min-height: 44px;
    }

    .login-submit mat-spinner {
      display: inline-block;
      margin-right: 8px;
    }

    .login-error {
      border-radius: 6px;
      background: rgba(179, 38, 30, 0.08);
      color: #b3261e;
      font-size: 13px;
      margin: 0;
      padding: 10px 12px;
    }

    @media (max-width: 768px) {
      .login-panel {
        grid-template-columns: 1fr;
        gap: 24px;
      }

      .login-intro {
        text-align: center;
      }

      .login-copy {
        margin-right: auto;
        margin-left: auto;
      }

      h1 {
        font-size: 30px;
      }
    }
  `],
})
export class LoginComponent {
  loginForm: LoginForm;
  isSubmitting = false;
  errorMessage = '';

  constructor(
    private fb: NonNullableFormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.buildForm();
  }

  private buildForm(): LoginForm {
    return this.fb.group({
      email: ['', [Validators.required]],
      password: ['', [Validators.required]],
    });
  }

  login(): void {
    if (this.loginForm.invalid || this.isSubmitting) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    this.authService.login(this.loginForm.getRawValue()).pipe(
      finalize(() => {
        this.isSubmitting = false;
      })
    ).subscribe({
      next: () => {
        this.router.navigateByUrl('/dashboard');
      },
      error: (error: ApiErrorResponse) => {
        this.errorMessage = this.getLoginErrorMessage(error);
      },
    });
  }

  private getLoginErrorMessage(error: ApiErrorResponse): string {
    if (error.status === 401 || error.status === 400) {
      return 'We could not sign you in with those credentials. Please check them and try again.';
    }

    return error.message || 'Unable to sign in right now. Please try again.';
  }
}
