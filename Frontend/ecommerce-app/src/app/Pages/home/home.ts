import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { Producto } from '../../Models/producto.model';
import { CategoriaHome } from '../../Models/categoria.model';
import { ProductoService } from '../../Services/producto.service';
import { CategoriaService } from '../../Services/categoria.service';
import { CarritoService } from '../../Services/carrito.service';
import { AuthService } from '../../Services/auth.service';
import { Router, RouterLink } from '@angular/router';
import { CarruselProductos } from '../../components/carrusel-productos/carrusel-productos';

interface GrupoCategoria {
  categoriaId: number;
  nombre: string;
  productos: Producto[];
}

@Component({
  selector: 'app-home',
  imports: [CarruselProductos, RouterLink],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class Home implements OnInit {
  private productoService = inject(ProductoService);
  private categoriaService = inject(CategoriaService);
  private carritoService = inject(CarritoService);
  private authService = inject(AuthService);
  private router = inject(Router);

  productos = signal<Producto[]>([]);
  categoriasHome = signal<CategoriaHome[]>([]);
  cargando = signal(true);
  error = signal<string | null>(null);
  agregandoId = signal<number | null>(null);

  destacados = computed(() => this.productos().filter((p) => p.destacado));

  categorias = computed<GrupoCategoria[]>(() => {
    const productosPorCategoria = new Map<number, Producto[]>();

    for (const producto of this.productos()) {
      if (!productosPorCategoria.has(producto.categoriaId)) {
        productosPorCategoria.set(producto.categoriaId, []);
      }
      productosPorCategoria.get(producto.categoriaId)!.push(producto);
    }

    return this.categoriasHome()
      .map((cat) => ({
        categoriaId: cat.id,
        nombre: cat.nombre,
        productos: productosPorCategoria.get(cat.id) ?? []
      }))
      .filter((grupo) => grupo.productos.length > 0);
  });

  ngOnInit(): void {
    this.productoService.obtenerTodos().subscribe({
      next: (data) => {
        this.productos.set(data);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('No se pudieron cargar los productos. Intentá de nuevo más tarde.');
        this.cargando.set(false);
      }
    });

    this.categoriaService.obtenerParaHome().subscribe({
      next: (data) => this.categoriasHome.set(data)
    });
  }

  agregarAlCarrito(producto: Producto): void {
    if (!this.authService.estaLogueado()) {
      this.router.navigateByUrl('/login');
      return;
    }

    this.agregandoId.set(producto.id);

    this.carritoService.agregarItem(producto.id, 1).subscribe({
      next: () => this.agregandoId.set(null),
      error: (err) => {
        this.agregandoId.set(null);
        alert(err.error?.mensaje ?? 'No se pudo agregar el producto al carrito');
      }
    });
  }
}
