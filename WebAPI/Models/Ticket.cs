using ProjetoLDS.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WebAPI.Enums;

namespace WebAPI.Models
{
    public class Ticket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public ContaUtilizador? ContaUtilizador { get; set; }
        [MaxLength(40)]
        public string Titulo { get; set; }

        [MaxLength(150)]
        public string Texto { get; set; }

        public EnumStatusTicket Status { get; set; }

        public Ticket() { }

        public Ticket(ContaUtilizador contaUtilizador, string titulo, string texto, EnumStatusTicket status)
        {
            ContaUtilizador = contaUtilizador;
            Titulo = titulo;
            Texto = texto;
            Status = status;
        }
    }
}
