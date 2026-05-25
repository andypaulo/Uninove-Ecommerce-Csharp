using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace EcommerceMVC.Controllers
{
    public class CarrinhoController : Controller
    {
        private const string ConnectionString =
            "Data Source=database/database.db;";

        [HttpPost]
        public IActionResult Adicionar([FromBody] AdicionarCarrinhoDTO dto)
        {
            using var connection =
                new SqliteConnection(ConnectionString);

            connection.Open();

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

                corrigir.CommandText =
                "ALTER TABLE carrinho ADD COLUMN valorTotal REAL DEFAULT 0";

                corrigir.ExecuteNonQuery();
            }
            catch
            {

            }

            var produtoCommand = connection.CreateCommand();

            produtoCommand.CommandText =
            "SELECT id, preco FROM produto WHERE id = @id";

            produtoCommand.Parameters.AddWithValue(
                "@id",
                dto.ProdutoId
            );

            int produtoId = 0;
            decimal preco = 0;

            using (var reader =
                produtoCommand.ExecuteReader())
            {
                if (!reader.Read())
                    return NotFound();

                produtoId = reader.GetInt32(0);
                preco = reader.GetDecimal(1);
            }

            int carrinhoId = 0;

            var carrinhoCommand =
                connection.CreateCommand();

            carrinhoCommand.CommandText =
                "SELECT id FROM carrinho LIMIT 1";

            var result =
                carrinhoCommand.ExecuteScalar();

            if (result == null)
            {
                var insertCarrinho =
                    connection.CreateCommand();

                insertCarrinho.CommandText =
                    "INSERT INTO carrinho (valorTotal) VALUES (0)";

                insertCarrinho.ExecuteNonQuery();

                var lastIdCommand =
                    connection.CreateCommand();

                lastIdCommand.CommandText =
                    "SELECT last_insert_rowid()";

                carrinhoId =
                    Convert.ToInt32(
                        lastIdCommand.ExecuteScalar()
                    );
            }
            else
            {
                carrinhoId =
                    Convert.ToInt32(result);
            }

            var itemCommand =
                connection.CreateCommand();

            itemCommand.CommandText = @"
            SELECT id, quantidade
            FROM itens_carrinho
            WHERE carrinho_id = @carrinho_id
            AND produto_id = @produto_id";

            itemCommand.Parameters.AddWithValue(
                "@carrinho_id",
                carrinhoId
            );

            itemCommand.Parameters.AddWithValue(
                "@produto_id",
                produtoId
            );

            int itemId = 0;
            int quantidade = 0;

            using (var reader =
                itemCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    itemId = reader.GetInt32(0);
                    quantidade = reader.GetInt32(1);
                }
            }

            if (itemId > 0)
            {
                var update =
                    connection.CreateCommand();

                update.CommandText = @"
                UPDATE itens_carrinho
                SET quantidade = @quantidade
                WHERE id = @id";

                update.Parameters.AddWithValue(
                    "@quantidade",
                    quantidade + 1
                );

                update.Parameters.AddWithValue(
                    "@id",
                    itemId
                );

                update.ExecuteNonQuery();
            }
            else
            {
                var insert =
                    connection.CreateCommand();

                insert.CommandText = @"
                INSERT INTO itens_carrinho
                (carrinho_id, produto_id, quantidade)
                VALUES
                (@carrinho_id, @produto_id, @quantidade)";

                insert.Parameters.AddWithValue(
                    "@carrinho_id",
                    carrinhoId
                );

                insert.Parameters.AddWithValue(
                    "@produto_id",
                    produtoId
                );

                insert.Parameters.AddWithValue(
                    "@quantidade",
                    1
                );

                insert.ExecuteNonQuery();
            }

            var totalCommand =
                connection.CreateCommand();

            totalCommand.CommandText = @"
            SELECT SUM(itens_carrinho.quantidade * produto.preco)
            FROM itens_carrinho
            INNER JOIN produto
            ON produto.id = itens_carrinho.produto_id
            WHERE itens_carrinho.carrinho_id = @carrinho_id";

            totalCommand.Parameters.AddWithValue(
                "@carrinho_id",
                carrinhoId
            );

            decimal total =
                Convert.ToDecimal(
                    totalCommand.ExecuteScalar()
                );

            var updateCarrinho =
                connection.CreateCommand();

            updateCarrinho.CommandText = @"
            UPDATE carrinho
            SET valorTotal = @total
            WHERE id = @id";

            updateCarrinho.Parameters.AddWithValue(
                "@total",
                total
            );

            updateCarrinho.Parameters.AddWithValue(
                "@id",
                carrinhoId
            );

            updateCarrinho.ExecuteNonQuery();

            return Ok();
        }

        [HttpGet]
        public IActionResult ObterItens()
        {
            using var connection =
                new SqliteConnection(ConnectionString);

            connection.Open();

            var command =
                connection.CreateCommand();

            command.CommandText = @"
            SELECT
                produto.id,
                produto.nome,
                produto.imagem,
                itens_carrinho.quantidade,
                produto.preco
            FROM itens_carrinho
            INNER JOIN produto
            ON produto.id = itens_carrinho.produto_id";

            var itens = new List<object>();

            using var reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                var imagemBanco =
                    reader.GetString(2);

                itens.Add(new
                {
                    produtoId = reader.GetInt32(0),
                    nome = reader.GetString(1),
                    imagem = imagemBanco.StartsWith("http") ? imagemBanco: "/images/" + imagemBanco,
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

}
