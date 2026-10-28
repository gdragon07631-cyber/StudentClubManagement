import { Roles } from '../api.js';
import { esc, fmtDate, toInputDate } from '../ui.js';

const isIn = (user, role) => !!user?.roles?.includes(role);

export function register(router) {
  router.add('/tasks', {
    nav: 'tasks', title: 'Nhiệm vụ',
    async render({ view, query, user, api, ui }) {
      const isMember = isIn(user, Roles.Member);
      const q = query.q || '', status = query.status || '', sort = query.sort || 'due';
      const data = await api.get('api/club-tasks', { q, status, sort });
      const items = data?.items || [];
      const members = data?.members || [];

      view.innerHTML = `<header class="page-header"><div><div class="page-eyebrow">Quản lý công việc</div><h1 class="page-title">${isMember ? 'Nhiệm vụ của tôi' : 'Nhiệm vụ CLB'}</h1><p class="page-subtitle">Theo dõi phân công, thời hạn và tiến độ từ dữ liệu nhiệm vụ hiện có.</p></div>${!isMember ? `<div class="page-actions"><a class="btn btn-primary btn-icon" href="#/tasks/new"><svg><use href="#i-plus"/></svg>Tạo nhiệm vụ mới</a></div>` : ''}</header>
<form class="surface filter-card"><div class="row g-2"><div class="col-lg-5"><input name="q" value="${esc(q)}" class="form-control" placeholder="Tìm theo tiêu đề nhiệm vụ"/></div><div class="col-lg-3"><select name="status" class="form-select"><option value="">Mọi trạng thái</option><option value="OPEN"${status === 'OPEN' ? ' selected' : ''}>OPEN</option><option value="IN_PROGRESS"${status === 'IN_PROGRESS' ? ' selected' : ''}>IN_PROGRESS</option><option value="COMPLETED"${status === 'COMPLETED' ? ' selected' : ''}>COMPLETED</option></select></div><div class="col-lg-2"><select name="sort" class="form-select"><option value="due">Hạn gần nhất</option><option value="title"${sort === 'title' ? ' selected' : ''}>Tên A-Z</option><option value="progress"${sort === 'progress' ? ' selected' : ''}>Tiến độ cao</option></select></div><div class="col"><button class="btn btn-primary w-100" type="submit">Lọc</button></div></div></form>
<section class="surface section-card"><div class="section-heading"><div><h2>Danh sách nhiệm vụ</h2><p>${items.length} nhiệm vụ trong phạm vi truy cập</p></div></div>
${items.map(x => {
  const badge = x.progress >= 100 ? 'success' : x.priority === 'HIGH' ? 'danger' : x.progress > 0 ? 'primary' : 'warning';
  const source = x.activity?.activityName || x.meeting?.title || 'Nhiệm vụ độc lập';
  const assignees = (x.assignments || []).length ? x.assignments.map(a => a.member?.fullName).filter(Boolean).join(', ') : 'Chưa phân công';
  return `<article class="task-item"><div class="task-top"><div><span class="status-badge ${badge}">${esc(x.priority)} · ${esc(x.status)}</span><div class="task-title">${esc(x.title)}</div><div class="task-meta">Nguồn: ${esc(source)} · Hạn: ${x.dueDate ? fmtDate(x.dueDate) : 'Chưa đặt'}</div><div class="task-meta mt-1">Phụ trách: ${esc(assignees)}</div></div><strong>${esc(x.progress)}%</strong></div><div class="progress-label"><span>Tiến độ thực hiện</span><span>${esc(x.progress)}%</span></div><div class="progress"><div class="progress-bar" style="width:${Number(x.progress) || 0}%"></div></div><div class="d-flex flex-wrap gap-2 mt-3">
${isMember ? `<form class="d-flex gap-2" data-progress="${esc(x.id)}"><input type="number" name="progress" min="0" max="100" value="${esc(x.progress)}" class="form-control form-control-sm" required/><button class="btn btn-sm btn-primary" type="submit">Cập nhật tiến độ</button></form>` : `<a class="btn btn-sm btn-light" href="#/tasks/${esc(x.id)}/edit">Chỉnh sửa</a><form class="d-flex gap-1" data-assign="${esc(x.id)}"><select name="memberId" data-number class="form-select form-select-sm" required><option value="">Chọn thành viên</option>${ui.options(members, 'id', 'name')}</select><button class="btn btn-sm btn-primary" type="submit">Phân công</button></form><a class="btn btn-sm btn-outline-primary" href="#/ai/recommendations/${esc(x.id)}">AI gợi ý</a>`}
</div></article>`;
}).join('')}
${items.length === 0 ? '<div class="empty-state">Chưa có nhiệm vụ phù hợp.</div>' : ''}</section>`;

      view.querySelector('form.filter-card').addEventListener('submit', e => {
        e.preventDefault();
        const f = e.target;
        router.go('/tasks', { q: f.q.value, status: f.status.value, sort: f.sort.value });
      });

      view.querySelectorAll('[data-assign]').forEach(form => {
        form.addEventListener('submit', async e => {
          e.preventDefault();
          const d = ui.formToObject(form);
          try {
            await api.post(`api/club-tasks/${encodeURIComponent(form.dataset.assign)}/assign`, { memberId: d.memberId });
            ui.flash('success', 'Đã phân công nhiệm vụ.');
          } catch (err) {
            ui.flash('danger', err?.message || 'Có lỗi xảy ra.');
          }
          router.reload();
        });
      });

      view.querySelectorAll('[data-progress]').forEach(form => {
        form.addEventListener('submit', async e => {
          e.preventDefault();
          const d = ui.formToObject(form);
          try {
            await api.post(`api/club-tasks/${encodeURIComponent(form.dataset.progress)}/progress`, { progress: d.progress });
            ui.flash('success', 'Đã cập nhật tiến độ.');
          } catch (err) {
            ui.flash('danger', err?.message || 'Có lỗi xảy ra.');
          }
          router.reload();
        });
      });
    },
  });

  router.add('/tasks/new', {
    nav: 'tasks', title: 'Tạo nhiệm vụ', roles: [Roles.Admin, Roles.ClubLeader],
    async render({ view, api, ui }) {
      const src = await api.get('api/club-tasks/sources');
      view.innerHTML = `<h1>Tạo nhiệm vụ</h1><form class="card card-body"><div data-form-error class="text-danger mb-3"></div>
  <label for="title">Tiêu đề</label><input id="title" name="title" class="form-control mb-2" required maxlength="200"/>
  <label for="description">Mô tả</label><textarea id="description" name="description" data-nullable class="form-control mb-2"></textarea>
  <div class="row"><div class="col"><label for="activityId">Hoạt động</label><select id="activityId" name="activityId" data-number class="form-select"><option value="">-- Không --</option>${ui.options(src?.activities, 'id', 'name')}</select></div>
  <div class="col"><label for="meetingId">Cuộc họp</label><select id="meetingId" name="meetingId" data-number class="form-select"><option value="">-- Không --</option>${ui.options(src?.meetings, 'id', 'name')}</select></div></div>
  <label for="dueDate" class="mt-2">Hạn</label><input id="dueDate" name="dueDate" data-nullable type="date" class="form-control mb-3"/>
  <button class="btn btn-primary" type="submit">Lưu</button>
</form>`;
      ui.onSubmit(view.querySelector('form'), async d => {
        await api.post('api/club-tasks', {
          id: 0, title: d.title, description: d.description, activityId: d.activityId, meetingId: d.meetingId,
          dueDate: d.dueDate, priority: 'MEDIUM', status: 'OPEN', progress: 0,
        });
        router.go('/tasks');
      });
    },
  });

  router.add('/tasks/:id/edit', {
    nav: 'tasks', title: 'Sửa nhiệm vụ', roles: [Roles.Admin, Roles.ClubLeader],
    async render({ view, params, api, ui }) {
      const data = await api.get(`api/club-tasks/${encodeURIComponent(params.id)}`);
      const t = data.task;
      const src = data.sources;
      view.innerHTML = `<h1>Sửa nhiệm vụ</h1><form class="card card-body"><div data-form-error class="text-danger mb-3"></div>
  <label for="title">Tiêu đề</label><input id="title" name="title" class="form-control mb-2" required maxlength="200" value="${esc(t.title)}"/>
  <label for="description">Mô tả</label><textarea id="description" name="description" data-nullable class="form-control mb-2">${esc(t.description)}</textarea>
  <div class="row"><div class="col"><label for="activityId">Hoạt động</label><select id="activityId" name="activityId" data-number class="form-select"><option value="">Không</option>${ui.options(src?.activities, 'id', 'name', t.activityId)}</select></div>
  <div class="col"><label for="meetingId">Cuộc họp</label><select id="meetingId" name="meetingId" data-number class="form-select"><option value="">Không</option>${ui.options(src?.meetings, 'id', 'name', t.meetingId)}</select></div></div>
  <div class="row mt-2"><div class="col"><label for="priority">Ưu tiên</label><input id="priority" name="priority" class="form-control" value="${esc(t.priority)}"/></div>
  <div class="col"><label for="dueDate">Hạn</label><input id="dueDate" name="dueDate" data-nullable type="date" class="form-control" value="${esc(toInputDate(t.dueDate))}"/></div>
  <div class="col"><label for="status">Trạng thái</label><input id="status" name="status" class="form-control" value="${esc(t.status)}"/></div></div>
  <button class="btn btn-primary mt-3" type="submit">Lưu thay đổi</button>
</form>`;
      ui.onSubmit(view.querySelector('form'), async d => {
        await api.put(`api/club-tasks/${encodeURIComponent(t.id)}`, {
          ...t, ...d, id: t.id,
        });
        router.go('/tasks');
      });
    },
  });

  router.add('/ai/recommendations/:taskId', {
    nav: 'tasks', title: 'AI gợi ý phân công', roles: [Roles.Admin, Roles.ClubLeader],
    async render({ view, params, api, ui }) {
      const taskId = params.taskId;
      const items = (await api.post(`api/ai/recommend/${encodeURIComponent(taskId)}`)) || [];
      view.innerHTML = `<h1>AI gợi ý phân công</h1><div class="alert alert-warning">AI không tự phân công. Người quản lý phải đọc lý do và bấm “Chấp nhận phân công”.</div>
<table class="table"><thead><tr><th>Thành viên</th><th>Phù hợp</th><th>Lý do</th><th>Quyết định</th></tr></thead><tbody>
${items.map(x => `<tr><td>${esc(x.memberName)} (#${esc(x.memberId)})</td><td>${esc(x.suitability)}%</td><td>${esc(x.reason)}</td><td><button type="button" class="btn btn-sm btn-success" data-accept="${esc(x.memberId)}">Chấp nhận phân công</button></td></tr>`).join('')}
</tbody></table><a class="btn btn-outline-secondary" href="#/tasks">Từ chối và quay lại</a>`;
      view.querySelectorAll('[data-accept]').forEach(btn => {
        btn.addEventListener('click', async () => {
          btn.disabled = true;
          try {
            await api.post(`api/club-tasks/${encodeURIComponent(taskId)}/assign`, { memberId: Number(btn.dataset.accept) });
            ui.flash('success', 'Đã phân công nhiệm vụ.');
          } catch (err) {
            ui.flash('danger', err?.message || 'Có lỗi xảy ra.');
          }
          router.go('/tasks');
        });
      });
    },
  });
}

