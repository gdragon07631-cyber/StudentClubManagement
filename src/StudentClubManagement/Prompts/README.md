# AI prompts

Prompt hệ thống được quản lý ở backend. AI chỉ nhận phần dữ liệu đã được lọc theo quyền, không nhận toàn bộ cơ sở dữ liệu và không được thực hiện quyết định nghiệp vụ.

- Chatbot: chỉ trả lời từ ngữ cảnh được cấp; thiếu dữ liệu phải nói rõ.
- Gợi ý phân công: ID phải thuộc danh sách ứng viên backend cung cấp; kết quả không tự ghi vào `task_assignments`.
- Đánh giá tham gia: chỉ cung cấp phân tích và căn cứ, không trừ điểm hoặc thay đổi trạng thái.
