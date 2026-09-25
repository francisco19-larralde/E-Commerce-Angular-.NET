import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_URL } from '../config/api-url.token';
import { Producto } from '../Models/producto.model';
import { Paginacion } from '../Models/paginacion.model';
import { FiltroCatalogo } from '../Models/filtro-catalogo.model';

export interface CrearProductoRequest {
  nombre: string;
  descripcion: string | null;
  precio: number;
  stock: number;
  imagenUrl: string | null;
  destacado: boolean;
  tieneVariantes: boolean;
  categoriaId: number;
}

export interface ActualizarEstadoRequest {
  destacado?: boolean;
  activo?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ProductoService {
  private http = inject(HttpClient);
  private apiUrl = `${inject(API_URL)}/productos`;

  obtenerTodos(): Observable<Producto[]> {
    return this.http.get<Producto[]>(this.apiUrl);
  }

  obtenerPorId(id: number): Observable<Producto> {
    return this.http.get<Producto>(`${this.apiUrl}/${id}`);
  }

  obtenerPaginado(
    pagina: number,
    tamanioPagina: number,
    categoriaId: number | null,
    busqueda: string
  ): Observable<Paginacion<Producto>> {
    let params = new HttpParams()
      .set('pagina', pagina)
      .set('tamanioPagina', tamanioPagina);

    if (categoriaId) {
      params = params.set('categoriaId', categoriaId);
    }
    if (busqueda.trim()) {
      params = params.set('busqueda', busqueda.trim());
    }

    return this.http.get<Paginacion<Producto>>(`${this.apiUrl}/admin`, { params });
  }

  crear(datos: CrearProductoRequest): Observable<Producto> {
    return this.http.post<Producto>(this.apiUrl, datos);
  }

  actualizar(id: number, datos: CrearProductoRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, datos);
  }

  buscar(termino: string): Observable<Producto[]> {
    const params = new HttpParams().set('termino', termino);
    return this.http.get<Producto[]>(`${this.apiUrl}/buscar`, { params });
  }

  actualizarEstado(id: number, cambios: ActualizarEstadoRequest): Observable<Producto> {
    return this.http.patch<Producto>(`${this.apiUrl}/${id}/estado`, cambios);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  obtenerCatalogo(filtro: FiltroCatalogo): Observable<Paginacion<Producto>> {
    let params = new HttpParams()
      .set('pagina', filtro.pagina)
      .set('tamanioPagina', filtro.tamanioPagina)
      .set('ordenarPor', filtro.ordenarPor);

    if (filtro.categoriaId) params = params.set('categoriaId', filtro.categoriaId);
    if (filtro.precioMin) params = params.set('precioMin', filtro.precioMin);
    if (filtro.precioMax) params = params.set('precioMax', filtro.precioMax);
    if (filtro.talle) params = params.set('talle', filtro.talle);
    if (filtro.busqueda?.trim()) params = params.set('busqueda', filtro.busqueda.trim());

    return this.http.get<Paginacion<Producto>>(`${this.apiUrl}/catalogo`, { params });
  }

  obtenerTallesDisponibles(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/talles-disponibles`);
  }
}
