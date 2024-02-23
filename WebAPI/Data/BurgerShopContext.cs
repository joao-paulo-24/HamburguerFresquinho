using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjetoLDS.Models;
using WebAPI.Models;

namespace WebAPI.Data
{
    public class BurgerShopContext:DbContext
    {
        public BurgerShopContext(DbContextOptions<BurgerShopContext> options) : base(options) { }

        public DbSet<ItemCompra> ItemCompras { get; set; }

        public DbSet<ItemPedido> ItemPedidos { get; set; }

        public DbSet<Pedido> Pedidos { get; set; }

        public DbSet<ContaUtilizador> ContaUtilizador { get; set; }

        public DbSet<Ingrediente> Ingredientes { get; set; }

        public DbSet<Ticket> Ticket { get; set; }

        public DbSet<Item> Items { get; set; }

        public DbSet<EditIngredientes> EditIngredientes { get;set; }

        public DbSet<Menu> Menus { get; set; }

        public DbSet<IngredienteAtribuido> IngredienteAtribuidos { get; set; }
        public DbSet<ItemAtribuido> ItemAtribuidos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Configurar a relação muitos-para-muitos entre Menu e ItemCompra
            modelBuilder.Entity<Menu>()
                .HasMany(menu => menu.Items)
                .WithMany()
                .UsingEntity(j =>
                {
                    j.ToTable("MenuItems");
                    j.Property<int>("MenuId");
                    j.Property<int>("ItemId");
                    j.HasKey("MenuId", "ItemId");
                });

            // Outras configurações podem ser adicionadas aqui

            base.OnModelCreating(modelBuilder);
        }
    }
}
