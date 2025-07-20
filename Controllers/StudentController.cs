using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GolestanSystem.Data;
using GolestanSystem.Models;
using GolestanSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace GolestanSystem.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var studentId = GetCurrentStudentId();
            var student = _context.Students
                .Include(s => s.Faculty)
                .FirstOrDefault(s => s.Id == studentId);

            return View(student);
        }

        public async Task<IActionResult> MyCourses()
        {
            var studentId = GetCurrentStudentId();

            var courses = await _context.CourseStudents
                .Include(cs => cs.CourseClass)
                    .ThenInclude(cc => cc.Course)
                .Include(cs => cs.CourseClass)
                    .ThenInclude(cc => cc.Professors!)
                    .ThenInclude(cp => cp.Professor)
                .Where(cs => cs.StudentId == studentId)
                .Select(cs => new StudentCourseViewModel
                {
                    CourseTitle = cs.CourseClass.Course.Title,
                    CourseCode = cs.CourseClass.Course.Code,
                    ClassTime = $"{cs.CourseClass.Day} {cs.CourseClass.StartTime}-{cs.CourseClass.EndTime}",
                    ExamTime = cs.CourseClass.Course.ExamTime,
                    Professors = string.Join(", ", cs.CourseClass.Professors!
                        .Select(p => $"{p.Professor!.FirstName} {p.Professor!.LastName}")),
                    CourseClassId = cs.CourseClassId
                })
                .ToListAsync();

            return View(courses);
        }

        public async Task<IActionResult> MyGrades()
        {
            var studentId = GetCurrentStudentId();

            var grades = await _context.CourseStudents
                .Include(cs => cs.CourseClass)
                    .ThenInclude(cc => cc.Course)
                .Where(cs => cs.StudentId == studentId && cs.Grade.HasValue)
                .Select(cs => new StudentGradeViewModel
                {
                    CourseTitle = cs.CourseClass.Course.Title,
                    CourseCode = cs.CourseClass.Course.Code,
                    Credits = cs.CourseClass.Course.Credits,
                    Grade = cs.Grade.Value,
                    IsPassed = cs.Grade >= 10
                })
                .ToListAsync();

            var passedCourses = grades.Where(g => g.IsPassed).ToList();
            ViewBag.OverallGPA = grades.Any() ? grades.Average(g => (double)g.Grade) : (double?)null;
            ViewBag.PassedCoursesCount = passedCourses.Count;

            return View(grades);
        }

        public async Task<IActionResult> MyInfo()
        {
            var studentId = GetCurrentStudentId();

            var student = await _context.Students
                .Include(s => s.Faculty)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
            {
                return NotFound();
            }

            var model = new StudentInfoViewModel
            {
                StudentId = student.StudentId,
                FullName = $"{student.FirstName} {student.LastName}",
                Email = student.Email,
                PhoneNumber = student.PhoneNumber,
                EntryDate = student.EntryDate.ToString("yyyy/MM/dd"),
                FacultyName = student.Faculty?.Name ?? "تعیین نشده",
                RegisterDate = student.RegisterDate.ToString("yyyy/MM/dd")
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DropCourse(int courseClassId)
        {
            var studentId = GetCurrentStudentId();

            var enrollment = await _context.CourseStudents
                .FirstOrDefaultAsync(cs => cs.StudentId == studentId && cs.CourseClassId == courseClassId);

            if (enrollment == null)
            {
                return NotFound();
            }

            _context.CourseStudents.Remove(enrollment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "لغو ثبت‌نام با موفقیت انجام شد";
            return RedirectToAction(nameof(MyCourses));
        }

        private int GetCurrentStudentId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}