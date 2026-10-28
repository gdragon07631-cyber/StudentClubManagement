import { Roles } from '../api.js';
import { fmtNumber } from '../ui.js';

const isIn = (user, role) => !!user?.roles?.includes(role);

export function register(router) {
  router.add('/reports', {
    nav: 'reports', title: 'Thống kê & Báo cáo', roles: [Roles.Admin, Roles.ClubLeader],
    async render({ view, user, api }) {
      const m = await api.get('api/reports');
      const isAdmin = isIn(user, Roles.Admin);
      const members = Number(m.members) || 0, activities = Number(m.activities) || 0, tasks = Number(m.tasks) || 0;
      const totalPoints = Number(m.totalPoints) || 0, income = Number(m.income) || 0, expense = Number(m.expense) || 0;
      const maxMetric = Math.max(1, members, activities, tasks);
      const totalFlow = income + expense;
      const incomePercent = totalFlow > 0 ? Math.round((income / totalFlow) * 1000) / 10 : 0;
      const expensePercent = totalFlow > 0 ? Math.round((expense / totalFlow) * 1000) / 10 : 0;
      const netBalance = income - expense;
      const avgPoints = members > 0 ? (totalPoints / members).toLocaleString('vi-VN', { minimumFractionDigits: 1, maximumFractionDigits: 1 }) : '0';

      view.innerHTML = `<header class="page-header">
  <div>
    <div class="page-eyebrow">Dữ liệu phân tích hệ thống</div>
    <h1 class="page-title">${isAdmin ? 'Thống kê & Báo cáo toàn hệ thống' : 'Thống kê câu lạc bộ'}</h1>
    <p class="page-subtitle">Tổng hợp chỉ số vận hành, nhân sự, hoạt động, nhiệm vụ và cân đối quỹ theo thời gian thực.</p>
  </div>
</header>
<section class="metric-grid">
  <div class="metric-card surface"><span class="metric-icon"><svg><use href="#i-users"/></svg></span><div class="metric-label">Thành viên</div><div class="metric-value">${members} <small>người</small></div><div class="metric-note success">Trong phạm vi quản lý</div></div>
  <div class="metric-card surface"><span class="metric-icon"><svg><use href="#i-calendar"/></svg></span><div class="metric-label">Hoạt động &amp; Sự kiện</div><div class="metric-value">${activities} <small>hoạt động</small></div><div class="metric-note">Toàn bộ kế hoạch</div></div>
  <div class="metric-card surface"><span class="metric-icon"><svg><use href="#i-clipboard"/></svg></span><div class="metric-label">Nhiệm vụ</div><div class="metric-value">${tasks} <small>công việc</small></div><div class="metric-note">Đã thiết lập trong CLB</div></div>
  <div class="metric-card surface"><span class="metric-icon"><svg><use href="#i-star"/></svg></span><div class="metric-label">Tổng điểm hoạt động</div><div class="metric-value">${fmtNumber(totalPoints)} <small>điểm</small></div><div class="metric-note">Trung bình: ~${avgPoints} điểm/thành viên</div></div>
</section>
${isAdmin ? `<section class="row g-3 mb-4">
  <div class="col-md-4"><div class="surface p-4"><div class="d-flex align-items-center gap-3"><span class="metric-icon" style="background:#eaf8ef; color:#0e8a4a; margin-bottom:0;"><svg><use href="#i-wallet"/></svg></span><div><div class="metric-label text-success">Tổng Thu</div><div class="h3 mb-0 text-success">${fmtNumber(income)} đ</div></div></div></div></div>
  <div class="col-md-4"><div class="surface p-4"><div class="d-flex align-items-center gap-3"><span class="metric-icon" style="background:#fdeeee; color:#d92d20; margin-bottom:0;"><svg><use href="#i-wallet"/></svg></span><div><div class="metric-label text-danger">Tổng Chi</div><div class="h3 mb-0 text-danger">${fmtNumber(expense)} đ</div></div></div></div></div>
  <div class="col-md-4"><div class="surface p-4"><div class="d-flex align-items-center gap-3"><span class="metric-icon" style="background:#eef4ff; color:#175cd3; margin-bottom:0;"><svg><use href="#i-chart"/></svg></span><div><div class="metric-label text-primary">Số Dư Ròng</div><div class="h3 mb-0 text-primary">${fmtNumber(netBalance)} đ</div></div></div></div></div>
</section>` : ''}
<div class="row g-4">
  <div class="col-lg-6">
    <section class="surface section-card p-4 h-100">
      <div class="section-heading no-border pt-0 px-0"><div><h2>Biểu đồ quy mô vận hành</h2><p>So sánh số lượng tương đối giữa các chỉ số trọng tâm</p></div></div>
      <div class="mt-3">
        <div class="mb-3"><div class="d-flex justify-content-between mb-1 small fw-bold"><span><svg style="width:14px; height:14px; margin-right:4px;"><use href="#i-users"/></svg>Thành viên CLB</span><span>${members} người (${Math.round((members / maxMetric) * 100)}%)</span></div><div class="progress" style="height:12px;"><div class="progress-bar" style="width:${Math.max(5, Math.floor((members * 100) / maxMetric))}%; background:#0759d9;"></div></div></div>
        <div class="mb-3"><div class="d-flex justify-content-between mb-1 small fw-bold"><span><svg style="width:14px; height:14px; margin-right:4px;"><use href="#i-calendar"/></svg>Sự kiện &amp; Hoạt động</span><span>${activities} hoạt động (${Math.round((activities / maxMetric) * 100)}%)</span></div><div class="progress" style="height:12px;"><div class="progress-bar" style="width:${Math.max(5, Math.floor((activities * 100) / maxMetric))}%; background:#008d76;"></div></div></div>
        <div class="mb-3"><div class="d-flex justify-content-between mb-1 small fw-bold"><span><svg style="width:14px; height:14px; margin-right:4px;"><use href="#i-clipboard"/></svg>Nhiệm vụ đã tạo</span><span>${tasks} nhiệm vụ (${Math.round((tasks / maxMetric) * 100)}%)</span></div><div class="progress" style="height:12px;"><div class="progress-bar" style="width:${Math.max(5, Math.floor((tasks * 100) / maxMetric))}%; background:#d77700;"></div></div></div>
      </div>
    </section>
  </div>
  ${isAdmin ? `<div class="col-lg-6"><section class="surface section-card p-4 h-100">
    <div class="section-heading no-border pt-0 px-0"><div><h2>Cơ cấu lưu chuyển quỹ</h2><p>Tỉ lệ tương quan giữa dòng tiền Thu vào và Chi ra</p></div></div>
    ${totalFlow > 0 ? `<div class="mt-3"><div class="d-flex justify-content-between mb-2 small fw-bold"><span class="text-success">Thu: ${incomePercent}% (${fmtNumber(income)} đ)</span><span class="text-danger">Chi: ${expensePercent}% (${fmtNumber(expense)} đ)</span></div><div class="progress" style="height:20px; border-radius:10px; overflow:hidden;"><div class="progress-bar" style="width:${incomePercent}%; background:#008d76;" title="Thu: ${incomePercent}%">${incomePercent}%</div><div class="progress-bar" style="width:${expensePercent}%; background:#df3347;" title="Chi: ${expensePercent}%">${expensePercent}%</div></div><div class="d-flex justify-content-around mt-4 pt-2 border-top text-center"><div><span class="badge bg-success mb-1">&nbsp;</span><div class="small text-muted">Dòng tiền vào</div><strong>${fmtNumber(income)} đ</strong></div><div><span class="badge bg-danger mb-1">&nbsp;</span><div class="small text-muted">Dòng tiền ra</div><strong>${fmtNumber(expense)} đ</strong></div><div><span class="badge bg-primary mb-1">&nbsp;</span><div class="small text-muted">Trạng thái quỹ</div><strong class="${netBalance >= 0 ? 'text-success' : 'text-danger'}">${netBalance >= 0 ? 'Thặng dư' : 'Bội chi'}</strong></div></div></div>` : '<div class="empty-state">Chưa có giao dịch tài chính để lập biểu đồ.</div>'}
  </section></div>` : ''}
</div>`;
    },
  });
}

