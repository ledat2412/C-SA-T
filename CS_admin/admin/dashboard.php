<main class="main-content">
    <section class="page-header">
      <h2>Dashboard Overview</h2>
      <p>Real-time performance and booth management metrics</p>
    </section>

    <section class="stats-grid">
      <div class="stat-card">
        <div class="stat-top">
          <div class="stat-icon">
            <i class="fa-solid fa-shop"></i>
          </div>
          <span class="stat-growth positive">+8.4% â†‘</span>
        </div>
        <p class="stat-label">Total Booths</p>
        <h3>128</h3>
      </div>

      <div class="stat-card">
        <div class="stat-top">
          <div class="stat-icon">
            <i class="fa-solid fa-users"></i>
          </div>
          <span class="stat-growth positive">+4.2% â†‘</span>
        </div>
        <p class="stat-label">Active Vendors</p>
        <h3>96</h3>
      </div>

      <div class="stat-card">
        <div class="stat-top">
          <div class="stat-icon">
            <i class="fa-solid fa-boxes-stacked"></i>
          </div>
          <span class="stat-growth positive">+11.3% â†‘</span>
        </div>
        <p class="stat-label">Total Products</p>
        <h3>2,430</h3>
      </div>

      <div class="stat-card">
        <div class="stat-top">
          <div class="stat-icon">
            <i class="fa-solid fa-wallet"></i>
          </div>
          <span class="stat-growth live">This Month</span>
        </div>
        <p class="stat-label">Platform Revenue</p>
        <h3>$52,300</h3>
      </div>
    </section>

    <section class="overview-grid">
      <div class="panel chart-panel">
        <div class="panel-header">
          <div>
            <h3>Booth Performance</h3>
            <p>Revenue trends for the last 7 days</p>
          </div>

          <button class="select-btn">
            Last 7 Days
            <i class="fa-solid fa-chevron-down"></i>
          </button>
        </div>

        <div class="chart-area">
          <div class="chart-grid-line line-1"></div>
          <div class="chart-grid-line line-2"></div>
          <div class="chart-grid-line line-3"></div>

          <svg viewBox="0 0 760 320" preserveAspectRatio="none" class="chart-svg">
            <defs>
              <linearGradient id="areaFill" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stop-color="#27d3d2" stop-opacity="0.28" />
                <stop offset="100%" stop-color="#27d3d2" stop-opacity="0.02" />
              </linearGradient>
            </defs>

            <path
              d="M 0 240
                 C 70 230, 100 205, 150 170
                 C 200 135, 255 130, 310 165
                 C 355 190, 410 215, 470 205
                 C 530 195, 565 140, 610 95
                 C 650 60, 710 58, 760 110
                 L 760 320 L 0 320 Z"
              fill="url(#areaFill)"
            ></path>

            <path
              d="M 0 240
                 C 70 230, 100 205, 150 170
                 C 200 135, 255 130, 310 165
                 C 355 190, 410 215, 470 205
                 C 530 195, 565 140, 610 95
                 C 650 60, 710 58, 760 110"
              fill="none"
              stroke="#18cfd0"
              stroke-width="4"
              stroke-linecap="round"
            ></path>
          </svg>

          <div class="chart-labels">
            <span>Mon</span>
            <span>Tue</span>
            <span>Wed</span>
            <span>Thu</span>
            <span>Fri</span>
            <span>Sat</span>
            <span>Sun</span>
          </div>
        </div>
      </div>

      <div class="panel rank-panel">
        <div class="panel-header simple">
          <div>
            <h3>Top Performing Booths</h3>
          </div>
        </div>

        <div class="rank-list">
          <div class="rank-item">
            <div class="rank-badge">1</div>
            <div class="rank-info">
              <h4>Booth A - Fashion</h4>
              <p>134 orders this week</p>
            </div>
            <strong>$4,200</strong>
          </div>

          <div class="rank-item">
            <div class="rank-badge">2</div>
            <div class="rank-info">
              <h4>Booth B - Food</h4>
              <p>118 orders this week</p>
            </div>
            <strong>$3,850</strong>
          </div>

          <div class="rank-item">
            <div class="rank-badge">3</div>
            <div class="rank-info">
              <h4>Booth C - Accessories</h4>
              <p>96 orders this week</p>
            </div>
            <strong>$2,900</strong>
          </div>

          <div class="rank-item">
            <div class="rank-badge">4</div>
            <div class="rank-info">
              <h4>Booth D - Drinks</h4>
              <p>81 orders this week</p>
            </div>
            <strong>$2,100</strong>
          </div>
        </div>

        <button class="outline-btn">View All Booths</button>
      </div>
    </section>

    <section class="panel table-panel">
      <div class="table-header">
        <h3>Recent Booth Activities</h3>
        <a href="#">Refresh List</a>
      </div>

      <div class="table-wrap">
        <table>
          <thead>
            <tr>
              <th>BOOTH ID</th>
              <th>VENDOR</th>
              <th>CATEGORY</th>
              <th>ACTIVITY</th>
              <th>REVENUE</th>
              <th>STATUS</th>
              <th>TIME</th>
            </tr>
          </thead>

          <tbody>
            <tr>
              <td class="booth-id">#BTH-1284</td>
              <td>
                <div class="vendor-cell">
                  <div class="avatar avatar-1">N</div>
                  <span>Nguyen Van A</span>
                </div>
              </td>
              <td>Fashion</td>
              <td>Added 12 new products</td>
              <td class="money">$320</td>
              <td><span class="status-badge success">ACTIVE</span></td>
              <td>2 mins ago</td>
            </tr>

            <tr>
              <td class="booth-id">#BTH-1283</td>
              <td>
                <div class="vendor-cell">
                  <div class="avatar avatar-2">T</div>
                  <span>Tran Thi B</span>
                </div>
              </td>
              <td>Food</td>
              <td>Updated booth information</td>
              <td class="money">$150</td>
              <td><span class="status-badge warning">PENDING</span></td>
              <td>10 mins ago</td>
            </tr>

            <tr>
              <td class="booth-id">#BTH-1282</td>
              <td>
                <div class="vendor-cell">
                  <div class="avatar avatar-3">L</div>
                  <span>Le Van C</span>
                </div>
              </td>
              <td>Accessories</td>
              <td>Received new booth booking</td>
              <td class="money">$520</td>
              <td><span class="status-badge info">RUNNING</span></td>
              <td>15 mins ago</td>
            </tr>

            <tr>
              <td class="booth-id">#BTH-1281</td>
              <td>
                <div class="vendor-cell">
                  <div class="avatar avatar-4">P</div>
                  <span>Pham Thi D</span>
                </div>
              </td>
              <td>Drinks</td>
              <td>Submitted booth renewal</td>
              <td class="money">$210</td>
              <td><span class="status-badge danger">CLOSED</span></td>
              <td>24 mins ago</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
</main>


