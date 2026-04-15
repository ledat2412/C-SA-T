<?php

if (!function_exists('admin_base_path')) {
    function admin_base_path()
    {
        static $basePath = null;

        if ($basePath !== null) {
            return $basePath;
        }

        $scriptName = isset($_SERVER['SCRIPT_NAME']) ? str_replace('\\', '/', (string) $_SERVER['SCRIPT_NAME']) : '';
        $directory = str_replace('\\', '/', dirname($scriptName));

        if ($directory === '/' || $directory === '\\' || $directory === '.') {
            $basePath = '';
            return $basePath;
        }

        $basePath = rtrim($directory, '/');
        return $basePath;
    }
}

if (!function_exists('admin_url')) {
    function admin_url($path = '')
    {
        $path = ltrim((string) $path, '/');
        $basePath = admin_base_path();

        if ($path === '') {
            return $basePath !== '' ? $basePath : '/';
        }

        return ($basePath !== '' ? $basePath : '') . '/' . $path;
    }
}

if (!function_exists('backend_base_url')) {
    function backend_base_url()
    {
        static $baseUrl = null;

        if ($baseUrl !== null) {
            return $baseUrl;
        }

        $configuredUrl = getenv('CSA_BACKEND_BASE_URL');
        if (is_string($configuredUrl) && trim($configuredUrl) !== '') {
            $baseUrl = rtrim(trim($configuredUrl), '/');
            return $baseUrl;
        }

        $baseUrl = 'https://localhost:7123';
        return $baseUrl;
    }
}

if (!function_exists('backend_api_url')) {
    function backend_api_url($path = '')
    {
        return backend_base_url() . '/api/' . ltrim((string) $path, '/');
    }
}

if (!function_exists('backend_public_url')) {
    function backend_public_url($path = '')
    {
        return backend_base_url() . '/' . ltrim((string) $path, '/');
    }
}

if (!function_exists('backend_connection_settings')) {
    function backend_connection_settings()
    {
        static $settings = null;

        if ($settings !== null) {
            return $settings;
        }

        $appSettingsPath = dirname(__DIR__) . '/VinhKhanh/VinhKhanh/appsettings.json';
        if (!is_file($appSettingsPath)) {
            $settings = array();
            return $settings;
        }

        $raw = file_get_contents($appSettingsPath);
        $decoded = is_string($raw) ? json_decode($raw, true) : null;
        if (!is_array($decoded) || empty($decoded['ConnectionStrings']['DefaultConnection'])) {
            $settings = array();
            return $settings;
        }

        $parts = explode(';', (string) $decoded['ConnectionStrings']['DefaultConnection']);
        $parsed = array();
        foreach ($parts as $part) {
            $part = trim($part);
            if ($part === '' || strpos($part, '=') === false) {
                continue;
            }

            list($key, $value) = explode('=', $part, 2);
            $parsed[strtolower(trim($key))] = trim($value);
        }

        $settings = array(
            'host' => isset($parsed['server']) ? $parsed['server'] : '127.0.0.1',
            'port' => isset($parsed['port']) ? (int) $parsed['port'] : 3306,
            'database' => isset($parsed['database']) ? $parsed['database'] : 'gianhang',
            'username' => isset($parsed['uid']) ? $parsed['uid'] : 'root',
            'password' => isset($parsed['pwd']) ? $parsed['pwd'] : '',
        );

        return $settings;
    }
}

if (!function_exists('admin_db_connection')) {
    function admin_db_connection()
    {
        if (!class_exists('mysqli')) {
            return null;
        }

        $settings = backend_connection_settings();
        if (empty($settings['database'])) {
            return null;
        }

        $mysqli = @new mysqli(
            $settings['host'],
            $settings['username'],
            $settings['password'],
            $settings['database'],
            (int) $settings['port']
        );

        if ($mysqli->connect_errno) {
            return null;
        }

        $mysqli->set_charset('utf8mb4');
        return $mysqli;
    }
}

if (!function_exists('admin_ensure_store_request_table')) {
    function admin_ensure_store_request_table($conn)
    {
        if (!$conn instanceof mysqli) {
            return false;
        }

        $sql = "
            CREATE TABLE IF NOT EXISTS yeucaugianhang (
                idYeuCau INT NOT NULL AUTO_INCREMENT,
                loaiYeuCau ENUM('them_gian_hang') NOT NULL DEFAULT 'them_gian_hang',
                idChuQuanLy INT NOT NULL,
                tenGianHang VARCHAR(150) NOT NULL,
                diaChi VARCHAR(255) DEFAULT NULL,
                moTa TEXT DEFAULT NULL,
                ngonNguMoTa VARCHAR(10) NOT NULL DEFAULT 'vi',
                lat DECIMAL(10,7) DEFAULT NULL,
                lon DECIMAL(10,7) DEFAULT NULL,
                phiHangThang DECIMAL(12,2) NOT NULL DEFAULT 0.00,
                tinhTrangDeXuat ENUM('dang_hoat_dong','tam_ngung','dong_cua') NOT NULL DEFAULT 'dang_hoat_dong',
                trangThaiYeuCau ENUM('cho_duyet','da_duyet','tu_choi') NOT NULL DEFAULT 'cho_duyet',
                ghiChuXuLy TEXT DEFAULT NULL,
                idTaiKhoanXuLy INT DEFAULT NULL,
                idGianHang INT DEFAULT NULL,
                ngayTao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP(),
                thoiGianXuLy DATETIME DEFAULT NULL,
                PRIMARY KEY (idYeuCau),
                KEY idx_yeucaugianhang_owner (idChuQuanLy),
                KEY idx_yeucaugianhang_status (trangThaiYeuCau),
                KEY idx_yeucaugianhang_store (idGianHang),
                CONSTRAINT fk_yeucaugianhang_owner
                    FOREIGN KEY (idChuQuanLy) REFERENCES chu_quan_ly (idChuQuanLy)
                    ON DELETE CASCADE ON UPDATE CASCADE,
                CONSTRAINT fk_yeucaugianhang_reviewer
                    FOREIGN KEY (idTaiKhoanXuLy) REFERENCES taikhoan (idTaiKhoan)
                    ON DELETE SET NULL ON UPDATE CASCADE,
                CONSTRAINT fk_yeucaugianhang_store
                    FOREIGN KEY (idGianHang) REFERENCES gianhang (idGianHang)
                    ON DELETE SET NULL ON UPDATE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci
        ";

        return $conn->query($sql) === true;
    }
}
