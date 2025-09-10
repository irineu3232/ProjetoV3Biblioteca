using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ProjetoBiblioteca.Data;
using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Controllers
{
    public class GeneroController : Controller
    {
        private readonly Database db = new Database();
        public IActionResult Index()
        {
            List<Genero> genero = new List<Genero>();
            using (var conn = db.GetConnection())
            {
                var sql = "select distinct id,nome,criado_em from Editoras order by nome";
                var cmd = new MySqlCommand(sql, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    genero.Add(new Genero
                    {
                        Nome = reader.GetString("nome"),
                        Id = reader.GetInt32("id"),
                        Criado_Em = reader.GetDateTime("criado_em"),
                    });
                }
            }
            return View(genero);
        }

        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Criar(Genero vm)
        {
            using var conn = db.GetConnection();

            using var cmd = new MySqlCommand("sp_genero_criar", conn);

            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("p_nome", vm.Nome);
            cmd.ExecuteNonQuery();
            return RedirectToAction("Criar");
        }
    }
}
