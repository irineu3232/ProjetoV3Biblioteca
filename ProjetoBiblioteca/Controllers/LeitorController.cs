using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
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
            using var cmd = new MySqlCommand("sp_livro_listar", conn) { CommandType = System.Data.CommandType.StoredProcedure };
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(new Leitor
                {
                    id = rd.GetInt32("id"),
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


        [HttpGet]
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





    }
}
