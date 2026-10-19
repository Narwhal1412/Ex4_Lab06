using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using Microsoft.Reporting.WebForms;

namespace Ex4_Lab06.Controllers
{
    public class ReportController : Controller
    {
        public ActionResult Index()
        {
            var courses = new List<SelectListItem>();

            // Copy the exact name from the <connectionStrings> section of Web.config
            string cs = ConfigurationManager.ConnectionStrings["SchoolDBConnectionString"].ConnectionString;

            using (var conn = new SqlConnection(cs))
            using (var cmd = new SqlCommand("SELECT CourseID, Title FROM Courses", conn))
            {
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        courses.Add(new SelectListItem
                        {
                            Value = r.GetInt32(0).ToString(),
                            Text = r.GetString(1)
                        });
                    }
                }
            }

            ViewBag.Courses = new SelectList(courses, "Value", "Text");
            return View();
        }

        [HttpPost]
        public ActionResult Generate(int selectedCourseID, string action)
        {
            // The adapter that runs your SELECT ... WHERE e.CourseID = @CourseID query
            var adapter = new SchoolDBConnectionTableAdapters.CoursesTableAdapter();
            var table = adapter.GetData(selectedCourseID);

            var report = new LocalReport
            {
                ReportPath = Server.MapPath("~/Reports/StudentReport.rdlc")
            };
            // Must match the dataset name inside the RDLC exactly
            report.DataSources.Add(new ReportDataSource("SchoolDBDataSet_Courses", (object)table));

            byte[] pdf = report.Render("PDF");

            if (action == "export")
                return File(pdf, "application/pdf", "StudentReport.pdf"); // downloads
            return File(pdf, "application/pdf");                          // shows in browser = preview
        }
    }
}