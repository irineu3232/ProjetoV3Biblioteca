using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Security;
using ProjetoBiblioteca.Autenticacao;
using ProjetoBiblioteca.Data;
using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Controllers
{
    [SessionAuthorize]
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



        [HttpGet]
        public IActionResult Editar(int Id)
        {
            using var conn = db.GetConnection();

            Autores autor = null;
            using (var cmd = new MySqlCommand("sp_select_autor", conn) { CommandType = System.Data.CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("id_aut", Id);
                using var rd = cmd.ExecuteReader();
                if (rd.Read())
                {
                    autor = new Autores
                    {
                        Id = rd.GetInt32("id"),
                        Nome = rd.GetString("nome"),

                    };
                }

            }


            return View(autor);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Editar(Autores autor)
        {
            if (autor.Id <= 0) return NotFound();

            using var conn2 = db.GetConnection();
            using var cmd = new MySqlCommand("sp_editar_autor", conn2) { CommandType = System.Data.CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("id_aut", autor.Id);
            cmd.Parameters.AddWithValue("nome_aut", autor.Nome);
            cmd.ExecuteNonQuery();

            TempData["Ok"] = "Livro atualizada!";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Excluir(int Id)
        {
            using var conn = db.GetConnection();

            using var cmd = new MySqlCommand("sp_deletar_autor", conn) { CommandType = System.Data.CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("id_aut", Id);
            cmd.ExecuteNonQuery();

            return RedirectToAction(nameof(Index));
        }

    }
}
