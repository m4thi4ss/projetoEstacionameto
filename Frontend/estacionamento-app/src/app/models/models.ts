// Veículos
export interface Veiculo {
  id?: number;
  placa: string;
  modelo?: string;
  cor?: string;
  tipo: TipoVeiculo | string;
  dataCadastro?: Date;
}

export enum TipoVeiculo {
  Carro = 1,
  Moto = 2,
  Caminhonete = 3,
  Van = 4
}

export interface VeiculoPatio {
  veiculoId: number;
  sessaoId: number;
  placa: string;
  modelo?: string;
  cor?: string;
  tipo: string;
  dataHoraEntrada: Date;
  tempoEstacionado: string;
  valorAtual: number;
}

export interface Sessao {
  id: number;
  veiculoId: number;
  placaVeiculo: string;
  dataHoraEntrada: Date;
  dataHoraSaida?: Date;
  valorCobrado?: number;
  sessaoAberta: boolean;
  tempoEstacionado: string;
}

export interface SessaoSaida {
  sessaoId: number;
  placa: string;
  dataHoraEntrada: Date;
  dataHoraSaida: Date;
  tempoEstacionado: string;
  valorCobrado: number;
}

export interface FaturamentoDiario {
  data: Date;
  valorTotal: number;
  quantidadeSaidas: number;
}

export interface VeiculoTempoEstacionado {
  placa: string;
  modelo?: string;
  quantidadeSessoes: number;
  totalHorasEstacionadas: number;
  tempoFormatado: string;
}

export interface OcupacaoPorHora {
  dataHora: Date;
  quantidadeVeiculos: number;
}

// Paginação
export interface PaginationParams {
  pageNumber: number;
  pageSize: number;
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface VeiculoFiltros extends PaginationParams {
  placa?: string;
  tipo?: number;
  orderBy?: string;
  descending?: boolean;
}

export interface SessaoFiltros extends PaginationParams {
  placa?: string;
  sessaoAberta?: boolean;
  dataInicio?: Date;
  dataFim?: Date;
  orderBy?: string;
  descending?: boolean;
}

// Auth Models
export interface LoginRequest {
  email: string;
  senha: string;
}

export interface LoginResponse {
  token: string;
  usuario: UsuarioAuth;
}

export interface UsuarioAuth {
  id: number;
  nome: string;
  email: string;
  perfil: string;
  ativo: boolean;
}

export interface CriarUsuarioRequest {
  nome: string;
  email: string;
  senha: string;
  perfil: number; // 1=Operador, 2=Admin
}

export interface AlterarSenhaRequest {
  senhaAtual: string;
  novaSenha: string;
}
