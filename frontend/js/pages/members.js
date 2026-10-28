import { Roles } from '../api.js';
import { esc, fmtDate } from '../ui.js';

const isIn = (user, role) => !!user?.roles?.includes(role);

/** Hiển thị kết quả AI đánh giá tham gia ở đầu trang (thay cho trang Chat của bản MVC). */
async function runParticipation({ view, api, ui }, memberId, btn) {
  if (btn) btn.disabled = true;
  let holder = view.querySelector('[data-ai-result]');
  if (!holder) {
    holder = document.createElement('div');
    holder.dataset.aiResult = '';
    view.prepend(holder);
  }
  try {
    const r = await api.post(`api/ai/participation/${memberId}`);
    holder.innerHTML = `<div class="calendar-item success mb-3"><span class="status-badge success">${esc(r?.function)}</span><p class="mt-3 text-dark" style="white-space:pre-wrap">${esc(r?.result)}</p></div>`;
  } catch (err) {
    holder.innerHTML = ui.alertBox('danger', err?.message || 'Có lỗi xảy ra.');
  } finally {
    if (btn) btn.disabled = false;
  }
  holder.scrollIntoView?.({ block: 'nearest' });
}

export function register(router) {
  // ---- Danh sách ----
  router.add('/members', {
    nav: 'members', title: 'Thành viên',
    async render(ctx) {
      const { view, query, user, api, ui } = ctx;
      const isAdmin = isIn(user, Roles.Admin), isMember = isIn(user, Roles.Member), isLeader = isIn(user, Roles.ClubLeader);
      const q = query.q || '', status = query.status || '', sort = query.sort || 'name', clubId = query.clubId || '';
      const data = await api.get('api/members', { q, clubId, status, sort });
      const items = data?.items || [];
      const clubs = data?.clubs || [];
      const title = isMember ? 'Hồ sơ của tôi' : isLeader ? 'Thành viên câu lạc bộ' : 'Quản lý thành viên';

      view.innerHTML = `<header class="page-header"><div><div class="page-eyebrow">Nhân sự CLB</div><h1 class="page-title">${esc(title)}</h1><p class="page-subtitle">Danh sách được giới hạn tự động theo vai trò và câu lạc bộ của tài khoản.</p></div>${isAdmin ? `<div class="page-actions"><a class="btn btn-primary btn-icon" href="#/members/new"><svg><use href="#i-plus"/></svg>Thêm thành viên</a></div>` : ''}</header>
<form class="surface filter-card"><div class="row g-2"><div class="col-lg-4"><input name="q" value="${esc(q)}" class="form-control" placeholder="Tìm tên hoặc MSSV"/></div>${isAdmin ? `<div class="col-lg-3"><select name="clubId" class="form-select"><option value="">Tất cả câu lạc bộ</option>${ui.options(clubs, 'id', 'clubName', clubId)}</select></div>` : ''}<div class="col-lg-2"><select name="status" class="form-select"><option value="">Mọi trạng thái</option><option value="ACTIVE"${status === 'ACTIVE' ? ' selected' : ''}>Đang hoạt động</option><option value="INACTIVE"${status === 'INACTIVE' ? ' selected' : ''}>Ngừng hoạt động</option></select></div><div class="col-lg-2"><select name="sort" class="form-select"><option value="name">Tên A-Z</option><option value="studentCode"${sort === 'studentCode' ? ' selected' : ''}>Theo MSSV</option><option value="newest"${sort === 'newest' ? ' selected' : ''}>Mới tham gia</option></select></div><div class="col"><button class="btn btn-primary w-100" type="submit">Lọc</button></div></div></form>
<section class="table-shell"><div class="section-heading"><div><h2>Danh sách thành viên</h2><p>${items.length} hồ sơ phù hợp</p></div></div><div class="table-responsive"><table class="table table-hover"><thead><tr><th>Thành viên</th><th>MSSV</th><th>Câu lạc bộ</th><th>Lớp</th><th>Trạng thái</th><th>Thao tác</th></tr></thead><tbody>${items.map(x => `<tr><td><strong>${esc(x.fullName)}</strong><br/><small class="text-muted">${esc(x.email)}</small></td><td><a href="#/members/${esc(x.id)}"><strong>${esc(x.studentCode)}</strong></a></td><td>${esc(x.club?.clubName)}</td><td>${esc(x.className)}</td><td><span class="status-badge ${x.memberStatus === 'ACTIVE' ? 'success' : 'warning'}">${esc(x.memberStatus)}</span></td><td><div class="d-flex flex-wrap gap-2"><a class="btn btn-sm btn-light" href="#/members/${esc(x.id)}">Chi tiết</a><a class="btn btn-sm btn-outline-primary" href="#/members/${esc(x.id)}/edit">Sửa</a>${!isMember ? `<button type="button" class="btn btn-sm btn-outline-primary" data-ai="${esc(x.id)}" title="AI đánh giá tham gia">AI đánh giá</button>` : ''}${isAdmin ? `<button type="button" class="btn btn-sm btn-light" data-toggle="${esc(x.id)}" data-status="${x.memberStatus === 'ACTIVE' ? 'INACTIVE' : 'ACTIVE'}">Đổi trạng thái</button>` : ''}</div></td></tr>`).join('')}</tbody></table></div>${items.length === 0 ? '<div class="empty-state">Không có thành viên phù hợp bộ lọc.</div>' : ''}</section>`;

      view.querySelector('form.filter-card').addEventListener('submit', e => {
        e.preventDefault();
        const f = e.target;
        router.go('/members', { q: f.q.value, clubId: f.clubId?.value, status: f.status.value, sort: f.sort.value });
      });
      view.querySelectorAll('[data-ai]').forEach(b => b.addEventListener('click', () => runParticipation(ctx, b.dataset.ai, b)));
      view.querySelectorAll('[data-toggle]').forEach(b => b.addEventListener('click', async () => {
        if (!ui.confirmAction('Xác nhận đổi trạng thái thành viên?')) return;
        try {
          await api.put(api.url(`api/members/${b.dataset.toggle}/status`, { status: b.dataset.status }));
        } catch (err) { ui.flash('danger', err?.message || 'Có lỗi xảy ra.'); }
        router.reload();
      }));
    },
  });

  // ---- Thêm mới (phải đăng ký trước /members/:id) ----
  router.add('/members/new', {
    nav: 'members', title: 'Thêm thành viên', roles: [Roles.Admin],
    async render({ view, api, ui }) {
      const lk = await api.get('api/members/lookups');
      view.innerHTML = `<h1>Thêm thành viên</h1><form class="card card-body"><div data-form-error class="text-danger"></div><div class="row g-3">
<div class="col-md-6"><label class="form-label" for="studentCode">MSSV</label><input id="studentCode" name="studentCode" class="form-control" required maxlength="30"/></div>
<div class="col-md-6"><label class="form-label" for="fullName">Họ tên</label><input id="fullName" name="fullName" class="form-control" required maxlength="160"/></div>
<div class="col-md-6"><label class="form-label" for="email">Email</label><input id="email" name="email" type="email" class="form-control" required maxlength="256"/></div>
<div class="col-md-6"><label class="form-label" for="clubId">Câu lạc bộ</label><select id="clubId" name="clubId" data-number class="form-select" required><option value="">-- Chọn câu lạc bộ --</option>${ui.options(lk?.clubs, 'id', 'clubName')}</select></div>
<div class="col-md-6"><label class="form-label" for="userId">Tài khoản đăng nhập</label><select id="userId" name="userId" data-number class="form-select"><option value="">-- Chưa liên kết tài khoản --</option>${ui.options(lk?.users, 'id', 'userName')}</select><div class="form-text">Bắt buộc liên kết nếu thành viên là Trưởng câu lạc bộ.</div></div>
<div class="col-md-4"><label class="form-label" for="className">Lớp</label><input id="className" name="className" data-nullable class="form-control" maxlength="60"/></div>
<div class="col-md-4"><label class="form-label" for="course">Khóa</label><input id="course" name="course" data-nullable class="form-control" maxlength="30"/></div>
<div class="col-md-4"><label class="form-label" for="phone">Điện thoại</label><input id="phone" name="phone" data-nullable type="tel" class="form-control" maxlength="30"/></div>
</div><button class="btn btn-primary mt-3" type="submit">Lưu</button></form>`;
      ui.onSubmit(view.querySelector('form'), async d => {
        await api.post('api/members', d);
        router.go('/members');
      });
    },
  });

  // ---- Chi tiết ----
  router.add('/members/:id', {
    nav: 'members', title: 'Chi tiết thành viên',
    async render(ctx) {
      const { view, params, user, api } = ctx;
      const m = await api.get(`api/members/${encodeURIComponent(params.id)}`);
      const isMember = isIn(user, Roles.Member);
      const skills = (m.skills || []).map(s => s.skillName).join(', ');
      view.innerHTML = `<div class="d-flex justify-content-between align-items-center mb-3"><h1>${esc(m.fullName)}</h1><div class="d-flex gap-2"><a class="btn btn-outline-primary" href="#/members/${esc(m.id)}/edit">Cập nhật hồ sơ</a>${!isMember ? `<button type="button" class="btn btn-primary btn-icon" data-ai><svg><use href="#i-spark"/></svg>AI đánh giá tham gia</button>` : ''}</div></div>
<div class="card card-body"><dl class="row mb-0"><dt class="col-sm-3">MSSV</dt><dd class="col-sm-9">${esc(m.studentCode)}</dd><dt class="col-sm-3">Email</dt><dd class="col-sm-9">${esc(m.email)}</dd><dt class="col-sm-3">Điện thoại</dt><dd class="col-sm-9">${esc(m.phone)}</dd><dt class="col-sm-3">Lớp / khóa</dt><dd class="col-sm-9">${esc(m.className)} / ${esc(m.course)}</dd><dt class="col-sm-3">Câu lạc bộ</dt><dd class="col-sm-9">${esc(m.club?.clubName)}</dd><dt class="col-sm-3">Kỹ năng</dt><dd class="col-sm-9">${esc(skills)}</dd></dl></div>`;
      const ai = view.querySelector('[data-ai]');
      if (ai) ai.addEventListener('click', () => runParticipation(ctx, m.id, ai));
    },
  });

  // ---- Sửa ----
  router.add('/members/:id/edit', {
    nav: 'members', title: 'Cập nhật hồ sơ thành viên',
    async render({ view, params, user, api, ui }) {
      const data = await api.get(`api/members/${encodeURIComponent(params.id)}/edit`);
      const m = data.member;
      const isAdmin = isIn(user, Roles.Admin), isMember = isIn(user, Roles.Member);
      view.innerHTML = `<h1>Cập nhật hồ sơ thành viên</h1><form class="card card-body"><div data-form-error class="text-danger"></div><div class="row g-3">
<div class="col-md-6"><label class="form-label" for="fullName">Họ tên</label><input id="fullName" name="fullName" class="form-control" required maxlength="160" value="${esc(m.fullName)}"/></div>
<div class="col-md-6"><label class="form-label" for="email">Email</label><input id="email" name="email" type="email" class="form-control" required value="${esc(m.email)}"/></div>
<div class="col-md-4"><label class="form-label" for="phone">Điện thoại</label><input id="phone" name="phone" data-nullable type="tel" class="form-control" value="${esc(m.phone)}"/></div>
<div class="col-md-4"><label class="form-label" for="className">Lớp</label><input id="className" name="className" data-nullable class="form-control" value="${esc(m.className)}"/></div>
<div class="col-md-4"><label class="form-label" for="course">Khóa</label><input id="course" name="course" data-nullable class="form-control" value="${esc(m.course)}"/></div>
${!isMember ? `<div class="col-md-6"><label class="form-label" for="position">Chức vụ</label><input id="position" name="position" data-nullable class="form-control" maxlength="80" value="${esc(m.position)}"/></div>` : ''}
${isAdmin ? `<div class="col-md-4"><label class="form-label" for="clubId">Câu lạc bộ</label><select id="clubId" name="clubId" data-number class="form-select" required>${ui.options(data.clubs, 'id', 'clubName', m.clubId)}</select></div>
<div class="col-md-4"><label class="form-label" for="userId">Tài khoản đăng nhập</label><select id="userId" name="userId" data-number class="form-select"><option value="">-- Chưa liên kết --</option>${ui.options(data.users, 'id', 'userName', m.userId)}</select></div>
<div class="col-md-4"><label class="form-label" for="memberStatus">Trạng thái</label><select id="memberStatus" name="memberStatus" class="form-select"><option value="ACTIVE"${m.memberStatus === 'ACTIVE' ? ' selected' : ''}>ACTIVE</option><option value="INACTIVE"${m.memberStatus === 'INACTIVE' ? ' selected' : ''}>INACTIVE</option></select></div>` : ''}
</div><button class="btn btn-primary mt-3" type="submit">Lưu thay đổi</button></form>`;
      ui.onSubmit(view.querySelector('form'), async d => {
        // Trường không hiển thị (ClubId/UserId/Position... với vai trò thấp) giữ nguyên giá trị hiện có.
        const body = { ...m, ...d, id: m.id };
        await api.put(`api/members/${encodeURIComponent(m.id)}`, body);
        ui.flash('success', 'Đã cập nhật hồ sơ thành viên.');
        router.go(`/members/${m.id}`);
      });
    },
  });
}
