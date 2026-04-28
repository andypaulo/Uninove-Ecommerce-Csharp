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

        // =========================
        // 🔹 LISTA (Gerenciamento)
        // =========================
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
                    command.CommandText = "SELECT id, nome, descricao, preco, estoque, imagem FROM produto";

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
                                Estoque = reader.GetInt32(4),
                                Imagem = reader.IsDBNull(5) ? "sem-foto.jpg" : reader.GetString(5)
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

        // =========================
        // 🔹 EDITAR (GET)
        // =========================
        public IActionResult Editar(int id)
        {
            Produto produto = null;

            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT id, nome, descricao, preco, imagem, estoque FROM produto WHERE id = @id";
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
                            Estoque = reader.IsDBNull(5) ? 0 : reader.GetInt32(5)
                        };
                    }
                }
            }

            if (produto == null)
                return NotFound();

            return View(produto);
        }

        // =========================
        // 🔹 EDITAR (POST)
        // =========================
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
                command.Parameters.AddWithValue("@estoque", produto.Estoque);

                command.ExecuteNonQuery();
            }

            return RedirectToAction("Index", "Home");
        }

        // =========================
        // 🔹 DELETE (GET)
        // =========================
        [HttpGet]
        public ActionResult Delete(int id)
        {
            if (id <= 0) return BadRequest();

            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT id, nome, descricao, preco, estoque, imagem FROM produto WHERE id = $id";
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

        // =========================
        // 🔹 DELETE (POST)
        // =========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                using var selectCmd = connection.CreateCommand();
                selectCmd.CommandText = "SELECT nome, estoque FROM produto WHERE id = $id";
                selectCmd.Parameters.AddWithValue("$id", id);

                string nomeProduto = "";
                using (var reader = selectCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        nomeProduto = reader.GetString(0);
                        if (reader.GetInt32(1) > 0)
                        {
                            TempData["ErroExclusao"] = "Estoque maior que zero não permitido.";
                            return RedirectToAction(nameof(Gerenciamento));
                        }
                    }
                }

                using var deleteCommand = connection.CreateCommand();
                deleteCommand.CommandText = "DELETE FROM produto WHERE id = $id";
                deleteCommand.Parameters.AddWithValue("$id", id);

                var rows = deleteCommand.ExecuteNonQuery();

                if (rows > 0)
                    TempData["Sucesso"] = $"Produto '{nomeProduto}' excluído com sucesso.";

                return RedirectToAction(nameof(Gerenciamento));
            }
            catch
            {
                return RedirectToAction(nameof(Gerenciamento));
            }
        }

        // =========================
        // 🔹 AUXILIARES
        // =========================
        private static bool PodeExcluirProduto(Produto produto, out string motivo)
        {
            if (produto.Estoque > 0)
            {
                motivo = "Estoque maior que zero não permitido.";
                return false;
            }
            motivo = string.Empty;
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
                Imagem = reader.IsDBNull(5) ? "sem-foto.jpg" : reader.GetString(5)
            };
        }

        // =========================
        // 🔹 OUTROS (mantido)
        // =========================
        public ActionResult Details(int id) => View();
        public ActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection c)
            => RedirectToAction(nameof(Gerenciamento));

        public ActionResult Edit(int id) => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection c)
            => RedirectToAction(nameof(Gerenciamento));
    }
}