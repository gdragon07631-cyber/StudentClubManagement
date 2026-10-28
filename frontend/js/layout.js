import { session, Roles } from './api.js';
import { esc, takeFlash, alertBox } from './ui.js';

const ROLE_LABEL = u => u.roles.includes(Roles.Admin) ? 'Quản trị hệ thống / Admin' : u.roles.includes(Roles.ClubLeader) ? 'Trưởng câu lạc bộ' : 'Thành viên';
const icon = id => `<svg><use href="#${id}"/></svg>`;
let currentKey = null;

/**
 * Dựng khung trang (sidebar + topbar cho người đã đăng nhập, topbar công khai cho khách).
 * Trả về phần tử nội dung để router đổ trang vào. Chỉ dựng lại khung khi đổi người dùng.
 */
export function renderShell(user, activeNav, isPublic) {
  const root = document.getElementById('root');
  const key = user ? `app:${user.userId}:${user.roles.join(',')}` : 'public';
  if (key !== currentKey) {
    currentKey = key;
    document.body.className = user ? 'app-authenticated' : 'app-public';
    root.innerHTML = user ? appShell(user) : publicShell();
    wire(root);
  }
  root.querySelectorAll('.side-link[data-nav]').forEach(a => a.classList.toggle('active', a.dataset.nav === activeNav));
  document.body.classList.remove('sidebar-open');
  const view = root.querySelector('#view');
  const flash = takeFlash();
  root.querySelector('#flash').innerHTML = flash ? alertBox(flash.type, flash.message) : '';
  return view;
}

function wire(root) {
  root.querySelectorAll('.mobile-menu').forEach(b => b.addEventListener('click', () => document.body.classList.add('sidebar-open')));
  root.querySelectorAll('.sidebar-close,.sidebar-backdrop').forEach(b => b.addEventListener('click', () => document.body.classList.remove('sidebar-open')));
  root.querySelector('[data-logout]')?.addEventListener('click', () => { session.clear(); currentKey = null; location.hash = '#/login'; });
}
document.addEventListener('keydown', e => { if (e.key === 'Escape') document.body.classList.remove('sidebar-open'); });

function publicShell() {
  return `<header class="public-topbar">
    <a class="brand" href="#/"><span class="brand-mark">C</span><span class="brand-name">Hệ thống quản lý CLB sinh viên</span></a>
    <div class="d-flex gap-2"><a class="btn btn-light" href="#/apply">Đăng ký CLB</a><a class="btn btn-primary" href="#/login">Đăng nhập</a></div>
  </header>
  <main class="public-content"><div id="flash"></div><div id="view"></div></main>`;
}

function appShell(u) {
  const admin = u.roles.includes(Roles.Admin), leader = u.roles.includes(Roles.ClubLeader), member = u.roles.includes(Roles.Member);
  const link = (nav, href, ic, text) => `<a class="side-link" data-nav="${nav}" href="${href}">${icon(ic)}<span>${text}</span></a>`;
  const role = ROLE_LABEL(u);
  return `<div class="app-shell">
  <aside class="sidebar" id="appSidebar">
    <div class="sidebar-brand-row"><a class="brand" href="#/dashboard"><span class="brand-mark">C</span><span class="brand-name">Hệ thống quản lý CLB sinh viên</span></a><button class="sidebar-close" type="button" aria-label="Đóng menu">×</button></div>
    <nav class="sidebar-nav" aria-label="Điều hướng chính">
      <div class="nav-section">${admin ? 'Quản trị hệ thống' : leader ? 'Quản lý câu lạc bộ' : 'Khu vực thành viên'}</div>
      ${link('dashboard', '#/dashboard', 'i-grid', 'Tổng quan')}
      ${admin ? link('users', '#/users', 'i-user', 'Quản lý tài khoản') + link('clubs', '#/clubs', 'i-building', 'Quản lý câu lạc bộ') : ''}
      ${!member ? link('applications', '#/applications', 'i-clipboard', 'Đơn đăng ký') : ''}
      ${link('members', '#/members', 'i-users', leader ? 'Thành viên câu lạc bộ' : member ? 'Hồ sơ của tôi' : 'Quản lý thành viên')}
      ${link('activities', '#/activities', 'i-calendar', 'Sự kiện &amp; Hoạt động')}
      ${link('tasks', '#/tasks', 'i-check', member ? 'Nhiệm vụ của tôi' : 'Nhiệm vụ CLB')}
      ${link('meetings', '#/meetings', 'i-calendar', 'Cuộc họp')}
      ${link('points', '#/points', 'i-star', 'Điểm hoạt động')}
      ${admin ? link('finance', '#/finance', 'i-wallet', 'Tài chính &amp; Quỹ') : ''}
      ${!member ? link('reports', '#/reports', 'i-chart', admin ? 'Thống kê &amp; Báo cáo' : 'Thống kê câu lạc bộ') : ''}
      ${link('notifications', '#/notifications', 'i-bell', 'Thông báo')}
      <div class="nav-section mt-4">Trợ lý AI Campus</div>
      ${link('ai', '#/ai', 'i-spark', 'Chatbot AI')}
      ${!member ? link('tasks-ai', '#/tasks', 'i-spark', 'AI gợi ý phân công') : ''}
    </nav>
    <div class="sidebar-footer">
      ${link('change-password', '#/change-password', 'i-user', 'Đổi mật khẩu')}
      <button class="side-link logout-link" type="button" data-logout>${icon('i-logout')}<span>Đăng xuất</span></button>
    </div>
  </aside>
  <div class="app-main">
    <header class="topbar">
      <button class="mobile-menu" type="button" aria-label="Mở menu">${icon('i-menu')}</button>
      <div class="topbar-search">${icon('i-search')}<span>Tìm kiếm tại từng trang dữ liệu...</span></div>
      <div class="topbar-actions"><span class="role-pill">${esc(role)}</span><a class="topbar-icon" href="#/notifications" aria-label="Thông báo">${icon('i-bell')}</a><span class="topbar-separator"></span>
        <a class="user-chip" href="#/change-password"><span class="user-avatar">${esc((u.userName || 'U')[0].toUpperCase())}</span><span><strong>${esc(u.userName)}</strong><small>${esc(role)}</small></span></a></div>
    </header>
    <main class="content-area"><div id="flash"></div><div id="view"></div></main>
  </div>
  <button class="sidebar-backdrop" type="button" aria-label="Đóng menu"></button>
</div>`;
}

