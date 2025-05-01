using System.ComponentModel.DataAnnotations;

namespace desafio_backend_2025.Models
{
    public class Conta
    {
        public int Id { get; set; }

        
        public required string CPF { get; set; }

        [Required]
        public int NumeroConta { get; set; }

        [Required]
        public int Agencia { get; set; }


        public string nome { get; set; }
        public string email { get; set; }
        public string endereco { get; set; }
        public string telefone { get; set; }
    }
}
