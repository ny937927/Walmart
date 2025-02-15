using Walmart.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;
using System.Security.Claims;
using Walmart.Model.Models;
using Walmart.Utility;
using Stripe.Checkout;
using System;
using Razorpay.Api;
using Newtonsoft.Json;
using System.Text;
using System.Security.Cryptography;
using Stripe.Climate;

namespace WalmartWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _db;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        [BindProperty]
        public OrderHeader OrderHeader { get; set; }

        public CartController(IUnitOfWork db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            ClaimsIdentity claimsIdentity = (ClaimsIdentity?)User.Identity;
            var claim = (claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier));

            if (claim != null)
            {
                HttpContext.Session.SetInt32(SD.SessionCart, _db.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).Count());

            }

            ShoppingCartVM = new()
            {
                ShoppingCartList = _db.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value,
                includeProperties: "Product"),
                OrderHeader = new()
            };

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetCartPrice(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }


        public IActionResult Summary()
        {

            ClaimsIdentity claimsIdentity = (ClaimsIdentity?)User.Identity;
            string userId = (claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier))?.Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _db.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _db.ApplicationUser.Get(u => u.Id == userId);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetCartPrice(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {

            ClaimsIdentity claimsIdentity = (ClaimsIdentity?)User.Identity;
            string userId = (claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier))?.Value;

            ShoppingCartVM.ShoppingCartList = _db.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product");

            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

            ApplicationUser applicationUser = _db.ApplicationUser.Get(u => u.Id == userId);

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetCartPrice(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //it is a regular customer account and we need to capture payment right away.
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                //It is a company user.
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }

            _db.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _db.Commit();

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count,
                };
                _db.OrderDetail.Add(orderDetail);
                _db.Commit();
            }


            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //it is a regular customer account and we need to capture payment right away.// Strip code
                //var domain = "https://localhost:7029/"; //Live link - https://ny937927demo1.bsite.net/
                //var options = new Stripe.Checkout.SessionCreateOptions
                //{
                //    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                //    CancelUrl = domain + $"customer/cart/Index",
                //    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                //    Mode = "payment",
                //};
                //foreach(var item in ShoppingCartVM.ShoppingCartList)
                //{
                //    var sessionLineItem = new SessionLineItemOptions
                //    {
                //        PriceData = new SessionLineItemPriceDataOptions
                //        {
                //            UnitAmount = (long)(item.Price * 100), // 20.50 = 2050
                //            Currency = "usd",
                //            ProductData = new SessionLineItemPriceDataProductDataOptions
                //            {
                //                Name = item.Product.Title
                //            }
                //        },
                //        Quantity = item.Count
                //    };
                //    options.LineItems.Add(sessionLineItem);
                //}

                //var service = new Stripe.Checkout.SessionService();
                //Session session = service.Create(options);

                //_db.OrderHeader.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id,session.Id,session.PaymentIntentId);
                //_db.Commit();
                //Response.Headers.Add("Location", session.Url);
                //return new StatusCodeResult(303);
                //---------------------------------------------------------------------RazorPay code
              
                
                //string key = "rzp_test_J5Ak2IFloaN58A";
                //string secret = "ZT9w8JQleZCZYEhDLaj6HsOe";

                //Random _random = new Random();
                //string transactionId = _random.Next(0, 3000).ToString();

                //Dictionary<string, object> input = new Dictionary<string, object>();
                //input.Add("amount", "50000");
                //input.Add("currency", "INR");
                //input.Add("receipt", transactionId);

                //RazorpayClient razorpayClient = new RazorpayClient(key, secret);
                //Razorpay.Api.Order order = razorpayClient.Order.Create(input);
                //ViewBag.orderId = order["id"].ToString();





                //--------

                var orderId = CreateOrder(ShoppingCartVM.OrderHeader);
                RazorPayOptionModel razorPayOptionModel = new RazorPayOptionModel()
                {
                    Key = SD.PublishedKey,
                    AmountInSubUnits = ShoppingCartVM.OrderHeader.OrderTotal,
                    Currency = "INR",
                    Name = ShoppingCartVM.OrderHeader.Name,
                    Description = "Testing for RazorPya",
                    ImageLogUrl = "",
                    OrderId = orderId,
                    ProdileName = ShoppingCartVM.OrderHeader.Name,
                    ProfileContact = ShoppingCartVM.OrderHeader.PhoneNumber,
                    ProfileEmail = applicationUser.Email,
                    Notes = new Dictionary<string, string>()
                    {
                        {"note 1", "this is a payment note" },{"note 2", " here also you can add max 15 notes"}
                    },
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id

                };

                TempData["RazorPayOptionModel"] = JsonConvert.SerializeObject(razorPayOptionModel);

                //------------------------------------------------------

                // MakePayment();
                return RedirectToAction(nameof(Payment));
            }

            return RedirectToAction(nameof(OrderConfirmation), ShoppingCartVM.OrderHeader);
        }

        public IActionResult Payment(int Id)
        {
            TempData["orderHeaderId"] = null;

            if (TempData["RazorPayOptionModel"] != null)
            {
                var razorPayOptionModel = JsonConvert.DeserializeObject<RazorPayOptionModel>(TempData["RazorPayOptionModel"].ToString());
                TempData["orderHeaderId"] = razorPayOptionModel.OrderHeaderId;
                return View(razorPayOptionModel);
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
                if (paymentstatus == "Success" && orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
                {
                    _db.OrderHeader.UpdateStatus(orderHeaderId, SD.StatusApproved, SD.PaymentStatusApproved);
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

        public IActionResult Capture()
        {
            return View();
        }

        //To Authorize payment Manually...
        public IActionResult CapturePayment(string paymentId)
        {
            RazorpayClient client = new RazorpayClient(SD.PublishedKey, SD.SecretKey);
            Razorpay.Api.Payment payment = client.Payment.Fetch(paymentId);
            var amount = payment.Attributes["amount"];
            var currency = payment.Attributes["currency"];

            Dictionary<string,object> options = new Dictionary<string, object>();
            options.Add("amount", amount);
            options.Add("currency", currency);
            Razorpay.Api.Payment paymentCapture = payment.Capture(options);

            return View("Success");
        }

        public IActionResult PaymentFail()
        {
            return View();
        }

        private bool CompareSignature(string orderId,string paymentId,string signature)
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
            

        private string CreateOrder(OrderHeader orderHeader)
        {
            try
            {
                RazorpayClient razorpayClient = new RazorpayClient(SD.PublishedKey, SD.SecretKey);
                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("amount", (orderHeader.OrderTotal * 100).ToString()   );
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

        //public IActionResult Payment(int Id)
        //{

        //    //ClaimsIdentity claimsIdentity = (ClaimsIdentity?)User.Identity;
        //    //string userId = (claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier))?.Value;


        //    //OrderHeader orderHeader = _db.OrderHeader.Get(x => x.Id == Id);

           


        //    return View();
        //}

        //used for RazorPay payment
        //[HttpPost]
        //public IActionResult Payment(string razorpay_payment_id,string razorpay_order_id,string razorpay_signature)
        //{
        //   Dictionary<string,string> attributes = new Dictionary<string,string>();
        //    attributes.Add("razorpay_payment_id", razorpay_payment_id);
        //    attributes.Add("razorpay_order_id", razorpay_order_id);
        //    attributes.Add("razorpay_signature", razorpay_signature);

        //    Utils.verifyPaymentSignature(attributes);

        //    string transactionId = razorpay_payment_id;
        //    string orderId = razorpay_order_id;

        //    return View("OrderConfirmation");
        //}

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _db.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            //if(orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            //{
            //    //this is an order by customer
            //    var service = new SessionService();
            //    Session session = service.Get(orderHeader.SessionId);

            //    if(session.PaymentStatus.ToLower() == "paid")
            //    {
            //        _db.OrderHeader.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
            //        _db.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
            //        _db.Commit();
            //    }

            //}

            List<ShoppingCart> shoppingCarts = _db.ShoppingCart.GetAll(u => u.ApplicationUserId  == orderHeader.ApplicationUserId).ToList();
            _db.ShoppingCart.RemoveRange(shoppingCarts);
            _db.Commit();
            HttpContext.Session.Clear();
            return View(id);
        }

        public IActionResult Plus(int? cartId)
        {
            ShoppingCart cartFromDb = _db.ShoppingCart.Get(u => u.Id == cartId);
            cartFromDb.Count += 1;
            _db.ShoppingCart.Update(cartFromDb);
            _db.Commit();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int? cartId)
        {
            ShoppingCart cartFromDb = _db.ShoppingCart.Get(u => u.Id == cartId,tracked:true);
            if (cartFromDb.Count <= 1)
            {
                //remove from cart
                _db.ShoppingCart.Remove(cartFromDb);
                HttpContext.Session.SetInt32(SD.SessionCart, _db.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
            }
            else
            {
                //Decrease the cart count from 1 value
                cartFromDb.Count -= 1;
                _db.ShoppingCart.Update(cartFromDb);
            }


            _db.Commit();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int? cartId)
        {
            ShoppingCart cartFromDb = _db.ShoppingCart.Get(u => u.Id == cartId,tracked:true);
            HttpContext.Session.SetInt32(SD.SessionCart, _db.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
            _db.ShoppingCart.Remove(cartFromDb);
           
            _db.Commit();
            return RedirectToAction(nameof(Index));
        }


        private double GetCartPrice(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }

      
    }
}
