using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GolestanSystem.Data;
using GolestanSystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password, string returnUrl = null)
    {
        if (username == "admin" && password == "password")
        {
            var adminClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "0"),
                new Claim(ClaimTypes.Name, "مدیر سیستم"),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var adminIdentity = new ClaimsIdentity(adminClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(new ClaimsPrincipal(adminIdentity));

            return RedirectToAction("Index", "Admin");
        }

        var student = await _context.Students
            .FirstOrDefaultAsync(s =>
                (s.StudentId == username || s.Email == username) &&
                s.PasswordHash == password);

        if (student != null)
        {
            var studentClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, student.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{student.FirstName} {student.LastName}"),
                new Claim(ClaimTypes.Role, "Student"),
                new Claim("StudentId", student.StudentId)
            };

            var studentIdentity = new ClaimsIdentity(studentClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(new ClaimsPrincipal(studentIdentity));

            return RedirectToAction("Index", "Student");
        }

        var professors = await _context.Professors.Include(p => p.Faculty).ToListAsync();
        for (int i = 0; i < _context.Professors.Count(); i++)
        {
            if (professors[i].PasswordHash == password && professors[i].Email == username)
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

        ViewData["Login"] = "Failed";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    private IActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }
}