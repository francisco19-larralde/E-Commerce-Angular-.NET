import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../Services/auth.service';
import { CarritoService } from '../../Services/carrito.service';
import { Buscador } from '../buscador/buscador';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, Buscador],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class Navbar {
  authService = inject(AuthService);
  carritoService = inject(CarritoService);
  router = inject(Router);
  menuAbierto = signal(false);

  alternarMenu(): void {
    this.menuAbierto.update((abierto) => !abierto);
  }

  cerrarMenu(): void {
    this.menuAbierto.set(false);
  }

  cerrarSesion(): void {
    this.cerrarMenu();
    this.authService.logout();
    this.router.navigate(['/home']);
  }
}
