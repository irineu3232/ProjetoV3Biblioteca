using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.Crmf;
using ProjetoBiblioteca.Data;
using ProjetoBiblioteca.Models;
using System.Data;

namespace ProjetoBiblioteca.Controllers
{
    public class LeitorController : Controller
    {
        private readonly Database db = new Database();

        public IActionResult Index()
        {
            var lista = new List<Leitor>();
            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_leitor_listar", conn) { CommandType = System.Data.CommandType.StoredProcedure };
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(new Leitor
                {
                    id = rd.GetInt32("id_leitor"),
                    nome = rd.GetString("nomeLeitor"),
                    foto = rd["foto_leitor"] == DBNull.Value ? null : (string?)rd.GetString("foto_leitor")
                });
            }
            return View(lista);
        }

        [HttpGet]
        public IActionResult Criar()
        {

        
            return View();
        }


        [HttpPost]
        public IActionResult Criar(Leitor leitor, IFormFile foto)
        {

            string? relPatch = null;

            if(foto != null && foto.Length >0)
            {
                var ext = Path.GetExtension(foto.FileName);
                //(Opicional) validar extensão

                var fileName = $"{Guid.NewGuid()}{ext}";
                var savedir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fotos");
                Directory.CreateDirectory(savedir);
                var asbPath = Path.Combine(savedir, fileName);
                using var fs = new FileStream(asbPath, FileMode.Create);
                foto.CopyTo(fs);
                relPatch = Path.Combine("fotos", fileName).Replace("\\", "/");
            }


            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_leitor_criar", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("p_nome", leitor.nome);
            cmd.Parameters.AddWithValue("p_foto", (object?)relPatch ?? DBNull.Value);
            cmd.ExecuteNonQuery();
           return View();
        }

        [HttpGet]
        public IActionResult Editar(Leitor vm)
        {
            using var conn = db.GetConnection();

            Leitor? leitor = null;
            using (var cmd = new MySqlCommand("sp_leitor_obter", conn) { CommandType = System.Data.CommandType.StoredProcedure })
            {
                cmd.Parameters.AddWithValue("l_id", vm.id);
                using var rd = cmd.ExecuteReader();
                if (rd.Read())
                {
                    leitor = new Leitor
                    {
                        id = rd.GetInt32("id_leitor"),
                        nome = rd.GetString("nomeLeitor"),
                        foto = rd["foto_leitor"] == DBNull.Value ? null : (string?)rd.GetString("foto_leitor")

                    };
                }

            }

            return View(leitor);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Editar(Leitor vm, IFormFile? foto)
        {
            string? relPath = null;

            if (foto != null && foto.Length > 0)
            {
                var ext = Path.GetExtension(foto.FileName);
                //(opicional) validar extensão

                var fileName = $"{Guid.NewGuid()}{ext}";
                var savedir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "fotos");
                Directory.CreateDirectory(savedir);
                var absPath = Path.Combine(savedir, fileName);
                using var fs = new FileStream(absPath, FileMode.Create);
                foto.CopyTo(fs);
                relPath = Path.Combine("capas", fileName).Replace("\\", "/");
            }


            if (vm.id <= 0) return NotFound();

            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_leitor_editar", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("l_id", vm.id);
            cmd.Parameters.AddWithValue("l_nome", vm.nome);
            cmd.Parameters.AddWithValue("l_foto", relPath);
            cmd.ExecuteNonQuery();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Excluir(int id)
        {
            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_leitor_excluir", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("l_id", id);
            cmd.ExecuteNonQuery();
            return RedirectToAction(nameof(Index));
        }
    }

   
}
