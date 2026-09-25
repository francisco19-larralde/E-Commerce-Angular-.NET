import { provideZonelessChangeDetection } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { AuthResponse } from '../Models/auth.model';
import { TokenStorageService } from './tokenStorage.service';

describe('TokenStorageService', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection()]
    });
  });

  afterEach(() => localStorage.clear());

  it('restaura una sesión vigente', () => {
    const respuesta = crearRespuesta(new Date(Date.now() + 60_000));
    localStorage.setItem('ecommerce_token', respuesta.token);
    localStorage.setItem('ecommerce_user', JSON.stringify(respuesta));

    const service = TestBed.inject(TokenStorageService);

    expect(service.obtenerToken()).toBe(respuesta.token);
    expect(service.obtenerUsuarioGuardado()?.email).toBe(respuesta.email);
  });

  it('descarta una sesión vencida', () => {
    const respuesta = crearRespuesta(new Date(Date.now() - 60_000));
    localStorage.setItem('ecommerce_token', respuesta.token);
    localStorage.setItem('ecommerce_user', JSON.stringify(respuesta));

    const service = TestBed.inject(TokenStorageService);

    expect(service.obtenerToken()).toBeNull();
    expect(localStorage.getItem('ecommerce_token')).toBeNull();
    expect(localStorage.getItem('ecommerce_user')).toBeNull();
  });

  it('descarta datos de sesión corruptos', () => {
    localStorage.setItem('ecommerce_token', 'token');
    localStorage.setItem('ecommerce_user', '{json-invalido');

    const service = TestBed.inject(TokenStorageService);

    expect(service.obtenerUsuarioGuardado()).toBeNull();
    expect(localStorage.getItem('ecommerce_token')).toBeNull();
  });
});

function crearRespuesta(expiracion: Date): AuthResponse {
  return {
    token: 'token-prueba',
    expiracion: expiracion.toISOString(),
    nombre: 'Usuario Prueba',
    email: 'usuario@example.test',
    roles: ['Cliente']
  };
}
