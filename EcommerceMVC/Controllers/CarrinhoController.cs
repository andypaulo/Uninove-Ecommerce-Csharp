using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System;

namespace EcommerceMVC.Controllers
{
    public class CarrinhoController : Controller
    {
        private const string ConnectionString = "Data Source=database/database.db;";

        [HttpPost]
        public IActionResult Adicionar([FromBody] AdicionarCarrinhoDTO dto)
        {
            if (dto == null) return BadRequest("Corpo inválido.");

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var produtoCommand = connection.CreateCommand();
            produtoCommand.CommandText = "SELECT id, preco, estoque, nome FROM produto WHERE id = @id";
            produtoCommand.Parameters.AddWithValue("@id", dto.ProdutoId);

            int produtoId = 0;
            decimal preco = 0;
            int estoqueDisponivel = 0;
            string nomeProduto = "";

            using (var reader = produtoCommand.ExecuteReader())
            {
                if (!reader.Read())
                    return NotFound("Produto não encontrado.");

                produtoId = reader.GetInt32(0);
                preco = reader.GetDecimal(1);
                estoqueDisponivel = reader.GetInt32(2);
                nomeProduto = reader.GetString(3);
            }

            if (estoqueDisponivel <= 0)
            {
                return BadRequest($"O mangá '{nomeProduto}' está esgotado e não pode ser adicionado ao carrinho.");
            }

            var criarCarrinho = connection.CreateCommand();
            criarCarrinho.CommandText = @"
            CREATE TABLE IF NOT EXISTS carrinho (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                valorTotal REAL DEFAULT 0
            )";
            criarCarrinho.ExecuteNonQuery();

            var criarItens = connection.CreateCommand();
            criarItens.CommandText = @"
            CREATE TABLE IF NOT EXISTS itens_carrinho (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                carrinho_id INTEGER,
                produto_id INTEGER,
                quantidade INTEGER
            )";
            criarItens.ExecuteNonQuery();

            try
            {
                var corrigir = connection.CreateCommand();
                corrigir.CommandText = "ALTER TABLE carrinho ADD COLUMN valorTotal REAL DEFAULT 0";
                corrigir.ExecuteNonQuery();
            }
            catch {}

            int carrinhoId = 0;
            var carrinhoCommand = connection.CreateCommand();
            carrinhoCommand.CommandText = "SELECT id FROM carrinho LIMIT 1";
            var result = carrinhoCommand.ExecuteScalar();

            if (result == null)
            {
                var insertCarrinho = connection.CreateCommand();
                insertCarrinho.CommandText = "INSERT INTO carrinho (valorTotal) VALUES (0)";
                insertCarrinho.ExecuteNonQuery();

                var lastIdCommand = connection.CreateCommand();
                lastIdCommand.CommandText = "SELECT last_insert_rowid()";
                carrinhoId = Convert.ToInt32(lastIdCommand.ExecuteScalar());
            }
            else
            {
                carrinhoId = Convert.ToInt32(result);
            }

            var itemCommand = connection.CreateCommand();
            itemCommand.CommandText = @"
            SELECT id, quantidade
            FROM itens_carrinho
            WHERE carrinho_id = @carrinho_id
            AND produto_id = @produto_id";

            itemCommand.Parameters.AddWithValue("@carrinho_id", carrinhoId);
            itemCommand.Parameters.AddWithValue("@produto_id", produtoId);

            int itemId = 0;
            int quantidadeNoCarrinho = 0;

            using (var reader = itemCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    itemId = reader.GetInt32(0);
                    quantidadeNoCarrinho = reader.GetInt32(1);
                }
            }

            if (itemId > 0)
            {
                if (quantidadeNoCarrinho + 1 > estoqueDisponivel)
                {
                    return BadRequest($"Você já possui a quantidade máxima disponível em estoque no seu carrinho ({estoqueDisponivel} un).");
                }

                var update = connection.CreateCommand();
                update.CommandText = @"
                UPDATE itens_carrinho
                SET quantidade = @quantidade
                WHERE id = @id";

                update.Parameters.AddWithValue("@quantidade", quantidadeNoCarrinho + 1);
                update.Parameters.AddWithValue("@id", itemId);
                update.ExecuteNonQuery();
            }
            else
            {
                var insert = connection.CreateCommand();
                insert.CommandText = @"
                INSERT INTO itens_carrinho
                (carrinho_id, produto_id, quantidade)
                VALUES
                (@carrinho_id, @produto_id, @quantidade)";

                insert.Parameters.AddWithValue("@carrinho_id", carrinhoId);
                insert.Parameters.AddWithValue("@produto_id", produtoId);
                insert.Parameters.AddWithValue("@quantidade", 1);
                insert.ExecuteNonQuery();
            }

            var totalCommand = connection.CreateCommand();
            totalCommand.CommandText = @"
            SELECT SUM(itens_carrinho.quantidade * produto.preco)
            FROM itens_carrinho
            INNER JOIN produto ON produto.id = itens_carrinho.produto_id
            WHERE itens_carrinho.carrinho_id = @carrinho_id";

            totalCommand.Parameters.AddWithValue("@carrinho_id", carrinhoId);
            decimal total = Convert.ToDecimal(totalCommand.ExecuteScalar());

            var updateCarrinho = connection.CreateCommand();
            updateCarrinho.CommandText = @"
            UPDATE carrinho
            SET valorTotal = @total
            WHERE id = @id";

            updateCarrinho.Parameters.AddWithValue("@total", total);
            updateCarrinho.Parameters.AddWithValue("@id", carrinhoId);
            updateCarrinho.ExecuteNonQuery();

            return Ok();
        }

        [HttpGet]
        public IActionResult ObterItens()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
            SELECT
                produto.id,
                produto.nome,
                produto.imagem,
                itens_carrinho.quantidade,
                produto.preco
            FROM itens_carrinho
            INNER JOIN produto ON produto.id = itens_carrinho.produto_id";

            var itens = new List<object>();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var imagemBanco = reader.GetString(2);
                itens.Add(new
                {
                    produtoId = reader.GetInt32(0),
                    nome = reader.GetString(1),
                    imagem = imagemBanco.StartsWith("http") ? imagemBanco : "/images/" + imagemBanco,
                    quantidade = reader.GetInt32(3),
                    preco = reader.GetDecimal(4)
                });
            }

            return Json(itens);
        }

        [HttpPost]
        public IActionResult Alterar([FromBody] AlterarQuantidadeDTO? dto)
        {
            if (dto == null) return BadRequest("Corpo inválido.");
            if (dto.NovaQuantidade < 1) return BadRequest("Quantidade mínima é 1.");

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var itemCommand = connection.CreateCommand();
            itemCommand.CommandText = @"
                SELECT id, carrinho_id, produto_id 
                FROM itens_carrinho 
                WHERE produto_id = @produto_id";
            itemCommand.Parameters.AddWithValue("@produto_id", dto.ProdutoId);

            int itemId = 0;
            int carrinhoId = 0;

            using (var reader = itemCommand.ExecuteReader())
            {
                if (!reader.Read()) return NotFound("Item não encontrado no carrinho.");
                itemId = reader.GetInt32(0);
                carrinhoId = reader.GetInt32(1);
            }

            var estoqueCommand = connection.CreateCommand();
            estoqueCommand.CommandText = "SELECT estoque FROM produto WHERE id = @id";
            estoqueCommand.Parameters.AddWithValue("@id", dto.ProdutoId);
            var estoqueResult = estoqueCommand.ExecuteScalar();

            if (estoqueResult != null && estoqueResult != DBNull.Value)
            {
                int estoque = Convert.ToInt32(estoqueResult);

                if (estoque <= 0)
                    return BadRequest("Este produto acabou de esgotar no estoque.");

                if (dto.NovaQuantidade > estoque)
                    return BadRequest($"Estoque insuficiente. Disponível: {estoque}.");
            }

            var update = connection.CreateCommand();
            update.CommandText = "UPDATE itens_carrinho SET quantidade = @quantidade WHERE id = @id";
            update.Parameters.AddWithValue("@quantidade", dto.NovaQuantidade);
            update.Parameters.AddWithValue("@id", itemId);
            update.ExecuteNonQuery();

            var totalCommand = connection.CreateCommand();
            totalCommand.CommandText = @"
                SELECT SUM(itens_carrinho.quantidade * produto.preco)
                FROM itens_carrinho
                INNER JOIN produto ON produto.id = itens_carrinho.produto_id
                WHERE itens_carrinho.carrinho_id = @carrinho_id";
            totalCommand.Parameters.AddWithValue("@carrinho_id", carrinhoId);

            var totalResult = totalCommand.ExecuteScalar();
            decimal total = totalResult != DBNull.Value ? Convert.ToDecimal(totalResult) : 0;

            var updateCarrinho = connection.CreateCommand();
            updateCarrinho.CommandText = "UPDATE carrinho SET valorTotal = @total WHERE id = @id";
            updateCarrinho.Parameters.AddWithValue("@total", total);
            updateCarrinho.Parameters.AddWithValue("@id", carrinhoId);
            updateCarrinho.ExecuteNonQuery();

            return Ok(new { total });
        }

        [HttpPost]
        public IActionResult Excluir([FromBody] ExcluirItemDTO? dto)
        {
            if (dto == null) return BadRequest("Corpo inválido.");

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var itemCommand = connection.CreateCommand();
            itemCommand.CommandText = "SELECT carrinho_id FROM itens_carrinho WHERE produto_id = @produto_id LIMIT 1";
            itemCommand.Parameters.AddWithValue("@produto_id", dto.ProdutoId);

            var carrinhoResult = itemCommand.ExecuteScalar();
            if (carrinhoResult == null) return NotFound("Item não encontrado no carrinho.");
            int carrinhoId = Convert.ToInt32(carrinhoResult);

            var deleteCommand = connection.CreateCommand();
            deleteCommand.CommandText = "DELETE FROM itens_carrinho WHERE produto_id = @produto_id";
            deleteCommand.Parameters.AddWithValue("@produto_id", dto.ProdutoId);
            deleteCommand.ExecuteNonQuery();

            var totalCommand = connection.CreateCommand();
            totalCommand.CommandText = @"
                SELECT SUM(itens_carrinho.quantidade * produto.preco)
                FROM itens_carrinho
                INNER JOIN produto ON produto.id = itens_carrinho.produto_id
                WHERE itens_carrinho.carrinho_id = @carrinho_id";
            totalCommand.Parameters.AddWithValue("@carrinho_id", carrinhoId);

            var totalResult = totalCommand.ExecuteScalar();
            decimal total = totalResult != DBNull.Value && totalResult != null ? Convert.ToDecimal(totalResult) : 0;

            var updateCarrinho = connection.CreateCommand();
            updateCarrinho.CommandText = "UPDATE carrinho SET valorTotal = @total WHERE id = @id";
            updateCarrinho.Parameters.AddWithValue("@total", total);
            updateCarrinho.Parameters.AddWithValue("@id", carrinhoId);
            updateCarrinho.ExecuteNonQuery();

            return Ok(new { total });
        }

        [HttpPost]
        public IActionResult FinalizarCompra()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                var buscarItensCmd = connection.CreateCommand();
                buscarItensCmd.Transaction = transaction;
                buscarItensCmd.CommandText = "SELECT produto_id, quantidade FROM itens_carrinho";

                var itensParaAbater = new List<(int produtoId, int qtd)>();

                using (var reader = buscarItensCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        itensParaAbater.Add((reader.GetInt32(0), reader.GetInt32(1)));
                    }
                }

                if (itensParaAbater.Count == 0)
                {
                    return BadRequest("Seu carrinho está vazio.");
                }

                foreach (var item in itensParaAbater)
                {
                    var checarEstoqueCmd = connection.CreateCommand();
                    checarEstoqueCmd.Transaction = transaction;
                    checarEstoqueCmd.CommandText = "SELECT estoque, nome FROM produto WHERE id = @id";
                    checarEstoqueCmd.Parameters.AddWithValue("@id", item.produtoId);

                    using (var readerEstoque = checarEstoqueCmd.ExecuteReader())
                    {
                        if (readerEstoque.Read())
                        {
                            int estoqueAtual = readerEstoque.GetInt32(0);
                            string nomeProduto = readerEstoque.GetString(1);

                            if (item.qtd > estoqueAtual)
                            {
                                return BadRequest($"Estoque insuficiente para o item: {nomeProduto}. Disponível: {estoqueAtual}");
                            }
                        }
                    }

                    var updateEstoqueCmd = connection.CreateCommand();
                    updateEstoqueCmd.Transaction = transaction;
                    updateEstoqueCmd.CommandText = @"
                        UPDATE produto 
                        SET estoque = estoque - @quantidade 
                        WHERE id = @id";

                    updateEstoqueCmd.Parameters.AddWithValue("@quantidade", item.qtd);
                    updateEstoqueCmd.Parameters.AddWithValue("@id", item.produtoId);
                    updateEstoqueCmd.ExecuteNonQuery();
                }

                var limparItensCmd = connection.CreateCommand();
                limparItensCmd.Transaction = transaction;
                limparItensCmd.CommandText = "DELETE FROM itens_carrinho";
                limparItensCmd.ExecuteNonQuery();

                var limparCarrinhoCmd = connection.CreateCommand();
                limparCarrinhoCmd.Transaction = transaction;
                limparCarrinhoCmd.CommandText = "UPDATE carrinho SET valorTotal = 0";
                limparCarrinhoCmd.ExecuteNonQuery();

                transaction.Commit();
                return Ok(new { mensagem = "Compra realizada com sucesso!" });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return StatusCode(500, $"Erro interno ao processar compra: {ex.Message}");
            }
        }
    }

    public class AdicionarCarrinhoDTO
    {
        public int ProdutoId { get; set; }
    }
    public class AlterarQuantidadeDTO
    {
        public int ProdutoId { get; set; }
        public int NovaQuantidade { get; set; }
    }
    public class ExcluirItemDTO
    {
        public int ProdutoId { get; set; }
    }
}