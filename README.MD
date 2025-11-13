# Backend - Sistema de Emissão de Notas Fiscais (Desafio Korp)

Este é o backend do desafio técnico, focado na implementação de um sistema de emissão de notas fiscais utilizando uma arquitetura de microsserviços. O projeto foi desenvolvido em C# com .NET, priorizando a resiliência, concorrência e boas práticas de design.

## 🏛️ Arquitetura

O coração do projeto é uma arquitetura de microsserviços desacoplada e orientada a eventos, usando o padrão *Saga* para comunicação assíncrona.



O sistema é dividido em dois serviços principais:

1.  **📦 Korp.Estoque**:
    * Responsável pelo CRUD de Produtos e controle de saldo.
    * Funciona como o "receptor" no fluxo de impressão, consumindo eventos do RabbitMQ para dar baixa no estoque.

2.  **🧾 Korp.Faturamento**:
    * Responsável pelo CRUD de Notas Fiscais.
    * Inicia o fluxo de impressão, atuando como o "publicador" de eventos.
    * Ouve os eventos de resposta (sucesso ou falha) para finalizar o status da nota.

Essa abordagem garante o **tratamento de falhas** : se o serviço de Estoque estiver offline, as solicitações de impressão ficarão na fila do RabbitMQ e serão processadas assim que o serviço voltar, sem perda de dados.

---

## ✨ Features Implementadas

Além dos requisitos básicos, o projeto implementa dois dos requisitos opcionais para demonstrar robustez.

* **CRUD de Produtos:** Criação e listagem de produtos com seus respectivos saldos.
* **CRUD de Notas Fiscais:**  Criação de notas com status inicial "Aberta" e numeração sequencial.
* **Fluxo de Impressão Assíncrono:**  Endpoint que inicia o processo de "impressão", mudando o status da nota e atualizando o saldo.
* **Resiliência e Tratamento de Falhas:**  O uso de RabbitMQ garante que o sistema se recupere de falhas.
* **✅ (Opcional) Implementação de Idempotência:**  O endpoint `.../imprimir` usa um header `X-Idempotency-Key` para garantir que requisições duplicadas não causem efeitos colaterais (como dar baixa no estoque duas vezes).
* **✅ (Opcional) Tratamento de Concorrência:** O serviço de Estoque usa `RowVersion` (Timestamp) no EF Core para detectar e tratar ativamente conflitos de concorrência (cenário onde duas notas tentam usar o último item do estoque ao mesmo tempo).

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

1. Korp.Estoque - Swagger: https://localhost:5274/swagger
2. Korp.Faturamento Swagger: https://localhost:7005/swagger


# 💻Frontend 

A interface foi desenvolvida em Angular, atendendo ao requisito principal do desafio. O projeto utiliza a abordagem de Standalone Components para uma arquitetura mais leve e moderna.

### 🛠️ Tecnologias e Bibliotecas
* Angular 17+: Framework principal.
* Angular Material: Utilizado como biblioteca de componentes visuais para garantir uma UI consistente e responsiva (uso de MatTable, MatCard, MatSnackBar, etc.).
* RxJS: Utilizado para o gerenciamento de chamadas assíncronas e reatividade na comunicação com as APIs de Estoque e Faturamento.
* UUID: Biblioteca utilizada para gerar identificadores únicos (GUIDs) no frontend, permitindo o envio do header X-Idempotency-Key para garantir a idempotência nas operações de impressão.

### ✨ Funcionalidades no Frontend
* Gestão de Estoque: Formulário para cadastro de produtos e listagem com visualização de saldo em tempo real.
* Emissão de Notas: Interface para criação de notas fiscais com múltiplos itens.
* Botão de Impressão Inteligente: Botão visível e intuitivo que dispara o processo assíncrono.
* Exibe indicador de processamento (Spinner) durante a requisição.
* Utiliza MatSnackBar (Toast) para feedback não-bloqueante de sucesso ou erro.
* Bloqueia a reimpressão de notas que não estejam com status "Aberta".

### 🚀 Como Rodar o Frontend (Modo Desenvolvimento)
```shell
cd frontend
npm install
```

```shell
ng serve
```

O projeto estará disponível em http://localhost:4200

## 🐳 Rodando Tudo com Docker 

### 1. Na raiz do projeto, crie um arquivo .env com a senha do banco:
```
DB_PASSWORD=SuaSenhaForteAqui!
```

### 2. Execute o comando de build e start:
```
docker-compose up --build
```