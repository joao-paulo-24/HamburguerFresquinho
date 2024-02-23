using System;
using WebAPI.Models;
using PayPal.Api;
using ProjetoLDS.Models;

namespace WebAPI.Services
{
    public class PaypalService
    {
        private static PayPal.Api.Payment payment;

        public Payment CreatePayment(string baseUrl, string cancelUrl, string returnUrl, List<ItemPedido> ItensPedido)
        {
            var apiContext = GetApiContext();

            var itemList = new ItemList()
            {
                items = new List<PayPal.Api.Item>()
            };

            foreach (var itempedido in ItensPedido)
            {
                itemList.items.Add(new PayPal.Api.Item
                {
                    name = itempedido.ItemCompra.Name,
                    currency = "EUR",
                    price = itempedido.Preco.ToString("F2")
                });
            }

            var payer = new Payer() { payment_method = "paypal" };

            var redirectUrls = new RedirectUrls()
            {
                cancel_url = cancelUrl,
                return_url = returnUrl
            };

            var details = new Details()
            {
                tax = "1.00",
                shipping = "0.00",
                subtotal = ItensPedido.Sum(item => item.Preco).ToString("F2")
            };

            var amount = new Amount()
            {
                currency = "EUR",
                total = (ItensPedido.Sum(item => item.Preco) + 3).ToString("F2"), // subtotal + tax + shipping
                details = details
            };

            var transactionList = new List<Transaction>();
            transactionList.Add(new Transaction()
            {
                description = "Pedido na Hamburguer Fresquinho",
                invoice_number = Guid.NewGuid().ToString(), // Gere um número de fatura exclusivo
                amount = amount,
                item_list = itemList
            });

            payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirectUrls
            };

            return payment.Create(apiContext);
        }

        public Payment ExecutePayment(string payerId, string paymentId)
        {
            var apiContext = GetApiContext();

            var paymentExecution = new PaymentExecution() { payer_id = payerId };
            payment = new Payment() { id = paymentId };

            return payment.Execute(apiContext, paymentExecution);
        }

        private APIContext GetApiContext()
        {
            var apiContext = new APIContext(new OAuthTokenCredential
                ("AbBTu0oiuMYr_HItYewDeY1m7IbVKfxnI7e0BjbM9BD3qePjMHRy8el58BquU7pKGtl2eUqnHJQ8Ycbj",
                "EN8kt8abAaWLtrM4mZTQxEKS2oHVwoKXDsRJpTWwPskmdUiqq9W82ovnrS93bNRDRwcDnRbbyQT4dnId")
                .GetAccessToken());
            apiContext.Config = ConfigManager.Instance.GetProperties();
            return apiContext;
        }
    }
}
