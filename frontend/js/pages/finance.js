import { Roles } from '../api.js';

export function register(router) {
  router.add('/finance', {
    nav: 'finance', title: 'Tài chính', roles: [Roles.Admin],
    async render({ view, query, api, ui }) {
      const sort = query.sort || 'newest';
      const type = query.type || '';
      const data = await api.get('api/finance', { from: query.from, to: query.to, type, sort });
      const list = data.transactions || [];
      const today = ui.toInputDate(new Date());
      view.innerHTML = `<h1>Tài chính</h1><div class="alert alert-primary"><strong>Số dư:</strong> ${ui.esc(ui.fmtNumber(data.balance))} đ</div>
      <form id="filter-form" class="card card-body mb-3"><div class="row g-2">
        <div class="col"><input type="date" name="from" value="${ui.esc(query.from || '')}" class="form-control"/></div>
        <div class="col"><input type="date" name="to" value="${ui.esc(query.to || '')}" class="form-control"/></div>
        <div class="col"><select name="type" class="form-select"><option value=""${type === '' ? ' selected' : ''}>Thu và chi</option><option value="INCOME"${type === 'INCOME' ? ' selected' : ''}>Thu</option><option value="EXPENSE"${type === 'EXPENSE' ? ' selected' : ''}>Chi</option></select></div>
        <div class="col"><select name="sort" class="form-select"><option value="newest"${sort !== 'amount' ? ' selected' : ''}>Ngày mới nhất</option><option value="amount"${sort === 'amount' ? ' selected' : ''}>Số tiền lớn nhất</option></select></div>
        <div class="col"><button class="btn btn-outline-primary" type="submit">Lọc báo cáo</button></div>
      </div></form>
      <details class="card card-body mb-3"><summary>Ghi nhận giao dịch</summary>
        <form id="create-form" class="row g-2 mt-2">
          <div data-form-error class="text-danger col-12"></div>
          <div class="col-md-2"><select name="transactionType" class="form-select"><option>INCOME</option><option>EXPENSE</option></select></div>
          <div class="col-md-2"><input name="category" required maxlength="100" class="form-control" placeholder="Danh mục"/></div>
          <div class="col-md-2"><input name="amount" type="number" min="0.01" step="0.01" required class="form-control" placeholder="Số tiền"/></div>
          <div class="col-md-2"><input name="transactionDate" type="date" required value="${ui.esc(today)}" class="form-control"/></div>
          <div class="col-md-3"><input name="description" maxlength="1000" class="form-control" placeholder="Mô tả"/></div>
          <div class="col"><button class="btn btn-primary" type="submit">Lưu</button></div>
        </form></details>
      <table class="table"><thead><tr><th>Ngày</th><th>Loại</th><th>Danh mục</th><th>Mô tả</th><th class="text-end">Số tiền</th></tr></thead>
      <tbody>${list.map(x => `<tr><td>${ui.esc(ui.fmtDate(x.transactionDate))}</td><td>${ui.esc(x.transactionType)}</td><td>${ui.esc(x.category)}</td><td>${ui.esc(x.description)}</td><td class="text-end">${ui.esc(ui.fmtNumber(x.amount))} đ</td></tr>`).join('')}</tbody></table>`;

      view.querySelector('#filter-form').addEventListener('submit', e => {
        e.preventDefault();
        const f = new FormData(e.target);
        router.go('/finance', { from: f.get('from'), to: f.get('to'), type: f.get('type'), sort: f.get('sort') });
      });
      ui.onSubmit(view.querySelector('#create-form'), async d => {
        if (!(d.amount > 0)) throw new Error('Giao dịch không hợp lệ; số tiền phải lớn hơn 0.');
        await api.post('api/finance', {
          transactionType: d.transactionType, category: d.category, amount: d.amount,
          description: d.description || null, transactionDate: d.transactionDate,
        });
        router.reload();
      });
    },
  });
}
