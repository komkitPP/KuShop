using KuShop.Models;
using KuShop.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.VisualBasic;
using System;
using System.Dynamic;

namespace KuShop.Controllers
{
    public class ReportController : Controller
    {
        //สร้าง Field สำหรับใช้งาน DBContext ที่กำหนด
        private readonly KuShopContext _db;
        //private object cart;

        //สร้าง Constructor สำหรับตัว Controller เมื่อเริ่มต้นให้ใช้ Obj ของ DBContext
        public ReportController(KuShopContext db)
        { _db = db; }

        public IActionResult StockTran()
        {
            //กำหนดค่าเริ่มต้น
            string pdid = "";
            var theMonth = DateTime.Now.Month;
            var theYear = DateTime.Now.Year;

            DateTime sDate = new DateTime(theYear, theMonth, 1);
            DateTime eDate = new DateTime(theYear, theMonth, 1).AddMonths(1).AddDays(-1);

            //สรุปซื้อสินค้าเข้า BuyDtls - เป็นการย้าย Stock เข้า
            var StkIn = from bd in _db.BuyDtls
                        join p in _db.Products on bd.PdId equals p.PdId into join_bd_p
                        from bd_p in join_bd_p.DefaultIfEmpty()

                        join b in _db.Buyings on bd.BuyId equals b.BuyId into join_bd_b
                        from bd_b in join_bd_b

                        where bd_p.PdId == pdid && bd_b.BuyDate >= sDate && bd_b.BuyDate <= eDate
                        group bd by new { bd_p.PdId, bd_p.PdName, bd_b.BuyDate } into g

                        select new RepStkTran
                        {
                            PdId = g.Key.PdId,
                            PdName = g.Key.PdName,
                            TranDate = g.Key.BuyDate,
                            TranType = "IN",
                            TranQty = g.Sum(x => x.BdtlQty)
                        };
            //สรุปขายสินค้าออก CartDtls - เป็นการย้าย Stock ออก
            var StkOut = from cd in _db.CartDtls
                         join p in _db.Products on cd.PdId equals p.PdId into join_cd_p
                         from cd_p in join_cd_p.DefaultIfEmpty()

                         join c in _db.Carts on cd.CartId equals c.CartId into join_cd_c
                         from cd_c in join_cd_c

                         where cd_p.PdId == pdid && cd_c.CartDate >= sDate && cd_c.CartDate <= eDate
                         group cd by new { cd_p.PdId, cd_p.PdName, cd_c.CartDate } into g

                         select new RepStkTran
                         {
                             PdId = g.Key.PdId,
                             PdName = g.Key.PdName,
                             TranDate = g.Key.CartDate,
                             TranType = "OUT",
                             TranQty = g.Sum(x => x.CdtlQty)
                         };

            //สร้าง dynamic model
            //เพื่อส่งข้อมูลให้ View แบบสองตารางพร้อมกัน
            dynamic DyModel = new ExpandoObject();
            //แบ่งเป็นส่วน IN รับข้อมูลจาก Obj StkIn
            DyModel.In = StkIn;
            //แบ่งเป็นส่วน Oot รับข้อมูลจาก Obj StkOut
            DyModel.Out = StkOut;

            //ส่งข้อมูลเพื่อแสดงที่ Form
            ViewBag.sDate = sDate.Date.ToString("yyyy-MM-dd");
            ViewBag.eDate = eDate.Date.ToString("yyyy-MM-dd");
            ViewData["Pd"] = new SelectList(_db.Products, "PdId", "PdName", pdid);
            return View(DyModel);

            
           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StockTran(string pdid, DateTime sDate, DateTime eDate)
        {
            //สรุปซื้อสินค้าเข้า BuyDtls - เป็นการย้าย Stock เข้า
            var StkIn = from bd in _db.BuyDtls
                         join p in _db.Products on bd.PdId equals p.PdId into join_bd_p
                         from bd_p in join_bd_p.DefaultIfEmpty()

                         join b in _db.Buyings on bd.BuyId equals b.BuyId into join_bd_b
                         from bd_b in join_bd_b
                         
                         where bd_p.PdId == pdid && bd_b.BuyDate >= sDate && bd_b.BuyDate <= eDate
                         group bd by new { bd_p.PdId,bd_p.PdName,bd_b.BuyDate } into g

                         select new RepStkTran
                         {
                             PdId = g.Key.PdId,
                             PdName = g.Key.PdName,
                             TranDate = g.Key.BuyDate,
                             TranType = "IN",
                             TranQty = g.Sum(x => x.BdtlQty)
                         };
            //สรุปขายสินค้าออก CartDtls - เป็นการย้าย Stock ออก
            var StkOut = from cd in _db.CartDtls
                        join p in _db.Products on cd.PdId equals p.PdId into join_cd_p
                        from cd_p in join_cd_p.DefaultIfEmpty()

                        join c in _db.Carts on cd.CartId equals c.CartId into join_cd_c
                        from cd_c in join_cd_c

                        where cd_p.PdId == pdid && cd_c.CartDate >= sDate && cd_c.CartDate <= eDate
                        group cd by new { cd_p.PdId, cd_p.PdName,cd_c.CartDate } into g

                        select new RepStkTran
                        {
                            PdId = g.Key.PdId,
                            PdName = g.Key.PdName,
                            TranDate= g.Key.CartDate,
                            TranType = "OUT",
                            TranQty = g.Sum(x => x.CdtlQty)
                        };

            //สร้าง dynamic model
            //เพื่อส่งข้อมูลให้ View แบบสองตารางพร้อมกัน
            dynamic DyModel = new ExpandoObject();
            //แบ่งเป็นส่วน IN รับข้อมูลจาก Obj StkIn
            DyModel.In = StkIn;
            //แบ่งเป็นส่วน OUT รับข้อมูลจาก Obj StkOut
            DyModel.Out = StkOut;

            ViewBag.sDate = sDate.Date.ToString("yyyy-MM-dd");
            ViewBag.eDate = eDate.Date.ToString("yyyy-MM-dd");
            ViewData["Pd"] = new SelectList(_db.Products, "PdId", "PdName",pdid);
            return View(DyModel);
        }

        public IActionResult SaleMonthly()
        {
            //กำหนดวันแรก และคำนวณหาวันสุดท้ายของเดือนปัจจุบัน
            var theMonth = DateTime.Now.Month;
            var theYear = DateTime.Now.Year;
            DateTime sDate = new DateTime(theYear, theMonth, 1);
            DateTime eDate = new DateTime(theYear, theMonth, 1).AddMonths(1).AddDays(-1); 
            
            var rep = from cd in _db.CartDtls

                      join p in _db.Products on cd.PdId equals p.PdId into join_cd_p
                      from cd_p in join_cd_p.DefaultIfEmpty()

                      join c in _db.Carts on cd.CartId equals c.CartId into join_cd
                      from c_cd in join_cd
                      where c_cd.CartDate >= sDate && c_cd.CartDate <= eDate
                      group cd by new { cd.PdId, cd_p.PdName } into g
                      select new RepSale
                      {
                          PdId = g.Key.PdId,
                          PdName = g.Key.PdName,
                          CdtlMoney = g.Sum(x => x.CdtlMoney),
                          CdtlQty = g.Sum(x => x.CdtlQty)
                      };
            ViewBag.sDate = sDate.Date.ToString("yyyy-MM-dd");
            ViewBag.eDate = eDate.Date.ToString("yyyy-MM-dd");
            return View(rep);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaleMonthly(DateTime sDate, DateTime eDate)
        {
            //var theMonth = DateTime.Now.Month;
            //var theYear = DateTime.Now.Year;

            //DateTime sDate = new DateTime(theYear, theMonth, 1);
            //DateTime eDate = new DateTime(theYear, theMonth, 1).AddMonths(1).AddDays(-1);
            var rep = from cd in _db.CartDtls

                      join p in _db.Products on cd.PdId equals p.PdId into join_cd_p
                      from cd_p in join_cd_p.DefaultIfEmpty()

                      join c in _db.Carts on cd.CartId equals c.CartId into join_cd
                      from c_cd in join_cd
                      where c_cd.CartDate >= sDate && c_cd.CartDate <= eDate
                      group cd by new { cd.PdId, cd_p.PdName } into g
                      select new RepSale
                      {
                          PdId = g.Key.PdId,
                          PdName = g.Key.PdName,
                          CdtlMoney = g.Sum(x => x.CdtlMoney),
                          CdtlQty = g.Sum(x => x.CdtlQty)
                      };
            ViewBag.sDate = sDate.Date.ToString("yyyy-MM-dd");
            ViewBag.eDate = eDate.Date.ToString("yyyy-MM-dd");
            return View(rep);
        }


        public IActionResult SaleDaily ()
        {
            DateTime thedate = DateTime.Now.Date;
            var rep =   from cd in _db.CartDtls
                          
                        join p in _db.Products on cd.PdId equals p.PdId into join_cd_p
                        from cd_p in join_cd_p.DefaultIfEmpty()
                          
                        join c in _db.Carts on cd.CartId equals c.CartId into join_cd
                        from c_cd in join_cd
                        where c_cd.CartDate== thedate
                        group cd by new { cd.PdId, cd_p.PdName } into g
                        select new RepSale
                        {
                            PdId = g.Key.PdId,
                            PdName = g.Key.PdName,
                            CdtlMoney = g.Sum(x => x.CdtlMoney),
                            CdtlQty = g.Sum(x => x.CdtlQty)
                        };
            ViewBag.theDate = thedate.Date.ToString("yyyy-MM-dd");
            return View(rep);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaleDaily(DateTime thedate)
        {
            var rep = from cd in _db.CartDtls

                      join p in _db.Products on cd.PdId equals p.PdId into join_cd_p
                      from cd_p in join_cd_p.DefaultIfEmpty()
                      join c in _db.Carts on cd.CartId equals c.CartId into join_cd
                      from c_cd in join_cd
                      where c_cd.CartDate == thedate
                      
                      group cd by new {cd.PdId,cd_p.PdName} into g
                      
                      select new RepSale
                      {
                          PdId = g.Key.PdId,
                          PdName = g.Key.PdName,
                          CdtlMoney = g.Sum(x=>x.CdtlMoney),
                          CdtlQty = g.Sum(x => x.CdtlQty)
                      };
            ViewBag.theDate = thedate.Date.ToString("yyyy-MM-dd");
            return View(rep);
        }
    }
}
