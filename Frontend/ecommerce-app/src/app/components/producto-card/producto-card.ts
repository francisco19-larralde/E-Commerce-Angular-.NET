import { Component, inject, input, output } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Producto } from '../../Models/producto.model';
import { DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-producto-card',
  imports: [RouterLink, DecimalPipe],
  templateUrl: './producto-card.html',
  styleUrl: './producto-card.css'
})
export class ProductoCard {
  private router = inject(Router);

  producto = input.required<Producto>();
  agregando = input(false);

  agregarClick = output<Producto>();

  onAgregarClick(): void {
    const producto = this.producto();

    if (producto.tieneVariantes) {
      this.router.navigate(['/productos', producto.id]);
      return;
    }

    this.agregarClick.emit(producto);
  }

  imagenProducto(): string {
    const producto = this.producto();

    if (producto.imagenUrl) {
      return producto.imagenUrl;
    }

    const categoria = producto.categoriaNombre?.toLocaleLowerCase('es') ?? '';

    if (categoria.includes('calzado')) return '/assets/store/fallback-calzado.jpg';
    if (categoria.includes('indumentaria')) return '/assets/store/fallback-indumentaria.jpg';
    if (categoria.includes('accesorio')) return '/assets/store/fallback-accesorios.jpg';

    return '/assets/store/landing-hero.jpg';
  }
}
