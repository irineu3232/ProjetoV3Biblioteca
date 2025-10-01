using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ProjetoBiblioteca.Data;
using ProjetoBiblioteca.Models;
using System.Data;

namespace ProjetoBiblioteca.Controllers
{
    public class EmprestimoController : Controller
    {
        private readonly Database db = new Database();
        //Private const string CART_KEY = "Carrinho";

        //------------------------ VITRINE ------------------------------//
        [HttpGet]
        public IActionResult Vitrine(string? q)
        {
            var itens = new List<Livros>();
            var titulos = new List<string>();

            using var conn = db.GetConnection();

            // 1) itens filtrados para exibir na grade
            using (var cmd = new MySqlCommand("sp_vitrine_buscar", conn) { CommandType = CommandType.StoredProcedure})
            {
                cmd.Parameters.AddWithValue("p_q", q ?? "");
                using var rd = cmd.ExecuteReader();
                while(rd.Read())
                {
                    itens.Add(new Livros
                    {
                        Id = rd.GetInt32("id"),
                        Titulo = rd.GetString("titulo"),
                        Capa = rd["capa_arquivo"] as string
                    });
                }
            }

            // 2) TODOS OS TITULOS ( para popular o datalist)
            // Você pode criar uma sp só de titulos. aqui reusamos com p_q vazio
            using (var cmdAll = new MySqlCommand("sp_vitrine_buscar", conn) { CommandType = CommandType.StoredProcedure})
            {
                cmdAll.Parameters.AddWithValue("p_q", "");
                using var rd2 = cmdAll.ExecuteReader();
                while(rd2.Read())
                {
                    // Evita duplicados, se necessário
                    var titulo = rd2.GetString("titulo");
                    if (!string.IsNullOrWhiteSpace(titulo) && !titulos.Contains(titulo))
                        titulos.Add(titulo);
                }
            }

            ViewBag.q = q ?? "";
            ViewBag.Titulos = titulos;

            return View(itens);
        }
    }
}
