using Razorpay.Api;
using System;
using System.Net;
using Walmart.Model.Models;

namespace Walmart.Utility
{
    public class RazorpayService
    {
        private readonly string _keyId = SD.PublishedKey; // Replace with your Razorpay Key ID
        private readonly string _keySecret = SD.SecretKey; // Replace with your Razorpay Key Secret

        public RazorpayService()
        {
            // Initialize Razorpay Client
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            RazorpayClient = new RazorpayClient(_keyId, _keySecret);
        }

        private RazorpayClient RazorpayClient { get; }

        public Refund RefundPayment(string paymentId, double amount)
        {
            try
            {
                // Fetch the payment object using the payment ID
                var payment = RazorpayClient.Payment.Fetch(paymentId);

                Dictionary<string, object> options = new Dictionary<string, object>();
                options.Add("amount", (amount * 100 ).ToString());
               

                // Initiate the refund (amount is in paise)
                var refund = payment.Refund(options);  // Amount in paise

                return refund;
            }
            catch (Exception ex)
            {
                throw new Exception("Error processing refund: " + ex.Message);
            }
        }
    }
}