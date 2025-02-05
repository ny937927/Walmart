using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Walmart.Model.Models
{
    public class OrderHeader
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }

        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime ShippingDate { get; set; }

        public double OrderTotal {  get; set; }

        public string? OrderStatus { get; set; }

        public string? PaymentStatus {  get; set; }

        public string? TrackingNumber { get; set; }
        public string? Carrier { get; set; }

        public DateTime PaymentDate { get; set; }

        public DateOnly PaymentDueDate { get; set; } // DateOnly & TimeOnly is the new data type introduce in.NET 8, it will convert to Date and Time  type only in db.

        public string? SessionId {  get; set; } // When user click on Place order, one session id get generated and look for PaymentIntentId, if we get this id within some time it means payment is successfull.

        public string? PaymentIntentId {  get; set; } // If payment is successfull then, we will be getting this Id.

        [Required]
        public string? Name { get; set; }
        [Required]
        public string? StreetAddress { get; set; }
        [Required]
        public string? City { get; set; }
        [Required]
        public string? State { get; set; }
        [Required]
        public string? PostalCode { get; set; }
        [Required]
        public string? PhoneNumber { get; set; }

    }
}
