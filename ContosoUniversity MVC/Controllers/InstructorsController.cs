using ContosoUniversity_MVC.Data;
using ContosoUniversity_MVC.Models;
using ContosoUniversity_MVC.Models.SchoolViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoUniversity_MVC.Controllers
{
    public class InstructorsController : Controller
    {
        private readonly SchoolContext _context;

        public InstructorsController(SchoolContext context)
        {
            _context = context;
        }
        // GET: InstructorsController
        // Index action using Eeager Loagind:
        //public async Task<ActionResult> Index(int? id, int? courseID)
        //{
        //    InstructorIndexData viewModel = new();
        //    viewModel.Instructors = await _context.Instructors
        //        .Include(i => i.OfficeAssignment)
        //        .Include(i => i.CourseAssignments)
        //           .ThenInclude(a => a.Course)
        //              .ThenInclude(c => c.Enrollments)
        //                 .ThenInclude(e => e.Student)
        //        .Include(i => i.CourseAssignments)
        //           .ThenInclude(a => a.Course)
        //              .ThenInclude(c => c.Department)
        //        .AsNoTracking()
        //        .ToListAsync();

        //    if (id is not null)
        //    {
        //        ViewData[nameof(CourseAssignment.InstructorID)] = id.Value;
        //        Instructor instructor = viewModel.Instructors.Where(
        //            i => i.ID == id.Value).Single();
        //        viewModel.Courses = instructor.CourseAssignments.Select(s => s.Course);
        //    }
        //    if (courseID is not null)
        //    {
        //        ViewData["CourseID"] = courseID.Value;
        //        viewModel.Enrollments = viewModel.Courses.Where(
        //            c => c.CourseID == courseID).Single().Enrollments;
        //    }

        //    return View(viewModel);
        //}

        /* Index actio using Explicit loading*/
        public async Task<IActionResult> Index(int? id, int? courseID)
        {
            InstructorIndexData viewModel = new();
            viewModel.Instructors = await _context.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments)
                   .ThenInclude(a => a.Course)
                      .ThenInclude(c => c.Department)
                 .OrderBy(i => i.LastName)
                 .ToListAsync();
            if (id is not null)
            {
                ViewData["InstructorID"] = id.Value;
                Instructor instructor = viewModel.Instructors
                    .Single(i => i.ID == id);
                viewModel.Courses = instructor.CourseAssignments
                    .Select(s => s.Course);
            }

            if (courseID is not null)
            {
                ViewData["CourseID"] = courseID.Value;
                Course selectedCourse = viewModel.Courses
                    .Single(c => c.CourseID == courseID);
                await _context.Entry(selectedCourse)
                    .Collection(c => c.Enrollments)
                    .LoadAsync();
                foreach (Enrollment enrollment in selectedCourse.Enrollments)
                {
                    await _context.Entry(enrollment).Reference(e => e.Student)
                        .LoadAsync();
                }
                viewModel.Enrollments = selectedCourse.Enrollments;
            }

            return View(viewModel);
        }
        /* GET: InstructorsController/Details/5 */
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: InstructorsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: InstructorsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: InstructorsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: InstructorsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: InstructorsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: InstructorsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
