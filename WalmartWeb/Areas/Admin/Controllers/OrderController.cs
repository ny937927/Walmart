using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Razorpay.Api;
using Stripe.V2;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Walmart.DataAccess.Repository.IRepository;
using Walmart.Model.Models;
using Walmart.Utility;
using WalmartWeb.Areas.Customer.Controllers;

namespace WalmartWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _db;
        private readonly RazorpayService _razorpayService;


        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork db, RazorpayService razorpayService)
        {
            _db = db;
            _razorpayService = razorpayService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            OrderVM= new()
            {
                OrderHeader = _db.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _db.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };

            return View(OrderVM);
        }

        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_PayNow(int orderId)
        {

            OrderVM.OrderHeader = _db.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser");
            OrderVM.OrderDetail = _db.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product");

            var razorOrderId = CreateOrder(OrderVM.OrderHeader);
            OrderVM.RazorPayOptionModel = new RazorPayOptionModel()
            {
                Key = SD.PublishedKey,
                AmountInSubUnits = OrderVM.OrderHeader.OrderTotal,
                Currency = "INR",
                Name = OrderVM.OrderHeader.Name,
                Description = "Testing for RazorPya",
                ImageLogUrl = "",
                OrderId = razorOrderId,
                ProdileName = OrderVM.OrderHeader.Name,
                ProfileContact = OrderVM.OrderHeader.PhoneNumber,
                ProfileEmail = OrderVM.OrderHeader.ApplicationUser.Email,
                Notes = new Dictionary<string, string>()
                    {
                        {"note 1", "this is a payment note" },{"note 2", " here also you can add max 15 notes"}
                    },
                OrderHeaderId = OrderVM.OrderHeader.Id

            };

            TempData["OrderVM"] = JsonConvert.SerializeObject(OrderVM);
            // MakePayment();
            return RedirectToAction(nameof(Payment));

        }

        public IActionResult Payment(int Id)
        {
            TempData["orderHeaderId"] = null;

            if (TempData["OrderVM"] != null)
            {
                var orderVM = JsonConvert.DeserializeObject<OrderVM>(TempData["OrderVM"].ToString());
                TempData["orderHeaderId"] = orderVM.OrderHeader.Id;
                return View(orderVM);
            }


            return View();
        }

        public IActionResult AfterPayment()
        {
            try
            {
                // Helper method to safely extract values from Request.Form
                string GetFormValue(string key) => Request.Form.ContainsKey(key) ? Request.Form[key].ToString() : string.Empty;

                // Retrieve form values
                string orderId = GetFormValue("orderid");
                string paymentId = GetFormValue("paymentId");
                string signature = GetFormValue("signature");
                string paymentstatus = GetFormValue("paymentstatus");
                string id = GetFormValue("orderheaderid");



                // Validate mandatory fields
                if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(paymentId) ||
                    string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(paymentstatus) || string.IsNullOrEmpty(id))
                {
                    TempData["error"] = "Payment Failed! Oops! Something went wrong while processing your payment.";
                    return View("PaymentFail");
                }

                // Check payment status and handle failure immediately
                if (paymentstatus == "Fail")
                {
                    TempData["error"] = "Payment Failed! Oops! Something went wrong while processing your payment.";
                    return RedirectToAction("PaymentFail");
                }

                int orderHeaderId = Convert.ToInt32(id);
                var orderHeader = _db.OrderHeader.Get(u => u.Id == orderHeaderId);

                // Validate the signature
                if (!CompareSignature(orderId, paymentId, signature))
                {
                    TempData["error"] = "Payment Failed! Oops! Something went wrong while processing your payment.";
                    return RedirectToAction("PaymentFail");
                }

                // Update payment info in the database

                _db.OrderHeader.UpdateStripePaymentId(orderHeaderId, signature, paymentId);

                // Only update the status if the payment was successful and not delayed
                if (paymentstatus == "Success" && orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
                {
                    _db.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                }

                _db.Commit();

                // Redirect to OrderConfirmation view
                return RedirectToAction(nameof(OrderConfirmation), new { id = orderHeaderId });
            }
            catch (Exception ex)
            {
                // Log exception (you may want to log this exception)
                TempData["error"] = "Payment Failed! Oops! Something went wrong while processing your payment.";
                return RedirectToAction("PaymentFail");
            }
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _db.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");

           

            return View(orderHeader.Id);
        }



        [HttpPost]
        [Authorize(Roles =SD.Role_Admin + "," + SD.Role_Employee )]
        public IActionResult UpdateOrderDetail(int orderId)
        {
            var orderHeaderFromDb = _db.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;

            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }

            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }
            _db.OrderHeader.Update(orderHeaderFromDb);
            _db.Commit();
            TempData["success"] = "Order Detail Updated Successfully.";

            return RedirectToAction(nameof(Details), new {orderId = orderHeaderFromDb.Id});
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _db.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProgress);
            _db.Commit();
            TempData["success"] = "Order Status Updated to Inprogress!!";
            return RedirectToAction(nameof(Details), new {orderId = OrderVM.OrderHeader.Id});

        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeaderFromDb = _db.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeaderFromDb.OrderStatus = SD.StatusShipped;
            orderHeaderFromDb.ShippingDate = DateTime.Now;

            if(orderHeaderFromDb.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeaderFromDb.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            _db.OrderHeader.Update(orderHeaderFromDb);
            _db.Commit();

            TempData["success"] = "Order Shipped Successfully!!";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });

        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeaderFromDb = _db.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
           
            if(orderHeaderFromDb.PaymentStatus == SD.PaymentStatusApproved)
            {
                //Razor pay refund code will come , Please check out on razor pay doc.

                try
                {
                    // Call Razorpay refund API
                    var refund = _razorpayService.RefundPayment(orderHeaderFromDb.PaymentIntentId, orderHeaderFromDb.OrderTotal);

                    if (refund != null)
                    {
                        //return Ok(new
                        //{
                        //    success = true,
                        //    refund = refund
                        //});

                        //After refund successfull please update status for customer
                        TempData["success"] = "Order Cancelled Successfully!! You will be recieving your Refund within 3-4 Business day. Thank you for Shopping!";
                        _db.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.StatusRefunded);
                    }

                    return BadRequest("Refund failed.");
                }
                catch (Exception ex)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = ex.Message
                    });
                }


            }
            else
            {
                _db.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.StatusCancelled);
                TempData["success"] = "Order Cancelled Successfully!!";

            }
            _db.Commit();
            
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });

        }

        #region APIs call
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;

            if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaders = _db.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                ClaimsIdentity claimsIdentity = (ClaimsIdentity?)User.Identity;
                string userId = (claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier))?.Value;

                orderHeaders = _db.OrderHeader.GetAll(u => u.ApplicationUserId == userId,includeProperties:"ApplicationUser").ToList();

            }
            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProgress);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }
            return Json(new { data = orderHeaders });
        }



        private string CreateOrder(OrderHeader orderHeader)
        {
            try
            {
                RazorpayClient razorpayClient = new RazorpayClient(SD.PublishedKey, SD.SecretKey);
                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("amount", (orderHeader.OrderTotal * 100).ToString());
                options.Add("currency", "INR");
                options.Add("payment_capture", "1"); // 0 -Manual payment Authorize  , 1- Auto payment Authorize capture
                options.Add("receipt", orderHeader.Id.ToString());

                Razorpay.Api.Order orderResponse = razorpayClient.Order.Create(options);
                var orderId = orderResponse.Attributes["id"].ToString();
                return orderId;

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private bool CompareSignature(string orderId, string paymentId, string signature)
        {
            var text = orderId + "|" + paymentId;
            var secret = SD.SecretKey;
            var generatedSignature = CalculateSHA256(text, secret);
            if (generatedSignature == signature)
            {
                return true;
            }
            else
            {
                return false;

            }
        }

        private string CalculateSHA256(string text, string secret)
        {
            string result = "";
            var enc = Encoding.Default;
            byte[]
            baText2BeHashed = enc.GetBytes(text),
            baSalt = enc.GetBytes(secret);
            HMACSHA256 hasher = new HMACSHA256(baSalt);
            byte[] baHashedText = hasher.ComputeHash(baText2BeHashed);
            result = string.Join("", baHashedText.ToList().Select(b => b.ToString("x2")).ToArray());
            return result;

        }

        #endregion
    }
 }
