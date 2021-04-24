using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Market.Context;
using Market.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Market.Controllers
{
    [Route("User")]
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly MarketContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string UserNameClaimType = "UserName";
        private const string UserIdClaimType = "UserId";
        private const string UserRoleClaimType = "UserRole";

        public LoginController(MarketContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("GetRegistry")]
        public async Task<IActionResult> GetRegistry()
        {
            var currentUserId =
                Guid.Parse(_httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == "UserId").Value);
            var users = await _context.Users.Where(x => x.Id != currentUserId).ToArrayAsync();
            return View(users);
        }

        [HttpGet("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/User/Login");
        }

        [HttpGet("SetRole")]
        public async Task<IActionResult> SetRole([FromQuery]Guid userId, [FromQuery]int role)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user != null)
            {
                user.Role = (UserRole) role;
                _context.Update(user);
                await _context.SaveChangesAsync();
            }

            return Redirect("/User/GetRegistry");
        }
        
        [HttpGet("Login")]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost("Login")]
        public IActionResult Login(string login, string password)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(x => x.Login == login && x.Password == password);
                if (user != null)
                {
                    var authClaims = new List<Claim>();
                    authClaims.Add(new Claim(UserIdClaimType, user.Id.ToString(), ClaimValueTypes.String));
                    authClaims.Add(new Claim(UserRoleClaimType, user.Role.ToString(), ClaimValueTypes.String));
                    authClaims.Add(new Claim(UserNameClaimType, user.FirstName, ClaimValueTypes.String));
                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(
                        new ClaimsIdentity(authClaims, "testClaims")));

                    return Redirect("/");
                }

                return View(new LoginModel {ErrorMessage = "Неверный Логин или Пароль"});
            }
            catch (ArgumentNullException ex)
            {
                return View(new LoginModel {ErrorMessage = "Логин или Пароль не могут быть пустыми"});
            }
        }

        [HttpGet("Registry")]
        public IActionResult Registry()
        {
            return View();
        }
        
        [HttpPost("Registry")]
        public IActionResult Registry(string login, string password, string firstName, string secondName)
        {
            var newUser = new DbUser
            {
                Login = login,
                Password = password,
                FirstName = firstName,
                SecondName = secondName,
                Role = UserRole.Customer
            };
            var user = _context.Users.FirstOrDefault(x => x.Login == login);
            if (user != null)
            {
                return View(new RegistryModel {ErrorMessage = "Этот логин занят. Придумайте новый логин."});
            }
            _context.Users.Add(newUser);
            _context.SaveChanges();
            return Redirect("/User/Login");
            
        }

        [HttpGet("GetUserBookings")]
        public async Task<IActionResult> GetUserBookings()
        {
            var currentUserId = Guid.Parse(
                _httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == "UserId").Value);
            DbBooking[] bookings = new DbBooking[0];
            if (_httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == "UserRole").Value == "Manager")
            {
                bookings = await _context.Bookings.Where(x => x.OwnerUserId == currentUserId && x.Count > 0).ToArrayAsync();
                
            }

            if (_httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == "UserRole").Value == "Customer")
            {
                bookings = await _context.Bookings.Where(x => x.BookerUserId == currentUserId && x.Count > 0).ToArrayAsync();
            }

            return View(bookings);
        }
    }
}