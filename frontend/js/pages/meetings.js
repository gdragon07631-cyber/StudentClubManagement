import { Roles } from '../api.js';

const MANAGERS = [Roles.Admin, Roles.ClubLeader];

export function register(router) {
  router.add('/meetings', {
    nav: 'meetings', title: 'Cuộc họp',
    async render({ view, query, user, api, ui }) {
      const q = query.q || '', status = query.status || '', sort = query.sort || 'newest';
      const isMember = user.roles.includes(Roles.Member);
      const cards = (await api.get('api/meetings', { q, status, sort })) || [];
      const esc = ui.esc;

      const cardHtml = card => {
        const x = card.meeting;
        const attendances = card.attendances || [];
        const unrecorded = card.unrecorded || [];
        const text = (label, v) => (v && String(v).trim() ? `<p class="mb-1"><strong>${label}:</strong> ${esc(v)}</p>` : '');
        const manage = !isMember ? `
          <div class="d-flex gap-2 mt-3">
            <a class="btn btn-sm btn-light" href="${router.href(`/meetings/${x.id}/edit`)}">Sửa / ghi biên bản</a>
          </div>
          <details class="mt-3 pt-2 border-top">
            <summary class="text-primary small" style="cursor:pointer; font-weight:600;">Điểm danh cuộc họp (${attendances.length} đã ghi)</summary>
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
        return `<article class="surface p-4">
          <div class="d-flex justify-content-between align-items-start gap-3">
            <div>
              <span class="status-badge primary">${esc(x.status)}</span>
              <h2 class="h4 mt-3 mb-2">${esc(x.title)}</h2>
            </div>
            <span class="metric-icon"><svg><use href="#i-calendar"/></svg></span>
          </div>
          <div class="calendar-item">
            <strong>${esc(ui.fmtDateTime(x.meetingTime))}</strong>
            <p class="mt-1">${esc(x.location || 'Chưa có địa điểm')}</p>
          </div>
          ${x.agenda && x.agenda.trim() ? `<p class="mt-3 mb-1"><strong>Chương trình:</strong> ${esc(x.agenda)}</p>` : ''}
          ${text('Biên bản', x.minutes)}${text('Quyết định', x.decisions)}
          ${manage}
        </article>`;
      };

      view.innerHTML = ui.pageHeader({
        eyebrow: 'Lịch làm việc', title: 'Cuộc họp',
        subtitle: 'Lịch họp, chương trình, biên bản và quyết định được lưu trong hệ thống.',
        actionsHtml: !isMember ? `<a class="btn btn-primary btn-icon" href="${router.href('/meetings/new')}"><svg><use href="#i-plus"/></svg>Tạo lịch họp</a>` : '',
      }) + `
      <form class="surface filter-card" data-filter><div class="row g-2">
        <div class="col-lg-5"><input name="q" value="${esc(q)}" class="form-control" placeholder="Tiêu đề hoặc địa điểm"/></div>
        <div class="col-lg-3"><input name="status" value="${esc(status)}" class="form-control" placeholder="Trạng thái"/></div>
        <div class="col-lg-2"><select name="sort" class="form-select">
          <option value="newest">Mới nhất</option>
          <option value="title"${sort === 'title' ? ' selected' : ''}>Tên A-Z</option>
        </select></div>
        <div class="col"><button class="btn btn-primary w-100" type="submit">Lọc</button></div>
      </div></form>
      <div class="stack">${cards.map(cardHtml).join('')}</div>
      ${cards.length ? '' : '<div class="surface empty-state">Chưa có cuộc họp phù hợp.</div>'}`;

      view.querySelector('[data-filter]').addEventListener('submit', e => {
        e.preventDefault();
        const d = ui.formToObject(e.target);
        router.go('/meetings', { q: d.q, status: d.status, sort: d.sort });
      });
      view.querySelectorAll('form[data-attendance]').forEach(form => {
        ui.onSubmit(form, async d => {
          await api.post(`api/meetings/${form.dataset.attendance}/attendance`, { memberId: Number(d.memberId), status: d.status || '' });
          router.reload();
        });
      });
    },
  });

  const formHtml = (ui, { m, clubs, isAdmin, isEdit }) => {
    const esc = ui.esc;
    return `<form class="card card-body">
      <div data-form-error class="text-danger mb-3"></div>
      ${isAdmin ? `<label class="form-label">Câu lạc bộ</label><select name="clubId" class="form-select mb-2" required>
        ${ui.options(clubs, 'id', 'clubName', m.clubId || '', isEdit ? undefined : '-- Chọn câu lạc bộ --')}</select>` : ''}
      <label class="form-label">Tiêu đề</label><input name="title" class="form-control mb-2" maxlength="180" required value="${esc(m.title)}"/>
      <label class="form-label">Thời gian</label><input name="meetingTime" type="datetime-local" class="form-control mb-2" required value="${esc(ui.toInputDateTime(m.meetingTime))}"/>
      <label class="form-label">Địa điểm</label><input name="location" class="form-control mb-2" maxlength="300" value="${esc(m.location)}"/>
      <label class="form-label">Chương trình</label><textarea name="agenda" class="form-control mb-2" maxlength="3000">${esc(m.agenda)}</textarea>
      <label class="form-label">Biên bản</label><textarea name="minutes" class="form-control mb-2" maxlength="6000">${esc(m.minutes)}</textarea>
      <label class="form-label">Quyết định</label><textarea name="decisions" class="form-control ${isEdit ? 'mb-2' : 'mb-3'}" maxlength="3000">${esc(m.decisions)}</textarea>
      ${isEdit ? `<label class="form-label">Trạng thái</label><input name="status" class="form-control mb-3" maxlength="30" value="${esc(m.status)}"/>` : ''}
      <div><button class="btn btn-primary" type="submit">${isEdit ? 'Lưu thay đổi' : 'Lưu'}</button></div>
    </form>`;
  };

  const bind = ({ view, api, ui }, m, isAdmin, isEdit, id) => {
    ui.onSubmit(view.querySelector('form'), async d => {
      const body = {
        clubId: isAdmin ? Number(d.clubId) : (m.clubId || 0),
        title: d.title, meetingTime: d.meetingTime, location: d.location || null,
        agenda: d.agenda || null, minutes: d.minutes || null, decisions: d.decisions || null,
        status: isEdit ? (d.status || null) : null,
      };
      if (isEdit) await api.put(`api/meetings/${id}`, body); else await api.post('api/meetings', body);
      router.go('/meetings');
    });
  };

  router.add('/meetings/new', {
    nav: 'meetings', title: 'Tạo cuộc họp', roles: MANAGERS,
    async render(ctx) {
      const { view, user, api, ui } = ctx;
      const isAdmin = user.roles.includes(Roles.Admin);
      const clubs = (await api.get('api/meetings/clubs')) || [];
      view.innerHTML = `<h1>Tạo cuộc họp</h1>` + formHtml(ui, { m: {}, clubs, isAdmin, isEdit: false });
      bind(ctx, {}, isAdmin, false);
    },
  });

  router.add('/meetings/:id/edit', {
    nav: 'meetings', title: 'Sửa cuộc họp', roles: MANAGERS,
    async render(ctx) {
      const { view, params, user, api, ui } = ctx;
      const isAdmin = user.roles.includes(Roles.Admin);
      const [m, clubs] = await Promise.all([api.get(`api/meetings/${params.id}`), api.get('api/meetings/clubs')]);
      view.innerHTML = `<h1>Sửa cuộc họp và biên bản</h1>` + formHtml(ui, { m, clubs: clubs || [], isAdmin, isEdit: true });
      bind(ctx, m, isAdmin, true, params.id);
    },
  });
}
