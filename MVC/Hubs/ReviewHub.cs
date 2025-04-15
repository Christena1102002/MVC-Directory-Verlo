using Microsoft.AspNetCore.SignalR;
using mvc.ViewModels.ReviewVM;
using System.Threading.Tasks;

namespace mvc.Hubs
{
    public class ReviewHub : Hub
    {
        public async Task SendNewReview(AddReviewVM review)
        {
            await Clients.All.SendAsync("NewReviewArrived", review);
        }

        public async Task NotifyNewReview(int businessId, string email, int rating, string comment)
        {
            await Clients.All.SendAsync("NewReviewArrived", new { 
                businessId, 
                email, 
                rating, 
                comment,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyReviewDeleted(int businessId, int reviewId)
        {
            await Clients.All.SendAsync("ReviewDeleted", new { 
                businessId, 
                reviewId,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
