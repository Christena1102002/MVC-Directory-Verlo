using Microsoft.Build.Evaluation;
using mvc.Enums;
using mvc.Repositories;
using mvc.Models;

namespace mvc.ViewModels.Payment
{
    public class OrderVM
    {
        public int PackageId { get; set; }
        public SubscriptionType Subscription { get; set; }
        public int BussnissId { get; set; }
        
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
    }
}
