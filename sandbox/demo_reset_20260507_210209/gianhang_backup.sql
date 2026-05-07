-- MariaDB dump 10.19  Distrib 10.4.32-MariaDB, for Win64 (AMD64)
--
-- Host: 127.0.0.1    Database: gianhang
-- ------------------------------------------------------
-- Server version	10.4.32-MariaDB

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `admin`
--

DROP TABLE IF EXISTS `admin`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `admin` (
  `idAdmin` int(11) NOT NULL AUTO_INCREMENT,
  `idTaiKhoan` int(11) NOT NULL,
  `hoTen` varchar(150) DEFAULT NULL,
  `ghiChu` varchar(255) DEFAULT NULL,
  `ngayTao` datetime NOT NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`idAdmin`),
  UNIQUE KEY `uq_admin_idTaiKhoan` (`idTaiKhoan`),
  CONSTRAINT `fk_admin_taikhoan` FOREIGN KEY (`idTaiKhoan`) REFERENCES `taikhoan` (`idTaiKhoan`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `admin`
--

LOCK TABLES `admin` WRITE;
/*!40000 ALTER TABLE `admin` DISABLE KEYS */;
INSERT INTO `admin` VALUES (1,1,'Quan tri he thong','Tai khoan admin mac dinh','2026-03-26 22:17:19');
/*!40000 ALTER TABLE `admin` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `chitiethoadon`
--

DROP TABLE IF EXISTS `chitiethoadon`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `chitiethoadon` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `idHoaDon` int(11) NOT NULL,
  `idMonAn` int(11) NOT NULL,
  `soLuong` int(11) NOT NULL DEFAULT 1,
  `donGia` decimal(12,2) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_chitiethoadon_idHoaDon` (`idHoaDon`),
  KEY `idx_chitiethoadon_idMonAn` (`idMonAn`),
  CONSTRAINT `fk_chiTietHoaDon_hoaDon` FOREIGN KEY (`idHoaDon`) REFERENCES `hoadon` (`idHoaDon`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_chiTietHoaDon_monAn` FOREIGN KEY (`idMonAn`) REFERENCES `monan` (`idMonAn`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `chitiethoadon`
--

LOCK TABLES `chitiethoadon` WRITE;
/*!40000 ALTER TABLE `chitiethoadon` DISABLE KEYS */;
INSERT INTO `chitiethoadon` VALUES (1,1,1,1,20000.00),(2,1,3,1,30000.00),(3,2,4,1,35000.00),(4,2,5,2,15000.00);
/*!40000 ALTER TABLE `chitiethoadon` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `chu_quan_ly`
--

DROP TABLE IF EXISTS `chu_quan_ly`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `chu_quan_ly` (
  `idChuQuanLy` int(11) NOT NULL AUTO_INCREMENT,
  `idTaiKhoan` int(11) NOT NULL,
  `hoTen` varchar(150) DEFAULT NULL,
  `sdt` varchar(20) DEFAULT NULL,
  `diaChi` varchar(255) DEFAULT NULL,
  `ngayTao` datetime NOT NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`idChuQuanLy`),
  UNIQUE KEY `uq_chuquanly_idTaiKhoan` (`idTaiKhoan`),
  CONSTRAINT `fk_chuquanly_taikhoan` FOREIGN KEY (`idTaiKhoan`) REFERENCES `taikhoan` (`idTaiKhoan`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `chu_quan_ly`
--

LOCK TABLES `chu_quan_ly` WRITE;
/*!40000 ALTER TABLE `chu_quan_ly` DISABLE KEYS */;
INSERT INTO `chu_quan_ly` VALUES (1,3,'Nguyen Van A','0909000009','Quan 4','2026-04-08 23:22:50');
/*!40000 ALTER TABLE `chu_quan_ly` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `gianhang`
--

DROP TABLE IF EXISTS `gianhang`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `gianhang` (
  `idGianHang` int(11) NOT NULL AUTO_INCREMENT,
  `idChuQuanLy` int(11) DEFAULT NULL,
  `ten` varchar(150) NOT NULL,
  `diaChi` varchar(255) DEFAULT NULL,
  `lat` decimal(10,7) DEFAULT NULL,
  `lon` decimal(10,7) DEFAULT NULL,
  `vongBo` decimal(8,2) NOT NULL DEFAULT 10.00,
  `luotTruyCap` int(11) NOT NULL DEFAULT 0,
  `tinhTrang` enum('dang_hoat_dong','tam_ngung','dong_cua') NOT NULL DEFAULT 'dang_hoat_dong',
  `phiHangThang` decimal(12,2) NOT NULL DEFAULT 0.00,
  `ngayDangKy` datetime NOT NULL DEFAULT current_timestamp(),
  `thoiGianCapNhat` datetime DEFAULT NULL,
  PRIMARY KEY (`idGianHang`),
  KEY `idx_gianhang_idChuQuanLy` (`idChuQuanLy`),
  KEY `idx_gianhang_toado` (`lat`,`lon`),
  CONSTRAINT `fk_gianhang_chuquanly` FOREIGN KEY (`idChuQuanLy`) REFERENCES `chu_quan_ly` (`idChuQuanLy`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=18 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `gianhang`
--

LOCK TABLES `gianhang` WRITE;
/*!40000 ALTER TABLE `gianhang` DISABLE KEYS */;
INSERT INTO `gianhang` VALUES (1,1,'Banh trang nuong Co Ba','Khu A - Pho am thuc Vinh Khanh',10.7626220,106.6601720,10.00,107,'dang_hoat_dong',500000.00,'2026-03-26 22:17:19','2026-04-17 08:30:14'),(2,1,'Tra sua May','Khu B - Pho am thuc Vinh Khanh',10.7631000,106.6609000,10.00,33,'dang_hoat_dong',701000.00,'2026-03-26 22:17:19','2026-05-04 19:27:18'),(3,1,'Xien que 88','Khu C - Pho am thuc Vinh Khanh',10.7634000,106.6611000,10.00,352,'dang_hoat_dong',650000.00,'2026-03-26 22:17:19','2026-05-04 19:41:03'),(14,1,'Tokyo Takoyaki','3 Trần duy',10.7624000,106.6611720,10.00,1,'dang_hoat_dong',100000.00,'2026-05-04 19:32:30','2026-05-07 20:54:46'),(15,1,'221','Khu A - Pho am thuc Vinh Khanh',21.0000000,23.0000000,10.00,1,'dong_cua',1000.00,'2026-05-07 20:33:23','2026-05-07 20:43:47'),(16,1,'skis','3 Trần duy',0.0000040,0.0000050,10.00,0,'dong_cua',2000.00,'2026-05-07 20:40:13','2026-05-07 20:43:38'),(17,1,'Xien que 882','Khu A - Pho am thuc Vinh Khanh',0.0000040,0.0000040,10.00,0,'dang_hoat_dong',4000.00,'2026-05-07 20:56:02','2026-05-07 20:56:02');
/*!40000 ALTER TABLE `gianhang` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `gianhangngonngu`
--

DROP TABLE IF EXISTS `gianhangngonngu`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `gianhangngonngu` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `idGianHang` int(11) NOT NULL,
  `idNgonNgu` int(11) NOT NULL,
  `ten` varchar(150) NOT NULL,
  `audioURL` varchar(255) DEFAULT NULL,
  `moTa` text DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_gianHang_ngonNgu` (`idGianHang`,`idNgonNgu`),
  KEY `idx_gianhangngonngu_idNgonNgu` (`idNgonNgu`),
  CONSTRAINT `fk_gianHangNgonNgu_gianHang` FOREIGN KEY (`idGianHang`) REFERENCES `gianhang` (`idGianHang`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_gianHangNgonNgu_ngonNgu` FOREIGN KEY (`idNgonNgu`) REFERENCES `ngonngu` (`idNgonNgu`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=34 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `gianhangngonngu`
--

LOCK TABLES `gianhangngonngu` WRITE;
/*!40000 ALTER TABLE `gianhangngonngu` DISABLE KEYS */;
INSERT INTO `gianhangngonngu` VALUES (1,1,1,'Banh trang nuong Co Ba','audio/gianhang_1_vi.mp3','Banh trang nuong gion, thom, an kem trung, hanh la va sot dac trung.'),(2,1,2,'Co Ba Grilled Rice Paper','audio/gianhang_1_en.mp3','A booth specializing in crispy and flavorful traditional grilled rice paper.'),(3,2,1,'Tra sua May','audio/gianhang_2_vi.mp3','Tra sua vi nhe, de uong, phu hop cho du khach di bo tham quan pho am thuc.'),(4,2,2,'May Milk Tea','audio/gianhang_2_en.mp3','A milk tea booth serving familiar drinks with a light and refreshing taste.'),(5,3,1,'Xien que 88','audio/gianhang_3_vi.mp3','Gian hang xien que voi nhieu lua chon an vat nong hoi va de thuong.'),(6,3,2,'Skewer 88','audio/gianhang_3_en.mp3','A street-food booth serving assorted skewers that are easy to grab and enjoy.'),(7,1,3,'Banh trang nuong Co Ba','audio/gianhang_1_ko.mp3','sigma beo skibidi dop dop'),(11,1,4,'Banh trang nuong Co Ba','audio/gianhang_1_ja.mp3','???????????????'),(23,14,1,'Tokyo Takoyaki','audio/gianhang_14_vi.mp3','Chào cậu'),(24,15,1,'221','audio/gianhang_15_vi.mp3','ssss'),(26,16,1,'skis','audio/gianhang_16_vi.mp3','sa'),(33,17,1,'Xien que 882','audio/gianhang_17_vi.mp3','sda');
/*!40000 ALTER TABLE `gianhangngonngu` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `goidichvu`
--

DROP TABLE IF EXISTS `goidichvu`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `goidichvu` (
  `idGoi` int(11) NOT NULL AUTO_INCREMENT,
  `ten` varchar(150) NOT NULL,
  `moTa` text DEFAULT NULL,
  `gia` decimal(12,2) NOT NULL DEFAULT 0.00,
  `thoiHanNgay` int(11) NOT NULL DEFAULT 1,
  `trangThai` enum('hoat_dong','tam_ngung','ngung_ap_dung') NOT NULL DEFAULT 'hoat_dong',
  `ngayTao` datetime NOT NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`idGoi`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `goidichvu`
--

LOCK TABLES `goidichvu` WRITE;
/*!40000 ALTER TABLE `goidichvu` DISABLE KEYS */;
INSERT INTO `goidichvu` VALUES (1,'Goi tham quan ngay','Truy cap app trong 1 ngay cho du khach.',15000.00,1,'hoat_dong','2026-04-15 02:00:17'),(2,'Goi tham quan tuan','Truy cap app trong 7 ngay cho du khach.',70000.00,7,'hoat_dong','2026-04-15 02:00:17'),(3,'Goi tieu chuan gian hang','Goi duy tri gian hang theo chu ky thang.',500000.00,30,'hoat_dong','2026-04-15 02:00:17'),(4,'admin','for testing',1000.00,31,'ngung_ap_dung','2026-05-07 20:06:45'),(5,'For_admin','testing',2000.00,30000,'hoat_dong','2026-05-07 20:10:59');
/*!40000 ALTER TABLE `goidichvu` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `hinhanhgianhang`
--

DROP TABLE IF EXISTS `hinhanhgianhang`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `hinhanhgianhang` (
  `idHinhAnh` int(11) NOT NULL AUTO_INCREMENT,
  `idGianHang` int(11) NOT NULL,
  `duongDan` varchar(255) NOT NULL,
  PRIMARY KEY (`idHinhAnh`),
  KEY `idx_hinhanhgianhang_idGianHang` (`idGianHang`),
  CONSTRAINT `fk_hinhAnhGianHang_gianHang` FOREIGN KEY (`idGianHang`) REFERENCES `gianhang` (`idGianHang`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=138 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `hinhanhgianhang`
--

LOCK TABLES `hinhanhgianhang` WRITE;
/*!40000 ALTER TABLE `hinhanhgianhang` DISABLE KEYS */;
INSERT INTO `hinhanhgianhang` VALUES (130,1,'chucchich.jpg'),(131,2,'mypham.jpg'),(132,3,'tet.jpg'),(134,14,'images/stores/store_14_20260506140120480.jpg'),(135,15,'images/stores/store_15_20260507133943548.jpg'),(136,16,'images/stores/store_16_20260507134021350.jpg'),(137,17,'images/stores/store_17_20260507135603099.jpg');
/*!40000 ALTER TABLE `hinhanhgianhang` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `hinhanhmonan`
--

DROP TABLE IF EXISTS `hinhanhmonan`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `hinhanhmonan` (
  `idHinhAnh` int(11) NOT NULL AUTO_INCREMENT,
  `idMonAn` int(11) NOT NULL,
  `duongDan` varchar(255) NOT NULL,
  PRIMARY KEY (`idHinhAnh`),
  KEY `idx_hinhanhmonan_idMonAn` (`idMonAn`),
  CONSTRAINT `fk_hinhAnhMonAn_monAn` FOREIGN KEY (`idMonAn`) REFERENCES `monan` (`idMonAn`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `hinhanhmonan`
--

LOCK TABLES `hinhanhmonan` WRITE;
/*!40000 ALTER TABLE `hinhanhmonan` DISABLE KEYS */;
INSERT INTO `hinhanhmonan` VALUES (1,1,'images/foods/food_1_20260507132707769.jpg'),(2,2,'images/foods/food_2_20260507133244844.jfif'),(3,3,'ts_truyenthong.jpg'),(4,4,'tra_dao_cam_sa.jpg'),(5,5,'xien_bo_vien.jpg'),(6,6,'xien_ca_vien.jpg');
/*!40000 ALTER TABLE `hinhanhmonan` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `hoadon`
--

DROP TABLE IF EXISTS `hoadon`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `hoadon` (
  `idHoaDon` int(11) NOT NULL AUTO_INCREMENT,
  `idKhachHang` int(11) DEFAULT NULL,
  `idPhienVaoApp` bigint(20) DEFAULT NULL,
  `idGoi` int(11) DEFAULT NULL,
  `maThanhToan` varchar(32) DEFAULT NULL,
  `maThietBi` varchar(100) DEFAULT NULL,
  `email` varchar(100) DEFAULT NULL,
  `guiEmail` tinyint(1) NOT NULL DEFAULT 0,
  `tongTien` decimal(12,2) NOT NULL DEFAULT 0.00,
  `thoiGianTao` datetime NOT NULL DEFAULT current_timestamp(),
  `thoiGianThanhToan` datetime DEFAULT NULL,
  `tinhTrang` enum('moi_tao','da_thanh_toan','da_huy','het_han') NOT NULL DEFAULT 'moi_tao',
  `cassoTransactionId` varchar(80) DEFAULT NULL,
  `ghiChu` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`idHoaDon`),
  UNIQUE KEY `uq_hoadon_maThanhToan` (`maThanhToan`),
  KEY `idx_hoadon_idKhachHang` (`idKhachHang`),
  KEY `idx_hoadon_idPhienVaoApp` (`idPhienVaoApp`),
  KEY `idx_hoadon_idGoi` (`idGoi`),
  KEY `idx_hoadon_maThietBi` (`maThietBi`),
  KEY `idx_hoadon_cassoTransactionId` (`cassoTransactionId`),
  CONSTRAINT `fk_hoaDon_khachHang` FOREIGN KEY (`idKhachHang`) REFERENCES `khachhang` (`idKhachHang`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `fk_hoadon_goidichvu` FOREIGN KEY (`idGoi`) REFERENCES `goidichvu` (`idGoi`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `fk_hoadon_phienvaoapp` FOREIGN KEY (`idPhienVaoApp`) REFERENCES `phien_vao_app` (`id`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=47 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `hoadon`
--

LOCK TABLES `hoadon` WRITE;
/*!40000 ALTER TABLE `hoadon` DISABLE KEYS */;
INSERT INTO `hoadon` VALUES (1,1,NULL,NULL,NULL,NULL,'kh1@gmail.com',0,50000.00,'2026-03-26 22:17:19',NULL,'moi_tao',NULL,'It cay'),(2,2,NULL,NULL,NULL,NULL,'kh2@gmail.com',0,65000.00,'2026-03-26 22:17:19',NULL,'da_thanh_toan',NULL,'Khong da'),(3,1,NULL,1,NULL,NULL,'kh1@gmail.com',0,15000.00,'2026-04-15 02:00:17',NULL,'da_thanh_toan',NULL,'Mua goi tham quan ngay'),(4,NULL,NULL,1,NULL,NULL,'ledat241205@gmail.com',0,15000.00,'2026-04-15 23:39:30',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(5,NULL,NULL,1,NULL,NULL,'ledat241205@gmail.com',0,15000.00,'2026-04-15 23:58:04',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(6,NULL,NULL,1,NULL,NULL,'ledat241205@gmail.com',0,15000.00,'2026-04-16 00:04:41',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(7,NULL,NULL,1,NULL,NULL,'ledat241205@gmail.com',0,15000.00,'2026-04-16 00:09:44',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(8,NULL,NULL,1,NULL,NULL,'ledat241205@gmail.com',0,15000.00,'2026-04-16 00:14:21',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(9,NULL,NULL,1,NULL,NULL,'ledat241205@gmail.com',0,15000.00,'2026-04-16 00:22:15',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(10,NULL,NULL,1,NULL,NULL,'ledat241205@gmail.com',0,15000.00,'2026-04-16 01:13:24',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(11,NULL,NULL,1,NULL,NULL,'ledat241205@gmail.com',0,15000.00,'2026-04-16 14:52:45',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(12,NULL,NULL,1,NULL,NULL,'caohoangthinh2@gmail.com',0,15000.00,'2026-04-16 14:53:33',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(13,NULL,NULL,1,NULL,NULL,'ledat241205@gmail.com',0,15000.00,'2026-04-16 14:55:35',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(14,NULL,NULL,1,NULL,NULL,'ledat241205@gmail.com',0,15000.00,'2026-04-16 14:55:51',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(15,NULL,NULL,1,NULL,NULL,'ledat241205@gmail.com',0,15000.00,'2026-04-17 07:27:37',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(16,NULL,NULL,1,NULL,NULL,'ledat241205@gmail.com',0,15000.00,'2026-04-20 17:42:56',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(17,NULL,NULL,3,NULL,NULL,'caohoangthinh2@gmail.com',0,500000.00,'2026-04-20 07:32:45',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(18,NULL,NULL,1,NULL,NULL,'ledat241205@gmail.com',0,15000.00,'2026-04-20 08:12:55',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(19,NULL,NULL,1,NULL,NULL,'ledat241205@gmail.com',0,15000.00,'2026-04-20 09:14:14',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(20,NULL,NULL,1,NULL,NULL,'ledat241205@gmail.com',0,15000.00,'2026-04-20 09:50:00',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(21,NULL,NULL,1,NULL,NULL,'ledat241204@gmail.com',0,15000.00,'2026-04-20 09:54:23',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(22,NULL,20,3,NULL,NULL,'caohoangthinh2@gmail.com',0,500000.00,'2026-04-21 07:30:29',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(23,NULL,21,3,NULL,NULL,'vobao142@gmail.com',0,500000.00,'2026-04-21 07:36:20',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(24,NULL,NULL,1,NULL,NULL,'ledat241205@gmail.com',0,15000.00,'2026-04-22 01:37:00',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(25,NULL,NULL,1,'CSAT25','APP-CLIENT-2DEBB7C666FC4D8FB67586A2ADA1B403',NULL,0,15000.00,'2026-04-23 00:12:06','2026-04-23 00:12:00','da_thanh_toan','14146688','Casso xac nhan: 126332855812-CSAT25 G1 DA1B403-CHUYEN TIEN-OQCH000APr6p-MOMO126332855812MOMO'),(26,NULL,NULL,1,'CSAT26','APP-CLIENT-2DEBB7C666FC4D8FB67586A2ADA1B403',NULL,0,15000.00,'2026-04-23 00:16:37','2026-04-23 00:17:00','da_thanh_toan','14146723','Casso xac nhan: 126332908818-CSAT26 G1 DA1B403-CHUYEN TIEN-OQCH000APrPT-MOMO126332908818MOMO'),(27,NULL,NULL,1,'CSAT27','APP-CLIENT-2DEBB7C666FC4D8FB67586A2ADA1B403',NULL,0,15000.00,'2026-04-23 00:24:24',NULL,'moi_tao',NULL,'VietQR CSAT27 G1 DA1B403; goi=1; thietBi=APP-CLIENT-2DEBB7C666FC4D8FB67586A2ADA1B403'),(28,NULL,NULL,1,'CSAT28','APP-CLIENT-2DEBB7C666FC4D8FB67586A2ADA1B403',NULL,0,15000.00,'2026-04-23 00:26:58','2026-04-23 00:27:00','da_thanh_toan','14146792','Casso xac nhan: 126333636282-CSAT28 G1 DA1B403-CHUYEN TIEN-OQCH000APs2V-MOMO126333636282MOMO'),(29,NULL,NULL,1,'CSAT29','APP-CLIENT-105CBB0F06044344A4E57D5373837CC3',NULL,0,15000.00,'2026-04-23 01:22:44',NULL,'moi_tao',NULL,'VietQR CSAT29 G1 D837CC3; goi=1; thietBi=APP-CLIENT-105CBB0F06044344A4E57D5373837CC3'),(30,NULL,NULL,1,NULL,NULL,NULL,0,15000.00,'2026-04-23 01:22:53',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(31,NULL,NULL,1,'CSAT31','APP-CLIENT-DA95C3C4A3CB4AB09AB8FF6D02E28D70',NULL,0,15000.00,'2026-04-23 14:16:58',NULL,'moi_tao',NULL,'VietQR CSAT31 G1 DE28D70; goi=1; thietBi=APP-CLIENT-DA95C3C4A3CB4AB09AB8FF6D02E28D70'),(32,NULL,NULL,1,NULL,NULL,NULL,0,15000.00,'2026-04-23 14:17:02',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(33,NULL,NULL,1,'CSAT33','APP-CLIENT-001A3B51EDAA47CF9BFEC7B9E3545783',NULL,0,15000.00,'2026-04-24 08:08:38',NULL,'moi_tao',NULL,'VietQR CSAT33 G1 D545783; goi=1; thietBi=APP-CLIENT-001A3B51EDAA47CF9BFEC7B9E3545783'),(34,NULL,NULL,1,NULL,NULL,NULL,0,15000.00,'2026-04-24 08:08:41',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(35,NULL,29,2,'CSAT35','APP-CLIENT-001A3B51EDAA47CF9BFEC7B9E3545783',NULL,0,70000.00,'2026-04-24 08:11:36','2026-04-24 08:12:00','da_thanh_toan','14157775','Casso xac nhan: 126481375736-CSAT35 G2 D545783-CHUYEN TIEN-OQCH000ATQsQ-MOMO126481375736MOMO'),(36,NULL,NULL,2,'CSAT36','APP-CLIENT-25BCBD37F168464DA77ACC67698D17B2',NULL,0,70000.00,'2026-04-27 09:32:21',NULL,'moi_tao',NULL,'VietQR CSAT36 G2 D8D17B2; goi=2; thietBi=APP-CLIENT-25BCBD37F168464DA77ACC67698D17B2'),(37,NULL,30,2,NULL,NULL,NULL,0,70000.00,'2026-04-27 09:32:24',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(38,NULL,NULL,2,'CSAT38','APP-CLIENT-E218502326004E439A51EBEE4B0DD316',NULL,0,70000.00,'2026-05-04 20:29:45',NULL,'moi_tao',NULL,'PayOS CSAT38 G2 D0DD316; goi=2; thietBi=APP-CLIENT-E218502326004E439A51EBEE4B0DD316'),(39,NULL,31,2,NULL,NULL,NULL,0,70000.00,'2026-05-04 20:29:48',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(40,NULL,NULL,1,'CSAT40','APP-CLIENT-E218502326004E439A51EBEE4B0DD316',NULL,0,15000.00,'2026-05-07 20:06:02',NULL,'moi_tao',NULL,'PayOS CSAT40 G1 D0DD316; goi=1; thietBi=APP-CLIENT-E218502326004E439A51EBEE4B0DD316'),(41,NULL,32,1,NULL,NULL,NULL,0,15000.00,'2026-05-07 20:06:06',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(42,NULL,NULL,4,'CSAT42','APP-CLIENT-E218502326004E439A51EBEE4B0DD316',NULL,0,1000.00,'2026-05-07 20:07:18',NULL,'moi_tao',NULL,'PayOS CSAT42 G4 D0DD316; goi=4; thietBi=APP-CLIENT-E218502326004E439A51EBEE4B0DD316'),(43,NULL,33,4,NULL,NULL,NULL,0,1000.00,'2026-05-07 20:08:06',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.'),(44,NULL,NULL,5,'CSAT44','APP-CLIENT-E218502326004E439A51EBEE4B0DD316',NULL,0,2000.00,'2026-05-07 20:11:28',NULL,'moi_tao',NULL,'PayOS CSAT44 G5 D0DD316; goi=5; thietBi=APP-CLIENT-E218502326004E439A51EBEE4B0DD316'),(45,NULL,NULL,5,'CSAT45','APP-CLIENT-E218502326004E439A51EBEE4B0DD316',NULL,0,2000.00,'2026-05-07 20:15:16',NULL,'moi_tao',NULL,'PayOS CSAT45 G5 D0DD316; goi=5; thietBi=APP-CLIENT-E218502326004E439A51EBEE4B0DD316'),(46,NULL,34,5,NULL,NULL,NULL,0,2000.00,'2026-05-07 20:15:28',NULL,'da_thanh_toan',NULL,'Bypass thanh toan QR de test package access.');
/*!40000 ALTER TABLE `hoadon` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `hoadongianhang`
--

DROP TABLE IF EXISTS `hoadongianhang`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `hoadongianhang` (
  `idHoaDonGianHang` int(11) NOT NULL AUTO_INCREMENT,
  `idGianHang` int(11) NOT NULL,
  `tongTien` decimal(12,2) NOT NULL DEFAULT 0.00,
  `ngayHetHan` datetime DEFAULT NULL,
  `trangThai` enum('chua_thanh_toan','da_thanh_toan','qua_han','da_huy') NOT NULL DEFAULT 'chua_thanh_toan',
  `ghiChu` varchar(255) DEFAULT NULL,
  `ngayTao` datetime NOT NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`idHoaDonGianHang`),
  KEY `idx_hoadongianhang_idGianHang` (`idGianHang`),
  CONSTRAINT `fk_hoadongianhang_gianhang` FOREIGN KEY (`idGianHang`) REFERENCES `gianhang` (`idGianHang`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `hoadongianhang`
--

LOCK TABLES `hoadongianhang` WRITE;
/*!40000 ALTER TABLE `hoadongianhang` DISABLE KEYS */;
INSERT INTO `hoadongianhang` VALUES (1,1,500000.00,'2026-05-15 02:00:17','chua_thanh_toan','Phi duy tri thang hien tai','2026-04-15 02:00:17'),(2,2,701000.00,'2026-05-15 02:00:17','chua_thanh_toan','Phi duy tri thang hien tai','2026-04-15 02:00:17'),(3,3,650000.00,'2026-05-15 02:00:17','chua_thanh_toan','Phi duy tri thang hien tai','2026-04-15 02:00:17'),(8,14,100000.00,'2026-06-04 19:32:30','da_thanh_toan','Phí duy trì tháng đầu tiên','2026-05-04 19:32:30'),(9,17,4000.00,'2026-06-07 20:56:02','chua_thanh_toan','Phi duy tri thang dau tien','2026-05-07 20:56:02');
/*!40000 ALTER TABLE `hoadongianhang` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `khachhang`
--

DROP TABLE IF EXISTS `khachhang`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `khachhang` (
  `idKhachHang` int(11) NOT NULL AUTO_INCREMENT,
  `idTaiKhoan` int(11) NOT NULL,
  `sdt` varchar(20) DEFAULT NULL,
  PRIMARY KEY (`idKhachHang`),
  UNIQUE KEY `uq_khachhang_idTaiKhoan` (`idTaiKhoan`),
  CONSTRAINT `fk_khachHang_taiKhoan` FOREIGN KEY (`idTaiKhoan`) REFERENCES `taikhoan` (`idTaiKhoan`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `khachhang`
--

LOCK TABLES `khachhang` WRITE;
/*!40000 ALTER TABLE `khachhang` DISABLE KEYS */;
INSERT INTO `khachhang` VALUES (1,4,'0909000001'),(2,2,'0909000002');
/*!40000 ALTER TABLE `khachhang` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `luot_truy_cap_ngay`
--

DROP TABLE IF EXISTS `luot_truy_cap_ngay`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `luot_truy_cap_ngay` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `idGianHang` int(11) NOT NULL,
  `ngay` date NOT NULL,
  `soLuot` int(11) NOT NULL DEFAULT 1,
  PRIMARY KEY (`id`),
  UNIQUE KEY `idx_gianhang_ngay` (`idGianHang`,`ngay`)
) ENGINE=InnoDB AUTO_INCREMENT=5117 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `luot_truy_cap_ngay`
--

LOCK TABLES `luot_truy_cap_ngay` WRITE;
/*!40000 ALTER TABLE `luot_truy_cap_ngay` DISABLE KEYS */;
INSERT INTO `luot_truy_cap_ngay` VALUES (1,1,'2026-04-23',1),(2,2,'2026-04-23',10),(3,3,'2026-04-23',20),(11,17,'2026-04-23',4),(12,18,'2026-04-23',1),(13,19,'2026-04-23',9),(14,20,'2026-04-23',17),(15,1,'2026-04-22',19),(16,3,'2026-04-22',23),(26,15,'2026-04-22',8),(27,16,'2026-04-22',4),(28,19,'2026-04-22',17),(29,20,'2026-04-22',3),(30,1,'2026-04-21',17),(31,3,'2026-04-21',9),(38,14,'2026-04-21',25),(39,15,'2026-04-21',11),(40,16,'2026-04-21',15),(41,17,'2026-04-21',11),(42,18,'2026-04-21',9),(43,19,'2026-04-21',16),(44,20,'2026-04-21',10),(45,1,'2026-04-20',8),(46,2,'2026-04-20',20),(47,3,'2026-04-20',21),(54,17,'2026-04-20',9),(55,18,'2026-04-20',13),(56,19,'2026-04-20',15),(57,20,'2026-04-20',7),(65,14,'2026-04-19',18),(66,16,'2026-04-19',15),(67,20,'2026-04-19',2),(68,2,'2026-04-18',19),(69,3,'2026-04-18',10),(76,15,'2026-04-18',15),(77,17,'2026-04-18',7),(78,18,'2026-04-18',6),(79,19,'2026-04-18',3),(80,20,'2026-04-18',22),(81,1,'2026-04-17',18),(82,2,'2026-04-17',21),(83,3,'2026-04-17',19),(90,14,'2026-04-17',4),(91,15,'2026-04-17',10),(92,16,'2026-04-17',21),(93,17,'2026-04-17',20),(94,19,'2026-04-17',4),(95,1,'2026-04-16',12),(96,2,'2026-04-16',2),(97,3,'2026-04-16',11),(102,14,'2026-04-16',21),(103,16,'2026-04-16',13),(104,18,'2026-04-16',9),(105,19,'2026-04-16',14),(106,1,'2026-04-15',20),(107,2,'2026-04-15',11),(108,3,'2026-04-15',12),(115,16,'2026-04-15',22),(116,17,'2026-04-15',8),(117,19,'2026-04-15',23),(118,1,'2026-04-14',2),(119,2,'2026-04-14',16),(120,3,'2026-04-14',24),(127,16,'2026-04-14',19),(128,18,'2026-04-14',25),(129,19,'2026-04-14',9),(130,20,'2026-04-14',5),(131,1,'2026-04-13',17),(132,2,'2026-04-13',11),(140,16,'2026-04-13',10),(141,18,'2026-04-13',24),(142,19,'2026-04-13',16),(143,20,'2026-04-13',11),(144,2,'2026-04-12',20),(151,14,'2026-04-12',24),(152,16,'2026-04-12',25),(153,17,'2026-04-12',20),(154,18,'2026-04-12',22),(155,20,'2026-04-12',1),(156,3,'2026-04-11',20),(162,14,'2026-04-11',10),(163,15,'2026-04-11',22),(164,16,'2026-04-11',2),(165,18,'2026-04-11',16),(166,19,'2026-04-11',7),(167,20,'2026-04-11',22),(168,1,'2026-04-10',1),(174,15,'2026-04-10',25),(175,17,'2026-04-10',17),(176,18,'2026-04-10',20),(177,19,'2026-04-10',3),(178,20,'2026-04-10',13),(179,1,'2026-04-09',9),(180,2,'2026-04-09',14),(181,3,'2026-04-09',16),(191,14,'2026-04-09',1),(192,15,'2026-04-09',12),(193,16,'2026-04-09',11),(194,17,'2026-04-09',18),(195,18,'2026-04-09',14),(196,19,'2026-04-09',17),(197,20,'2026-04-09',23),(198,1,'2026-04-08',11),(199,2,'2026-04-08',11),(200,3,'2026-04-08',22),(208,14,'2026-04-08',10),(209,15,'2026-04-08',9),(210,17,'2026-04-08',6),(211,19,'2026-04-08',11),(212,20,'2026-04-08',22),(213,1,'2026-04-07',1),(214,2,'2026-04-07',1),(215,3,'2026-04-07',18),(221,15,'2026-04-07',5),(222,16,'2026-04-07',20),(223,17,'2026-04-07',8),(224,18,'2026-04-07',8),(225,2,'2026-04-06',1),(226,3,'2026-04-06',13),(234,14,'2026-04-06',18),(235,15,'2026-04-06',22),(236,16,'2026-04-06',6),(237,17,'2026-04-06',2),(238,18,'2026-04-06',10),(239,19,'2026-04-06',4),(240,1,'2026-04-05',19),(241,2,'2026-04-05',19),(242,3,'2026-04-05',25),(250,15,'2026-04-05',23),(251,16,'2026-04-05',24),(252,17,'2026-04-05',11),(253,18,'2026-04-05',20),(254,19,'2026-04-05',19),(255,20,'2026-04-05',24),(256,3,'2026-04-04',5),(263,15,'2026-04-04',2),(264,16,'2026-04-04',7),(265,17,'2026-04-04',20),(266,18,'2026-04-04',5),(267,19,'2026-04-04',24),(268,20,'2026-04-04',11),(269,1,'2026-04-03',20),(270,2,'2026-04-03',18),(280,14,'2026-04-03',22),(281,15,'2026-04-03',13),(282,16,'2026-04-03',25),(283,17,'2026-04-03',1),(284,20,'2026-04-03',23),(285,2,'2026-04-02',13),(286,3,'2026-04-02',11),(293,14,'2026-04-02',13),(294,17,'2026-04-02',7),(295,19,'2026-04-02',5),(296,20,'2026-04-02',23),(297,1,'2026-04-01',6),(298,2,'2026-04-01',22),(299,3,'2026-04-01',4),(307,14,'2026-04-01',5),(308,15,'2026-04-01',8),(309,16,'2026-04-01',2),(310,17,'2026-04-01',19),(311,18,'2026-04-01',9),(312,2,'2026-03-31',6),(321,14,'2026-03-31',24),(322,16,'2026-03-31',3),(323,18,'2026-03-31',24),(324,20,'2026-03-31',25),(325,1,'2026-03-30',14),(326,2,'2026-03-30',4),(327,3,'2026-03-30',3),(334,14,'2026-03-30',13),(335,16,'2026-03-30',6),(336,17,'2026-03-30',10),(337,18,'2026-03-30',20),(338,19,'2026-03-30',9),(339,2,'2026-03-29',8),(340,3,'2026-03-29',14),(348,14,'2026-03-29',24),(349,15,'2026-03-29',12),(350,17,'2026-03-29',24),(351,18,'2026-03-29',11),(352,19,'2026-03-29',2),(353,20,'2026-03-29',14),(354,1,'2026-03-28',18),(355,2,'2026-03-28',22),(356,3,'2026-03-28',15),(364,16,'2026-03-28',3),(365,17,'2026-03-28',22),(366,20,'2026-03-28',1),(367,1,'2026-03-27',9),(374,14,'2026-03-27',7),(375,15,'2026-03-27',25),(376,16,'2026-03-27',15),(377,17,'2026-03-27',10),(378,19,'2026-03-27',15),(379,1,'2026-03-26',18),(380,3,'2026-03-26',7),(385,14,'2026-03-26',17),(386,15,'2026-03-26',24),(387,16,'2026-03-26',11),(388,17,'2026-03-26',3),(389,18,'2026-03-26',11),(390,19,'2026-03-26',4),(391,20,'2026-03-26',8),(392,1,'2026-03-25',3),(401,14,'2026-03-25',8),(402,15,'2026-03-25',11),(403,16,'2026-03-25',23),(404,17,'2026-03-25',21),(405,18,'2026-03-25',21),(406,20,'2026-03-25',18),(407,1,'2026-03-24',22),(408,3,'2026-03-24',24),(414,14,'2026-03-24',9),(415,15,'2026-03-24',2),(416,17,'2026-03-24',25),(417,18,'2026-03-24',13),(418,19,'2026-03-24',2),(419,1,'2026-03-23',10),(420,2,'2026-03-23',24),(421,3,'2026-03-23',1),(427,14,'2026-03-23',22),(428,19,'2026-03-23',12),(429,20,'2026-03-23',14),(430,1,'2026-03-22',4),(431,3,'2026-03-22',21),(438,14,'2026-03-22',25),(439,16,'2026-03-22',7),(440,17,'2026-03-22',10),(441,19,'2026-03-22',10),(442,20,'2026-03-22',16),(443,1,'2026-03-21',12),(444,3,'2026-03-21',13),(451,15,'2026-03-21',19),(452,16,'2026-03-21',10),(453,17,'2026-03-21',20),(454,18,'2026-03-21',13),(455,19,'2026-03-21',8),(456,20,'2026-03-21',5),(457,1,'2026-03-20',10),(458,2,'2026-03-20',3),(467,14,'2026-03-20',2),(468,15,'2026-03-20',18),(469,17,'2026-03-20',1),(470,18,'2026-03-20',11),(471,1,'2026-03-19',17),(472,3,'2026-03-19',11),(476,15,'2026-03-19',16),(477,17,'2026-03-19',5),(478,18,'2026-03-19',6),(479,19,'2026-03-19',1),(480,20,'2026-03-19',6),(481,1,'2026-03-18',15),(482,3,'2026-03-18',1),(491,15,'2026-03-18',25),(492,16,'2026-03-18',3),(493,17,'2026-03-18',6),(494,19,'2026-03-18',10),(495,20,'2026-03-18',25),(496,1,'2026-03-17',11),(497,2,'2026-03-17',1),(498,3,'2026-03-17',22),(507,14,'2026-03-17',1),(508,17,'2026-03-17',7),(509,18,'2026-03-17',19),(510,1,'2026-03-16',18),(511,2,'2026-03-16',9),(512,3,'2026-03-16',2),(520,14,'2026-03-16',25),(521,15,'2026-03-16',6),(522,18,'2026-03-16',16),(523,19,'2026-03-16',9),(524,2,'2026-03-15',9),(525,3,'2026-03-15',12),(533,14,'2026-03-15',17),(534,16,'2026-03-15',20),(535,17,'2026-03-15',18),(536,18,'2026-03-15',1),(537,20,'2026-03-15',3),(538,1,'2026-03-14',7),(539,2,'2026-03-14',7),(546,14,'2026-03-14',5),(547,18,'2026-03-14',1),(548,19,'2026-03-14',13),(549,1,'2026-03-13',22),(550,2,'2026-03-13',10),(551,3,'2026-03-13',9),(558,14,'2026-03-13',22),(559,15,'2026-03-13',24),(560,16,'2026-03-13',9),(561,17,'2026-03-13',11),(562,19,'2026-03-13',19),(563,1,'2026-03-12',1),(564,3,'2026-03-12',13),(572,14,'2026-03-12',25),(573,15,'2026-03-12',18),(574,17,'2026-03-12',4),(575,20,'2026-03-12',16),(576,3,'2026-03-11',22),(585,14,'2026-03-11',20),(586,15,'2026-03-11',12),(587,16,'2026-03-11',1),(588,17,'2026-03-11',21),(589,18,'2026-03-11',24),(590,19,'2026-03-11',1),(591,3,'2026-03-10',17),(598,15,'2026-03-10',14),(599,16,'2026-03-10',25),(600,17,'2026-03-10',2),(601,18,'2026-03-10',17),(602,20,'2026-03-10',4),(603,1,'2026-03-09',4),(604,2,'2026-03-09',16),(605,3,'2026-03-09',5),(614,14,'2026-03-09',16),(615,15,'2026-03-09',20),(616,16,'2026-03-09',25),(617,17,'2026-03-09',14),(618,18,'2026-03-09',6),(619,1,'2026-03-08',14),(620,3,'2026-03-08',16),(628,15,'2026-03-08',20),(629,16,'2026-03-08',10),(630,17,'2026-03-08',3),(631,19,'2026-03-08',7),(632,20,'2026-03-08',24),(633,1,'2026-03-07',4),(634,2,'2026-03-07',20),(635,3,'2026-03-07',2),(644,14,'2026-03-07',7),(645,15,'2026-03-07',1),(646,16,'2026-03-07',25),(647,17,'2026-03-07',14),(648,18,'2026-03-07',23),(649,19,'2026-03-07',21),(650,20,'2026-03-07',15),(651,1,'2026-03-06',5),(652,3,'2026-03-06',5),(659,14,'2026-03-06',10),(660,15,'2026-03-06',18),(661,16,'2026-03-06',4),(662,17,'2026-03-06',17),(663,18,'2026-03-06',9),(664,19,'2026-03-06',2),(665,1,'2026-03-05',13),(666,2,'2026-03-05',11),(667,3,'2026-03-05',24),(673,14,'2026-03-05',25),(674,15,'2026-03-05',12),(675,16,'2026-03-05',14),(676,19,'2026-03-05',1),(677,20,'2026-03-05',1),(678,1,'2026-03-04',22),(684,14,'2026-03-04',18),(685,15,'2026-03-04',15),(686,17,'2026-03-04',24),(687,18,'2026-03-04',15),(688,20,'2026-03-04',13),(689,1,'2026-03-03',7),(690,3,'2026-03-03',10),(700,14,'2026-03-03',22),(701,15,'2026-03-03',6),(702,17,'2026-03-03',23),(703,18,'2026-03-03',13),(704,19,'2026-03-03',19),(705,20,'2026-03-03',16),(706,1,'2026-03-02',8),(714,14,'2026-03-02',7),(715,15,'2026-03-02',24),(716,16,'2026-03-02',13),(717,17,'2026-03-02',24),(718,19,'2026-03-02',7),(719,20,'2026-03-02',4),(720,1,'2026-03-01',19),(721,2,'2026-03-01',7),(722,3,'2026-03-01',22),(732,15,'2026-03-01',16),(733,16,'2026-03-01',1),(734,17,'2026-03-01',7),(735,18,'2026-03-01',8),(736,19,'2026-03-01',13),(737,20,'2026-03-01',8),(738,1,'2026-02-28',19),(739,3,'2026-02-28',18),(746,14,'2026-02-28',9),(747,16,'2026-02-28',7),(748,17,'2026-02-28',11),(749,18,'2026-02-28',25),(750,19,'2026-02-28',8),(751,20,'2026-02-28',10),(752,2,'2026-02-27',1),(753,3,'2026-02-27',13),(760,14,'2026-02-27',14),(761,16,'2026-02-27',25),(762,17,'2026-02-27',5),(763,18,'2026-02-27',20),(764,19,'2026-02-27',20),(765,1,'2026-02-26',5),(766,2,'2026-02-26',23),(767,3,'2026-02-26',5),(775,14,'2026-02-26',5),(776,16,'2026-02-26',5),(777,18,'2026-02-26',6),(778,19,'2026-02-26',11),(779,20,'2026-02-26',2),(780,1,'2026-02-25',13),(781,3,'2026-02-25',5),(788,15,'2026-02-25',20),(789,16,'2026-02-25',9),(790,18,'2026-02-25',20),(791,19,'2026-02-25',20),(792,20,'2026-02-25',6),(793,1,'2026-02-24',9),(794,2,'2026-02-24',16),(795,3,'2026-02-24',11),(802,14,'2026-02-24',4),(803,15,'2026-02-24',15),(804,18,'2026-02-24',9),(805,19,'2026-02-24',16),(806,20,'2026-02-24',13),(807,1,'2026-02-23',6),(808,2,'2026-02-23',2),(809,3,'2026-02-23',3),(817,14,'2026-02-23',20),(818,16,'2026-02-23',8),(819,19,'2026-02-23',19),(820,20,'2026-02-23',4),(821,1,'2026-02-22',6),(822,2,'2026-02-22',14),(823,3,'2026-02-22',13),(832,14,'2026-02-22',15),(833,15,'2026-02-22',23),(834,18,'2026-02-22',21),(835,19,'2026-02-22',6),(836,20,'2026-02-22',10),(837,1,'2026-02-21',23),(838,2,'2026-02-21',8),(847,14,'2026-02-21',19),(848,16,'2026-02-21',22),(849,17,'2026-02-21',18),(850,19,'2026-02-21',8),(851,20,'2026-02-21',15),(852,1,'2026-02-20',16),(853,2,'2026-02-20',23),(864,14,'2026-02-20',22),(865,15,'2026-02-20',5),(866,16,'2026-02-20',3),(867,17,'2026-02-20',6),(868,18,'2026-02-20',7),(869,20,'2026-02-20',12),(870,3,'2026-02-19',17),(878,17,'2026-02-19',7),(879,18,'2026-02-19',19),(880,19,'2026-02-19',3),(881,20,'2026-02-19',6),(882,1,'2026-02-18',14),(883,2,'2026-02-18',8),(891,14,'2026-02-18',24),(892,18,'2026-02-18',11),(893,19,'2026-02-18',6),(894,20,'2026-02-18',2),(895,1,'2026-02-17',21),(906,14,'2026-02-17',2),(907,16,'2026-02-17',2),(908,18,'2026-02-17',10),(909,19,'2026-02-17',7),(910,20,'2026-02-17',14),(914,15,'2026-02-16',1),(915,16,'2026-02-16',22),(916,17,'2026-02-16',3),(917,18,'2026-02-16',19),(918,19,'2026-02-16',6),(919,20,'2026-02-16',22),(920,1,'2026-02-15',8),(921,2,'2026-02-15',13),(922,3,'2026-02-15',24),(932,14,'2026-02-15',15),(933,16,'2026-02-15',22),(934,17,'2026-02-15',20),(935,19,'2026-02-15',1),(936,20,'2026-02-15',7),(937,1,'2026-02-14',3),(938,2,'2026-02-14',18),(939,3,'2026-02-14',2),(947,14,'2026-02-14',19),(948,15,'2026-02-14',21),(949,16,'2026-02-14',25),(950,18,'2026-02-14',18),(951,19,'2026-02-14',24),(952,2,'2026-02-13',12),(960,14,'2026-02-13',4),(961,15,'2026-02-13',14),(962,16,'2026-02-13',24),(963,17,'2026-02-13',1),(964,18,'2026-02-13',3),(965,20,'2026-02-13',25),(966,1,'2026-02-12',6),(967,3,'2026-02-12',5),(973,15,'2026-02-12',10),(974,16,'2026-02-12',9),(975,18,'2026-02-12',11),(976,20,'2026-02-12',1),(977,1,'2026-02-11',11),(978,2,'2026-02-11',15),(979,3,'2026-02-11',13),(990,15,'2026-02-11',2),(991,16,'2026-02-11',10),(992,18,'2026-02-11',7),(993,19,'2026-02-11',16),(994,20,'2026-02-11',6),(995,2,'2026-02-10',18),(996,3,'2026-02-10',9),(1005,14,'2026-02-10',16),(1006,15,'2026-02-10',12),(1007,16,'2026-02-10',12),(1008,17,'2026-02-10',1),(1009,18,'2026-02-10',18),(1010,20,'2026-02-10',5),(1011,1,'2026-02-09',19),(1012,3,'2026-02-09',25),(1020,14,'2026-02-09',22),(1021,15,'2026-02-09',25),(1022,16,'2026-02-09',7),(1023,17,'2026-02-09',12),(1024,18,'2026-02-09',21),(1025,19,'2026-02-09',11),(1026,1,'2026-02-08',16),(1027,2,'2026-02-08',7),(1028,3,'2026-02-08',8),(1037,16,'2026-02-08',24),(1038,17,'2026-02-08',21),(1039,18,'2026-02-08',17),(1040,19,'2026-02-08',14),(1041,20,'2026-02-08',1),(1042,3,'2026-02-07',16),(1049,14,'2026-02-07',5),(1050,16,'2026-02-07',7),(1051,17,'2026-02-07',1),(1052,18,'2026-02-07',8),(1053,19,'2026-02-07',5),(1054,20,'2026-02-07',19),(1055,1,'2026-02-06',17),(1056,2,'2026-02-06',25),(1057,3,'2026-02-06',8),(1065,14,'2026-02-06',23),(1066,17,'2026-02-06',23),(1067,18,'2026-02-06',2),(1068,19,'2026-02-06',25),(1069,20,'2026-02-06',11),(1070,1,'2026-02-05',7),(1071,2,'2026-02-05',21),(1072,3,'2026-02-05',4),(1079,15,'2026-02-05',13),(1080,16,'2026-02-05',3),(1081,19,'2026-02-05',9),(1082,20,'2026-02-05',17),(1083,1,'2026-02-04',19),(1084,2,'2026-02-04',15),(1090,15,'2026-02-04',13),(1091,16,'2026-02-04',13),(1092,17,'2026-02-04',9),(1093,20,'2026-02-04',17),(1094,1,'2026-02-03',16),(1095,2,'2026-02-03',24),(1096,3,'2026-02-03',10),(1106,15,'2026-02-03',3),(1107,17,'2026-02-03',14),(1108,18,'2026-02-03',19),(1109,19,'2026-02-03',15),(1110,20,'2026-02-03',3),(1111,3,'2026-02-02',3),(1119,14,'2026-02-02',5),(1120,15,'2026-02-02',23),(1121,16,'2026-02-02',18),(1122,17,'2026-02-02',13),(1123,18,'2026-02-02',25),(1124,19,'2026-02-02',21),(1125,2,'2026-02-01',19),(1126,3,'2026-02-01',23),(1133,14,'2026-02-01',5),(1134,15,'2026-02-01',7),(1135,16,'2026-02-01',5),(1136,17,'2026-02-01',12),(1137,18,'2026-02-01',23),(1138,19,'2026-02-01',15),(1139,20,'2026-02-01',3),(1140,1,'2026-01-31',23),(1141,3,'2026-01-31',21),(1148,14,'2026-01-31',12),(1149,16,'2026-01-31',21),(1150,17,'2026-01-31',1),(1151,18,'2026-01-31',7),(1152,19,'2026-01-31',6),(1153,1,'2026-01-30',25),(1154,2,'2026-01-30',20),(1159,14,'2026-01-30',12),(1160,16,'2026-01-30',7),(1161,17,'2026-01-30',24),(1162,20,'2026-01-30',18),(1163,1,'2026-01-29',13),(1170,14,'2026-01-29',15),(1171,15,'2026-01-29',6),(1172,16,'2026-01-29',10),(1173,18,'2026-01-29',23),(1174,19,'2026-01-29',10),(1175,20,'2026-01-29',25),(1176,2,'2026-01-28',20),(1177,3,'2026-01-28',9),(1182,14,'2026-01-28',18),(1183,16,'2026-01-28',9),(1184,17,'2026-01-28',16),(1185,19,'2026-01-28',1),(1186,20,'2026-01-28',14),(1187,1,'2026-01-27',2),(1188,3,'2026-01-27',6),(1195,14,'2026-01-27',19),(1196,15,'2026-01-27',4),(1197,16,'2026-01-27',19),(1198,17,'2026-01-27',16),(1199,18,'2026-01-27',3),(1200,20,'2026-01-27',23),(1201,1,'2026-01-26',12),(1202,2,'2026-01-26',12),(1212,16,'2026-01-26',16),(1213,17,'2026-01-26',5),(1214,18,'2026-01-26',18),(1215,19,'2026-01-26',22),(1216,20,'2026-01-26',24),(1217,1,'2026-01-25',14),(1218,2,'2026-01-25',19),(1219,3,'2026-01-25',22),(1227,15,'2026-01-25',22),(1228,19,'2026-01-25',8),(1229,20,'2026-01-25',24),(1230,2,'2026-01-24',8),(1231,3,'2026-01-24',15),(1238,14,'2026-01-24',9),(1239,17,'2026-01-24',5),(1240,18,'2026-01-24',25),(1241,1,'2026-01-23',5),(1242,2,'2026-01-23',2),(1243,3,'2026-01-23',23),(1253,14,'2026-01-23',5),(1254,15,'2026-01-23',24),(1255,16,'2026-01-23',22),(1256,17,'2026-01-23',8),(1257,18,'2026-01-23',10),(1258,19,'2026-01-23',16),(1259,1,'2026-01-22',5),(1260,2,'2026-01-22',16),(1268,15,'2026-01-22',23),(1269,16,'2026-01-22',19),(1270,18,'2026-01-22',19),(1271,19,'2026-01-22',12),(1272,2,'2026-01-21',18),(1273,3,'2026-01-21',17),(1278,14,'2026-01-21',23),(1279,17,'2026-01-21',2),(1280,19,'2026-01-21',5),(1281,3,'2026-01-20',5),(1288,14,'2026-01-20',8),(1289,16,'2026-01-20',25),(1290,17,'2026-01-20',21),(1291,18,'2026-01-20',22),(1292,19,'2026-01-20',24),(1293,1,'2026-01-19',3),(1294,2,'2026-01-19',1),(1295,3,'2026-01-19',13),(1305,16,'2026-01-19',15),(1306,17,'2026-01-19',21),(1307,18,'2026-01-19',8),(1308,1,'2026-01-18',25),(1309,3,'2026-01-18',1),(1317,14,'2026-01-18',15),(1318,16,'2026-01-18',1),(1319,18,'2026-01-18',23),(1320,19,'2026-01-18',5),(1321,20,'2026-01-18',10),(1322,1,'2026-01-17',9),(1323,2,'2026-01-17',1),(1324,3,'2026-01-17',17),(1332,14,'2026-01-17',20),(1333,15,'2026-01-17',11),(1334,16,'2026-01-17',14),(1335,17,'2026-01-17',18),(1336,18,'2026-01-17',8),(1337,20,'2026-01-17',2),(1338,1,'2026-01-16',2),(1339,2,'2026-01-16',3),(1340,3,'2026-01-16',24),(1348,14,'2026-01-16',8),(1349,15,'2026-01-16',21),(1350,16,'2026-01-16',8),(1351,17,'2026-01-16',1),(1352,19,'2026-01-16',22),(1353,20,'2026-01-16',13),(1354,2,'2026-01-15',5),(1362,16,'2026-01-15',19),(1363,17,'2026-01-15',12),(1364,18,'2026-01-15',21),(1365,19,'2026-01-15',21),(1366,20,'2026-01-15',10),(1367,1,'2026-01-14',25),(1368,2,'2026-01-14',12),(1369,3,'2026-01-14',7),(1379,15,'2026-01-14',20),(1380,16,'2026-01-14',14),(1381,17,'2026-01-14',6),(1382,19,'2026-01-14',4),(1383,20,'2026-01-14',22),(1384,2,'2026-01-13',20),(1385,3,'2026-01-13',4),(1396,14,'2026-01-13',12),(1397,15,'2026-01-13',10),(1398,16,'2026-01-13',10),(1399,18,'2026-01-13',20),(1400,19,'2026-01-13',12),(1401,1,'2026-01-12',11),(1402,2,'2026-01-12',12),(1409,15,'2026-01-12',25),(1410,17,'2026-01-12',18),(1411,18,'2026-01-12',3),(1412,19,'2026-01-12',1),(1413,20,'2026-01-12',8),(1414,1,'2026-01-11',8),(1421,14,'2026-01-11',22),(1422,16,'2026-01-11',10),(1423,20,'2026-01-11',12),(1424,2,'2026-01-10',19),(1433,14,'2026-01-10',23),(1434,16,'2026-01-10',4),(1435,18,'2026-01-10',22),(1436,19,'2026-01-10',9),(1437,20,'2026-01-10',3),(1438,1,'2026-01-09',11),(1439,2,'2026-01-09',14),(1440,3,'2026-01-09',14),(1450,14,'2026-01-09',14),(1451,15,'2026-01-09',22),(1452,16,'2026-01-09',14),(1453,17,'2026-01-09',3),(1454,18,'2026-01-09',7),(1455,20,'2026-01-09',9),(1456,1,'2026-01-08',24),(1457,2,'2026-01-08',5),(1458,3,'2026-01-08',25),(1467,14,'2026-01-08',19),(1468,16,'2026-01-08',25),(1469,18,'2026-01-08',20),(1470,19,'2026-01-08',15),(1471,20,'2026-01-08',3),(1472,3,'2026-01-07',11),(1481,14,'2026-01-07',23),(1482,18,'2026-01-07',21),(1483,19,'2026-01-07',2),(1484,20,'2026-01-07',3),(1485,1,'2026-01-06',2),(1486,2,'2026-01-06',25),(1495,15,'2026-01-06',15),(1496,16,'2026-01-06',21),(1497,17,'2026-01-06',17),(1498,18,'2026-01-06',20),(1499,19,'2026-01-06',11),(1500,20,'2026-01-06',9),(1501,1,'2026-01-05',25),(1502,2,'2026-01-05',20),(1510,14,'2026-01-05',20),(1511,17,'2026-01-05',3),(1512,18,'2026-01-05',2),(1513,20,'2026-01-05',21),(1514,1,'2026-01-04',4),(1515,2,'2026-01-04',7),(1516,3,'2026-01-04',7),(1524,14,'2026-01-04',1),(1525,15,'2026-01-04',5),(1526,16,'2026-01-04',12),(1527,17,'2026-01-04',16),(1528,18,'2026-01-04',14),(1529,20,'2026-01-04',10),(1530,1,'2026-01-03',19),(1536,14,'2026-01-03',2),(1537,15,'2026-01-03',10),(1538,17,'2026-01-03',2),(1539,18,'2026-01-03',2),(1540,19,'2026-01-03',15),(1541,20,'2026-01-03',20),(1542,1,'2026-01-02',17),(1543,2,'2026-01-02',12),(1553,15,'2026-01-02',21),(1554,16,'2026-01-02',14),(1555,17,'2026-01-02',2),(1556,19,'2026-01-02',16),(1557,20,'2026-01-02',7),(1558,1,'2026-01-01',25),(1559,2,'2026-01-01',9),(1560,3,'2026-01-01',11),(1570,14,'2026-01-01',19),(1571,16,'2026-01-01',10),(1572,17,'2026-01-01',11),(1573,18,'2026-01-01',21),(1574,19,'2026-01-01',8),(1575,20,'2026-01-01',22),(1576,1,'2025-12-31',8),(1577,2,'2025-12-31',14),(1584,15,'2025-12-31',13),(1585,16,'2025-12-31',6),(1586,17,'2025-12-31',21),(1587,19,'2025-12-31',21),(1588,20,'2025-12-31',10),(1589,1,'2025-12-30',1),(1590,2,'2025-12-30',11),(1591,3,'2025-12-30',5),(1598,14,'2025-12-30',25),(1599,17,'2025-12-30',24),(1600,19,'2025-12-30',6),(1601,20,'2025-12-30',23),(1602,2,'2025-12-29',5),(1603,3,'2025-12-29',23),(1611,14,'2025-12-29',25),(1612,15,'2025-12-29',23),(1613,16,'2025-12-29',6),(1614,17,'2025-12-29',17),(1615,19,'2025-12-29',21),(1616,20,'2025-12-29',1),(1617,3,'2025-12-28',18),(1626,14,'2025-12-28',22),(1627,15,'2025-12-28',17),(1628,16,'2025-12-28',13),(1629,17,'2025-12-28',20),(1630,18,'2025-12-28',3),(1631,19,'2025-12-28',13),(1632,1,'2025-12-27',8),(1633,2,'2025-12-27',8),(1634,3,'2025-12-27',13),(1643,17,'2025-12-27',17),(1644,19,'2025-12-27',1),(1645,3,'2025-12-26',17),(1654,14,'2025-12-26',4),(1655,15,'2025-12-26',8),(1656,16,'2025-12-26',6),(1657,18,'2025-12-26',24),(1658,19,'2025-12-26',25),(1659,20,'2025-12-26',4),(1660,1,'2025-12-25',18),(1661,2,'2025-12-25',3),(1662,3,'2025-12-25',12),(1668,15,'2025-12-25',8),(1669,19,'2025-12-25',10),(1670,1,'2025-12-24',24),(1671,2,'2025-12-24',3),(1680,14,'2025-12-24',23),(1681,15,'2025-12-24',19),(1682,16,'2025-12-24',18),(1683,17,'2025-12-24',6),(1684,18,'2025-12-24',8),(1685,19,'2025-12-24',17),(1686,20,'2025-12-24',10),(1687,1,'2025-12-23',17),(1688,2,'2025-12-23',3),(1696,14,'2025-12-23',12),(1697,15,'2025-12-23',17),(1698,16,'2025-12-23',3),(1699,17,'2025-12-23',12),(1700,18,'2025-12-23',12),(1701,20,'2025-12-23',10),(1702,2,'2025-12-22',23),(1710,14,'2025-12-22',9),(1711,15,'2025-12-22',23),(1712,16,'2025-12-22',25),(1713,17,'2025-12-22',15),(1714,18,'2025-12-22',5),(1715,19,'2025-12-22',10),(1716,1,'2025-12-21',6),(1717,2,'2025-12-21',3),(1724,14,'2025-12-21',18),(1725,16,'2025-12-21',11),(1726,17,'2025-12-21',7),(1727,18,'2025-12-21',8),(1728,2,'2025-12-20',3),(1729,3,'2025-12-20',3),(1739,14,'2025-12-20',22),(1740,15,'2025-12-20',20),(1741,16,'2025-12-20',13),(1742,17,'2025-12-20',24),(1743,18,'2025-12-20',2),(1744,20,'2025-12-20',8),(1745,1,'2025-12-19',10),(1746,2,'2025-12-19',2),(1747,3,'2025-12-19',12),(1755,14,'2025-12-19',5),(1756,15,'2025-12-19',15),(1757,16,'2025-12-19',15),(1758,18,'2025-12-19',12),(1759,19,'2025-12-19',17),(1760,20,'2025-12-19',11),(1761,1,'2025-12-18',23),(1762,2,'2025-12-18',17),(1763,3,'2025-12-18',4),(1770,14,'2025-12-18',20),(1771,15,'2025-12-18',14),(1772,16,'2025-12-18',11),(1773,17,'2025-12-18',13),(1774,18,'2025-12-18',7),(1775,19,'2025-12-18',11),(1776,20,'2025-12-18',19),(1777,1,'2025-12-17',4),(1778,2,'2025-12-17',24),(1779,3,'2025-12-17',19),(1785,14,'2025-12-17',10),(1786,15,'2025-12-17',22),(1787,16,'2025-12-17',18),(1788,18,'2025-12-17',7),(1789,1,'2025-12-16',17),(1790,3,'2025-12-16',23),(1799,15,'2025-12-16',5),(1800,16,'2025-12-16',16),(1801,17,'2025-12-16',9),(1802,18,'2025-12-16',18),(1803,19,'2025-12-16',18),(1804,1,'2025-12-15',23),(1805,2,'2025-12-15',17),(1806,3,'2025-12-15',20),(1814,15,'2025-12-15',15),(1815,16,'2025-12-15',2),(1816,18,'2025-12-15',23),(1817,19,'2025-12-15',4),(1818,20,'2025-12-15',23),(1824,16,'2025-12-14',14),(1825,17,'2025-12-14',11),(1826,20,'2025-12-14',7),(1827,1,'2025-12-13',10),(1828,2,'2025-12-13',20),(1829,3,'2025-12-13',17),(1837,15,'2025-12-13',20),(1838,16,'2025-12-13',19),(1839,17,'2025-12-13',19),(1840,18,'2025-12-13',15),(1841,20,'2025-12-13',24),(1842,2,'2025-12-12',12),(1843,3,'2025-12-12',11),(1849,14,'2025-12-12',10),(1850,15,'2025-12-12',17),(1851,16,'2025-12-12',4),(1852,17,'2025-12-12',12),(1853,18,'2025-12-12',8),(1854,1,'2025-12-11',8),(1855,2,'2025-12-11',10),(1856,3,'2025-12-11',9),(1862,14,'2025-12-11',12),(1863,15,'2025-12-11',15),(1864,16,'2025-12-11',24),(1865,17,'2025-12-11',22),(1866,19,'2025-12-11',19),(1867,20,'2025-12-11',6),(1868,2,'2025-12-10',20),(1869,3,'2025-12-10',19),(1876,14,'2025-12-10',23),(1877,15,'2025-12-10',24),(1878,16,'2025-12-10',17),(1879,18,'2025-12-10',6),(1880,19,'2025-12-10',13),(1881,20,'2025-12-10',7),(1882,1,'2025-12-09',4),(1883,3,'2025-12-09',16),(1891,14,'2025-12-09',2),(1892,20,'2025-12-09',9),(1893,1,'2025-12-08',23),(1894,3,'2025-12-08',7),(1901,17,'2025-12-08',22),(1902,18,'2025-12-08',18),(1903,19,'2025-12-08',12),(1904,20,'2025-12-08',9),(1905,1,'2025-12-07',23),(1913,14,'2025-12-07',19),(1914,15,'2025-12-07',17),(1915,16,'2025-12-07',3),(1916,17,'2025-12-07',1),(1917,18,'2025-12-07',9),(1918,20,'2025-12-07',25),(1919,1,'2025-12-06',20),(1920,2,'2025-12-06',23),(1921,3,'2025-12-06',15),(1930,14,'2025-12-06',3),(1931,15,'2025-12-06',2),(1932,17,'2025-12-06',12),(1933,19,'2025-12-06',6),(1934,20,'2025-12-06',20),(1935,3,'2025-12-05',21),(1944,14,'2025-12-05',24),(1945,15,'2025-12-05',23),(1946,16,'2025-12-05',7),(1947,17,'2025-12-05',2),(1948,18,'2025-12-05',8),(1949,19,'2025-12-05',19),(1950,20,'2025-12-05',16),(1951,1,'2025-12-04',8),(1952,2,'2025-12-04',6),(1959,14,'2025-12-04',4),(1960,16,'2025-12-04',12),(1961,17,'2025-12-04',8),(1962,18,'2025-12-04',9),(1963,2,'2025-12-03',5),(1964,3,'2025-12-03',21),(1971,15,'2025-12-03',15),(1972,16,'2025-12-03',14),(1973,17,'2025-12-03',20),(1974,20,'2025-12-03',13),(1975,1,'2025-12-02',18),(1976,2,'2025-12-02',13),(1977,3,'2025-12-02',12),(1982,14,'2025-12-02',13),(1983,16,'2025-12-02',16),(1984,18,'2025-12-02',21),(1985,19,'2025-12-02',19),(1986,20,'2025-12-02',10),(1987,1,'2025-12-01',12),(1988,3,'2025-12-01',11),(1993,16,'2025-12-01',25),(1994,17,'2025-12-01',2),(1995,18,'2025-12-01',14),(1996,19,'2025-12-01',24),(1997,20,'2025-12-01',16),(1998,1,'2025-11-30',22),(1999,2,'2025-11-30',22),(2007,15,'2025-11-30',15),(2008,17,'2025-11-30',10),(2009,20,'2025-11-30',8),(2010,2,'2025-11-29',24),(2011,3,'2025-11-29',16),(2019,15,'2025-11-29',21),(2020,16,'2025-11-29',3),(2021,20,'2025-11-29',8),(2022,1,'2025-11-28',12),(2023,3,'2025-11-28',4),(2032,14,'2025-11-28',19),(2033,15,'2025-11-28',19),(2034,16,'2025-11-28',14),(2035,17,'2025-11-28',4),(2036,20,'2025-11-28',23),(2037,1,'2025-11-27',7),(2046,14,'2025-11-27',24),(2047,18,'2025-11-27',4),(2048,19,'2025-11-27',17),(2049,20,'2025-11-27',17),(2050,1,'2025-11-26',5),(2051,2,'2025-11-26',5),(2059,16,'2025-11-26',23),(2060,17,'2025-11-26',25),(2061,19,'2025-11-26',20),(2062,20,'2025-11-26',15),(2063,1,'2025-11-25',22),(2064,2,'2025-11-25',12),(2071,14,'2025-11-25',18),(2072,15,'2025-11-25',16),(2073,17,'2025-11-25',15),(2074,18,'2025-11-25',6),(2075,19,'2025-11-25',8),(2076,20,'2025-11-25',19),(2077,1,'2025-11-24',20),(2078,2,'2025-11-24',12),(2079,3,'2025-11-24',10),(2086,14,'2025-11-24',20),(2087,17,'2025-11-24',19),(2088,18,'2025-11-24',24),(2089,20,'2025-11-24',11),(2090,3,'2025-11-23',2),(2097,14,'2025-11-23',8),(2098,17,'2025-11-23',13),(2099,18,'2025-11-23',1),(2100,19,'2025-11-23',25),(2101,20,'2025-11-23',9),(2102,1,'2025-11-22',21),(2103,2,'2025-11-22',4),(2104,3,'2025-11-22',19),(2110,14,'2025-11-22',5),(2111,20,'2025-11-22',22),(2112,1,'2025-11-21',4),(2113,2,'2025-11-21',22),(2121,14,'2025-11-21',18),(2122,16,'2025-11-21',21),(2123,18,'2025-11-21',4),(2124,19,'2025-11-21',6),(2125,20,'2025-11-21',25),(2126,2,'2025-11-20',6),(2127,3,'2025-11-20',17),(2137,15,'2025-11-20',18),(2138,17,'2025-11-20',22),(2139,18,'2025-11-20',25),(2140,19,'2025-11-20',17),(2141,20,'2025-11-20',21),(2142,1,'2025-11-19',17),(2143,3,'2025-11-19',1),(2153,14,'2025-11-19',12),(2154,15,'2025-11-19',5),(2155,16,'2025-11-19',25),(2156,17,'2025-11-19',8),(2157,18,'2025-11-19',16),(2158,19,'2025-11-19',2),(2159,20,'2025-11-19',20),(2160,1,'2025-11-18',10),(2168,14,'2025-11-18',24),(2169,15,'2025-11-18',11),(2170,16,'2025-11-18',5),(2171,17,'2025-11-18',16),(2172,18,'2025-11-18',14),(2173,19,'2025-11-18',24),(2174,1,'2025-11-17',2),(2175,2,'2025-11-17',19),(2183,14,'2025-11-17',22),(2184,16,'2025-11-17',8),(2185,17,'2025-11-17',8),(2186,18,'2025-11-17',18),(2187,20,'2025-11-17',10),(2188,1,'2025-11-16',14),(2189,3,'2025-11-16',5),(2197,14,'2025-11-16',7),(2198,15,'2025-11-16',3),(2199,16,'2025-11-16',25),(2200,17,'2025-11-16',25),(2201,18,'2025-11-16',25),(2202,19,'2025-11-16',9),(2203,20,'2025-11-16',12),(2204,1,'2025-11-15',6),(2205,3,'2025-11-15',3),(2214,14,'2025-11-15',17),(2215,15,'2025-11-15',24),(2216,16,'2025-11-15',1),(2217,17,'2025-11-15',5),(2218,20,'2025-11-15',10),(2219,2,'2025-11-14',21),(2220,3,'2025-11-14',23),(2230,15,'2025-11-14',9),(2231,17,'2025-11-14',6),(2232,18,'2025-11-14',16),(2233,19,'2025-11-14',5),(2234,2,'2025-11-13',21),(2235,3,'2025-11-13',21),(2243,14,'2025-11-13',15),(2244,16,'2025-11-13',23),(2245,17,'2025-11-13',25),(2246,19,'2025-11-13',23),(2247,20,'2025-11-13',17),(2248,1,'2025-11-12',19),(2249,2,'2025-11-12',23),(2250,3,'2025-11-12',25),(2259,16,'2025-11-12',22),(2260,17,'2025-11-12',25),(2261,18,'2025-11-12',6),(2262,20,'2025-11-12',21),(2263,2,'2025-11-11',17),(2264,3,'2025-11-11',10),(2273,14,'2025-11-11',11),(2274,16,'2025-11-11',19),(2275,18,'2025-11-11',11),(2276,1,'2025-11-10',18),(2283,14,'2025-11-10',8),(2284,16,'2025-11-10',2),(2285,17,'2025-11-10',18),(2286,18,'2025-11-10',17),(2287,19,'2025-11-10',23),(2288,2,'2025-11-09',25),(2296,14,'2025-11-09',8),(2297,15,'2025-11-09',17),(2298,17,'2025-11-09',20),(2299,18,'2025-11-09',24),(2300,20,'2025-11-09',16),(2301,1,'2025-11-08',17),(2302,2,'2025-11-08',16),(2308,16,'2025-11-08',4),(2309,17,'2025-11-08',8),(2310,19,'2025-11-08',18),(2311,20,'2025-11-08',16),(2312,1,'2025-11-07',24),(2313,2,'2025-11-07',9),(2314,3,'2025-11-07',23),(2323,14,'2025-11-07',10),(2324,15,'2025-11-07',23),(2325,17,'2025-11-07',8),(2326,19,'2025-11-07',16),(2327,1,'2025-11-06',2),(2328,2,'2025-11-06',22),(2329,3,'2025-11-06',15),(2334,14,'2025-11-06',9),(2335,16,'2025-11-06',19),(2336,17,'2025-11-06',11),(2337,18,'2025-11-06',6),(2338,19,'2025-11-06',22),(2339,20,'2025-11-06',9),(2340,1,'2025-11-05',13),(2341,2,'2025-11-05',5),(2342,3,'2025-11-05',21),(2351,14,'2025-11-05',2),(2352,15,'2025-11-05',12),(2353,16,'2025-11-05',16),(2354,1,'2025-11-04',9),(2355,2,'2025-11-04',14),(2360,14,'2025-11-04',1),(2361,15,'2025-11-04',4),(2362,16,'2025-11-04',20),(2363,17,'2025-11-04',21),(2364,19,'2025-11-04',24),(2365,2,'2025-11-03',5),(2372,14,'2025-11-03',1),(2373,15,'2025-11-03',16),(2374,16,'2025-11-03',13),(2375,17,'2025-11-03',22),(2376,18,'2025-11-03',13),(2377,19,'2025-11-03',7),(2378,20,'2025-11-03',12),(2379,1,'2025-11-02',12),(2380,2,'2025-11-02',11),(2381,3,'2025-11-02',16),(2390,14,'2025-11-02',6),(2391,15,'2025-11-02',3),(2392,17,'2025-11-02',17),(2393,18,'2025-11-02',25),(2394,19,'2025-11-02',4),(2395,20,'2025-11-02',2),(2396,1,'2025-11-01',2),(2397,2,'2025-11-01',6),(2398,3,'2025-11-01',3),(2407,14,'2025-11-01',4),(2408,16,'2025-11-01',3),(2409,17,'2025-11-01',6),(2410,1,'2025-10-31',21),(2411,3,'2025-10-31',17),(2420,14,'2025-10-31',23),(2421,15,'2025-10-31',17),(2422,16,'2025-10-31',6),(2423,17,'2025-10-31',18),(2424,18,'2025-10-31',14),(2425,19,'2025-10-31',22),(2426,1,'2025-10-30',14),(2427,2,'2025-10-30',1),(2428,3,'2025-10-30',24),(2437,14,'2025-10-30',11),(2438,15,'2025-10-30',15),(2439,16,'2025-10-30',20),(2440,17,'2025-10-30',14),(2441,20,'2025-10-30',14),(2442,1,'2025-10-29',12),(2448,14,'2025-10-29',8),(2449,16,'2025-10-29',7),(2450,17,'2025-10-29',22),(2451,18,'2025-10-29',3),(2452,19,'2025-10-29',25),(2453,20,'2025-10-29',16),(2454,1,'2025-10-28',25),(2455,2,'2025-10-28',10),(2463,14,'2025-10-28',24),(2464,18,'2025-10-28',8),(2465,19,'2025-10-28',8),(2466,20,'2025-10-28',11),(2467,1,'2025-10-27',3),(2468,2,'2025-10-27',22),(2476,14,'2025-10-27',22),(2477,15,'2025-10-27',16),(2478,16,'2025-10-27',19),(2479,18,'2025-10-27',18),(2480,20,'2025-10-27',24),(2481,2,'2025-10-26',25),(2482,3,'2025-10-26',12),(2490,14,'2025-10-26',15),(2491,15,'2025-10-26',23),(2492,16,'2025-10-26',14),(2493,17,'2025-10-26',19),(2494,18,'2025-10-26',4),(2495,19,'2025-10-26',25),(2496,1,'2025-10-25',14),(2497,2,'2025-10-25',1),(2498,3,'2025-10-25',20),(2506,14,'2025-10-25',25),(2507,15,'2025-10-25',24),(2508,16,'2025-10-25',8),(2509,17,'2025-10-25',8),(2510,18,'2025-10-25',3),(2511,20,'2025-10-25',22),(2512,1,'2025-10-24',18),(2513,3,'2025-10-24',15),(2521,15,'2025-10-24',16),(2522,16,'2025-10-24',22),(2523,19,'2025-10-24',25),(2524,20,'2025-10-24',5),(2525,1,'2025-10-23',5),(2526,2,'2025-10-23',5),(2527,3,'2025-10-23',18),(2531,14,'2025-10-23',11),(2532,15,'2025-10-23',7),(2533,18,'2025-10-23',13),(2534,19,'2025-10-23',15),(2535,20,'2025-10-23',21),(2536,1,'2025-10-22',13),(2537,2,'2025-10-22',22),(2538,3,'2025-10-22',8),(2543,14,'2025-10-22',19),(2544,15,'2025-10-22',14),(2545,16,'2025-10-22',23),(2546,18,'2025-10-22',1),(2547,20,'2025-10-22',23),(2548,1,'2025-10-21',23),(2549,2,'2025-10-21',3),(2558,14,'2025-10-21',19),(2559,16,'2025-10-21',22),(2560,17,'2025-10-21',22),(2561,18,'2025-10-21',16),(2562,19,'2025-10-21',25),(2563,1,'2025-10-20',7),(2564,2,'2025-10-20',16),(2565,3,'2025-10-20',24),(2572,14,'2025-10-20',5),(2573,15,'2025-10-20',16),(2574,16,'2025-10-20',20),(2575,18,'2025-10-20',21),(2576,19,'2025-10-20',11),(2577,20,'2025-10-20',17),(2578,1,'2025-10-19',24),(2579,2,'2025-10-19',4),(2580,3,'2025-10-19',17),(2588,14,'2025-10-19',3),(2589,15,'2025-10-19',16),(2590,16,'2025-10-19',8),(2591,17,'2025-10-19',10),(2592,19,'2025-10-19',24),(2593,20,'2025-10-19',14),(2594,1,'2025-10-18',9),(2595,2,'2025-10-18',9),(2596,3,'2025-10-18',19),(2601,15,'2025-10-18',7),(2602,16,'2025-10-18',14),(2603,17,'2025-10-18',20),(2604,18,'2025-10-18',18),(2605,20,'2025-10-18',10),(2606,1,'2025-10-17',15),(2607,2,'2025-10-17',25),(2618,14,'2025-10-17',5),(2619,15,'2025-10-17',8),(2620,17,'2025-10-17',3),(2621,19,'2025-10-17',5),(2622,1,'2025-10-16',24),(2623,2,'2025-10-16',18),(2624,3,'2025-10-16',15),(2631,14,'2025-10-16',9),(2632,15,'2025-10-16',24),(2633,16,'2025-10-16',10),(2634,17,'2025-10-16',19),(2635,18,'2025-10-16',9),(2636,19,'2025-10-16',14),(2637,20,'2025-10-16',6),(2638,2,'2025-10-15',17),(2639,3,'2025-10-15',17),(2645,14,'2025-10-15',16),(2646,15,'2025-10-15',18),(2647,16,'2025-10-15',13),(2648,18,'2025-10-15',2),(2649,20,'2025-10-15',18),(2650,2,'2025-10-14',15),(2651,3,'2025-10-14',22),(2661,15,'2025-10-14',5),(2662,16,'2025-10-14',14),(2663,17,'2025-10-14',3),(2664,18,'2025-10-14',3),(2665,20,'2025-10-14',14),(2666,1,'2025-10-13',11),(2667,2,'2025-10-13',4),(2668,3,'2025-10-13',17),(2677,14,'2025-10-13',7),(2678,15,'2025-10-13',19),(2679,17,'2025-10-13',7),(2680,18,'2025-10-13',8),(2681,19,'2025-10-13',15),(2682,20,'2025-10-13',24),(2683,1,'2025-10-12',11),(2684,2,'2025-10-12',8),(2692,14,'2025-10-12',8),(2693,16,'2025-10-12',9),(2694,17,'2025-10-12',18),(2695,19,'2025-10-12',10),(2696,20,'2025-10-12',10),(2697,1,'2025-10-11',13),(2698,2,'2025-10-11',2),(2699,3,'2025-10-11',23),(2707,15,'2025-10-11',10),(2708,16,'2025-10-11',14),(2709,17,'2025-10-11',7),(2710,18,'2025-10-11',4),(2711,19,'2025-10-11',2),(2712,20,'2025-10-11',22),(2713,3,'2025-10-10',19),(2722,14,'2025-10-10',16),(2723,15,'2025-10-10',4),(2724,16,'2025-10-10',22),(2725,17,'2025-10-10',9),(2726,18,'2025-10-10',14),(2727,19,'2025-10-10',1),(2728,20,'2025-10-10',17),(2729,1,'2025-10-09',20),(2730,2,'2025-10-09',7),(2738,15,'2025-10-09',9),(2739,16,'2025-10-09',16),(2740,17,'2025-10-09',16),(2741,18,'2025-10-09',19),(2742,19,'2025-10-09',16),(2743,1,'2025-10-08',16),(2744,2,'2025-10-08',4),(2745,3,'2025-10-08',25),(2754,14,'2025-10-08',15),(2755,15,'2025-10-08',20),(2756,16,'2025-10-08',2),(2757,17,'2025-10-08',7),(2758,18,'2025-10-08',25),(2759,20,'2025-10-08',12),(2760,1,'2025-10-07',20),(2761,2,'2025-10-07',23),(2768,17,'2025-10-07',4),(2769,18,'2025-10-07',11),(2770,19,'2025-10-07',16),(2771,20,'2025-10-07',22),(2772,1,'2025-10-06',19),(2773,2,'2025-10-06',1),(2774,3,'2025-10-06',21),(2783,14,'2025-10-06',16),(2784,15,'2025-10-06',3),(2785,16,'2025-10-06',10),(2786,17,'2025-10-06',15),(2787,18,'2025-10-06',10),(2788,19,'2025-10-06',20),(2789,1,'2025-10-05',1),(2790,2,'2025-10-05',24),(2791,3,'2025-10-05',14),(2798,15,'2025-10-05',2),(2799,17,'2025-10-05',19),(2800,18,'2025-10-05',25),(2801,19,'2025-10-05',13),(2802,2,'2025-10-04',19),(2803,3,'2025-10-04',4),(2809,15,'2025-10-04',12),(2810,16,'2025-10-04',25),(2811,18,'2025-10-04',18),(2812,19,'2025-10-04',18),(2813,1,'2025-10-03',2),(2814,2,'2025-10-03',23),(2823,14,'2025-10-03',20),(2824,15,'2025-10-03',12),(2825,16,'2025-10-03',13),(2826,17,'2025-10-03',21),(2827,18,'2025-10-03',12),(2828,19,'2025-10-03',25),(2829,20,'2025-10-03',17),(2830,1,'2025-10-02',13),(2838,14,'2025-10-02',20),(2839,15,'2025-10-02',3),(2840,16,'2025-10-02',19),(2841,19,'2025-10-02',10),(2842,20,'2025-10-02',22),(2843,2,'2025-10-01',8),(2844,3,'2025-10-01',10),(2849,14,'2025-10-01',9),(2850,15,'2025-10-01',4),(2851,16,'2025-10-01',5),(2852,18,'2025-10-01',24),(2853,19,'2025-10-01',25),(2854,3,'2025-09-30',3),(2861,14,'2025-09-30',20),(2862,15,'2025-09-30',15),(2863,17,'2025-09-30',15),(2864,19,'2025-09-30',25),(2865,20,'2025-09-30',9),(2866,1,'2025-09-29',17),(2867,2,'2025-09-29',23),(2868,3,'2025-09-29',10),(2876,14,'2025-09-29',10),(2877,15,'2025-09-29',13),(2878,18,'2025-09-29',25),(2879,20,'2025-09-29',24),(2880,1,'2025-09-28',14),(2881,2,'2025-09-28',24),(2889,14,'2025-09-28',5),(2890,16,'2025-09-28',18),(2891,18,'2025-09-28',5),(2892,19,'2025-09-28',18),(2893,20,'2025-09-28',14),(2894,2,'2025-09-27',23),(2895,3,'2025-09-27',22),(2904,14,'2025-09-27',11),(2905,17,'2025-09-27',3),(2906,19,'2025-09-27',9),(2907,1,'2025-09-26',6),(2908,2,'2025-09-26',15),(2909,3,'2025-09-26',24),(2917,14,'2025-09-26',11),(2918,15,'2025-09-26',2),(2919,16,'2025-09-26',6),(2920,17,'2025-09-26',24),(2921,18,'2025-09-26',14),(2922,19,'2025-09-26',6),(2923,20,'2025-09-26',15),(2924,1,'2025-09-25',17),(2925,3,'2025-09-25',4),(2932,14,'2025-09-25',14),(2933,16,'2025-09-25',9),(2934,17,'2025-09-25',1),(2935,18,'2025-09-25',16),(2936,19,'2025-09-25',6),(2937,20,'2025-09-25',2),(2938,1,'2025-09-24',6),(2939,3,'2025-09-24',18),(2949,14,'2025-09-24',5),(2950,15,'2025-09-24',23),(2951,16,'2025-09-24',7),(2952,17,'2025-09-24',3),(2953,19,'2025-09-24',3),(2954,20,'2025-09-24',22),(2955,1,'2025-09-23',22),(2956,3,'2025-09-23',3),(2965,15,'2025-09-23',9),(2966,16,'2025-09-23',9),(2967,17,'2025-09-23',21),(2968,19,'2025-09-23',15),(2969,2,'2025-09-22',5),(2970,3,'2025-09-22',5),(2978,14,'2025-09-22',24),(2979,18,'2025-09-22',7),(2980,1,'2025-09-21',24),(2981,2,'2025-09-21',7),(2989,14,'2025-09-21',6),(2990,15,'2025-09-21',7),(2991,17,'2025-09-21',25),(2992,19,'2025-09-21',21),(2993,20,'2025-09-21',10),(2994,3,'2025-09-20',22),(3003,14,'2025-09-20',15),(3004,17,'2025-09-20',15),(3005,18,'2025-09-20',5),(3006,20,'2025-09-20',23),(3007,1,'2025-09-19',8),(3014,14,'2025-09-19',19),(3015,15,'2025-09-19',8),(3016,16,'2025-09-19',5),(3017,17,'2025-09-19',20),(3018,18,'2025-09-19',6),(3019,19,'2025-09-19',25),(3020,20,'2025-09-19',10),(3021,1,'2025-09-18',18),(3022,3,'2025-09-18',10),(3029,14,'2025-09-18',23),(3030,16,'2025-09-18',23),(3031,17,'2025-09-18',6),(3032,18,'2025-09-18',23),(3033,19,'2025-09-18',6),(3034,1,'2025-09-17',24),(3035,2,'2025-09-17',6),(3036,3,'2025-09-17',15),(3042,14,'2025-09-17',13),(3043,15,'2025-09-17',1),(3044,16,'2025-09-17',19),(3045,18,'2025-09-17',25),(3046,19,'2025-09-17',7),(3047,2,'2025-09-16',21),(3048,3,'2025-09-16',23),(3056,14,'2025-09-16',2),(3057,15,'2025-09-16',12),(3058,16,'2025-09-16',20),(3059,17,'2025-09-16',16),(3060,19,'2025-09-16',22),(3061,20,'2025-09-16',5),(3062,1,'2025-09-15',17),(3063,3,'2025-09-15',15),(3072,14,'2025-09-15',25),(3073,15,'2025-09-15',18),(3074,18,'2025-09-15',25),(3075,19,'2025-09-15',19),(3076,1,'2025-09-14',6),(3080,14,'2025-09-14',5),(3081,15,'2025-09-14',16),(3082,16,'2025-09-14',3),(3083,17,'2025-09-14',11),(3084,18,'2025-09-14',2),(3085,19,'2025-09-14',25),(3086,3,'2025-09-13',9),(3093,14,'2025-09-13',20),(3094,15,'2025-09-13',14),(3095,16,'2025-09-13',15),(3096,17,'2025-09-13',24),(3097,18,'2025-09-13',25),(3098,19,'2025-09-13',6),(3099,2,'2025-09-12',5),(3110,14,'2025-09-12',13),(3111,15,'2025-09-12',14),(3112,17,'2025-09-12',16),(3113,18,'2025-09-12',17),(3114,20,'2025-09-12',6),(3115,1,'2025-09-11',13),(3116,2,'2025-09-11',8),(3125,15,'2025-09-11',5),(3126,16,'2025-09-11',8),(3127,1,'2025-09-10',9),(3128,2,'2025-09-10',21),(3136,14,'2025-09-10',11),(3137,15,'2025-09-10',23),(3138,16,'2025-09-10',25),(3139,17,'2025-09-10',15),(3140,18,'2025-09-10',20),(3141,20,'2025-09-10',11),(3142,1,'2025-09-09',2),(3143,2,'2025-09-09',20),(3144,3,'2025-09-09',13),(3150,14,'2025-09-09',23),(3151,15,'2025-09-09',21),(3152,16,'2025-09-09',14),(3153,18,'2025-09-09',15),(3154,19,'2025-09-09',19),(3155,20,'2025-09-09',1),(3156,2,'2025-09-08',12),(3157,3,'2025-09-08',13),(3165,14,'2025-09-08',5),(3166,17,'2025-09-08',6),(3167,18,'2025-09-08',17),(3168,19,'2025-09-08',23),(3169,20,'2025-09-08',17),(3170,2,'2025-09-07',18),(3171,3,'2025-09-07',4),(3179,15,'2025-09-07',21),(3180,17,'2025-09-07',23),(3181,18,'2025-09-07',23),(3182,19,'2025-09-07',25),(3183,1,'2025-09-06',7),(3194,15,'2025-09-06',19),(3195,16,'2025-09-06',6),(3196,17,'2025-09-06',8),(3197,18,'2025-09-06',18),(3198,19,'2025-09-06',3),(3199,20,'2025-09-06',9),(3200,1,'2025-09-05',2),(3210,14,'2025-09-05',1),(3211,15,'2025-09-05',15),(3212,16,'2025-09-05',4),(3213,17,'2025-09-05',25),(3214,18,'2025-09-05',20),(3215,19,'2025-09-05',22),(3216,2,'2025-09-04',1),(3217,3,'2025-09-04',10),(3226,15,'2025-09-04',22),(3227,16,'2025-09-04',15),(3228,19,'2025-09-04',24),(3229,1,'2025-09-03',12),(3230,2,'2025-09-03',11),(3231,3,'2025-09-03',2),(3238,15,'2025-09-03',4),(3239,16,'2025-09-03',1),(3240,17,'2025-09-03',23),(3241,18,'2025-09-03',17),(3242,19,'2025-09-03',25),(3243,20,'2025-09-03',19),(3244,1,'2025-09-02',8),(3245,2,'2025-09-02',15),(3246,3,'2025-09-02',18),(3255,14,'2025-09-02',1),(3256,15,'2025-09-02',17),(3257,16,'2025-09-02',22),(3258,17,'2025-09-02',12),(3259,20,'2025-09-02',12),(3260,1,'2025-09-01',12),(3261,2,'2025-09-01',8),(3262,3,'2025-09-01',3),(3273,14,'2025-09-01',23),(3274,15,'2025-09-01',11),(3275,16,'2025-09-01',21),(3276,19,'2025-09-01',12),(3277,20,'2025-09-01',14),(3278,1,'2025-08-31',18),(3279,2,'2025-08-31',11),(3280,3,'2025-08-31',22),(3287,14,'2025-08-31',14),(3288,15,'2025-08-31',13),(3289,16,'2025-08-31',1),(3290,17,'2025-08-31',23),(3291,18,'2025-08-31',1),(3292,19,'2025-08-31',14),(3293,20,'2025-08-31',7),(3294,1,'2025-08-30',10),(3295,2,'2025-08-30',5),(3302,14,'2025-08-30',10),(3303,15,'2025-08-30',4),(3304,17,'2025-08-30',9),(3305,20,'2025-08-30',19),(3306,1,'2025-08-29',21),(3307,3,'2025-08-29',3),(3314,14,'2025-08-29',25),(3315,15,'2025-08-29',21),(3316,16,'2025-08-29',23),(3317,17,'2025-08-29',10),(3318,18,'2025-08-29',22),(3319,20,'2025-08-29',19),(3320,3,'2025-08-28',17),(3329,15,'2025-08-28',4),(3330,16,'2025-08-28',2),(3331,18,'2025-08-28',17),(3332,19,'2025-08-28',5),(3333,1,'2025-08-27',4),(3334,2,'2025-08-27',17),(3335,3,'2025-08-27',4),(3341,14,'2025-08-27',7),(3342,15,'2025-08-27',20),(3343,16,'2025-08-27',17),(3344,17,'2025-08-27',20),(3345,18,'2025-08-27',12),(3346,19,'2025-08-27',6),(3347,1,'2025-08-26',10),(3348,2,'2025-08-26',12),(3349,3,'2025-08-26',6),(3358,14,'2025-08-26',23),(3359,15,'2025-08-26',7),(3360,17,'2025-08-26',3),(3361,18,'2025-08-26',9),(3362,19,'2025-08-26',11),(3363,20,'2025-08-26',6),(3364,1,'2025-08-25',24),(3365,3,'2025-08-25',25),(3370,14,'2025-08-25',13),(3371,16,'2025-08-25',7),(3372,17,'2025-08-25',10),(3373,18,'2025-08-25',20),(3374,19,'2025-08-25',12),(3375,1,'2025-08-24',24),(3376,3,'2025-08-24',5),(3384,14,'2025-08-24',8),(3385,15,'2025-08-24',19),(3386,16,'2025-08-24',16),(3387,18,'2025-08-24',17),(3388,19,'2025-08-24',22),(3389,20,'2025-08-24',7),(3390,1,'2025-08-23',20),(3391,3,'2025-08-23',9),(3400,14,'2025-08-23',4),(3401,15,'2025-08-23',23),(3402,16,'2025-08-23',15),(3403,18,'2025-08-23',25),(3404,1,'2025-08-22',5),(3405,3,'2025-08-22',20),(3416,14,'2025-08-22',16),(3417,15,'2025-08-22',13),(3418,16,'2025-08-22',14),(3419,17,'2025-08-22',22),(3420,20,'2025-08-22',15),(3421,1,'2025-08-21',16),(3422,2,'2025-08-21',25),(3423,3,'2025-08-21',16),(3432,15,'2025-08-21',11),(3433,16,'2025-08-21',15),(3434,18,'2025-08-21',22),(3435,19,'2025-08-21',24),(3436,20,'2025-08-21',16),(3437,1,'2025-08-20',24),(3444,15,'2025-08-20',12),(3445,16,'2025-08-20',9),(3446,18,'2025-08-20',3),(3447,19,'2025-08-20',18),(3448,1,'2025-08-19',5),(3449,2,'2025-08-19',20),(3450,3,'2025-08-19',25),(3459,14,'2025-08-19',13),(3460,15,'2025-08-19',13),(3461,17,'2025-08-19',25),(3462,18,'2025-08-19',11),(3463,19,'2025-08-19',16),(3464,20,'2025-08-19',1),(3465,3,'2025-08-18',15),(3474,14,'2025-08-18',25),(3475,15,'2025-08-18',20),(3476,16,'2025-08-18',2),(3477,17,'2025-08-18',24),(3478,19,'2025-08-18',14),(3479,2,'2025-08-17',12),(3480,3,'2025-08-17',25),(3489,14,'2025-08-17',1),(3490,15,'2025-08-17',11),(3491,17,'2025-08-17',2),(3492,18,'2025-08-17',2),(3493,20,'2025-08-17',4),(3494,1,'2025-08-16',22),(3495,3,'2025-08-16',13),(3504,14,'2025-08-16',17),(3505,15,'2025-08-16',14),(3506,16,'2025-08-16',9),(3507,17,'2025-08-16',21),(3508,18,'2025-08-16',18),(3509,19,'2025-08-16',16),(3510,20,'2025-08-16',6),(3511,1,'2025-08-15',14),(3512,2,'2025-08-15',18),(3513,3,'2025-08-15',17),(3522,15,'2025-08-15',15),(3523,16,'2025-08-15',19),(3524,18,'2025-08-15',11),(3525,19,'2025-08-15',17),(3526,1,'2025-08-14',22),(3527,2,'2025-08-14',16),(3528,3,'2025-08-14',1),(3535,14,'2025-08-14',9),(3536,17,'2025-08-14',14),(3537,18,'2025-08-14',11),(3538,19,'2025-08-14',22),(3539,20,'2025-08-14',1),(3540,1,'2025-08-13',13),(3541,2,'2025-08-13',10),(3550,14,'2025-08-13',11),(3551,15,'2025-08-13',7),(3552,16,'2025-08-13',21),(3553,17,'2025-08-13',9),(3554,18,'2025-08-13',1),(3555,19,'2025-08-13',19),(3556,20,'2025-08-13',25),(3557,1,'2025-08-12',25),(3558,3,'2025-08-12',21),(3564,14,'2025-08-12',14),(3565,15,'2025-08-12',10),(3566,16,'2025-08-12',13),(3567,17,'2025-08-12',17),(3568,19,'2025-08-12',14),(3569,20,'2025-08-12',5),(3570,1,'2025-08-11',6),(3571,2,'2025-08-11',1),(3572,3,'2025-08-11',9),(3578,14,'2025-08-11',5),(3579,15,'2025-08-11',22),(3580,16,'2025-08-11',19),(3581,17,'2025-08-11',1),(3582,1,'2025-08-10',2),(3583,3,'2025-08-10',17),(3590,14,'2025-08-10',24),(3591,16,'2025-08-10',13),(3592,17,'2025-08-10',14),(3593,18,'2025-08-10',8),(3594,19,'2025-08-10',20),(3595,20,'2025-08-10',2),(3596,3,'2025-08-09',20),(3605,14,'2025-08-09',20),(3606,15,'2025-08-09',8),(3607,17,'2025-08-09',8),(3608,18,'2025-08-09',11),(3609,19,'2025-08-09',23),(3610,20,'2025-08-09',1),(3611,3,'2025-08-08',15),(3620,14,'2025-08-08',13),(3621,15,'2025-08-08',1),(3622,18,'2025-08-08',9),(3623,19,'2025-08-08',11),(3624,1,'2025-08-07',25),(3625,2,'2025-08-07',11),(3626,3,'2025-08-07',23),(3635,16,'2025-08-07',22),(3636,17,'2025-08-07',3),(3637,18,'2025-08-07',12),(3638,19,'2025-08-07',9),(3639,20,'2025-08-07',15),(3640,1,'2025-08-06',6),(3641,2,'2025-08-06',1),(3642,3,'2025-08-06',3),(3649,14,'2025-08-06',25),(3650,15,'2025-08-06',16),(3651,16,'2025-08-06',22),(3652,17,'2025-08-06',1),(3653,18,'2025-08-06',12),(3654,1,'2025-08-05',6),(3655,3,'2025-08-05',21),(3663,14,'2025-08-05',15),(3664,16,'2025-08-05',2),(3665,17,'2025-08-05',19),(3666,18,'2025-08-05',3),(3667,19,'2025-08-05',3),(3668,2,'2025-08-04',23),(3669,3,'2025-08-04',13),(3678,14,'2025-08-04',25),(3679,15,'2025-08-04',21),(3680,19,'2025-08-04',23),(3681,20,'2025-08-04',9),(3682,3,'2025-08-03',1),(3690,15,'2025-08-03',23),(3691,16,'2025-08-03',24),(3692,17,'2025-08-03',23),(3693,18,'2025-08-03',13),(3694,19,'2025-08-03',20),(3695,20,'2025-08-03',10),(3696,3,'2025-08-02',6),(3703,14,'2025-08-02',10),(3704,15,'2025-08-02',4),(3705,17,'2025-08-02',15),(3706,20,'2025-08-02',21),(3707,1,'2025-08-01',3),(3708,3,'2025-08-01',15),(3716,14,'2025-08-01',11),(3717,15,'2025-08-01',1),(3718,16,'2025-08-01',13),(3719,18,'2025-08-01',23),(3720,19,'2025-08-01',18),(3721,20,'2025-08-01',14),(3722,2,'2025-07-31',14),(3723,3,'2025-07-31',9),(3732,14,'2025-07-31',10),(3733,15,'2025-07-31',23),(3734,16,'2025-07-31',11),(3735,17,'2025-07-31',24),(3736,19,'2025-07-31',25),(3737,20,'2025-07-31',13),(3738,1,'2025-07-30',6),(3739,2,'2025-07-30',4),(3740,3,'2025-07-30',11),(3750,14,'2025-07-30',10),(3751,16,'2025-07-30',1),(3752,17,'2025-07-30',10),(3753,19,'2025-07-30',25),(3754,20,'2025-07-30',22),(3761,14,'2025-07-29',2),(3762,15,'2025-07-29',6),(3763,16,'2025-07-29',17),(3764,18,'2025-07-29',25),(3765,19,'2025-07-29',9),(3766,20,'2025-07-29',20),(3767,2,'2025-07-28',14),(3768,3,'2025-07-28',2),(3776,15,'2025-07-28',4),(3777,17,'2025-07-28',17),(3778,18,'2025-07-28',19),(3779,19,'2025-07-28',22),(3780,1,'2025-07-27',13),(3781,2,'2025-07-27',11),(3789,17,'2025-07-27',10),(3790,18,'2025-07-27',19),(3791,19,'2025-07-27',23),(3792,3,'2025-07-26',10),(3800,14,'2025-07-26',23),(3801,15,'2025-07-26',5),(3802,16,'2025-07-26',11),(3803,17,'2025-07-26',7),(3804,18,'2025-07-26',10),(3805,20,'2025-07-26',20),(3806,1,'2025-07-25',2),(3807,2,'2025-07-25',14),(3816,14,'2025-07-25',24),(3817,15,'2025-07-25',13),(3818,16,'2025-07-25',18),(3819,17,'2025-07-25',4),(3820,18,'2025-07-25',12),(3821,19,'2025-07-25',15),(3822,20,'2025-07-25',20),(3823,1,'2025-07-24',4),(3824,2,'2025-07-24',15),(3831,14,'2025-07-24',5),(3832,15,'2025-07-24',21),(3833,16,'2025-07-24',13),(3834,17,'2025-07-24',12),(3835,18,'2025-07-24',16),(3836,19,'2025-07-24',13),(3837,20,'2025-07-24',5),(3838,1,'2025-07-23',8),(3839,2,'2025-07-23',25),(3840,3,'2025-07-23',1),(3847,14,'2025-07-23',6),(3848,15,'2025-07-23',4),(3849,16,'2025-07-23',2),(3850,18,'2025-07-23',22),(3851,19,'2025-07-23',25),(3852,2,'2025-07-22',21),(3859,14,'2025-07-22',10),(3860,15,'2025-07-22',6),(3861,16,'2025-07-22',20),(3862,17,'2025-07-22',10),(3863,18,'2025-07-22',19),(3864,19,'2025-07-22',1),(3865,1,'2025-07-21',12),(3866,2,'2025-07-21',7),(3874,15,'2025-07-21',2),(3875,17,'2025-07-21',12),(3876,18,'2025-07-21',2),(3877,19,'2025-07-21',17),(3878,20,'2025-07-21',10),(3879,2,'2025-07-20',7),(3880,3,'2025-07-20',24),(3889,16,'2025-07-20',11),(3890,17,'2025-07-20',17),(3891,18,'2025-07-20',18),(3892,19,'2025-07-20',25),(3893,20,'2025-07-20',8),(3894,1,'2025-07-19',20),(3895,2,'2025-07-19',18),(3896,3,'2025-07-19',22),(3902,14,'2025-07-19',11),(3903,17,'2025-07-19',8),(3904,18,'2025-07-19',11),(3905,20,'2025-07-19',25),(3906,2,'2025-07-18',19),(3907,3,'2025-07-18',14),(3915,15,'2025-07-18',23),(3916,16,'2025-07-18',24),(3917,17,'2025-07-18',8),(3918,18,'2025-07-18',10),(3919,19,'2025-07-18',4),(3920,20,'2025-07-18',2),(3921,2,'2025-07-17',20),(3922,3,'2025-07-17',19),(3930,15,'2025-07-17',16),(3931,17,'2025-07-17',20),(3932,18,'2025-07-17',4),(3933,1,'2025-07-16',5),(3934,2,'2025-07-16',7),(3935,3,'2025-07-16',11),(3944,14,'2025-07-16',18),(3945,16,'2025-07-16',7),(3946,17,'2025-07-16',5),(3947,18,'2025-07-16',5),(3948,19,'2025-07-16',7),(3949,20,'2025-07-16',13),(3950,2,'2025-07-15',14),(3957,14,'2025-07-15',5),(3958,15,'2025-07-15',13),(3959,16,'2025-07-15',22),(3960,17,'2025-07-15',18),(3961,18,'2025-07-15',8),(3962,19,'2025-07-15',24),(3963,20,'2025-07-15',3),(3964,2,'2025-07-14',3),(3965,3,'2025-07-14',12),(3971,14,'2025-07-14',12),(3972,16,'2025-07-14',20),(3973,18,'2025-07-14',21),(3974,19,'2025-07-14',7),(3975,20,'2025-07-14',24),(3976,1,'2025-07-13',8),(3977,3,'2025-07-13',6),(3985,15,'2025-07-13',11),(3986,16,'2025-07-13',15),(3987,17,'2025-07-13',14),(3988,18,'2025-07-13',11),(3989,19,'2025-07-13',17),(3990,1,'2025-07-12',13),(3991,2,'2025-07-12',13),(3998,15,'2025-07-12',5),(3999,17,'2025-07-12',4),(4000,18,'2025-07-12',2),(4001,19,'2025-07-12',1),(4002,2,'2025-07-11',6),(4003,3,'2025-07-11',2),(4009,14,'2025-07-11',13),(4010,16,'2025-07-11',20),(4011,18,'2025-07-11',15),(4012,19,'2025-07-11',13),(4013,20,'2025-07-11',19),(4014,1,'2025-07-10',8),(4015,2,'2025-07-10',15),(4021,15,'2025-07-10',8),(4022,18,'2025-07-10',1),(4023,19,'2025-07-10',4),(4024,20,'2025-07-10',20),(4025,1,'2025-07-09',14),(4026,2,'2025-07-09',18),(4027,3,'2025-07-09',21),(4035,14,'2025-07-09',11),(4036,15,'2025-07-09',17),(4037,16,'2025-07-09',7),(4038,17,'2025-07-09',13),(4039,18,'2025-07-09',17),(4040,19,'2025-07-09',8),(4041,1,'2025-07-08',20),(4042,3,'2025-07-08',4),(4050,16,'2025-07-08',12),(4051,17,'2025-07-08',20),(4052,18,'2025-07-08',20),(4053,20,'2025-07-08',16),(4054,1,'2025-07-07',16),(4055,2,'2025-07-07',22),(4056,3,'2025-07-07',9),(4063,14,'2025-07-07',9),(4064,15,'2025-07-07',14),(4065,17,'2025-07-07',11),(4066,18,'2025-07-07',18),(4067,19,'2025-07-07',10),(4068,20,'2025-07-07',4),(4069,2,'2025-07-06',25),(4074,15,'2025-07-06',11),(4075,16,'2025-07-06',14),(4076,17,'2025-07-06',7),(4077,18,'2025-07-06',6),(4078,19,'2025-07-06',3),(4079,20,'2025-07-06',10),(4080,1,'2025-07-05',8),(4081,2,'2025-07-05',22),(4082,3,'2025-07-05',12),(4090,14,'2025-07-05',22),(4091,15,'2025-07-05',1),(4092,17,'2025-07-05',21),(4093,18,'2025-07-05',5),(4094,19,'2025-07-05',12),(4095,20,'2025-07-05',6),(4096,1,'2025-07-04',16),(4097,3,'2025-07-04',10),(4103,15,'2025-07-04',22),(4104,16,'2025-07-04',25),(4105,19,'2025-07-04',20),(4106,20,'2025-07-04',6),(4107,1,'2025-07-03',2),(4108,3,'2025-07-03',3),(4117,14,'2025-07-03',3),(4118,15,'2025-07-03',13),(4119,17,'2025-07-03',24),(4120,20,'2025-07-03',12),(4121,1,'2025-07-02',14),(4122,2,'2025-07-02',21),(4129,14,'2025-07-02',5),(4130,15,'2025-07-02',22),(4131,18,'2025-07-02',19),(4132,20,'2025-07-02',19),(4133,1,'2025-07-01',1),(4134,2,'2025-07-01',8),(4135,3,'2025-07-01',1),(4142,16,'2025-07-01',20),(4143,17,'2025-07-01',1),(4144,19,'2025-07-01',11),(4145,1,'2025-06-30',11),(4146,2,'2025-06-30',6),(4147,3,'2025-06-30',20),(4158,14,'2025-06-30',6),(4159,15,'2025-06-30',7),(4160,16,'2025-06-30',3),(4161,17,'2025-06-30',25),(4162,19,'2025-06-30',2),(4163,1,'2025-06-29',7),(4164,2,'2025-06-29',6),(4165,3,'2025-06-29',16),(4173,14,'2025-06-29',25),(4174,16,'2025-06-29',8),(4175,18,'2025-06-29',14),(4176,19,'2025-06-29',23),(4177,20,'2025-06-29',10),(4186,14,'2025-06-28',24),(4187,15,'2025-06-28',11),(4188,16,'2025-06-28',11),(4189,18,'2025-06-28',19),(4190,19,'2025-06-28',16),(4191,20,'2025-06-28',20),(4192,2,'2025-06-27',12),(4193,3,'2025-06-27',12),(4201,14,'2025-06-27',17),(4202,17,'2025-06-27',4),(4203,19,'2025-06-27',22),(4204,1,'2025-06-26',20),(4205,2,'2025-06-26',15),(4206,3,'2025-06-26',15),(4216,15,'2025-06-26',11),(4217,16,'2025-06-26',9),(4218,17,'2025-06-26',5),(4219,18,'2025-06-26',20),(4220,20,'2025-06-26',6),(4221,1,'2025-06-25',18),(4222,3,'2025-06-25',17),(4230,14,'2025-06-25',2),(4231,16,'2025-06-25',8),(4232,17,'2025-06-25',14),(4233,19,'2025-06-25',2),(4234,20,'2025-06-25',23),(4235,1,'2025-06-24',22),(4236,2,'2025-06-24',20),(4246,14,'2025-06-24',21),(4247,17,'2025-06-24',20),(4248,18,'2025-06-24',14),(4249,19,'2025-06-24',4),(4250,20,'2025-06-24',18),(4251,2,'2025-06-23',4),(4252,3,'2025-06-23',5),(4260,14,'2025-06-23',4),(4261,15,'2025-06-23',6),(4262,16,'2025-06-23',3),(4263,17,'2025-06-23',21),(4264,19,'2025-06-23',19),(4265,20,'2025-06-23',16),(4266,1,'2025-06-22',21),(4267,2,'2025-06-22',3),(4268,3,'2025-06-22',1),(4275,14,'2025-06-22',19),(4276,15,'2025-06-22',1),(4277,16,'2025-06-22',12),(4278,17,'2025-06-22',5),(4279,18,'2025-06-22',1),(4280,20,'2025-06-22',10),(4281,1,'2025-06-21',12),(4282,2,'2025-06-21',14),(4291,15,'2025-06-21',14),(4292,16,'2025-06-21',10),(4293,17,'2025-06-21',5),(4294,18,'2025-06-21',21),(4295,19,'2025-06-21',17),(4296,20,'2025-06-21',2),(4297,1,'2025-06-20',13),(4298,3,'2025-06-20',25),(4308,14,'2025-06-20',10),(4309,15,'2025-06-20',11),(4310,16,'2025-06-20',13),(4311,17,'2025-06-20',16),(4312,18,'2025-06-20',15),(4313,19,'2025-06-20',11),(4314,1,'2025-06-19',12),(4315,2,'2025-06-19',12),(4316,3,'2025-06-19',18),(4323,16,'2025-06-19',10),(4324,17,'2025-06-19',7),(4325,20,'2025-06-19',24),(4326,1,'2025-06-18',18),(4327,2,'2025-06-18',10),(4337,14,'2025-06-18',17),(4338,15,'2025-06-18',22),(4339,19,'2025-06-18',19),(4340,20,'2025-06-18',12),(4341,1,'2025-06-17',24),(4342,3,'2025-06-17',24),(4347,17,'2025-06-17',19),(4348,18,'2025-06-17',17),(4349,19,'2025-06-17',18),(4350,1,'2025-06-16',3),(4351,2,'2025-06-16',2),(4352,3,'2025-06-16',8),(4361,14,'2025-06-16',16),(4362,15,'2025-06-16',24),(4363,16,'2025-06-16',20),(4364,17,'2025-06-16',12),(4365,19,'2025-06-16',9),(4366,20,'2025-06-16',25),(4367,2,'2025-06-15',7),(4368,3,'2025-06-15',24),(4377,14,'2025-06-15',20),(4378,15,'2025-06-15',21),(4379,16,'2025-06-15',10),(4380,17,'2025-06-15',8),(4381,19,'2025-06-15',17),(4382,20,'2025-06-15',19),(4383,1,'2025-06-14',20),(4384,2,'2025-06-14',12),(4385,3,'2025-06-14',4),(4393,16,'2025-06-14',23),(4394,17,'2025-06-14',24),(4395,18,'2025-06-14',6),(4396,19,'2025-06-14',4),(4397,20,'2025-06-14',16),(4398,1,'2025-06-13',5),(4399,2,'2025-06-13',23),(4400,3,'2025-06-13',19),(4407,14,'2025-06-13',10),(4408,17,'2025-06-13',9),(4409,18,'2025-06-13',18),(4410,19,'2025-06-13',15),(4411,20,'2025-06-13',1),(4412,1,'2025-06-12',19),(4421,14,'2025-06-12',2),(4422,15,'2025-06-12',20),(4423,16,'2025-06-12',3),(4424,17,'2025-06-12',22),(4425,18,'2025-06-12',5),(4426,20,'2025-06-12',7),(4427,1,'2025-06-11',3),(4428,3,'2025-06-11',9),(4436,15,'2025-06-11',24),(4437,17,'2025-06-11',18),(4438,18,'2025-06-11',3),(4439,19,'2025-06-11',5),(4440,20,'2025-06-11',7),(4441,1,'2025-06-10',19),(4442,3,'2025-06-10',23),(4449,14,'2025-06-10',17),(4450,15,'2025-06-10',4),(4451,17,'2025-06-10',21),(4452,2,'2025-06-09',6),(4462,14,'2025-06-09',23),(4463,16,'2025-06-09',10),(4464,17,'2025-06-09',18),(4465,19,'2025-06-09',12),(4466,20,'2025-06-09',21),(4467,1,'2025-06-08',3),(4468,2,'2025-06-08',25),(4469,3,'2025-06-08',2),(4474,16,'2025-06-08',16),(4475,18,'2025-06-08',9),(4476,19,'2025-06-08',15),(4477,20,'2025-06-08',24),(4478,1,'2025-06-07',19),(4479,2,'2025-06-07',10),(4480,3,'2025-06-07',4),(4489,15,'2025-06-07',23),(4490,16,'2025-06-07',22),(4491,17,'2025-06-07',4),(4492,18,'2025-06-07',25),(4493,20,'2025-06-07',7),(4494,1,'2025-06-06',9),(4495,2,'2025-06-06',19),(4496,3,'2025-06-06',4),(4504,17,'2025-06-06',12),(4505,20,'2025-06-06',1),(4506,1,'2025-06-05',14),(4507,2,'2025-06-05',3),(4508,3,'2025-06-05',9),(4518,15,'2025-06-05',24),(4519,16,'2025-06-05',7),(4520,18,'2025-06-05',7),(4521,19,'2025-06-05',19),(4528,16,'2025-06-04',10),(4529,18,'2025-06-04',4),(4530,19,'2025-06-04',23),(4531,20,'2025-06-04',18),(4532,1,'2025-06-03',13),(4533,2,'2025-06-03',14),(4534,3,'2025-06-03',9),(4542,14,'2025-06-03',13),(4543,15,'2025-06-03',18),(4544,16,'2025-06-03',9),(4545,17,'2025-06-03',22),(4546,18,'2025-06-03',22),(4547,1,'2025-06-02',21),(4548,3,'2025-06-02',16),(4556,14,'2025-06-02',6),(4557,15,'2025-06-02',3),(4558,16,'2025-06-02',18),(4559,17,'2025-06-02',14),(4560,20,'2025-06-02',16),(4561,1,'2025-06-01',12),(4562,2,'2025-06-01',21),(4563,3,'2025-06-01',9),(4574,16,'2025-06-01',2),(4575,17,'2025-06-01',24),(4576,18,'2025-06-01',17),(4577,19,'2025-06-01',25),(4578,20,'2025-06-01',12),(4579,1,'2025-05-31',9),(4580,2,'2025-05-31',5),(4584,14,'2025-05-31',23),(4585,15,'2025-05-31',1),(4586,19,'2025-05-31',22),(4587,20,'2025-05-31',3),(4588,1,'2025-05-30',16),(4589,2,'2025-05-30',9),(4596,14,'2025-05-30',22),(4597,16,'2025-05-30',20),(4598,17,'2025-05-30',7),(4599,20,'2025-05-30',22),(4600,2,'2025-05-29',6),(4601,3,'2025-05-29',6),(4611,15,'2025-05-29',4),(4612,17,'2025-05-29',4),(4613,18,'2025-05-29',13),(4614,19,'2025-05-29',14),(4615,20,'2025-05-29',22),(4616,1,'2025-05-28',12),(4617,3,'2025-05-28',14),(4626,14,'2025-05-28',2),(4627,15,'2025-05-28',6),(4628,16,'2025-05-28',6),(4629,18,'2025-05-28',3),(4630,19,'2025-05-28',9),(4631,20,'2025-05-28',24),(4632,1,'2025-05-27',9),(4633,2,'2025-05-27',14),(4634,3,'2025-05-27',10),(4642,14,'2025-05-27',14),(4643,15,'2025-05-27',11),(4644,17,'2025-05-27',8),(4645,20,'2025-05-27',24),(4646,1,'2025-05-26',13),(4647,2,'2025-05-26',2),(4654,14,'2025-05-26',11),(4655,17,'2025-05-26',10),(4656,18,'2025-05-26',1),(4657,19,'2025-05-26',21),(4658,20,'2025-05-26',18),(4659,1,'2025-05-25',1),(4660,3,'2025-05-25',5),(4668,14,'2025-05-25',4),(4669,16,'2025-05-25',17),(4670,19,'2025-05-25',6),(4671,20,'2025-05-25',13),(4672,2,'2025-05-24',8),(4681,14,'2025-05-24',14),(4682,15,'2025-05-24',10),(4683,16,'2025-05-24',21),(4684,18,'2025-05-24',23),(4685,19,'2025-05-24',6),(4686,20,'2025-05-24',8),(4687,1,'2025-05-23',12),(4688,2,'2025-05-23',12),(4689,3,'2025-05-23',21),(4697,14,'2025-05-23',10),(4698,15,'2025-05-23',22),(4699,16,'2025-05-23',20),(4700,17,'2025-05-23',8),(4701,19,'2025-05-23',11),(4702,1,'2025-05-22',8),(4703,3,'2025-05-22',17),(4711,14,'2025-05-22',18),(4712,15,'2025-05-22',2),(4713,16,'2025-05-22',12),(4714,19,'2025-05-22',11),(4715,20,'2025-05-22',6),(4716,1,'2025-05-21',22),(4717,3,'2025-05-21',14),(4726,14,'2025-05-21',17),(4727,15,'2025-05-21',14),(4728,16,'2025-05-21',15),(4729,17,'2025-05-21',23),(4730,1,'2025-05-20',6),(4737,14,'2025-05-20',11),(4738,15,'2025-05-20',15),(4739,16,'2025-05-20',4),(4740,18,'2025-05-20',20),(4741,19,'2025-05-20',1),(4742,20,'2025-05-20',8),(4743,1,'2025-05-19',9),(4744,2,'2025-05-19',13),(4745,3,'2025-05-19',21),(4753,14,'2025-05-19',22),(4754,18,'2025-05-19',14),(4755,19,'2025-05-19',18),(4756,20,'2025-05-19',6),(4757,1,'2025-05-18',10),(4758,2,'2025-05-18',9),(4759,3,'2025-05-18',16),(4767,14,'2025-05-18',15),(4768,15,'2025-05-18',20),(4769,16,'2025-05-18',16),(4770,18,'2025-05-18',2),(4771,19,'2025-05-18',23),(4772,2,'2025-05-17',22),(4773,3,'2025-05-17',18),(4779,14,'2025-05-17',7),(4780,15,'2025-05-17',24),(4781,16,'2025-05-17',2),(4782,17,'2025-05-17',20),(4783,19,'2025-05-17',4),(4784,20,'2025-05-17',13),(4785,1,'2025-05-16',8),(4786,3,'2025-05-16',9),(4794,14,'2025-05-16',18),(4795,16,'2025-05-16',3),(4796,17,'2025-05-16',14),(4797,19,'2025-05-16',14),(4798,1,'2025-05-15',19),(4799,3,'2025-05-15',10),(4810,14,'2025-05-15',15),(4811,15,'2025-05-15',3),(4812,16,'2025-05-15',21),(4813,17,'2025-05-15',4),(4814,19,'2025-05-15',9),(4815,1,'2025-05-14',5),(4816,2,'2025-05-14',12),(4817,3,'2025-05-14',1),(4826,14,'2025-05-14',3),(4827,15,'2025-05-14',2),(4828,16,'2025-05-14',20),(4829,17,'2025-05-14',8),(4830,19,'2025-05-14',13),(4831,1,'2025-05-13',6),(4832,2,'2025-05-13',12),(4833,3,'2025-05-13',24),(4841,14,'2025-05-13',18),(4842,15,'2025-05-13',6),(4843,16,'2025-05-13',19),(4844,17,'2025-05-13',18),(4845,18,'2025-05-13',15),(4846,19,'2025-05-13',3),(4847,20,'2025-05-13',6),(4848,1,'2025-05-12',25),(4857,14,'2025-05-12',8),(4858,15,'2025-05-12',1),(4859,18,'2025-05-12',10),(4860,20,'2025-05-12',15),(4861,1,'2025-05-11',9),(4862,2,'2025-05-11',22),(4863,3,'2025-05-11',2),(4871,15,'2025-05-11',13),(4872,16,'2025-05-11',24),(4873,17,'2025-05-11',24),(4874,18,'2025-05-11',10),(4875,20,'2025-05-11',14),(4876,1,'2025-05-10',10),(4877,2,'2025-05-10',5),(4878,3,'2025-05-10',22),(4885,16,'2025-05-10',17),(4886,17,'2025-05-10',22),(4887,18,'2025-05-10',18),(4888,19,'2025-05-10',22),(4889,1,'2025-05-09',15),(4890,3,'2025-05-09',8),(4898,14,'2025-05-09',1),(4899,16,'2025-05-09',19),(4900,18,'2025-05-09',7),(4901,19,'2025-05-09',12),(4902,2,'2025-05-08',16),(4911,15,'2025-05-08',23),(4912,17,'2025-05-08',19),(4913,18,'2025-05-08',23),(4914,19,'2025-05-08',19),(4915,2,'2025-05-07',8),(4916,3,'2025-05-07',25),(4923,14,'2025-05-07',8),(4924,15,'2025-05-07',20),(4925,16,'2025-05-07',24),(4926,17,'2025-05-07',18),(4927,18,'2025-05-07',13),(4928,19,'2025-05-07',22),(4929,20,'2025-05-07',9),(4930,3,'2025-05-06',4),(4936,14,'2025-05-06',16),(4937,15,'2025-05-06',3),(4938,16,'2025-05-06',21),(4939,17,'2025-05-06',17),(4940,18,'2025-05-06',6),(4941,1,'2025-05-05',16),(4942,3,'2025-05-05',19),(4951,14,'2025-05-05',17),(4952,15,'2025-05-05',2),(4953,16,'2025-05-05',10),(4954,17,'2025-05-05',5),(4955,20,'2025-05-05',19),(4956,2,'2025-05-04',5),(4957,3,'2025-05-04',8),(4964,14,'2025-05-04',9),(4965,15,'2025-05-04',20),(4966,18,'2025-05-04',10),(4967,19,'2025-05-04',17),(4968,20,'2025-05-04',2),(4969,1,'2025-05-03',5),(4970,2,'2025-05-03',8),(4971,3,'2025-05-03',13),(4978,14,'2025-05-03',7),(4979,15,'2025-05-03',14),(4980,16,'2025-05-03',13),(4981,17,'2025-05-03',23),(4982,18,'2025-05-03',16),(4983,20,'2025-05-03',12),(4984,2,'2025-05-02',15),(4985,3,'2025-05-02',21),(4991,16,'2025-05-02',17),(4992,17,'2025-05-02',22),(4993,18,'2025-05-02',17),(4994,19,'2025-05-02',6),(4995,20,'2025-05-02',15),(4996,1,'2025-05-01',9),(4997,2,'2025-05-01',24),(5002,14,'2025-05-01',6),(5003,16,'2025-05-01',21),(5004,17,'2025-05-01',1),(5005,20,'2025-05-01',8),(5006,1,'2025-04-30',18),(5016,14,'2025-04-30',11),(5017,15,'2025-04-30',13),(5018,16,'2025-04-30',10),(5019,17,'2025-04-30',18),(5020,18,'2025-04-30',4),(5021,20,'2025-04-30',13),(5022,1,'2025-04-29',11),(5023,2,'2025-04-29',25),(5030,14,'2025-04-29',2),(5031,15,'2025-04-29',10),(5032,17,'2025-04-29',25),(5033,19,'2025-04-29',15),(5034,1,'2025-04-28',22),(5035,2,'2025-04-28',5),(5036,3,'2025-04-28',14),(5042,14,'2025-04-28',1),(5043,15,'2025-04-28',5),(5044,16,'2025-04-28',4),(5045,17,'2025-04-28',23),(5046,18,'2025-04-28',5),(5047,20,'2025-04-28',12),(5048,1,'2025-04-27',10),(5049,3,'2025-04-27',5),(5056,14,'2025-04-27',15),(5057,15,'2025-04-27',1),(5058,16,'2025-04-27',25),(5059,17,'2025-04-27',17),(5060,19,'2025-04-27',3),(5061,20,'2025-04-27',20),(5111,1,'2026-04-27',4),(5113,3,'2026-04-27',2),(5114,14,'2026-05-07',1),(5115,2,'2026-05-07',1),(5116,15,'2026-05-07',1);
/*!40000 ALTER TABLE `luot_truy_cap_ngay` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `luot_truy_cap_thiet_bi_ngay`
--

DROP TABLE IF EXISTS `luot_truy_cap_thiet_bi_ngay`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `luot_truy_cap_thiet_bi_ngay` (
  `idLuotTruyCapThietBiNgay` int(11) NOT NULL AUTO_INCREMENT,
  `idGianHang` int(11) NOT NULL,
  `maThietBi` varchar(100) NOT NULL,
  `ngay` date NOT NULL,
  `ngayTao` datetime NOT NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`idLuotTruyCapThietBiNgay`),
  UNIQUE KEY `uq_lttbtn_gianhang_thietbi_ngay` (`idGianHang`,`maThietBi`,`ngay`),
  KEY `idx_lttbtn_ngay` (`ngay`),
  KEY `idx_lttbtn_mathietbi_ngay` (`maThietBi`,`ngay`),
  CONSTRAINT `fk_lttbtn_gianhang` FOREIGN KEY (`idGianHang`) REFERENCES `gianhang` (`idGianHang`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `luot_truy_cap_thiet_bi_ngay`
--

LOCK TABLES `luot_truy_cap_thiet_bi_ngay` WRITE;
/*!40000 ALTER TABLE `luot_truy_cap_thiet_bi_ngay` DISABLE KEYS */;
INSERT INTO `luot_truy_cap_thiet_bi_ngay` VALUES (1,1,'SMOKE-TEST-DEVICE-1777255330','2026-04-27','2026-04-27 09:02:10'),(2,1,'PHONE-TEST-VIA-NGROK','2026-04-27','2026-04-27 09:23:36'),(3,3,'APP-CLIENT-25BCBD37F168464DA77ACC67698D17B2','2026-04-27','2026-04-27 09:32:40'),(4,14,'APP-CLIENT-E218502326004E439A51EBEE4B0DD316','2026-05-07','2026-05-07 19:49:29'),(5,2,'APP-CLIENT-E218502326004E439A51EBEE4B0DD316','2026-05-07','2026-05-07 19:55:21'),(6,15,'APP-CLIENT-E218502326004E439A51EBEE4B0DD316','2026-05-07','2026-05-07 20:42:42');
/*!40000 ALTER TABLE `luot_truy_cap_thiet_bi_ngay` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `monan`
--

DROP TABLE IF EXISTS `monan`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `monan` (
  `idMonAn` int(11) NOT NULL AUTO_INCREMENT,
  `idGianHang` int(11) NOT NULL,
  `ten` varchar(150) NOT NULL,
  `donGia` decimal(12,2) NOT NULL,
  `thoiGianCapNhat` datetime DEFAULT NULL,
  `tinhTrang` enum('con_ban','het_mon','ngung_ban') NOT NULL DEFAULT 'con_ban',
  PRIMARY KEY (`idMonAn`),
  KEY `idx_monan_idGianHang` (`idGianHang`),
  CONSTRAINT `fk_monAn_gianHang` FOREIGN KEY (`idGianHang`) REFERENCES `gianhang` (`idGianHang`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `monan`
--

LOCK TABLES `monan` WRITE;
/*!40000 ALTER TABLE `monan` DISABLE KEYS */;
INSERT INTO `monan` VALUES (1,1,'Banh trang nuong trung',20000.00,'2026-05-07 20:27:07','con_ban'),(2,1,'Banh trang nuong pho mai',25000.00,'2026-05-07 20:32:44','con_ban'),(3,2,'Tra sua truyen thong',30000.00,'2026-04-15 02:00:17','con_ban'),(4,2,'Tra dao cam sa',35000.00,'2026-04-15 02:00:17','con_ban'),(5,3,'Xien bo vien',15000.00,'2026-04-15 02:00:17','con_ban'),(6,3,'Xien ca vien',12000.00,'2026-04-15 02:00:17','con_ban');
/*!40000 ALTER TABLE `monan` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `monanngonngu`
--

DROP TABLE IF EXISTS `monanngonngu`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `monanngonngu` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `idMonAn` int(11) NOT NULL,
  `idNgonNgu` int(11) NOT NULL,
  `ten` varchar(150) NOT NULL,
  `moTa` text DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_monAn_ngonNgu` (`idMonAn`,`idNgonNgu`),
  KEY `idx_monanngonngu_idNgonNgu` (`idNgonNgu`),
  CONSTRAINT `fk_monAnNgonNgu_monAn` FOREIGN KEY (`idMonAn`) REFERENCES `monan` (`idMonAn`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_monAnNgonNgu_ngonNgu` FOREIGN KEY (`idNgonNgu`) REFERENCES `ngonngu` (`idNgonNgu`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `monanngonngu`
--

LOCK TABLES `monanngonngu` WRITE;
/*!40000 ALTER TABLE `monanngonngu` DISABLE KEYS */;
INSERT INTO `monanngonngu` VALUES (1,1,1,'Banh trang nuong trung','Banh trang nuong voi trung, hanh la va sot.'),(2,1,2,'Grilled Rice Paper with Egg','Grilled rice paper with egg, scallion, and sauce.'),(3,2,1,'Banh trang nuong pho mai','Banh trang nuong phu pho mai beo thom.'),(4,2,2,'Grilled Rice Paper with Cheese','Grilled rice paper topped with creamy cheese.'),(5,3,1,'Tra sua truyen thong','Tra sua vi truyen thong, thom tra va beo sua.'),(6,3,2,'Traditional Milk Tea','Classic milk tea with rich tea flavor and creamy milk.'),(7,4,1,'Tra dao cam sa','Thuc uong ket hop dao, cam va sa thanh mat.'),(8,4,2,'Peach Orange Lemongrass Tea','A refreshing drink made with peach, orange, and lemongrass.'),(9,5,1,'Xien bo vien','Bo vien nuong xien que, an kem tuong ot.'),(10,5,2,'Beef Ball Skewer','Grilled beef ball skewer served with chili sauce.'),(11,6,1,'Xien ca vien','Ca vien chien hoac nuong, phu hop an vat.'),(12,6,2,'Fish Ball Skewer','Fried or grilled fish ball skewer, suitable for snacks.');
/*!40000 ALTER TABLE `monanngonngu` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `ngonngu`
--

DROP TABLE IF EXISTS `ngonngu`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ngonngu` (
  `idNgonNgu` int(11) NOT NULL AUTO_INCREMENT,
  `maNgonNgu` varchar(10) NOT NULL,
  `ten` varchar(50) NOT NULL,
  `trangThai` enum('hoat_dong','an') NOT NULL DEFAULT 'hoat_dong',
  PRIMARY KEY (`idNgonNgu`),
  UNIQUE KEY `uq_ngonngu_maNgonNgu` (`maNgonNgu`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `ngonngu`
--

LOCK TABLES `ngonngu` WRITE;
/*!40000 ALTER TABLE `ngonngu` DISABLE KEYS */;
INSERT INTO `ngonngu` VALUES (1,'vi','Tieng Viet','hoat_dong'),(2,'en','English','hoat_dong'),(3,'ko','Korean','hoat_dong'),(4,'ja','Japanese','hoat_dong');
/*!40000 ALTER TABLE `ngonngu` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `phien_vao_app`
--

DROP TABLE IF EXISTS `phien_vao_app`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `phien_vao_app` (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `idThietBi` int(11) DEFAULT NULL,
  `maThietBi` varchar(255) NOT NULL,
  `idGoi` int(11) DEFAULT NULL,
  `qrRaw` text DEFAULT NULL,
  `accessToken` varchar(128) NOT NULL,
  `batDauLuc` datetime NOT NULL DEFAULT current_timestamp(),
  `hetHanLuc` datetime NOT NULL,
  `trangThai` enum('hieu_luc','het_han','huy') NOT NULL DEFAULT 'hieu_luc',
  PRIMARY KEY (`id`),
  UNIQUE KEY `uq_accessToken` (`accessToken`),
  KEY `idx_phien_vao_app_maThietBi` (`maThietBi`),
  KEY `idx_phien_vao_app_idThietBi` (`idThietBi`),
  KEY `idx_phien_vao_app_idGoi` (`idGoi`),
  CONSTRAINT `fk_phien_vao_app_goidichvu` FOREIGN KEY (`idGoi`) REFERENCES `goidichvu` (`idGoi`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `fk_phien_vao_app_thietbi_id` FOREIGN KEY (`idThietBi`) REFERENCES `thietbi` (`idThietBi`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `fk_phien_vao_app_thietbi_ma` FOREIGN KEY (`maThietBi`) REFERENCES `thietbi` (`maThietBi`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=35 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `phien_vao_app`
--

LOCK TABLES `phien_vao_app` WRITE;
/*!40000 ALTER TABLE `phien_vao_app` DISABLE KEYS */;
INSERT INTO `phien_vao_app` VALUES (20,6,'APP-CLIENT-665ACDF462C54606BC9CB7EB1B9BE8EC',3,'vkaccess://login?token=C1A31075E4D73D84D21262D1DD3E6994825107200BD0DCFEA794C63E03C707BE','C1A31075E4D73D84D21262D1DD3E6994825107200BD0DCFEA794C63E03C707BE','2026-04-21 00:30:29','2026-05-21 00:30:29','hieu_luc'),(21,10,'APP-CLIENT-354615A68010491AAB7BB738E9D116FF',3,'vkaccess://login?token=C3BA01677D6E5AB29AAE8E61E6A313356F2D336CCF3B5E2F395A09DA98C66A61','C3BA01677D6E5AB29AAE8E61E6A313356F2D336CCF3B5E2F395A09DA98C66A61','2026-04-21 00:36:20','2026-05-21 00:36:20','hieu_luc'),(29,13,'APP-CLIENT-001A3B51EDAA47CF9BFEC7B9E3545783',2,'vkaccess://login?token=B045C95635E60906AF7CAFE1152C3F11EF71519B75E716CCD12B0AD4270D12D2','B045C95635E60906AF7CAFE1152C3F11EF71519B75E716CCD12B0AD4270D12D2','2026-04-24 01:12:11','2026-05-01 01:12:11','hieu_luc'),(30,14,'APP-CLIENT-25BCBD37F168464DA77ACC67698D17B2',2,'vkaccess://login?token=3ABFA410FDA7A5FA44F4D7F466B8FE9393A0BC55DE2999E371FCDE464A058E69','3ABFA410FDA7A5FA44F4D7F466B8FE9393A0BC55DE2999E371FCDE464A058E69','2026-04-27 02:32:24','2026-05-04 02:32:24','het_han'),(31,15,'APP-CLIENT-E218502326004E439A51EBEE4B0DD316',2,'vkaccess://login?token=22297DE77EC9DE850404D2C67C3AC930135041A1FABBEED243565FD2E4498C08','22297DE77EC9DE850404D2C67C3AC930135041A1FABBEED243565FD2E4498C08','2026-05-04 13:29:48','2026-05-11 13:29:48','huy'),(32,15,'APP-CLIENT-E218502326004E439A51EBEE4B0DD316',1,'vkaccess://login?token=9C824CB162B7F34220C5BCC0586F359A3E6A6C971441AEA28850D33ECC7AEF16','9C824CB162B7F34220C5BCC0586F359A3E6A6C971441AEA28850D33ECC7AEF16','2026-05-07 13:06:06','2026-05-08 13:06:06','huy'),(33,15,'APP-CLIENT-E218502326004E439A51EBEE4B0DD316',4,'vkaccess://login?token=11CA9C67F679450027624BB254EA9C20241FA60C9D38338A0F0B2D270701A7A5','11CA9C67F679450027624BB254EA9C20241FA60C9D38338A0F0B2D270701A7A5','2026-05-07 13:08:06','2026-06-07 13:08:06','huy'),(34,15,'APP-CLIENT-E218502326004E439A51EBEE4B0DD316',5,'vkaccess://login?token=FBC5CD4AB814035DE1B67DEA534083BC6A3023C33DEE24385F51BC5E3DBB1875','FBC5CD4AB814035DE1B67DEA534083BC6A3023C33DEE24385F51BC5E3DBB1875','2026-05-07 13:15:28','2108-06-26 13:15:28','hieu_luc');
/*!40000 ALTER TABLE `phien_vao_app` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'NO_AUTO_VALUE_ON_ZERO' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_phien_vao_app_before_insert` BEFORE INSERT ON `phien_vao_app` FOR EACH ROW BEGIN
  DECLARE v_idThietBi INT DEFAULT NULL;
  DECLARE v_maThietBi VARCHAR(255) DEFAULT NULL;

  IF NEW.idThietBi IS NULL AND NEW.maThietBi IS NOT NULL AND NEW.maThietBi <> '' THEN
    SELECT tb.idThietBi
      INTO v_idThietBi
    FROM thietbi tb
    WHERE tb.maThietBi = NEW.maThietBi
    LIMIT 1;

    SET NEW.idThietBi = v_idThietBi;
  END IF;

  IF (NEW.maThietBi IS NULL OR NEW.maThietBi = '') AND NEW.idThietBi IS NOT NULL THEN
    SELECT tb.maThietBi
      INTO v_maThietBi
    FROM thietbi tb
    WHERE tb.idThietBi = NEW.idThietBi
    LIMIT 1;

    SET NEW.maThietBi = v_maThietBi;
  END IF;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_general_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'NO_AUTO_VALUE_ON_ZERO' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_phien_vao_app_before_update` BEFORE UPDATE ON `phien_vao_app` FOR EACH ROW BEGIN
  DECLARE v_idThietBi INT DEFAULT NULL;
  DECLARE v_maThietBi VARCHAR(255) DEFAULT NULL;

  IF NEW.idThietBi IS NULL AND NEW.maThietBi IS NOT NULL AND NEW.maThietBi <> '' THEN
    SELECT tb.idThietBi
      INTO v_idThietBi
    FROM thietbi tb
    WHERE tb.maThietBi = NEW.maThietBi
    LIMIT 1;

    SET NEW.idThietBi = v_idThietBi;
  END IF;

  IF (NEW.maThietBi IS NULL OR NEW.maThietBi = '') AND NEW.idThietBi IS NOT NULL THEN
    SELECT tb.maThietBi
      INTO v_maThietBi
    FROM thietbi tb
    WHERE tb.idThietBi = NEW.idThietBi
    LIMIT 1;

    SET NEW.maThietBi = v_maThietBi;
  END IF;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `taikhoan`
--

DROP TABLE IF EXISTS `taikhoan`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `taikhoan` (
  `idTaiKhoan` int(11) NOT NULL AUTO_INCREMENT,
  `email` varchar(100) NOT NULL,
  `matKhau` varchar(255) NOT NULL,
  `username` varchar(50) NOT NULL,
  `loaiTaiKhoan` enum('khach_hang','chu_quan_ly','admin') NOT NULL DEFAULT 'khach_hang',
  `tinhTrang` enum('hoat_dong','khoa') NOT NULL DEFAULT 'hoat_dong',
  `tinhTrangDangKy` enum('cho_duyet','da_duyet','tu_choi') NOT NULL DEFAULT 'da_duyet',
  `ngayTao` datetime NOT NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`idTaiKhoan`),
  UNIQUE KEY `uq_taikhoan_email` (`email`),
  UNIQUE KEY `uq_taikhoan_username` (`username`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `taikhoan`
--

LOCK TABLES `taikhoan` WRITE;
/*!40000 ALTER TABLE `taikhoan` DISABLE KEYS */;
INSERT INTO `taikhoan` VALUES (1,'1','1','1','admin','hoat_dong','da_duyet','2026-03-26 22:17:19'),(2,'kh2@gmail.com','123456','khachhang2','khach_hang','hoat_dong','da_duyet','2026-03-26 22:17:19'),(3,'chu1@demo.com','123456','chuquanly01','chu_quan_ly','hoat_dong','da_duyet','2026-04-08 23:22:49'),(4,'kh1@gmail.com','123456','khachhang1','khach_hang','hoat_dong','da_duyet','2026-03-26 22:17:19');
/*!40000 ALTER TABLE `taikhoan` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `thietbi`
--

DROP TABLE IF EXISTS `thietbi`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `thietbi` (
  `idThietBi` int(11) NOT NULL AUTO_INCREMENT,
  `maThietBi` varchar(255) NOT NULL,
  `maKichHoat` varchar(100) DEFAULT NULL,
  `idTaiKhoan` int(11) DEFAULT NULL,
  `daKichHoat` tinyint(1) NOT NULL DEFAULT 0,
  `thoiGianKichHoat` datetime DEFAULT NULL,
  `ngayTao` datetime NOT NULL DEFAULT current_timestamp(),
  `lanCuoiHoatDong` datetime DEFAULT NULL,
  `trangThai` enum('cho_kich_hoat','hoat_dong','khoa') NOT NULL DEFAULT 'cho_kich_hoat',
  `loaiThietBi` enum('app_client','portal_web','hardware') NOT NULL DEFAULT 'app_client',
  `platform` varchar(32) DEFAULT NULL,
  `model` varchar(128) DEFAULT NULL,
  `manufacturer` varchar(128) DEFAULT NULL,
  `appVersion` varchar(32) DEFAULT NULL,
  PRIMARY KEY (`idThietBi`),
  UNIQUE KEY `uq_thietbi_maThietBi` (`maThietBi`),
  KEY `idx_thietbi_idTaiKhoan` (`idTaiKhoan`),
  KEY `idx_loai_lanCuoi` (`loaiThietBi`,`lanCuoiHoatDong`),
  CONSTRAINT `fk_thietbi_taikhoan` FOREIGN KEY (`idTaiKhoan`) REFERENCES `taikhoan` (`idTaiKhoan`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=16 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `thietbi`
--

LOCK TABLES `thietbi` WRITE;
/*!40000 ALTER TABLE `thietbi` DISABLE KEYS */;
INSERT INTO `thietbi` VALUES (1,'DEVICE-DEMO-001','ACT-001',NULL,1,'2026-04-15 02:00:17','2026-04-15 02:00:17','2026-04-15 02:00:17','hoat_dong','hardware',NULL,NULL,NULL,NULL),(2,'DEVICE-DEMO-002','ACT-002',NULL,0,NULL,'2026-04-15 02:00:17',NULL,'cho_kich_hoat','hardware',NULL,NULL,NULL,NULL),(3,'DEVICE-PACKAGE-PORTAL','ACT-PACKAGE-PORTAL',NULL,1,'2026-04-15 23:39:30','2026-04-15 23:39:30','2026-04-16 01:09:39','hoat_dong','portal_web',NULL,NULL,NULL,NULL),(4,'APP-CLIENT-2DEBB7C666FC4D8FB67586A2ADA1B403','ACT-APP-CLIENT-2DEBB7C666FC4D8FB67586A2ADA1B403',NULL,1,'2026-04-16 01:13:24','2026-04-16 01:13:24','2026-04-23 01:19:51','hoat_dong','app_client','Android','sdk_gphone64_x86_64','Google','1.0'),(5,'APP-CLIENT-564E3C683FA64FB8B2AC10624ABA88CB','ACT-APP-CLIENT-564E3C683FA64FB8B2AC10624ABA88CB',NULL,1,'2026-04-16 14:29:03','2026-04-16 14:29:03','2026-04-17 02:30:26','hoat_dong','app_client',NULL,NULL,NULL,NULL),(6,'APP-CLIENT-665ACDF462C54606BC9CB7EB1B9BE8EC','ACT-APP-CLIENT-665ACDF462C54606BC9CB7EB1B9BE8EC',NULL,1,'2026-04-17 08:10:40','2026-04-17 08:10:40','2026-04-21 07:40:29','hoat_dong','app_client','Android','M2101K6G','Xiaomi','1.0'),(7,'APP-CLIENT-8B62701DC01F4AAA802789862106629D','ACT-APP-CLIENT-8B62701DC01F4AAA802789862106629D',NULL,1,'2026-04-17 08:55:30','2026-04-17 08:55:30','2026-04-17 09:18:12','hoat_dong','app_client',NULL,NULL,NULL,NULL),(8,'APP-CLIENT-AE58F501CB654A71A5E12E7E25AFB051','ACT-APP-CLIENT-AE58F501CB654A71A5E12E7E25AFB051',NULL,1,'2026-04-17 09:25:27','2026-04-17 09:25:27','2026-04-17 09:25:34','hoat_dong','app_client',NULL,NULL,NULL,NULL),(9,'APP-CLIENT-DF4D7217011A45F980D11709417F6CB9','ACT-APP-CLIENT-DF4D7217011A45F980D11709417F6CB9',NULL,1,'2026-04-20 09:50:00','2026-04-20 09:50:00','2026-04-20 10:36:18','hoat_dong','app_client','Android','CPH2743','OPPO','1.0'),(10,'APP-CLIENT-354615A68010491AAB7BB738E9D116FF','ACT-APP-CLIENT-354615A68010491AAB7BB738E9D116FF',NULL,1,'2026-04-21 07:36:20','2026-04-21 07:36:20','2026-04-21 07:41:20','hoat_dong','app_client',NULL,NULL,NULL,NULL),(11,'APP-CLIENT-105CBB0F06044344A4E57D5373837CC3','ACT-APP-CLIENT-105CBB0F06044344A4E57D5373837CC3',NULL,1,'2026-04-23 01:22:44','2026-04-23 01:22:44','2026-04-23 02:13:09','hoat_dong','app_client','Android','sdk_gphone64_x86_64','Google','1.0'),(12,'APP-CLIENT-DA95C3C4A3CB4AB09AB8FF6D02E28D70','ACT-APP-CLIENT-DA95C3C4A3CB4AB09AB8FF6D02E28D70',NULL,1,'2026-04-23 14:16:58','2026-04-23 14:16:58','2026-04-24 02:15:27','hoat_dong','app_client','Android','sdk_gphone64_x86_64','Google','1.0.2'),(13,'APP-CLIENT-001A3B51EDAA47CF9BFEC7B9E3545783','ACT-APP-CLIENT-001A3B51EDAA47CF9BFEC7B9E3545783',NULL,1,'2026-04-24 08:08:38','2026-04-24 08:08:38','2026-04-27 09:21:36','hoat_dong','app_client','Android','M2101K6G','Xiaomi','1.0.5'),(14,'APP-CLIENT-25BCBD37F168464DA77ACC67698D17B2','ACT-APP-CLIENT-25BCBD37F168464DA77ACC67698D17B2',NULL,1,'2026-04-27 09:32:21','2026-04-27 09:32:21','2026-04-27 09:33:14','hoat_dong','app_client','Android','M2101K6G','Xiaomi','1.0.6'),(15,'APP-CLIENT-E218502326004E439A51EBEE4B0DD316','ACT-APP-CLIENT-E218502326004E439A51EBEE4B0DD316',NULL,1,'2026-05-04 20:29:45','2026-05-04 20:29:45','2026-05-07 20:44:58','hoat_dong','app_client','Android','M2101K6G','Xiaomi','1.0.7');
/*!40000 ALTER TABLE `thietbi` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tour`
--

DROP TABLE IF EXISTS `tour`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tour` (
  `idTour` int(11) NOT NULL AUTO_INCREMENT,
  `ten` varchar(255) NOT NULL,
  `moTa` text DEFAULT NULL,
  `idNgonNgu` int(11) DEFAULT NULL,
  `doDaiPhutDeXuat` int(11) DEFAULT NULL,
  `anhBia` varchar(500) DEFAULT NULL,
  `danhMuc` varchar(100) DEFAULT NULL,
  `tinhTrang` enum('hoat_dong','an') NOT NULL DEFAULT 'hoat_dong',
  `ngayTao` datetime NOT NULL DEFAULT current_timestamp(),
  `ngayCapNhat` datetime NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`idTour`),
  KEY `idx_tour_tinhtrang` (`tinhTrang`),
  KEY `idx_tour_ngonngu` (`idNgonNgu`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tour`
--

LOCK TABLES `tour` WRITE;
/*!40000 ALTER TABLE `tour` DISABLE KEYS */;
INSERT INTO `tour` VALUES (1,'Asia Food Tour Demo','3 stop am thuc',NULL,NULL,NULL,NULL,'hoat_dong','2026-05-04 12:45:53','2026-05-04 12:45:53'),(2,'ski','',1,NULL,NULL,NULL,'hoat_dong','2026-05-04 19:19:08','2026-05-04 19:19:08'),(4,'Bánh tráng nướng Cô Ba','',NULL,NULL,NULL,NULL,'hoat_dong','2026-05-04 19:46:24','2026-05-04 19:46:24'),(5,'test','',NULL,NULL,NULL,NULL,'an','2026-05-04 19:59:03','2026-05-05 07:32:40'),(8,'Bánh tráng nướng Cô Ba','không biết nói gì',NULL,NULL,NULL,NULL,'hoat_dong','2026-05-07 20:41:35','2026-05-07 20:41:35');
/*!40000 ALTER TABLE `tour` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tour_diem`
--

DROP TABLE IF EXISTS `tour_diem`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tour_diem` (
  `idTourDiem` int(11) NOT NULL AUTO_INCREMENT,
  `idTour` int(11) NOT NULL,
  `idGianHang` int(11) NOT NULL,
  `thuTu` int(11) NOT NULL,
  `audioIntroUrl` varchar(500) DEFAULT NULL,
  `thoiGianDeXuatPhut` int(11) DEFAULT NULL,
  `ghiChu` text DEFAULT NULL,
  PRIMARY KEY (`idTourDiem`),
  UNIQUE KEY `uq_tour_thutu` (`idTour`,`thuTu`),
  UNIQUE KEY `uq_tour_gianhang` (`idTour`,`idGianHang`),
  KEY `idx_tour_diem_tour` (`idTour`),
  KEY `fk_tour_diem_gianhang` (`idGianHang`),
  CONSTRAINT `fk_tour_diem_gianhang` FOREIGN KEY (`idGianHang`) REFERENCES `gianhang` (`idGianHang`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_tour_diem_tour` FOREIGN KEY (`idTour`) REFERENCES `tour` (`idTour`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=33 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tour_diem`
--

LOCK TABLES `tour_diem` WRITE;
/*!40000 ALTER TABLE `tour_diem` DISABLE KEYS */;
INSERT INTO `tour_diem` VALUES (1,1,1,1,NULL,NULL,NULL),(2,1,2,2,NULL,NULL,NULL),(3,1,3,3,NULL,NULL,NULL),(11,4,14,1,NULL,NULL,NULL),(12,4,3,2,NULL,NULL,NULL),(13,4,1,3,NULL,NULL,NULL),(14,4,2,4,NULL,NULL,NULL),(19,2,3,1,NULL,NULL,NULL),(20,2,1,2,NULL,NULL,NULL),(21,2,2,3,NULL,NULL,NULL),(22,2,14,4,NULL,NULL,NULL),(23,5,2,1,NULL,NULL,NULL),(24,5,14,2,NULL,NULL,NULL),(25,5,1,3,NULL,NULL,NULL),(26,5,3,4,NULL,NULL,NULL),(27,8,15,1,NULL,NULL,NULL),(28,8,16,2,NULL,NULL,NULL),(29,8,2,3,NULL,NULL,NULL),(30,8,1,4,NULL,NULL,NULL),(31,8,3,5,NULL,NULL,NULL),(32,8,14,6,NULL,NULL,NULL);
/*!40000 ALTER TABLE `tour_diem` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tour_tien_do`
--

DROP TABLE IF EXISTS `tour_tien_do`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `tour_tien_do` (
  `idTourTienDo` int(11) NOT NULL AUTO_INCREMENT,
  `idTour` int(11) NOT NULL,
  `maThietBi` varchar(100) NOT NULL,
  `stepHienTai` int(11) NOT NULL DEFAULT 0,
  `startedAt` datetime DEFAULT NULL,
  `completedAt` datetime DEFAULT NULL,
  `ngayCapNhat` datetime NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  PRIMARY KEY (`idTourTienDo`),
  UNIQUE KEY `uq_tien_do_tour_thietbi` (`idTour`,`maThietBi`),
  KEY `idx_tien_do_completed` (`completedAt`),
  CONSTRAINT `fk_tien_do_tour` FOREIGN KEY (`idTour`) REFERENCES `tour` (`idTour`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tour_tien_do`
--

LOCK TABLES `tour_tien_do` WRITE;
/*!40000 ALTER TABLE `tour_tien_do` DISABLE KEYS */;
INSERT INTO `tour_tien_do` VALUES (1,1,'DEV-TEST-1',3,'2026-05-04 12:47:45','2026-05-04 05:47:45','2026-05-04 12:47:45'),(3,1,'DEV-TEST-2',3,'2026-05-04 13:15:32','2026-05-04 06:15:32','2026-05-04 13:15:32'),(5,1,'DEV-TEST-3',3,'2026-05-04 13:15:32','2026-05-04 06:15:32','2026-05-04 13:15:32'),(6,1,'SMOKE-X',2,'2026-05-04 19:04:29',NULL,'2026-05-04 19:04:29'),(7,1,'codex-advance-check',2,'2026-05-04 19:09:21',NULL,'2026-05-04 19:09:21');
/*!40000 ALTER TABLE `tour_tien_do` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `yeucaugianhang`
--

DROP TABLE IF EXISTS `yeucaugianhang`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `yeucaugianhang` (
  `idYeuCau` int(11) NOT NULL AUTO_INCREMENT,
  `idChuQuanLy` int(11) NOT NULL,
  `tenDeNghi` varchar(150) NOT NULL,
  `diaChiDeNghi` varchar(255) DEFAULT NULL,
  `ghiChuGui` text DEFAULT NULL,
  `trangThai` enum('cho_duyet','da_duyet','tu_choi','cho_thanh_toan') NOT NULL DEFAULT 'cho_duyet',
  `idGianHang` int(11) DEFAULT NULL,
  `ngayGui` datetime NOT NULL DEFAULT current_timestamp(),
  `ngayXuLy` datetime DEFAULT NULL,
  PRIMARY KEY (`idYeuCau`),
  KEY `idx_yeucaugianhang_owner` (`idChuQuanLy`),
  KEY `idx_yeucaugianhang_status` (`trangThai`),
  KEY `idx_yeucaugianhang_store` (`idGianHang`),
  CONSTRAINT `fk_yeucaugianhang_owner` FOREIGN KEY (`idChuQuanLy`) REFERENCES `chu_quan_ly` (`idChuQuanLy`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_yeucaugianhang_store` FOREIGN KEY (`idGianHang`) REFERENCES `gianhang` (`idGianHang`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `yeucaugianhang`
--

LOCK TABLES `yeucaugianhang` WRITE;
/*!40000 ALTER TABLE `yeucaugianhang` DISABLE KEYS */;
INSERT INTO `yeucaugianhang` VALUES (9,1,'Tokyo Takoyaki','3 Trần duy',NULL,'cho_thanh_toan',14,'2026-05-04 19:30:03','2026-05-04 19:32:30');
/*!40000 ALTER TABLE `yeucaugianhang` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-05-07 21:02:09
