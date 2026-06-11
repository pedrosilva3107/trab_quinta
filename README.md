# Sistema de Farmácia (SNGPC / MIP) — ASP.NET Core Web API

Projeto monolítico simplificado que implementa o fluxo de entrada de mercadoria,
controle de estoque, controle de medicamentos controlados (SNGPC) e ponto de venda.

## Stack
- ASP.NET Core 10 Web API (Controllers)
- Entity Framework Core + SQLite (`farmacia.db`, criado automaticamente)
- Swagger (interface de testes em `/swagger`)
- Frontend Angular (pasta `frontend/`)

## Como rodar

### Opção 1 - tudo via API (mais simples para a defesa)
O Angular já vem **compilado e copiado** para `wwwroot/`, então basta subir o backend:
```
dotnet run
```
Acesse:
- `http://localhost:<porta>/` → frontend Angular
- `http://localhost:<porta>/swagger` → Swagger (a porta aparece no console)

### Opção 2 - desenvolvimento (Angular com hot-reload)
Em um terminal, suba a API:
```
dotnet run --urls http://localhost:5180
```
Em outro terminal, suba o Angular (usa `frontend/proxy.conf.json` para redirecionar `/api` e `/swagger` para a API):
```
cd frontend
npm install
npm start
```
Acesse `http://localhost:4200/`.

> Após alterar o frontend, gere o build de produção e copie para `wwwroot`:
> ```
> cd frontend
> npm run build
> cd ..
> rm -rf wwwroot/* && cp -r frontend/dist/frontend/browser/* wwwroot/
> ```

Login de farmacêutico de demonstração (RF03): usuário `ana`, senha `1234`.

## Estrutura
- `Models/` — entidades: Produto, LoteEstoque, Farmaceutico, RetencaoSngpc, Venda
- `Models/Dto/` — `NfeDto` (XML simplificado da NF-e) e `VendaDto` (payload do PDV)
- `Helpers/CryptoHelper.cs` — criptografia AES (RNF03/LGPD) e hash de senha (RF03)
- `Data/AppDbContext.cs` — contexto EF Core, com seed do farmacêutico "ana"
- `Controllers/`
  - `ProdutosController` — catálogo (RF02)
  - `EntradaController` — importação do XML da NF-e (RF01-RF04)
  - `EstoqueController` — saldo e alertas de vencimento (RF05)
  - `VendasController` — PDV, abate de estoque (RF03, RF07)
  - `SngpcController` — retenções e arquivo de transmissão (RF06)

## Roteiro de defesa: requisito → onde está implementado

| Requisito | Implementação |
|---|---|
| RF01 - Importar XML da NF-e | `EntradaController.ImportarXml`, modelo `NfeDto` (deserialização XML). Use `exemplo-nfe.xml` |
| RF02 - Categorizar Controlado/Comum/Conveniência | `enum TipoProduto`; aplicado em `EntradaController` ao criar/atualizar `Produto` |
| RF03 - Assinatura/senha do farmacêutico p/ controlados | `EntradaController` e `VendasController` validam `FarmaceuticoNome`/`Senha` via `CryptoHelper.HashSenha` contra `Farmaceutico.SenhaHash` |
| RF04 - Lote e validade | `LoteEstoque` (NumeroLote, DataValidade), gravado em `EntradaController` |
| RF05 - Alerta 30 dias antes do vencimento | `EstoqueController.GetProximosVencimentos` (`/api/estoque/vencimentos?dias=30`) |
| RF06 - Arquivo de transmissão SNGPC | `SngpcController.GerarArquivoTransmissao` (`POST /api/sngpc/transmissao`) gera `.txt` com as movimentações de controlados |
| RF07 - Abate automático do estoque na venda | `VendasController.Confirmar` decrementa `LoteEstoque.Quantidade` |
| RNF01 - Atualização de estoque < 2s | Operação simples em SQLite local (commit único via `SaveChangesAsync`), sem chamadas externas |
| RNF02 - Disponibilidade 99,9% | Discutir como decisão arquitetural (não implementado em código): hospedagem redundante, monitoramento, etc. |
| RNF03 - Criptografia de dados de receita/paciente (LGPD) | `CryptoHelper.Criptografar/Descriptografar` (AES-256), campo `RetencaoSngpc.ReceitaPacienteCriptografado` |

## Fluxo de teste sugerido (via Swagger)
1. `POST /api/entrada/importar-xml` — envie `exemplo-nfe.xml`. Para o item "Rivotril"
   (Controlado), informe `FarmaceuticoNome=ana`, `FarmaceuticoSenha=1234`,
   `NomePaciente` e `NumeroReceita`.
2. `GET /api/estoque` — confirme que os 3 lotes foram criados.
3. `GET /api/estoque/vencimentos?dias=400` — veja que o "Rivotril" (validade próxima) aparece.
4. `GET /api/sngpc/retencoes` — veja a retenção criada, com a receita já descriptografada na resposta.
5. `POST /api/vendas` — venda alguns itens (use os `LoteEstoqueId` retornados em `/api/estoque`).
   Para o item controlado, informe novamente as credenciais do farmacêutico.
6. `GET /api/estoque` — confirme que o saldo foi abatido (RF07).
7. `POST /api/sngpc/transmissao` — baixa o arquivo `.txt` com as movimentações (entrada + saída) e marca como transmitidas.

## Observações sobre simplificações (para a defesa)
- O XML da NF-e usado é um **layout simplificado**, não o schema oficial completo da SEFAZ —
  feito para focar nas regras de negócio (RF01-RF04) sem a complexidade do XML real.
- A "assinatura digital" do farmacêutico (RF03) foi simplificada para usuário/senha (hash SHA-256).
  Em produção seria um certificado digital (ICP-Brasil).
- Arquitetura original do trabalho previa microsserviços (Catálogo, Estoque, SNGPC, Inbound) +
  RabbitMQ/Kafka + API Gateway com JWT. Aqui está consolidado em um único projeto de API (.NET)
  para viabilizar a implementação e a defesa individual, mantendo a separação por
  Controllers/camadas como preparação para uma futura decomposição em microsserviços.
  O **frontend Angular foi implementado** (item da arquitetura original atendido); Gateway/JWT
  e mensageria (RabbitMQ/Kafka) **não foram implementados** nesta versão.
