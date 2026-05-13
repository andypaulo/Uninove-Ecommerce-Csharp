using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EcommerceMVC.Models;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace EcommerceMVC.Controllers
{
    public class ProdutoController : Controller
    {
        private const string ConnectionString = "Data Source=database/database.db;";

        public IActionResult Editar(int id)
        {
            Produto? produto = null;

            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                // ✅ CORRIGIDO (agora inclui estoque)
                command.CommandText = "SELECT id, nome, descricao, preco, imagem, estoque, categoria FROM produto WHERE id = @id";
                command.Parameters.AddWithValue("@id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        produto = new Produto
                        {
                            Id = reader.GetInt32(0),
                            Nome = reader.GetString(1),
                            Descricao = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            Preco = reader.GetDecimal(3),
                            Imagem = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            Estoque = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                            Categoria = reader.IsDBNull(6) ? "" : reader.GetString(6)
                        };
                    }
                }
            }

            if (produto == null)
                return NotFound();

            return View(produto);
        }

        [HttpPost]
        public IActionResult Editar(Produto produto)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = @"
                    UPDATE produto 
                    SET nome = @nome, 
                        descricao = @descricao, 
                        preco = @preco, 
                        imagem = @imagem,
                        estoque = @estoque
                    WHERE id = @id";

                command.Parameters.AddWithValue("@id", produto.Id);
                command.Parameters.AddWithValue("@nome", produto.Nome);
                command.Parameters.AddWithValue("@descricao", produto.Descricao ?? "");
                command.Parameters.AddWithValue("@preco", produto.Preco);
                command.Parameters.AddWithValue("@imagem", produto.Imagem ?? "");
                command.Parameters.AddWithValue("@estoque", produto.Estoque); // 

                command.ExecuteNonQuery();
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Gerenciamento()
        {
            var listaProdutos = new List<Produto>();
            SQLitePCL.Batteries_V2.Init();

            try
            {
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT id, nome, descricao, preco, estoque, imagem, categoria FROM produto";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            listaProdutos.Add(new Produto
                            {
                                Id = reader.GetInt32(0),
                                Nome = reader.GetString(1),
                                Descricao = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Preco = reader.GetDecimal(3),
                                Estoque = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                Imagem = reader.IsDBNull(5) ? "sem-foto.jpg" : reader.GetString(5),
                                Categoria = reader.IsDBNull(6) ? "" : reader.GetString(6)
                            });
                        }
                    }
                }
                return View(listaProdutos);
            }
            catch
            {
                return BadRequest("Erro ao carregar banco de dados.");
            }
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            if (id <= 0) return BadRequest();

            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT id, nome, descricao, preco, estoque, imagem, categoria FROM produto WHERE id = $id";
                command.Parameters.AddWithValue("$id", id);

                using var reader = command.ExecuteReader();
                if (!reader.Read()) return NotFound();

                var produto = MapProduto(reader);

                if (!PodeExcluirProduto(produto, out var motivo))
                {
                    TempData["ErroExclusao"] = motivo;
                    return RedirectToAction(nameof(Gerenciamento));
                }

                return View(produto);
            }
            catch
            {
                return RedirectToAction(nameof(Gerenciamento));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                using var deleteCommand = connection.CreateCommand();
                deleteCommand.CommandText = "DELETE FROM produto WHERE id = $id";
                deleteCommand.Parameters.AddWithValue("$id", id);

                deleteCommand.ExecuteNonQuery();

                return RedirectToAction(nameof(Gerenciamento));
            }
            catch
            {
                return RedirectToAction(nameof(Gerenciamento));
            }
        }

        private static bool PodeExcluirProduto(Produto produto, out string motivo)
        {
            if (produto.Estoque > 0)
            {
                motivo = "Estoque maior que zero não permitido.";
                return false;
            }
            motivo = "";
            return true;
        }

        private static Produto MapProduto(SqliteDataReader reader)
        {
            return new Produto
            {
                Id = reader.GetInt32(0),
                Nome = reader.GetString(1),
                Descricao = reader.IsDBNull(2) ? "" : reader.GetString(2),
                Preco = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3),
                Estoque = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                Imagem = reader.IsDBNull(5) ? "sem-foto.jpg" : reader.GetString(5),
                Categoria = reader.IsDBNull(6) ? "" : reader.GetString(6)
            };
        }
    
[HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Produto produto)
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);

                connection.Open();

                var command = connection.CreateCommand();

                command.CommandText = @"INSERT INTO produto
                (nome, descricao, preco, estoque, imagem, categoria)
                VALUES
                (@nome, @descricao, @preco, @estoque, @imagem, @categoria)";

                command.Parameters.AddWithValue("@nome", produto.Nome);
                command.Parameters.AddWithValue("@descricao", produto.Descricao ?? "");
                command.Parameters.AddWithValue("@preco", produto.Preco);
                command.Parameters.AddWithValue("@estoque", produto.Estoque);
                command.Parameters.AddWithValue("@imagem", produto.Imagem ?? "");
                command.Parameters.AddWithValue("@categoria", produto.Categoria);

                command.ExecuteNonQuery();

                return RedirectToAction(nameof(Gerenciamento));
            }
            catch
            {
                return BadRequest("Erro ao cadastrar produto.");
            }
        }
    }
}