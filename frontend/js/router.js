import { session, ApiError, api } from './api.js';
import * as ui from './ui.js';
import { renderShell } from './layout.js';

const routes = [];

/** '/members/:id/edit' -> regex + tên tham số */
function compile(path) {
  const keys = [];
  const re = new RegExp('^' + path.replace(/\/:([A-Za-z]+)/g, (_, k) => { keys.push(k); return '/([^/]+)'; }) + '/?$');
  return { re, keys };
}

export const router = {
  /**
   * Đăng ký route.
   * opts: { title, nav (khóa menu sidebar), public (không cần đăng nhập), roles (mảng vai trò được phép), render(ctx) }
   * ctx: { view, params, query, user, api, ui, router }
   */
  add(path, opts) { routes.push({ path, ...compile(path), ...opts }); },
  /** Link dạng hash, ví dụ router.href('/members', {q:'a'}) -> '#/members?q=a' */
  href(path, query) { return '#' + api.url(path, query); },
  go(path, query) { location.hash = '#' + api.url(path, query); },
  /** Làm mới trang hiện tại. */
  reload() { render(); },
};

function parseHash() {
  const raw = location.hash.replace(/^#/, '') || '/';
  const [path, qs] = raw.split('?');
  return { path: path || '/', query: Object.fromEntries(new URLSearchParams(qs || '')) };
}

function defaultPath() { return session.get() ? '/dashboard' : '/login'; }

let token = 0;
async function render() {
  const my = ++token;
  let { path, query } = parseHash();
  if (path === '/') { router.go(defaultPath()); return; }
  const route = routes.find(r => r.re.test(path));
  const user = session.get();
  if (route && !route.public && !user) { router.go('/login', { returnUrl: path }); return; }
  const view = renderShell(user, route?.nav, route?.public);
  if (!route) { view.innerHTML = notFound(); document.title = 'Không tìm thấy'; return; }
  if (route.roles && !route.roles.some(r => user?.roles?.includes(r))) { view.innerHTML = accessDenied(); return; }
  const m = path.match(route.re);
  const params = Object.fromEntries(route.keys.map((k, i) => [k, decodeURIComponent(m[i + 1])]));
  document.title = `${route.title || ''} - Hệ thống quản lý CLB sinh viên`;
  view.innerHTML = '<div class="surface empty-state">Đang tải...</div>';
  try {
    const body = document.createElement('div');
    await route.render({ view: body, params, query, user, api, ui, router });
    if (my !== token) return; // đã chuyển sang trang khác
    view.replaceChildren(...body.childNodes);
  } catch (err) {
    if (my !== token) return;
    if (err instanceof ApiError && err.status === 403) view.innerHTML = accessDenied();
    else if (err instanceof ApiError && err.status === 404) view.innerHTML = notFound();
    else if (err instanceof ApiError && err.status === 401) { /* api.js đã chuyển về trang đăng nhập */ }
    else { console.error(err); view.innerHTML = ui.alertBox('danger', err?.message || 'Có lỗi xảy ra.'); }
  }
}

const accessDenied = () => `<div class="surface empty-state"><h1 class="h4">Không có quyền truy cập</h1><p>Bạn không có quyền xem trang này.</p><a class="btn btn-primary" href="#/">Về trang chính</a></div>`;
const notFound = () => `<div class="surface empty-state"><h1 class="h4">Không tìm thấy</h1><p>Trang hoặc dữ liệu bạn yêu cầu không tồn tại.</p><a class="btn btn-primary" href="#/">Về trang chính</a></div>`;

export function start() {
  window.addEventListener('hashchange', render);
  render();
}

