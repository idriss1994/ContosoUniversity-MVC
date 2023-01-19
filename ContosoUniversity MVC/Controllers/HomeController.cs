using ContosoUniversity_MVC.Data;
using ContosoUniversity_MVC.Models;
using ContosoUniversity_MVC.Models.SchoolViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoUniversity_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SchoolContext _context;

        public HomeController(ILogger<HomeController> logger,
            SchoolContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // About action using LINQ to get Data from Database:
        //public async Task<IActionResult> About()
        //{
        //    IQueryable<EnrollmentDateGroup> data = from student in _context.Students
        //                                           group student by student.EnrollmentDate
        //                                           into dateGroup
        //                                           select new EnrollmentDateGroup()
        //                                           {
        //                                               EnrollmentDate = dateGroup.Key,
        //                                               StudentCount = dateGroup.Count()

        //                                           };

        //    return View(await data.AsNoTracking().ToListAsync());
        //}


        /* About action using ADO.NET to get Data from Database */
        public async Task<IActionResult> About()
        {
            List<EnrollmentDateGroup> groups = new();
            DbConnection conn = _context.Database.GetDbConnection();

            try
            {
                await conn.OpenAsync();
                using DbCommand command = conn.CreateCommand();

                string query = "SELECT EnrollmentDate, COUNT(*) AS StudentCount "
                    + "FROM Person "
                    + "WHERE Discriminator = 'Student' "
                    + "GROUP BY EnrollmentDate";

                command.CommandText = query;
                DbDataReader reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        var row = new EnrollmentDateGroup
                        {
                            EnrollmentDate = reader.GetDateTime(0),
                            StudentCount = reader.GetInt32(1)
                        };
                        groups.Add(row);
                    }
                }

                reader.Dispose();
            }
            finally
            {
                conn.Close();
            }

            return View(groups);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
