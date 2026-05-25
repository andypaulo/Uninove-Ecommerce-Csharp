using EcommerceMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace EcommerceMVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index(string? categoria, string? pesquisa, int pagina = 1)
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
                    string query = @"
                        SELECT id, nome, descricao, preco, estoque, imagem, categoria
                        FROM Produto
                        WHERE 1=1";

                    if (!string.IsNullOrEmpty(categoria))
                    {
                        query += " AND categoria = @categoria";
                        command.Parameters.AddWithValue("@categoria", categoria);
                    }

                    if (!string.IsNullOrEmpty(pesquisa))
                    {
                        query += " AND nome LIKE @pesquisa";
                        command.Parameters.AddWithValue("@pesquisa", "%" + pesquisa + "%");
                    }

                    query += " LIMIT @limite OFFSET @offset";
                    command.Parameters.AddWithValue("@limite", limite);
                    command.Parameters.AddWithValue("@offset", offset);

                    command.CommandText = query;

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
                    string countQuery = "SELECT COUNT(*) FROM Produto WHERE 1=1";

                    if (!string.IsNullOrEmpty(categoria))
                    {
                        countQuery += " AND categoria = @categoria";
                        totalCommand.Parameters.AddWithValue("@categoria", categoria);
                    }

                    if (!string.IsNullOrEmpty(pesquisa))
                    {
                        countQuery += " AND nome LIKE @pesquisa";
                        totalCommand.Parameters.AddWithValue("@pesquisa", "%" + pesquisa + "%");
                    }

                    totalCommand.CommandText = countQuery;
                    int totalProdutos = Convert.ToInt32(totalCommand.ExecuteScalar());

                    // Garante ao menos 1 página se o resultado for vazio
                    int totalPaginas = (int)Math.Ceiling((double)totalProdutos / limite);
                    if (totalPaginas == 0) totalPaginas = 1;

                    // Passa os estados necessários para manter os filtros vivos na View
                    ViewBag.PaginaAtual = pagina;
                    ViewBag.TotalPaginas = totalPaginas;
                    ViewBag.Categoria = categoria;
                    ViewBag.Pesquisa = pesquisa;
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