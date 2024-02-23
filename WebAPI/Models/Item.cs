using ProjetoLDS.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using WebAPI.Enums;

namespace WebAPI.Models
{
	public class Item : ItemCompra
	{

		public List<Ingrediente>? Ingredientes { get; set; } = new List<Ingrediente>();
		[Required]
		public EnumTypeComida TypeComida { get; set; }


		public Item()
		{

		}

		public Item(EnumTypeComida typeComida, string name, int preco)
		{
			TypeComida = typeComida;
			Name = name;
			Preco = preco;
		}

		/// <summary>
		/// Usar item, consiste em remover o stock dos ingredientes
		/// </summary>
		public override void Use(List<EditIngredientes>? editIngredientes)
		{
			List<Ingrediente> ingredienteToUse = new List<Ingrediente>(Ingredientes);

			foreach (var item in Ingredientes)
			{
				foreach (var editIngrediente in editIngredientes)
				{
					if (editIngrediente.Item.Id != Id)
					{
						continue;
					}

					if (editIngrediente.Ingrediente.Id == item.Id)
					{
						if (editIngrediente.Active)
							item.Use();

						ingredienteToUse.Remove(item);
					}
				}
			}

			foreach (var item in ingredienteToUse) { item.Use(); }
		}

		/// <summary>
		/// Adicionar Ingredientes ao item
		/// </summary>
		/// <param name="ingrediente"> Ingrediente a adicionar </param>
		private void AddIngrediente(Ingrediente ingrediente)
		{
			Ingredientes.Add(ingrediente);
		}

		/// <summary>
		/// Adicionar uma lista de ingredientes ao item
		/// </summary>
		/// <param name="ingredientes"> Ingredientes a adicionar </param>
		private void AddIngredientes(IEnumerable<Ingrediente> ingredientes)
		{
			foreach (var ingrediente in ingredientes)
			{
				Ingredientes.Add(ingrediente);
			}
		}

		public override bool CheckHaveStock() {

			foreach (var item in Ingredientes)
			{
				if(!item.CheckStock())
					return false;
			}

			return true;	
		}
	}
} 