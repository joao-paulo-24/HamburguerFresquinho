using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebAPI.Enums;

namespace ProjetoLDS.Models
{
    public class ContaUtilizador
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Username { get; set; }

        [MinLength(6)]
        public string Password { get; set; }

        [EmailAddress]
        [Required]
        public string Email { get; set; }

        public int? Pontos { get; set; }

        [Range(0, 2)]
        public EnumUserRole Role { get; set; }


        public ContaUtilizador()
        {

        }

        public ContaUtilizador(string username, string password, string email, EnumUserRole role)
        {
            Username = username;
            Password = password;
            Email = email;
            Pontos = 0;
            Role = role;
        }

        public ContaUtilizador(string username, string password, string email)
        {
            Username = username;
            Password = password; 
            Email = email;
            Pontos = 0;
        }

        public void AdicionarPontos(int pontosAdicionar)
        {
            Pontos += pontosAdicionar;
        }

        public void RemoverPontos(int pontosRemover)
        {
            // Pontos insuficientes
            if (pontosRemover > Pontos)
            {
                throw new Exception("Pontos insuficientes para comprar o item.");
            }

            Pontos -= pontosRemover;
        }
    }
}