using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ProjetoBiblioteca.Autenticacao;
using ProjetoBiblioteca.Data;
using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Controllers
{
    [SessionAuthorize]
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



        [HttpGet]
        public IActionResult Editar(int Id)
        {
            using var conn = db.GetConnection();

            Editoras? editor = null;
            using (var cmd = new MySqlCommand("sp_select_editora", conn) { CommandType = System.Data.CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("id_edi", Id);
                using var rd = cmd.ExecuteReader();
                if (rd.Read())
                {
                    editor = new Editoras
                    {
                        Id = rd.GetInt32("id"),
                        Nome = rd.GetString("nome"),

                    };
                }

            }


            return View(editor);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Editar(Editoras editor)
        {
            if (editor.Id <= 0) return NotFound();

            using var conn2 = db.GetConnection();
            using var cmd = new MySqlCommand("sp_editar_editora", conn2) { CommandType = System.Data.CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("id_edi", editor.Id);
            cmd.Parameters.AddWithValue("nome_edi", editor.Nome);
            cmd.ExecuteNonQuery();

            TempData["Ok"] = "Livro atualizada!";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Excluir(int Id)
        {
            using var conn = db.GetConnection();

            using var cmd = new MySqlCommand("sp_deletar_editora", conn) { CommandType = System.Data.CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("id_edi", Id);
            cmd.ExecuteNonQuery();

            return RedirectToAction(nameof(Index));
        }



    }
}
