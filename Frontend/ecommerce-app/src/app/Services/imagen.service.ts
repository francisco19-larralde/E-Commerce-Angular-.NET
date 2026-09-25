import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_URL } from '../config/api-url.token';

@Injectable({
  providedIn: 'root'
})
export class ImagenService {
  private http = inject(HttpClient);
  private apiUrl = `${inject(API_URL)}/productos`;

  subir(productoId: number, archivo: File): Observable<{ imagenUrl: string }> {
    const formData = new FormData();
    formData.append('archivo', archivo);
    return this.http.post<{ imagenUrl: string }>(`${this.apiUrl}/${productoId}/imagen`, formData);
  }

  eliminar(productoId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${productoId}/imagen`);
  }
}
