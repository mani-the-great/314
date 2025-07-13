using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GolestanSystem.Data;
using GolestanSystem.Models;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
namespace GolestanSystem.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {
        private readonly ApplicationDbContext _context;
        public async Task<IActionResult> Index()
        {
            var classes = await _context.CourseClasses
                .Include(cc => cc.Course)
                .Include(cc => cc.Professors)
                    .ThenInclude(cp => cp.Professor)
                .ToListAsync();
            ViewData["profID"] = HttpContext.Session.GetString("profID");
            
            return View(classes);
        }
        public async Task<IActionResult> ManageClassStudents(int id)
        {
            var courseClass = await _context.CourseClasses
                .Include(cc => cc.Course)
                .Include(cc => cc.Students)
                    .ThenInclude(cs => cs.Student)
                .FirstOrDefaultAsync(cc => cc.Id == id);

            if (courseClass == null)
            {
                return NotFound();
            }

            var availableStudents = await _context.Students
                .Where(s => !s.CourseStudents.Any(cs => cs.CourseClassId == id))
                .ToListAsync();

            ViewBag.AvailableStudents = availableStudents;
            return View(courseClass);
        }
        public ProfessorController(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}
