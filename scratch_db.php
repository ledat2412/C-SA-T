<?php
require_once 'C:/xampp/htdocs/C-SA-T/CS_admin/connect.php';

$conn = admin_db_connection();
if ($conn) {
    // Check if column exists
    $result = $conn->query("SHOW COLUMNS FROM gianhang LIKE 'luotTruyCap'");
    if ($result && $result->num_rows == 0) {
        $conn->query("ALTER TABLE gianhang ADD COLUMN luotTruyCap INT NOT NULL DEFAULT 0 AFTER vongBo");
        echo "Column luotTruyCap added.\n";
        
        // Populate with random values for demonstration
        $conn->query("UPDATE gianhang SET luotTruyCap = FLOOR(RAND() * 500)");
        echo "Random data populated.\n";
    } else {
        echo "Column already exists.\n";
    }
    $conn->close();
} else {
    echo "DB connection failed.\n";
}
