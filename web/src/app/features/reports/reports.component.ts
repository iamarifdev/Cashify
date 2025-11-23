import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-reports',
  imports: [CommonModule],
  template: `<div class="text-slate-200">Reports will render here.</div>`
})
export class ReportsComponent {}
