<?php
$auth = isset($_SESSION['admin_auth']) && is_array($_SESSION['admin_auth']) ? $_SESSION['admin_auth'] : array();
$idTaiKhoan = isset($auth['idTaiKhoan']) ? (int) $auth['idTaiKhoan'] : 0;
$selectedAccountId = isset($_GET['selected']) ? (int) $_GET['selected'] : 0;
$statusFilter = isset($_GET['status']) ? strtolower(trim((string) $_GET['status'])) : 'all';
$accountAction = isset($_GET['action']) ? strtolower(trim((string) $_GET['action'])) : '';
$flashMessage = isset($_GET['message']) ? (string) $_GET['message'] : '';
$flashNotice = isset($_GET['notice']) ? (string) $_GET['notice'] : '';

if (!in_array($statusFilter, array('all', 'active', 'locked'), true)) {
    $statusFilter = 'all';
}

function account_api_url($idTaiKhoan)
{
    return backend_api_url('Admin/accounts') . '?idTaiKhoan=' . rawurlencode((string) $idTaiKhoan);
}

function account_api_status_url($idTaiKhoan, $targetAccountId)
{
    return backend_api_url('Admin/accounts/' . rawurlencode((string) $targetAccountId) . '/status') . '?idTaiKhoan=' . rawurlencode((string) $idTaiKhoan);
}

function account_list_url($statusFilter, $selectedAccountId = 0, $message = '', $notice = '', $action = '')
{
    $params = array('usecase' => 'account');

    if ($statusFilter !== 'all') {
        $params['status'] = $statusFilter;
    }

    if ($selectedAccountId > 0) {
        $params['selected'] = $selectedAccountId;
    }

    if ($message !== '') {
        $params['message'] = $message;
    }

    if ($notice !== '') {
        $params['notice'] = $notice;
    }

    if ($action !== '') {
        $params['action'] = $action;
    }

    return admin_url('index1st.php?' . http_build_query($params));
}

function account_request_json($method, $url, $payload, &$error, &$httpCode = 0)
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
    $httpCode = (int) curl_getinfo($ch, CURLINFO_HTTP_CODE);
    $curlError = curl_error($ch);
    curl_close($ch);

    if ($body === false) {
        $error = $curlError !== '' ? $curlError : 'Không thể kết nối backend.';
        return null;
    }

    $decoded = json_decode($body, true);
    if ($httpCode >= 400) {
        if (is_array($decoded) && isset($decoded['message'])) {
            $error = (string) $decoded['message'];
        } else {
            $error = 'API trả về HTTP ' . $httpCode . '.';
        }

        return null;
    }

    if ($decoded === null && trim((string) $body) !== '') {
        $error = 'Phản hồi API không hợp lệ.';
        return null;
    }

    return $decoded;
}

function account_role_meta($role)
{
    $role = strtolower(trim((string) $role));
    if ($role === 'admin') {
        return array(
            'label' => 'Admin',
            'class' => 'admin',
            'description' => 'Toàn quyền hệ thống',
        );
    }

    return array(
        'label' => 'Chủ quản lý',
        'class' => 'owner',
        'description' => 'Quản lý gian hàng',
    );
}

function account_status_meta($status, $registerStatus)
{
    $status = strtolower(trim((string) $status));
    $registerStatus = strtolower(trim((string) $registerStatus));

    if ($status === 'khoa') {
        return array('label' => 'Đã khóa', 'class' => 'locked');
    }

    if ($registerStatus !== '' && $registerStatus !== 'da_duyet') {
        return array('label' => 'Chờ duyệt', 'class' => 'review');
    }

    return array('label' => 'Hoạt động', 'class' => 'active');
}

function account_initials($account)
{
    $source = '';
    if (!empty($account['hoTen'])) {
        $source = (string) $account['hoTen'];
    } elseif (!empty($account['username'])) {
        $source = (string) $account['username'];
    }

    $source = trim($source);
    if ($source === '') {
        return 'TK';
    }

    $parts = preg_split('/[\s_]+/u', $source);
    $letters = '';

    foreach ($parts as $part) {
        if ($part === '') {
            continue;
        }

        $letters .= function_exists('mb_substr') ? mb_substr($part, 0, 1, 'UTF-8') : substr($part, 0, 1);
        if ((function_exists('mb_strlen') ? mb_strlen($letters, 'UTF-8') : strlen($letters)) >= 2) {
            break;
        }
    }

    return function_exists('mb_strtoupper') ? mb_strtoupper($letters, 'UTF-8') : strtoupper($letters);
}

function account_display_name($account)
{
    if (!empty($account['hoTen'])) {
        return (string) $account['hoTen'];
    }

    if (!empty($account['username'])) {
        return (string) $account['username'];
    }

    return 'Tài khoản';
}

function account_format_datetime($value)
{
    if (empty($value)) {
        return 'Chưa có';
    }

    $timestamp = strtotime((string) $value);
    if ($timestamp === false) {
        return 'Chưa có';
    }

    return date('d/m/Y H:i', $timestamp);
}

function account_detail_id($account)
{
    if (!empty($account['idAdmin'])) {
        return 'Admin #' . (int) $account['idAdmin'];
    }

    if (!empty($account['idChuQuanLy'])) {
        return 'Chủ quản lý #' . (int) $account['idChuQuanLy'];
    }

    return 'Tài khoản #' . (int) ($account['idTaiKhoan'] ?? 0);
}

function account_role_options()
{
    return array(
        'admin' => 'Admin',
        'chu_quan_ly' => 'Chủ quản lý',
    );
}

function account_status_options()
{
    return array(
        'hoat_dong' => 'Hoạt động',
        'khoa' => 'Khóa',
    );
}

function account_create_defaults()
{
    return array(
        'username' => '',
        'hoTen' => '',
        'email' => '',
        'matKhau' => '',
        'loaiTaiKhoan' => 'chu_quan_ly',
        'tinhTrang' => 'hoat_dong',
        'idLienKet' => '',
    );
}

function account_validate_create_payload($values, &$error)
{
    $error = '';

    if ($values['username'] === '') {
        $error = 'Tài khoản đăng nhập không được để trống.';
        return false;
    }

    if ($values['hoTen'] === '') {
        $error = 'Tên người dùng không được để trống.';
        return false;
    }

    if ($values['email'] === '') {
        $error = 'Email không được để trống.';
        return false;
    }

    if ($values['matKhau'] === '') {
        $error = 'Mật khẩu không được để trống.';
        return false;
    }

    if (!filter_var($values['email'], FILTER_VALIDATE_EMAIL)) {
        $error = 'Email không đúng định dạng.';
        return false;
    }

    if (!array_key_exists($values['loaiTaiKhoan'], account_role_options())) {
        $error = 'Vai trò tài khoản không hợp lệ.';
        return false;
    }

    if (!array_key_exists($values['tinhTrang'], account_status_options())) {
        $error = 'Tình trạng tài khoản không hợp lệ.';
        return false;
    }

    if ($values['idLienKet'] !== '' && (!ctype_digit($values['idLienKet']) || (int) $values['idLienKet'] <= 0)) {
        $error = 'ID liên kết phải là số nguyên dương.';
        return false;
    }

    return true;
}

function account_fetch_from_database(&$error)
{
    $error = '';
    $conn = admin_db_connection();
    if (!$conn instanceof mysqli) {
        $error = 'Không thể mở kết nối DB fallback.';
        return array();
    }

    $sql = "
        SELECT
            tk.idTaiKhoan,
            tk.username,
            tk.email,
            tk.loaiTaiKhoan,
            tk.tinhTrang,
            tk.tinhTrangDangKy,
            tk.ngayTao,
            ad.idAdmin,
            cql.idChuQuanLy,
            COALESCE(ad.hoTen, cql.hoTen) AS hoTen
        FROM taikhoan tk
        LEFT JOIN admin ad ON ad.idTaiKhoan = tk.idTaiKhoan
        LEFT JOIN chu_quan_ly cql ON cql.idTaiKhoan = tk.idTaiKhoan
        WHERE tk.loaiTaiKhoan IN ('admin', 'chu_quan_ly')
        ORDER BY
            CASE tk.loaiTaiKhoan
                WHEN 'admin' THEN 0
                WHEN 'chu_quan_ly' THEN 1
                ELSE 2
            END,
            tk.ngayTao DESC,
            tk.idTaiKhoan";

    $result = $conn->query($sql);
    if (!$result) {
        $error = 'Không thể đọc danh sách tài khoản: ' . $conn->error;
        $conn->close();
        return array();
    }

    $items = array();
    while ($row = $result->fetch_assoc()) {
        $items[] = $row;
    }

    $result->free();
    $conn->close();
    return $items;
}

function account_fetch_one_from_database($idTaiKhoan, &$error)
{
    $error = '';
    $conn = admin_db_connection();
    if (!$conn instanceof mysqli) {
        $error = 'Không thể mở kết nối DB fallback.';
        return null;
    }

    $stmt = $conn->prepare("
        SELECT
            tk.idTaiKhoan,
            tk.username,
            tk.email,
            tk.loaiTaiKhoan,
            tk.tinhTrang,
            tk.tinhTrangDangKy,
            tk.ngayTao,
            ad.idAdmin,
            cql.idChuQuanLy,
            COALESCE(ad.hoTen, cql.hoTen) AS hoTen
        FROM taikhoan tk
        LEFT JOIN admin ad ON ad.idTaiKhoan = tk.idTaiKhoan
        LEFT JOIN chu_quan_ly cql ON cql.idTaiKhoan = tk.idTaiKhoan
        WHERE tk.idTaiKhoan = ?
        LIMIT 1
    ");
    if (!$stmt) {
        $error = $conn->error;
        $conn->close();
        return null;
    }

    $stmt->bind_param('i', $idTaiKhoan);
    $stmt->execute();
    $result = $stmt->get_result();
    $row = $result ? $result->fetch_assoc() : null;
    if ($result) {
        $result->free();
    }
    $stmt->close();
    $conn->close();
    return $row ?: null;
}

function account_db_value_exists($conn, $sql, $types, $params)
{
    $stmt = $conn->prepare($sql);
    if (!$stmt) {
        return false;
    }

    $stmt->bind_param($types, ...$params);
    $stmt->execute();
    $result = $stmt->get_result();
    $exists = $result && $result->fetch_row();
    if ($result) {
        $result->free();
    }
    $stmt->close();

    return (bool) $exists;
}

function account_create_in_database($values, &$error)
{
    $error = '';
    $conn = admin_db_connection();
    if (!$conn instanceof mysqli) {
        $error = 'Không thể mở kết nối DB fallback.';
        return null;
    }

    $username = $values['username'];
    $email = $values['email'];
    $hoTen = $values['hoTen'];
    $matKhau = $values['matKhau'];
    $loaiTaiKhoan = $values['loaiTaiKhoan'];
    $tinhTrang = $values['tinhTrang'];
    $idLienKet = $values['idLienKet'] !== '' ? (int) $values['idLienKet'] : null;

    if (account_db_value_exists($conn, "SELECT 1 FROM taikhoan WHERE email = ? LIMIT 1", 's', array($email))) {
        $error = 'Email đã tồn tại trong hệ thống.';
        $conn->close();
        return null;
    }

    if (account_db_value_exists($conn, "SELECT 1 FROM taikhoan WHERE username = ? LIMIT 1", 's', array($username))) {
        $error = 'Tài khoản đăng nhập đã tồn tại trong hệ thống.';
        $conn->close();
        return null;
    }

    if ($idLienKet !== null) {
        if ($loaiTaiKhoan === 'admin' && account_db_value_exists($conn, "SELECT 1 FROM admin WHERE idAdmin = ? LIMIT 1", 'i', array($idLienKet))) {
            $error = 'ID admin đã tồn tại trong hệ thống.';
            $conn->close();
            return null;
        }

        if ($loaiTaiKhoan === 'chu_quan_ly' && account_db_value_exists($conn, "SELECT 1 FROM chu_quan_ly WHERE idChuQuanLy = ? LIMIT 1", 'i', array($idLienKet))) {
            $error = 'ID chủ quản lý đã tồn tại trong hệ thống.';
            $conn->close();
            return null;
        }
    }

    $conn->begin_transaction();

    try {
        $stmt = $conn->prepare("
            INSERT INTO taikhoan (email, matKhau, username, loaiTaiKhoan, tinhTrang, tinhTrangDangKy)
            VALUES (?, ?, ?, ?, ?, 'da_duyet')
        ");
        if (!$stmt) {
            throw new Exception($conn->error);
        }

        $stmt->bind_param('sssss', $email, $matKhau, $username, $loaiTaiKhoan, $tinhTrang);
        $stmt->execute();
        $insertedAccountId = (int) $conn->insert_id;
        $stmt->close();

        if ($loaiTaiKhoan === 'admin') {
            if ($idLienKet !== null) {
                $stmt = $conn->prepare("INSERT INTO admin (idAdmin, idTaiKhoan, hoTen) VALUES (?, ?, ?)");
                if (!$stmt) {
                    throw new Exception($conn->error);
                }
                $stmt->bind_param('iis', $idLienKet, $insertedAccountId, $hoTen);
            } else {
                $stmt = $conn->prepare("INSERT INTO admin (idTaiKhoan, hoTen) VALUES (?, ?)");
                if (!$stmt) {
                    throw new Exception($conn->error);
                }
                $stmt->bind_param('is', $insertedAccountId, $hoTen);
            }
        } else {
            if ($idLienKet !== null) {
                $stmt = $conn->prepare("INSERT INTO chu_quan_ly (idChuQuanLy, idTaiKhoan, hoTen) VALUES (?, ?, ?)");
                if (!$stmt) {
                    throw new Exception($conn->error);
                }
                $stmt->bind_param('iis', $idLienKet, $insertedAccountId, $hoTen);
            } else {
                $stmt = $conn->prepare("INSERT INTO chu_quan_ly (idTaiKhoan, hoTen) VALUES (?, ?)");
                if (!$stmt) {
                    throw new Exception($conn->error);
                }
                $stmt->bind_param('is', $insertedAccountId, $hoTen);
            }
        }

        $stmt->execute();
        $stmt->close();
        $conn->commit();
        $conn->close();

        $lookupError = '';
        return account_fetch_one_from_database($insertedAccountId, $lookupError);
    } catch (Throwable $throwable) {
        $conn->rollback();
        $error = $throwable->getMessage();
        $conn->close();
        return null;
    }
}

function account_update_status_in_database($currentAdminAccountId, $targetAccountId, $targetStatus, &$error)
{
    $error = '';
    $normalizedStatus = strtolower(trim((string) $targetStatus));
    if (!array_key_exists($normalizedStatus, account_status_options())) {
        $error = 'Tình trạng tài khoản không hợp lệ.';
        return false;
    }

    if ($currentAdminAccountId > 0 && $currentAdminAccountId === $targetAccountId && $normalizedStatus === 'khoa') {
        $error = 'Không thể khóa chính tài khoản admin đang đăng nhập.';
        return false;
    }

    $conn = admin_db_connection();
    if (!$conn instanceof mysqli) {
        $error = 'Không thể mở kết nối DB fallback.';
        return false;
    }

    $accountStmt = $conn->prepare("
        SELECT tk.loaiTaiKhoan, cql.idChuQuanLy
        FROM taikhoan tk
        LEFT JOIN chu_quan_ly cql ON cql.idTaiKhoan = tk.idTaiKhoan
        WHERE tk.idTaiKhoan = ?
        LIMIT 1
    ");
    if (!$accountStmt) {
        $error = $conn->error;
        $conn->close();
        return false;
    }

    $accountStmt->bind_param('i', $targetAccountId);
    $accountStmt->execute();
    $accountResult = $accountStmt->get_result();
    $accountRow = $accountResult ? $accountResult->fetch_assoc() : null;
    if ($accountResult) {
        $accountResult->free();
    }
    $accountStmt->close();

    if (!is_array($accountRow)) {
        $error = 'Không tìm thấy tài khoản cần cập nhật.';
        $conn->close();
        return false;
    }

    $conn->begin_transaction();

    try {
        $updateStmt = $conn->prepare("UPDATE taikhoan SET tinhTrang = ? WHERE idTaiKhoan = ?");
        if (!$updateStmt) {
            throw new Exception($conn->error);
        }

        $updateStmt->bind_param('si', $normalizedStatus, $targetAccountId);
        $updateStmt->execute();
        $updateStmt->close();

        $affectedStores = 0;
        if (($accountRow['loaiTaiKhoan'] ?? '') === 'chu_quan_ly' && $normalizedStatus === 'khoa' && !empty($accountRow['idChuQuanLy'])) {
            $ownerId = (int) $accountRow['idChuQuanLy'];
            $storeStmt = $conn->prepare("
                UPDATE gianhang
                SET tinhTrang = CASE
                    WHEN tinhTrang = 'dong_cua' THEN 'dong_cua'
                    ELSE 'tam_ngung'
                END,
                thoiGianCapNhat = NOW()
                WHERE idChuQuanLy = ?
            ");
            if (!$storeStmt) {
                throw new Exception($conn->error);
            }

            $storeStmt->bind_param('i', $ownerId);
            $storeStmt->execute();
            $affectedStores = (int) $storeStmt->affected_rows;
            $storeStmt->close();
        }

        $conn->commit();
        $conn->close();

        if (($accountRow['loaiTaiKhoan'] ?? '') === 'chu_quan_ly' && $normalizedStatus === 'khoa') {
            return 'Cập nhật tình trạng tài khoản thành công. Đã tạm ngừng ' . $affectedStores . ' gian hàng của chủ quản lý này.';
        }

        return 'Cập nhật tình trạng tài khoản thành công.';
    } catch (Throwable $throwable) {
        $conn->rollback();
        $error = $throwable->getMessage();
        $conn->close();
        return false;
    }
}

$accountError = '';
$accountNotice = $flashNotice;
$accountMessage = $flashMessage;
$createValues = account_create_defaults();
$showCreatePanel = $accountAction === 'new';

if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['account_action']) && $_POST['account_action'] === 'create') {
    $showCreatePanel = true;
    $createValues = array(
        'username' => trim((string) ($_POST['username'] ?? '')),
        'hoTen' => trim((string) ($_POST['hoTen'] ?? '')),
        'email' => trim((string) ($_POST['email'] ?? '')),
        'matKhau' => (string) ($_POST['matKhau'] ?? ''),
        'loaiTaiKhoan' => trim((string) ($_POST['loaiTaiKhoan'] ?? 'chu_quan_ly')),
        'tinhTrang' => trim((string) ($_POST['tinhTrang'] ?? 'hoat_dong')),
        'idLienKet' => trim((string) ($_POST['idLienKet'] ?? '')),
    );

    if (account_validate_create_payload($createValues, $validationError)) {
        $payload = array(
            'username' => $createValues['username'],
            'email' => $createValues['email'],
            'matKhau' => $createValues['matKhau'],
            'hoTen' => $createValues['hoTen'],
            'loaiTaiKhoan' => $createValues['loaiTaiKhoan'],
            'tinhTrang' => $createValues['tinhTrang'],
            'idLienKet' => $createValues['idLienKet'] !== '' ? (int) $createValues['idLienKet'] : null,
        );

        $apiHttpCode = 0;
        $apiError = '';
        $createdAccount = $idTaiKhoan > 0 ? account_request_json('POST', account_api_url($idTaiKhoan), $payload, $apiError, $apiHttpCode) : null;

        if (!is_array($createdAccount)) {
            $fallbackError = '';
            $createdAccount = account_create_in_database($createValues, $fallbackError);
            if (is_array($createdAccount)) {
                header('Location: ' . account_list_url('all', (int) $createdAccount['idTaiKhoan'], 'Tạo tài khoản mới thành công.', 'Trang tài khoản đang dùng DB fallback vì API chưa sẵn sàng.'));
                exit;
            }

            $accountError = $apiError !== '' ? $apiError : $fallbackError;
            if ($apiError !== '' && $fallbackError !== '' && $fallbackError !== $apiError) {
                $accountError .= ' | ' . $fallbackError;
            }
        } else {
            header('Location: ' . account_list_url('all', (int) $createdAccount['idTaiKhoan'], 'Tạo tài khoản mới thành công.'));
            exit;
        }
    } else {
        $accountError = $validationError;
    }
}

if ($_SERVER['REQUEST_METHOD'] === 'POST' && isset($_POST['account_action']) && $_POST['account_action'] === 'update_status') {
    $targetAccountId = isset($_POST['targetAccountId']) ? (int) $_POST['targetAccountId'] : 0;
    $targetStatus = strtolower(trim((string) ($_POST['targetStatus'] ?? '')));

    if ($targetAccountId <= 0) {
        $accountError = 'Không xác định được tài khoản cần cập nhật.';
    } elseif ($idTaiKhoan > 0 && $targetAccountId === $idTaiKhoan && $targetStatus === 'khoa') {
        $accountError = 'Khong the khoa chinh tai khoan admin dang dang nhap. Hay dang nhap bang mot admin khac neu muon doi trang thai tai khoan nay.';
    } elseif (!array_key_exists($targetStatus, account_status_options())) {
        $accountError = 'Tình trạng tài khoản không hợp lệ.';
    } else {
        $apiHttpCode = 0;
        $apiError = '';
        $apiResult = $idTaiKhoan > 0
            ? account_request_json('PATCH', account_api_status_url($idTaiKhoan, $targetAccountId), array('tinhTrang' => $targetStatus), $apiError, $apiHttpCode)
            : null;

        if (!is_array($apiResult)) {
            $fallbackError = '';
            $fallbackMessage = account_update_status_in_database($idTaiKhoan, $targetAccountId, $targetStatus, $fallbackError);
            if ($fallbackMessage !== false) {
                $nextFilter = $targetStatus === 'khoa' ? 'locked' : 'active';
                header('Location: ' . account_list_url($nextFilter, $targetAccountId, $fallbackMessage, 'Trang tài khoản đang dùng DB fallback vì API chưa sẵn sàng.'));
                exit;
            }

            $accountError = $apiError !== '' ? $apiError : $fallbackError;
            if ($apiError !== '' && $fallbackError !== '' && $fallbackError !== $apiError) {
                $accountError .= ' | ' . $fallbackError;
            }
        } else {
            $nextFilter = $targetStatus === 'khoa' ? 'locked' : 'active';
            $successMessage = isset($apiResult['message']) && $apiResult['message'] !== '' ? (string) $apiResult['message'] : 'Cập nhật tình trạng tài khoản thành công.';
            header('Location: ' . account_list_url($nextFilter, $targetAccountId, $successMessage));
            exit;
        }
    }
}

$accountHttpCode = 0;
$accounts = $idTaiKhoan > 0 ? account_request_json('GET', account_api_url($idTaiKhoan), null, $accountError, $accountHttpCode) : array();

if (!is_array($accounts)) {
    $fallbackError = '';
    $fallbackAccounts = account_fetch_from_database($fallbackError);
    if (count($fallbackAccounts) > 0) {
        $accounts = $fallbackAccounts;
        if ($accountNotice === '') {
            $accountNotice = 'Trang tài khoản đang dùng dữ liệu trực tiếp từ DB vì API chưa sẵn sàng.';
        }
        if ($accountHttpCode < 400) {
            $accountError = '';
        }
    } elseif ($fallbackError !== '') {
        $accountError = trim($accountError . ' | ' . $fallbackError, ' |');
    } else {
        $accounts = array();
    }
}

$totalAccounts = count($accounts);
$activeCount = 0;
$lockedCount = 0;
$adminCount = 0;
$ownerCount = 0;

foreach ($accounts as $account) {
    if (!is_array($account)) {
        continue;
    }

    $statusMeta = account_status_meta($account['tinhTrang'] ?? '', $account['tinhTrangDangKy'] ?? '');
    if ($statusMeta['class'] === 'active') {
        $activeCount++;
    } elseif ($statusMeta['class'] === 'locked') {
        $lockedCount++;
    }

    if (($account['loaiTaiKhoan'] ?? '') === 'admin') {
        $adminCount++;
    } elseif (($account['loaiTaiKhoan'] ?? '') === 'chu_quan_ly') {
        $ownerCount++;
    }
}

$filteredAccounts = array();
foreach ($accounts as $account) {
    if (!is_array($account)) {
        continue;
    }

    $statusMeta = account_status_meta($account['tinhTrang'] ?? '', $account['tinhTrangDangKy'] ?? '');
    if ($statusFilter === 'all' || $statusMeta['class'] === $statusFilter) {
        $filteredAccounts[] = $account;
    }
}

$filteredCount = count($filteredAccounts);
$selectedAccount = null;
foreach ($filteredAccounts as $account) {
    if (isset($account['idTaiKhoan']) && (int) $account['idTaiKhoan'] === $selectedAccountId) {
        $selectedAccount = $account;
        break;
    }
}

if ($selectedAccount === null && $filteredCount > 0) {
    $selectedAccount = $filteredAccounts[0];
}
?>
<main class="main-content">
  <section class="account-page">
    <div class="account-layout">
      <div class="account-left">
        <div class="page-head">
          <div>
            <h2>Danh sách tài khoản</h2>
            <p>Chỉ hiển thị tài khoản Admin và Chủ quản lý hiện có trong hệ thống</p>
          </div>

          <div class="page-head-actions">
            <button class="primary-btn" type="button" onclick="window.location.href='<?php echo htmlspecialchars(account_list_url($statusFilter, $selectedAccountId, '', '', 'new'), ENT_QUOTES, 'UTF-8'); ?>'">
              <i class="fa-solid fa-plus"></i>
              <span>Thêm tài khoản mới</span>
            </button>

            <div class="page-tabs">
              <?php
              $tabs = array(
                  array('key' => 'all', 'label' => 'Tất cả', 'count' => $totalAccounts),
                  array('key' => 'active', 'label' => 'Hoạt động', 'count' => $activeCount),
                  array('key' => 'locked', 'label' => 'Đã khóa', 'count' => $lockedCount),
              );
              foreach ($tabs as $tab) {
                  $tabUrl = account_list_url($tab['key'], $selectedAccountId);
              ?>
              <a class="head-tab <?php echo $statusFilter === $tab['key'] ? 'active' : ''; ?>" href="<?php echo htmlspecialchars($tabUrl, ENT_QUOTES, 'UTF-8'); ?>">
                <?php echo htmlspecialchars($tab['label'], ENT_QUOTES, 'UTF-8'); ?> (<?php echo (int) $tab['count']; ?>)
              </a>
              <?php } ?>
            </div>
          </div>
        </div>

        <?php if ($accountError !== '') { ?>
        <div class="store-edit-alert error"><?php echo htmlspecialchars($accountError, ENT_QUOTES, 'UTF-8'); ?></div>
        <?php } ?>

        <?php if ($accountNotice !== '') { ?>
        <div class="store-edit-alert warning"><?php echo htmlspecialchars($accountNotice, ENT_QUOTES, 'UTF-8'); ?></div>
        <?php } ?>

        <?php if ($accountMessage !== '') { ?>
        <div class="store-edit-alert success"><?php echo htmlspecialchars($accountMessage, ENT_QUOTES, 'UTF-8'); ?></div>
        <?php } ?>

        <div class="panel account-table-panel">
          <div class="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>TÀI KHOẢN</th>
                  <th>TÊN NGƯỜI DÙNG</th>
                  <th>EMAIL</th>
                  <th>VAI TRÒ</th>
                  <th>NGÀY TẠO</th>
                  <th>TÌNH TRẠNG</th>
                </tr>
              </thead>
              <tbody>
                <?php if ($filteredCount === 0) { ?>
                <tr>
                  <td colspan="6" class="account-empty">Không có tài khoản phù hợp để hiển thị.</td>
                </tr>
                <?php } ?>

                <?php foreach ($filteredAccounts as $account) { ?>
                <?php
                $roleMeta = account_role_meta($account['loaiTaiKhoan'] ?? '');
                $statusMeta = account_status_meta($account['tinhTrang'] ?? '', $account['tinhTrangDangKy'] ?? '');
                $isSelected = $selectedAccount !== null
                    && isset($selectedAccount['idTaiKhoan'], $account['idTaiKhoan'])
                    && (int) $selectedAccount['idTaiKhoan'] === (int) $account['idTaiKhoan'];
                $selectUrl = account_list_url($statusFilter, (int) $account['idTaiKhoan']);
                ?>
                <tr class="<?php echo $isSelected ? 'account-row-active' : ''; ?>">
                  <td class="username">
                    <a class="account-link" href="<?php echo htmlspecialchars($selectUrl, ENT_QUOTES, 'UTF-8'); ?>">
                      <?php echo htmlspecialchars((string) $account['username'], ENT_QUOTES, 'UTF-8'); ?>
                    </a>
                  </td>
                  <td class="display-name"><?php echo htmlspecialchars(account_display_name($account), ENT_QUOTES, 'UTF-8'); ?></td>
                  <td><?php echo htmlspecialchars((string) $account['email'], ENT_QUOTES, 'UTF-8'); ?></td>
                  <td><span class="role-badge <?php echo htmlspecialchars($roleMeta['class'], ENT_QUOTES, 'UTF-8'); ?>"><?php echo htmlspecialchars($roleMeta['label'], ENT_QUOTES, 'UTF-8'); ?></span></td>
                  <td><?php echo htmlspecialchars(account_format_datetime($account['ngayTao'] ?? ''), ENT_QUOTES, 'UTF-8'); ?></td>
                  <td>
                    <span class="status-text <?php echo htmlspecialchars($statusMeta['class'], ENT_QUOTES, 'UTF-8'); ?>">
                      <span class="mini-dot"></span>
                      <?php echo htmlspecialchars($statusMeta['label'], ENT_QUOTES, 'UTF-8'); ?>
                    </span>
                  </td>
                </tr>
                <?php } ?>
              </tbody>
            </table>
          </div>
        </div>

        <div class="table-footer">
          <p>Hiển thị <?php echo $filteredCount; ?> tài khoản hợp lệ trong hệ thống</p>
          <div class="pagination">
            <button type="button" aria-label="Trước"><i class="fa-solid fa-chevron-left"></i></button>
            <button class="active" type="button">1</button>
            <button type="button" aria-label="Sau"><i class="fa-solid fa-chevron-right"></i></button>
          </div>
        </div>
      </div>

      <div class="account-right">
        <?php if ($showCreatePanel) { ?>
        <div class="panel permission-panel account-create-panel">
          <div class="account-create-hero">
            <div class="account-create-icon">
              <i class="fa-solid fa-user-plus"></i>
            </div>
            <div class="account-create-copy">
              <div class="account-create-head">
                <h3>Thêm tài khoản mới</h3>
                <a class="account-close-link" href="<?php echo htmlspecialchars(account_list_url($statusFilter, $selectedAccountId), ENT_QUOTES, 'UTF-8'); ?>">Đóng</a>
              </div>
              <p class="permission-desc">Tạo nhanh tài khoản Admin hoặc Chủ quản lý. Hệ thống sẽ kiểm tra trùng email, tài khoản đăng nhập và ID liên kết nếu bạn nhập thủ công.</p>
            </div>
          </div>

          <form method="post" class="account-form">
            <input type="hidden" name="account_action" value="create" />

            <label>
              <span>Tài khoản đăng nhập</span>
              <input type="text" name="username" placeholder="Ví dụ: nguyenvana" value="<?php echo htmlspecialchars($createValues['username'], ENT_QUOTES, 'UTF-8'); ?>" required />
            </label>

            <label>
              <span>Tên người dùng</span>
              <input type="text" name="hoTen" placeholder="Ví dụ: Nguyễn Văn A" value="<?php echo htmlspecialchars($createValues['hoTen'], ENT_QUOTES, 'UTF-8'); ?>" required />
            </label>

            <label>
              <span>Email</span>
              <input type="email" name="email" placeholder="name@example.com" value="<?php echo htmlspecialchars($createValues['email'], ENT_QUOTES, 'UTF-8'); ?>" required />
            </label>

            <label>
              <span>Mật khẩu</span>
              <input type="text" name="matKhau" placeholder="Nhập mật khẩu ban đầu" value="<?php echo htmlspecialchars($createValues['matKhau'], ENT_QUOTES, 'UTF-8'); ?>" required />
            </label>

            <div class="account-form-grid">
              <label>
                <span>Vai trò</span>
                <select name="loaiTaiKhoan">
                  <?php foreach (account_role_options() as $roleValue => $roleLabel) { ?>
                  <option value="<?php echo htmlspecialchars($roleValue, ENT_QUOTES, 'UTF-8'); ?>" <?php echo $createValues['loaiTaiKhoan'] === $roleValue ? 'selected' : ''; ?>>
                    <?php echo htmlspecialchars($roleLabel, ENT_QUOTES, 'UTF-8'); ?>
                  </option>
                  <?php } ?>
                </select>
              </label>

              <label>
                <span>Tình trạng</span>
                <select name="tinhTrang">
                  <?php foreach (account_status_options() as $statusValue => $statusLabel) { ?>
                  <option value="<?php echo htmlspecialchars($statusValue, ENT_QUOTES, 'UTF-8'); ?>" <?php echo $createValues['tinhTrang'] === $statusValue ? 'selected' : ''; ?>>
                    <?php echo htmlspecialchars($statusLabel, ENT_QUOTES, 'UTF-8'); ?>
                  </option>
                  <?php } ?>
                </select>
              </label>
            </div>

            <label>
              <span>ID liên kết</span>
              <input type="number" min="1" step="1" name="idLienKet" placeholder="Để trống nếu tự tăng" value="<?php echo htmlspecialchars($createValues['idLienKet'], ENT_QUOTES, 'UTF-8'); ?>" />
              <small>Để trống nếu muốn hệ thống tự tăng. Nếu nhập tay, backend sẽ kiểm tra trùng ID trước khi tạo.</small>
            </label>

            <button class="update-btn" type="submit">Tạo tài khoản</button>
          </form>
        </div>
        <?php } else { ?>
        <div class="panel permission-panel">
          <h3>Thông tin tài khoản</h3>
          <p class="permission-desc">Bảng bên trái chỉ hiển thị Admin và Chủ quản lý. Bấm vào từng dòng để xem chi tiết.</p>

          <?php if ($selectedAccount !== null) { ?>
          <?php
          $selectedRole = account_role_meta($selectedAccount['loaiTaiKhoan'] ?? '');
          $selectedStatus = account_status_meta($selectedAccount['tinhTrang'] ?? '', $selectedAccount['tinhTrangDangKy'] ?? '');
          $selectedRoleKey = strtolower(trim((string) ($selectedAccount['loaiTaiKhoan'] ?? '')));
          $isSelectedCurrentAdmin = $selectedRoleKey === 'admin' && (int) ($selectedAccount['idTaiKhoan'] ?? 0) === $idTaiKhoan;
          ?>
          <div class="section-label">TÀI KHOẢN ĐANG CHỌN</div>
          <div class="selected-user">
            <div class="selected-avatar"><?php echo htmlspecialchars(account_initials($selectedAccount), ENT_QUOTES, 'UTF-8'); ?></div>
            <div>
              <h4><?php echo htmlspecialchars((string) $selectedAccount['username'], ENT_QUOTES, 'UTF-8'); ?></h4>
              <p><?php echo htmlspecialchars(account_display_name($selectedAccount), ENT_QUOTES, 'UTF-8'); ?></p>
            </div>
          </div>

          <div class="section-label role-label">VAI TRÒ HỆ THỐNG</div>
          <div class="role-option selected static">
            <span class="radio-ui"></span>
            <div class="role-option-content">
              <strong class="<?php echo htmlspecialchars($selectedRole['class'], ENT_QUOTES, 'UTF-8'); ?>"><?php echo htmlspecialchars($selectedRole['label'], ENT_QUOTES, 'UTF-8'); ?></strong>
              <p><?php echo htmlspecialchars($selectedRole['description'], ENT_QUOTES, 'UTF-8'); ?></p>
            </div>
          </div>

          <div class="readonly-stack">
            <div class="readonly-item">
              <span>Email</span>
              <strong><?php echo htmlspecialchars((string) $selectedAccount['email'], ENT_QUOTES, 'UTF-8'); ?></strong>
            </div>
            <div class="readonly-item">
              <span>Tình trạng</span>
              <strong><?php echo htmlspecialchars($selectedStatus['label'], ENT_QUOTES, 'UTF-8'); ?></strong>
            </div>
            <div class="readonly-item">
              <span>Ngày tạo</span>
              <strong><?php echo htmlspecialchars(account_format_datetime($selectedAccount['ngayTao'] ?? ''), ENT_QUOTES, 'UTF-8'); ?></strong>
            </div>
            <div class="readonly-item">
              <span>Bản ghi liên kết</span>
              <strong><?php echo htmlspecialchars(account_detail_id($selectedAccount), ENT_QUOTES, 'UTF-8'); ?></strong>
            </div>
          </div>

          <div class="section-label role-label">CẬP NHẬT TÌNH TRẠNG</div>
          <form method="post" class="account-form account-status-form">
            <input type="hidden" name="account_action" value="update_status" />
            <input type="hidden" name="targetAccountId" value="<?php echo (int) $selectedAccount['idTaiKhoan']; ?>" />

            <label>
              <span>Tình trạng tài khoản</span>
              <select name="targetStatus">
                <?php foreach (account_status_options() as $statusValue => $statusLabel) { ?>
                <?php $disableStatusOption = $isSelectedCurrentAdmin && $statusValue === 'khoa'; ?>
                <option value="<?php echo htmlspecialchars($statusValue, ENT_QUOTES, 'UTF-8'); ?>" <?php echo (($selectedAccount['tinhTrang'] ?? '') === $statusValue) ? 'selected' : ''; ?> <?php echo $disableStatusOption ? 'disabled' : ''; ?>>
                  <?php echo htmlspecialchars($statusLabel, ENT_QUOTES, 'UTF-8'); ?>
                </option>
                <?php } ?>
              </select>
              <?php if (($selectedAccount['loaiTaiKhoan'] ?? '') === 'chu_quan_ly') { ?>
              <small>Nếu khóa chủ quản lý, toàn bộ gian hàng của chủ này sẽ tự chuyển sang trạng thái tạm ngừng.</small>
              <?php } elseif ((int) ($selectedAccount['idTaiKhoan'] ?? 0) === $idTaiKhoan) { ?>
              <small>Admin dang dang nhap khong the tu khoa. Hay dang nhap bang mot admin khac neu muon doi trang thai tai khoan nay.</small>
              <small>Bạn không thể khóa chính tài khoản admin đang đăng nhập.</small>
              <?php } ?>
            </label>

            <button class="update-btn" type="submit">Lưu tình trạng</button>
          </form>
          <?php } else { ?>
          <div class="account-empty-side">Không có tài khoản nào trong bộ lọc hiện tại để xem chi tiết.</div>
          <?php } ?>
        </div>

        <div class="role-chart-card">
          <h4>Phân bố vai trò</h4>

          <div class="role-chart-item">
            <div class="chart-row">
              <span>Admin</span>
              <strong><?php echo $adminCount; ?></strong>
            </div>
            <div class="progress-line purple-line">
              <span style="width: <?php echo $totalAccounts > 0 ? round(($adminCount / $totalAccounts) * 100, 2) : 0; ?>%;"></span>
            </div>
          </div>

          <div class="role-chart-item">
            <div class="chart-row">
              <span>Chủ quản lý</span>
              <strong><?php echo $ownerCount; ?></strong>
            </div>
            <div class="progress-line cyan-line">
              <span style="width: <?php echo $totalAccounts > 0 ? round(($ownerCount / $totalAccounts) * 100, 2) : 0; ?>%;"></span>
            </div>
          </div>

          <p class="role-summary-note">Trang này không hiển thị tài khoản khách hàng.</p>
        </div>
        <?php } ?>
      </div>
    </div>
  </section>
</main>
