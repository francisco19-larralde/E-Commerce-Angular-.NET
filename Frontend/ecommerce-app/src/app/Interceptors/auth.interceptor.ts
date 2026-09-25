import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { TokenStorageService } from '../Services/tokenStorage.service';
import { API_URL } from '../config/api-url.token';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenStorage = inject(TokenStorageService);
  const router = inject(Router);
  const token = tokenStorage.obtenerToken();
  const apiUrl = inject(API_URL);
  const esPeticionApi = perteneceALaApi(req.url, apiUrl);

  const peticion = token && esPeticionApi
    ? req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    })
    : req;

  return next(peticion).pipe(
    catchError((error) => {
      if (esPeticionApi && error.status === 401) {
        tokenStorage.limpiar();
        void router.navigateByUrl('/login');
      }

      return throwError(() => error);
    })
  );
};

function perteneceALaApi(urlPeticion: string, apiUrl: string): boolean {
  try {
    const peticion = new URL(urlPeticion, window.location.origin);
    const api = new URL(apiUrl, window.location.origin);
    const rutaApi = api.pathname.replace(/\/$/, '');

    return (
      peticion.origin === api.origin &&
      (peticion.pathname === rutaApi || peticion.pathname.startsWith(`${rutaApi}/`))
    );
  } catch {
    return false;
  }
}
