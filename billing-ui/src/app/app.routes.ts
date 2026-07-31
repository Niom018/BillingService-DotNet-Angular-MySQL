import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { LoginComponent } from './features/login/login.component';
import { ProductsComponent } from './features/products/products.component';
import { CustomersComponent } from './features/customers/customers.component';
import { OrderHomeComponent } from './features/orders/order-home.component';
import { OrderCreateComponent } from './features/orders/order-create.component';
import { OrderDetailComponent } from './features/orders/order-detail.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'products', component: ProductsComponent, canActivate: [authGuard] },
  { path: 'customers', component: CustomersComponent, canActivate: [authGuard] },
  { path: 'orders', component: OrderHomeComponent, canActivate: [authGuard] },
  { path: 'orders/new', component: OrderCreateComponent, canActivate: [authGuard] },
  { path: 'orders/:id', component: OrderDetailComponent, canActivate: [authGuard] },
  { path: '', redirectTo: 'products', pathMatch: 'full' },
  { path: '**', redirectTo: 'products' }
];
