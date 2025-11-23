import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-transactions',
  imports: [CommonModule],
  template: `
    <div class="text-slate-200">Transactions list goes here.</div>
  `
})
export class TransactionsComponent {}
