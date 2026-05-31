import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthStorageService } from '../services/auth-storage.service';

const anonymousEndpoints = ['/auth/login', '/auth/register'];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authStorage = inject(AuthStorageService);
  const token = authStorage.getAccessToken();

  if (token && !anonymousEndpoints.some(endpoint => req.url.includes(endpoint))) {
    return next(req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    }));
  }

  return next(req);
};
