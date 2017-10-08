﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private HubContext _context;
        public HomeController()
        {
            _context = new HubContext();
        }

        public IActionResult Index()
        {
            return Redirect("/SignIn");
        }

        [Route("/Forbidden")]
        public IActionResult Forbidden()
        {
            return View("~/Views/Home/Forbidden.cshtml");
        }

        [Route("/SignIn")]
        public IActionResult SignIn()
        {
            return View("~/Views/Home/SignIn.cshtml");
        }

        [HttpPost]
        [Route("/SignIn")]
        public async Task<IActionResult> ProcessSignInAsync(string emailLogin, string passwordLogin, string ip1, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!string.IsNullOrWhiteSpace(emailLogin))
            {
                var account = _context.Accounts.Where(x => x.email == emailLogin).First();

                //Check If Password Exists
                if (account.password != GenerateHashedPassword(passwordLogin, GetBytes(account.salt)))
                    return View("~/Views/SignInError.cshtml", "error");

                //If Account Exists, Create Principal Object For Authenticated User
                var claims = new List<Claim>
                {
                    new Claim("sub", account.Id.ToString()),
                    new Claim("given_name", account.firstname + " " + account.lastname),
                    new Claim("role", account.usertype)
                };

                var id = new ClaimsIdentity(claims, "password");
                var p = new ClaimsPrincipal(id);

                await HttpContext.SignInAsync("MyCookieAuthenticationScheme", p);

                if (!string.IsNullOrWhiteSpace(returnUrl)) return LocalRedirect(returnUrl);
                else if (account.usertype == "Student") return LocalRedirect("/Student");
                else return LocalRedirect("/Admin");
            }

            return View("~/Views/SignInError.cshtml", "error");
        }

        public byte[] GenerateSalt()
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return salt;
        }

        public string GenerateHashedPassword(string password, byte[] salt)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));
            return hashed;
        }

        public static byte[] GetBytes(string str)
        {
            return Convert.FromBase64String(str);
        }

        public static string GetString(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }
    }
}