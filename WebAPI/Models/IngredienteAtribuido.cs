using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    public class IngredienteAtribuido
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string ItemName { get; set; }
        public int ItemId { get; set; }
        public int IdIngrediente { get; set; }
    }
}