import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_URL } from '../config/api-url.token';
import { Orden } from '../Models/orden.model';

export interface CheckoutRequest {
  cuponCodigo: string | null;
  numeroTarjeta: string;
  nombreTitular: string;
  vencimiento: string;
  cvv: string;
}

@Injectable({
  providedIn: 'root'
})
export class OrdenService {
  private http = inject(HttpClient);
  private apiUrl = `${inject(API_URL)}/ordenes`;

  checkout(datos: CheckoutRequest): Observable<Orden> {
    return this.http.post<Orden>(`${this.apiUrl}/checkout`, datos);
  }

  obtenerMisCompras(): Observable<Orden[]> {
    return this.http.get<Orden[]>(`${this.apiUrl}/mis-compras`);
  }

  obtenerDetalle(id: number): Observable<Orden> {
    return this.http.get<Orden>(`${this.apiUrl}/${id}`);
  }

  validarCupon(codigo: string): Observable<{ porcentajeDescuento: number }> {
    const params = new HttpParams().set('codigo', codigo);
    return this.http.get<{ porcentajeDescuento: number }>(`${this.apiUrl}/validar-cupon`, { params });
  }
}
