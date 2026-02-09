# Sistema de Gerenciamento de Estacionamento

Sistema completo para controle de entrada/saída de veículos em estacionamento, desenvolvido com ASP.NET Core Web API e Angular 17.

---

## Rodar na sua máquina

**Para rodar o projeto sem erros, use o guia:** **[COMO_RODAR.md](COMO_RODAR.md)**

Em resumo:
1. **Pré-requisitos:** .NET 8 SDK e Node.js 18+ instalados.
2. **Terminal 1** – Backend: `cd Backend/EstacionamentoAPI`, depois `dotnet restore`, depois `dotnet run` (deixe rodando).
3. **Terminal 2** – Frontend: `cd Frontend/estacionamento-app`, depois `npm install`, depois `npm start`.
4. Abra **http://localhost:4200** e faça login com: `admin@estacionamento.com` / `admin123`.

Se der erro de porta em uso ou “arquivo em uso”, consulte a seção **Problemas comuns** no [COMO_RODAR.md](COMO_RODAR.md).

---

### Tecnologias
- **Backend:** ASP.NET Core 8.0 Web API
- **Banco de Dados:** SQLite com Entity Framework Core
- **Frontend:** Angular 17 (Standalone Components)
- **Arquitetura:** MVC em camadas (Models, DTOs, Repositories, Interfaces, Controllers, ViewModels)

### Funcionalidades Implementadas

#### 1. Cadastro de Veículos
- **Campos:** Placa (obrigatório), Modelo, Cor, Tipo
- **Validações:**
  - Placa é única (não permite duplicatas)
  - Validação de campos obrigatórios
  - Conversão automática de placa para maiúsculas
- **Operações:** Criar, Editar, Listar, Excluir

#### 2. Movimentação de Entrada/Saída
- **Entrada:**
  - Veículo não pode entrar se já estiver no pátio (sessão aberta)
  - Registro automático de data/hora de entrada
- **Saída:**
  - Veículo só pode sair se tiver sessão aberta
  - Cálculo e exibição do valor antes de confirmar
  - Registro de data/hora de saída e valor cobrado

#### 3. Precificação
- **Primeira hora:** R$ 5,00 (configurável)
- **Demais horas:** R$ 3,00 por hora (configurável)
- **Arredondamento:** Qualquer fração de hora é cobrada como hora completa
  - Exemplo: 1h30min = 2 horas = R$ 5,00 (1ª hora) + R$ 3,00 (2ª hora) = R$ 8,00
  - Exemplo: 45min = 1 hora = R$ 5,00

#### 4. Interface de Operação (Frontend)

**Tela "Pátio Agora":**
- Lista veículos com sessão aberta
- Busca por placa
- Exibe: Placa, Modelo, Cor, Tipo, Hora de Entrada, Tempo Estacionado, Valor Atual
- Ação "Registrar Saída" com confirmação mostrando valor

**Tela "Cadastro de Veículo":**
- Formulário para criar/editar veículo
- Lista todos os veículos cadastrados
- Botão para registrar entrada rápida
- Validações no formulário

**Tela "Relatórios":**
- 3 tipos de relatórios em abas

#### 5. Relatórios (SQL / Consultas)

**Faturamento por Dia:**
- Últimos 7, 15, 30, 60 ou 90 dias
- Mostra: Data, Valor Total, Quantidade de Saídas, Ticket Médio
- Totalizadores gerais

**Top 10 Veículos por Tempo Estacionado:**
- Período customizável (data início e fim)
- Ranking dos veículos que mais tempo ficaram estacionados
- Mostra: Placa, Modelo, Quantidade de Sessões, Total de Horas

**Ocupação por Hora:**
- Período customizável
- Quantidade de veículos no pátio por hora do dia
- Útil para identificar horários de pico

## Arquitetura do Projeto

### Backend (ASP.NET Core Web API)

```
Backend/EstacionamentoAPI/
├── Controllers/           # Endpoints da API
│   ├── VeiculosController.cs
│   ├── SessoesController.cs
│   └── RelatoriosController.cs
├── Models/               # Entidades do banco
│   ├── Veiculo.cs
│   ├── Sessao.cs
│   └── Configuracao.cs
├── DTOs/                 # Data Transfer Objects
│   ├── VeiculoDTO.cs
│   └── SessaoDTO.cs
├── ViewModels/           # Modelos de resposta
│   ├── VeiculoViewModel.cs
│   ├── SessaoViewModel.cs
│   └── RelatorioViewModel.cs
├── Interfaces/           # Contratos dos repositórios
│   ├── IVeiculoRepository.cs
│   ├── ISessaoRepository.cs
│   ├── IRelatorioRepository.cs
│   └── IConfiguracaoRepository.cs
├── Repositories/         # Implementação de acesso a dados
│   ├── VeiculoRepository.cs
│   ├── SessaoRepository.cs
│   ├── RelatorioRepository.cs
│   └── ConfiguracaoRepository.cs
├── Data/                 # Contexto do Entity Framework
│   └── EstacionamentoContext.cs
└── Program.cs           # Configuração e startup
```

### Frontend (Angular 17)

```
Frontend/estacionamento-app/
├── src/
│   ├── app/
│   │   ├── components/
│   │   │   ├── patio/           # Tela "Pátio Agora"
│   │   │   ├── veiculos/        # Tela "Cadastro de Veículos"
│   │   │   └── relatorios/      # Tela "Relatórios"
│   │   ├── models/              # Interfaces TypeScript
│   │   │   └── models.ts
│   │   ├── services/            # Serviços HTTP
│   │   │   └── api.service.ts
│   │   ├── app.component.*      # Componente raiz
│   │   ├── app.routes.ts        # Rotas da aplicação
│   │   └── app.config.ts        # Configurações
│   ├── index.html
│   ├── main.ts
│   └── styles.css
├── angular.json
├── package.json
└── tsconfig.json
```

## Como Executar

> **Recomendado:** Use o passo a passo completo em **[COMO_RODAR.md](COMO_RODAR.md)** para evitar erros (portas, ordem de execução, login).

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) (recomendado: 18 LTS ou 20 LTS)
- Angular CLI é opcional; o `npm start` usa o que está no projeto.

### Backend (API)

**Importante:** Execute o Backend **primeiro** e deixe-o rodando. Depois abra outro terminal para o Frontend.

1. Navegue até a pasta do backend:
```bash
cd Backend/EstacionamentoAPI
```

2. Restaure as dependências:
```bash
dotnet restore
```

3. Execute a aplicação:
```bash
dotnet run
```

A API estará disponível em: `http://localhost:5000`

Swagger UI (documentação): `http://localhost:5000/swagger`

**Nota:** O banco de dados SQLite será criado automaticamente na primeira execução (`estacionamento.db`)

### Frontend (Angular)

1. Navegue até a pasta do frontend:
```bash
cd Frontend/estacionamento-app
```

2. Instale as dependências:
```bash
npm install
```

3. Execute a aplicação:
```bash
npm start
```

A aplicação estará disponível em: `http://localhost:4200`

**Login padrão:** E-mail `admin@estacionamento.com` / Senha `admin123`

**Se der erro:** Porta em uso, “arquivo em uso” no build ou tela em branco, veja **[COMO_RODAR.md](COMO_RODAR.md)** (seção Problemas comuns).

## Endpoints da API

### Veículos
- `GET /api/veiculos` - Lista todos os veículos
- `GET /api/veiculos/{id}` - Busca veículo por ID
- `GET /api/veiculos/placa/{placa}` - Busca veículo por placa
- `POST /api/veiculos` - Cadastra novo veículo
- `PUT /api/veiculos/{id}` - Atualiza veículo
- `DELETE /api/veiculos/{id}` - Remove veículo

### Sessões
- `GET /api/sessoes` - Lista todas as sessões
- `GET /api/sessoes/{id}` - Busca sessão por ID
- `GET /api/sessoes/patio?placa={placa}` - Lista veículos no pátio
- `POST /api/sessoes/entrada` - Registra entrada
- `POST /api/sessoes/saida/calcular` - Calcula valor da saída
- `POST /api/sessoes/saida` - Confirma saída

### Relatórios
- `GET /api/relatorios/faturamento-diario?dias={dias}` - Faturamento por dia
- `GET /api/relatorios/top-veiculos-tempo?dataInicio={date}&dataFim={date}` - Top 10 veículos
- `GET /api/relatorios/ocupacao-por-hora?dataInicio={date}&dataFim={date}` - Ocupação por hora

## Regras de Precificação

A precificação segue as seguintes regras:

1. **Primeira hora:** R$ 5,00 fixo
2. **Horas adicionais:** R$ 3,00 por hora
3. **Arredondamento:** Sempre para cima (ceil)
   - 0-60 minutos = 1 hora = R$ 5,00
   - 61-120 minutos = 2 horas = R$ 8,00
   - 121-180 minutos = 3 horas = R$ 11,00
   - E assim por diante...

**Fórmula:**
```
Horas totais = TETO(tempo em horas)
Se horas totais <= 1:
    Valor = R$ 5,00
Senão:
    Valor = R$ 5,00 + ((horas totais - 1) × R$ 3,00)
```

Os valores são configuráveis no banco de dados (tabela `Configuracoes`).

## Internacionalização

O sistema está preparado para comercialização no Brasil e Argentina:

- Formato de moeda configurável
- Campos de texto sem formatação rígida de placa
- Suporte a diferentes tipos de veículos
- Interface em português (pode ser estendida para espanhol)

## Banco de Dados

### Schema SQLite

**Tabela: Veiculos**
- Id (INTEGER, PK)
- Placa (TEXT, UNIQUE, NOT NULL)
- Modelo (TEXT)
- Cor (TEXT)
- Tipo (INTEGER, NOT NULL) - Enum: 1=Carro, 2=Moto, 3=Caminhonete, 4=Van
- DataCadastro (TEXT)

**Tabela: Sessoes**
- Id (INTEGER, PK)
- VeiculoId (INTEGER, FK)
- DataHoraEntrada (TEXT, NOT NULL)
- DataHoraSaida (TEXT)
- ValorCobrado (DECIMAL(10,2))
- SessaoAberta (INTEGER, NOT NULL) - Boolean

**Tabela: Configuracoes**
- Id (INTEGER, PK)
- Chave (TEXT, UNIQUE)
- Valor (TEXT)
- Descricao (TEXT)

Dados iniciais:
- PrecoPrimeiraHora: 5.00
- PrecoDemaisHoras: 3.00

## Interface do Usuário

O frontend foi desenvolvido com design moderno e responsivo:

- **Cards visuais** para veículos no pátio
- **Tabelas organizadas** para listagens
- **Formulários validados** com feedback visual
- **Modal de confirmação** antes de registrar saída
- **Filtros e buscas** em tempo real
- **Relatórios com gráficos visuais** e totalizadores
- **Menu de navegação** intuitivo
- **Cores e layout profissionais**

## Validações Implementadas

### Backend
- Placa obrigatória e única
- Tipo de veículo obrigatório
- Veículo não pode entrar se já estiver no pátio
- Veículo só pode sair se tiver sessão aberta
- Validações de data nos relatórios

### Frontend
- Campos obrigatórios marcados com *
- Limites de caracteres
- Validação de formulário antes de envio
- Confirmações para ações destrutivas
- Mensagens de erro amigáveis

## Observações

- O projeto segue boas práticas de desenvolvimento
- Código limpo e bem organizado
- Separação de responsabilidades (SoC)
- Padrão Repository para acesso a dados
- Dependency Injection no ASP.NET Core
- Componentes standalone no Angular (padrão mais recente)
- CORS configurado para desenvolvimento

## Suporte Futuro

O sistema está preparado para expansões:
- Adicionar autenticação/autorização
- Relatórios mais avançados com gráficos
- Exportação para PDF/Excel
- Notificações em tempo real
- Sistema de reservas
- Múltiplos estacionamentos
- Integração com sistemas de pagamento

---
