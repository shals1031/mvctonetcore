using System;
using System.ComponentModel.DataAnnotations;

namespace TeliconLatest.Models
{
    public class Settings
    {
        [Required]
        public string Name { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Parish { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Fax { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string GctReg { get; set; }
        [Required]
        public double DefaultTaxRate { get; set; }
        [Required]
        public int TeamMax { get; set; }
        [Range(300, 1200)]
        [Required]
        public int TimeOut { get; set; }
    }
}