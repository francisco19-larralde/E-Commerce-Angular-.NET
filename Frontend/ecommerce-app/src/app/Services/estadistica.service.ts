import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_URL } from '../config/api-url.token';
import { ResumenVentas, VentaPorDia, ProductoMasVendido } from '../Models/estadisticas.model';

@Injectable({
  providedIn: 'root'
})
export class EstadisticaService {
  private http = inject(HttpClient);
  private apiUrl = `${inject(API_URL)}/estadisticas`;

  obtenerResumen(): Observable<ResumenVentas> {
    return this.http.get<ResumenVentas>(`${this.apiUrl}/resumen`);
  }

  obtenerVentasPorDia(dias = 30): Observable<VentaPorDia[]> {
    return this.http.get<VentaPorDia[]>(`${this.apiUrl}/ventas-por-dia`, { params: { dias } });
  }

  obtenerProductosMasVendidos(cantidad = 5): Observable<ProductoMasVendido[]> {
    return this.http.get<ProductoMasVendido[]>(`${this.apiUrl}/productos-mas-vendidos`, { params: { cantidad } });
  }
}
