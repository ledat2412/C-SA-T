-- Sync missing schema/data from the 2026-04-21 test dump.
-- Safe to run on an existing MariaDB 10.4 database: it adds missing columns
-- and uses INSERT IGNORE so existing rows are preserved.

ALTER TABLE thietbi
  ADD COLUMN IF NOT EXISTS loaiThietBi ENUM('app_client','portal_web','hardware') NOT NULL DEFAULT 'app_client' AFTER trangThai,
  ADD COLUMN IF NOT EXISTS platform VARCHAR(32) NULL AFTER loaiThietBi,
  ADD COLUMN IF NOT EXISTS model VARCHAR(128) NULL AFTER platform,
  ADD COLUMN IF NOT EXISTS manufacturer VARCHAR(128) NULL AFTER model,
  ADD COLUMN IF NOT EXISTS appVersion VARCHAR(32) NULL AFTER manufacturer,
  ADD INDEX IF NOT EXISTS idx_loai_lanCuoi (loaiThietBi, lanCuoiHoatDong);

UPDATE thietbi SET loaiThietBi = 'portal_web' WHERE maThietBi LIKE 'DEVICE-PACKAGE-PORTAL%';
UPDATE thietbi SET loaiThietBi = 'app_client' WHERE maThietBi LIKE 'APP-CLIENT-%';
UPDATE thietbi
SET loaiThietBi = 'hardware'
WHERE maThietBi NOT LIKE 'APP-CLIENT-%'
  AND maThietBi NOT LIKE 'DEVICE-PACKAGE-PORTAL%';

UPDATE thietbi
SET platform = COALESCE(platform, 'Android'),
    model = COALESCE(model, 'sdk_gphone64_x86_64'),
    manufacturer = COALESCE(manufacturer, 'Google'),
    appVersion = COALESCE(appVersion, '1.0')
WHERE maThietBi = 'APP-CLIENT-2DEBB7C666FC4D8FB67586A2ADA1B403';

INSERT IGNORE INTO taikhoan (`idTaiKhoan`, `email`, `matKhau`, `username`, `loaiTaiKhoan`, `tinhTrang`, `tinhTrangDangKy`, `ngayTao`) VALUES
(5, 'finneybea58@hotmail.com', '12345678', '321', 'chu_quan_ly', 'hoat_dong', 'da_duyet', '2026-04-16 21:26:44'),
(6, 'finneybea58_2@hotmail.com', '12345678', '321x', 'chu_quan_ly', 'hoat_dong', 'da_duyet', '2026-04-16 21:27:33'),
(7, 'finneybea528@hotmail.com', '12345678', '122', 'chu_quan_ly', 'hoat_dong', 'da_duyet', '2026-04-16 21:28:38');

INSERT IGNORE INTO chu_quan_ly (`idChuQuanLy`, `idTaiKhoan`, `hoTen`, `sdt`, `diaChi`, `ngayTao`) VALUES
(2, 5, 'test', NULL, NULL, '2026-04-16 21:26:44'),
(3, 6, 'test2', NULL, NULL, '2026-04-16 21:27:33'),
(4, 7, 'test23', NULL, NULL, '2026-04-16 21:28:38');

INSERT IGNORE INTO gianhangngonngu (`id`, `idGianHang`, `idNgonNgu`, `ten`, `audioURL`, `moTa`) VALUES
(10, 1, 3, 'Banh trang nuong Co Ba', 'audio/gianhang_1_ko.mp3', 'こんにちは'),
(11, 1, 4, 'Banh trang nuong Co Ba', 'audio/gianhang_1_ja.mp3', 'こんにちは');

INSERT IGNORE INTO thietbi (`idThietBi`, `maThietBi`, `maKichHoat`, `idTaiKhoan`, `daKichHoat`, `thoiGianKichHoat`, `ngayTao`, `lanCuoiHoatDong`, `trangThai`, `loaiThietBi`, `platform`, `model`, `manufacturer`, `appVersion`) VALUES
(5, 'APP-CLIENT-564E3C683FA64FB8B2AC10624ABA88CB', 'ACT-APP-CLIENT-564E3C683FA64FB8B2AC10624ABA88CB', NULL, 1, '2026-04-16 14:29:03', '2026-04-16 14:29:03', '2026-04-17 02:30:26', 'hoat_dong', 'app_client', NULL, NULL, NULL, NULL),
(6, 'APP-CLIENT-665ACDF462C54606BC9CB7EB1B9BE8EC', 'ACT-APP-CLIENT-665ACDF462C54606BC9CB7EB1B9BE8EC', NULL, 1, '2026-04-17 08:10:40', '2026-04-17 08:10:40', '2026-04-21 07:40:29', 'hoat_dong', 'app_client', 'Android', 'M2101K6G', 'Xiaomi', '1.0'),
(7, 'APP-CLIENT-8B62701DC01F4AAA802789862106629D', 'ACT-APP-CLIENT-8B62701DC01F4AAA802789862106629D', NULL, 1, '2026-04-17 08:55:30', '2026-04-17 08:55:30', '2026-04-17 09:18:12', 'hoat_dong', 'app_client', NULL, NULL, NULL, NULL),
(8, 'APP-CLIENT-AE58F501CB654A71A5E12E7E25AFB051', 'ACT-APP-CLIENT-AE58F501CB654A71A5E12E7E25AFB051', NULL, 1, '2026-04-17 09:25:27', '2026-04-17 09:25:27', '2026-04-17 09:25:34', 'hoat_dong', 'app_client', NULL, NULL, NULL, NULL),
(9, 'APP-CLIENT-DF4D7217011A45F980D11709417F6CB9', 'ACT-APP-CLIENT-DF4D7217011A45F980D11709417F6CB9', NULL, 1, '2026-04-20 09:50:00', '2026-04-20 09:50:00', '2026-04-20 10:36:18', 'hoat_dong', 'app_client', 'Android', 'CPH2743', 'OPPO', '1.0'),
(10, 'APP-CLIENT-354615A68010491AAB7BB738E9D116FF', 'ACT-APP-CLIENT-354615A68010491AAB7BB738E9D116FF', NULL, 1, '2026-04-21 07:36:20', '2026-04-21 07:36:20', '2026-04-21 07:41:20', 'hoat_dong', 'app_client', NULL, NULL, NULL, NULL);

INSERT IGNORE INTO phien_vao_app (`id`, `idThietBi`, `maThietBi`, `idGoi`, `qrRaw`, `accessToken`, `batDauLuc`, `hetHanLuc`, `trangThai`) VALUES
(15, 6, 'APP-CLIENT-665ACDF462C54606BC9CB7EB1B9BE8EC', 3, 'vkaccess://login?token=7FC5AF1C1EAD698A0A0F780B8AA43E2648D775AD29E5DFA73BC2FBE97B061DC0', '7FC5AF1C1EAD698A0A0F780B8AA43E2648D775AD29E5DFA73BC2FBE97B061DC0', '2026-04-20 00:32:45', '2026-05-20 00:32:45', 'huy'),
(16, 4, 'APP-CLIENT-2DEBB7C666FC4D8FB67586A2ADA1B403', 1, 'vkaccess://login?token=4046B209A85C2FB7438323F5E6C52D086A3FDEE612A8E0839D8BF0D59030ECD4', '4046B209A85C2FB7438323F5E6C52D086A3FDEE612A8E0839D8BF0D59030ECD4', '2026-04-20 01:12:55', '2026-04-21 01:12:55', 'huy'),
(17, 4, 'APP-CLIENT-2DEBB7C666FC4D8FB67586A2ADA1B403', 1, 'vkaccess://login?token=D697EEC3EF629924A02958DCC1A7B249EAB87579F5D95B46F88D83433FBB55F4', 'D697EEC3EF629924A02958DCC1A7B249EAB87579F5D95B46F88D83433FBB55F4', '2026-04-20 02:14:14', '2026-04-21 02:14:14', 'hieu_luc'),
(18, 9, 'APP-CLIENT-DF4D7217011A45F980D11709417F6CB9', 1, 'vkaccess://login?token=1D8AB863D1B25CA4AE7DAD3000E73E8006C09C965F69E0028A248C7F1D7DC621', '1D8AB863D1B25CA4AE7DAD3000E73E8006C09C965F69E0028A248C7F1D7DC621', '2026-04-20 02:50:00', '2026-04-21 02:50:00', 'huy'),
(19, 9, 'APP-CLIENT-DF4D7217011A45F980D11709417F6CB9', 1, 'vkaccess://login?token=02A466FC81F16F5AF3218244EC8059018E17AB6E9788CF49A37AF54D070A9DCC', '02A466FC81F16F5AF3218244EC8059018E17AB6E9788CF49A37AF54D070A9DCC', '2026-04-20 02:54:23', '2026-04-21 02:54:23', 'hieu_luc'),
(20, 6, 'APP-CLIENT-665ACDF462C54606BC9CB7EB1B9BE8EC', 3, 'vkaccess://login?token=C1A31075E4D73D84D21262D1DD3E6994825107200BD0DCFEA794C63E03C707BE', 'C1A31075E4D73D84D21262D1DD3E6994825107200BD0DCFEA794C63E03C707BE', '2026-04-21 00:30:29', '2026-05-21 00:30:29', 'hieu_luc'),
(21, 10, 'APP-CLIENT-354615A68010491AAB7BB738E9D116FF', 3, 'vkaccess://login?token=C3BA01677D6E5AB29AAE8E61E6A313356F2D336CCF3B5E2F395A09DA98C66A61', 'C3BA01677D6E5AB29AAE8E61E6A313356F2D336CCF3B5E2F395A09DA98C66A61', '2026-04-21 00:36:20', '2026-05-21 00:36:20', 'hieu_luc');

INSERT IGNORE INTO hoadon (`idHoaDon`, `idKhachHang`, `idPhienVaoApp`, `idGoi`, `email`, `tongTien`, `thoiGianTao`, `tinhTrang`, `ghiChu`) VALUES
(17, NULL, 15, 3, 'caohoangthinh2@gmail.com', 500000.00, '2026-04-20 07:32:45', 'da_thanh_toan', 'Bypass thanh toan QR de test package access.'),
(18, NULL, 16, 1, 'ledat241205@gmail.com', 15000.00, '2026-04-20 08:12:55', 'da_thanh_toan', 'Bypass thanh toan QR de test package access.'),
(19, NULL, 17, 1, 'ledat241205@gmail.com', 15000.00, '2026-04-20 09:14:14', 'da_thanh_toan', 'Bypass thanh toan QR de test package access.'),
(20, NULL, 18, 1, 'ledat241205@gmail.com', 15000.00, '2026-04-20 09:50:00', 'da_thanh_toan', 'Bypass thanh toan QR de test package access.'),
(21, NULL, 19, 1, 'ledat241204@gmail.com', 15000.00, '2026-04-20 09:54:23', 'da_thanh_toan', 'Bypass thanh toan QR de test package access.'),
(22, NULL, 20, 3, 'caohoangthinh2@gmail.com', 500000.00, '2026-04-21 07:30:29', 'da_thanh_toan', 'Bypass thanh toan QR de test package access.'),
(23, NULL, 21, 3, 'vobao142@gmail.com', 500000.00, '2026-04-21 07:36:20', 'da_thanh_toan', 'Bypass thanh toan QR de test package access.');

ALTER TABLE taikhoan AUTO_INCREMENT = 8;
ALTER TABLE chu_quan_ly AUTO_INCREMENT = 5;
ALTER TABLE gianhangngonngu AUTO_INCREMENT = 15;
ALTER TABLE thietbi AUTO_INCREMENT = 11;
ALTER TABLE phien_vao_app AUTO_INCREMENT = 22;
ALTER TABLE hoadon AUTO_INCREMENT = 24;
