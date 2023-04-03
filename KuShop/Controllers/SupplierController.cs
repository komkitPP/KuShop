using KuShop.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace KuShop.Controllers
{
    public class SupplierController : Controller
    {
        //สร้าง Field สำหรับใช้งาน DBContext ที่กำหนด
        private readonly KuShopContext _db;
        //private object cart;

        //สร้าง Constructor สำหรับตัว Controller เมื่อเริ่มต้นให้ใช้ Obj ของ DBContext
        public SupplierController(KuShopContext db)
        { _db = db; }

        public IActionResult Index()
        {
            var Sup = from s in _db.Suppliers
                      select s;
            if (Sup ==null) return NotFound();
            return View(Sup);
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
            var obj = _db.Suppliers.Find(id);
            if (obj == null)
            {
                TempData["ErrorMassage"] = "ไม่พบข้อมูลที่ระบุ";
                return RedirectToAction("Shop", "Home");
            }
            return View(obj);
        }

    }
}
