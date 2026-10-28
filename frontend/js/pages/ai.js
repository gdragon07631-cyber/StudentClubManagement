import { esc } from '../ui.js';

export function register(router) {
  router.add('/ai', {
    nav: 'ai', title: 'Trợ lý CLB AI',
    async render({ view, api, ui }) {
      view.innerHTML = `<header class="page-header"><div><div class="page-eyebrow">Trợ lý AI Campus</div><h1 class="page-title">Chatbot CLB</h1><p class="page-subtitle">Tra cứu nhiệm vụ, hoạt động và dữ liệu bạn được phép xem. Chatbot không tự thay đổi dữ liệu nghiệp vụ.</p></div></header>
<div class="dashboard-grid">
  <section class="surface p-4">
    <form>
      <div data-form-error class="text-danger mb-3"></div>
      <label for="question">Bạn muốn hỏi điều gì?</label>
      <textarea id="question" name="question" required rows="6" class="form-control mb-3" placeholder="Ví dụ: Nhiệm vụ và điểm hoạt động của tôi?"></textarea>
      <button class="btn btn-primary btn-icon" type="submit"><svg><use href="#i-spark"/></svg>Gửi câu hỏi</button>
    </form>
    <div data-result></div>
  </section>
  <aside class="surface ai-panel">
    <h2>Nguyên tắc sử dụng</h2>
    <p>AI chỉ sử dụng dữ liệu trong phạm vi tài khoản hiện tại. AI không truy cập tài chính của thành viên, không tự duyệt đơn và không tự phân công.</p>
    <a class="btn" href="#/dashboard">Quay lại tổng quan</a>
  </aside>
</div>`;
      const resultBox = view.querySelector('[data-result]');
      ui.onSubmit(view.querySelector('form'), async d => {
        const res = await api.post('api/ai/chat', { question: d.question });
        resultBox.innerHTML = `<div class="calendar-item success mt-4"><span class="status-badge success">${esc(res?.function)}</span><p class="mt-3 text-dark" style="white-space:pre-wrap">${esc(res?.result)}</p></div>`;
      });
    },
  });
}

