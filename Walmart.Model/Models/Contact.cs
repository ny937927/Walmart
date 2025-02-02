using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Walmart.Model.Models
{
    public class Contact
    {

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string Name { get; set; }

        [Required]
        [MaxLength(30)]
        public string EmailId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; }
    }
}
