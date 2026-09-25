import { provideZonelessChangeDetection } from '@angular/core';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { TokenStorageService } from '../Services/tokenStorage.service';
import { API_URL } from '../config/api-url.token';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpTesting: HttpTestingController;
  let tokenStorage: TokenStorageService;
  let router: Router;
  let apiUrl: string;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        provideRouter([]),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting()
      ]
    });

    http = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
    tokenStorage = TestBed.inject(TokenStorageService);
    router = TestBed.inject(Router);
    apiUrl = TestBed.inject(API_URL);
    tokenStorage.guardar({
      token: 'token-prueba',
      expiracion: new Date(Date.now() + 60_000).toISOString(),
      nombre: 'Usuario Prueba',
      email: 'usuario@example.test',
      roles: ['Cliente']
    });
  });

  afterEach(() => {
    httpTesting.verify();
    localStorage.clear();
  });

  it('adjunta el token solamente a peticiones de la API', () => {
    http.get(`${apiUrl}/productos`).subscribe();
    http.get('https://cdn.example.test/imagen.jpg').subscribe();
    http.get(`${apiUrl}.sitio-malicioso.test`).subscribe();

    const peticionApi = httpTesting.expectOne(`${apiUrl}/productos`);
    const peticionExterna = httpTesting.expectOne('https://cdn.example.test/imagen.jpg');
    const peticionParecida = httpTesting.expectOne(`${apiUrl}.sitio-malicioso.test`);

    expect(peticionApi.request.headers.get('Authorization')).toBe('Bearer token-prueba');
    expect(peticionExterna.request.headers.has('Authorization')).toBeFalse();
    expect(peticionParecida.request.headers.has('Authorization')).toBeFalse();

    peticionApi.flush({});
    peticionExterna.flush({});
    peticionParecida.flush({});
  });

  it('limpia la sesión y redirige al login ante un 401 de la API', () => {
    const navegar = spyOn(router, 'navigateByUrl').and.resolveTo(true);

    http.get(`${apiUrl}/carrito`).subscribe({ error: () => undefined });
    httpTesting
      .expectOne(`${apiUrl}/carrito`)
      .flush({}, { status: 401, statusText: 'Unauthorized' });

    expect(tokenStorage.obtenerToken()).toBeNull();
    expect(navegar).toHaveBeenCalledWith('/login');
  });
});
