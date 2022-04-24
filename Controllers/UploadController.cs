

using Document_Saver.Data;
using Document_Saver.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Diagnostics;

namespace Document_Saver.Controllers
{
    public class UploadController : Controller
    {
        private readonly DocumentDetailsContext _DB;
        public UploadController(DocumentDetailsContext DB)
        {
            _DB = DB;
        }
        [HttpGet]

        public IActionResult Index(string Sorting_Order,string Search_Data)
        {
            IEnumerable<Documents> objectDocumentlist = _DB.Document;

            var students = from stu in _DB.Document select stu;
            {
                students = students.Where(stu => stu.Document_Name.ToUpper().Contains(Search_Data.ToUpper()));
                  
            }
            ViewBag.SortingDocumentName = String.IsNullOrEmpty(Sorting_Order) ? "Document_Name" : "";

            var Documents = from stu in _DB.Document.AsQueryable() select stu;
            switch (Sorting_Order)
            {
                case "Document_Name":
                    Documents = Documents.OrderByDescending(stu => stu.Document_Name);
                    break;
                default:
                    Documents = Documents.OrderBy(stu => stu.Document_Name);
                    break;

            }
            return View(Documents.ToList());
           
            return View(_DB.Document.Where(x=>x.Is_Active==true).ToList());
        }


    
        public IActionResult Download(string fileName)
          {
              if (fileName == null)
              {
                  return Content("filename not present");
              }
              else
              {

                  var filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\Files", fileName);

                  byte[] bytes = System.IO.File.ReadAllBytes(filepath);


                  return File(bytes, "application/octet-stream", fileName);
              }
          }
  
   
        public IActionResult Upload()
        {
            return View();
        }
   
        [HttpPost]
        public IActionResult Upload(IFormFile files,Documents obj)
        {
            if (files != null)
            {
                if (files.Length > 0)
                {
                    
                    var fileName = Path.GetFileName(files.FileName);

                  
                    var myUniqueFileName = Convert.ToString(Guid.NewGuid());

                  
                    var fileExtension = Path.GetExtension(fileName);

                  
                    var newFileName = String.Concat(myUniqueFileName, fileExtension);

              
                    var filepath =
            new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files")).Root + $@"\{newFileName}";

                    using (FileStream fs = System.IO.File.Create(filepath))
                    {
                        files.CopyTo(fs);
                        fs.Flush();
                    }
                    var fileModel = new Documents
                    {
                        Document_Id = obj.Document_Id,
                        Document_Name = fileName,
                        File_Name = newFileName,
                        File_Type = fileExtension,

                        Process_Id = obj.Process_Id = Process.GetCurrentProcess().Id.ToString(),
                        //ProjectDetails=obj.ProjectDetails,
                        Created_At = obj.Created_At,
                        Created_By = obj.Created_By,
                        Project_Id = obj.Project_Id,
                        Is_Active = obj.Is_Active,
                        Is_Deleted = obj.Is_Deleted,
                        Updated_At = obj.Updated_At,
                        Updated_By = obj.Updated_By,
                       /* Process_Id= obj.Process_Id = Process.GetCurrentProcess().Id.ToString()*/
                };

                    _DB.Document.Add(fileModel);
                    _DB.SaveChanges();
                }
                TempData["Message"] = "File successfully uploaded";
                return RedirectToAction("Index");
            }
            return View();
        }
       /* [HttpGet]

        public ActionResult Show()

        {

            string[] filePaths = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files");

            List<Documents> files = new List<Documents>();

            foreach (string filePath in filePaths)

            {

                files.Add(new Documents { File_Name = Path.GetFileName(filePath) });

            }

            return View(files);

        }
*/

        public FileResult ViewFile(string fileName)

        {

            string filepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files");

            byte[] pdfByte = System.IO.File.ReadAllBytes(filepath);

            return File(pdfByte, "application/pdf");

        }

        public IActionResult Delete(int? Document_Id)
        {
            bool result = false;
            if (Document_Id == null || Document_Id == 0)
            {
                return NotFound();
            }
            Documents files  = _DB.Document.FirstOrDefault(u => u.Document_Id == Document_Id);
            // product.Category = _db.Category.Find(product.Category.Id);


            if (files == null)
            {
                return NotFound();
            }
            return View(files);
        }
        //Post
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? Document_Id)
        {
            bool result = false;
            var obj = _DB.Document.Find(Document_Id);
            if (obj == null)
            {
                return NotFound();
            }
            string upload = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Files");
            var oldfile = Path.Combine(upload, obj.File_Name);
            if (System.IO.File.Exists(oldfile))
            {
                System.IO.File.Delete(oldfile);
            }
            if (upload != null)
            {
                obj.Is_Active = false;
                _DB.Document.Remove(obj);
                _DB.SaveChanges();
                result = true;
            }
                return RedirectToAction("Index");
            



        }
    }
    

}