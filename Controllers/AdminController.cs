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
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region مدیریت اساتید
        public async Task<IActionResult> Professors()
        {
            var professors = await _context.Professors
                .Include(p => p.Faculty)
                .ToListAsync();
            return View(professors);
        }

        public IActionResult AddProfessor()
        {
            ViewBag.Faculties = _context.Faculties
            .Select(f => new SelectListItem
            {
                Value = f.Id.ToString(),
                Text = f.Name
            })
            .ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProfessor(Professor model)
        {
            if (ModelState.IsValid)
            {
                if (_context.Professors.Any(p => p.ProfessorId == model.ProfessorId))
                {
                    ModelState.AddModelError("ProfessorId", "کد استاد تکراری است");
                    ViewBag.Faculties = _context.Faculties.ToList();
                    return View(model);
                }

                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "ایمیل تکراری است");
                    ViewBag.Faculties = _context.Faculties.ToList();
                    return View(model);
                }

                model.Role = "Professor";
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Professors));
            }
            ViewBag.Faculties = _context.Faculties.ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProfessor(int id)
        {
            var professor = await _context.Professors.FindAsync(id);
            if (professor == null)
            {
                return NotFound();
            }

            var hasClasses = await _context.CourseProfessors
                .AnyAsync(cp => cp.ProfessorId == id);

            if (hasClasses)
            {
                TempData["ErrorMessage"] = "این استاد به کلاس‌هایی تخصیص داده شده و قابل حذف نیست";
                return RedirectToAction(nameof(Professors));
            }

            _context.Professors.Remove(professor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Professors));
        }
        #endregion

        #region مدیریت دانشجویان
        public async Task<IActionResult> Students()
        {
            var students = await _context.Students
                .Include(s => s.Faculty)
                .ToListAsync();
            return View(students);
        }

        public IActionResult AddStudent()
        {
            ViewBag.Faculties = new SelectList(_context.Faculties, "Id", "Name");
            ViewBag.Prerequisites = new MultiSelectList(_context.Courses, "Id", "Title");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(Student model)
        {
            if (_context.Students.Any(s => s.StudentId == model.StudentId))
            {
                ModelState.AddModelError("StudentId", "این شماره دانشجویی قبلا ثبت شده است");
            }

            if (ModelState.IsValid)
            {
                model.Role = "Student";
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Students));
            }

            ViewBag.Faculties = new SelectList(_context.Faculties, "Id", "Name");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            var hasCourses = await _context.CourseStudents
                .AnyAsync(cs => cs.StudentId == id);

            if (hasCourses)
            {
                TempData["ErrorMessage"] = "این دانشجو در کلاس‌هایی ثبت نام کرده و قابل حذف نیست";
                return RedirectToAction(nameof(Students));
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Students));
        }
        #endregion

        #region مدیریت دروس
        public async Task<IActionResult> Courses()
        {
            var courses = await _context.Courses
                .Include(c => c.Faculty)
                .ToListAsync();
            return View(courses);
        }

        public IActionResult AddCourse()
        {
            ViewBag.Faculties = _context.Faculties
                .Select(f => new SelectListItem
                {
                    Value = f.Id.ToString(),
                    Text = f.Name
                })
                .ToList();
            ViewBag.Prerequisites = _context.Courses
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Title} ({c.Code})"
                })
                .ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourse(Course model, int[] prerequisiteIds)
        {
            if (ModelState.IsValid)
            {
                if (_context.Courses.Any(c => c.Code == model.Code))
                {
                    ModelState.AddModelError("Code", "کد درس تکراری است");
                    ViewBag.Faculties = _context.Faculties.ToList();
                    ViewBag.Prerequisites = _context.Courses.ToList();
                    return View(model);
                }

                _context.Add(model);
                await _context.SaveChangesAsync();

                if (prerequisiteIds != null && prerequisiteIds.Length > 0)
                {
                    foreach (var preId in prerequisiteIds)
                    {
                        _context.Prerequisites.Add(new Prerequisite
                        {
                            CourseId = model.Id,
                            PrerequisiteCourseId = preId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Courses));
            }
            ViewBag.Faculties = _context.Faculties.ToList();
            ViewBag.Prerequisites = _context.Courses.ToList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                var course = await _context.Courses
                    .Include(c => c.Classes)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (course == null)
                {
                    return NotFound();
                }

                if (course.Classes.Any())
                {
                    TempData["ErrorMessage"] = "این درس دارای کلاس است و قابل حذف نیست";
                    return RedirectToAction(nameof(Courses));
                }

                var prerequisites = await _context.Prerequisites
                    .Where(p => p.CourseId == id)
                    .ToListAsync();
                _context.Prerequisites.RemoveRange(prerequisites);

                var isPrerequisiteFor = await _context.Prerequisites
                    .Where(p => p.PrerequisiteCourseId == id)
                    .ToListAsync();
                _context.Prerequisites.RemoveRange(isPrerequisiteFor);

                _context.Courses.Remove(course);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "درس با موفقیت حذف شد";
                return RedirectToAction(nameof(Courses));
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "امکان حذف این درس وجود ندارد. ابتدا وابستگی‌های آن را بررسی کنید.";
                return RedirectToAction(nameof(Courses));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "خطایی در حذف درس رخ داد";
                return RedirectToAction(nameof(Courses));
            }
        }
        #endregion

        #region مدیریت کلاس‌های درس
        public async Task<IActionResult> CourseClasses()
        {
            var classes = await _context.CourseClasses
                .Include(cc => cc.Course)
                .Include(cc => cc.Professors)
                    .ThenInclude(cp => cp.Professor)
                .ToListAsync();
            return View(classes);
        }

        public IActionResult AddClass()
        {
            ViewBag.Professors = _context.Professors
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.FirstName} {p.LastName}"
                })
                .ToList();
            ViewBag.Courses = _context.Courses
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Title} ({c.Code})"
                })
                .ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddClass(CourseClass model, int[] professorIds)
        {
            if (ModelState.IsValid)
            {
                var timeConflict = await _context.CourseClasses
                    .AnyAsync(cc => cc.Building == model.Building &&
                                   cc.RoomNumber == model.RoomNumber &&
                                   cc.Day == model.Day &&
                                   ((cc.StartTime <= model.StartTime && cc.EndTime > model.StartTime) ||
                                    (cc.StartTime < model.EndTime && cc.EndTime >= model.EndTime) ||
                                    (cc.StartTime >= model.StartTime && cc.EndTime <= model.EndTime)));

                if (timeConflict)
                {
                    ModelState.AddModelError("", "تداخل زمانی با کلاس دیگر در همین مکان وجود دارد");
                    ViewBag.Courses = _context.Courses
                        .Select(c => new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = $"{c.Title} ({c.Code})"
                        }).ToList();
                    ViewBag.Professors = _context.Professors
                        .Select(p => new SelectListItem
                        {
                            Value = p.Id.ToString(),
                            Text = $"{p.FirstName} {p.LastName}"
                        }).ToList();
                    return View(model);
                }
                _context.Add(model);
                await _context.SaveChangesAsync();
                if (professorIds != null && professorIds.Length > 0)
                {
                    foreach (var profId in professorIds)
                    {
                        _context.CourseProfessors.Add(new CourseProfessor
                        {
                            ProfessorId = profId,
                            CourseClassId = model.Id
                        });
                    }
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(CourseClasses));
            }
            ViewBag.Courses = _context.Courses
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.Title} ({c.Code})"
                }).ToList();
            ViewBag.Professors = _context.Professors
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.FirstName} {p.LastName}"
                }).ToList();
            return View(model);
        }

        public async Task<IActionResult> ChangeProfessor(int id)
        {
            var courseProfessor = await _context.CourseProfessors
                .Include(cc => cc.CourseClass.Course)
                .Include(cc => cc.Professor)
                .FirstOrDefaultAsync(cc => cc.CourseClass.Id == id);
            ViewBag.Professors = _context.Professors
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.FirstName} {p.LastName}"
                })
                .ToList();
            return View(courseProfessor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeProfessor(CourseProfessor model, int[] professorIds)
        {
            if (professorIds != null && professorIds.Length > 0)
            {
                foreach (var profId in professorIds)
                {
                    var courseprofessor = await _context.CourseProfessors.FindAsync(model.ProfessorId, model.CourseClassId);
                    _context.CourseProfessors.Remove(courseprofessor);
                    _context.CourseProfessors.Add(new CourseProfessor
                    {
                        ProfessorId = profId,
                        CourseClassId = model.CourseClassId
                    });
                    ViewBag.ProfId = profId;
                    ViewBag.courseclassid = model.CourseClassId;
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ChangeProfessor));
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudentToClass(int classId, int studentId)
        {
            var courseClass = await _context.CourseClasses
                .Include(cc => cc.Course)
                .ThenInclude(c => c.Prerequisites)
                .FirstOrDefaultAsync(cc => cc.Id == classId);

            if (courseClass == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
            {
                return NotFound();
            }

            foreach (var prerequisite in courseClass.Course.Prerequisites)
            {
                var hasPassed = await _context.CourseStudents
                    .AnyAsync(cs => cs.StudentId == studentId &&
                                   cs.CourseClass.CourseId == prerequisite.PrerequisiteCourseId &&
                                   cs.Grade >= 10);

                if (!hasPassed)
                {
                    var preCourse = await _context.Courses.FindAsync(prerequisite.PrerequisiteCourseId);
                    TempData["ErrorMessage"] = $"دانشجو پیش‌نیاز {preCourse.Title} را پاس نکرده است";
                    return RedirectToAction(nameof(ManageClassStudents), new { id = classId });
                }
            }

            var studentClasses = await _context.CourseStudents
                .Include(cs => cs.CourseClass)
                .Where(cs => cs.StudentId == studentId)
                .ToListAsync();

            foreach (var sc in studentClasses)
            {
                if (sc.CourseClass.Day == courseClass.Day &&
                    ((sc.CourseClass.StartTime <= courseClass.StartTime && sc.CourseClass.EndTime > courseClass.StartTime) ||
                     (sc.CourseClass.StartTime < courseClass.EndTime && sc.CourseClass.EndTime >= courseClass.EndTime) ||
                     (sc.CourseClass.StartTime >= courseClass.StartTime && sc.CourseClass.EndTime <= courseClass.EndTime)))
                {
                    TempData["ErrorMessage"] = "تداخل زمانی با کلاس دیگری که دانشجو در آن ثبت نام کرده است";
                    return RedirectToAction(nameof(ManageClassStudents), new { id = classId });
                }
            }

            var currentStudentsCount = await _context.CourseStudents
                .CountAsync(cs => cs.CourseClassId == classId);

            if (currentStudentsCount >= courseClass.Capacity)
            {
                TempData["ErrorMessage"] = "ظرفیت کلاس تکمیل شده است";
                return RedirectToAction(nameof(ManageClassStudents), new { id = classId });
            }

            _context.CourseStudents.Add(new CourseStudent
            {
                StudentId = studentId,
                CourseClassId = classId
            });
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageClassStudents), new { id = classId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClass(int id)
        {
            var classProfessors = await _context.CourseProfessors
                .Where(cp => cp.CourseClassId == id)
                .ToListAsync();
            _context.CourseProfessors.RemoveRange(classProfessors);

            var classStudents = await _context.CourseStudents
                .Where(cs => cs.CourseClassId == id)
                .ToListAsync();
            _context.CourseStudents.RemoveRange(classStudents);

            var courseClass = await _context.CourseClasses.FindAsync(id);
            if (courseClass != null)
            {
                _context.CourseClasses.Remove(courseClass);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(CourseClasses));
        }
        #endregion

        #region مدیریت دانشکده‌ها
        public async Task<IActionResult> Faculties()
        {
            var faculties = await _context.Faculties.ToListAsync();
            return View(faculties);
        }

        public IActionResult AddFaculty()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFaculty(Faculty model)
        {
            if (ModelState.IsValid)
            {
                if (_context.Faculties.Any(f => f.Name == model.Name))
                {
                    ModelState.AddModelError("Name", "نام دانشکده تکراری است");
                    return View(model);
                }

                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Faculties));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFaculty(int id)
        {
            var faculty = await _context.Faculties.FindAsync(id);
            if (faculty == null)
            {
                return NotFound();
            }

            var hasCourses = await _context.Courses.AnyAsync(c => c.FacultyId == id);
            var hasProfessors = await _context.Professors.AnyAsync(p => p.FacultyId == id);
            var hasStudents = await _context.Students.AnyAsync(s => s.FacultyId == id);

            if (hasCourses || hasProfessors || hasStudents)
            {
                TempData["ErrorMessage"] = "این دانشکده مورد استفاده است و قابل حذف نیست";
                return RedirectToAction(nameof(Faculties));
            }

            _context.Faculties.Remove(faculty);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Faculties));
        }
        #endregion
    }
}