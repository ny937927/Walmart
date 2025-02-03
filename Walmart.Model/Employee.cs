using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Walmart.Model
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name="Employee Name")]
        public string Name { get; set; }

        [Required]
        public string StreetAddress {  get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string PostalCode { get; set; }
        [Required]
        public double Salary {  get; set; }

        [Required]
        public DateOnly DOB { get; set; }
    }
}
