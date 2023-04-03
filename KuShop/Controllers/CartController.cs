using KuShop.Models;
using KuShop.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Threading;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace KuShop.Controllers
{
    public class CartController : Controller
    {
        //สร้าง Field สำหรับใช้งาน DBContext ที่กำหนด
        private readonly KuShopContext _db;
        //private object cart;

        //สร้าง Constructor สำหรับตัว Controller เมื่อเริ่มต้นให้ใช้ Obj ของ DBContext
        public CartController(KuShopContext db)
        { _db = db; }

        public IActionResult List(string cusid) 
        {
            var cart = from c in _db.Carts
                      where c.CusId.Equals(cusid)
                      orderby c.CartId descending
                      select c;
            return View(cart);
        }
        

        public IActionResult Confirm(string cartid)
        {
            ////////////////////////
            // ตรวจสอบ  Cart CF
            var cart = _db.Carts.Find(cartid);
            if(cart.CartCf!="N") 
            {
                //ถ้าตะกร้าไม่ถูกยืนยันเป็นเท็จ - คือถูกยืนยันแล้ว
                //ให้ย้ายไปหน้า Shop เลย
                return RedirectToAction("Shop", "Home");
            }

            /////////////////////////
            //หาสินค้าที่อยู่ใน Detail เพื่อไป ตัด Stock สินค้า
            var cartdtl = from ctd in _db.CartDtls
                          where ctd.CartId.Equals(cartid)
                          select ctd;
            int rowCount = cartdtl.ToList().Count;
            if (rowCount == 0)
            {
                TempData["Errormessage"] = "การยืนยันผิดพลาด";
                return RedirectToAction("Show", "Cart", new { cartid = cartid });
            }

            //หาสินค้าของแต่ละรายการใน Detail
            foreach (var detail in cartdtl)
            {
                //Update Product
                //จะมีการฟ้อง Error เพราะว่ามีการสร้าง Connection ไปที่  DbContext ซ้อนกัน
                //จะให้ Close ก่อน แต่แก้ได้โดยไป Set Connection String ใน appsettings.json
                //ให้เพิ่ม MultipleActiveResultSets=True ต่อท้าย
                Product pd = _db.Products.Find(detail.PdId);
                //ปรับค่า Stock ปัจจุบัน และ วันที่ขายวันสุดท้ายจากระบบ
                pd.PdStk = pd.PdStk - detail.CdtlQty;
                pd.PdLastSale = DateTime.Now.Date;
                _db.Entry(pd).State = EntityState.Modified;
            }
            _db.SaveChanges();

            ////////////////////////////
            //Update ตะกร้า Confirm แล้ว
            var master = _db.Carts.Find(cartid);
            if (master == null)
            {
                TempData["Errormessage"] = "ไม่พบตะกร้า";
                return RedirectToAction("Show", "Cart", new { cartid = cartid });
            }
            master.CartCf = "Y";
            _db.Entry(master).State = EntityState.Modified;
            _db.SaveChanges();

            /////////////////////
            //ลบ Session ตะกร้า
            HttpContext.Session.Remove("CartId");
            HttpContext.Session.Remove("CartQty");
            HttpContext.Session.Remove("CartMoney");

            TempData["Successmessage"] = "ยืนยันคำสั่งซื้อแล้ว";
            return RedirectToAction("Shop", "Home");
        }
        
        public IActionResult Check()
        {
            //ตรวจสอบตะกร้า ว่ามีของลูกค้าปัจจุบัน และยังไม่ได้ทำการ CF หรือไม่
            //ถ้ามีแล้วให้ใช้ CartId นั้น 
            string cusid = HttpContext.Session.GetString("CusId");
            var cart = from ct in _db.Carts
                       where ct.CusId.Equals(cusid) && ct.CartCf != "Y"
                       select ct;
            int rowCount = cart.ToList().Count;
            //ถ้ามีตะกร้า
            if (rowCount > 0)
            {
                Cart obj = new Cart();
                foreach (var item in cart)
                {
                    obj = item;
                }
                //กำหนด Session ต่างๆของตะกร้า
                HttpContext.Session.SetString("CartId", obj.CartId);
                HttpContext.Session.SetString("CartQty", obj.CartQty.ToString());
                HttpContext.Session.SetString("CartMoney", obj.CartMoney.ToString());
            }
            return RedirectToAction("Shop", "Home");
        }
        

        public IActionResult Delete(string cartid)
        {
            var master = _db.Carts.Find(cartid);
            ////////////////////////
            // ตรวจสอบ  Cart CF
            if (master.CartCf != "N")
            {
                //ถ้าตะกร้าไม่ถูกยืนยันเป็นเท็จ - คือถูกยืนยันแล้ว
                //ให้ย้ายไปหน้า Shop เลย
                return RedirectToAction("Shop", "Home");
            }
            ////////////////////////
            // ลบ Master
            if (master == null)
            {
                TempData["Errormessage"] = "ไม่พบตะกร้า";
                return RedirectToAction("Show", "Cart", new { cartid = cartid });
            }
            _db.Carts.Remove(master);
            _db.SaveChanges();

            ////////////////////////
            //ลบ Master แล้วต้องลบ Detail ด้วย
            //เลือกข้อมูลเป็น Set ต้องใช้ linq
            var detail = from ctd in _db.CartDtls 
                         where ctd.CartId== cartid
                         select ctd ;
            foreach (var item in detail)
            {
                _db.CartDtls.Remove(item);
            }
            _db.SaveChanges();

            ////////////////////////
            // ลบตะกร้าแล้ว ลบ Session ด้วย
            HttpContext.Session.Remove("CartId");
            HttpContext.Session.Remove("CartQty");
            HttpContext.Session.Remove("CartMoney");

            TempData["Successmessage"] = "ยกเลิกคำสั่งซื้อแล้ว";
            return RedirectToAction("Shop", "Home");
        }

        public IActionResult DeleteDtl(string pdid, string cartid)
        {
            var cfcart = _db.Carts.Find(cartid);
            ////////////////////////
            // ตรวจสอบ  Cart CF
            if (cfcart.CartCf != "N")
            {
                //ถ้าตะกร้าไม่ถูกยืนยันเป็นเท็จ - คือถูกยืนยันแล้ว
                //ให้ย้ายไปหน้า Shop เลย
                return RedirectToAction("Shop", "Home");
            }

            ////////////////////////
            ///ลย Detail
            var obj = _db.CartDtls.Find(cartid, pdid);
            if (obj == null)
            {
                TempData["Errormessage"] = "ไม่พบข้อมูล";
                return RedirectToAction("Show", "Cart", new { cartid = cartid });
            }
            _db.CartDtls.Remove(obj);
            _db.SaveChanges();

            //////////////// เหมือนตอน AddDtl
            //ปรับยอดMaster - Cart
            //หายอด Sum Dtl
            var cartmoney = _db.CartDtls.Where(a => a.CartId == cartid).Sum(b => b.CdtlMoney);
            var cartqty = _db.CartDtls.Where(a => a.CartId == cartid).Sum(b => b.CdtlQty);
            ////////////// ถ้าจำนวนสินค้าเป็น 0 ก็ลบตะกร้าทิ้งเลย
            if(cartqty==0)
            {
                /// ลบ Master
                var master = _db.Carts.Find(cartid);
                _db.Carts.Remove(master);
                _db.SaveChanges();

                // ลบตะกร้าแล้ว ลบ Session ด้วย
                HttpContext.Session.Remove("CartId");
                HttpContext.Session.Remove("CartQty");
                HttpContext.Session.Remove("CartMoney");

                TempData["Successmessage"] = "ยกเลิกคำสั่งซื้อแล้ว";
                return RedirectToAction("Shop", "Home");
            }
            else
            {
                //Update Cart
                var cart = _db.Carts.Find(cartid);
                cart.CartQty = cartqty;
                cart.CartMoney = cartmoney;
                _db.SaveChanges();
                
                /// Update Session
                HttpContext.Session.SetString("CartMoney", cartmoney.ToString());
                HttpContext.Session.SetString("CartQty", cartqty.ToString());

                return RedirectToAction("Show", "Cart", new { cartid = cartid });
            }
        }
        public IActionResult Show(string cartid) 
        {
            //ตรวจสอบตะกร้า
            if (cartid == null)
            {
                TempData["ErrorMessage"] = "ต้องระบุตะกร้า";
                return RedirectToAction("Index", "Home");
            }
            
            //ตรวจสอบว่าเป็นตะกร้าของลูกค้าที่ใช้งานอยู่หรือไม่
            //ได้ข้อมูล cart เป็นส่วน Master
            string cusid = HttpContext.Session.GetString("CusId");
            var cart = from ct in _db.Carts
                       where ct.CartId == cartid &&
                             ct.CusId == cusid
                       select ct;
            if (cart == null)
            {
                TempData["ErrorMessage"] = "ไม่พบตะกร้าที่ระบุ";
                return RedirectToAction("Index", "Home");
            }
            
            //เลือกข้อมูลของตะกร้าที่ระบุ 
            //สร้าง ViewModel ชื่อ CidVM เพื่อแสดงชื่อสินค้าของ CartDtl (ส่วน Detail)
            var cartdtl = from ctd in _db.CartDtls 
                          join p in _db.Products on ctd.PdId equals p.PdId into join_ctd_p
                          from ctd_p in join_ctd_p.DefaultIfEmpty()
                          where ctd.CartId == cartid
                          select new CtdVM 
                          { 
                            CartId = ctd.CartId,
                            PdId= ctd.PdId,
                            PdName = ctd_p.PdName,
                            CdtlMoney = ctd.CdtlMoney,
                            CdtlPrice = ctd.CdtlPrice,
                            CdtlQty = ctd.CdtlQty
                          };
            //สร้าง dynamic model
            //เพื่อส่งข้อมูลให้ View แบบสองตารางพร้อมกัน
            dynamic DyModel = new ExpandoObject();
            //แบ่งเป็นส่วน Master รับข้อมูลจาก Obj cart
            DyModel.Master =cart;
            //ส่วน Detail รับข้อมูลจาก Obj cartdtl
            DyModel.Detail = cartdtl;
            //ส่ง Dynamic Model ไปที่ View
            return View(DyModel); 
        }  
        public IActionResult AddDtl(string pdid)
        {
            //Login หรือยัง
            if(HttpContext.Session.GetString("CusId")==null)
            {
                TempData["ErrorMessage"] = "กรุณา Login ก่อนซื้อสินค้า";
                return RedirectToAction("Login", "Home");
            }

            //ตรวจสอบว่ามีการส่ง id มาหรือไม่
            if (pdid == null)
            {
                TempData["ErrorMessage"] = "ต้องระบุค่า ID";
                return RedirectToAction("Index","Home");
            }

            //ถ้ายังไม่มีตะกร้า
            if (HttpContext.Session.GetString("CartId") == null)
            {
                //ไปสร้างตะกร้า
                return RedirectToAction("Add", new { pdid = pdid });
            }

            /////////////////////////////
            //เอาสินค้าใส่ตะกร้า
            //หาข้อมูล-ราคาขาย-สินค้าที่เลือก
            var pd = _db.Products.Find(pdid);
            
            //ดูจำนวนเดิม
            //สร้าง Obj และระบุค่าใน Field ต่างๆ
            string cartid = HttpContext.Session.GetString("CartId");
            var cartdtl =   from ctd in _db.CartDtls
                            where ctd.CartId.Equals(cartid)
                                && ctd.PdId.Equals(pdid)
                            select ctd;
            if (cartdtl.ToList().Count == 0)
            {
                //insert CartDtl
                CartDtl obj = new CartDtl();
                obj.CartId = cartid;
                obj.PdId = pdid;
                obj.CdtlQty = 1; //บันทึกสินค้าเข้าครั้งแรกสินค้าเป็น 1
                obj.CdtlPrice = pd.PdPrice; //ราคาต่อชิ้นดึงมาจาก pd
                obj.CdtlMoney = pd.PdPrice * 1; //ยอดเงินรวมของรายการ = ราคา * จำนวน(1)
                _db.Entry(obj).State = EntityState.Added;
            }
            else //มีการซื้อสินค้านี้แล้ว ทำการเพิ่มจำนวนสินค้า คำนวน และ Update
            {
                //update CartDtl
                foreach (var obj in cartdtl)
                {
                    obj.CdtlQty = obj.CdtlQty + 1;
                    obj.CdtlMoney = pd.PdPrice * obj.CdtlQty;
                    _db.Entry(obj).State = EntityState.Modified;
                }
            }
            _db.SaveChanges();

            //ปรับยอดMaster - Cart
            //หายอด Sum Dtl
            var cartmoney = _db.CartDtls.Where(a => a.CartId == cartid).Sum(b => b.CdtlMoney);
            var cartqty = _db.CartDtls.Where(a => a.CartId == cartid).Sum(b => b.CdtlQty);
            //Update Cart
            var cart = _db.Carts.Find(cartid);
            cart.CartQty= cartqty;
            cart.CartMoney = cartmoney;
            _db.SaveChanges();

            HttpContext.Session.SetString("CartMoney", cartmoney.ToString());
            HttpContext.Session.SetString("CartQty", cartqty.ToString());
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Add(string pdid) 
        {
            if (HttpContext.Session.GetString("CusId") == null)
            {
                TempData["ErrorMessage"] = "กรุณา Login ก่อนซื้อสินค้า";
                return RedirectToAction("Login", "Home");
            }

            //ตรวจสอบว่ามีการส่ง id มาหรือไม่
            if (pdid == null)
            {
                TempData["ErrorMessage"] = "ต้องระบุค่ารหัสสินค้า";
                return RedirectToAction("Index", "Home");
            }

            //ตรวจสอบว่ามีตะกร้าหรือยัง
            //ถ้าไม่มีก็ GenID แล้วกำหนด Session
            if (HttpContext.Session.GetString("CartId")==null)
            {
                //GenID
                string theId;
                int rowCount;
                int i = 0;
                string cusid = HttpContext.Session.GetString("CusId");
                CultureInfo us = new CultureInfo("en-US");
                do
                {
                    //สร้าง id จากปีและเดือนปัจจุบัน และต่อด้วย String 000x
                    i++;
                    theId = string.Concat(DateTime.Now.ToString("'CT'yyyyMMdd", us), i.ToString("0000"));
                    //ทำการตรวจสอบว่ามี id ที่ซ้ำกันหรือไม่
                    //วนจนกว่าจะไม่ซ่ำ
                    var    cart = from ct in _db.Carts
                               where ct.CartId.Equals(theId)
                               select ct;
                    rowCount = cart.ToList().Count;
                }
                while (rowCount != 0);
                
                //สร้างตะกร้าใหม่
                try
                {
                    //สร้าง Obj และระบุค่าใน Field ต่างๆ
                    Cart obj = new Cart();
                    obj.CartId = theId;
                    obj.CusId = cusid;
                    obj.CartDate = DateTime.Now.Date;
                    obj.CartQty = 0;
                    obj.CartMoney = 0;
                    //กำหนดสถานะทำงานเป็น Add และสั่่งบันทึก
                    _db.Entry(obj).State = EntityState.Added;
                    _db.SaveChanges();

                    //กำหนด Session ต่างๆของตะกร้า
                    HttpContext.Session.SetString("CartId", theId);
                    HttpContext.Session.SetString("CartQty", "0");
                    HttpContext.Session.SetString("CartMoney", "0");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "การสร้างตะกร้าผิดพลาด";
                    return RedirectToAction("Index", "Home");
                }
            }

            //เรียก AddDtl พร้อมส่งรหัสสินค้า เพื่อเอาสินค้าใส่ตะกร้า 
            return RedirectToAction("AddDtl", new { pdid = pdid });
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
