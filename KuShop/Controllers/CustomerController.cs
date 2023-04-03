using KuShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace KuShop.Controllers
{

    public class CustomerController : Controller
    {
        //สร้าง Field สำหรับใช้งาน DBContext ที่กำหนด
        private readonly KuShopContext _db;
        //private object cart;

        //สร้าง Constructor สำหรับตัว Controller เมื่อเริ่มต้นให้ใช้ Obj ของ DBContext
        public CustomerController(KuShopContext db)
        { _db = db; }

        public IActionResult Show(string id)
        {
            //ตรวจสอบว่ามีการส่ง id มาหรือไม่
            if (id == null)
            {
                ViewBag.ErrorMassage = "ต้องระบุค่า ID";
                return RedirectToAction("Index");
            }
            // ทำการเขียน Query หา Record ของ Customer.CusId จาก id ที่ส่งมา
            var obj = _db.Customers.Find(id);
            if (obj == null)
            {
                ViewBag.ErrorMassage = "ไม่พบข้อมูลที่ระบุ";
                return RedirectToAction("Index");
            }
            //ตั้งชื่อ File เป็น 'รหัสผู้ใช้.jpg'
            var fileName = id.ToString() + ".jpg";
            // กำหนด Path หรือ Directory ที่เก็บรูป 'imgcus'
            var imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\imgcus");
            // นำ Path และ ชื่อ File มาต่อกัน
            var filePath = Path.Combine(imgPath, fileName);

            // ทำการตรวจสอบว่ามี File นี้อยู่ Path ที่กำหนดหรือไม่
            // ถ้ามี ก็กำหนดให้ส่งต่ำแหน่งของรูปภาพไปให้ View
            // ถ้าไม่มี ก็กำหนดให้เรียกรูปภาพ Default ตามตำแหน่งและชื่อFile Default
            if (System.IO.File.Exists(filePath))
                ViewBag.ImgFile = "/imgcus/" + id + ".jpg";
            else
                ViewBag.ImgFile = "/img/login.png";

            return View(obj);
        }
        [HttpPost] //ระบุว่าเป็นการทำงานแบบ POST
        [ValidateAntiForgeryToken] //ป้องกันการโจมตี Cross-Site Request Forgery
        public IActionResult ImgUpload(IFormFile imgfiles, string theid)
        {
            if (imgfiles == null)
            {
                ViewBag.ErrorMessage = "ID Not Found";
                return RedirectToAction("Show");
            }
            //Getting FileName
            var LocalfileName = Path.GetFileName(imgfiles.FileName);

            //ให้ระบบสร้าง FileName ที่ Unique (Guid)
            //var NewFileName = Convert.ToString(Guid.NewGuid());
            //หรือสร้างเป็นพวกวันเวลาที่สร้าง
            //var NewFileName = theid+DateTime.Now.Ticks.ToString();
            var NewFileName = theid;

            //Getting file Extension
            var FileExtension = Path.GetExtension(LocalfileName);

            //ต่อ FileName กับ FileExtension
            var UpFileName = NewFileName + FileExtension;

            //กำหนดตำแหน่งที่ต้องการเก็บ File
            var savedir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\imgcus");
            //ต่อ ต่ำแหน่งที่ต้องการเก็บ File และ ชื่อFile
            var Filepath = Path.Combine(savedir, UpFileName);
            //ทำการ Upload
            using (FileStream fs = System.IO.File.Create(Filepath))
            {
                imgfiles.CopyTo(fs);
                fs.Flush();
            }

            //ย้ายหน้า controller = "Home", action = "Index", id = ""
            return RedirectToAction("Show", new { id = theid });
        }


        public IActionResult ImgDelete(string id)
        {
            var fileName = id.ToString() + ".jpg";
            //กำหนดตำแหน่งที่ตั้งของ File
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\imgcus");
            //เชื่อต่อ ตำแหน่ง กับ ชื่อFile
            var filePath = Path.Combine(imagePath, fileName);
            //ทำการตรวจสอบว่ามี File อยู่หรือไม่
            if (System.IO.File.Exists(filePath))
            {
                //ถ้ามีให้ลบ
                System.IO.File.Delete(filePath);
            }
            //controller = "Home", action = "Index", id = ""
            return RedirectToAction("Show", new { id = id });
        }
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Create()

        {
            ViewData["Pdt"] = new SelectList(_db.Customers, "CusId", "CusName");
            
            return View();
        }
        public IActionResult Create(Customer obj)
        {
           
            try
            {
                if (ModelState.IsValid)
                {
                    _db.Customers.Add(obj); //ส่งคำสั่ง Add ผ่าน DBContext
                    _db.SaveChanges(); // Execute คำสั่ง
                    return RedirectToAction("Show","Product"); // ย้ายทำงาน Action Index
                }
            }
            catch (Exception ex)
            {
                //ถ้าไม่ Valid ก็ สร้าง Error Message ขึ้นมา แล้ว ส่ง Obj กลับไปที่ View
                TempData["ErrorMassage"] = ex.Message;
                return View(obj);
            }
            //ถ้าไม่ Valid ก็ สร้าง Error Message ขึ้นมา แล้ว ส่ง Obj กลับไปที่ View
            TempData["ErrorMassage"] = "การบันทึกผิดพลาด";
            return View(obj);
        }
    }

}

