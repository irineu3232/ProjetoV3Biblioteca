using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ProjetoBiblioteca.Data;
using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Controllers
{
    public class EdicaoController : Controller
    {
        private readonly Database db = new Database();
        public IActionResult Index()
        {
            List<Editoras> editor = new List<Editoras>();
            using (var conn = db.GetConnection())
            {
                var sql = "select distinct id,nome,criado_em from Editoras order by nome";
                var cmd = new MySqlCommand(sql, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    editor.Add(new Editoras
                    {
                        Nome = reader.GetString("nome"),
                        Id = reader.GetInt32("id"),
                        Criado_Em = reader.GetDateTime("criado_em"),
                    });
                }
            }
            return View(editor);
        }

        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Criar(Editoras vm)
        {
            using var conn = db.GetConnection();

            using var cmd = new MySqlCommand("sp_editora_criar", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("p_nome", vm.Nome);
            cmd.ExecuteNonQuery();
            return RedirectToAction("Index");
        }
    }
}
