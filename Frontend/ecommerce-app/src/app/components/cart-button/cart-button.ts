import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CarritoService } from '../../Services/carrito.service';

@Component({
  selector: 'app-cart-button',
  imports: [RouterLink],
  templateUrl: './cart-button.html',
  styleUrl: './cart-button.css'
})
export class CartButton {
  carritoService = inject(CarritoService);
}
