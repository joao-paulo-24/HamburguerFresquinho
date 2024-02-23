using ProjetoLDS.Models;
using WebAPI.Models;

namespace MVC.Models
{
    public class CriarPedidoViewModel
    {
        public List<ItemCompra> ItemCarrinho { get; set; }
        public List<ItemCompra> ItemParaCompra { get; set; }

        public Pedido Pedido { get; set; }

        public CriarPedidoViewModel(List<ItemCompra> itemCarrinho,List<ItemCompra> itemParaCompra) { 
        
            ItemParaCompra = itemParaCompra;
            ItemCarrinho = itemCarrinho;
        }

        public int ObterPrecoCarrinho()
        {
            int preco = 0;
            foreach (var item in ItemCarrinho)
            {
                preco += item.Preco;
            }

            return preco;
        }
    }
}
