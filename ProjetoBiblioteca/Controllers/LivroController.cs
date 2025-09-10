using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using ProjetoBiblioteca.Data;
using ProjetoBiblioteca.Models;

namespace ProjetoBiblioteca.Controllers
{
    public class LivroController : Controller
    {
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
                    AutorId = rd["autor"] == DBNull.Value ? null : (int?)rd.GetInt32("autor"),
                    EditoraId = rd["editora"] == DBNull.Value ? null : (int?)rd.GetInt32("editora"),
                    GeneroId = rd["genero"] == DBNull.Value ? null : (int?)rd.GetInt32("genero"),
                    Autor = rd["autores"] as string,
                    Editor = rd["editoras"] as string,
                    Genero = rd["generos"] as string,
                    Ano = rd["ano"] == DBNull.Value ? null : (short?)rd.GetInt16("ano"),
                    Isbn = rd["isbn"] as string,
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
            var vm = new LivroFormVM
            {
                Autores = CarregarAutores(conn),
                Editoras = CarregarEditoras(conn),
                CarregarGeneros = CarregarGeneros(conn)
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Criar(LivroFormVM vm)
        {
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
            // using var cmd = new MySqlCommand("sp_livro_criar", conn2)
        }

        private readonly Database db = new Database();

        // Helpers para carregar os selects via SP (Stored Procedure)
        private List<SelectListItem> CarregarAutores(MySqlConnection conn)
        {
            var list = new List<SelectListItem>();
            using var cmd = new MySqlCommand("sp_autor_listar", conn) { CommandType = System.Data.CommandType.StoredProcedure };
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
