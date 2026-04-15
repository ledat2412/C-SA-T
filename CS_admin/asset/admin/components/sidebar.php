<?php
$sidebarActive = isset($sidebarActive) ? $sidebarActive : '';

if (!function_exists('sidebar_active_class')) {
  function sidebar_active_class($key, $active)
  {
    return $key === $active ? ' active' : '';
  }
}
?>
<div>
  <div class="brand">
    <div class="brand-icon">
      <i class="fa-solid fa-table-cells-large"></i>
    </div>
    <div class="brand-text">
      <h2>Hệ thống Admin</h2>
      <p>ADMINISTRATOR</p>
    </div>
  </div>

  <nav class="sidebar-nav">
    <a href="<?php echo htmlspecialchars(admin_url('index1st.php?usecase=dashboard'), ENT_QUOTES, 'UTF-8'); ?>" class="nav-item<?php echo sidebar_active_class('dashboard', $sidebarActive); ?>" data-sidebar-item="dashboard">
      <i class="fa-solid fa-chart-column"></i>
      <span>Tổng quan</span>
    </a>
    <a href="<?php echo htmlspecialchars(admin_url('index1st.php?usecase=store'), ENT_QUOTES, 'UTF-8'); ?>" class="nav-item<?php echo sidebar_active_class('store', $sidebarActive); ?>" data-sidebar-item="store">
      <i class="fa-solid fa-store"></i>
      <span>Gian hàng</span>
    </a>
    <a href="<?php echo htmlspecialchars(admin_url('index1st.php?usecase=account'), ENT_QUOTES, 'UTF-8'); ?>" class="nav-item<?php echo sidebar_active_class('account', $sidebarActive); ?>" data-sidebar-item="account">
      <i class="fa-solid fa-users"></i>
      <span>Khách hàng</span>
    </a>
    <a href="<?php echo htmlspecialchars(admin_url('index1st.php?usecase=report'), ENT_QUOTES, 'UTF-8'); ?>" class="nav-item<?php echo sidebar_active_class('report', $sidebarActive); ?>" data-sidebar-item="report">
      <i class="fa-solid fa-chart-simple"></i>
      <span>Báo cáo</span>
    </a>
  </nav>
</div>

<div>
  <div class="sidebar-divider"></div>
  <a href="<?php echo htmlspecialchars(admin_url('index1st.php?usecase=account'), ENT_QUOTES, 'UTF-8'); ?>" class="nav-item settings-link<?php echo sidebar_active_class('settings', $sidebarActive); ?>" data-sidebar-item="settings">
    <i class="fa-solid fa-gear"></i>
    <span>Settings</span>
  </a>
  <div class="profile-card">
    <div class="profile-left">
      <div class="profile-avatar">L</div>
      <div class="profile-info">
        <h4>Lan Hương</h4>
        <p>System Admin</p>
      </div>
    </div>
    <form class="logout-form" method="post" action="<?php echo htmlspecialchars(admin_url('logout.php'), ENT_QUOTES, 'UTF-8'); ?>">
      <button class="logout-btn" type="submit" aria-label="Đăng xuất">
        <i class="fa-solid fa-right-from-bracket"></i>
      </button>
    </form>
  </div>
</div>
