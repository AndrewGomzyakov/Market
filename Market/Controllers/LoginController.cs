using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Market.Context;
using Market.Models;
using Market.ServiceInterfaces;
using Market.Views.Login;
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
        private readonly ISmsProvider _smsProvider;
        private const string UserNameClaimType = "UserName";
        private const string UserIdClaimType = "UserId";
        private const string UserRoleClaimType = "UserRole";

        public LoginController(MarketContext context, IHttpContextAccessor httpContextAccessor, ISmsProvider smsProvider)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _smsProvider = smsProvider;
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
                if (user == null)
                {
                    return View(new LoginModel {ErrorMessage = "Неверный Логин или Пароль"});
                }

                if (user.Status != UserStatus.Approved)
                {
                    return Redirect($"/User/ConfirmPhone?phone={login}");
                }
                var authClaims = new List<Claim>();
                authClaims.Add(new Claim(UserIdClaimType, user.Id.ToString(), ClaimValueTypes.String));
                authClaims.Add(new Claim(UserRoleClaimType, user.Role.ToString(), ClaimValueTypes.String));
                authClaims.Add(new Claim(UserNameClaimType, user.FirstName, ClaimValueTypes.String));
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(
                    new ClaimsIdentity(authClaims, "testClaims")));

                return Redirect("/");
                
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
        public async Task<IActionResult> Registry(string login, string password, string firstName, string secondName)
        {
            var newUser = new DbUser
            {
                Login = login,
                Password = password,
                FirstName = firstName,
                SecondName = secondName,
                Role = UserRole.Customer,
                Status = UserStatus.NotApproved
            };
            var user = _context.Users.FirstOrDefault(x => x.Login == login);
            if (user != null && user.Status == UserStatus.Approved)
            {
                return View(new RegistryModel {ErrorMessage = "Этот логин занят. Придумайте новый логин."});
            }

            var isPhone = Regex.Match(login, "7\\d{10}");
            if (!isPhone.Success)
            {
                return View(new RegistryModel {ErrorMessage = "Введите номер в формате \"79XXXXXXXXX\""});
            }
            var randomGenerator = new Random();
            var code = randomGenerator.Next(0, 1000000).ToString("000000");
            newUser.Code = code;
            if (user != null)
            {
                user.Login = login;
                user.Password = password;
                user.FirstName = firstName;
                user.SecondName = secondName;
                user.Role = UserRole.Customer;
                user.Status = UserStatus.NotApproved;
                user.Code = code;
                _context.Users.Update(user);
            }
            else
            {
                _context.Users.Add(newUser);
            }
            await _context.SaveChangesAsync();
            await _smsProvider.SendMessage(login, code);
            return Redirect($"/User/ConfirmPhone?phone={login}");
            
        }

        [HttpGet("ConfirmPhone")]
        public IActionResult ConfirmPhone([FromQuery] string phone)
        {
            return View(new PhoneConfirmModel{Login = phone});
        }

        [HttpPost("ConfirmPhone")]
        public async Task<IActionResult> ConfirmPhone(string login, string code)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Login == login);
            if (user == null)
            {
                return View(new PhoneConfirmModel {ErrorMessage = "Пользователя с данным номером не существует."});
            }
            
            if (user.Code != code)
            {
                return View(new PhoneConfirmModel {ErrorMessage = "Неверный код"});
            }

            user.Status = UserStatus.Approved;
            _context.Update(user);
            await _context.SaveChangesAsync();
            
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