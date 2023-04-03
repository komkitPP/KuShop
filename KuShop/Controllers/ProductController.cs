using Microsoft.AspNetCore.Mvc;
using KuShop.Models;
using KuShop.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace KuShop.Controllers
{

    public class ProductController : Controller
    {
        //สร้าง Field สำหรับใช้งาน DBContext ที่กำหนด
        private readonly KuShopContext _db;

        //สร้าง Constructor สำหรับตัว Controller เมื่อเริ่มต้นให้ใช้ Obj ของ DBContext
        public ProductController(KuShopContext db)
        { _db = db; }
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("DutyId") != "admin")
            {
                TempData["Errormessage"] = "คุณไม่มีสิทธิใช้งาน";
                return RedirectToAction("Shop", "Home");
            }

            var pd = from p in _db.Products
                     join pt in _db.ProductTypes on p.PdtId equals pt.PdtId into join_p_pt
                     from p_pt in join_p_pt.DefaultIfEmpty()
                     join b in _db.Brands on p.BrandId equals b.BrandId into join_p_b
                     from p_b in join_p_b.DefaultIfEmpty()
                     select new PdVM
                     {
                         PdId = p.PdId,
                         PdName = p.PdName,
                         PdtName = p_pt.PdtName,
                         BrandName = p_b.BrandName,
                         PdPrice = p.PdPrice,
                         PdCost = p.PdCost,
                         PdStk = p.PdStk
                     };
            if (pd == null) return NotFound();
            return View(pd);
        }

        [HttpPost] //ระบุว่าเป็นการทำงานแบบ Post
        [ValidateAntiForgeryToken] // ป้องกันการโจมตี Cross_site Request Forgery
        public IActionResult Index(string? stext)
        {
            if (HttpContext.Session.GetString("DutyId") != "admin")
            {
                TempData["Errormessage"] = "คุณไม่มีสิทธิใช้งาน";
                return RedirectToAction("Shop", "Home");
            }
            if (stext == null)
            {
                return RedirectToAction("Index");
            }

            var pd = from p in _db.Products
                     join pt in _db.ProductTypes on p.PdtId equals pt.PdtId into join_p_pt
                     from p_pt in join_p_pt.DefaultIfEmpty()
                     join b in _db.Brands on p.BrandId equals b.BrandId into join_p_b
                     from p_b in join_p_b.DefaultIfEmpty()
                     where p.PdName.Contains(stext) ||
                            p_pt.PdtName.Contains(stext) ||
                            p_b.BrandName.Contains(stext)
                     select new PdVM
                     {
                         PdId = p.PdId,
                         PdName = p.PdName,
                         PdtName = p_pt.PdtName,
                         BrandName = p_b.BrandName,
                         PdPrice = p.PdPrice,
                         PdCost = p.PdCost,
                         PdStk = p.PdStk
                     };
            if (pd == null) return NotFound();

            ViewBag.stext = stext;
            return View(pd);
        }

        public IActionResult Show(string id)
        {
            //ตรวจสอบว่ามีการส่ง id มาหรือไม่
            if (id == null)
            {
                TempData["ErrorMassage"] = "ต้องระบุค่า ID";
                return RedirectToAction("Shop", "Home");
            }
            // ทำการเขียน Query หา Record ของ Product.pdId จาก id ที่ส่งมา
            var obj = _db.Products.Find(id);
            if (obj == null)
            {
                TempData["ErrorMassage"] = "ไม่พบข้อมูลที่ระบุ";
                return RedirectToAction("Shop", "Home");
            }
            var fileName = id.ToString() + ".jpg";
            // กำหนด Path หรือ Directory ที่เก็บรูป 'imgcus'
            var imgPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\imagepd");
            // นำ Path และ ชื่อ File มาต่อกัน
            var filePath = Path.Combine(imgPath, fileName);

            // ทำการตรวจสอบว่ามี File นี้อยู่ Path ที่กำหนดหรือไม่
            // ถ้ามี ก็กำหนดให้ส่งต่ำแหน่งของรูปภาพไปให้ View
            // ถ้าไม่มี ก็กำหนดให้เรียกรูปภาพ Default ตามตำแหน่งและชื่อFile Default
            if (System.IO.File.Exists(filePath))
                ViewBag.ImgFile = "/imagepd/" + id + ".jpg";
            else
                ViewBag.ImgFile = "/img/login.png";
            return View(obj);

        }

        //การเรียกผ่าน Link เป็นการทำงานแบบ Get
        //เป็นการสร้างใหม่ไม่ต้องทำ หรือ คำนวณ อะไร ทำการ return ไปที่ view ได้เลย
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("DutyId") != "admin")
            {
                TempData["Errormessage"] = "คุณไม่มีสิทธิใช้งาน";
                return RedirectToAction("Shop", "Home");
            }
            //อ่านข้อมูลจากตารางลง SelectList แล้วใส่ข้อมูลลงตัว ViewData
            ViewData["Pdt"] = new SelectList(_db.ProductTypes, "PdtId", "PdtName");
            ViewData["Brand"] = new SelectList(_db.Brands, "BrandId", "BrandName");
            return View();
        }

        [HttpPost] //ระบุว่าเป็นการทำงานแบบ Post
        [ValidateAntiForgeryToken] // ป้องกันการโจมตี Cross_site Request Forgery
        //ค่าที่ส่งมาจาก Form เป็น Object ของ Model ที่ระบุ ตัว Controller ก็รับค่าเป็น Object
        public IActionResult Create(Product obj)
        {
            if (HttpContext.Session.GetString("DutyId") != "admin")
            {
                TempData["Errormessage"] = "คุณไม่มีสิทธิใช้งาน";
                return RedirectToAction("Shop", "Home");
            }
            try
            {
                if (ModelState.IsValid)
                {
                    _db.Products.Add(obj); //ส่งคำสั่ง Add ผ่าน DBContext
                    _db.SaveChanges(); // Execute คำสั่ง
                    return RedirectToAction("Index"); // ย้ายทำงาน Action Index
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


        public IActionResult Edit(string id)
        {
            if (HttpContext.Session.GetString("DutyId") != "admin")
            {
                TempData["Errormessage"] = "คุณไม่มีสิทธิใช้งาน";
                return RedirectToAction("Shop", "Home");
            }
            //ตรวจสอบว่ามีการส่ง id มาหรือไม่
            if (id == null)
            {
                TempData["ErrorMassage"] = "ต้องระบุค่า ID";
                return RedirectToAction("Index");
            }
            // ทำการเขียน Query หา Record ของ Product.pdId จาก id ที่ส่งมา
            var obj = _db.Products.Find(id);
            if (obj == null)
            {
                ViewBag.ErrorMassage = "ไม่พบข้อมูลที่ระบุ";
                return RedirectToAction("Index");
            }
            //อ่านข้อมูลจากตารางลง SelectList แล้วใส่ข้อมูลลงตัว ViewData
            //และกำนหนดว่า Select ที่เลือก เป็น id ของ obj นั้นๆ
            ViewData["Pdt"] = new SelectList(_db.ProductTypes, "PdtId", "PdtName", obj.PdtId);
            ViewData["Brand"] = new SelectList(_db.Brands, "BrandId", "BrandName", obj.BrandId);
            return View(obj);
        }

        [HttpPost] //ระบุว่าเป็นการทำงานแบบ Post
        [ValidateAntiForgeryToken] // ป้องกันการโจมตี Cross_site Request Forgery
        //ค่าที่ส่งมาจาก Form เป็น Object ของ Model ที่ระบุ ตัว Controller ก็รับค่าเป็น Object
        public IActionResult Edit(Product obj)
        {
            if (HttpContext.Session.GetString("DutyId") != "admin")
            {
                TempData["Errormessage"] = "คุณไม่มีสิทธิใช้งาน";
                return RedirectToAction("Shop", "Home");
            }
            try
            {
                if (ModelState.IsValid)
                {
                    _db.Products.Update(obj); //ส่งคำสั่ง Update ผ่าน DBContext
                    _db.SaveChanges(); // Execute คำสั่ง
                    return RedirectToAction("Index"); // ย้ายทำงาน Action Index
                }
            }
            catch (Exception ex)
            {
                //ถ้าไม่ Valid ก็ สร้าง Error Message ขึ้นมา แล้ว ส่ง Obj กลับไปที่ View
                TempData["ErrorMassage"] = ex.Message;
                return View(obj);
            }
            //ถ้าไม่ Valid ก็ สร้าง Error Message ขึ้นมา แล้ว ส่ง Obj กลับไปที่ View
            TempData["ErrorMassage"] = "การแก้ไขผิดพลาด";
            //อ่านข้อมูลจากตารางลง SelectList แล้วใส่ข้อมูลลงตัว ViewData
            //และกำนหนดว่า Select ที่เลือก เป็น id ของ obj นั้นๆ
            ViewData["Pdt"] = new SelectList(_db.ProductTypes, "PdtId", "PdtName", obj.PdtId);
            ViewData["Brand"] = new SelectList(_db.Brands, "BrandId", "BrandName", obj.BrandId);
            return View(obj);

        }

        public IActionResult Delete(string id)
        {
            if (HttpContext.Session.GetString("DutyId") != "admin")
            {
                TempData["Errormessage"] = "คุณไม่มีสิทธิใช้งาน";
                return RedirectToAction("Shop", "Home");
            }
            //ตรวจสอบว่ามีการส่ง id มาหรือไม่
            if (id == null)
            {
                ViewBag.ErrorMassage = "ต้องระบุค่า ID";
                return RedirectToAction("Index");
            }
            // ทำการเขียน Query หา Record ของ Product.pdId จาก id ที่ส่งมา
            var obj = _db.Products.Find(id);
            if (obj == null)
            {
                ViewBag.ErrorMassage = "ไม่พบข้อมูลที่ระบุ";
                return RedirectToAction("Index");
            }

            ViewBag.pdtName = _db.ProductTypes.FirstOrDefault(pt => pt.PdtId == obj.PdtId).PdtName;
            ViewBag.brandName = _db.Brands.FirstOrDefault(br => br.BrandId == obj.BrandId).BrandName;
            return View(obj);
        }

        [HttpPost] //ระบุว่าเป็นการทำงานแบบ Post
        [ValidateAntiForgeryToken] // ป้องกันการโจมตี Cross_site Request Forgery
        //**** ค่าที่ส่งมาจาก Form เป็น string  ต้องรับค่าเป็น string
        // แต่ถ้ารับค่าเป็น string จะ Error เพราะเป็นการประกาศ method ซ้ำจึงต้องตั้งชื่อใหม่ เป็น DeletePost
        // และตัวแปรที่รับ จะต้องเหมือนกับชือที่ส่งมาจาก View ด้วย จากตัวอย่างคือ PdId
        public IActionResult DeletePost(string PdId)
        {
            if (HttpContext.Session.GetString("DutyId") != "admin")
            {
                TempData["Errormessage"] = "คุณไม่มีสิทธิใช้งาน";
                return RedirectToAction("Shop", "Home");
            }
            try
            {
                // ทำการเขียน Query หา Record ของ Product.pdId จาก id ที่ส่งมา
                var obj = _db.Products.Find(PdId);
                if (obj == null)
                {
                    ViewBag.ErrorMassage = "ไม่พบข้อมูลที่ระบุ";
                    return RedirectToAction("Index");
                }
                _db.Products.Remove(obj); //ส่งคำสั่ง Remove ผ่าน DBContext
                _db.SaveChanges(); // Execute คำสั่ง
                return RedirectToAction("Index"); // ย้ายทำงาน Action Index              
            }
            catch (Exception ex)
            {
                //ถ้าไม่ Valid ก็ สร้าง Error Message ขึ้นมา แล้ว ส่ง Obj กลับไปที่ View
                TempData["ErrorMassage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
