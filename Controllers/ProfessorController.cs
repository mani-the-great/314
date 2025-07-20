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
            HttpContext.Session.SetString("ccID", id.ToString());
            return View(courseClass);
        }

        public async Task<IActionResult> SubmitStudentScore(int id)
        {
            ViewData["ccID"] = HttpContext.Session.GetString("ccID");
            var coursestudent = await _context.CourseStudents
            .Include(cs => cs.Student)
            .Include(cs => cs.CourseClass.Course)
            .FirstOrDefaultAsync(cs => cs.StudentId == id && cs.CourseClassId.ToString() == ViewData["ccID"].ToString());
            if (coursestudent == null)
            {
                return NotFound();
            }
            return View(coursestudent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitStudentScore(CourseStudent model)
        {
            if (ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(SubmitStudentScore));
        }

        public async Task<IActionResult> RemoveStudentFromClass(int classId, int studentId)
        {
            var enrollment = await _context.CourseStudents
                .FirstOrDefaultAsync(cs => cs.StudentId == studentId && cs.CourseClassId == classId);

            if (enrollment != null)
            {
                _context.CourseStudents.Remove(enrollment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ManageClassStudents), new { id = classId });
        }

        public ProfessorController(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}
