﻿using System.ComponentModel.DataAnnotations;

namespace Airline.Models.Login
{
    public class ResetPassWordViewModel
    {
        [Required]
        public string UserName { get; set; }


        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }


        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }


        [Required]
        public string Token { get; set; }
    }
}
