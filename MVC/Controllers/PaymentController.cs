using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using mvc.Enums;
using mvc.Models;
using mvc.Models.Authorize;
using mvc.RepoInterfaces;
using mvc.ViewModels.Payment;
using PayPalCheckoutSdk.Orders;
using System.Security.Claims;
using System.Threading.Tasks;

namespace mvc.Controllers
{
    public class PaymentController : Controller
    {
        IPaymentRepository _paymentRepository;
        IBussinessRepository _bussinessRepository;
        IPackageRepository _packageRepository;

        public PaymentController(IPaymentRepository paymentRepository,IConfiguration configuration,IBussinessRepository bussinessRepository ,IPackageRepository packageRepository)
        {
            _paymentRepository = paymentRepository;
            _bussinessRepository = bussinessRepository;
            _packageRepository= packageRepository;

        }


        [Authorize]
        public async Task<IActionResult> CreateOrder(OrderVM orderVM)
        {
            try
            {
                var business = await _bussinessRepository.GetByIdAsync(orderVM.BussnissId);
                if (business == null)
                {
                    ModelState.AddModelError("BussnissId", "Business not found ");

                }
                // Check if the package exists
                var package = await _packageRepository.GetByIdAsync(orderVM.PackageId);
                if (package == null)
                {
                    ModelState.AddModelError("PackageId", "Package not found ");

                }
                switch (orderVM.Subscription)
                {
                    case SubscriptionType.Monthly:
                        if (orderVM.Amount != package.MonthlyPrice)
                        {
                            ModelState.AddModelError("Amount", "invalid aomunt");
                        }
                        break;
                    case SubscriptionType.Yearly:
                        if (orderVM.Amount != package.YearlyPrice)
                        {
                            ModelState.AddModelError("Amount", "invalid aomunt");
                        }
                        break;
                    default:
                        ModelState.AddModelError("Subscription", "invalid Subscription");
                        break;
                }
                if (ModelState.IsValid)
                {


                    //save new record in checkout table with status pending
                    Checkout checkout = new Checkout
                    {
                        Amount = orderVM.Amount,
                        PaymentMethod = Enums.PaymentMethod.PayPal,
                        PaymentStatus = PaymentStatus.Pending,
                        BusinessId = orderVM.BussnissId,
                        PackageId = orderVM.PackageId,
                        UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        SubscriptionType = orderVM.Subscription
                    };
                    //need to check if bussness is belong to this user
                   
                    await _paymentRepository.AddAsync(checkout);
                    await _paymentRepository.SaveAsync();
                    // Create the order using the payment repository
                    string baseUrl = $"{Request.Scheme}://{Request.Host}";
                    var approvalUrl = await _paymentRepository.CreateOrderAsync(checkout, orderVM.Currency, baseUrl);
                    return Redirect(approvalUrl);
                }
                string errors =string.Join(" , ",ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));


                return RedirectToAction("Error", new {msg=errors});
                
            }
            catch (Exception ex)
            {
                // Log error
                return RedirectToAction("Error", new {msg=ex.Message});
            }
        }

        [HttpGet]
        public async Task<IActionResult> Success(int id, SubscriptionType subscription, int businessId, int packageid, string token)
        {
            // try
            //{

            Order? response = await _paymentRepository.CapturePaymentAsync(token);

            if (response.Status == "COMPLETED")
            {
                //update payment status and transactionId in checkout table =>i need checout id
                Checkout checkout = await _paymentRepository.GetByIdAsync(id);
                checkout.PaymentStatus = PaymentStatus.Completed;
                checkout.TransactionId = response.Id;

                //update bussness table => subscription end date and package id => need bussiness id
                Business business = await _bussinessRepository.GetByIdAsync(businessId);
                if (subscription == SubscriptionType.Monthly)
                {
                    business.SubscriptionEndDate = DateTime.UtcNow.AddMonths(1);
                }
                else
                {
                    business.SubscriptionEndDate = DateTime.UtcNow.AddYears(1);
                }
                business.PackageId = packageid;
                await _paymentRepository.SaveAsync();
                //view show success message
                return View("Success");
            }

            return RedirectToAction("Error", new { msg = $"payment Status {response.Status}" });
       
            //}
            //catch (Exception ex)
            //{
            //    // Log error
            //    return RedirectToAction("Error");
            //}
        }

        [HttpGet]
        public async Task<IActionResult> Cancel(int id)
        {
            //update payment status in checkout table => need checout id
            Checkout checkout =await _paymentRepository.GetByIdAsync(id);
            checkout.PaymentStatus = PaymentStatus.Failed;
            return View(checkout);
        }

        [HttpGet]
        public IActionResult Error(string msg)
        {
            ViewBag.msg = msg;
            return View();
        }

        
        public async Task<IActionResult> GetAllChecouts()
        {
            var checkouts =await _paymentRepository.GetAll().ToListAsync();
            return View(checkouts);
        }

    }
}
