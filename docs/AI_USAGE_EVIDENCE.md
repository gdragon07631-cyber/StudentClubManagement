# Minh chứng sử dụng AI khi lập trình

Tài liệu này phục vụ tiêu chí 9 của Bài kiểm tra thường xuyên 2. AI được dùng như công cụ hỗ trợ phân tích và lập trình; nhóm vẫn kiểm tra, build, test và chịu trách nhiệm về mã nguồn cuối cùng.

## Phạm vi AI đã hỗ trợ

| Công việc | Yêu cầu/prompt tiêu biểu | Kết quả được kiểm tra |
|---|---|---|
| Phân tích yêu cầu | Đối chiếu Master Prompt với 16 user story và các entity bắt buộc | Không thêm actor/module ngoài báo cáo; các điểm chưa chốt có `TODO(BUSINESS_RULE_CONFIRMATION)` |
| Thiết kế dữ liệu | Tạo quan hệ, unique index và check constraint bằng EF Core SQL Server | Migration `InitialSqlServer` được tạo và áp dụng trên SQL Server LocalDB; migration thực tế giúp phát hiện và loại bỏ multiple cascade paths |
| Authentication/authorization | Thiết kế Identity với `ADMIN`, `DEPARTMENT_LEADER`, `MEMBER` và kiểm tra scope backend | Controller dùng `[Authorize]`; `AccessScopeService` kiểm tra thành viên/bộ phận |
| CRUD và giao diện | Rà soát các màn hình MVC/Razor theo 10 tiêu chí của bài kiểm tra | Bổ sung Create/Edit, xử lý trạng thái, tìm kiếm/lọc/sắp xếp và xác nhận thao tác nguy hiểm |
| Kiểm thử | Sinh test cho đăng ký trùng, progress, số dư, duyệt đơn và seed | Test được chạy bằng `dotnet test`; kết quả cuối phải đạt 100% |
| Tài liệu | Soạn hướng dẫn restore/build/run/migration/demo | Lệnh được đối chiếu trực tiếp với solution và project hiện tại |

## Vị trí minh chứng trong mã nguồn

- `Services/ApplicationServices.cs`: luồng nghiệp vụ và AI có kiểm tra dữ liệu đầu vào/đầu ra.
- `Prompts/README.md`: nguyên tắc prompt và giới hạn AI.
- `Controllers/ModulesControllers.cs`: authorization phía server và quyết định phân công cuối cùng do người quản lý thực hiện.
- `tests/StudentClubManagement.Tests/ClubServiceTests.cs`: kiểm thử kết quả thay vì chấp nhận mã do AI tạo mà không xác minh.
- `Migrations/`: schema thực tế do EF Core sinh và đã được kiểm tra trên SQL Server LocalDB.

## Nguyên tắc an toàn

- Không đưa API key hoặc mật khẩu thật vào prompt/source.
- Không dùng AI để tự duyệt đơn, tự phân công, tự đổi điểm, trạng thái hoặc giao dịch.
- AI gợi ý phân công chỉ chọn ID do backend cung cấp; người quản lý phải bấm chấp nhận.
- Mọi thay đổi đều phải qua build, test và kiểm tra database.

Khi nộp bài, nhóm có thể bổ sung ảnh chụp lịch sử trao đổi với công cụ AI vào phụ lục nếu giảng viên yêu cầu bằng chứng hình ảnh.
