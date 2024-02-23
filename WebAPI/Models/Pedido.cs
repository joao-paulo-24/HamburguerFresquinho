using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebAPI.Enums;
using ProjetoLDS.Models;

namespace WebAPI.Models
{
	public class Pedido
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		public List<ItemPedido>? ItensPedido { get; set; }

		[Required]
		public ContaUtilizador? ContaUtilizador { get; set; }

		[Required]
		public DateTime Data { get; set; }

		[Required]
		public EnumStatusPedido Status { get; set; }

		public bool Pago { get; set; }

		public Pedido()
		{

		}

		public Pedido(ContaUtilizador contaUtilizador, List<ItemPedido> itemsPedidos)
		{
			ItensPedido = new List<ItemPedido>();
			ContaUtilizador = contaUtilizador;
			Data = DateTime.Now;
			Status = EnumStatusPedido.Pendente;
			Pago = false;

			foreach (ItemPedido item in itemsPedidos)
			{
				((List<ItemPedido>)ItensPedido).Add(item);
			}
		}

		public int PrecoPedido()
		{
			int preco = 0;
			foreach (var item in ItensPedido)
			{
				if (!item.UsePoints)
					preco += item.Preco;
			}

			return preco;
		}
	}
}
