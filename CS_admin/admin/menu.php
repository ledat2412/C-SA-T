<?php
$auth = isset($_SESSION['admin_auth']) && is_array($_SESSION['admin_auth']) ? $_SESSION['admin_auth'] : array();
$idTaiKhoan = isset($auth['idTaiKhoan']) ? (int) $auth['idTaiKhoan'] : 0;
$loaiTaiKhoan = isset($auth['loaiTaiKhoan']) ? (string) $auth['loaiTaiKhoan'] : 'admin';
$idGianHang = isset($_GET['idGianHang']) ? (int) $_GET['idGianHang'] : 0;
$selectedFoodId = isset($_GET['foodId']) ? (int) $_GET['foodId'] : 0;
$isCreateFoodMode = isset($_GET['mode']) && $_GET['mode'] === 'create';
$pageMessage = null;
$pageError = '';

function menu_page_call_json($method, $url, $payload, &$error, &$httpCode = 0)
{
    $error = '';
    $httpCode = 0;

    $ch = curl_init($url);
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
    curl_setopt($ch, CURLOPT_CUSTOMREQUEST, strtoupper($method));
    curl_setopt($ch, CURLOPT_HTTPHEADER, array('Accept: application/json', 'Content-Type: application/json'));
    curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
    curl_setopt($ch, CURLOPT_SSL_VERIFYHOST, false);

    if ($payload !== null) {
        curl_setopt($ch, CURLOPT_POSTFIELDS, json_encode($payload, JSON_UNESCAPED_UNICODE));
    }

    $body = curl_exec($ch);
    if ($body === false) {
        $error = curl_error($ch) !== '' ? curl_error($ch) : 'Không thể kết nối backend.';
        curl_close($ch);
        return null;
    }

    $httpCode = (int) curl_getinfo($ch, CURLINFO_HTTP_CODE);
    curl_close($ch);

    $decoded = $body !== '' ? json_decode($body, true) : array();
    if ($httpCode >= 400) {
        if (is_array($decoded) && !empty($decoded['message'])) {
            $error = (string) $decoded['message'];
        } else {
            $error = 'API trả về HTTP ' . $httpCode . '.';
        }
        return null;
    }

    if ($decoded === null && trim((string) $body) !== '') {
        $error = 'Phản hồi từ backend không hợp lệ.';
        return null;
    }

    return is_array($decoded) ? $decoded : array();
}

function menu_page_store_detail_url($role, $idTaiKhoan, $idGianHang)
{
    $endpoint = $role === 'chu_quan_ly' ? 'Owner/stores/' : 'Admin/stores/';
    return backend_api_url($endpoint . rawurlencode((string) $idGianHang)) . '?idTaiKhoan=' . rawurlencode((string) $idTaiKhoan) . '&lang=vi';
}

function menu_page_foods_url($role, $idTaiKhoan, $idGianHang)
{
    $endpoint = $role === 'chu_quan_ly' ? 'Owner/stores/' : 'Admin/stores/';
    return backend_api_url($endpoint . rawurlencode((string) $idGianHang) . '/foods') . '?idTaiKhoan=' . rawurlencode((string) $idTaiKhoan);
}

function menu_page_food_collection_url($role, $idTaiKhoan)
{
    $endpoint = $role === 'chu_quan_ly' ? 'Owner/foods' : 'Admin/foods';
    return backend_api_url($endpoint) . '?idTaiKhoan=' . rawurlencode((string) $idTaiKhoan);
}

function menu_page_food_detail_url($role, $idTaiKhoan, $idMonAn)
{
    $endpoint = $role === 'chu_quan_ly' ? 'Owner/foods/' : 'Admin/foods/';
    return backend_api_url($endpoint . rawurlencode((string) $idMonAn)) . '?idTaiKhoan=' . rawurlencode((string) $idTaiKhoan);
}

function menu_page_food_image_url($role, $idTaiKhoan, $idMonAn)
{
    $endpoint = $role === 'chu_quan_ly' ? 'Owner/foods/' : 'Admin/foods/';
    return backend_api_url($endpoint . rawurlencode((string) $idMonAn) . '/image') . '?idTaiKhoan=' . rawurlencode((string) $idTaiKhoan);
}

function menu_page_proxy_image_url($path)
{
    $path = trim((string) $path);
    if ($path === '') {
        return '';
    }

    return admin_url('api/image-proxy.php') . '?path=' . rawurlencode($path);
}

function menu_page_call_file_upload($url, $fieldName, $fileInfo, &$error, &$httpCode = 0)
{
    $error = '';
    $httpCode = 0;

    if (!is_array($fileInfo) || empty($fileInfo['tmp_name']) || !is_file($fileInfo['tmp_name'])) {
        $error = 'Không tìm thấy file tạm để tải lên.';
        return null;
    }

    $mimeType = !empty($fileInfo['type']) ? (string) $fileInfo['type'] : 'application/octet-stream';
    $fileName = !empty($fileInfo['name']) ? (string) $fileInfo['name'] : 'food-image';

    $payload = array(
        $fieldName => new CURLFile($fileInfo['tmp_name'], $mimeType, $fileName),
    );

    $ch = curl_init($url);
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
    curl_setopt($ch, CURLOPT_POST, true);
    curl_setopt($ch, CURLOPT_HTTPHEADER, array('Accept: application/json'));
    curl_setopt($ch, CURLOPT_POSTFIELDS, $payload);
    curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
    curl_setopt($ch, CURLOPT_SSL_VERIFYHOST, false);

    $body = curl_exec($ch);
    if ($body === false) {
        $error = curl_error($ch) !== '' ? curl_error($ch) : 'Không thể tải ảnh món ăn lên backend.';
        curl_close($ch);
        return null;
    }

    $httpCode = (int) curl_getinfo($ch, CURLINFO_HTTP_CODE);
    curl_close($ch);

    $decoded = $body !== '' ? json_decode($body, true) : array();
    if ($httpCode >= 400) {
        if (is_array($decoded) && !empty($decoded['message'])) {
            $error = (string) $decoded['message'];
        } else {
            $error = 'API trả về HTTP ' . $httpCode . '.';
        }
        return null;
    }

    if ($decoded === null && trim((string) $body) !== '') {
        $error = 'Phản hồi tải ảnh món ăn không hợp lệ.';
        return null;
    }

    return is_array($decoded) ? $decoded : array();
}

function menu_page_redirect_url($idGianHang, $selectedFoodId = 0, $flash = '', $createMode = false)
{
    $params = array(
        'usecase' => 'menu',
        'idGianHang' => (int) $idGianHang,
    );

    if ($selectedFoodId > 0) {
        $params['foodId'] = (int) $selectedFoodId;
    }
    if ($flash !== '') {
        $params['flash'] = $flash;
    }
    if ($createMode) {
        $params['mode'] = 'create';
    }

    return admin_url('index1st.php?' . http_build_query($params));
}

function menu_page_status_options()
{
    return array(
        'con_ban' => 'Còn bán',
        'het_mon' => 'Hết món',
        'ngung_ban' => 'Ngừng bán',
    );
}

function menu_page_status_meta($status)
{
    $status = strtolower(trim((string) $status));
    $map = array(
        'con_ban' => array('label' => 'Còn bán', 'class' => 'available'),
        'het_mon' => array('label' => 'Hết món', 'class' => 'out'),
        'ngung_ban' => array('label' => 'Ngừng bán', 'class' => 'paused'),
    );

    return isset($map[$status]) ? $map[$status] : $map['con_ban'];
}

function menu_page_format_money($value)
{
    return number_format((float) $value, 0, ',', '.') . ' đ';
}

function menu_page_db_get_store($idGianHang, $idTaiKhoan, $role)
{
    $conn = admin_db_connection();
    if (!$conn instanceof mysqli) {
        return null;
    }

    $sql = "
        SELECT
            gh.idGianHang,
            gh.ten,
            gh.tinhTrang,
            gh.phiHangThang,
            gh.thoiGianCapNhat
        FROM gianhang gh
        INNER JOIN chu_quan_ly cql ON cql.idChuQuanLy = gh.idChuQuanLy
        INNER JOIN taikhoan tk ON tk.idTaiKhoan = cql.idTaiKhoan
        WHERE gh.idGianHang = ?
    ";
    if ($role === 'chu_quan_ly') {
        $sql .= " AND tk.idTaiKhoan = ?";
    }
    $sql .= " LIMIT 1";

    $stmt = $conn->prepare($sql);
    if (!$stmt) {
        $conn->close();
        return null;
    }

    if ($role === 'chu_quan_ly') {
        $stmt->bind_param('ii', $idGianHang, $idTaiKhoan);
    } else {
        $stmt->bind_param('i', $idGianHang);
    }

    $stmt->execute();
    $result = $stmt->get_result();
    $row = $result ? $result->fetch_assoc() : null;
    if ($result) {
        $result->free();
    }
    $stmt->close();
    $conn->close();

    return is_array($row) ? $row : null;
}

function menu_page_db_get_foods($idGianHang, $idTaiKhoan, $role)
{
    $conn = admin_db_connection();
    if (!$conn instanceof mysqli) {
        return array();
    }

    $sql = "
        SELECT
            ma.idMonAn,
            ma.idGianHang,
            ma.ten,
            ma.donGia,
            ma.tinhTrang,
            (
                SELECT ham.duongDan
                FROM hinhanhmonan ham
                WHERE ham.idMonAn = ma.idMonAn
                ORDER BY ham.idHinhAnh
                LIMIT 1
            ) AS hinhAnh
        FROM monan ma
        INNER JOIN gianhang gh ON gh.idGianHang = ma.idGianHang
        INNER JOIN chu_quan_ly cql ON cql.idChuQuanLy = gh.idChuQuanLy
        INNER JOIN taikhoan tk ON tk.idTaiKhoan = cql.idTaiKhoan
        WHERE ma.idGianHang = ?
    ";
    if ($role === 'chu_quan_ly') {
        $sql .= " AND tk.idTaiKhoan = ?";
    }
    $sql .= " ORDER BY ma.idMonAn";

    $stmt = $conn->prepare($sql);
    if (!$stmt) {
        $conn->close();
        return array();
    }

    if ($role === 'chu_quan_ly') {
        $stmt->bind_param('ii', $idGianHang, $idTaiKhoan);
    } else {
        $stmt->bind_param('i', $idGianHang);
    }

    $stmt->execute();
    $result = $stmt->get_result();
    $foods = array();
    if ($result) {
        while ($row = $result->fetch_assoc()) {
            $foods[] = $row;
        }
        $result->free();
    }

    $stmt->close();
    $conn->close();
    return $foods;
}

function menu_page_db_save_food_image($idMonAn, $fileInfo, $idTaiKhoan, $role, &$error)
{
    $error = '';
    if (!is_array($fileInfo) || empty($fileInfo['tmp_name']) || !is_file($fileInfo['tmp_name'])) {
        $error = 'Không tìm thấy file ảnh món ăn hợp lệ.';
        return '';
    }

    $conn = admin_db_connection();
    if (!$conn instanceof mysqli) {
        $error = 'Không thể kết nối DB để lưu ảnh món ăn.';
        return '';
    }

    $sql = "
        SELECT ham.idHinhAnh, ham.duongDan
        FROM monan ma
        INNER JOIN gianhang gh ON gh.idGianHang = ma.idGianHang
        INNER JOIN chu_quan_ly cql ON cql.idChuQuanLy = gh.idChuQuanLy
        INNER JOIN taikhoan tk ON tk.idTaiKhoan = cql.idTaiKhoan
        LEFT JOIN hinhanhmonan ham ON ham.idMonAn = ma.idMonAn
        WHERE ma.idMonAn = ?
    ";
    if ($role === 'chu_quan_ly') {
        $sql .= " AND tk.idTaiKhoan = ?";
    }
    $sql .= " ORDER BY ham.idHinhAnh LIMIT 1";

    $stmt = $conn->prepare($sql);
    if (!$stmt) {
        $error = $conn->error;
        $conn->close();
        return '';
    }

    if ($role === 'chu_quan_ly') {
        $stmt->bind_param('ii', $idMonAn, $idTaiKhoan);
    } else {
        $stmt->bind_param('i', $idMonAn);
    }

    $stmt->execute();
    $result = $stmt->get_result();
    $row = $result ? $result->fetch_assoc() : null;
    if ($result) {
        $result->free();
    }
    $stmt->close();

    if (!is_array($row)) {
        $error = 'Bạn không có quyền cập nhật ảnh cho món ăn này.';
        $conn->close();
        return '';
    }

    $backendRoot = dirname(__DIR__, 2) . '/VinhKhanh/VinhKhanh/wwwroot';
    $targetDir = $backendRoot . '/images/foods';
    if (!is_dir($targetDir) && !mkdir($targetDir, 0777, true) && !is_dir($targetDir)) {
        $error = 'Không thể tạo thư mục lưu ảnh món ăn.';
        $conn->close();
        return '';
    }

    $extension = pathinfo((string) $fileInfo['name'], PATHINFO_EXTENSION);
    $extension = $extension !== '' ? '.' . strtolower($extension) : '.jpg';
    $fileName = 'food_' . (int) $idMonAn . '_' . gmdate('YmdHis') . '_' . mt_rand(100, 999) . $extension;
    $relativePath = 'images/foods/' . $fileName;
    $absolutePath = $targetDir . '/' . $fileName;

    if (!move_uploaded_file($fileInfo['tmp_name'], $absolutePath)) {
        $error = 'Không thể lưu file ảnh món ăn.';
        $conn->close();
        return '';
    }

    $existingImageId = isset($row['idHinhAnh']) ? (int) $row['idHinhAnh'] : 0;
    $existingPath = isset($row['duongDan']) ? (string) $row['duongDan'] : '';

    if ($existingImageId > 0) {
        $updateStmt = $conn->prepare("
            UPDATE hinhanhmonan
            SET duongDan = ?
            WHERE idHinhAnh = ?
        ");
        if (!$updateStmt) {
            $error = $conn->error;
            $conn->close();
            return '';
        }
        $updateStmt->bind_param('si', $relativePath, $existingImageId);
        $updateStmt->execute();
        $updateStmt->close();
    } else {
        $insertStmt = $conn->prepare("
            INSERT INTO hinhanhmonan (idMonAn, duongDan)
            VALUES (?, ?)
        ");
        if (!$insertStmt) {
            $error = $conn->error;
            $conn->close();
            return '';
        }
        $insertStmt->bind_param('is', $idMonAn, $relativePath);
        $insertStmt->execute();
        $insertStmt->close();
    }

    $conn->close();

    if ($existingPath !== '') {
        $oldAbsolutePath = $backendRoot . '/' . ltrim(str_replace('\\', '/', $existingPath), '/');
        if (is_file($oldAbsolutePath) && realpath(dirname($oldAbsolutePath)) === realpath($targetDir) && $oldAbsolutePath !== $absolutePath) {
            @unlink($oldAbsolutePath);
        }
    }

    return $relativePath;
}

function menu_page_db_create_food($payload, $idTaiKhoan, $role, &$error)
{
    $error = '';
    $store = menu_page_db_get_store((int) $payload['idGianHang'], $idTaiKhoan, $role);
    if (!is_array($store)) {
        $error = 'Bạn không có quyền với gian hàng này.';
        return 0;
    }

    $conn = admin_db_connection();
    if (!$conn instanceof mysqli) {
        $error = 'Không thể kết nối DB.';
        return 0;
    }

    $stmt = $conn->prepare("
        INSERT INTO monan (idGianHang, ten, donGia, thoiGianCapNhat, tinhTrang)
        VALUES (?, ?, ?, NOW(), ?)
    ");
    if (!$stmt) {
        $error = $conn->error;
        $conn->close();
        return 0;
    }

    $idStore = (int) $payload['idGianHang'];
    $ten = trim((string) $payload['ten']);
    $donGia = (float) $payload['donGia'];
    $tinhTrang = trim((string) $payload['tinhTrang']);
    $stmt->bind_param('isds', $idStore, $ten, $donGia, $tinhTrang);
    $stmt->execute();

    $newId = $stmt->errno === 0 ? (int) $conn->insert_id : 0;
    if ($stmt->errno !== 0) {
        $error = $stmt->error !== '' ? $stmt->error : 'Không thể thêm món ăn mới.';
    }

    $stmt->close();
    $conn->close();
    return $newId;
}

function menu_page_db_update_food($idMonAn, $payload, $idTaiKhoan, $role, &$error)
{
    $error = '';
    $conn = admin_db_connection();
    if (!$conn instanceof mysqli) {
        $error = 'Không thể kết nối DB.';
        return false;
    }

    $sql = "
        SELECT ma.idMonAn
        FROM monan ma
        INNER JOIN gianhang gh ON gh.idGianHang = ma.idGianHang
        INNER JOIN chu_quan_ly cql ON cql.idChuQuanLy = gh.idChuQuanLy
        INNER JOIN taikhoan tk ON tk.idTaiKhoan = cql.idTaiKhoan
        WHERE ma.idMonAn = ?
    ";
    if ($role === 'chu_quan_ly') {
        $sql .= " AND tk.idTaiKhoan = ?";
    }
    $sql .= " LIMIT 1";

    $checkStmt = $conn->prepare($sql);
    if (!$checkStmt) {
        $error = $conn->error;
        $conn->close();
        return false;
    }

    if ($role === 'chu_quan_ly') {
        $checkStmt->bind_param('ii', $idMonAn, $idTaiKhoan);
    } else {
        $checkStmt->bind_param('i', $idMonAn);
    }

    $checkStmt->execute();
    $checkResult = $checkStmt->get_result();
    $allowed = $checkResult ? $checkResult->fetch_assoc() : null;
    if ($checkResult) {
        $checkResult->free();
    }
    $checkStmt->close();

    if (!is_array($allowed)) {
        $error = 'Bạn không có quyền chỉnh sửa món ăn này.';
        $conn->close();
        return false;
    }

    $stmt = $conn->prepare("
        UPDATE monan
        SET ten = ?, donGia = ?, tinhTrang = ?, thoiGianCapNhat = NOW()
        WHERE idMonAn = ? AND idGianHang = ?
    ");
    if (!$stmt) {
        $error = $conn->error;
        $conn->close();
        return false;
    }

    $ten = trim((string) $payload['ten']);
    $donGia = (float) $payload['donGia'];
    $tinhTrang = trim((string) $payload['tinhTrang']);
    $idStore = (int) $payload['idGianHang'];
    $stmt->bind_param('sdsii', $ten, $donGia, $tinhTrang, $idMonAn, $idStore);
    $stmt->execute();

    $success = $stmt->errno === 0;
    if (!$success) {
        $error = $stmt->error !== '' ? $stmt->error : 'Không thể cập nhật món ăn.';
    }

    $stmt->close();
    $conn->close();
    return $success;
}

$flash = isset($_GET['flash']) ? (string) $_GET['flash'] : '';
if ($flash === 'created') {
    $pageMessage = array('type' => 'success', 'text' => 'Đã thêm món ăn mới cho gian hàng.');
} elseif ($flash === 'updated') {
    $pageMessage = array('type' => 'success', 'text' => 'Đã cập nhật món ăn thành công.');
}
if (isset($_GET['image']) && $_GET['image'] === 'failed') {
    $pageMessage = array('type' => 'error', 'text' => 'Món ăn đã được lưu nhưng ảnh chưa cập nhật được. Bạn có thể thử tải ảnh lại.');
}

$storeSummary = null;
$foods = array();
$selectedFood = null;
$formData = array(
    'idMonAn' => 0,
    'ten' => '',
    'donGia' => '',
    'tinhTrang' => 'con_ban',
    'hinhAnh' => '',
);

if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['food_form_submit'])) {
    $formData = array(
        'idMonAn' => isset($_POST['idMonAn']) ? (int) $_POST['idMonAn'] : 0,
        'ten' => trim((string) ($_POST['ten'] ?? '')),
        'donGia' => trim((string) ($_POST['donGia'] ?? '')),
        'tinhTrang' => trim((string) ($_POST['tinhTrang'] ?? 'con_ban')),
        'hinhAnh' => trim((string) ($_POST['currentHinhAnh'] ?? '')),
    );
    $uploadedImage = isset($_FILES['foodImage']) && is_array($_FILES['foodImage']) ? $_FILES['foodImage'] : null;
    $hasUploadedImage = $uploadedImage !== null && isset($uploadedImage['error']) && (int) $uploadedImage['error'] === UPLOAD_ERR_OK;

    if ($idGianHang <= 0) {
        $pageMessage = array('type' => 'error', 'text' => 'Không xác định được gian hàng để quản lý món ăn.');
    } elseif ($formData['ten'] === '') {
        $pageMessage = array('type' => 'error', 'text' => 'Tên món ăn không được để trống.');
    } elseif ($formData['donGia'] === '' || !is_numeric($formData['donGia']) || (float) $formData['donGia'] < 0) {
        $pageMessage = array('type' => 'error', 'text' => 'Đơn giá món ăn không hợp lệ.');
    } elseif (!isset(menu_page_status_options()[$formData['tinhTrang']])) {
        $pageMessage = array('type' => 'error', 'text' => 'Trạng thái món ăn không hợp lệ.');
    } else {
        $payload = array(
            'idGianHang' => $idGianHang,
            'ten' => $formData['ten'],
            'donGia' => (float) $formData['donGia'],
            'tinhTrang' => $formData['tinhTrang'],
        );

        if ($formData['idMonAn'] > 0) {
            $apiError = '';
            $apiHttpCode = 0;
            $updateResult = menu_page_call_json('PUT', menu_page_food_detail_url($loaiTaiKhoan, $idTaiKhoan, $formData['idMonAn']), $payload, $apiError, $apiHttpCode);

            if ($updateResult === null && ($apiHttpCode === 0 || $apiHttpCode === 404 || $apiHttpCode === 405)) {
                $fallbackError = '';
                if (menu_page_db_update_food($formData['idMonAn'], $payload, $idTaiKhoan, $loaiTaiKhoan, $fallbackError)) {
                    $imageFailed = false;
                    if ($hasUploadedImage) {
                        $imageError = '';
                        $imagePath = menu_page_db_save_food_image($formData['idMonAn'], $uploadedImage, $idTaiKhoan, $loaiTaiKhoan, $imageError);
                        $imageFailed = $imagePath === '';
                    }
                    header('Location: ' . menu_page_redirect_url($idGianHang, $formData['idMonAn'], 'updated') . ($imageFailed ? '&image=failed' : ''));
                    exit;
                }

                $pageMessage = array('type' => 'error', 'text' => $fallbackError !== '' ? $fallbackError : $apiError);
            } elseif ($updateResult === null) {
                $pageMessage = array('type' => 'error', 'text' => 'Cập nhật món ăn thất bại: ' . $apiError);
            } else {
                $redirectFoodId = isset($updateResult['idMonAn']) ? (int) $updateResult['idMonAn'] : $formData['idMonAn'];
                $imageFailed = false;
                if ($hasUploadedImage) {
                    $imageError = '';
                    $imageHttpCode = 0;
                    $imageResult = menu_page_call_file_upload(menu_page_food_image_url($loaiTaiKhoan, $idTaiKhoan, $redirectFoodId), 'image', $uploadedImage, $imageError, $imageHttpCode);
                    if ($imageResult === null && ($imageHttpCode === 0 || $imageHttpCode === 404 || $imageHttpCode === 405)) {
                        $fallbackImageError = '';
                        $fallbackImagePath = menu_page_db_save_food_image($redirectFoodId, $uploadedImage, $idTaiKhoan, $loaiTaiKhoan, $fallbackImageError);
                        $imageFailed = $fallbackImagePath === '';
                    } elseif ($imageResult === null) {
                        $imageFailed = true;
                    }
                }
                header('Location: ' . menu_page_redirect_url($idGianHang, $redirectFoodId, 'updated') . ($imageFailed ? '&image=failed' : ''));
                exit;
            }
        } else {
            $apiError = '';
            $apiHttpCode = 0;
            $createResult = menu_page_call_json('POST', menu_page_food_collection_url($loaiTaiKhoan, $idTaiKhoan), $payload, $apiError, $apiHttpCode);

            if ($createResult === null && ($apiHttpCode === 0 || $apiHttpCode === 404 || $apiHttpCode === 405)) {
                $fallbackError = '';
                $newFoodId = menu_page_db_create_food($payload, $idTaiKhoan, $loaiTaiKhoan, $fallbackError);
                if ($newFoodId > 0) {
                    $imageFailed = false;
                    if ($hasUploadedImage) {
                        $imageError = '';
                        $imagePath = menu_page_db_save_food_image($newFoodId, $uploadedImage, $idTaiKhoan, $loaiTaiKhoan, $imageError);
                        $imageFailed = $imagePath === '';
                    }
                    header('Location: ' . menu_page_redirect_url($idGianHang, $newFoodId, 'created') . ($imageFailed ? '&image=failed' : ''));
                    exit;
                }

                $pageMessage = array('type' => 'error', 'text' => $fallbackError !== '' ? $fallbackError : $apiError);
            } elseif ($createResult === null) {
                $pageMessage = array('type' => 'error', 'text' => 'Thêm món ăn thất bại: ' . $apiError);
            } else {
                $newFoodId = isset($createResult['idMonAn']) ? (int) $createResult['idMonAn'] : 0;
                $imageFailed = false;
                if ($hasUploadedImage && $newFoodId > 0) {
                    $imageError = '';
                    $imageHttpCode = 0;
                    $imageResult = menu_page_call_file_upload(menu_page_food_image_url($loaiTaiKhoan, $idTaiKhoan, $newFoodId), 'image', $uploadedImage, $imageError, $imageHttpCode);
                    if ($imageResult === null && ($imageHttpCode === 0 || $imageHttpCode === 404 || $imageHttpCode === 405)) {
                        $fallbackImageError = '';
                        $fallbackImagePath = menu_page_db_save_food_image($newFoodId, $uploadedImage, $idTaiKhoan, $loaiTaiKhoan, $fallbackImageError);
                        $imageFailed = $fallbackImagePath === '';
                    } elseif ($imageResult === null) {
                        $imageFailed = true;
                    }
                }
                header('Location: ' . menu_page_redirect_url($idGianHang, $newFoodId, 'created') . ($imageFailed ? '&image=failed' : ''));
                exit;
            }
        }
    }
}

if ($idGianHang <= 0) {
    $pageError = 'Không xác định được gian hàng cần quản lý món ăn.';
} else {
    $detailError = '';
    $detailHttpCode = 0;
    $storeSummary = menu_page_call_json('GET', menu_page_store_detail_url($loaiTaiKhoan, $idTaiKhoan, $idGianHang), null, $detailError, $detailHttpCode);
    if (!is_array($storeSummary)) {
        $storeSummary = menu_page_db_get_store($idGianHang, $idTaiKhoan, $loaiTaiKhoan);
    }

    if (!is_array($storeSummary)) {
        $pageError = $detailError !== '' ? $detailError : 'Không tải được thông tin gian hàng để quản lý món ăn.';
    } else {
        $foodsError = '';
        $foodsHttpCode = 0;
        $foods = menu_page_call_json('GET', menu_page_foods_url($loaiTaiKhoan, $idTaiKhoan, $idGianHang), null, $foodsError, $foodsHttpCode);
        if (!is_array($foods)) {
            $foods = menu_page_db_get_foods($idGianHang, $idTaiKhoan, $loaiTaiKhoan);
        } elseif (count($foods) > 0 && (!array_key_exists('hinhAnh', $foods[0]) || !array_key_exists('idMonAn', $foods[0]))) {
            $dbFoods = menu_page_db_get_foods($idGianHang, $idTaiKhoan, $loaiTaiKhoan);
            if (count($dbFoods) > 0) {
                $dbFoodsById = array();
                foreach ($dbFoods as $dbFood) {
                    $dbFoodsById[(int) ($dbFood['idMonAn'] ?? 0)] = $dbFood;
                }

                foreach ($foods as $index => $foodRow) {
                    $foodId = (int) ($foodRow['idMonAn'] ?? 0);
                    if ($foodId > 0 && isset($dbFoodsById[$foodId])) {
                        $foods[$index]['hinhAnh'] = $dbFoodsById[$foodId]['hinhAnh'] ?? '';
                    }
                }
            }
        }

        foreach ($foods as $food) {
            if ((int) ($food['idMonAn'] ?? 0) === $selectedFoodId) {
                $selectedFood = $food;
                break;
            }
        }

        if (!$isCreateFoodMode && $selectedFood === null && count($foods) > 0) {
            $selectedFood = $foods[0];
        }

        if ($_SERVER['REQUEST_METHOD'] !== 'POST' || ($pageMessage !== null && $pageMessage['type'] === 'success')) {
            if ($isCreateFoodMode || $selectedFood === null) {
                $formData = array(
                    'idMonAn' => 0,
                    'ten' => '',
                    'donGia' => '',
                    'tinhTrang' => 'con_ban',
                    'hinhAnh' => '',
                );
            } else {
                $formData = array(
                    'idMonAn' => (int) ($selectedFood['idMonAn'] ?? 0),
                    'ten' => (string) ($selectedFood['ten'] ?? ''),
                    'donGia' => isset($selectedFood['donGia']) ? (string) $selectedFood['donGia'] : '',
                    'tinhTrang' => (string) ($selectedFood['tinhTrang'] ?? 'con_ban'),
                    'hinhAnh' => isset($selectedFood['hinhAnh']) ? (string) $selectedFood['hinhAnh'] : '',
                );
            }
        }
    }
}

$activeCount = 0;
foreach ($foods as $foodRow) {
    if (($foodRow['tinhTrang'] ?? '') === 'con_ban') {
        $activeCount++;
    }
}

$storeDisplayName = isset($storeSummary['ten']) ? (string) $storeSummary['ten'] : 'Gian hàng';
$storeFeeLabel = isset($storeSummary['phiHangThang']) ? menu_page_format_money($storeSummary['phiHangThang']) : 'Chưa có';
$formTitle = $formData['idMonAn'] > 0 ? 'Chỉnh sửa món ăn' : 'Thêm món ăn mới';
$saveButtonLabel = $formData['idMonAn'] > 0 ? 'Lưu món ăn' : 'Tạo món ăn';
$currentFoodImageUrl = isset($formData['hinhAnh']) && $formData['hinhAnh'] !== '' ? menu_page_proxy_image_url($formData['hinhAnh']) : '';
?>
<main class="main-content">
  <section class="menu-management-page">
    <div class="page-head">
      <div>
        <h2>Quản lý món ăn</h2>
        <p>Chủ gian hàng có thể cập nhật tên món, đơn giá và trạng thái bán cho từng món trong gian hàng này.</p>
      </div>

      <div class="page-head-actions">
        <a class="secondary-btn link-btn" href="<?php echo htmlspecialchars(admin_url('index1st.php?usecase=branchdetail2&idGianHang=' . (int) $idGianHang), ENT_QUOTES, 'UTF-8'); ?>">Quay lại gian hàng</a>
        <?php if ($pageError === '') { ?>
        <a class="primary-btn link-btn" href="<?php echo htmlspecialchars(menu_page_redirect_url($idGianHang, 0, '', true), ENT_QUOTES, 'UTF-8'); ?>">
          <i class="fa-solid fa-plus"></i>
          <span>Thêm món ăn mới</span>
        </a>
        <?php } ?>
      </div>
    </div>

    <?php if ($pageMessage !== null) { ?>
    <div class="store-edit-alert <?php echo htmlspecialchars($pageMessage['type'], ENT_QUOTES, 'UTF-8'); ?>">
      <?php echo htmlspecialchars($pageMessage['text'], ENT_QUOTES, 'UTF-8'); ?>
    </div>
    <?php } ?>

    <?php if ($pageError !== '') { ?>
    <div class="store-edit-alert error">
      <?php echo htmlspecialchars($pageError, ENT_QUOTES, 'UTF-8'); ?>
    </div>
    <?php } else { ?>
    <div class="stats-row">
      <div class="panel stat-card">
        <p class="stat-label">Gian hàng đang quản lý</p>
        <h3><?php echo htmlspecialchars($storeDisplayName, ENT_QUOTES, 'UTF-8'); ?></h3>
      </div>

      <div class="panel stat-card">
        <p class="stat-label">Tổng số món</p>
        <h3 class="accent"><?php echo (int) count($foods); ?></h3>
      </div>

      <div class="panel stat-card">
        <p class="stat-label">Đang bán</p>
        <h3 class="success-text"><?php echo (int) $activeCount; ?></h3>
      </div>
    </div>

    <div class="menu-layout">
      <div class="panel list-panel">
        <div class="card-head">
          <h3><i class="fa-solid fa-list"></i> Danh sách món ăn</h3>
          <span class="table-note">Phí gian hàng: <?php echo htmlspecialchars($storeFeeLabel, ENT_QUOTES, 'UTF-8'); ?></span>
        </div>

        <?php if (count($foods) === 0) { ?>
        <div class="empty-state">
          <h4>Chưa có món ăn nào</h4>
          <p>Bạn có thể thêm món đầu tiên cho gian hàng này ngay từ khung bên phải.</p>
        </div>
        <?php } else { ?>
        <div class="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Ảnh</th>
                <th>Món ăn</th>
                <th>Đơn giá</th>
                <th>Trạng thái</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <?php foreach ($foods as $food) { ?>
              <?php $statusMeta = menu_page_status_meta($food['tinhTrang'] ?? 'con_ban'); ?>
              <tr class="<?php echo ((int) ($food['idMonAn'] ?? 0) === (int) $formData['idMonAn'] && $formData['idMonAn'] > 0) ? 'selected-row' : ''; ?>">
                <td>
                  <?php $foodImageUrl = !empty($food['hinhAnh']) ? menu_page_proxy_image_url((string) $food['hinhAnh']) : ''; ?>
                  <div class="food-thumb<?php echo $foodImageUrl !== '' ? ' has-image' : ''; ?>"<?php echo $foodImageUrl !== '' ? ' style="background-image:url(\'' . htmlspecialchars($foodImageUrl, ENT_QUOTES, 'UTF-8') . '\')"' : ''; ?>>
                    <?php if ($foodImageUrl === '') { ?>
                    <span><?php echo htmlspecialchars((string) strtoupper(substr((string) ($food['ten'] ?? 'M'), 0, 1)), ENT_QUOTES, 'UTF-8'); ?></span>
                    <?php } ?>
                  </div>
                </td>
                <td>
                  <div class="food-name-cell">
                    <strong><?php echo htmlspecialchars((string) ($food['ten'] ?? 'Món ăn'), ENT_QUOTES, 'UTF-8'); ?></strong>
                    <span>#<?php echo (int) ($food['idMonAn'] ?? 0); ?></span>
                  </div>
                </td>
                <td class="money"><?php echo htmlspecialchars(menu_page_format_money($food['donGia'] ?? 0), ENT_QUOTES, 'UTF-8'); ?></td>
                <td>
                  <span class="status-badge <?php echo htmlspecialchars($statusMeta['class'], ENT_QUOTES, 'UTF-8'); ?>">
                    <span class="mini-dot"></span>
                    <?php echo htmlspecialchars($statusMeta['label'], ENT_QUOTES, 'UTF-8'); ?>
                  </span>
                </td>
                <td class="action-col">
                  <a class="edit-link" href="<?php echo htmlspecialchars(menu_page_redirect_url($idGianHang, (int) ($food['idMonAn'] ?? 0)), ENT_QUOTES, 'UTF-8'); ?>">Chỉnh sửa</a>
                </td>
              </tr>
              <?php } ?>
            </tbody>
          </table>
        </div>
        <?php } ?>
      </div>

      <div class="panel form-panel">
        <div class="card-head">
          <h3><i class="fa-solid fa-bowl-food"></i> <?php echo htmlspecialchars($formTitle, ENT_QUOTES, 'UTF-8'); ?></h3>
          <?php if ($formData['idMonAn'] > 0) { ?>
          <a class="edit-link" href="<?php echo htmlspecialchars(menu_page_redirect_url($idGianHang, 0, '', true), ENT_QUOTES, 'UTF-8'); ?>">Tạo món mới</a>
          <?php } ?>
        </div>

        <form class="menu-form" method="post" enctype="multipart/form-data" action="<?php echo htmlspecialchars(menu_page_redirect_url($idGianHang, $formData['idMonAn'], '', $formData['idMonAn'] <= 0), ENT_QUOTES, 'UTF-8'); ?>">
          <input type="hidden" name="food_form_submit" value="1" />
          <input type="hidden" name="idMonAn" value="<?php echo (int) $formData['idMonAn']; ?>" />
          <input type="hidden" name="currentHinhAnh" value="<?php echo htmlspecialchars((string) $formData['hinhAnh'], ENT_QUOTES, 'UTF-8'); ?>" />

          <div class="food-image-panel">
            <div class="food-image-preview<?php echo $currentFoodImageUrl !== '' ? ' has-image' : ''; ?>"<?php echo $currentFoodImageUrl !== '' ? ' style="background-image:url(\'' . htmlspecialchars($currentFoodImageUrl, ENT_QUOTES, 'UTF-8') . '\')"' : ''; ?>>
              <?php if ($currentFoodImageUrl === '') { ?>
              <span>Chưa có ảnh món ăn</span>
              <?php } ?>
            </div>

            <label class="form-field">
              <span>Hình ảnh món ăn</span>
              <input type="file" name="foodImage" accept="image/*" />
              <small class="form-help">Bạn có thể thêm hoặc thay ảnh món ăn ngay khi lưu form này.</small>
            </label>
          </div>

          <label class="form-field">
            <span>Tên món ăn</span>
            <input type="text" name="ten" value="<?php echo htmlspecialchars($formData['ten'], ENT_QUOTES, 'UTF-8'); ?>" required />
          </label>

          <label class="form-field">
            <span>Đơn giá</span>
            <input type="number" min="0" step="1000" name="donGia" value="<?php echo htmlspecialchars($formData['donGia'], ENT_QUOTES, 'UTF-8'); ?>" required />
            <small class="form-help">Nhập giá bán theo đơn vị đồng. Chủ gian hàng được phép tự cập nhật giá món ăn.</small>
          </label>

          <label class="form-field">
            <span>Trạng thái</span>
            <select name="tinhTrang">
              <?php foreach (menu_page_status_options() as $statusValue => $statusLabel) { ?>
              <option value="<?php echo htmlspecialchars($statusValue, ENT_QUOTES, 'UTF-8'); ?>" <?php echo $formData['tinhTrang'] === $statusValue ? 'selected' : ''; ?>><?php echo htmlspecialchars($statusLabel, ENT_QUOTES, 'UTF-8'); ?></option>
              <?php } ?>
            </select>
          </label>

          <div class="form-actions">
            <button class="primary-btn" type="submit">
              <i class="fa-solid fa-floppy-disk"></i>
              <span><?php echo htmlspecialchars($saveButtonLabel, ENT_QUOTES, 'UTF-8'); ?></span>
            </button>
          </div>
        </form>

        <div class="menu-help">
          <h4>Gợi ý quản lý</h4>
          <p>Dùng trạng thái <strong>Còn bán</strong> khi món sẵn sàng phục vụ, chuyển sang <strong>Hết món</strong> nếu tạm hết trong ngày, hoặc <strong>Ngừng bán</strong> khi không muốn hiển thị nữa.</p>
        </div>
      </div>
    </div>
    <?php } ?>
  </section>
</main>
