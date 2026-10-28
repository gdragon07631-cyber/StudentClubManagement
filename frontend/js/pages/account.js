import { session } from '../api.js';

export function register(router) {
  router.add('/login', {
    public: true, title: 'Đăng nhập',
    async render({ view, query, api, ui }) {
      if (session.get()) { router.go('/dashboard'); return; }
      view.innerHTML = `<div class="row justify-content-center"><div class="col-md-5 col-lg-4"><div class="card shadow-sm"><div class="card-body p-4">
        <h1 class="h3 mb-3">Đăng nhập</h1>
        <form novalidate>
          <div data-form-error class="text-danger mb-3"></div>
          <div class="mb-3"><label class="form-label" for="login">Email hoặc MSSV</label><input id="login" name="login" class="form-control" required autocomplete="username"/></div>
          <div class="mb-3"><label class="form-label" for="password">Mật khẩu</label><input id="password" name="password" type="password" class="form-control" required autocomplete="current-password"/></div>
          <button class="btn btn-primary w-100" type="submit">Đăng nhập</button>
        </form><hr/><a href="#/apply">Đăng ký tham gia CLB</a></div></div></div></div>`;
      const form = view.querySelector('form');
      form.removeAttribute('novalidate');
      ui.onSubmit(form, async data => {
        const res = await api.post('api/auth/login', { login: data.login, password: data.password });
        session.set(res);
        const back = query.returnUrl && query.returnUrl.startsWith('/') ? query.returnUrl : '/dashboard';
        router.go(back);
      });
    },
  });

  router.add('/change-password', {
    nav: 'change-password', title: 'Đổi mật khẩu',
    async render({ view, api, ui }) {
      view.innerHTML = ui.pageHeader({ title: 'Đổi mật khẩu' }) + `<form class="surface card-body col-xl-6">
        <div data-form-error class="text-danger mb-3"></div>
        <div class="mb-3"><label class="form-label">Mật khẩu hiện tại</label><input name="currentPassword" type="password" class="form-control" required autocomplete="current-password"/></div>
        <div class="mb-3"><label class="form-label">Mật khẩu mới</label><input name="newPassword" type="password" class="form-control" minlength="8" required autocomplete="new-password"/></div>
        <div class="mb-3"><label class="form-label">Nhập lại mật khẩu</label><input name="confirmPassword" type="password" class="form-control" required autocomplete="new-password"/></div>
        <button class="btn btn-primary" type="submit">Đổi mật khẩu</button></form>`;
      ui.onSubmit(view.querySelector('form'), async d => {
        if (d.newPassword !== d.confirmPassword) throw new Error('Mật khẩu nhập lại không khớp.');
        await api.post('api/auth/change-password', { currentPassword: d.currentPassword, newPassword: d.newPassword });
        ui.flash('success', 'Đã đổi mật khẩu.');
        router.go('/dashboard');
      });
    },
  });

  router.add('/access-denied', {
    public: true, title: 'Không có quyền',
    async render({ view }) { view.innerHTML = `<div class="surface empty-state"><h1 class="h4">Không có quyền truy cập</h1><a class="btn btn-primary" href="#/">Về trang chính</a></div>`; },
  });
}

