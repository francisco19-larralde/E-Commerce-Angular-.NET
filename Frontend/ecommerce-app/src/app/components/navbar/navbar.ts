import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../Services/auth.service';
import { Buscador } from '../buscador/buscador';
import { AccountMenu } from '../account-menu/account-menu';
import { CartButton } from '../cart-button/cart-button';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, Buscador, AccountMenu, CartButton],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class Navbar {
  authService = inject(AuthService);
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
