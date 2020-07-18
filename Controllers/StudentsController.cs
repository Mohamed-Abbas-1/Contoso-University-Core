using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Syncfusion.Pdf.Grid;
using Syncfusion.Pdf;
using System.Data;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Drawing;
using System.IO;
using Syncfusion.Pdf.Parsing;

namespace ContosoUniversity.Controllers
{
    public class StudentsController : Controller
    {
        private readonly SchoolContext _context;

        public StudentsController(SchoolContext context)
        {
            _context = context;
        }

        // GET: Students
        public async Task<IActionResult> Index(string sortOrder, string SearchString , string currentFilter, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["LNameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "last_name_desc" : "";
            ViewData["FNameSortParm"] = sortOrder =="first_name" ? "first_name_desc" : "first_name";
            ViewData["DateSortParm"] = sortOrder == "date" ? "date_desc" : "date";

            if (SearchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                SearchString = currentFilter;
            }

            ViewData["currentFilter"] = SearchString;

            var students =  from student in _context.Students
                            select student;

            if (!string.IsNullOrEmpty(SearchString))
            {
                students = students.Where(s => s.LastName.Contains(SearchString)
                || s.FirstMidName.Contains(SearchString));
            }

            switch (sortOrder)
            {
                case "last_name_desc":
                    students =  students.OrderByDescending(s => s.LastName);
                    break;
                case "first_name":
                    students = students.OrderBy(s => s.FirstMidName);
                    break;
                case "first_name_desc":
                    students = students.OrderByDescending(s => s.FirstMidName);
                    break;
                case "date":
                    students = students.OrderBy(s => s.EnrollmentDate);
                    break;
                case "date_desc":
                    students = students.OrderByDescending(s => s.EnrollmentDate);
                    break;
                default:
                    students = students.OrderBy(s => s.LastName);
                    break;
            }

            int pageSize = 5;
            return View(await PaginatedList<Student>.CreateAsync(students.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.Include(s => s.Enrollments).ThenInclude(e => e.Course).AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,LastName,FirstMidName,EnrollmentDate")] Student student)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    _context.Add(student);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                //Log the error (uncomment ex variable name and write a log.
                ModelState.AddModelError("", "Unable to save changes. " +
                    "Try again, and if the problem persists " +
                    "see your system administrator.");
            }
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,LastName,FirstMidName,EnrollmentDate")] Student student)
        {
            if (id != student.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. " +
                "Try again, and if the problem persists, " +
                "see your system administrator.");
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (student == null)
            {
                return NotFound();
            }

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] =
            "Delete failed. Try again, and if the problem persists " +
            "see your system administrator.";
            }
            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
                return RedirectToAction(nameof(Index));
            try
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException /* ex */)
            {
                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
            }
        }

        public IActionResult PrintToday()
        {
            //Create a new PDF document.
            PdfDocument doc = new PdfDocument();
            //Add a page.
            PdfPage page = doc.Pages.Add();
            //Create a PdfGrid.
            PdfGrid pdfGrid = new PdfGrid();
            //Loads the image as stream
            FileStream imageStream = new FileStream("img/1.jpg", FileMode.Open, FileAccess.Read);
            RectangleF bounds = new RectangleF(10, 0, 505, 100);
            PdfImage image = PdfImage.FromStream(imageStream);
            //Draws the image to the PDF page
            page.Graphics.DrawImage(image, bounds);

            ///////////////////////Text///////////////////////
            /////Create PDF graphics for the page.
            PdfGraphics graphics = page.Graphics;
            //Set the standard font.
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 15);
            //Draw the text.
            string text = "Students Records For Today!!!";
            graphics.DrawString(text, font, new PdfSolidBrush(new PdfColor(26, 155, 203)), new Syncfusion.Drawing.PointF(150, 120));
            //Add values to list
            // var data = _Context.Students.ToList();

            var data = from student in _context.Students
                       where student.EnrollmentDate > DateTime.Now.AddDays(-1)
                       orderby student.FirstMidName descending
                       select new
                       {
                           student.LastName,
                           student.FirstMidName,
                           student.EnrollmentDate,
                           student.Enrollments.Count
                       };
            //Add list to IEnumerable
            IEnumerable<object> dataTable = data;
            //Assign data source.
            pdfGrid.DataSource = dataTable;
            /////// Header/////////////
            PdfGridRow header = pdfGrid.Headers[0];

            header.Cells[0].Value = "First Name";

            header.Cells[1].Value = "Last Name";

            header.Cells[2].Value = "Enrollment Date";

            header.Cells[3].Value = "Enrollment courses number";
            header.Style.TextBrush = new PdfSolidBrush(new PdfColor(255, 255, 255));
            header.Style.BackgroundBrush = new PdfSolidBrush(new PdfColor(126, 155, 203));
            header.Style.Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 11f, PdfFontStyle.Bold);

            for (int i = 0; i < header.Cells.Count; i++)
            {
                header.Cells[i].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
                header.Cells[i].Style.CellPadding = new PdfPaddings(10, 10, 10, 10);
            }
            /////////////////////Rows/////////////////////
            //Adds cell customizations
            for (int i = 0; i < pdfGrid.Rows.Count; i++)
            {
                pdfGrid.Rows[i].Style.TextBrush = new PdfSolidBrush(new PdfColor(126, 155, 203));

                pdfGrid.Rows[i].Style.Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 10f, PdfFontStyle.Bold);
                for (int j = 0; j < pdfGrid.Rows[i].Cells.Count; j++)
                {
                    pdfGrid.Rows[i].Cells[j].Style.CellPadding = new PdfPaddings(10, 10, 10, 10);
                }

            }

            //Draw grid to the page of PDF document.
            pdfGrid.Draw(page, new Syncfusion.Drawing.PointF(10, 150));


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
            string fileName = "Today'sStudent.pdf";
            //Creates a FileContentResult object by using the file contents, content type, and file name.
            return File(stream, contentType, fileName);
        } 
        public IActionResult PrintAll()
        {
            //Create a new PDF document.
            PdfDocument doc = new PdfDocument();
            //Add a page.
            PdfPage page = doc.Pages.Add();
            //Create a PdfGrid.
            PdfGrid pdfGrid = new PdfGrid();
            //Loads the image as stream
            FileStream imageStream = new FileStream("img/1.jpg", FileMode.Open, FileAccess.Read);
            RectangleF bounds = new RectangleF(10, 0, 505, 100);
            PdfImage image = PdfImage.FromStream(imageStream);
            //Draws the image to the PDF page
            page.Graphics.DrawImage(image, bounds);

            ///////////////////////Text///////////////////////
            /////Create PDF graphics for the page.
            PdfGraphics graphics = page.Graphics;
            //Set the standard font.
            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 15);
            //Draw the text.
            string text = "All Students Records!!!";
            graphics.DrawString(text, font, new PdfSolidBrush(new PdfColor(26, 155, 203)), new Syncfusion.Drawing.PointF(180, 120));
            //Add values to list
            // var data = _Context.Students.ToList();

            var data = from student in _context.Students
                       orderby student.FirstMidName descending
                       select new
                       {
                           student.LastName,
                           student.FirstMidName,
                           student.EnrollmentDate,
                           student.Enrollments.Count
                       };
            //Add list to IEnumerable
            IEnumerable<object> dataTable = data;
            //Assign data source.
            pdfGrid.DataSource = dataTable;
            /////// Header/////////////
            PdfGridRow header = pdfGrid.Headers[0];

            header.Cells[0].Value = "First Name";

            header.Cells[1].Value = "Last Name";

            header.Cells[2].Value = "Enrollment Date";

            header.Cells[3].Value = "Enrollment courses number";
            header.Style.TextBrush = new PdfSolidBrush(new PdfColor(255, 255, 255));
            header.Style.BackgroundBrush = new PdfSolidBrush(new PdfColor(126, 155, 203));
            header.Style.Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 11f, PdfFontStyle.Bold);

            for (int i = 0; i < header.Cells.Count; i++)
            {
                header.Cells[i].StringFormat = new PdfStringFormat(PdfTextAlignment.Center, PdfVerticalAlignment.Middle);
                header.Cells[i].Style.CellPadding = new PdfPaddings(10, 10, 10, 10);
            }
            /////////////////////Rows/////////////////////
            //Adds cell customizations
            for (int i = 0; i < pdfGrid.Rows.Count; i++)
            {
                pdfGrid.Rows[i].Style.TextBrush = new PdfSolidBrush(new PdfColor(126, 155, 203));

                pdfGrid.Rows[i].Style.Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 10f, PdfFontStyle.Bold);
                for (int j = 0; j < pdfGrid.Rows[i].Cells.Count; j++)
                {
                    pdfGrid.Rows[i].Cells[j].Style.CellPadding = new PdfPaddings(10, 10, 10, 10);
                }

            }

            //Draw grid to the page of PDF document.
            pdfGrid.Draw(page, new Syncfusion.Drawing.PointF(10, 150));


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
            string fileName = "AllStudents.pdf";
            //Creates a FileContentResult object by using the file contents, content type, and file name.
            return File(stream, contentType, fileName);
        }
        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.ID == id);
        }
    }
}
