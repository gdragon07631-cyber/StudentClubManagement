import { Roles } from '../api.js';

export function register(router) {
  router.add('/points', {
    nav: 'points', title: 'Điểm hoạt động',
    async render({ view, query, user, api, ui }) {
      const sort = query.sort || 'newest';
      const data = (await api.get('api/points', { sort })) || {};
      const items = data.items || [];
      const members = data.activeMembers || [];
      const rules = data.rules || [];
      const esc = ui.esc;
      const isAdmin = user.roles.includes(Roles.Admin);
      const total = items.reduce((s, x) => s + Number(x.points || 0), 0);
      // RecordedAt lưu UTC (không kèm múi giờ) -> đổi sang giờ địa phương như ToLocalTime() cũ.
      const local = v => ui.fmtDateTime(v && !/(Z|[+-]\d\d:?\d\d)$/.test(v) ? v + 'Z' : v);
      const n1 = v => new Intl.NumberFormat('vi-VN', { minimumFractionDigits: 1, maximumFractionDigits: 1 }).format(Number(v) || 0);

      view.innerHTML = `
      <header class="page-header">
        <div>
          <div class="page-eyebrow">Đánh giá &amp; Khen thưởng</div>
          <h1 class="page-title">Lịch sử điểm hoạt động</h1>
          <p class="page-subtitle">Điểm luôn được lưu theo lịch sử và nguồn; dữ liệu demo không phải công thức điểm chính thức.</p>
        </div>
      </header>
      <div class="d-flex justify-content-between align-items-center mb-3">
        <div class="alert alert-info py-2 px-3 mb-0 flex-grow-1 me-3">
          Tổng số: <strong>${items.length}</strong> bản ghi điểm | Tổng tích lũy: <strong>${esc(ui.fmtNumber(total))}</strong> điểm
        </div>
        <form class="d-flex gap-2" data-sort>
          <select name="sort" class="form-select form-select-sm">
            <option value="newest"${sort === 'newest' ? ' selected' : ''}>Mới nhất</option>
            <option value="points"${sort === 'points' ? ' selected' : ''}>Điểm cao nhất</option>
          </select>
        </form>
      </div>
      ${isAdmin ? `
      <details class="card card-body mb-3">
        <summary class="fw-bold text-primary" style="cursor:pointer">Ghi nhận điểm hoạt động mới</summary>
        <form class="row g-2 mt-2" data-record>
          <div data-form-error class="text-danger col-12"></div>
          <div class="col-md-4">
            <label class="form-label small">Thành viên</label>
            <select name="memberId" class="form-select form-select-sm" required>
              <option value="">-- Chọn thành viên --</option>
              ${members.map(m => `<option value="${esc(m.id)}">${esc(m.fullName)} (${esc(m.studentCode)}) - ${esc(m.clubName)}</option>`).join('')}
            </select>
          </div>
          <div class="col-md-3">
            <label class="form-label small">Quy tắc điểm</label>
            <select name="pointRuleId" class="form-select form-select-sm" required>
              <option value="">-- Chọn quy tắc --</option>
              ${rules.map(r => `<option value="${esc(r.id)}">${esc(r.ruleName)} (${esc(r.pointValue)} đ)</option>`).join('')}
            </select>
          </div>
          <div class="col-md-2">
            <label class="form-label small">Nguồn</label>
            <input name="sourceType" value="MANUAL" required maxlength="30" class="form-control form-control-sm" placeholder="VD: MANUAL" />
          </div>
          <input type="hidden" name="sourceId" value="0" data-number />
          <div class="col-md-3">
            <label class="form-label small">Số điểm</label>
            <input name="points" type="number" step="0.5" required class="form-control form-control-sm" placeholder="Số điểm" />
          </div>
          <div class="col-md-10">
            <label class="form-label small">Lý do ghi điểm</label>
            <input name="reason" required maxlength="500" class="form-control form-control-sm" placeholder="Nhập lý do ghi nhận điểm thưởng/phạt..." />
          </div>
          <div class="col-md-2 d-flex align-items-end">
            <button class="btn btn-sm btn-primary w-100" type="submit">Ghi nhận</button>
          </div>
        </form>
      </details>` : ''}
      <div class="table-shell">
        <div class="table-responsive">
          <table class="table table-hover">
            <thead><tr><th>Thành viên</th><th>Quy tắc</th><th>Nguồn</th><th>Điểm</th><th>Lý do</th><th>Thời gian</th></tr></thead>
            <tbody>
              ${items.map(x => `<tr>
                <td><strong>${esc(x.member?.fullName)}</strong><br/><small class="text-muted">${esc(x.member?.studentCode)}</small></td>
                <td>${esc(x.pointRule?.ruleName)}</td>
                <td><span class="badge bg-light text-dark">${esc(x.sourceType)} #${esc(x.sourceId)}</span></td>
                <td><strong class="${x.points >= 0 ? 'text-success' : 'text-danger'}">${x.points > 0 ? '+' : ''}${esc(n1(x.points))}</strong></td>
                <td>${esc(x.reason)}</td>
                <td>${esc(local(x.recordedAt))}</td>
              </tr>`).join('')}
            </tbody>
          </table>
        </div>
        ${items.length ? '' : '<div class="empty-state">Chưa có lịch sử điểm hoạt động nào.</div>'}
      </div>`;

      view.querySelector('[data-sort] select').addEventListener('change', e => router.go('/points', { sort: e.target.value }));
      view.querySelector('[data-sort]').addEventListener('submit', e => e.preventDefault());

      const form = view.querySelector('form[data-record]');
      if (form) {
        ui.onSubmit(form, async d => {
          await api.post('api/points', {
            memberId: Number(d.memberId), pointRuleId: Number(d.pointRuleId),
            sourceType: d.sourceType, sourceId: Number(d.sourceId) || 0,
            points: Number(d.points), reason: d.reason,
          });
          ui.flash('success', 'Đã ghi nhận điểm hoạt động.');
          router.reload();
        });
      }
    },
  });
}
