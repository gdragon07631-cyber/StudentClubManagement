import { Roles } from '../api.js';

const MANAGERS = [Roles.Admin, Roles.ClubLeader];

export function register(router) {
  router.add('/activities', {
    nav: 'activities', title: 'Sự kiện & Hoạt động',
    async render({ view, query, user, api, ui }) {
      const q = query.q || '', status = query.status || '', sort = query.sort || 'newest';
      const isMember = user.roles.includes(Roles.Member);
      const cards = (await api.get('api/activities', { q, status, sort })) || [];
      const esc = ui.esc;

      const sel = (v, cur) => (v === cur ? ' selected' : '');

      const cardHtml = card => {
        const x = card.activity;
        const badge = x.status === 'COMPLETED' ? 'success' : x.status === 'CANCELLED' ? 'danger' : 'primary';
        const attendances = card.attendances || [];
        const unrecorded = card.unrecorded || [];
        const endTime = ui.fmtDateTime(x.endTime).split(' ')[1] || '';
        const memberButtons = isMember && x.status !== 'CANCELLED'
          ? `<button type="button" class="btn btn-primary btn-sm" data-act="register" data-id="${esc(x.id)}">Đăng ký</button>
             <button type="button" class="btn btn-light btn-sm" data-act="unregister" data-id="${esc(x.id)}">Hủy đăng ký</button>` : '';
        const managerButtons = !isMember
          ? `<a class="btn btn-light btn-sm" href="${router.href(`/activities/${x.id}/edit`)}">Chỉnh sửa</a>
             ${x.status !== 'CANCELLED' ? `<button type="button" class="btn btn-outline-danger btn-sm" data-act="cancel-activity" data-id="${esc(x.id)}">Hủy hoạt động</button>` : ''}` : '';
        const attendanceHtml = !isMember ? `
          <details class="mt-3 pt-2 border-top">
            <summary class="text-primary small" style="cursor:pointer; font-weight:600;">Điểm danh thành viên (${attendances.length} đã ghi)</summary>
            ${unrecorded.length && x.status !== 'CANCELLED' ? `
              <form data-attendance="${esc(x.id)}" class="d-flex flex-wrap gap-2 mt-2 align-items-center">
                <select name="memberId" class="form-select form-select-sm" style="max-width:200px" required>
                  <option value="">-- Chọn thành viên --</option>
                  ${unrecorded.map(m => `<option value="${esc(m.id)}">${esc(m.fullName)} (${esc(m.studentCode)})</option>`).join('')}
                </select>
                <select name="status" class="form-select form-select-sm" style="max-width:120px" required>
                  <option value="PRESENT">Có mặt</option>
                  <option value="LATE">Đi muộn</option>
                  <option value="ABSENT">Vắng mặt</option>
                </select>
                <button class="btn btn-sm btn-primary" type="submit">Điểm danh</button>
              </form>` : ''}
            ${attendances.length ? `
              <div class="small mt-2"><ul class="list-unstyled mb-0">
                ${attendances.map(att => {
                  const b = att.attendanceStatus === 'PRESENT' ? 'success' : att.attendanceStatus === 'LATE' ? 'warning' : 'danger';
                  const t = att.attendanceStatus === 'PRESENT' ? 'Có mặt' : att.attendanceStatus === 'LATE' ? 'Đi muộn' : 'Vắng';
                  return `<li class="d-flex justify-content-between py-1 border-bottom">
                    <span>${esc(att.member?.fullName)} (${esc(att.member?.studentCode)})</span>
                    <span class="status-badge ${b}">${t}</span></li>`;
                }).join('')}
              </ul></div>` : '<p class="text-muted small mt-2 mb-0">Chưa có bản ghi điểm danh nào.</p>'}
          </details>` : '';
        return `<div class="col-md-6">
          <article class="surface h-100 p-4">
            <div class="d-flex justify-content-between gap-3">
              <span class="metric-icon"><svg><use href="#i-calendar"/></svg></span>
              <span class="status-badge ${badge}">${esc(x.status)}</span>
            </div>
            <h2 class="h4 mt-3">${esc(x.activityName)}</h2>
            <p class="text-muted">${esc(x.description || 'Chưa có mô tả.')}</p>
            <div class="calendar-item success mt-3">
              <strong>${esc(ui.fmtDateTime(x.startTime))}</strong>
              <p class="mt-1">${esc(x.location || 'Chưa có địa điểm')} · Kết thúc ${esc(endTime)}</p>
            </div>
            <div class="d-flex flex-wrap gap-2 mt-3">${memberButtons}${managerButtons}</div>
            ${attendanceHtml}
          </article>
        </div>`;
      };

      view.innerHTML = ui.pageHeader({
        eyebrow: 'Lịch CLB', title: 'Sự kiện & Hoạt động',
        subtitle: 'Theo dõi lịch trình, địa điểm, trạng thái và thao tác đăng ký theo quyền hiện có.',
        actionsHtml: !isMember ? `<a class="btn btn-primary btn-icon" href="${router.href('/activities/new')}"><svg><use href="#i-plus"/></svg>Tạo hoạt động</a>` : '',
      }) + `
      <form class="surface filter-card" data-filter><div class="row g-2">
        <div class="col-lg-5"><input name="q" value="${esc(q)}" class="form-control" placeholder="Tên hoạt động hoặc địa điểm"/></div>
        <div class="col-lg-3"><select name="status" class="form-select">
          <option value="">Mọi trạng thái</option>
          <option value="PLANNED"${sel('PLANNED', status)}>Sắp diễn ra</option>
          <option value="CANCELLED"${sel('CANCELLED', status)}>Đã hủy</option>
          <option value="COMPLETED"${sel('COMPLETED', status)}>Hoàn thành</option>
        </select></div>
        <div class="col-lg-2"><select name="sort" class="form-select">
          <option value="newest">Mới nhất</option>
          <option value="name"${sel('name', sort)}>Tên A-Z</option>
        </select></div>
        <div class="col"><button class="btn btn-primary w-100" type="submit">Lọc</button></div>
      </div></form>
      <div class="row g-4">${cards.map(cardHtml).join('')}</div>
      ${cards.length ? '' : '<div class="surface empty-state">Không có hoạt động phù hợp.</div>'}`;

      view.querySelector('[data-filter]').addEventListener('submit', e => {
        e.preventDefault();
        const d = ui.formToObject(e.target);
        router.go('/activities', { q: d.q, status: d.status, sort: d.sort });
      });

      const run = async (fn, okMsg) => {
        try { await fn(); if (okMsg) ui.flash('success', okMsg); }
        catch (err) { ui.flash('danger', err?.message || 'Có lỗi xảy ra.'); }
        router.reload();
      };
      view.querySelectorAll('button[data-act]').forEach(btn => btn.addEventListener('click', () => {
        const id = btn.dataset.id;
        switch (btn.dataset.act) {
          case 'register': run(() => api.post(`api/activities/${id}/register`), 'Đăng ký hoạt động thành công.'); break;
          case 'unregister':
            run(async () => {
              try { await api.del(`api/activities/${id}/register`); }
              catch { throw new Error('Không tìm thấy đăng ký hoạt động.'); }
            });
            break;
          case 'cancel-activity':
            if (ui.confirmAction('Xác nhận hủy hoạt động?')) run(() => api.post(`api/activities/${id}/cancel-activity`));
            break;
        }
      }));
      view.querySelectorAll('form[data-attendance]').forEach(form => {
        ui.onSubmit(form, async d => {
          await api.post(`api/activities/${form.dataset.attendance}/attendance`, { memberId: Number(d.memberId), status: d.status || '' });
          router.reload();
        });
      });
    },
  });

  const formHtml = (ui, { a, clubs, isAdmin, isEdit }) => {
    const esc = ui.esc;
    return `<form class="card card-body">
      <div data-form-error class="text-danger mb-3"></div>
      ${isAdmin ? `<label class="form-label">Câu lạc bộ</label><select name="clubId" class="form-select mb-2" required>
        ${ui.options(clubs, 'id', 'clubName', a.clubId || '', '-- Chọn câu lạc bộ --')}</select>` : ''}
      <label class="form-label">Tên hoạt động</label><input name="activityName" class="form-control mb-2" maxlength="180" required value="${esc(a.activityName)}"/>
      <label class="form-label">Mô tả</label><textarea name="description" class="form-control mb-2" maxlength="2000">${esc(a.description)}</textarea>
      <div class="row">
        <div class="col"><label class="form-label">Bắt đầu</label><input name="startTime" type="datetime-local" class="form-control" required value="${esc(ui.toInputDateTime(a.startTime))}"/></div>
        <div class="col"><label class="form-label">Kết thúc</label><input name="endTime" type="datetime-local" class="form-control" required value="${esc(ui.toInputDateTime(a.endTime))}"/></div>
      </div>
      <label class="form-label mt-2">Địa điểm</label><input name="location" class="form-control ${isEdit ? 'mb-2' : 'mb-3'}" maxlength="300" value="${esc(a.location)}"/>
      ${isEdit ? `<label class="form-label">Trạng thái</label><input name="status" class="form-control mb-2" maxlength="30" value="${esc(a.status)}"/>
      <label class="form-label">Kết quả</label><textarea name="result" class="form-control mb-3" maxlength="2000">${esc(a.result)}</textarea>` : ''}
      <div><button class="btn btn-primary" type="submit">${isEdit ? 'Lưu thay đổi' : 'Lưu'}</button></div>
    </form>`;
  };

  const bind = (ctx, a, isAdmin, isEdit, id) => {
    const { view, api, ui } = ctx;
    ui.onSubmit(view.querySelector('form'), async d => {
      if (d.endTime < d.startTime) throw new Error('Thời gian kết thúc phải sau thời gian bắt đầu.');
      const body = {
        clubId: isAdmin ? Number(d.clubId) : (a.clubId || 0),
        activityName: d.activityName, description: d.description || null,
        startTime: d.startTime, endTime: d.endTime, location: d.location || null,
        status: isEdit ? (d.status || null) : null, result: isEdit ? (d.result || null) : null,
      };
      if (isEdit) await api.put(`api/activities/${id}`, body); else await api.post('api/activities', body);
      router.go('/activities');
    });
  };

  router.add('/activities/new', {
    nav: 'activities', title: 'Tạo hoạt động', roles: MANAGERS,
    async render(ctx) {
      const { view, user, api, ui } = ctx;
      const isAdmin = user.roles.includes(Roles.Admin);
      const clubs = (await api.get('api/activities/clubs')) || [];
      view.innerHTML = `<h1>Tạo hoạt động</h1>` + formHtml(ui, { a: {}, clubs, isAdmin, isEdit: false });
      bind(ctx, {}, isAdmin, false);
    },
  });

  router.add('/activities/:id/edit', {
    nav: 'activities', title: 'Sửa hoạt động', roles: MANAGERS,
    async render(ctx) {
      const { view, params, user, api, ui } = ctx;
      const isAdmin = user.roles.includes(Roles.Admin);
      const [a, clubs] = await Promise.all([api.get(`api/activities/${params.id}`), api.get('api/activities/clubs')]);
      view.innerHTML = `<h1>Sửa hoạt động</h1>` + formHtml(ui, { a, clubs: clubs || [], isAdmin, isEdit: true });
      bind(ctx, a, isAdmin, true, params.id);
    },
  });
}
