import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { ApiService } from '../../services/api.service';
import { DialogService } from '../../services/dialog.service';
import { FaturamentoDiario, VeiculoTempoEstacionado, OcupacaoPorHora } from '../../models/models';

@Component({
  selector: 'app-relatorios',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule],
  templateUrl: './relatorios.component.html',
  styleUrls: ['./relatorios.component.css']
})
export class RelatoriosComponent implements OnInit {
  // Faturamento
  faturamentos: FaturamentoDiario[] = [];
  diasFaturamento: number = 7;
  
  // Top Veículos
  topVeiculos: VeiculoTempoEstacionado[] = [];
  dataInicioTop: string = '';
  dataFimTop: string = '';
  
  // Ocupação
  ocupacoes: OcupacaoPorHora[] = [];
  dataInicioOcupacao: string = '';
  dataFimOcupacao: string = '';

  loading: boolean = false;
  error: string = '';
  relatorioAtivo: string = 'faturamento';

  constructor(
    private apiService: ApiService,
    private dialogService: DialogService
  ) {}

  ngOnInit(): void {
    this.setDatasDefault();
    // Carrega o relatório da aba ativa inicial
    if (this.relatorioAtivo === 'faturamento') {
      this.carregarFaturamento();
    }
  }

  setDatasDefault(): void {
    const hoje = new Date();
    const seteDiasAtras = new Date(hoje);
    seteDiasAtras.setDate(hoje.getDate() - 7);

    this.dataInicioTop = this.formatDate(seteDiasAtras);
    this.dataFimTop = this.formatDate(hoje);
    this.dataInicioOcupacao = this.formatDate(seteDiasAtras);
    this.dataFimOcupacao = this.formatDate(hoje);
  }

  formatDate(date: Date): string {
    return date.toISOString().split('T')[0];
  }

  selecionarRelatorio(tipo: string): void {
    this.relatorioAtivo = tipo;
    this.error = '';

    // Sempre recarrega ao selecionar
    if (tipo === 'faturamento') {
      this.carregarFaturamento();
    } else if (tipo === 'topVeiculos') {
      this.carregarTopVeiculos();
    } else if (tipo === 'ocupacao') {
      this.carregarOcupacao();
    }
  }

  carregarFaturamento(): void {
    this.loading = true;
    this.error = '';

    this.apiService.getFaturamentoDiario(this.diasFaturamento).subscribe({
      next: (data) => {
        this.faturamentos = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao carregar faturamento. Certifique-se de que o backend está rodando.';
        this.loading = false;
        console.error('Erro completo:', err);
      }
    });
  }

  carregarTopVeiculos(): void {
    if (!this.dataInicioTop || !this.dataFimTop) {
      this.dialogService.alert('Atenção', 'Selecione o período');
      return;
    }

    this.loading = true;
    this.error = '';

    // Criar datas no timezone local (sem conversão UTC)
    const [anoInicio, mesInicio, diaInicio] = this.dataInicioTop.split('-').map(Number);
    const [anoFim, mesFim, diaFim] = this.dataFimTop.split('-').map(Number);
    
    const dataInicio = new Date(anoInicio, mesInicio - 1, diaInicio, 0, 0, 0);
    const dataFim = new Date(anoFim, mesFim - 1, diaFim, 23, 59, 59);

    this.apiService.getTopVeiculosPorTempo(dataInicio, dataFim).subscribe({
      next: (data) => {
        this.topVeiculos = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao carregar top veículos. Verifique o período selecionado.';
        this.loading = false;
        console.error('Erro completo:', err);
      }
    });
  }

  carregarOcupacao(): void {
    if (!this.dataInicioOcupacao || !this.dataFimOcupacao) {
      this.dialogService.alert('Atenção', 'Selecione o período');
      return;
    }

    this.loading = true;
    this.error = '';

    // Criar datas no timezone local (sem conversão UTC)
    const [anoInicio, mesInicio, diaInicio] = this.dataInicioOcupacao.split('-').map(Number);
    const [anoFim, mesFim, diaFim] = this.dataFimOcupacao.split('-').map(Number);
    
    const dataInicio = new Date(anoInicio, mesInicio - 1, diaInicio, 0, 0, 0);
    const dataFim = new Date(anoFim, mesFim - 1, diaFim, 23, 59, 59);

    this.apiService.getOcupacaoPorHora(dataInicio, dataFim).subscribe({
      next: (data) => {
        this.ocupacoes = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message || 'Erro ao carregar ocupação. Verifique o período selecionado.';
        this.loading = false;
        console.error('Erro completo:', err);
      }
    });
  }

  getTotalFaturamento(): number {
    return this.faturamentos.reduce((sum, f) => sum + f.valorTotal, 0);
  }

  getTotalSaidas(): number {
    return this.faturamentos.reduce((sum, f) => sum + f.quantidadeSaidas, 0);
  }
}
