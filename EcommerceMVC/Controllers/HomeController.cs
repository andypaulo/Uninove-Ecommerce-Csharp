using EcommerceMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace EcommerceMVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index(string? categoria, int pagina = 1)
        {
            var listaProdutos = new List<Produto>();

            SQLitePCL.Batteries_V2.Init();
            string connectionString = "Data Source=database/database.db;";

            int limite = 21;
            int offset = (pagina - 1) * limite;

            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    if (!string.IsNullOrEmpty(categoria))
                    {
                        command.CommandText = @"
                         SELECT id, nome, descricao, preco, estoque, imagem, categoria
                         FROM Produto
                         WHERE categoria = @categoria
                         LIMIT @limite
                         OFFSET @offset";

                        command.Parameters.AddWithValue("@categoria", categoria);
                    }
                    else
                    {
                        command.CommandText = @"
                         SELECT id, nome, descricao, preco, estoque, imagem, categoria
                         FROM Produto
                         LIMIT @limite
                         OFFSET @offset";
                    }

                    command.Parameters.AddWithValue("@limite", limite);
                    command.Parameters.AddWithValue("@offset", offset);

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
                                Imagem = reader.IsDBNull(5) ? "sem-imagem.jpg" : reader.GetString(5),
                                Categoria = reader.IsDBNull(6) ? "" : reader.GetString(6)
                            });
                        }
                    }

                    var totalCommand = connection.CreateCommand();

                    if (!string.IsNullOrEmpty(categoria))
                    {
                        totalCommand.CommandText = @"
                        SELECT COUNT(*)
                        FROM Produto
                        WHERE categoria = @categoria";

                        totalCommand.Parameters.AddWithValue("@categoria", categoria);
                    }
                    else
                    {
                        totalCommand.CommandText =
                            "SELECT COUNT(*) FROM Produto";
                    }

                    int totalProdutos =
                        Convert.ToInt32(
                            totalCommand.ExecuteScalar()
                        );

                    int totalPaginas =
                        (int)Math.Ceiling(
                            (double)totalProdutos / limite
                        );

                    ViewBag.PaginaAtual = pagina;
                    ViewBag.TotalPaginas = totalPaginas;
                    ViewBag.Categoria = categoria;
                }

                return View(listaProdutos);
            }
            catch (Exception ex)
            {
                ViewBag.DbState = "Erro: " + ex.Message;
            }

            ViewData["Message"] = "Projeto de E-commerce iniciado!";
            return View(new List<Produto>());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            Produto? produto = null;
            string connectionString = "Data Source=database/database.db;";

            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT id, nome, descricao, preco, estoque, imagem FROM Produto WHERE id = $id";
                    command.Parameters.AddWithValue("$id", id);

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
                                Estoque = reader.GetInt32(4),
                                Imagem = reader.IsDBNull(5) ? "sem-foto.jpg" : reader.GetString(5)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.DbState = "Erro: " + ex.Message;
                return NotFound();
            }

            if (produto == null) return NotFound();
            return View(produto);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        public IActionResult Gerenciamento()
        {
            var listaProdutos = new List<Produto>();
            SQLitePCL.Batteries_V2.Init();
            string connectionString = "Data Source=database/database.db;";

            try
            {
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT id, nome, descricao, preco, estoque, imagem FROM Produto";

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
            catch (Exception ex)
            {
                ViewBag.DbState = "Erro: " + ex.Message;
                return View(new List<Produto>());
            }
        }
    }
}