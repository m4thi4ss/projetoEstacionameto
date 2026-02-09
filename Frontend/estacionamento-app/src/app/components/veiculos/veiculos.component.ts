import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ApiService } from '../../services/api.service';
import { DialogService } from '../../services/dialog.service';
import { Veiculo, TipoVeiculo, PagedResult, VeiculoFiltros } from '../../models/models';
import { PaginationComponent } from '../pagination/pagination.component';

@Component({
  selector: 'app-veiculos',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent, MatIconModule, MatTooltipModule],
  templateUrl: './veiculos.component.html',
  styleUrls: ['./veiculos.component.css']
})
export class VeiculosComponent implements OnInit {
  pagedResult: PagedResult<Veiculo> = {
    items: [],
    pageNumber: 1,
    pageSize: 5, // Alterado de 10 para 5 (deve ser igual ao filtros)
    totalCount: 0,
    totalPages: 0,
    hasPreviousPage: false,
    hasNextPage: false
  };

  filtros: VeiculoFiltros = {
    pageNumber: 1,
    pageSize: 5, // Alterado de 10 para 5 (padrão inicial)
    placa: '',
    tipo: undefined,
    orderBy: 'placa',
    descending: false
  };

  veiculo: Veiculo = { placa: '', tipo: TipoVeiculo.Carro };
  editando: boolean = false;
  veiculoEditandoId: number | null = null;
  loading: boolean = false;
  error: string = '';
  showForm: boolean = false;
  showFilters: boolean = false;

  tiposVeiculo = [
    { nome: 'Todos', valor: undefined },
    { nome: 'Carro', valor: TipoVeiculo.Carro },
    { nome: 'Moto', valor: TipoVeiculo.Moto },
    { nome: 'Caminhonete', valor: TipoVeiculo.Caminhonete },
    { nome: 'Van', valor: TipoVeiculo.Van }
  ];

  opcoesOrdenacao = [
    { nome: 'Placa', valor: 'placa' },
    { nome: 'Modelo', valor: 'modelo' },
    { nome: 'Tipo', valor: 'tipo' },
    { nome: 'Data de Cadastro', valor: 'data' }
  ];

  constructor(
    private apiService: ApiService,
    private dialogService: DialogService
  ) {}

  ngOnInit(): void {
    this.carregarVeiculos();
  }

  carregarVeiculos(): void {
    this.loading = true;
    this.apiService.getVeiculosPaged(this.filtros).subscribe({
      next: (data) => {
        this.pagedResult = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Erro ao carregar veículos';
        this.loading = false;
        console.error(err);
      }
    });
  }

  aplicarFiltros(): void {
    this.filtros.pageNumber = 1; // Reset para primeira página
    this.carregarVeiculos();
  }

  limparFiltros(): void {
    this.filtros = {
      pageNumber: 1,
      pageSize: this.filtros.pageSize,
      placa: '',
      tipo: undefined,
      orderBy: 'placa',
      descending: false
    };
    this.carregarVeiculos();
  }

  ordenarPor(campo: string): void {
    if (this.filtros.orderBy === campo) {
      this.filtros.descending = !this.filtros.descending;
    } else {
      this.filtros.orderBy = campo;
      this.filtros.descending = false;
    }
    this.carregarVeiculos();
  }

  onPageChanged(page: number): void {
    this.filtros.pageNumber = page;
    this.carregarVeiculos();
  }

  onPageSizeChanged(pageSize: number): void {
    console.log('📏 Page Size Changed:', pageSize);
    this.filtros.pageSize = pageSize;
    this.filtros.pageNumber = 1; // Reset para primeira página
    this.carregarVeiculos();
  }

  novoVeiculo(): void {
    this.veiculo = { placa: '', tipo: TipoVeiculo.Carro };
    this.editando = false;
    this.showForm = true;
  }

  editarVeiculo(v: Veiculo): void {
    let tipoNumerico: TipoVeiculo;
    
    if (typeof v.tipo === 'string') {
      switch (v.tipo) {
        case 'Carro':
          tipoNumerico = TipoVeiculo.Carro;
          break;
        case 'Moto':
          tipoNumerico = TipoVeiculo.Moto;
          break;
        case 'Caminhonete':
          tipoNumerico = TipoVeiculo.Caminhonete;
          break;
        case 'Van':
          tipoNumerico = TipoVeiculo.Van;
          break;
        default:
          tipoNumerico = TipoVeiculo.Carro;
      }
    } else {
      tipoNumerico = v.tipo as TipoVeiculo;
    }

    this.veiculo = {
      ...v,
      tipo: tipoNumerico
    };
    
    this.editando = true;
    this.veiculoEditandoId = v.id!;
    this.showForm = true;
  }

  salvarVeiculo(): void {
    if (!this.veiculo.placa) {
      this.dialogService.alert('Atenção', 'Placa é obrigatória');
      return;
    }

    this.loading = true;

    if (this.editando && this.veiculoEditandoId) {
      this.apiService.updateVeiculo(this.veiculoEditandoId, this.veiculo).subscribe({
        next: () => {
          this.carregarVeiculos();
          this.cancelar();
          this.dialogService.alert('Sucesso', 'Veículo atualizado com sucesso!');
        },
        error: (err) => {
          this.error = 'Erro ao atualizar veículo';
          this.loading = false;
          this.dialogService.alert('Erro', this.error);
        }
      });
    } else {
      this.apiService.createVeiculo(this.veiculo).subscribe({
        next: () => {
          this.carregarVeiculos();
          this.cancelar();
          this.dialogService.alert('Sucesso', 'Veículo cadastrado com sucesso!');
        },
        error: (err) => {
          this.error = 'Erro ao cadastrar veículo';
          this.loading = false;
          this.dialogService.alert('Erro', this.error);
        }
      });
    }
  }

  deletarVeiculo(id: number, placa: string): void {
    this.dialogService.confirm('Excluir veículo', `Deseja realmente excluir o veículo ${placa}?`).then(ok => {
      if (!ok) return;
      this.apiService.deleteVeiculo(id).subscribe({
        next: () => {
          this.carregarVeiculos();
          this.dialogService.alert('Sucesso', 'Veículo excluído com sucesso!');
        },
        error: (err) => {
          this.error = 'Erro ao excluir veículo';
          this.dialogService.alert('Erro', this.error);
        }
      });
    });
  }

  registrarEntrada(placa: string): void {
    this.apiService.registrarEntrada(placa).subscribe({
      next: () => {
        this.dialogService.alert('Sucesso', `Entrada registrada para ${placa}`);
      },
      error: (err) => {
        const mensagem = err.error?.message || 'Erro ao registrar entrada';
        this.dialogService.alert('Aviso', mensagem);
      }
    });
  }

  cancelar(): void {
    this.veiculo = { placa: '', tipo: TipoVeiculo.Carro };
    this.editando = false;
    this.veiculoEditandoId = null;
    this.showForm = false;
    this.error = '';
  }

  getTipoNome(tipo: string | TipoVeiculo): string {
    if (typeof tipo === 'string') return tipo;
    return TipoVeiculo[tipo] || 'Desconhecido';
  }

  /** Retorna o ícone do Material Icons para o tipo de veículo (apenas ícone, sem texto). */
  getTipoIcon(tipo: string | TipoVeiculo): string {
    const nome = this.getTipoNome(tipo).toLowerCase();
    if (nome === 'moto') return 'two_wheeler';
    if (nome === 'caminhonete') return 'local_shipping';
    if (nome === 'van') return 'airport_shuttle';
    return 'directions_car'; // Carro ou padrão
  }

  toggleFilters(): void {
    this.showFilters = !this.showFilters;
  }
}
