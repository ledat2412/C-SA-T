-- Migration: thêm loaiThietBi + metadata cho bảng thietbi
-- Apply trên MySQL/MariaDB

ALTER TABLE thietbi
  ADD COLUMN loaiThietBi ENUM('app_client','portal_web','hardware') NOT NULL DEFAULT 'app_client' AFTER trangThai,
  ADD COLUMN platform VARCHAR(32) NULL AFTER loaiThietBi,
  ADD COLUMN model VARCHAR(128) NULL AFTER platform,
  ADD COLUMN manufacturer VARCHAR(128) NULL AFTER model,
  ADD COLUMN appVersion VARCHAR(32) NULL AFTER manufacturer,
  ADD INDEX idx_loai_lanCuoi (loaiThietBi, lanCuoiHoatDong);

UPDATE thietbi SET loaiThietBi = 'portal_web' WHERE maThietBi LIKE 'DEVICE-PACKAGE-PORTAL%';
UPDATE thietbi SET loaiThietBi = 'app_client' WHERE maThietBi LIKE 'APP-CLIENT-%';
UPDATE thietbi SET loaiThietBi = 'hardware'
  WHERE maThietBi NOT LIKE 'APP-CLIENT-%'
    AND maThietBi NOT LIKE 'DEVICE-PACKAGE-PORTAL%';
