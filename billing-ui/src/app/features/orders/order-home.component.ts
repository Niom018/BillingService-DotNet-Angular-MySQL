import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-order-home',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './order-home.component.html'
})
export class OrderHomeComponent {
  orderId: number | null = null;

  constructor(private router: Router) {}

  goToOrder(): void {
    if (this.orderId) {
      this.router.navigate(['/orders', this.orderId]);
    }
  }
}
