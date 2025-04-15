using mvc.Enums;
using mvc.Models;
using mvc.RepoInterfaces;
using mvc.Models;
using PayPalCheckoutSdk.Core;

using PayPalCheckoutSdk.Orders;

//using PayPal.Api;
using System.Threading.Tasks;


namespace mvc.Repositories
{
    public class PaymentRepository:GeniricRepository<int, Checkout>, IPaymentRepository,IGeniricRepository<int, Checkout>
    {
        private readonly IConfiguration _config;
        private PayPalHttpClient _client;
        public PaymentRepository(ProjectContext context, IConfiguration config) :base(context)
        {
            _config = config;
            var environment = new SandboxEnvironment(
                _config["PayPalSettings:ClientId"],
                _config["PayPalSettings:ClientSecret"]);
            _client = new PayPalHttpClient(environment);
        }
       
        public async Task<string> CreateOrderAsync(Checkout checkout, string currency,string baseUrl)
        {
            var order = new OrderRequest()
            {
                CheckoutPaymentIntent = "CAPTURE",
                PurchaseUnits = new List<PurchaseUnitRequest>()
            {
                new PurchaseUnitRequest()
                {
                    AmountWithBreakdown = new AmountWithBreakdown()
                    {
                        CurrencyCode = currency,
                        Value = checkout.Amount.ToString("0.00")
                    }
                }
            },
                ApplicationContext = new ApplicationContext()
                {
                    ReturnUrl = $"{baseUrl}/payment/success/{checkout.Id}/{checkout.BusinessId}/{checkout.PackageId}/{checkout.SubscriptionType}",
                    CancelUrl = $"{baseUrl}/payment/cancel/{checkout.Id}"
                }
            };

            var request = new OrdersCreateRequest();
            request.Prefer("return=representation");
            request.RequestBody(order);

            var response = await _client.Execute(request);
            var result = response.Result<Order>();
            // Find approve link
            var approveLink = result.Links.FirstOrDefault(link => link.Rel == "approve");

            return approveLink?.Href;
        }

        public async Task<Order> CapturePaymentAsync(string orderId)
        {
            var request = new OrdersCaptureRequest(orderId);
            request.RequestBody(new OrderActionRequest());

            var response = await _client.Execute(request);
            var result = response.Result<Order>();
            
            return result;
        }

        


    }
}
