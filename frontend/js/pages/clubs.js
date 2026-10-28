import { Roles } from '../api.js';
import { esc } from '../ui.js';

export function register(router) {
  router.add('/clubs', {
    nav: 'clubs', title: 'Quản lý câu lạc bộ', roles: [Roles.Admin],
    async render({ view, api }) {
      const clubs = (await api.get('api/clubs')) || [];
      view.innerHTML = `<header class="page-header">
  <div><div class="page-eyebrow">Dành cho Admin</div><h1 class="page-title">Quản lý câu lạc bộ</h1><p class="page-subtitle">Admin quản lý toàn bộ câu lạc bộ trong hệ thống.</p></div>
  <div class="page-actions"><a class="btn btn-primary btn-icon" href="#/clubs/new"><svg><use href="#i-plus"/></svg>Thêm câu lạc bộ</a></div>
</header>
<section class="department-grid">
${clubs.map(c => `<article class="department-card surface">
  <span class="metric-icon"><svg><use href="#i-building"/></svg></span>
  <h3>${esc(c.clubName)}</h3>
  <p>${esc(c.description || 'Chưa có mô tả.')}</p>
  <div class="department-stats"><span><span>Thành viên</span><strong>${esc(c.memberCount)}</strong></span><span><span>Hoạt động</span><strong>${esc(c.activityCount)}</strong></span><span><span>Cuộc họp</span><strong>${esc(c.meetingCount)}</strong></span></div>
  <div class="d-flex gap-2 mt-3"><a class="btn btn-light btn-sm flex-fill" href="#/members?clubId=${esc(c.id)}">Xem thành viên</a><a class="btn btn-primary btn-sm flex-fill" href="#/clubs/${esc(c.id)}/edit">Cập nhật</a></div>
</article>`).join('')}
</section>
${clubs.length === 0 ? '<div class="surface empty-state">Chưa có câu lạc bộ. Hãy tạo câu lạc bộ đầu tiên.</div>' : ''}`;
    },
  });

  router.add('/clubs/new', {
    nav: 'clubs', title: 'Thêm câu lạc bộ', roles: [Roles.Admin],
    async render({ view, api, ui }) {
      view.innerHTML = `<header class="page-header"><div><div class="page-eyebrow">Dành cho Admin</div><h1 class="page-title">Thêm câu lạc bộ</h1><p class="page-subtitle">Tạo một câu lạc bộ mới trong hệ thống.</p></div></header>
<form class="surface card-body col-xl-8">
  <div data-form-error class="text-danger mb-3"></div>
  <div class="mb-3"><label class="form-label" for="clubName">Tên câu lạc bộ</label><input id="clubName" name="clubName" class="form-control" required maxlength="150"/></div>
  <div class="mb-3"><label class="form-label" for="description">Mô tả</label><textarea id="description" name="description" data-nullable class="form-control" rows="5" maxlength="1000"></textarea></div>
  <div class="d-flex gap-2"><button class="btn btn-primary" type="submit">Lưu câu lạc bộ</button><a class="btn btn-light" href="#/clubs">Hủy</a></div>
</form>`;
      ui.onSubmit(view.querySelector('form'), async d => {
        await api.post('api/clubs', { id: 0, clubName: d.clubName, description: d.description });
        ui.flash('success', 'Đã thêm câu lạc bộ mới.');
        router.go('/clubs');
      });
    },
  });

  router.add('/clubs/:id/edit', {
    nav: 'clubs', title: 'Cập nhật câu lạc bộ', roles: [Roles.Admin],
    async render({ view, params, api, ui }) {
      const club = await api.get(`api/clubs/${encodeURIComponent(params.id)}`);
      view.innerHTML = `<header class="page-header"><div><div class="page-eyebrow">Dành cho Admin</div><h1 class="page-title">Cập nhật câu lạc bộ</h1></div></header>
<form class="surface card-body col-xl-8">
  <div data-form-error class="text-danger mb-3"></div>
  <div class="mb-3"><label class="form-label" for="clubName">Tên câu lạc bộ</label><input id="clubName" name="clubName" class="form-control" required maxlength="150" value="${esc(club.clubName)}"/></div>
  <div class="mb-3"><label class="form-label" for="description">Mô tả</label><textarea id="description" name="description" data-nullable class="form-control" rows="5" maxlength="1000">${esc(club.description)}</textarea></div>
  <div class="d-flex gap-2"><button class="btn btn-primary" type="submit">Lưu thay đổi</button><a class="btn btn-light" href="#/clubs">Hủy</a></div>
</form>`;
      ui.onSubmit(view.querySelector('form'), async d => {
        await api.put(`api/clubs/${encodeURIComponent(club.id)}`, { id: club.id, clubName: d.clubName, description: d.description });
        ui.flash('success', 'Đã cập nhật câu lạc bộ.');
        router.go('/clubs');
      });
    },
  });
}

