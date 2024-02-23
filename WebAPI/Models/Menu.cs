using ProjetoLDS.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models
{
	public class Menu : ItemCompra
	{
		public List<Item>? Items { get; set; }

		public Menu() { }

		public Menu(string nome, int preco)
		{
			Name = nome;
			Preco = preco;
		}

		public Menu(string nome, int preco, int pontosNecessarios)
		{
			Name = nome;
			Preco = preco;
			PontosNecessarios = pontosNecessarios;
		}

		public override void Use(List<EditIngredientes>? editIngredientes)
		{
			if (Items != null)
				foreach (var item in Items)
				{
					item.Use(editIngredientes);
				}
		}

		public override bool CheckHaveStock()
		{
			if (Items != null)
				foreach (var item in Items)
				{
					if (!item.CheckHaveStock())
						return false;
				}

			return true;
		}
	}
}
