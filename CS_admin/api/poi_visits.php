<?php
require_once dirname(__DIR__) . '/connect.php';

header('Content-Type: application/json');

$idGianHang = isset($_GET['id']) ? (int) $_GET['id'] : 0;
if ($idGianHang <= 0) {
    echo json_encode(array('error' => 'Invalid ID'));
    exit;
}

$conn = admin_db_connection();
if (!$conn) {
    echo json_encode(array('error' => 'Database connection failed'));
    exit;
}

$sql = "SELECT ngay, soLuot FROM luot_truy_cap_ngay WHERE idGianHang = ? AND ngay >= DATE_SUB(CURDATE(), INTERVAL 1 YEAR) ORDER BY ngay ASC";
$stmt = $conn->prepare($sql);
if (!$stmt) {
    echo json_encode(array('error' => $conn->error));
    exit;
}

$stmt->bind_param('i', $idGianHang);
$stmt->execute();
$result = $stmt->get_result();

$data = array();
while ($row = $result->fetch_assoc()) {
    $data[$row['ngay']] = (int) $row['soLuot'];
}

$stmt->close();
$conn->close();

echo json_encode($data);
