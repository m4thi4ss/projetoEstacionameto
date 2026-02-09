import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { ApiService } from '../../services/api.service';
import { Sessao, PagedResult, SessaoFiltros } from '../../models/models';
import { PaginationComponent } from '../pagination/pagination.component';

@Component({
  selector: 'app-historico-sessoes',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent, MatIconModule],
  templateUrl: './historico-sessoes.component.html',
  styleUrls: ['./historico-sessoes.component.css']
})
export class HistoricoSessoesComponent implements OnInit {
  pagedResult: PagedResult<Sessao> = {
    items: [],
    pageNumber: 1,
    pageSize: 5, // Alterado de 10 para 5 (deve ser igual ao filtros)
    totalCount: 0,
    totalPages: 0,
    hasPreviousPage: false,
    hasNextPage: false
  };

  filtros: SessaoFiltros = {
    pageNumber: 1,
    pageSize: 5, // Alterado de 10 para 5 (padrão inicial)
    placa: '',
    sessaoAberta: undefined,
    dataInicio: undefined,
    dataFim: undefined,
    orderBy: 'dataEntrada',
    descending: true
  };

  loading: boolean = false;
  error: string = '';
  showFilters: boolean = true;

  // Datas para o filtro (formato input date)
  dataInicioInput: string = '';
  dataFimInput: string = '';

  statusOpcoes = [
    { nome: 'Todas', valor: undefined },
    { nome: 'Abertas', valor: true },
    { nome: 'Fechadas', valor: false }
  ];

  opcoesOrdenacao = [
    { nome: 'Data de Entrada', valor: 'dataEntrada' },
    { nome: 'Data de Saída', valor: 'dataSaida' },
    { nome: 'Valor Cobrado', valor: 'valor' },
    { nome: 'Placa', valor: 'placa' }
  ];

  constructor(private apiService: ApiService) {}

  ngOnInit(): void {
    this.inicializarDatas();
    this.carregarSessoes();
  }

  inicializarDatas(): void {
    // Últimos 7 dias por padrão
    const hoje = new Date();
    const seteDiasAtras = new Date();
    seteDiasAtras.setDate(hoje.getDate() - 7);

    this.dataInicioInput = this.formatarDataParaInput(seteDiasAtras);
    this.dataFimInput = this.formatarDataParaInput(hoje);

    this.filtros.dataInicio = seteDiasAtras;
    this.filtros.dataFim = hoje;
  }

  formatarDataParaInput(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  carregarSessoes(): void {
    this.loading = true;
    this.error = '';

    this.apiService.getSessoesPaged(this.filtros).subscribe({
      next: (data) => {
        this.pagedResult = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erro ao carregar sessões';
        this.loading = false;
        console.error(err);
      }
    });
  }

  aplicarFiltros(): void {
    // Atualizar filtros de data a partir dos inputs
    if (this.dataInicioInput) {
      this.filtros.dataInicio = new Date(this.dataInicioInput + 'T00:00:00');
    }
    if (this.dataFimInput) {
      this.filtros.dataFim = new Date(this.dataFimInput + 'T23:59:59');
    }

    this.filtros.pageNumber = 1;
    this.carregarSessoes();
  }

  limparFiltros(): void {
    this.filtros = {
      pageNumber: 1,
      pageSize: this.filtros.pageSize,
      placa: '',
      sessaoAberta: undefined,
      dataInicio: undefined,
      dataFim: undefined,
      orderBy: 'dataEntrada',
      descending: true
    };

    this.dataInicioInput = '';
    this.dataFimInput = '';
    
    this.carregarSessoes();
  }

  onPageChanged(page: number): void {
    this.filtros.pageNumber = page;
    this.carregarSessoes();
  }

  onPageSizeChanged(pageSize: number): void {
    this.filtros.pageSize = pageSize;
    this.filtros.pageNumber = 1;
    this.carregarSessoes();
  }

  toggleFilters(): void {
    this.showFilters = !this.showFilters;
  }

  getStatusClass(aberta: boolean): string {
    return aberta ? 'status-aberta' : 'status-fechada';
  }

  getStatusTexto(aberta: boolean): string {
    return aberta ? 'Aberta' : 'Fechada';
  }
}
