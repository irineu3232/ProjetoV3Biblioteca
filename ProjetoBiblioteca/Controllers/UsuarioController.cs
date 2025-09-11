using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Crypto.Generators;
using ProjetoBiblioteca.Data;
using ProjetoBiblioteca.Models;
using BCrypt.Net;


// Para instalar o BCrypt, vai para tools Nuget Package Manager, depois nuget package console
// Depois insira no terminal Install-Package BCrypt.Net-Next para instalar o BCrypt para criptografar as senhas na hora de enviar ao banco de dados.


namespace ProjetoBiblioteca.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly Database db = new Database();

        public IActionResult Index()
        {
            List<Usuarios> usuarios = new List<Usuarios>();
            using (var conn = db.GetConnection())
            {
                var sql = "select distinct nome, email, role, ativo, criado_Em from Usuarios order by nome";
                var cmd = new MySqlCommand(sql, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    usuarios.Add(new Usuarios
                    {
                        Nome = reader.GetString("nome"),
                        Email = reader.GetString("email"),
                        Role = reader.GetString("role"),
                        Ativo = reader.GetInt32("ativo"),
                        CriadoEm = reader.GetDateTime("criado_Em")
                    });
                }
            }
            return View(usuarios);
        }

        public IActionResult CriarUsuario()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CriarUsuario(Usuarios vm)
        {
            using var conn = db.GetConnection();

            using var cmd = new MySqlCommand("sp_usuario_criar", conn);

            var senhaHash = BCrypt.Net.BCrypt.HashPassword(vm.Senha, workFactor: 12);


            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("p_nome", vm.Nome);
            cmd.Parameters.AddWithValue("p_email", vm.Email);
            cmd.Parameters.AddWithValue("p_senha_hash", senhaHash);      
            cmd.Parameters.AddWithValue("p_role", vm.Role);
            cmd.ExecuteNonQuery();

            return RedirectToAction("Index");
        }
    }
}
