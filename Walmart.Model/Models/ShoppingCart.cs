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
    public class ShoppingCart
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }

        [ValidateNever]
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [Range(1,1000,ErrorMessage ="Count should be between 1 and 1000")]
        public int Count { get; set; }

        public string? ApplicationUserId {  get; set; }

        [ValidateNever]
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser? ApplicationUser { get; set; }

        [NotMapped] // This will not mapped the property with db.
        public double Price { get; set; }
    }
}
