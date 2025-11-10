# Backend - Sistema de Emissão de Notas Fiscais (Desafio Korp)

[cite_start]Este é o backend do desafio técnico[cite: 2], focado na implementação de um sistema de emissão de notas fiscais utilizando uma arquitetura de microsserviços. O projeto foi desenvolvido em C# com .NET, priorizando a resiliência, concorrência e boas práticas de design.

## 🏛️ Arquitetura

[cite_start]O coração do projeto é uma arquitetura de microsserviços [cite: 40] desacoplada e orientada a eventos, usando o padrão *Saga* para comunicação assíncrona.



[cite_start]O sistema é dividido em dois serviços principais[cite: 41]:

1.  **📦 Korp.Estoque**:
    * [cite_start]Responsável pelo CRUD de Produtos e controle de saldo[cite: 42].
    * Funciona como o "receptor" no fluxo de impressão, consumindo eventos do RabbitMQ para dar baixa no estoque.

2.  **🧾 Korp.Faturamento**:
    * [cite_start]Responsável pelo CRUD de Notas Fiscais[cite: 42].
    * Inicia o fluxo de impressão, atuando como o "publicador" de eventos.
    * Ouve os eventos de resposta (sucesso ou falha) para finalizar o status da nota.

[cite_start]Essa abordagem garante o **tratamento de falhas** [cite: 43][cite_start]: se o serviço de Estoque estiver offline, as solicitações de impressão ficarão na fila do RabbitMQ e serão processadas assim que o serviço voltar, sem perda de dados[cite: 45].

---

## ✨ Features Implementadas

Além dos requisitos básicos, o projeto implementa dois dos requisitos opcionais para demonstrar robustez.

* [cite_start]**CRUD de Produtos:** [cite: 19] Criação e listagem de produtos com seus respectivos saldos.
* [cite_start]**CRUD de Notas Fiscais:** [cite: 25] Criação de notas com status inicial "Aberta" e numeração sequencial.
* [cite_start]**Fluxo de Impressão Assíncrono:** [cite: 31] [cite_start]Endpoint que inicia o processo de "impressão", mudando o status da nota [cite: 35] [cite_start]e atualizando o saldo[cite: 37].
* [cite_start]**Resiliência e Tratamento de Falhas:** [cite: 43] O uso de RabbitMQ garante que o sistema se recupere de falhas.
* [cite_start]**✅ (Opcional) Implementação de Idempotência:** [cite: 52] [cite_start]O endpoint `.../imprimir` usa um header `X-Idempotency-Key` para garantir que requisições duplicadas não causem efeitos colaterais (como dar baixa no estoque duas vezes)[cite: 53].
* [cite_start]**✅ (Opcional) Tratamento de Concorrência:** [cite: 48] [cite_start]O serviço de Estoque usa `RowVersion` (Timestamp) no EF Core para detectar e tratar ativamente conflitos de concorrência (cenário onde duas notas tentam usar o último item do estoque ao mesmo tempo)[cite: 49].

---

## 🛠️ Tecnologias Utilizadas

* **.NET 8** (C#)
* **ASP.NET Core** (para as Web APIs)
* **Entity Framework Core** (para acesso a dados)
* **Docker** (para rodar os serviços de infraestrutura)
* **SQL Server** (rodando em Docker, com bancos separados)
* **RabbitMQ** (rodando em Docker, para mensageria)
* **MassTransit** (Abstração de alto nível para RabbitMQ em .NET)
* **Swagger/OpenAPI** (para documentação de API)

---

## 🏁 Como Rodar o Projeto

Você precisará do **.NET 8 SDK** e do **Docker Desktop** instalados.

### 1. Iniciar a Infraestrutura (Docker)

Os serviços de banco de dados e mensageria rodam em containers.

```bash
# 1. Inicie o SQL Server (lembre-se de usar sua senha)
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=SuaSenhaForteAqui" -p 1433:1433 --name korp-sql-server -d [mcr.microsoft.com/mssql/server:2022-latest](https://mcr.microsoft.com/mssql/server:2022-latest)

# 2. Inicie o RabbitMQ (com o painel de gerenciamento)
docker run -d --hostname my-rabbit --name korp-rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

## 2 - Preparar os Bancos de Dados (Migrations)

Execute as migrations do EF Core para cada serviço (em terminais separados).

```bash
# Terminal 1: Korp.Estoque
cd src/Korp.Estoque
dotnet ef database update

# Terminal 2: Korp.Faturamento
cd src/Korp.Faturamento
dotnet ef database update
```
## 3 - Rodar as APIs

```bash
# Terminal 1: Korp.Estoque
cd src/Korp.Estoque
dotnet run

# Terminal 2: Korp.Faturamento
cd src/Korp.Faturamento
dotnet run
```

## Testes

### rotas da aplicação para testes:

1. Korp.Estoque - Swagger: https://localhost:<porta_estoque>/swagger
2. Korp.Faturamento Swagger: https://localhost:<porta_faturamento>/swagger