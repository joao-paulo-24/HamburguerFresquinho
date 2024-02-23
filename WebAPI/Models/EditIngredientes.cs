using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models
{
    /// <summary>
    /// Igrediente alterado com a informação do item que foi alterado e se o item foi adicionado ou removido
    /// </summary>
    public class EditIngredientes
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Item Item { get; set; }
        public Ingrediente Ingrediente { get; set; }
        public bool Active { get; set; }

        public EditIngredientes() { }

        public EditIngredientes(Item item,Ingrediente ingrediente, bool active) {
            Item = item;
            Ingrediente = ingrediente;
            Active = active;
        }
    }
}
