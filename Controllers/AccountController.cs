using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


public class AccountController : Controller
{
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

        var professor = await _context.Professors
            .FirstOrDefaultAsync(p =>
                (p.ProfessorId == username || p.Email == username) &&
                p.PasswordHash == password);

        if (professor != null)
        {
            var professorClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, professor.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{professor.FirstName} {professor.LastName}"),
                new Claim(ClaimTypes.Role, "Professor"),
                new Claim("ProfessorId", professor.ProfessorId)
            };

            var professorIdentity = new ClaimsIdentity(professorClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(new ClaimsPrincipal(professorIdentity));

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
            return LocalRedirect(returnUrl ?? "/");
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

    private bool IsValidUser(string username, string password)
    {
        return username == "admin" && password == "password";
    }
}