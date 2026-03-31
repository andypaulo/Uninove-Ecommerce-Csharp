using EcommerceMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.Data.Sqlite;

namespace EcommerceMVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
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
                                Estoque = reader.GetInt32(0),
                                Imagem = reader.IsDBNull(4) ? "sem-foto.jpg" : reader.GetString(4)
                            });
                        }
                    }
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
