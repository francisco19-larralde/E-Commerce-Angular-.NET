import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Carrito } from '../Models/carrito.model';
import { API_URL } from '../config/api-url.token';

@Injectable({
  providedIn: 'root'
})
export class CarritoService {
  private http = inject(HttpClient);
  private apiUrl = `${inject(API_URL)}/carrito`;

  private carritoActual = signal<Carrito | null>(null);
  carrito = this.carritoActual.asReadonly();


  cantidadItems = computed(() =>
    this.carritoActual()?.items.reduce((total, item) => total + item.cantidad, 0) ?? 0
  );

  cargarCarrito(): void {
    this.http.get<Carrito>(this.apiUrl).subscribe({
      next: (carrito) => this.carritoActual.set(carrito),
      error: (err) => console.error('Error al cargar el carrito', err)
    });
  }

  agregarItem(productoId: number, cantidad: number = 1, varianteId: number | null = null): Observable<Carrito> {
    return this.http
      .post<Carrito>(`${this.apiUrl}/items`, { productoId, varianteId, cantidad })
      .pipe(tap((carrito) => this.carritoActual.set(carrito)));
  }

  actualizarCantidad(itemId: number, cantidad: number): Observable<Carrito> {
    return this.http
      .put<Carrito>(`${this.apiUrl}/items/${itemId}`, { cantidad })
      .pipe(tap((carrito) => this.carritoActual.set(carrito)));
  }

  eliminarItem(itemId: number): Observable<Carrito> {
    return this.http
      .delete<Carrito>(`${this.apiUrl}/items/${itemId}`)
      .pipe(tap((carrito) => this.carritoActual.set(carrito)));
  }

  vaciarCarrito(): void {
    this.http.delete(this.apiUrl).subscribe({
      next: () => this.carritoActual.set(null)
    });
  }

  limpiarLocal(): void {
    this.carritoActual.set(null);
  }
}
