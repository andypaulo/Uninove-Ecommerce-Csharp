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

            // CRIAR TABELA CARRINHO
            var criarCarrinho = connection.CreateCommand();

            criarCarrinho.CommandText = @"
            CREATE TABLE IF NOT EXISTS carrinho (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                valorTotal REAL DEFAULT 0
            )";

            criarCarrinho.ExecuteNonQuery();

            // CRIAR TABELA ITENS_CARRINHO
            var criarItens = connection.CreateCommand();

            criarItens.CommandText = @"
            CREATE TABLE IF NOT EXISTS itens_carrinho (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                carrinhoId INTEGER,
                produtoId INTEGER,
                quantidade INTEGER,
                precoUnitario REAL
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
            WHERE carrinhoId = @carrinhoId
            AND produtoId = @produtoId";

            itemCommand.Parameters.AddWithValue(
                "@carrinhoId",
                carrinhoId
            );

            itemCommand.Parameters.AddWithValue(
                "@produtoId",
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
                (carrinhoId, produtoId, quantidade, precoUnitario)
                VALUES
                (@carrinhoId, @produtoId, @quantidade, @preco)";

                insert.Parameters.AddWithValue(
                    "@carrinhoId",
                    carrinhoId
                );

                insert.Parameters.AddWithValue(
                    "@produtoId",
                    produtoId
                );

                insert.Parameters.AddWithValue(
                    "@quantidade",
                    1
                );

                insert.Parameters.AddWithValue(
                    "@preco",
                    preco
                );

                insert.ExecuteNonQuery();
            }

            var totalCommand =
                connection.CreateCommand();

            totalCommand.CommandText = @"
            SELECT SUM(quantidade * precoUnitario)
            FROM itens_carrinho
            WHERE carrinhoId = @carrinhoId";

            totalCommand.Parameters.AddWithValue(
                "@carrinhoId",
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
                produto.nome,
                produto.imagem,
                itens_carrinho.quantidade,
                itens_carrinho.precoUnitario
            FROM itens_carrinho
            INNER JOIN produto
            ON produto.id = itens_carrinho.produtoId";

            var itens = new List<object>();

            using var reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                var imagemBanco =
                    reader.GetString(1);

                itens.Add(new
                {
                    nome = reader.GetString(0),

                    imagem =
                        imagemBanco.StartsWith("http")
                        ? imagemBanco
                        : "/images/" + imagemBanco,

                    quantidade =
                        reader.GetInt32(2),

                    preco =
                        reader.GetDecimal(3)
                });
            }

            return Json(itens);
        }
    }

    public class AdicionarCarrinhoDTO
    {
        public int ProdutoId { get; set; }
    }
}