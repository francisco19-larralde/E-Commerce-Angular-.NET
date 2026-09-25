import { Injectable, signal } from '@angular/core';
import { AuthResponse } from '../Models/auth.model';

const TOKEN_KEY = 'ecommerce_token';
const USER_KEY = 'ecommerce_user';

@Injectable({
  providedIn: 'root'
})
export class TokenStorageService {
  private usuarioActual = signal<AuthResponse | null>(this.leerUsuarioValido());
  readonly usuario = this.usuarioActual.asReadonly();

  guardar(respuesta: AuthResponse): void {
    localStorage.setItem(TOKEN_KEY, respuesta.token);
    localStorage.setItem(USER_KEY, JSON.stringify(respuesta));
    this.usuarioActual.set(respuesta);
  }

  limpiar(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.usuarioActual.set(null);
  }

  obtenerToken(): string | null {
    return this.usuarioActual()?.token ?? null;
  }

  obtenerUsuarioGuardado(): AuthResponse | null {
    return this.usuarioActual();
  }

  private leerUsuarioValido(): AuthResponse | null {
    const guardado = localStorage.getItem(USER_KEY);
    const token = localStorage.getItem(TOKEN_KEY);
    if (!guardado || !token) return null;

    try {
      const usuario = JSON.parse(guardado) as AuthResponse;
      const expiracion = Date.parse(usuario.expiracion);

      if (!usuario.token || usuario.token !== token || !Number.isFinite(expiracion) || expiracion <= Date.now()) {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(USER_KEY);
        return null;
      }

      return usuario;
    } catch {
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(USER_KEY);
      return null;
    }
  }
}
