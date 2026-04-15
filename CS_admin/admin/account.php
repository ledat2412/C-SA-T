<main class="main-content">
  <section class="account-page">
    <div class="account-layout">
      <div class="account-left">
        <div class="page-head">
          <div>
            <h2>Danh sÃ¡ch TÃ i khoáº£n</h2>
            <p>Quáº£n lÃ½ Ä‘á»‹nh danh ngÆ°á»i dÃ¹ng vÃ  phÃ¢n cáº¥p vai trÃ² truy cáº­p há»‡ thá»‘ng.</p>
          </div>

          <div class="page-tabs">
            <button class="head-tab active">Táº¥t cáº£ (48)</button>
            <button class="head-tab">Hoáº¡t Ä‘á»™ng</button>
            <button class="head-tab">Bá»‹ khÃ³a</button>
          </div>
        </div>

        <div class="panel account-table-panel">
          <div class="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>TÃŠN NGÆ¯á»œI DÃ™NG</th>
                  <th>EMAIL</th>
                  <th>VAI TRÃ’</th>
                  <th>NGÃ€Y ÄÄ‚NG KÃ</th>
                  <th>TÃŒNH TRáº NG</th>
                </tr>
              </thead>

              <tbody>
                <tr>
                  <td class="username">nguyenvan_a</td>
                  <td>a.nguyen@email.com</td>
                  <td><span class="role-badge admin">ADMIN</span></td>
                  <td>20/10/2023</td>
                  <td>
                    <span class="status-text active">
                      <span class="mini-dot"></span>
                      HoÃ n táº¥t
                    </span>
                  </td>
                </tr>

                <tr>
                  <td class="username">tran_thi_b</td>
                  <td>b.tran@email.com</td>
                  <td><span class="role-badge manager">MANAGER</span></td>
                  <td>15/11/2023</td>
                  <td>
                    <span class="status-text active">
                      <span class="mini-dot"></span>
                      HoÃ n táº¥t
                    </span>
                  </td>
                </tr>

                <tr>
                  <td class="username">le_van_c</td>
                  <td>c.le@email.com</td>
                  <td><span class="role-badge staff">STAFF</span></td>
                  <td>01/12/2023</td>
                  <td>
                    <span class="status-text pending">
                      <span class="mini-dot"></span>
                      Chá» duyá»‡t
                    </span>
                  </td>
                </tr>

                <tr>
                  <td class="username">pham_d</td>
                  <td>d.pham@email.com</td>
                  <td><span class="role-badge staff">STAFF</span></td>
                  <td>05/12/2023</td>
                  <td>
                    <span class="status-text active">
                      <span class="mini-dot"></span>
                      HoÃ n táº¥t
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <div class="table-footer">
          <p>Hiá»ƒn thá»‹ 1-10 trÃªn 48 ngÆ°á»i dÃ¹ng</p>

          <div class="pagination">
            <button><i class="fa-solid fa-chevron-left"></i></button>
            <button class="active">1</button>
            <button>2</button>
            <button>3</button>
            <button><i class="fa-solid fa-chevron-right"></i></button>
          </div>
        </div>
      </div>

      <div class="account-right">
        <div class="panel permission-panel">
          <h3>PhÃ¢n quyá»n Vai trÃ²</h3>
          <p class="permission-desc">
            Chá»n ngÆ°á»i dÃ¹ng vÃ  gÃ¡n vai trÃ² tÆ°Æ¡ng á»©ng Ä‘á»ƒ phÃ¢n quyá»n há»‡ thá»‘ng.
          </p>

          <div class="section-label">NGÆ¯á»œI DÃ™NG ÄANG CHá»ŒN</div>
          <div class="selected-user">
            <div class="selected-avatar">LT</div>
            <div>
              <h4>le_van_c</h4>
              <p>NhÃ¢n viÃªn má»›i</p>
            </div>
          </div>

          <div class="section-label role-label">CHá»ŒN VAI TRÃ’ GÃN</div>

          <label class="role-option">
            <input type="radio" name="role">
            <span class="radio-ui"></span>
            <div class="role-option-content">
              <strong class="purple">Admin</strong>
              <p>ToÃ n quyá»n há»‡ thá»‘ng</p>
            </div>
          </label>

          <label class="role-option">
            <input type="radio" name="role">
            <span class="radio-ui"></span>
            <div class="role-option-content">
              <strong class="blue">Manager</strong>
              <p>Quáº£n lÃ½ Ä‘á»™i ngÅ© &amp; bÃ¡o cÃ¡o</p>
            </div>
          </label>

          <label class="role-option selected">
            <input type="radio" name="role" checked>
            <span class="radio-ui"></span>
            <div class="role-option-content">
              <strong class="dark">Staff</strong>
              <p>Truy cáº­p dá»¯ liá»‡u giá»›i háº¡n</p>
            </div>
          </label>

          <button class="update-btn">Cáº­p nháº­t Vai trÃ²</button>
        </div>

        <div class="role-chart-card">
          <h4>PhÃ¢n bá»‘ Vai trÃ²</h4>

          <div class="role-chart-item">
            <div class="chart-row">
              <span>Admin</span>
              <strong>4</strong>
            </div>
            <div class="progress-line purple-line">
              <span style="width: 15%;"></span>
            </div>
          </div>

          <div class="role-chart-item">
            <div class="chart-row">
              <span>Manager</span>
              <strong>12</strong>
            </div>
            <div class="progress-line blue-line">
              <span style="width: 45%;"></span>
            </div>
          </div>

          <div class="role-chart-item">
            <div class="chart-row">
              <span>Staff</span>
              <strong>32</strong>
            </div>
            <div class="progress-line cyan-line">
              <span style="width: 82%;"></span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</main>


