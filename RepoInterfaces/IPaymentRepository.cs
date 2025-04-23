using mvc.Models;
using PayPalCheckoutSdk.Orders;


namespace mvc.RepoInterfaces
{
    public interface IPaymentRepository : IGeniricRepository<int,Checkout>
    {
        Task<string> CreateOrderAsync(Checkout checkout, string currency,string baseUrl);
        Task<Order?> CapturePaymentAsync(string orderId);
       
    }
}
