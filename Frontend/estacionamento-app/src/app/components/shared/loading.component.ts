import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-loading',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div class="loading-container" [ngClass]="{'inline': inline}">
      <mat-icon class="spinner-icon">refresh</mat-icon>
      <p *ngIf="message" class="loading-message">{{ message }}</p>
    </div>
  `,
  styles: [`
    .loading-container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 3rem;
      min-height: 200px;
    }

    .loading-container.inline {
      min-height: auto;
      padding: 1rem;
    }

    .spinner-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      color: #2196F3;
      animation: spin 1s linear infinite;
    }

    .loading-message {
      margin-top: 1rem;
      color: #666;
      font-size: 0.95rem;
    }

    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }
  `]
})
export class LoadingComponent {
  @Input() message: string = '';
  @Input() inline: boolean = false;
}
