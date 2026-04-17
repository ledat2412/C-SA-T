<?php
require_once dirname(__DIR__) . '/connect.php';
header('Content-Type: application/json; charset=utf-8');
session_start();

if (!isset($_SESSION['admin_auth']) || empty($_SESSION['admin_auth']['isLoggedIn'])) {
    http_response_code(401);
    echo json_encode(array('success' => false, 'message' => 'Unauthorized.'));
    exit;
}

$auth = $_SESSION['admin_auth'];
if (!isset($auth['loaiTaiKhoan']) || $auth['loaiTaiKhoan'] !== 'admin') {
    http_response_code(403);
    echo json_encode(array('success' => false, 'message' => 'Forbidden.'));
    exit;
}

$idTaiKhoan = isset($auth['idTaiKhoan']) ? (int) $auth['idTaiKhoan'] : 0;

$url = backend_api_url('Admin/devices') . '?idTaiKhoan=' . rawurlencode((string) $idTaiKhoan);
$ch = curl_init($url);
curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
curl_setopt($ch, CURLOPT_HTTPHEADER, array('Accept: application/json'));
curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
curl_setopt($ch, CURLOPT_SSL_VERIFYHOST, false);
curl_setopt($ch, CURLOPT_TIMEOUT, 8);

$body = curl_exec($ch);
$httpCode = (int) curl_getinfo($ch, CURLINFO_HTTP_CODE);
$curlError = curl_error($ch);
curl_close($ch);

if ($body === false || $httpCode >= 400) {
    // fallback: query DB directly
    $conn = admin_db_connection();
    if (!$conn instanceof mysqli) {
        http_response_code(503);
        echo json_encode(array('success' => false, 'message' => 'Backend không khả dụng.'));
        exit;
    }

    $sql = "
        SELECT
            tb.idThietBi, tb.maThietBi, tb.daKichHoat,
            tb.thoiGianKichHoat, tb.lanCuoiHoatDong, tb.trangThai,
            tb.idTaiKhoan,
            COALESCE(ad.hoTen, cql.hoTen) AS tenChuSoHuu,
            tk.email AS emailChuSoHuu
        FROM thietbi tb
        LEFT JOIN taikhoan tk ON tk.idTaiKhoan = tb.idTaiKhoan
        LEFT JOIN admin ad ON ad.idTaiKhoan = tk.idTaiKhoan
        LEFT JOIN chu_quan_ly cql ON cql.idTaiKhoan = tk.idTaiKhoan
        ORDER BY tb.lanCuoiHoatDong DESC, tb.idThietBi DESC";

    $result = $conn->query($sql);
    $items = array();
    if ($result) {
        while ($row = $result->fetch_assoc()) {
            $items[] = $row;
        }
        $result->free();
    }
    $conn->close();

    echo json_encode($items, JSON_UNESCAPED_UNICODE);
    exit;
}

echo $body;
