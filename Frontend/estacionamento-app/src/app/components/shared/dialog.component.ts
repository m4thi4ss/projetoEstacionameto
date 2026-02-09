import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { DialogService } from '../../services/dialog.service';

@Component({
  selector: 'app-dialog',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div *ngIf="(dialogService.dialog$ | async) as dialog" class="dialog-overlay" (click)="onOverlayClick(dialog)">
      <div class="dialog-content" (click)="$event.stopPropagation()">
        <h2>
          <mat-icon class="dialog-icon">{{ dialog.type === 'confirm' ? 'help_outline' : 'info' }}</mat-icon>
          {{ dialog.title }}
        </h2>
        <p class="dialog-message">{{ dialog.message }}</p>
        <div class="dialog-actions">
          <ng-container *ngIf="dialog.type === 'alert'">
            <button type="button" class="btn btn-primary" (click)="dialogService.close()">
              <mat-icon>check</mat-icon> OK
            </button>
          </ng-container>
          <ng-container *ngIf="dialog.type === 'confirm'">
            <button type="button" class="btn btn-success" (click)="dialogService.close(true)">
              <mat-icon>check</mat-icon> Confirmar
            </button>
            <button type="button" class="btn btn-secondary" (click)="dialogService.close(false)">
              <mat-icon>close</mat-icon> Cancelar
            </button>
          </ng-container>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dialog-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.6);
      backdrop-filter: blur(4px);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
      animation: dialogFadeIn 0.3s ease;
    }

    .dialog-content {
      background: white;
      padding: 2rem;
      border-radius: 16px;
      max-width: 440px;
      width: 90%;
      box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
      animation: dialogSlideUp 0.3s ease;
    }

    .dialog-content h2 {
      margin: 0 0 1rem 0;
      color: #333;
      font-size: 1.35rem;
      display: flex;
      align-items: center;
      gap: 10px;
    }

    .dialog-icon {
      color: #667eea;
      font-size: 28px;
      width: 28px;
      height: 28px;
    }

    .dialog-message {
      margin: 0 0 1.5rem 0;
      color: #555;
      line-height: 1.5;
      font-size: 15px;
    }

    .dialog-actions {
      display: flex;
      gap: 10px;
      justify-content: flex-end;
      flex-wrap: wrap;
    }

    .dialog-actions .btn {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 10px 18px;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      font-size: 14px;
      font-weight: 500;
      transition: all 0.2s;
    }

    .dialog-actions .btn mat-icon {
      font-size: 20px;
      width: 20px;
      height: 20px;
    }

    .dialog-actions .btn-primary {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
    }

    .dialog-actions .btn-success {
      background: #28a745;
      color: white;
    }

    .dialog-actions .btn-secondary {
      background: #6c757d;
      color: white;
    }

    .dialog-actions .btn:hover {
      opacity: 0.9;
      transform: translateY(-1px);
    }

    @keyframes dialogFadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }

    @keyframes dialogSlideUp {
      from {
        transform: translateY(20px);
        opacity: 0;
      }
      to {
        transform: translateY(0);
        opacity: 1;
      }
    }
  `]
})
export class DialogComponent {
  constructor(public dialogService: DialogService) {}

  onOverlayClick(dialog: { type: string }): void {
    if (dialog.type === 'alert') {
      this.dialogService.close();
    }
  }
}
