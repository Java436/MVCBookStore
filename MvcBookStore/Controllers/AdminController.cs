using MvcBookStore.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.IO;
using System.Drawing.Drawing2D;
using System.Net;

namespace MvcBookStore.Controllers
{
    public class AdminController : Controller
    {
        //Use DbContext to manage database
        QLBANSACHEntities database = new QLBANSACHEntities();

        // GET: Admin
        public ActionResult Index()
        {
            if (Session["Admin"] == null)
                return RedirectToAction("Login");
            return View();
        }

        // GET: Admin
        [HttpGet]
        public ActionResult Login() 
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(ADMIN admin)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(admin.UserAdmin))
                    ModelState.AddModelError(string.Empty, "User name không được để trống");
                if (string.IsNullOrEmpty(admin.PassAdmin))
                    ModelState.AddModelError(string.Empty, "Password không được để trống");

                //Kiểm tra có admin này hay chưa
                var adminDB = database.ADMINs.FirstOrDefault(ad => ad.UserAdmin == admin.UserAdmin && ad.PassAdmin == admin.PassAdmin);
                if (adminDB == null)
                    ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng");
                else
                {
                    Session["Admin"] = adminDB;
                    ViewBag.ThongBao = "Đăng nhập admin thành công";
                    return RedirectToAction("Index", "Admin");
                }
            }
            return View();
        }

        public ActionResult Sach(int? page)
        {
            var dsSach = database.SACHes.ToList();
            //Tạo biến cho biết số sách mỗi trang
            int pageSize = 7;
            //Tạo biến số trang
            int pageNum = (page ?? 1);
            return View(dsSach.OrderBy(sach => sach.Masach).ToPagedList(pageNum, pageSize));
        }

        public ActionResult ChuDe(int? page)
        {
            var dsChuDE = database.CHUDEs.ToList();
            //Tạo biến cho biết số sách mỗi trang
            int pageSize = 7;
            //Tạo biến số trang
            int pageNum = (page ?? 1);
            return View(dsChuDE.OrderBy(sach => sach.MaCD).ToPagedList(pageNum, pageSize));
        }

        public ActionResult NhaXuatBan(int? page)
        {
            var dsChuDE = database.NHAXUATBANs.ToList();
            //Tạo biến cho biết số sách mỗi trang
            int pageSize = 7;
            //Tạo biến số trang
            int pageNum = (page ?? 1);
            return View(dsChuDE.OrderBy(sach => sach.MaNXB).ToPagedList(pageNum, pageSize));
        }

        //Tạo mới sách
        [HttpGet]
        public ActionResult ThemSach()
        {
            ViewBag.MaCD = new SelectList(database.CHUDEs.ToList(), "MaCD", "TenChuDe");
            ViewBag.MaNXB = new SelectList(database.NHAXUATBANs.ToList(), "MaNXB", "TenNXB");
            return View();
        }

        [HttpPost]
        public ActionResult ThemSach(SACH sach, HttpPostedFileBase Hinhminhhoa)
        {
            ViewBag.MaCD = new SelectList(database.CHUDEs.ToList(), "MaCD", "TenChuDe");
            ViewBag.MaNXB = new SelectList(database.NHAXUATBANs.ToList(), "MaNXB", "TenNXB");

            if (Hinhminhhoa == null)
            {
                ViewBag.ThongBao = "Vui lòng chọn ảnh bìa";
                return View();
            }
            else
            {
                if (ModelState.IsValid) //Nếu dữ liệu sách đầy đủ
                {
                    //Lấy tên file của hình được up lên
                    var fileName = Path.GetFileName(Hinhminhhoa.FileName);

                    //Tạo đường dẫn tới file
                    var path = Path.Combine(Server.MapPath("~/Images"), fileName);

                    //Kiểm tra hình đã tồn tại trong hệ thống chưa
                    if (System.IO.File.Exists(path))
                    {
                        ViewBag.ThongBao = "Hình đã tồn tại";
                    }
                    else
                    {
                        Hinhminhhoa.SaveAs(path); //Lưu vào hệ thống
                    }
                    //Lưu tên sách vào trường Hinhminhhoa
                    sach.Hinhminhhoa = fileName;
                    //Lưu vào CSDL
                    database.SACHes.Add(sach);
                    database.SaveChanges();
                }
                return RedirectToAction("Sach");
            }    
        }

        public ActionResult ChiTietSach(int id)
        {
            var sach = database.SACHes.FirstOrDefault(s => s.Masach == id);
            if (sach == null) //Không thấy sách
            {
                Response.StatusCode = 404;
                return null;
            }
            return View(sach); //Hiển thị thông tin sách cần
        }

        public ActionResult SuaSach(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var sach = database.SACHes.FirstOrDefault(s => s.Masach == id);
            if (sach == null)
            {
                return HttpNotFound();
            }
            ViewBag.MaCD = new SelectList(database.CHUDEs.ToList(), "MaCD", "TenChuDe");
            ViewBag.MaNXB = new SelectList(database.NHAXUATBANs.ToList(), "MaNXB", "TenNXB");
            return View(sach);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaSach([Bind(Include = "Masach,TenSach,Donvitinh,Dongia,Mota,Hinhminhhoa,MaCD,MaNXB,Ngaycapnhat,Soluongban,solanxem")] SACH sach, HttpPostedFileBase Hinhminhhoa)
        {
            if (ModelState.IsValid)
            {
                if (Hinhminhhoa != null)
                {
                    var fileName = Path.GetFileName(Hinhminhhoa.FileName);
                    var path = Path.Combine(Server.MapPath("~/image"), fileName);
                    sach.Hinhminhhoa = fileName;
                    Hinhminhhoa.SaveAs(path);
                }

                database.Entry(sach).State = EntityState.Modified;
                database.SaveChanges();
                return RedirectToAction("Sach");
            }
            ViewBag.MaCD = new SelectList(database.CHUDEs.ToList(), "MaCD", "TenChuDe");
            ViewBag.MaNXB = new SelectList(database.NHAXUATBANs.ToList(), "MaNXB", "TenNXB");
            return View(sach);
        }

        public ActionResult XoaSach(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var sach = database.SACHes.FirstOrDefault(s => s.Masach == id);
            if (sach == null)
            {
                return HttpNotFound();
            }
            return View(sach);
        }

        [HttpPost, ActionName("XoaSach")]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhanXoa(int id)
        {
            var sach = database.SACHes.FirstOrDefault(s => s.Masach == id);
            database.SACHes.Remove(sach);
            database.SaveChanges();
            return RedirectToAction("Sach");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                database.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}