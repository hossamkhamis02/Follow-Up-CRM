import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../auth/auth.service';

const anonymousEndpoints = ['/auth/login', '/auth/register'];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  if (token && !anonymousEndpoints.some(endpoint => req.url.includes(endpoint))) {
    return next(req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    }));
  }

  return next(req);
};
