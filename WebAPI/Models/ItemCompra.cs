using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebAPI.Models;

namespace ProjetoLDS.Models
{
    public class ItemCompra
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }


        [MaxLength(30)]
        public string? Name { get; set; }

        [NotMapped]
        public IFormFile? Image { get; set; }

        public string? ImagePath { get; set; }

        /// <summary>
        /// Preço atual do itemCompra
        /// </summary>
        [Required]
        [Range(0, Double.PositiveInfinity)]
        public int Preco { get; set; }

        public int PontosNecessarios { get; set; } = -1;

        public ItemCompra() { }

        public ItemCompra(string name, int preco)
        {
            Name = name;
            Preco = preco;
        }

        public ItemCompra(string name, int preco, int pontosNecessarios)
        {
            Name = name;
            Preco = preco;
            PontosNecessarios = pontosNecessarios;
        }

        public virtual void Use(List<EditIngredientes>? editIngredientes) { throw new Exception(); }

		public virtual bool CheckHaveStock() { throw new Exception(); }
	}
}