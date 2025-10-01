﻿using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProjetoBiblioteca.Models
{
    public class Livros
    {
        public int Id { get; set; }
        public string? Titulo { get; set; }
        public int? AutorId { get; set; }
        public int? EditoraId { get; set; }
        public int? GeneroId { get; set; }
        public short? Ano { get; set; }
        public string? Isbn { get; set; }
        public int QuantidadeTotal { get; set; }
        public int QuantidadeDisponivel { get; set; }
        public DateTime CriadoEm { get; set; }
        public string? Autor { get; set; }
        public string? Editor { get; set; }
        public string? Genero { get; set; }

        public string? Capa { get; set; }

        // Para exibição (JOINs)
        public List<SelectListItem> Autores { get; set; } = new();
        public List<SelectListItem> Editoras { get; set; } = new();
        public List<SelectListItem> Generos { get; set; } = new();
    }
}
