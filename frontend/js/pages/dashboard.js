import { Roles } from '../api.js';

const icon = name => `<svg><use href="#i-${name}"/></svg>`;
const pct = v => Math.max(0, Math.min(100, Number(v) || 0));

function metric(href, ic, label, value, unit, note, noteClass, ui) {
  return `<a class="metric-card surface" href="${href}"><span class="metric-icon">${icon(ic)}</span><div class="metric-label">${ui.esc(label)}</div><div class="metric-value">${ui.esc(value)} <small>${ui.esc(unit)}</small></div><div class="metric-note${noteClass ? ' ' + noteClass : ''}">${ui.esc(note)}</div></a>`;
}

function adminView(d, ui, router) {
  const clubs = d.clubItems || [];
  const acts = d.upcomingActivities || [];
  const meets = d.upcomingMeetings || [];
  return `<header class="page-header">
    <div><div class="page-eyebrow">Campus Executive Hub</div><h1 class="page-title">Quản lý câu lạc bộ &amp; điều hành</h1><p class="page-subtitle">Theo dõi cơ cấu, thành viên, hoạt động và nhiệm vụ từ dữ liệu vận hành thực tế.</p></div>
    <div class="page-actions"><a class="btn btn-light btn-icon" href="#/reports">${icon('chart')}Xem báo cáo</a><a class="btn btn-primary btn-icon" href="#/clubs/new">${icon('plus')}Thêm câu lạc bộ</a></div>
  </header>
  <section class="metric-grid">
    ${metric('#/clubs', 'building', 'Câu lạc bộ', d.clubs, 'câu lạc bộ', 'Toàn bộ câu lạc bộ trong hệ thống', '', ui)}
    ${metric('#/members', 'users', 'Nhân sự hiện diện', d.members, 'thành viên', 'Dữ liệu hồ sơ đang quản lý', 'success', ui)}
    ${metric('#/tasks', 'clipboard', 'Công việc chưa hoàn tất', d.openTasks, 'nhiệm vụ', 'Tiến độ dưới 100%', '', ui)}
    ${metric('#/activities', 'calendar', 'Sự kiện & hoạt động', d.activities, 'hoạt động', 'Tất cả trạng thái trong hệ thống', '', ui)}
  </section>
  <section class="surface section-card mb-4">
    <div class="section-heading"><div><h2>Danh sách câu lạc bộ</h2><p>Số thành viên được lấy trực tiếp từ dữ liệu hiện tại.</p></div><a class="section-link" href="#/clubs">Xem tất cả →</a></div>
    ${clubs.length ? `<div class="department-grid p-3">${clubs.map(c => `<article class="department-card surface"><span class="metric-icon">${icon('building')}</span><h3>${ui.esc(c.clubName)}</h3><p>${ui.esc(c.description ?? 'Chưa có mô tả cho câu lạc bộ này.')}</p><div class="department-stats"><span><span>Thành viên</span><strong>${ui.esc(c.memberCount)} người</strong></span></div><div class="d-flex gap-2 mt-3"><a class="btn btn-light btn-sm flex-fill" href="${router.href('/members', { clubId: c.id })}">Chi tiết</a><a class="btn btn-primary btn-sm flex-fill" href="#/clubs/${encodeURIComponent(c.id)}/edit">Cập nhật</a></div></article>`).join('')}</div>` : '<div class="empty-state">Chưa có câu lạc bộ.</div>'}
  </section>
  <div class="dashboard-grid">
    <section class="surface section-card">
      <div class="section-heading"><div><h2>Hoạt động &amp; lịch họp sắp tới</h2><p>Lịch gần nhất của câu lạc bộ.</p></div></div>
      <div class="calendar-list">
        ${acts.map(a => `<article class="calendar-item success"><span class="status-badge success">Hoạt động</span><h3>${ui.esc(a.activityName)}</h3><p>${ui.esc(ui.fmtDateTime(a.startTime))} · ${ui.esc(a.location ?? 'Chưa có địa điểm')}</p></article>`).join('')}
        ${meets.map(m => `<article class="calendar-item"><span class="status-badge primary">Cuộc họp</span><h3>${ui.esc(m.title)}</h3><p>${ui.esc(ui.fmtDateTime(m.meetingTime))} · ${ui.esc(m.location ?? 'Chưa có địa điểm')}</p></article>`).join('')}
        ${!acts.length && !meets.length ? '<div class="empty-state">Chưa có lịch sắp tới.</div>' : ''}
      </div>
    </section>
    <aside class="stack dashboard-aside"><section class="surface ai-panel"><h2>Trợ lý CLB AI</h2><p>Hỗ trợ tra cứu dữ liệu được phân quyền. AI chỉ gợi ý, không tự thay người quản lý ra quyết định.</p><a class="btn" href="#/ai">Mở Chatbot AI</a></section><section class="surface section-card"><div class="section-heading no-border"><div><h2>Tài chính</h2><p>Số dư hiện tại</p></div></div><div class="p-4 pt-2"><div class="metric-value">${ui.esc(ui.fmtNumber(d.balance))} <small>đ</small></div><a class="section-link d-inline-block mt-3" href="#/finance">Xem sổ quỹ →</a></div></section></aside>
  </div>`;
}

function leaderView(d, ui) {
  const tasks = d.myTasks || [];
  const meets = d.upcomingMeetings || [];
  const taskHtml = tasks.slice(0, 4).map(t => {
    const badge = t.priority === 'HIGH' ? 'danger' : (t.progress > 0 ? 'primary' : 'warning');
    const names = (t.assignments || []).map(x => x.member?.fullName).filter(Boolean);
    return `<article class="task-item"><div class="task-top"><div><span class="status-badge ${badge}">${ui.esc(t.priority)}</span><div class="task-title">${ui.esc(t.title)}</div><div class="task-meta">Phụ trách: ${ui.esc(names.length ? names.join(', ') : 'Chưa phân công')} · Hạn: ${ui.esc(t.dueDate ? ui.fmtDate(t.dueDate) : 'Chưa đặt')}</div></div><strong>${ui.esc(t.progress)}%</strong></div><div class="progress-label"><span>Tiến độ thực hiện</span><span>${ui.esc(t.status)}</span></div><div class="progress"><div class="progress-bar" style="width:${pct(t.progress)}%"></div></div></article>`;
  }).join('');
  return `<header class="page-header">
    <div><div class="page-eyebrow">Quản lý câu lạc bộ</div><h1 class="page-title">Dashboard ${ui.esc(d.clubName ?? 'của Trưởng câu lạc bộ')}</h1><p class="page-subtitle">Chào ${ui.esc(d.displayName)}. Dưới đây là tiến độ nhiệm vụ và hoạt động thuộc câu lạc bộ của bạn.</p></div>
    <div class="page-actions"><a class="btn btn-light btn-icon" href="#/reports">${icon('chart')}Xem báo cáo CLB</a><a class="btn btn-primary btn-icon" href="#/tasks/new">${icon('plus')}Tạo nhiệm vụ mới</a></div>
  </header>
  <section class="metric-grid">
    ${metric('#/members', 'users', 'Thành viên câu lạc bộ', d.members, 'thành viên', 'Theo phạm vi phân quyền', 'success', ui)}
    ${metric('#/tasks', 'clipboard', 'Nhiệm vụ của CLB', d.openTasks, 'đang mở', 'Trong danh sách liên quan', '', ui)}
    ${metric('#/activities', 'calendar', 'Hoạt động CLB', d.activities, 'hoạt động', 'Lịch sự kiện hiện có', '', ui)}
    ${metric('#/notifications', 'bell', 'Thông báo mới', d.unreadNotifications, 'chưa đọc', 'Thông báo dành cho bạn', '', ui)}
  </section>
  <div class="dashboard-grid">
    <div class="stack">
      <section class="surface section-card"><div class="section-heading"><div><h2>Nhiệm vụ cần xử lý &amp; sắp đến hạn</h2><p>Ưu tiên phân công và theo dõi tiến độ đúng hạn.</p></div><a class="section-link" href="#/tasks">Xem tất cả (${tasks.length})</a></div>
      ${tasks.length ? taskHtml : '<div class="empty-state">Chưa có nhiệm vụ trong phạm vi ban.</div>'}
      </section>
      <section class="surface section-card"><div class="section-heading"><div><h2>Thành viên &amp; mức độ tham gia</h2><p>Danh sách chi tiết và điểm được giữ trong module tương ứng.</p></div><a class="section-link" href="#/members">Xem thành viên CLB →</a></div><div class="p-4 d-flex flex-wrap gap-2"><a class="btn btn-light" href="#/members">Hồ sơ thành viên</a><a class="btn btn-light" href="#/points">Điểm hoạt động</a><a class="btn btn-light" href="#/reports">Thống kê câu lạc bộ</a></div></section>
    </div>
    <aside class="stack dashboard-aside">
      <section class="surface ai-panel"><h2>Gợi ý phân công từ AI</h2><p>Chọn một nhiệm vụ trong danh sách để yêu cầu AI phân tích ứng viên phù hợp. Bạn luôn là người xác nhận cuối cùng.</p><a class="btn" href="#/tasks">Chọn nhiệm vụ để gợi ý</a></section>
      <section class="surface section-card"><div class="section-heading no-border"><div><h2>Lịch họp &amp; sự kiện</h2><p>Lịch gần nhất liên quan đến CLB.</p></div><a class="section-link" href="#/meetings">Xem lịch</a></div><div class="calendar-list">${meets.map(m => `<article class="calendar-item"><span class="status-badge primary">${ui.esc(m.status)}</span><h3>${ui.esc(m.title)}</h3><p>${ui.esc(ui.fmtDateTime(m.meetingTime))} · ${ui.esc(m.location ?? 'Chưa có địa điểm')}</p></article>`).join('')}${!meets.length ? '<div class="empty-state">Chưa có cuộc họp.</div>' : ''}</div></section>
    </aside>
  </div>`;
}

function memberView(d, ui) {
  const tasks = d.myTasks || [];
  const acts = d.upcomingActivities || [];
  const meets = d.upcomingMeetings || [];
  const notes = d.recentNotifications || [];
  const taskHtml = tasks.slice(0, 4).map(t => {
    const cls = t.progress >= 100 ? 'success' : (t.priority === 'HIGH' ? 'danger' : 'primary');
    return `<article class="task-item"><div class="task-top"><div><span class="status-badge ${cls}">${ui.esc(t.status)}</span><div class="task-title">${ui.esc(t.title)}</div><div class="task-meta">Hạn: ${ui.esc(t.dueDate ? ui.fmtDate(t.dueDate) : 'Chưa đặt')} · ${ui.esc(t.priority)}</div></div><strong>${ui.esc(t.progress)}%</strong></div><div class="progress-label"><span>Tiến độ thực hiện</span><span>${ui.esc(t.progress)}%</span></div><div class="progress"><div class="progress-bar" style="width:${pct(t.progress)}%"></div></div></article>`;
  }).join('');
  return `<section class="hero-card"><div><span class="hero-kicker">Khu vực thành viên</span><h1>Chào ${ui.esc(d.displayName)}!</h1><p>Bạn có <strong>${ui.esc(d.openTasks)} nhiệm vụ</strong> đang mở và <strong>${ui.esc(d.unreadNotifications)} thông báo</strong> chưa đọc.</p></div><a class="btn btn-icon" href="#/tasks">${icon('check')}Xem việc cần làm</a></section>
  <section class="metric-grid">
    ${metric('#/points', 'star', 'Điểm hoạt động', ui.fmtNumber(d.points), 'điểm', 'Tổng từ lịch sử điểm của bạn', '', ui)}
    ${metric('#/tasks', 'check', 'Nhiệm vụ đang làm', d.openTasks, 'nhiệm vụ', 'Tiến độ dưới 100%', '', ui)}
    ${metric('#/activities', 'calendar', 'Sự kiện hiện có', d.activities, 'sự kiện', 'Xem và đăng ký trong module hoạt động', '', ui)}
    ${metric('#/notifications', 'bell', 'Thông báo mới', d.unreadNotifications, 'chưa đọc', 'Dành riêng cho tài khoản của bạn', '', ui)}
  </section>
  <div class="dashboard-grid">
    <div class="stack">
      <section class="surface section-card"><div class="section-heading"><div><h2>Nhiệm vụ của tôi</h2><p>Tiến độ công việc được người quản lý phân công.</p></div><a class="section-link" href="#/tasks">Xem tất cả</a></div>${tasks.length ? taskHtml : '<div class="empty-state">Bạn chưa có nhiệm vụ được phân công.</div>'}</section>
      <section class="surface section-card"><div class="section-heading"><div><h2>Sự kiện &amp; hoạt động sắp diễn ra</h2><p>Đăng ký tham gia trong module Hoạt động.</p></div><a class="section-link" href="#/activities">Xem hoạt động</a></div><div class="calendar-list">${acts.map(a => `<article class="calendar-item success"><span class="status-badge success">${ui.esc(a.status)}</span><h3>${ui.esc(a.activityName)}</h3><p>${ui.esc(ui.fmtDateTime(a.startTime))} · ${ui.esc(a.location ?? 'Chưa có địa điểm')}</p></article>`).join('')}${!acts.length ? '<div class="empty-state">Chưa có hoạt động sắp tới.</div>' : ''}</div></section>
    </div>
    <aside class="stack dashboard-aside">
      <section class="surface section-card"><div class="section-heading no-border"><div><h2>Lịch họp &amp; Thông báo</h2><p>Dữ liệu mới nhất dành cho bạn.</p></div></div><div class="calendar-list">${meets.slice(0, 2).map(m => `<article class="calendar-item"><span class="status-badge primary">Cuộc họp</span><h3>${ui.esc(m.title)}</h3><p>${ui.esc(ui.fmtDateTime(m.meetingTime))}</p></article>`).join('')}</div><div class="notification-list">${notes.map(n => `<article class="notification-item"><span class="notification-dot"></span><div><strong>${ui.esc(n.title)}</strong><p>${ui.esc(ui.fmtDateTime(n.createdAt))}</p></div></article>`).join('')}</div></section>
      <section class="surface ai-panel"><h2>Trợ lý CLB AI</h2><p>Hỏi về nhiệm vụ, điểm hoạt động hoặc nội dung cuộc họp trong phạm vi bạn được xem.</p><a class="btn" href="#/ai">Hỏi Chatbot AI</a></section>
    </aside>
  </div>`;
}

export function register(router) {
  router.add('/dashboard', {
    nav: 'dashboard', title: 'Tổng quan',
    async render({ view, user, api, ui }) {
      const d = await api.get('api/dashboard');
      // ui.fmtDateTime / fmtDate / fmtNumber được truyền qua đối tượng ui đã có sẵn.
      const roles = user?.roles || [];
      if (roles.includes(Roles.Admin)) view.innerHTML = adminView(d, ui, router);
      else if (roles.includes(Roles.ClubLeader)) view.innerHTML = leaderView(d, ui);
      else view.innerHTML = memberView(d, ui);
    },
  });
}
