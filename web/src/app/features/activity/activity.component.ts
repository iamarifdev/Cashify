import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-activity',
  imports: [CommonModule],
  template: `<div class="text-slate-200">Activity timeline will render here.</div>`
})
export class ActivityComponent {}
