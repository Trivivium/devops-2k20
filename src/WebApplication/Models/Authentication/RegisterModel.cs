﻿using System;
namespace WebApplication.Models.Authentication
{
    public class RegisterModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string RepeatedPassword { get; set; }
        public string PasswordHash { get; set; }
    }
}
