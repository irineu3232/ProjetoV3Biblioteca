using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using ProjetoBiblioteca.Autenticacao;
using ProjetoBiblioteca.Data;
using ProjetoBiblioteca.Models;
using System.Data;

namespace ProjetoBiblioteca.Controllers
{
    [SessionAuthorize]
    public class LivroController : Controller
    {
        private readonly Database db = new Database();

        [HttpGet]
        public IActionResult Index()
        {
            var lista = new List<Livros>();
            using var conn = db.GetConnection();
            using var cmd = new MySqlCommand("sp_livro_listar", conn) { CommandType = System.Data.CommandType.StoredProcedure };
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                lista.Add(new Livros
                {
                    Id = rd.GetInt32("id"),
                    Titulo = rd.GetString("titulo"),
                    AutorId = rd["autorId"] == DBNull.Value ? null : (int?)rd.GetInt32("autorId"),
                    EditoraId = rd["editoraId"] == DBNull.Value ? null : (int?)rd.GetInt32("editoraId"),
                    GeneroId = rd["generoId"] == DBNull.Value ? null : (int?)rd.GetInt32("generoId"),
                    Autor = rd["autor_nome"] as string,
                    Editor = rd["editora_nome"] as string,
                    Genero = rd["genero_nome"] as string,
                    Ano = rd["ano"] == DBNull.Value ? null : (short?)rd.GetInt16("ano"),
                    Isbn = rd["isbn"] as string,
                    Capa= rd["capa_arquivo"] == DBNull.Value ? null : (string?)rd.GetString("capa_arquivo"),
                    QuantidadeTotal = rd.GetInt32("quantidade_total"),
                    QuantidadeDisponivel = rd.GetInt32("quantidade_disponivel"),
                    CriadoEm = rd.GetDateTime("criado_em")
                });
            }
            return View(lista);
        }

        [HttpGet]
        public IActionResult Criar()
        {
            using var conn = db.GetConnection();


            ViewBag.Autores = CarregarAutores(conn);
            ViewBag.Editoras = CarregarEditoras(conn);
            ViewBag.Generos = CarregarGeneros(conn);
            
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Criar(Livros vm, IFormFile? capa)
        {
            string? relPath = null;

            if (capa != null && capa.Length > 0)
            {
                var ext = Path.GetExtension(capa.FileName);
                //(opicional) validar extensão

                var fileName = $"{Guid.NewGuid()}{ext}";
                var savedir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "capas");
                Directory.CreateDirectory(savedir);
                var absPath = Path.Combine(savedir, fileName);
                using var fs = new FileStream(absPath, FileMode.Create);
                capa.CopyTo(fs);
                relPath = Path.Combine("capas", fileName).Replace("\\", "/");
            }


            if(string.IsNullOrWhiteSpace(vm.Titulo) || vm.QuantidadeTotal < 1)
            {
                ModelState.AddModelError("", "Informe titulo e uma quantidade total válida (>-1).");
                
            }
            if (!ModelState.IsValid)
            {
                using var conn = db.GetConnection();
                vm.Autores = CarregarAutores(conn);
                vm.Editoras = CarregarEditoras(conn);
                vm.Generos = CarregarGeneros(conn);
                return View(vm);
            }
            using var conn2 = db.GetConnection();
            using var cmd = new MySqlCommand("sp_livro_criar", conn2) { CommandType = System.Data.CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("p_titulo", vm.Titulo);
            cmd.Parameters.AddWithValue("p_autor", (object?)vm.AutorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_editora", (object?)vm.EditoraId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_genero", (object?)vm.GeneroId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_ano", (object?)vm.Ano ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_isbn", (object?)vm.Isbn ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_quantidade", (object ?)vm.QuantidadeTotal ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_capa_arquivo", (object?)relPath ?? DBNull.Value);
            cmd.ExecuteNonQuery();

            TempData["ok"] = "Livro cadastrado!";
            return RedirectToAction(nameof(Index));
            // using var cmd = new MySqlCommand("sp_livro_criar", conn2)
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            using var conn = db.GetConnection();

            Livros? livro = null;
            using (var cmd = new MySqlCommand("sp_livro_obter", conn) { CommandType = System.Data.CommandType.StoredProcedure})
            {
                cmd.Parameters.AddWithValue("p_id", id);
                using var rd = cmd.ExecuteReader();
                if(rd.Read())
                {
                    livro = new Livros
                    {
                        Id = rd.GetInt32("id"),
                        Titulo = rd.GetString("titulo"),
                        AutorId = rd["autorId"] == DBNull.Value ? null : (int?)rd.GetInt32("autorId"),
                        EditoraId = rd["editoraId"] == DBNull.Value ? null : (int?)rd.GetInt32("editoraId"),
                        GeneroId = rd["generoId"] == DBNull.Value ? null : (int?)rd.GetInt32("generoId"),
                        Ano = rd["ano"] == DBNull.Value ? null : (short?)rd.GetInt16("ano"),
                        Isbn = rd["isbn"] as string,
                        QuantidadeTotal = rd.GetInt32("quantidade_total"),
                        Capa = rd.GetString("capa_arquivo")

                    };
                }

            }
            if (livro == null) return NotFound();

            ViewBag.Autores = CarregarAutores(conn);
            ViewBag.Editoras = CarregarEditoras(conn);
            ViewBag.Generos = CarregarGeneros(conn);

            return View(livro);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Editar(Livros model, IFormFile capa)
        {
            string? relPath = null;

            if (capa != null && capa.Length > 0)
            {
                var ext = Path.GetExtension(capa.FileName);
                //(opicional) validar extensão

                var fileName = $"{Guid.NewGuid()}{ext}";
                var savedir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "capas");
                Directory.CreateDirectory(savedir);
                var absPath = Path.Combine(savedir, fileName);
                using var fs = new FileStream(absPath, FileMode.Create);
                capa.CopyTo(fs);
                relPath = Path.Combine("capas", fileName).Replace("\\", "/");
            }


            if (model.Id <= 0) return NotFound();
            if(string.IsNullOrWhiteSpace(model.Titulo) || model.QuantidadeTotal < 1)
            {
                ModelState.AddModelError("", "Informe titulo e quantidade total (>=1).");
            }

            using var conn2 = db.GetConnection();
            using var cmd = new MySqlCommand("sp_livro_atualizar", conn2) { CommandType = System.Data.CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("p_id", model.Id);
            cmd.Parameters.AddWithValue("P_titulo", model.Titulo);
            cmd.Parameters.AddWithValue("p_autor", model.AutorId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_editora", model.GeneroId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_genero", model.GeneroId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_ano", model.Ano ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("p_isbn", (object)model.Isbn ?? DBNull.Value);
            cmd.Parameters.AddWithValue("p_novo_total", model.QuantidadeTotal);
            cmd.Parameters.AddWithValue("p_capa_arquivo",(object?) relPath ?? DBNull.Value);
            cmd.ExecuteNonQuery();

            TempData["Ok"] = "Livro atualizada!";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Excluir(int id)
        {
            using var conn = db.GetConnection();
            try
            {

                using var cmd = new MySqlCommand("sp_livro_excluir", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("p_id", id);
                cmd.ExecuteNonQuery();
                TempData["ok"] = "Livro excluido!";
            }
            catch (MySqlException ex)
            {
                // É a mensagem que vai dar erro, a caso já tenha algo ligada ao livro, que vai impossibilitar o delete, evitando que o sistema crashe
                TempData["ok"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

  
        // Helpers para carregar os selects via SP (Stored Procedure)
        private List<SelectListItem> CarregarAutores(MySqlConnection conn)
        {
            var list = new List<SelectListItem>();
            using var cmd = new MySqlCommand("sp_autor_listar", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                list.Add(new SelectListItem { Value = rd.GetInt32("id").ToString(), Text = rd.GetString("nome") });
            return list;
        }
        private List<SelectListItem> CarregarEditoras(MySqlConnection conn)
        {
            var list = new List<SelectListItem>();
            using var cmd = new MySqlCommand("sp_editora_listar", conn) { CommandType = System.Data.CommandType.StoredProcedure };
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                list.Add(new SelectListItem { Value = rd.GetInt32("id").ToString(), Text = rd.GetString("nome") });
            return list;
        }
        private List<SelectListItem> CarregarGeneros(MySqlConnection conn)
        {
            var list = new List<SelectListItem>();
            using var cmd = new MySqlCommand("sp_genero_listar", conn) { CommandType = System.Data.CommandType.StoredProcedure };
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                list.Add(new SelectListItem { Value = rd.GetInt32("id").ToString(), Text = rd.GetString("nome") });
            return list;
        }

    }
}
