import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LandingBenefits } from './landing-benefits/landing-benefits';
import { LandingEditorial } from './landing-editorial/landing-editorial';
import { LandingHero } from './landing-hero/landing-hero';


@Component({
  selector: 'app-landing',
  imports: [RouterLink, LandingHero, LandingEditorial, LandingBenefits],
  templateUrl: './landing.html',
  styleUrl: './landing.css'
})
export class Landing {
  menuAbierto = signal(false);

  alternarMenu(): void {
    this.menuAbierto.update((abierto) => !abierto);
  }

  cerrarMenu(): void {
    this.menuAbierto.set(false);
  }
}
