using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAPI.Migrations
{
    /// <inheritdoc />
    public partial class @new : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContaUtilizador",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Pontos = table.Column<int>(type: "int", nullable: true),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContaUtilizador", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IngredienteAtribuidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    IdIngrediente = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredienteAtribuidos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemAtribuidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    IdItem = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemAtribuidos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemCompras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Preco = table.Column<int>(type: "int", nullable: false),
                    PontosNecessarios = table.Column<int>(type: "int", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeComida = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCompras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContaUtilizadorId = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Pago = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pedidos_ContaUtilizador_ContaUtilizadorId",
                        column: x => x.ContaUtilizadorId,
                        principalTable: "ContaUtilizador",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ticket",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContaUtilizadorId = table.Column<int>(type: "int", nullable: true),
                    Titulo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ticket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ticket_ContaUtilizador_ContaUtilizadorId",
                        column: x => x.ContaUtilizadorId,
                        principalTable: "ContaUtilizador",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Ingredientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TypeComida = table.Column<int>(type: "int", nullable: true),
                    Stock = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<int>(type: "int", nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ingredientes_ItemCompras_ItemId",
                        column: x => x.ItemId,
                        principalTable: "ItemCompras",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => new { x.MenuId, x.ItemId });
                    table.ForeignKey(
                        name: "FK_MenuItems_ItemCompras_ItemId",
                        column: x => x.ItemId,
                        principalTable: "ItemCompras",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MenuItems_ItemCompras_MenuId",
                        column: x => x.MenuId,
                        principalTable: "ItemCompras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemPedidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemCompraId = table.Column<int>(type: "int", nullable: false),
                    UsePoints = table.Column<bool>(type: "bit", nullable: false),
                    Preco = table.Column<int>(type: "int", nullable: false),
                    PedidoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemPedidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemPedidos_ItemCompras_ItemCompraId",
                        column: x => x.ItemCompraId,
                        principalTable: "ItemCompras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemPedidos_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EditIngredientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    IngredienteId = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ItemPedidoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditIngredientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EditIngredientes_Ingredientes_IngredienteId",
                        column: x => x.IngredienteId,
                        principalTable: "Ingredientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EditIngredientes_ItemCompras_ItemId",
                        column: x => x.ItemId,
                        principalTable: "ItemCompras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EditIngredientes_ItemPedidos_ItemPedidoId",
                        column: x => x.ItemPedidoId,
                        principalTable: "ItemPedidos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EditIngredientes_IngredienteId",
                table: "EditIngredientes",
                column: "IngredienteId");

            migrationBuilder.CreateIndex(
                name: "IX_EditIngredientes_ItemId",
                table: "EditIngredientes",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EditIngredientes_ItemPedidoId",
                table: "EditIngredientes",
                column: "ItemPedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredientes_ItemId",
                table: "Ingredientes",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemPedidos_ItemCompraId",
                table: "ItemPedidos",
                column: "ItemCompraId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemPedidos_PedidoId",
                table: "ItemPedidos",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_ItemId",
                table: "MenuItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_ContaUtilizadorId",
                table: "Pedidos",
                column: "ContaUtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_ContaUtilizadorId",
                table: "Ticket",
                column: "ContaUtilizadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EditIngredientes");

            migrationBuilder.DropTable(
                name: "IngredienteAtribuidos");

            migrationBuilder.DropTable(
                name: "ItemAtribuidos");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "Ticket");

            migrationBuilder.DropTable(
                name: "Ingredientes");

            migrationBuilder.DropTable(
                name: "ItemPedidos");

            migrationBuilder.DropTable(
                name: "ItemCompras");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "ContaUtilizador");
        }
    }
}
