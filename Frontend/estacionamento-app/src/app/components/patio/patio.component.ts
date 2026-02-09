import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { ApiService } from '../../services/api.service';
import { ToastService } from '../../services/toast.service';
import { LoadingComponent } from '../shared/loading.component';
import { EmptyStateComponent } from '../shared/empty-state.component';
import { VeiculoPatio, SessaoSaida } from '../../models/models';

@Component({
  selector: 'app-patio',
  standalone: true,
  imports: [CommonModule, FormsModule, LoadingComponent, EmptyStateComponent, MatIconModule],
  templateUrl: './patio.component.html',
  styleUrls: ['./patio.component.css']
})
export class PatioComponent implements OnInit {
  veiculos: VeiculoPatio[] = [];
  filtroPlaca: string = '';
  loading: boolean = false;
  error: string = '';
  saidaCalculada: SessaoSaida | null = null;
  showConfirmModal: boolean = false;

  constructor(
    private apiService: ApiService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.carregarPatio();
  }

  carregarPatio(): void {
    this.loading = true;
    this.error = '';
    
    this.apiService.getPatioAgora(this.filtroPlaca || undefined).subscribe({
      next: (data) => {
        this.veiculos = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erro ao carregar veículos no pátio';
        this.loading = false;
        this.toastService.error('✕ Erro ao carregar veículos no pátio');
        console.error(err);
      }
    });
  }

  filtrar(): void {
    this.carregarPatio();
  }

  calcularSaida(veiculo: VeiculoPatio): void {
    this.loading = true;
    this.apiService.calcularSaida(veiculo.sessaoId).subscribe({
      next: (data) => {
        this.saidaCalculada = data;
        this.showConfirmModal = true;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erro ao calcular saída';
        this.loading = false;
        this.toastService.error('✕ Erro ao calcular saída');
        console.error(err);
      }
    });
  }

  confirmarSaida(): void {
    if (!this.saidaCalculada) return;

    this.loading = true;
    this.apiService.registrarSaida(this.saidaCalculada.sessaoId).subscribe({
      next: () => {
        this.showConfirmModal = false;
        this.saidaCalculada = null;
        this.carregarPatio();
        this.toastService.success('✓ Saída registrada com sucesso!');
      },
      error: (err) => {
        this.error = 'Erro ao registrar saída';
        this.loading = false;
        this.toastService.error('✕ Erro ao registrar saída');
        console.error(err);
      }
    });
  }

  cancelarSaida(): void {
    this.showConfirmModal = false;
    this.saidaCalculada = null;
  }

  /** Retorna o ícone do Material Icons para o tipo de veículo (apenas ícone, sem texto). */
  getTipoIcon(tipo: string): string {
    const t = (tipo || '').toLowerCase();
    if (t === 'moto') return 'two_wheeler';
    if (t === 'caminhonete') return 'local_shipping';
    if (t === 'van') return 'airport_shuttle';
    return 'directions_car'; // Carro ou padrão
  }
}
