using GolestanSystem.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public IActionResult Login(string returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password, string returnUrl)
    {
        if (IsValidUser(username, password))
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
            return LocalRedirect(returnUrl ?? "/Admin");
        }
        else
        {
            var professors = await _context.Professors.Include(p => p.Faculty).ToListAsync();
            for (int i=0; i < _context.Professors.Count(); i++)
            {
                if(professors[i].PasswordHash==password && professors[i].Email == username)
                {
                    var claims = new List<Claim>
                    {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "Professor")
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity));
                    HttpContext.Session.SetString("profID", professors[i].ProfessorId.ToString());
                    return LocalRedirect(returnUrl ?? "/Professor");
                }
            }
        }

        ModelState.AddModelError(string.Empty, "نام کاربری یا رمز عبور نادرست است");
        ViewData["Login"] = "Failed";
        return View();
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> AccessDenied()
    {
        return View();
    }

    private bool IsValidUser(string username, string password)
    {
        return username == "admin" && password == "password";
    }
}
