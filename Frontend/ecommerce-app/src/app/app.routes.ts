import { Routes } from '@angular/router';
import { guestGuard } from './Guards/guest.guard';
import { authGuard } from './Guards/auth.guard';
import { adminGuard } from './Guards/admin.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./Pages/landing/landing').then((m) => m.Landing)
  },
  {
    path: 'home',
    loadComponent: () => import('./Pages/home/home').then((m) => m.Home)
  },
  {
    path: 'catalogo',
    loadComponent: () => import('./Pages/catalogo/catalogo').then((m) => m.Catalogo)
  },
  {
    path: 'productos/:id',
    loadComponent: () =>
      import('./Pages/producto-detalle/producto-detalle').then((m) => m.ProductoDetalle)
  },
  {
    path: 'login',
    loadComponent: () => import('./Pages/login/login').then((m) => m.Login),
    canActivate: [guestGuard]
  },
  {
    path: 'registro',
    loadComponent: () => import('./Pages/registro/registro').then((m) => m.Registro),
    canActivate: [guestGuard]
  },
  {
    path: 'carrito',
    loadComponent: () => import('./Pages/carrito/carrito').then((m) => m.CarritoPage),
    canActivate: [authGuard]
  },
  {
    path: 'checkout',
    loadComponent: () => import('./Pages/checkout/checkout').then((m) => m.Checkout),
    canActivate: [authGuard]
  },
  {
    path: 'mis-compras',
    loadComponent: () => import('./Pages/mis-compras/mis-compras').then((m) => m.MisCompras),
    canActivate: [authGuard]
  },
  {
    path: 'mis-compras/:id',
    loadComponent: () => import('./Pages/mis-compras/mis-compras').then((m) => m.MisCompras),
    canActivate: [authGuard]
  },

  {
    path: 'admin',
    loadComponent: () =>
      import('./Pages/admin/admin-layout/admin-layout').then((m) => m.AdminLayout),
    canActivate: [adminGuard],
    children: [
      { path: '', redirectTo: 'productos', pathMatch: 'full' },
      {
        path: 'productos',
        loadComponent: () =>
          import('./Pages/admin/productos-admin/productos-admin').then((m) => m.ProductosAdmin)
      },
      {
        path: 'productos/nuevo',
        loadComponent: () =>
          import('./Pages/admin/producto-form/producto-form').then((m) => m.ProductoForm)
      },
      {
        path: 'productos/editar/:id',
        loadComponent: () =>
          import('./Pages/admin/producto-form/producto-form').then((m) => m.ProductoForm)
      },
      {
        path: 'categorias',
        loadComponent: () =>
          import('./Pages/admin/categorias-admin/categorias-admin').then(
            (m) => m.CategoriasAdmin
          )
      },
      {
        path: 'estadisticas',
        loadComponent: () =>
          import('./Pages/admin/dashboard-admin/dashboard-admin').then((m) => m.DashboardAdmin)
      }
    ]
  },

  { path: '**', redirectTo: 'home' }
];
