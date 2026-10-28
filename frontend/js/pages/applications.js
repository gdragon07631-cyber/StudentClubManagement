import { Roles } from '../api.js';
import { esc, fmtDate } from '../ui.js';

const isIn = (user, role) => !!user?.roles?.includes(role);

export function register(router) {
  // ---- Đăng ký công khai ----
  router.add('/apply', {
    public: true, title: 'Đăng ký tham gia câu lạc bộ',
    async render({ view, api, ui }) {
      const clubs = (await api.get('api/membership-applications/clubs')) || [];
      const maxYear = new Date().getFullYear();
      view.innerHTML = `<div class="d-flex justify-content-between align-items-center mb-3"><h1>Đăng ký tham gia câu lạc bộ</h1><a class="btn btn-outline-primary" href="#/apply/status">Tra cứu trạng thái đơn</a></div>
<form class="card card-body"><div data-form-error class="text-danger mb-3"></div><div class="row g-3">
  <div class="col-12"><label for="clubId">Câu lạc bộ</label><select id="clubId" name="clubId" data-number class="form-select" required><option value="">-- Chọn câu lạc bộ --</option>${ui.options(clubs, 'id', 'clubName')}</select></div>
  <div class="col-md-6"><label for="studentCode">MSSV</label><input id="studentCode" name="studentCode" class="form-control" required maxlength="30"/></div>
  <div class="col-md-6"><label for="fullName">Họ tên</label><input id="fullName" name="fullName" class="form-control" required maxlength="160"/></div>
  <div class="col-md-6"><label for="email">Email</label><input id="email" name="email" type="email" class="form-control" required maxlength="256"/></div>
  <div class="col-md-6"><label for="phone">Điện thoại</label><input id="phone" name="phone" data-nullable type="tel" class="form-control" maxlength="30"/></div>
  <div class="col-md-6"><label for="className">Lớp</label><input id="className" name="className" data-nullable class="form-control" maxlength="60"/></div>
  <div class="col-md-6"><label for="course">Khóa</label><input id="course" name="course" data-nullable class="form-control" maxlength="30"/></div>
  <div class="col-md-6"><label for="birthYear">Năm sinh</label><input id="birthYear" name="birthYear" type="number" min="1900" max="${maxYear}" class="form-control" required/></div>
  <div class="col-12"><label for="interests">Sở thích</label><textarea id="interests" name="interests" class="form-control" rows="3" maxlength="1000" required></textarea></div>
  <div class="col-12"><label for="joinReason">Lý do chọn câu lạc bộ</label><textarea id="joinReason" name="joinReason" class="form-control" rows="3" maxlength="2000" required></textarea></div>
  <div class="col-12"><label for="expectations">Mong muốn khi vào câu lạc bộ</label><textarea id="expectations" name="expectations" class="form-control" rows="3" maxlength="2000" required></textarea></div>
</div><button class="btn btn-primary mt-3" type="submit">Gửi đăng ký</button></form>`;
      ui.onSubmit(view.querySelector('form'), async d => {
        await api.post('api/membership-applications', d);
        router.go('/apply/status', { code: d.studentCode });
      });
    },
  });

  // ---- Tra cứu trạng thái đơn công khai ----
  router.add('/apply/status', {
    public: true, title: 'Trạng thái đăng ký',
    async render({ view, query, api }) {
      const code = query.code || '';
      const data = code ? await api.get('api/membership-applications/status', { code }) : null;
      const app = data?.application;
      view.innerHTML = `<h1>Trạng thái đăng ký</h1>
<form class="surface filter-card mb-3"><div class="row g-2"><div class="col-md-8"><input name="code" value="${esc(code)}" class="form-control" placeholder="Nhập MSSV để tra cứu..." required/></div><div class="col-md-4"><button class="btn btn-primary w-100" type="submit">Tra cứu</button></div></div></form>
${!code ? '' : !app ? '<div class="alert alert-warning">Không tìm thấy đơn đăng ký.</div>' : `<div class="card card-body"><h2 class="h4">${esc(app.fullName)}</h2><p>MSSV: ${esc(app.studentCode)}</p><p>Trạng thái: <strong>${esc(app.status)}</strong></p>${app.status === 'APPROVED' ? '<p>Đơn đã được duyệt. Vui lòng liên hệ quản trị viên hoặc trưởng ban để nhận tài khoản và mật khẩu đăng nhập.</p>' : ''}${app.status === 'REJECTED' ? `<p>Lý do: ${esc(app.rejectionReason)}</p>` : ''}</div>`}`;
      view.querySelector('form').addEventListener('submit', e => {
        e.preventDefault();
        router.go('/apply/status', { code: e.target.code.value });
      });
    },
  });

  // ---- Danh sách xét duyệt đơn (Admin / ClubLeader) ----
  router.add('/applications', {
    nav: 'applications', title: 'Xét duyệt đơn đăng ký', roles: [Roles.Admin, Roles.ClubLeader],
    async render({ view, query, user, api, ui }) {
      const isAdmin = isIn(user, Roles.Admin);
      const q = query.q || '', clubId = query.clubId || '', status = query.status || '', sort = query.sort || 'newest';
      const data = await api.get('api/membership-applications', { q, clubId, status, sort });
      const items = data?.items || [];
      const clubs = data?.clubs || [];
      const total = items.length;
      const pending = items.filter(x => x.status === 'PENDING').length;
      const approved = items.filter(x => x.status === 'APPROVED').length;
      const rejected = items.filter(x => x.status === 'REJECTED').length;

      view.innerHTML = `<header class="page-header"><div><div class="page-eyebrow">Quản lý câu lạc bộ · Đơn đăng ký</div><h1 class="page-title">Xét duyệt đơn đăng ký thành viên</h1><p class="page-subtitle">Admin xem tất cả; Trưởng câu lạc bộ chỉ xử lý đơn thuộc câu lạc bộ của mình.</p></div></header>
<section class="metric-grid">
  <div class="metric-card surface"><span class="metric-icon"><svg><use href="#i-clipboard"/></svg></span><div class="metric-label">Tổng số đơn</div><div class="metric-value">${total} <small>hồ sơ</small></div></div>
  <div class="metric-card surface"><span class="metric-icon"><svg><use href="#i-calendar"/></svg></span><div class="metric-label">Chờ xét duyệt</div><div class="metric-value">${pending} <small>đang chờ</small></div></div>
  <div class="metric-card surface"><span class="metric-icon"><svg><use href="#i-check"/></svg></span><div class="metric-label">Đã phê duyệt</div><div class="metric-value">${approved} <small>chính thức</small></div></div>
  <div class="metric-card surface"><span class="metric-icon"><svg><use href="#i-user"/></svg></span><div class="metric-label">Đã từ chối</div><div class="metric-value">${rejected} <small>không đạt</small></div></div>
</section>
<form class="surface filter-card"><div class="row g-2"><div class="col-lg-4"><input name="q" value="${esc(q)}" class="form-control" placeholder="Tìm theo họ tên, MSSV hoặc email"/></div>${isAdmin ? `<div class="col-lg-3"><select name="clubId" class="form-select"><option value="">Tất cả câu lạc bộ</option>${ui.options(clubs, 'id', 'clubName', clubId)}</select></div>` : ''}<div class="col-lg-2"><select name="status" class="form-select"><option value="">Mọi trạng thái</option><option value="PENDING"${status === 'PENDING' ? ' selected' : ''}>Chờ duyệt</option><option value="APPROVED"${status === 'APPROVED' ? ' selected' : ''}>Đã duyệt</option><option value="REJECTED"${status === 'REJECTED' ? ' selected' : ''}>Đã từ chối</option></select></div><div class="col-lg-2"><select name="sort" class="form-select"><option value="newest">Mới nhất</option><option value="name"${sort === 'name' ? ' selected' : ''}>Tên A-Z</option></select></div><div class="col"><button class="btn btn-primary w-100" type="submit">Lọc</button></div></div></form>
<section class="table-shell"><div class="section-heading"><div><h2>Danh sách nộp đơn</h2><p>${total} hồ sơ phù hợp</p></div></div><div class="table-responsive"><table class="table table-hover"><thead><tr><th>Ứng viên</th><th>Câu lạc bộ</th><th>MSSV &amp; lớp</th><th>Ngày nộp</th><th>Trạng thái</th><th>Hành động</th></tr></thead><tbody>
${items.map(x => {
  const badge = x.status === 'APPROVED' ? 'success' : x.status === 'REJECTED' ? 'danger' : 'warning';
  return `<tr>
  <td><strong>${esc(x.fullName)}</strong><br/><small class="text-muted">${esc(x.email)}</small><details class="mt-2"><summary>Thông tin đăng ký</summary><dl><dt>Năm sinh</dt><dd>${esc(x.birthYear)}</dd><dt>Sở thích</dt><dd style="white-space:pre-wrap">${esc(x.interests)}</dd><dt>Lý do chọn câu lạc bộ</dt><dd style="white-space:pre-wrap">${esc(x.joinReason)}</dd><dt>Mong muốn khi vào câu lạc bộ</dt><dd style="white-space:pre-wrap">${esc(x.expectations)}</dd></dl></details></td>
  <td>${esc(x.club?.clubName)}</td>
  <td><strong>${esc(x.studentCode)}</strong><br/><small class="text-muted">${esc(x.className)} · ${esc(x.course)}</small></td>
  <td>${fmtDate(x.appliedAt)}</td>
  <td><span class="status-badge ${badge}">${esc(x.status)}</span></td>
  <td>${x.status === 'PENDING' ? `<div class="d-flex flex-wrap gap-2"><button type="button" class="btn btn-primary btn-sm" data-approve="${esc(x.id)}">Duyệt đơn</button><form class="d-flex gap-1" data-reject="${esc(x.id)}"><input name="reason" required placeholder="Lý do từ chối" class="form-control form-control-sm"/><button class="btn btn-outline-danger btn-sm" type="submit">Từ chối</button></form></div>` : x.status === 'APPROVED' ? `<a href="#/applications/${esc(x.id)}/issue-account" class="btn btn-outline-primary btn-sm">Cấp / xem tài khoản</a>` : '<span class="text-muted small">Đã xử lý</span>'}</td>
</tr>`;
}).join('')}
</tbody></table></div>${items.length === 0 ? '<div class="empty-state">Không có đơn đăng ký phù hợp.</div>' : ''}</section>`;

      view.querySelector('form.filter-card').addEventListener('submit', e => {
        e.preventDefault();
        const f = e.target;
        router.go('/applications', { q: f.q.value, clubId: f.clubId?.value, status: f.status.value, sort: f.sort.value });
      });

      view.querySelectorAll('[data-approve]').forEach(btn => {
        btn.addEventListener('click', async () => {
          btn.disabled = true;
          try {
            await api.post(`api/membership-applications/${encodeURIComponent(btn.dataset.approve)}/approve`);
            ui.flash('success', 'Đã duyệt đơn và tạo hồ sơ thành viên. Chọn "Cấp tài khoản" để tạo tài khoản đăng nhập cho sinh viên.');
          } catch (err) {
            ui.flash('danger', err?.message || 'Có lỗi xảy ra.');
          }
          router.reload();
        });
      });

      view.querySelectorAll('[data-reject]').forEach(form => {
        form.addEventListener('submit', async e => {
          e.preventDefault();
          const reason = form.reason.value;
          try {
            await api.post(`api/membership-applications/${encodeURIComponent(form.dataset.reject)}/reject`, { reason });
          } catch (err) {
            ui.flash('danger', err?.message || 'Có lỗi xảy ra.');
          }
          router.reload();
        });
      });
    },
  });

  // ---- Cấp tài khoản ----
  router.add('/applications/:id/issue-account', {
    nav: 'applications', title: 'Cấp tài khoản sinh viên', roles: [Roles.Admin, Roles.ClubLeader],
    async render({ view, params, api, ui }) {
      const m = await api.get(`api/membership-applications/${encodeURIComponent(params.id)}/issue-account`);
      view.innerHTML = `<h1>Cấp tài khoản sinh viên</h1>
<p>Thành viên: <strong>${esc(m.fullName)}</strong></p>
${m.existingUsername ? `<div class="alert alert-success">Đã cấp tài khoản: <strong>${esc(m.existingUsername)}</strong>. Mật khẩu không được hiển thị lại.</div>` : `<form class="card card-body">
  <div data-form-error class="text-danger mb-3"></div>
  <div class="mb-3"><label for="username">Tên đăng nhập</label><input id="username" name="username" class="form-control" required maxlength="50" autocomplete="off" value="${esc(m.username)}"/></div>
  <div class="mb-3"><label for="password">Mật khẩu</label><input id="password" name="password" type="password" class="form-control" required minlength="8" autocomplete="new-password"/></div>
  <div class="mb-3"><label for="confirmPassword">Nhập lại mật khẩu</label><input id="confirmPassword" name="confirmPassword" type="password" class="form-control" required autocomplete="new-password"/></div>
  <p>Mật khẩu có ít nhất 8 ký tự, gồm chữ hoa, chữ thường và chữ số. Hãy ghi nhớ mật khẩu để bàn giao trực tiếp cho sinh viên.</p>
  <button class="btn btn-primary" type="submit">Cấp tài khoản thành viên</button>
</form>`}
<a href="#/applications" class="btn btn-link mt-3">Quay lại danh sách đơn</a>`;

      const form = view.querySelector('form');
      if (form) {
        ui.onSubmit(form, async d => {
          if (d.password !== d.confirmPassword) throw new Error('Mật khẩu nhập lại không khớp.');
          await api.post(`api/membership-applications/${encodeURIComponent(m.id)}/issue-account`, {
            id: m.id, fullName: m.fullName, username: d.username, password: d.password, confirmPassword: d.confirmPassword,
          });
          ui.flash('success', `Đã cấp tài khoản ${d.username.trim()} cho sinh viên.`);
          router.go('/applications');
        });
      }
    },
  });
}

