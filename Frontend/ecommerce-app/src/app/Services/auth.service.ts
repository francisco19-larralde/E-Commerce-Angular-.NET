import { Injectable, computed, effect, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { LoginRequest, RegistroRequest, AuthResponse } from '../Models/auth.model';
import { CarritoService } from './carrito.service';
import { TokenStorageService } from './tokenStorage.service';
import { API_URL } from '../config/api-url.token';


@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private carritoService = inject(CarritoService);
  private tokenStorage = inject(TokenStorageService);
  private apiUrl = `${inject(API_URL)}/auth`;

  usuario = this.tokenStorage.usuario;
  estaLogueado = computed(() => this.usuario() !== null);
  esAdmin = computed(() => this.usuario()?.roles.includes('Admin') ?? false);

  constructor() {
    effect((onCleanup) => {
      const usuario = this.usuario();

      if (!usuario) {
        this.carritoService.limpiarLocal();
        return;
      }

      this.carritoService.cargarCarrito();
      const demora = Date.parse(usuario.expiracion) - Date.now();

      if (demora <= 0) {
        this.tokenStorage.limpiar();
        return;
      }

      const temporizador = window.setTimeout(() => this.tokenStorage.limpiar(), demora);
      onCleanup(() => window.clearTimeout(temporizador));
    });
  }

  login(datos: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, datos).pipe(
      tap((respuesta) => this.guardarSesion(respuesta))
    );
  }

  registro(datos: RegistroRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/registro`, datos).pipe(
      tap((respuesta) => this.guardarSesion(respuesta))
    );
  }

  logout(): void {
    this.tokenStorage.limpiar();
  }

  private guardarSesion(respuesta: AuthResponse): void {
    this.tokenStorage.guardar(respuesta);
  }
}
