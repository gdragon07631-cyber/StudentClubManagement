import { Roles, session } from '../api.js';
import { esc } from '../ui.js';

export function register(router) {
  router.add('/users', {
    nav: 'users', title: 'Tài khoản và phân quyền', roles: [Roles.Admin],
    async render({ view, api, ui }) {
      const data = await api.get('api/users');
      const accounts = data?.accounts || [];
      const members = data?.members || [];
      const currentRoles = data?.currentRoles || {};

      view.innerHTML = `<div class="d-flex justify-content-between"><h1>Tài khoản và phân quyền</h1><a class="btn btn-primary" href="#/users/new">Tạo tài khoản</a></div>
<p class="text-muted mt-3">Khi cấp quyền Trưởng câu lạc bộ, chọn hồ sơ thành viên thuộc CLB được quản lý. Người dùng đăng nhập lại sau khi đổi quyền.</p>
<table class="table mt-3"><thead><tr><th>Username</th><th>Email</th><th>Trạng thái</th><th>Đặt vai trò</th><th></th></tr></thead><tbody>
${accounts.map(x => {
  const role = currentRoles[x.id] || '';
  const availMembers = members.filter(m => m.userId === x.id || m.userId == null);
  return `<tr>
  <td>${esc(x.userName)}</td>
  <td>${esc(x.email)}</td>
  <td><span class="badge ${x.status === 'ACTIVE' ? 'bg-success' : 'bg-secondary'}">${esc(x.status)}</span></td>
  <td><form class="d-flex gap-2" data-role-form="${esc(x.id)}">
    <select name="role" class="form-select form-select-sm" required>
      <option value="" disabled${role === '' ? ' selected' : ''}>Chưa có vai trò</option>
      <option value="ADMIN"${role === Roles.Admin ? ' selected' : ''}>Admin</option>
      <option value="CLUB_LEADER"${role === Roles.ClubLeader ? ' selected' : ''}>Trưởng câu lạc bộ</option>
      <option value="MEMBER"${role === Roles.Member ? ' selected' : ''}>Thành viên</option>
    </select>
    <select name="memberId" data-number class="form-select form-select-sm" aria-label="Hồ sơ và câu lạc bộ quản lý">
      <option value="">-- Chọn hồ sơ / CLB --</option>
      ${availMembers.map(m => `<option value="${esc(m.id)}"${m.userId === x.id ? ' selected' : ''}>${esc(m.fullName)} (${esc(m.studentCode)}) — ${esc(m.club?.clubName)}</option>`).join('')}
    </select>
    <button class="btn btn-sm btn-primary" type="submit">Lưu</button>
  </form></td>
  <td><button type="button" class="btn btn-sm btn-outline-secondary" data-toggle="${esc(x.id)}">Khóa/Mở</button></td>
</tr>`;
}).join('')}
</tbody></table>`;

      view.querySelectorAll('[data-role-form]').forEach(form => {
        form.addEventListener('submit', async e => {
          e.preventDefault();
          const id = form.dataset.roleForm;
          const d = ui.formToObject(form);
          try {
            const res = await api.post(`api/users/${encodeURIComponent(id)}/role`, { role: d.role, memberId: d.memberId });
            if (res?.isCurrentUser) {
              session.clear();
              ui.flash('success', 'Đã lưu vai trò. Vui lòng đăng nhập lại để sử dụng quyền mới.');
              router.go('/login');
              return;
            }
            ui.flash('success', 'Đã lưu vai trò. Người dùng cần đăng nhập lại để sử dụng quyền mới.');
          } catch (err) {
            ui.flash('danger', err?.message || 'Có lỗi xảy ra.');
          }
          router.reload();
        });
      });

      view.querySelectorAll('[data-toggle]').forEach(btn => {
        btn.addEventListener('click', async () => {
          if (!ui.confirmAction('Xác nhận đổi trạng thái tài khoản?')) return;
          try {
            await api.post(`api/users/${encodeURIComponent(btn.dataset.toggle)}/toggle-status`);
          } catch (err) {
            ui.flash('danger', err?.message || 'Có lỗi xảy ra.');
          }
          router.reload();
        });
      });
    },
  });

  router.add('/users/new', {
    nav: 'users', title: 'Tạo tài khoản', roles: [Roles.Admin],
    async render({ view, api, ui }) {
      view.innerHTML = `<h1>Tạo tài khoản</h1><form class="card card-body col-lg-7">
  <div data-form-error class="text-danger mb-3"></div>
  <label for="username">Tên đăng nhập</label><input id="username" name="username" class="form-control mb-2" required maxlength="50"/>
  <label for="email">Email</label><input id="email" name="email" type="email" class="form-control mb-2" required/>
  <label for="password">Mật khẩu tạm thời</label><input id="password" name="password" type="password" class="form-control mb-2" required minlength="8"/>
  <label for="role">Vai trò</label>
  <select id="role" name="role" class="form-select mb-3">
    <option value="ADMIN">Admin</option>
    <option value="CLUB_LEADER">Trưởng câu lạc bộ</option>
    <option value="MEMBER" selected>Thành viên</option>
  </select>
  <p class="form-text">Tài khoản Trưởng câu lạc bộ cần được liên kết với hồ sơ sinh viên thuộc đúng câu lạc bộ.</p>
  <button class="btn btn-primary" type="submit">Tạo tài khoản</button>
</form>`;
      ui.onSubmit(view.querySelector('form'), async d => {
        await api.post('api/users', d);
        router.go('/users');
      });
    },
  });
}

