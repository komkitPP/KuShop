using KuShop.Models;
using KuShop.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;

namespace KuShop.Controllers
{
    public class HomeController : Controller
    {
        //สร้าง Field สำหรับใช้งาน DBContext ที่กำหนด
        private readonly KuShopContext _db;

        //สร้าง Constructor สำหรับตัว Controller เมื่อเริ่มต้นให้ใช้ Obj ของ DBContext
        public HomeController(KuShopContext db)
        { _db = db; }

        public IActionResult Index()
        {
            return RedirectToAction("Shop");
        }

        public IActionResult Shop()
        {
            //from p in _db.Products.Take(4)
            //ทำงานเหมือนกัน Product/Index แต่ Return ต่างกัน
            var pd = from p in _db.Products.Take(4)
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
        public IActionResult Shop(string? stext)
        {
            if (stext == null)
            {
                stext = "";
            }
            //var pd = from p in _db.Products
            //         select p;
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
            if (pd == null)
            {
                ViewBag.ErrorMassage = "ไม่พบสินค้าที่ระบุ";
                ViewBag.stext = stext;
                //return RedirectToAction("Shop");
                return View();
            }

            ViewBag.stext = stext;
            return View(pd);
        }

        //ให้เรียกหน้า Login แบบ Get ได้
        public IActionResult Login()
        { return View(); }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string userName,string userPass)
        {
            //Query หาว่ามี Login Password ที่ระบุหรือไม่
            var cus = from c in _db.Customers
                      where c.CusLogin.Equals(userName)
                            && c.CusPass.Equals(userPass)
                      select c ;
            
           // var cus = _db.Customers.Find(userName);
            //ถ้าข้อมูลเท่ากับ 0 ให้บอกว่าหาข้อมูลไม่พบ
            if(cus.ToList().Count==0)
            {
                //ถ้าใช้ RedirectToAction ไม่สามารถใช้ ViewBag ได้ ต้องใช้ TempData
                TempData["ErrorMessage"]= "ระบุผู้ใช้หรือรหัสผ่านไม่ถูกต้อง";
                //ViewBag.ErrorMessage = "ระบุผู้ใช้หรือรหัสผ่านไม่ถูกต้อง";
                return View();
            }

            //ถ้าหาข้อมูลพบ ให้เก็บค่าเข้า Session 
            string CusId ="";
            string CusName ;
            string CusEmail ;
            foreach (var item in cus)
            {
                //อ่านค่าจาก Object เข้าตัวแปร
                CusId = item.CusId;
                CusName = item.CusName;
                CusEmail = item.CusEmail;
                //เอาค่าจากตัวแปรเข้า Session
                HttpContext.Session.SetString("CusId", CusId);
                HttpContext.Session.SetString("CusName", CusName);
                HttpContext.Session.SetString("CusEmail", CusEmail);
                //Update  Column ของตารางที่ระบุ
                //โดยทำการอ่านค่าตาม รหัส หรือ id ที่ระบุ
                //และระบุค่าแต่ละ Field ของ Record นั้นๆ
                //แล้วกำหนดให้ทำการปรับเปลี่ยน (Modified)
                var theRecord = _db.Customers.Find(CusId);
                theRecord.LastLogin = DateTime.Now.Date;
                //theRecord.CusAdd = "ABCD";
                //theRecord.CusEmail = "a@b.com";
                _db.Entry(theRecord).State = EntityState.Modified;                
            }
            //ทำการบันทึกทุก Record ที่สั่ง Modified ไว้
            _db.SaveChanges();

            //*******ทำการตรวจสอบตะกร้าเดิมที่ตกค้าง***********
            return RedirectToAction("Check","Cart");
            //ทำการย้ายไปหน้าที่ต้องการ
            //return RedirectToAction("Index");
        }
        public IActionResult Logout()
        {
            //ล้างทุก Session และย้ายกลับหน้า Index
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
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