using Microsoft.AspNetCore.Mvc;
using EcommerceMVC.Models;
using Microsoft.Data.Sqlite;
using System.Linq;

namespace EcommerceMVC.Controllers
{
    public class ProdutoController : Controller
    {
        string connectionString = "Data Source=database/database.db;";

        // GET: /Produto/Editar/1
        public IActionResult Editar(int id)
        {
            Produto produto = null;

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT id, nome, descricao, preco, imagem FROM Produto WHERE id = @id";
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
                            Imagem = reader.IsDBNull(4) ? "" : reader.GetString(4)
                        };
                    }
                }
            }

            if (produto == null)
                return NotFound();

            return View(produto);
        }

        // POST: salvar edição no banco
        [HttpPost]
        public IActionResult Editar(Produto produto)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Produto 
                    SET nome = @nome, 
                        descricao = @descricao, 
                        preco = @preco, 
                        imagem = @imagem 
                    WHERE id = @id";

                command.Parameters.AddWithValue("@id", produto.Id);
                command.Parameters.AddWithValue("@nome", produto.Nome);
                command.Parameters.AddWithValue("@descricao", produto.Descricao ?? "");
                command.Parameters.AddWithValue("@preco", produto.Preco);
                command.Parameters.AddWithValue("@imagem", produto.Imagem ?? "");

                command.ExecuteNonQuery();
            }

            return RedirectToAction("Index", "Home");
        }
    }
}