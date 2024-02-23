using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WebAPI.Models;

namespace ProjetoLDS.Models
{
    public class ItemPedido
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public ItemCompra ItemCompra { get; set; }

        public bool UsePoints { get; set; }

        /// <summary>
        /// Preço Pago pelo itemCompra
        /// </summary>
        [Required]
        [Range(0, Double.PositiveInfinity)]
        public int Preco { get; set; }

        public List<EditIngredientes>? EditIngredientes { get; set; }

        public ItemPedido() { }

        public ItemPedido(ItemCompra itemCompra)
        {
            ItemCompra = itemCompra;
        }

        public ItemPedido(ItemCompra itemCompra, int preco)
        {
            ItemCompra = itemCompra;
            Preco = preco;
        }

        public ItemPedido(ItemCompra itemCompra, int preco, List<EditIngredientes> editIngredientes)
        {
            ItemCompra = itemCompra;
            Preco = preco;
            EditIngredientes = new List<EditIngredientes>(editIngredientes);
        }

        public ItemPedido(ItemCompra itemCompra,List<EditIngredientes> editIngredientes)
        {
            ItemCompra = itemCompra;
            EditIngredientes = new List<EditIngredientes>(editIngredientes);
        }
    }
}
