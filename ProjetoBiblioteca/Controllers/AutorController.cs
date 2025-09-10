using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Security;
using ProjetoBiblioteca.Data;
using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Controllers
{
    public class AutorController : Controller
    {
        private readonly Database db = new Database();
        public IActionResult Index()
        {
            List<Autores> autor = new List<Autores>();
            using (var conn = db.GetConnection())
            {
                var sql = "select  id,nome,criado_em from Autores order by nome";
                var cmd = new MySqlCommand(sql, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    autor.Add(new Autores
                    {
                        Nome = reader.GetString("nome"),
                        Id = reader.GetInt32("id"),
                        Criado_Em = reader.GetDateTime("criado_em"),
                    });
                }
            }
            return View(autor);
        }

        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Criar(Autores vm)
        {
            using var conn = db.GetConnection();

            using var cmd = new MySqlCommand("sp_autor_criar", conn);

            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("p_nome", vm.Nome);
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }
    }
}
