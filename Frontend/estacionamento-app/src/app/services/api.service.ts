import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { 
  Veiculo, 
  VeiculoPatio, 
  Sessao, 
  SessaoSaida,
  FaturamentoDiario,
  VeiculoTempoEstacionado,
  OcupacaoPorHora,
  UsuarioAuth,
  CriarUsuarioRequest,
  AlterarSenhaRequest,
  PagedResult,
  VeiculoFiltros,
  SessaoFiltros
} from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = 'http://localhost:5000/api';

  constructor(private http: HttpClient) {
    console.log('🔗 API Service inicializado - URL:', this.baseUrl);
  }

  // Veículos
  getVeiculos(): Observable<Veiculo[]> {
    return this.http.get<Veiculo[]>(`${this.baseUrl}/veiculos`);
  }

  getVeiculosPaged(filtros: VeiculoFiltros): Observable<PagedResult<Veiculo>> {
    let params = new HttpParams()
      .set('PageNumber', filtros.pageNumber.toString())
      .set('PageSize', filtros.pageSize.toString());

    if (filtros.placa) {
      params = params.set('Placa', filtros.placa);
    }
    if (filtros.tipo !== undefined && filtros.tipo !== null) {
      params = params.set('Tipo', filtros.tipo.toString());
    }
    if (filtros.orderBy) {
      params = params.set('OrderBy', filtros.orderBy);
    }
    if (filtros.descending !== undefined) {
      params = params.set('Descending', filtros.descending.toString());
    }

    return this.http.get<PagedResult<Veiculo>>(`${this.baseUrl}/veiculos/paged`, { params });
  }

  getVeiculoById(id: number): Observable<Veiculo> {
    return this.http.get<Veiculo>(`${this.baseUrl}/veiculos/${id}`);
  }

  getVeiculoByPlaca(placa: string): Observable<Veiculo> {
    return this.http.get<Veiculo>(`${this.baseUrl}/veiculos/placa/${placa}`);
  }

  createVeiculo(veiculo: Veiculo): Observable<Veiculo> {
    return this.http.post<Veiculo>(`${this.baseUrl}/veiculos`, veiculo);
  }

  updateVeiculo(id: number, veiculo: Partial<Veiculo>): Observable<Veiculo> {
    return this.http.put<Veiculo>(`${this.baseUrl}/veiculos/${id}`, veiculo);
  }

  deleteVeiculo(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/veiculos/${id}`);
  }

  // Sessões
  getSessoes(): Observable<Sessao[]> {
    return this.http.get<Sessao[]>(`${this.baseUrl}/sessoes`);
  }

  getSessoesPaged(filtros: SessaoFiltros): Observable<PagedResult<Sessao>> {
    let params = new HttpParams()
      .set('PageNumber', filtros.pageNumber.toString())
      .set('PageSize', filtros.pageSize.toString());

    if (filtros.placa) {
      params = params.set('Placa', filtros.placa);
    }
    if (filtros.sessaoAberta !== undefined && filtros.sessaoAberta !== null) {
      params = params.set('SessaoAberta', filtros.sessaoAberta.toString());
    }
    if (filtros.dataInicio) {
      params = params.set('DataInicio', this.formatarData(filtros.dataInicio));
    }
    if (filtros.dataFim) {
      params = params.set('DataFim', this.formatarData(filtros.dataFim));
    }
    if (filtros.orderBy) {
      params = params.set('OrderBy', filtros.orderBy);
    }
    if (filtros.descending !== undefined) {
      params = params.set('Descending', filtros.descending.toString());
    }

    return this.http.get<PagedResult<Sessao>>(`${this.baseUrl}/sessoes/paged`, { params });
  }

  getPatioAgora(placa?: string): Observable<VeiculoPatio[]> {
    let params = new HttpParams();
    if (placa) {
      params = params.set('placa', placa);
    }
    return this.http.get<VeiculoPatio[]>(`${this.baseUrl}/sessoes/patio`, { params });
  }

  registrarEntrada(placa: string): Observable<Sessao> {
    return this.http.post<Sessao>(`${this.baseUrl}/sessoes/entrada`, { placa });
  }

  calcularSaida(sessaoId: number): Observable<SessaoSaida> {
    return this.http.post<SessaoSaida>(`${this.baseUrl}/sessoes/saida/calcular`, { sessaoId });
  }

  registrarSaida(sessaoId: number): Observable<Sessao> {
    return this.http.post<Sessao>(`${this.baseUrl}/sessoes/saida`, { sessaoId });
  }

  // Relatórios
  getFaturamentoDiario(dias: number = 7): Observable<FaturamentoDiario[]> {
    return this.http.get<FaturamentoDiario[]>(`${this.baseUrl}/relatorios/faturamento-diario?dias=${dias}`);
  }

  getTopVeiculosPorTempo(dataInicio: Date, dataFim: Date): Observable<VeiculoTempoEstacionado[]> {
    const params = new HttpParams()
      .set('dataInicio', this.formatarData(dataInicio))
      .set('dataFim', this.formatarData(dataFim));
    return this.http.get<VeiculoTempoEstacionado[]>(`${this.baseUrl}/relatorios/top-veiculos-tempo`, { params });
  }

  getOcupacaoPorHora(dataInicio: Date, dataFim: Date): Observable<OcupacaoPorHora[]> {
    const params = new HttpParams()
      .set('dataInicio', this.formatarData(dataInicio))
      .set('dataFim', this.formatarData(dataFim));
    return this.http.get<OcupacaoPorHora[]>(`${this.baseUrl}/relatorios/ocupacao-por-hora`, { params });
  }

  // Usuários (Admin)
  getUsuarios(): Observable<UsuarioAuth[]> {
    return this.http.get<UsuarioAuth[]>(`${this.baseUrl}/usuarios`);
  }

  createUsuario(usuario: CriarUsuarioRequest): Observable<UsuarioAuth> {
    return this.http.post<UsuarioAuth>(`${this.baseUrl}/usuarios`, usuario);
  }

  updateUsuario(id: number, usuario: Partial<UsuarioAuth>): Observable<UsuarioAuth> {
    return this.http.put<UsuarioAuth>(`${this.baseUrl}/usuarios/${id}`, usuario);
  }

  deleteUsuario(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/usuarios/${id}`);
  }

  alterarSenha(dados: AlterarSenhaRequest): Observable<any> {
    return this.http.post(`${this.baseUrl}/usuarios/alterar-senha`, dados);
  }

  // Helper para formatar datas
  private formatarData(data: Date): string {
    const ano = data.getFullYear();
    const mes = String(data.getMonth() + 1).padStart(2, '0');
    const dia = String(data.getDate()).padStart(2, '0');
    const hora = String(data.getHours()).padStart(2, '0');
    const min = String(data.getMinutes()).padStart(2, '0');
    const seg = String(data.getSeconds()).padStart(2, '0');
    return `${ano}-${mes}-${dia}T${hora}:${min}:${seg}`;
  }
}
