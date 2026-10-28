import { esc, fmtDateTime } from '../ui.js';

export function register(router) {
  router.add('/notifications', {
    nav: 'notifications', title: 'Thông báo',
    async render({ view, api }) {
      const items = (await api.get('api/notifications')) || [];
      const unread = items.filter(x => !x.isRead).length;
      view.innerHTML = `<header class="page-header"><div><div class="page-eyebrow">Hộp thư CLB</div><h1 class="page-title">Thông báo</h1><p class="page-subtitle">Các thông tin mới nhất gửi tới tài khoản của bạn.</p></div></header>
<section class="surface section-card"><div class="section-heading"><div><h2>Danh sách thông báo</h2><p>${unread} thông báo chưa đọc</p></div></div><div class="notification-list">
${items.map(x => `<article class="notification-item"><span class="notification-dot" style="opacity:${x.isRead ? '.25' : '1'}"></span><div class="flex-grow-1"><div class="d-flex justify-content-between gap-3"><strong>${esc(x.title)}</strong><small class="text-muted">${fmtDateTime(x.createdAt)}</small></div><p class="mt-1">${esc(x.content)}</p>${!x.isRead ? `<button type="button" class="btn btn-sm btn-light mt-2" data-read="${esc(x.id)}">Đánh dấu đã đọc</button>` : ''}</div></article>`).join('')}
</div>${items.length === 0 ? '<div class="empty-state">Chưa có thông báo.</div>' : ''}</section>`;
      view.querySelectorAll('[data-read]').forEach(btn => {
        btn.addEventListener('click', async () => {
          btn.disabled = true;
          await api.post(`api/notifications/${encodeURIComponent(btn.dataset.read)}/read`);
          router.reload();
        });
      });
    },
  });
}

