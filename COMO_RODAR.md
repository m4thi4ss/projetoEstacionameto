# Como rodar o projeto (passo a passo)

Siga estes passos **na ordem** para rodar o sistema na sua máquina sem erros.

---

## 1. Pré-requisitos (instale antes)

| Ferramenta    | Versão mínima | Onde baixar |
|---------------|----------------|-------------|
| **.NET SDK**  | 8.0            | https://dotnet.microsoft.com/download/dotnet/8.0 |
| **Node.js**   | 18.x (LTS)     | https://nodejs.org/ |

**Verificar instalação:**
```bash
dotnet --version   # deve mostrar 8.x.x
node --version     # deve mostrar v18.x ou v20.x
npm --version      # deve mostrar 9.x ou 10.x
```

---

## 2. Terminal 1 – Backend (API)

Abra um terminal na **pasta raiz do projeto** (onde está este arquivo) e execute:

```bash
cd Backend/EstacionamentoAPI
dotnet restore
dotnet run
```

**Espere** aparecer algo como: `Now listening on: http://localhost:5000`  
**Deixe esse terminal aberto.** Não feche.

- API: http://localhost:5000  
- Swagger: http://localhost:5000/swagger  
- O banco SQLite (`estacionamento.db`) é criado automaticamente na primeira execução.

---

## 3. Terminal 2 – Frontend (Angular)

Abra **outro terminal** (novo), na pasta raiz do projeto:

```bash
cd Frontend/estacionamento-app
npm install
npm start
```

**Espere** a compilação terminar e aparecer: `Application bundle generation complete` e a URL.  
A aplicação abre em: **http://localhost:4200**

---

## 4. Acessar o sistema

1. Abra o navegador em: **http://localhost:4200**
2. Você será redirecionado para a tela de **Login**.
3. Use as credenciais padrão:
   - **E-mail:** `admin@estacionamento.com`
   - **Senha:** `admin123`
4. Clique em **Entrar**. Você será levado para a tela **Pátio**.

---

## Problemas comuns

### “Port 4200 is already in use” ou “Port 5000 is already in use”
Outra instância do projeto (ou outro app) está usando a porta.  
**Solução:** Feche o outro processo ou use a opção “Y” quando o Angular perguntar se quer usar outra porta (ex.: 4201). Se usar outra porta, acesse a URL que o terminal mostrar.

### “Não foi possível copiar ... EstacionamentoAPI.exe ... arquivo em uso”
A API já está rodando em outro terminal.  
**Solução:** Use a API que já está rodando, ou feche aquele processo (Ctrl+C no terminal da API) e rode de novo.

### Erro ao abrir a aplicação no navegador (tela em branco ou erro de rede)
O **Backend não está rodando** ou não está em `http://localhost:5000`.  
**Solução:** Confirme que o Terminal 1 está com `dotnet run` ativo e que não deu erro. A API precisa estar no ar antes do frontend.

### `npm install` falha com erro de versão (ERESOLVE, peer dependency)
**Solução:** Use Node.js **18 LTS** ou **20 LTS**. Evite Node 15 ou 16. Reinstale as dependências: apague a pasta `node_modules` e o arquivo `package-lock.json` dentro de `Frontend/estacionamento-app`, depois rode `npm install` de novo.

### Tela de login não carrega ou dá erro 401
A API está fora do ar ou em outra porta. Confirme que no Terminal 1 está rodando `dotnet run` e que a URL da API no frontend é `http://localhost:5000` (em `Frontend/estacionamento-app/src/app/services/api.service.ts` a variável `baseUrl` deve ser essa).

---

## Resumo

| O que        | Onde rodar              | Comando        | URL              |
|-------------|--------------------------|----------------|------------------|
| Backend API | `Backend/EstacionamentoAPI` | `dotnet run`   | http://localhost:5000  |
| Frontend    | `Frontend/estacionamento-app` | `npm install` e `npm start` | http://localhost:4200  |
| Login       | Navegador                | -              | admin@estacionamento.com / admin123 |

**Ordem:** 1) Backend, 2) Frontend, 3) Abrir http://localhost:4200 e fazer login.
