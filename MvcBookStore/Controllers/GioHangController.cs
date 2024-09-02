using MvcBookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcBookStore.Controllers
{
    public class GioHangController : Controller
    {
        // GET: GioHang
        public List<MatHangMua> LayGioHang()
        {
            List<MatHangMua> gioHang = Session["GioHang"] as List<MatHangMua>;
            //Nếu giỏ hàng chưa tồn tại thì tạo mới và đưa vào Session
            if (gioHang == null)
            {
                gioHang = new List<MatHangMua>();
                Session["GioHang"] = gioHang;
            }
            return gioHang;
        }

        public ActionResult ThemSanPhamVaoGio(int MaSP)
        {
            //Lấy giỏ hàng hiện tại
            List<MatHangMua> gioHang = LayGioHang();

            //Kiểm tra xem có tồn tại mặt hàng trong giỏ hay chưa
            //Nếu có thì tăng số lượng lên 1, ngược lại thêm vào giỏ
            MatHangMua sanPham = gioHang.FirstOrDefault(s => s.MaSach == MaSP);
            if (sanPham == null) //Sản phẩm chưa có trong giỏ
            {
                sanPham = new MatHangMua(MaSP);
                gioHang.Add(sanPham);
            }
            else
            {
                sanPham.SoLuong++; //Sản phẩm đã có trong giỏ thì tăng số lượng lên 1
            }
            return RedirectToAction("Details", "BookStore", new { id = MaSP });
        }

        private int TinhTongSL()
        {
            int tongSL = 0;
            List<MatHangMua> gioHang = LayGioHang();
            if (gioHang != null)
                tongSL = gioHang.Sum(sp => sp.SoLuong);
            return tongSL;
        }

        private double TinhTongTien()
        {
            double TongTien = 0;
            List<MatHangMua> gioHang = LayGioHang();
            if (gioHang != null)
                TongTien = gioHang.Sum(sp => sp.ThanhTien());
            return TongTien;
        }

        public ActionResult HienThiGioHang()
        {
            List<MatHangMua> gioHang = LayGioHang();

            //Nếu giỏ hàng trống thì trả về trang ban đầu
            if (gioHang == null || gioHang.Count == 0)
            {
                return RedirectToAction("Index", "BookStore");
            }
            ViewBag.TongSL = TinhTongSL();
            ViewBag.TongTien = TinhTongTien();
            return View(gioHang); //Trả về View hiển thị thông tin giỏ hàng
        }

        public ActionResult GioHangPartial()
        {
            ViewBag.TongSL = TinhTongSL();
            ViewBag.TongTien = TinhTongTien();
            return PartialView();
        }

        public ActionResult XoaMatHang(int MaSP)
        {
            List<MatHangMua> gioHang = LayGioHang();

            //Lấy sản phẩm trong giỏ hàng
            var sanpham = gioHang.FirstOrDefault(s => s.MaSach == MaSP);
            if (sanpham != null)
            {
                gioHang.RemoveAll(s => s.MaSach == MaSP);
                return RedirectToAction("HienThiGioHang"); //Quay về trang giỏ hàng
            }
            if (gioHang.Count == 0) //Quay về trang chủ nếu giỏ hàng không có gì 
                return RedirectToAction("Index", "BookStore");
            return RedirectToAction("HienThiGioHang");
        }

        public ActionResult CapNhatMatHang(int MaSP, int SoLuong)
        {
            List<MatHangMua> gioHang = LayGioHang();
            //Lấy sản phẩm trong giỏ hàng
            var sanpham = gioHang.FirstOrDefault(s => s.MaSach == MaSP);

            if (sanpham != null)
            {
                // Cập nhật số lượng tương ứng
                //Lưu ý số lượng phải lớn hơn hoặc bằng 1
                sanpham.SoLuong = SoLuong;
            }
            return RedirectToAction("HienThiGioHang"); //Quay về trang giỏ hàng
        }

        public ActionResult DatHang()
        {
            if (Session["TaiKhoan"] == null) //Chưa đăng nhập
                return RedirectToAction("DangNhap", "NguoiDung");
            List<MatHangMua> gioHang = LayGioHang();
            if (gioHang == null || gioHang.Count == 0) //Chưa có giỏ hàng hoặc chưa có sp
                return RedirectToAction("Index", "BookStore");

            ViewBag.TongSL = TinhTongSL();
            ViewBag.TongTien = TinhTongTien();
            return View(gioHang); //Trả về View hiển thị thông tin giỏ hàng
        }

        QLBANSACHEntities database = new QLBANSACHEntities();

        public ActionResult DongYDatHang()
        {
            KHACHHANG khach = Session["TaiKhoan"] as KHACHHANG; //Khách
            List<MatHangMua> gioHang = LayGioHang(); //Giỏ hàng

            DONDATHANG DonHang = new DONDATHANG(); //Tạo mới đơn đặt hàng
            DonHang.MaKH = khach.MaKH;
            DonHang.NgayDH = DateTime.Now;
            DonHang.Trigia = (decimal)TinhTongTien();
            DonHang.Dagiao = false;
            DonHang.Tennguoinhan = khach.HoTenKH;
            DonHang.Diachinhan = khach.DiachiKH;
            DonHang.Dienthoainhan = khach.DienthoaiKH;
            DonHang.HTThanhtoan = false;
            DonHang.HTGiaohang = false;

            database.DONDATHANGs.Add(DonHang);
            database.SaveChanges();

            //Lần lượt thêm từng chi tiết cho đơn hàng trên
            foreach (var sanpham in gioHang)
            {
                CTDATHANG chitiet = new CTDATHANG();
                chitiet.SODH = DonHang.SODH;
                chitiet.Masach = sanpham.MaSach;
                chitiet.Soluong = sanpham.SoLuong;
                chitiet.Dongia = (decimal)sanpham.DonGia;
                database.CTDATHANGs.Add(chitiet);
            }
            database.SaveChanges();

            //Xoá giỏ hàng
            Session["GioHang"] = null;
            return RedirectToAction("HoanThanhDonHang");
        }

        public ActionResult HoanThanhDonHang()
        {
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }
    }
}