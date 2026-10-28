// Tiện ích giao diện dùng chung cho mọi trang.

/** Escape HTML — LUÔN dùng khi chèn dữ liệu người dùng vào innerHTML. */
export function esc(value) {
  return String(value ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
}

// API trả DateTime không kèm múi giờ (giờ địa phương) nên new Date() đọc đúng như nhập.
const asDate = v => { if (!v) return null; const d = new Date(v); return isNaN(d) ? null : d; };
const pad = n => String(n).padStart(2, '0');

export const fmtDate = v => { const d = asDate(v); return d ? `${pad(d.getDate())}/${pad(d.getMonth() + 1)}/${d.getFullYear()}` : ''; };
export const fmtDateTime = v => { const d = asDate(v); return d ? `${pad(d.getDate())}/${pad(d.getMonth() + 1)}/${d.getFullYear()} ${pad(d.getHours())}:${pad(d.getMinutes())}` : ''; };
export const fmtMoney = v => `${new Intl.NumberFormat('vi-VN').format(Number(v) || 0)} đ`;
export const fmtNumber = v => new Intl.NumberFormat('vi-VN').format(Number(v) || 0);
/** Giá trị cho <input type="datetime-local">. */
export const toInputDateTime = v => { const d = asDate(v); return d ? `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}` : ''; };
/** Giá trị cho <input type="date">. */
export const toInputDate = v => { const d = asDate(v); return d ? `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}` : ''; };

/** Thông báo flash: giữ qua một lần chuyển trang (thay cho TempData). */
export function flash(type, message) { sessionStorage.setItem('scm.flash', JSON.stringify({ type, message })); }
export function takeFlash() {
  const raw = sessionStorage.getItem('scm.flash'); if (!raw) return null;
  sessionStorage.removeItem('scm.flash');
  try { return JSON.parse(raw); } catch { return null; }
}

export const alertBox = (type, message) => `<div class="alert alert-${esc(type)} app-alert">${esc(message)}</div>`;

/** Đọc form thành object; checkbox -> boolean; ô số rỗng -> null khi có data-number. */
export function formToObject(form) {
  const out = {};
  for (const el of form.elements) {
    if (!el.name || el.disabled) continue;
    if (el.type === 'checkbox') out[el.name] = el.checked;
    else if (el.type === 'radio') { if (el.checked) out[el.name] = el.value; }
    else if (el.type === 'number' || el.dataset.number !== undefined) out[el.name] = el.value === '' ? null : Number(el.value);
    else out[el.name] = el.value === '' && el.dataset.nullable !== undefined ? null : el.value;
  }
  return out;
}

/** Hiện/ẩn lỗi trong phần tử [data-form-error] của form. */
export function setFormError(form, message) {
  let box = form.querySelector('[data-form-error]');
  if (!box) { box = document.createElement('div'); box.className = 'text-danger mb-3'; box.dataset.formError = ''; form.prepend(box); }
  box.textContent = message || '';
}

/** Gắn xử lý submit: tự khóa nút, bắt lỗi API và hiện vào form. handler(data, form) có thể ném ApiError. */
export function onSubmit(form, handler) {
  form.addEventListener('submit', async e => {
    e.preventDefault();
    if (!form.reportValidity()) return;
    const btn = form.querySelector('[type=submit],button:not([type])');
    if (btn) btn.disabled = true;
    setFormError(form, '');
    try { await handler(formToObject(form), form); }
    catch (err) { setFormError(form, err?.message || 'Có lỗi xảy ra.'); }
    finally { if (btn) btn.disabled = false; }
  });
}

export function badge(status) {
  const map = { ACTIVE: 'success', APPROVED: 'success', COMPLETED: 'success', DONE: 'success', PRESENT: 'success', INCOME: 'success',
    PENDING: 'warning', PLANNED: 'warning', IN_PROGRESS: 'info', INACTIVE: 'secondary', REJECTED: 'danger', CANCELLED: 'danger', ABSENT: 'danger', EXPENSE: 'danger' };
  return `<span class="badge text-bg-${map[status] || 'secondary'}">${esc(status)}</span>`;
}

/** <option> list. items: mảng; value/label: hàm hoặc tên thuộc tính; selected: giá trị đang chọn. */
export function options(items, value, label, selected, placeholder) {
  const v = typeof value === 'function' ? value : x => x[value];
  const l = typeof label === 'function' ? label : x => x[label];
  const head = placeholder === undefined ? '' : `<option value="">${esc(placeholder)}</option>`;
  return head + (items || []).map(x => `<option value="${esc(v(x))}"${String(v(x)) === String(selected ?? '') ? ' selected' : ''}>${esc(l(x))}</option>`).join('');
}

export const emptyState = text => `<div class="surface empty-state">${esc(text)}</div>`;

export function pageHeader({ eyebrow, title, subtitle, actionsHtml } = {}) {
  return `<header class="page-header"><div>${eyebrow ? `<div class="page-eyebrow">${esc(eyebrow)}</div>` : ''}<h1 class="page-title">${esc(title)}</h1>${subtitle ? `<p class="page-subtitle">${esc(subtitle)}</p>` : ''}</div>${actionsHtml ? `<div class="page-actions">${actionsHtml}</div>` : ''}</header>`;
}

/** Hộp xác nhận đơn giản. */
export const confirmAction = message => window.confirm(message);
