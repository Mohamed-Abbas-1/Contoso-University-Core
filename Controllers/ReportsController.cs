using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Stimulsoft.Report.Web;
using Stimulsoft.Report.Mvc;
using Stimulsoft.Report;
using ContosoUniversity.Models;
using ContosoUniversity.Data;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Drawing;
using System.IO;
using Microsoft.Extensions.Hosting;
using Syncfusion.Pdf.Grid;

namespace ContosoUniversity.Controllers
{
    public class ReportsController : Controller
    {
        private readonly SchoolContext _Context ;
        private readonly IHostEnvironment _hostEnvironment;

        public ReportsController(IHostEnvironment hostEnvironment  , SchoolContext context)
        {
            _hostEnvironment = hostEnvironment;
            _Context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        private StiReport LoadReport()
        {
            StiReport report = new StiReport();
           StiNetCoreViewer.GetReportResult(this, report);

            return report;
        }

       
        public IActionResult PrintReport()
        {
            StiReport report = LoadReport();
            return StiNetCoreReportResponse.PrintAsPdf(report);
        }

        public IActionResult ExportReport()
        {
            StiReport report = LoadReport();

            return StiNetCoreReportResponse.ResponseAsPdf(report);
        }

        
        public IActionResult report()
        {
            return View();
        }
       
        public IActionResult Print()
        {
            HtmlToPdfConverter converter = new HtmlToPdfConverter();

            WebKitConverterSettings settings = new WebKitConverterSettings();

            
            settings.WebKitPath = Path.Combine(_hostEnvironment.ContentRootPath, "QtBinariesWindows");
            converter.ConverterSettings = settings;

            PdfDocument document = converter.Convert("https://localhost:44389/reports/report");

            MemoryStream ms = new MemoryStream();
            document.Save(ms);
            document.Close(true);

            ms.Position = 0;

            FileStreamResult fileStreamResult = new FileStreamResult(ms, "application/pdf");
            fileStreamResult.FileDownloadName = "Students.pdf";

            return fileStreamResult;
        }

        public IActionResult CreateDocument()
        {
            //Create a new PDF document.
            PdfDocument doc = new PdfDocument();
            //Add a page.
            PdfPage page = doc.Pages.Add();
            //Create a PdfGrid.
            PdfGrid pdfGrid = new PdfGrid();
            //Add values to list
           // var data = _Context.Students.ToList();

            var data = from student in _Context.Students
                        orderby student.FirstMidName descending
                        select new
                        {
                            student.LastName,
                            student.FirstMidName,
                            student.EnrollmentDate ,
                            student.Enrollments.Count
                        };
            //Add list to IEnumerable
            IEnumerable<object> dataTable = data;
            //Assign data source.
            pdfGrid.DataSource = dataTable;
            //Draw grid to the page of PDF document.
            pdfGrid.Draw(page, new Syncfusion.Drawing.PointF(10, 10));
            //Save the PDF document to stream
            MemoryStream stream = new MemoryStream();
            doc.Save(stream);
            //If the position is not set to '0' then the PDF will be empty.
            stream.Position = 0;
            //Close the document.
            doc.Close(true);
            //Defining the ContentType for pdf file.
            string contentType = "application/pdf";
            //Define the file name.
            string fileName = "Output.pdf";
            //Creates a FileContentResult object by using the file contents, content type, and file name.
            return File(stream, contentType, fileName);
        }
        
    }
}