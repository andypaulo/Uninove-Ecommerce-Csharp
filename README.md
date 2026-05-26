# Mangá e Lamen — E-Commerce

> Sistema de e-commerce desenvolvido para a disciplina de **Projeto e Modelagem de Sistemas de Software** — UNINOVE 5º Semestre.

---

## 📖 Sobre o Projeto

O **Mangá e Lamen** é uma plataforma de vendas online voltada para fãs de leitura, com foco em mangás e livros em geral. O sistema foi desenvolvido pela equipe **Code Nexus** como projeto integrador do semestre, aplicando na prática os conhecimentos das disciplinas de Métodos Ágeis, Engenharia de Software, Processos de Negócio, IHC e Técnicas de Programação.

A plataforma permite que clientes naveguem pelo catálogo, visualizem detalhes dos produtos e gerenciem seu carrinho de compras. Do lado administrativo, oferece um painel completo para gerenciamento do acervo da loja.

---

## 🚀 Funcionalidades

### Cliente
- Visualizar página inicial com informações de contato da loja
- Navegar pela listagem completa de produtos
- Ver detalhes de cada produto (nome, descrição, preço, estoque e categoria)
- Adicionar produtos ao carrinho
- Alterar quantidade de itens no carrinho
- Remover itens do carrinho
- Finalizar compra

### Administrador
- Cadastrar novos produtos (nome, descrição, preço, estoque, imagem e categoria)
- Editar produtos já cadastrados
- Excluir produtos do sistema
- Visualizar todos os produtos no painel de gerenciamento

---

## 🖥️ Telas do Sistema

| Tela | Descrição |
|---|---|
| Página Inicial | Apresenta um pouco sobre a loja e as formas de contato |
| Listagem de Produtos | Exibe todos os produtos disponíveis |
| Detalhes do Produto | Informações completas do produto selecionado |
| Gerenciamento de Produtos | Painel administrativo de CRUD |
| Carrinho de Compras | Gerenciamento dos itens selecionados |

---

## 🛠️ Tecnologias Utilizadas

- **ASP.NET MVC** — framework web
- **C#** — linguagem de programação
- **.NET 9** — plataforma de desenvolvimento
- **SQLite** — banco de dados local
- **Bootstrap** — estilização e responsividade
- **HTML/CSS** — estrutura e design das interfaces

---

## ⚙️ Como Rodar o Projeto

### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) instalado
- [Visual Studio 2022](https://visualstudio.microsoft.com/) ou superior (recomendado)

### Passo a passo

1. **Clone o repositório**
```bash
git clone https://github.com/seu-usuario/seu-repositorio.git
```

2. **Acesse a pasta do projeto**
```bash
cd EcommerceMVC
```

3. **Restaure as dependências**
```bash
dotnet restore
```

4. **Execute o projeto**
```bash
dotnet run
```

5. **Acesse no navegador**
```
https://localhost:5001
```

> O banco de dados SQLite já está incluído no repositório na pasta `/database`. Não é necessária nenhuma configuração adicional.

---

## 📁 Estrutura do Projeto

```
EcommerceMVC/
├── Controllers/
│   └── HomeController.cs       # Lógica das páginas e ações
├── Models/
│   └── Produto.cs              # Modelo de dados do produto
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml        # Página inicial
│   │   ├── Details.cshtml      # Detalhes do produto
│   │   ├── Gerenciamento.cshtml# Painel administrativo
│   │   └── Carrinho.cshtml     # Carrinho de compras
│   └── Shared/
│       └── _Layout.cshtml      # Layout base
├── database/
│   └── database.db             # Banco de dados SQLite
├── wwwroot/
│   └── img/                    # Imagens dos produtos
└── README.md
```

---

## 👥 Equipe — Code Nexus

| Membro | Função |
|---|---|
| Anderson Paulo | Gerente de Projetos / Desenvolvedor / QA |
| Kaick Delfino | UX Designer |
| João Marcelo | Desenvolvedor Back-end |
| Larisson da Silva | Desenvolvedor Back-end / Banco de Dados |
| Marcos Henrique | Desenvolvedor |
| Lucas Alberto | Desenvolvedor |
| José Rafael | Desenvolvedor |

---

## 🎓 Informações Acadêmicas

| | |
|---|---|
| **Instituição** | Universidade Nove de Julho — UNINOVE |
| **Disciplina** | Projeto e Modelagem de Sistemas de Software |
| **Professora** | Priscilla Cunha |
| **Semestre** | 5º Semestre — 2026 |

---

## 🎬 Vídeo de Apresentação

📺 [Assistir no YouTube](https://youtu.be/NzVUihCsta4)
