using ContosoUniversity_MVC.Data;
using ContosoUniversity_MVC.Models;
using ContosoUniversity_MVC.Models.SchoolViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
        public async Task<ActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            Instructor instructor = await _context.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments).ThenInclude(a => a.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.ID == id);

            return View(instructor);
        }

        // GET: InstructorsController/Create
        public ActionResult Create()
        {
            Instructor instructor = new()
            {
                CourseAssignments = new List<CourseAssignment>()
            };

            PopulateAssignedCourseData(instructor);

            return View();
        }

        // POST: InstructorsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("FirstMidName, HireDate, LastName, OfficeAssignment")] Instructor instructor,
            string[] selectedCourses)
        {
            if (selectedCourses is not null)
            {
                instructor.CourseAssignments = new List<CourseAssignment>();
                foreach (var courseID in selectedCourses)
                {
                    var courseToAdd = new CourseAssignment
                    {
                        InstructorID = instructor.ID,
                        CourseID = int.Parse(courseID)
                    };
                    instructor.CourseAssignments.Add(courseToAdd);
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(instructor);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        // GET: InstructorsController/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            Instructor instructor = await _context.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments).ThenInclude(a => a.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.ID == id);
            if (instructor is null)
            {
                return NotFound();
            }

            PopulateAssignedCourseData(instructor);
            return View(instructor);
        }

        // POST: InstructorsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int? id, string[] selectedCourses)
        {
            if (id is null)
            {
                return NotFound();
            }

            Instructor instructorToUpdate = await _context.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments).ThenInclude(a => a.Course)
                .FirstOrDefaultAsync(i => i.ID == id);

            if (await TryUpdateModelAsync<Instructor>(instructorToUpdate,
                        "",
                        i => i.FirstMidName, i => i.LastName, i => i.HireDate, i => i.OfficeAssignment))
            {
                if (string.IsNullOrWhiteSpace(instructorToUpdate.OfficeAssignment?.Location))
                {
                    instructorToUpdate.OfficeAssignment = null;
                }

                UpdateInstructorCourses(selectedCourses, instructorToUpdate);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");

                    PopulateAssignedCourseData(instructorToUpdate);
                    return View(instructorToUpdate);
                }

                return RedirectToAction(nameof(Index));
            }

            PopulateAssignedCourseData(instructorToUpdate);
            return View(instructorToUpdate);
        }

        // GET: InstructorsController/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            Instructor instructor = await _context.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments).ThenInclude(a => a.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.ID == id);

            return View(instructor);
        }

        // POST: InstructorsController/Delete/5
        [HttpPost, ActionName(nameof(Delete))]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Instructor instructor = await _context.Instructors
                 .Include(i => i.CourseAssignments)
                 .SingleAsync(i => i.ID == id);

            var departments = await _context.Departments
                .Where(d => d.InstructorID == id)
                .ToListAsync();
            departments.ForEach(d => d.InstructorID = null);

            _context.Instructors.Remove(instructor);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private void PopulateAssignedCourseData(Instructor instructor)
        {
            var allCourses = _context.Courses;
            var instructorCourses = new HashSet<int>(
                instructor.CourseAssignments.Select(a => a.CourseID));
            var viewModel = new List<AssignedCourseData>();
            foreach (var course in allCourses)
            {
                viewModel.Add(new AssignedCourseData
                {
                    CourseID = course.CourseID,
                    Title = course.Title,
                    Assigned = instructorCourses.Contains(course.CourseID)
                });
            }
            ViewData["Courses"] = viewModel;
        }

        private void UpdateInstructorCourses(string[] selectedCourses, Instructor instructorToUpdate)
        {
            if (selectedCourses is null)
            {
                instructorToUpdate.CourseAssignments = new List<CourseAssignment>();
                return;
            }

            HashSet<string> selectedCoursesHS = new(selectedCourses);
            HashSet<int> instructorCourses = new(instructorToUpdate.CourseAssignments.Select(a => a.CourseID));

            foreach (var course in _context.Courses)
            {
                if (selectedCoursesHS.Contains(course.CourseID.ToString()))
                {
                    if (!instructorCourses.Contains(course.CourseID))
                    {
                        instructorToUpdate.CourseAssignments.Add(new CourseAssignment
                        {
                            InstructorID = instructorToUpdate.ID,
                            CourseID = course.CourseID
                        });
                    }
                }
                else
                {
                    if (instructorCourses.Contains(course.CourseID))
                    {
                        CourseAssignment courseToRemove = instructorToUpdate.CourseAssignments
                            .FirstOrDefault(a => a.CourseID == course.CourseID);
                        _context.Remove(courseToRemove);
                    }
                }
            }
        }
    }
}
