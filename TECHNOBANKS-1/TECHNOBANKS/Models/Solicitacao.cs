using System;
using System.ComponentModel.DataAnnotations;

namespace TECHNOBANKS.Models
{
    public class Solicitacao
    {
        public int Id { get; set; }

        [Required]
        public string Tipo { get; set; }

        [Required]
        public string Descricao { get; set; }

        [Required]
        public string Endereco { get; set; }

        public string Status { get; set; } = "Aberto";

        public DateTime DataAbertura { get; set; } = DateTime.Now;
    }
}