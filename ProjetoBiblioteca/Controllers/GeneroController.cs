using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using ProjetoBiblioteca.Autenticacao;
using ProjetoBiblioteca.Data;
using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Controllers
{
    [SessionAuthorize]
    public class GeneroController : Controller
    {
        private readonly Database db = new Database();
        public IActionResult Index()
        {
            List<Genero> genero = new List<Genero>();
            using (var conn = db.GetConnection())
            {
                var sql = "select distinct id,nome,criado_em from Generos order by nome";
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

        [HttpGet]
        public IActionResult Editar(int Id)
        {
            using var conn = db.GetConnection();

            Genero? genero = null;
            using (var cmd = new MySqlCommand("sp_select_genero", conn) { CommandType = System.Data.CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("id_gen", Id);
                using var rd = cmd.ExecuteReader();
                if (rd.Read())
                {
                    genero = new Genero
                    {
                        Id = rd.GetInt32("id"),
                        Nome = rd.GetString("nome"),
                       
                    };
                }

            }
           

            return View(genero);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Editar(Genero genero)
        {
            if (genero.Id <= 0) return NotFound();
            
            using var conn2 = db.GetConnection();
            using var cmd = new MySqlCommand("sp_editar_genero", conn2) { CommandType = System.Data.CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("id_gen", genero.Id);
            cmd.Parameters.AddWithValue("nome_gen", genero.Nome);
            cmd.ExecuteNonQuery();

            TempData["Ok"] = "Livro atualizada!";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Excluir(int Id)
        {
            using var conn = db.GetConnection();

            using var cmd = new MySqlCommand("sp_deletar_genero", conn) { CommandType = System.Data.CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("id_gen", Id);
            cmd.ExecuteNonQuery();

            return RedirectToAction(nameof(Index));
        }


    }      
    
}
