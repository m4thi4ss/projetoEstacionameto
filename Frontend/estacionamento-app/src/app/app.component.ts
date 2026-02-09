import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from './services/auth.service';
import { DialogService } from './services/dialog.service';
import { ToastComponent } from './components/shared/toast.component';
import { DialogComponent } from './components/shared/dialog.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, ToastComponent, DialogComponent, MatIconModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'Sistema de Estacionamento';

  constructor(
    public authService: AuthService,
    private dialogService: DialogService
  ) {}

  logout(): void {
    this.dialogService.confirm('Sair', 'Deseja realmente sair do sistema?').then(ok => {
      if (ok) this.authService.logout();
    });
  }
}
