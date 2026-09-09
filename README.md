# Hệ thống quản lý câu lạc bộ sinh viên có tích hợp AI

Đồ án Nhóm 15 xây dựng bằng ASP.NET Core MVC, C#, Razor Views, Entity Framework Core, ASP.NET Core Identity và SQL Server. Phiên bản hiện tại quản lý một câu lạc bộ.

## Chức năng

Hệ thống triển khai đúng phạm vi: xác thực và phân quyền; quản lý tài khoản; đăng ký tham gia CLB; thành viên; ban; hoạt động; đăng ký và điểm danh hoạt động; nhiệm vụ; cuộc họp và điểm danh; quy tắc/lịch sử điểm; thu chi; thống kê báo cáo; thông báo; chatbot; AI gợi ý phân công; AI đánh giá mức tham gia.

AI chỉ phân tích và gợi ý. AI không duyệt đơn, tự phân công, thay đổi điểm/trạng thái/vai trò hoặc ghi giao dịch tài chính.

## Actor

- Admin/Ban chủ nhiệm: quản trị toàn hệ thống, bao gồm tài chính.
- Trưởng ban: dữ liệu nhân sự, nhiệm vụ, báo cáo và AI trong phạm vi ban.
- Thành viên: dữ liệu cá nhân, hoạt động, cuộc họp, nhiệm vụ, điểm, thông báo và chatbot.
- Sinh viên đăng ký: gửi biểu mẫu và tra cứu trạng thái, chưa phải tài khoản Thành viên.

## Yêu cầu môi trường

- .NET SDK 10.0
- Trình duyệt Chrome, Firefox hoặc Edge
- SQL Server LocalDB (hoặc một SQL Server tương thích và connection string thay thế).

## Cài đặt và chạy development

```powershell
$env:DOTNET_CLI_HOME = Join-Path (Get-Location) '.dotnet-cli'
dotnet restore StudentClubManagement.slnx
dotnet run --project src/StudentClubManagement/StudentClubManagement.csproj
```

Lần chạy đầu ứng dụng tự áp dụng migration và tạo seed demo. Connection string nằm trong `appsettings.json`; có thể ghi đè bằng `ConnectionStrings__DefaultConnection`.

## Database và migration

```powershell
dotnet tool install dotnet-ef --version 10.0.11 --tool-path .tools
./.tools/dotnet-ef migrations add TenMigration --project src/StudentClubManagement --startup-project src/StudentClubManagement
./.tools/dotnet-ef database update --project src/StudentClubManagement --startup-project src/StudentClubManagement
```

Migration `InitialSqlServer` bao gồm foreign key, unique constraint, index và check constraint cho số tiền, tiến độ và thời gian hoạt động.

## Build và test

```powershell
dotnet build StudentClubManagement.slnx
dotnet test StudentClubManagement.slnx
# Khi ứng dụng đang chạy, kiểm chứng CRUD thành viên end-to-end:
powershell -ExecutionPolicy Bypass -File tests/member-crud-smoke.ps1
```

Sáu test tự động bao phủ đăng ký hoạt động trùng, giới hạn progress, tính số dư, hành vi duyệt đơn không tự tạo tài khoản và seed đủ role/tài khoản demo.

## Kịch bản trình bày trước giảng viên

1. Mở trang đăng ký công khai, gửi đơn và tra cứu trạng thái bằng MSSV.
2. Đăng nhập Admin, lọc đơn `PENDING`, duyệt đơn và kiểm tra hồ sơ thành viên được tạo.
3. Trình bày CRUD thành viên, ban, hoạt động, nhiệm vụ, cuộc họp; thử tìm kiếm, lọc và sắp xếp.
4. Đăng nhập Thành viên, đăng ký hoạt động, xem/cập nhật nhiệm vụ của mình, xem điểm và thông báo.
5. Mở Dashboard/Báo cáo bằng Admin và Trưởng ban để chứng minh dữ liệu được giới hạn theo vai trò.
6. Dùng AI gợi ý phân công; chỉ sau khi người quản lý bấm **Chấp nhận phân công** hệ thống mới ghi database.
7. Mở tài chính bằng Admin và chứng minh Thành viên không truy cập được.

## Quản lý mã nguồn

Project có `.gitignore` loại trừ `bin/`, `obj/`, database runtime files, công cụ cục bộ và secret. Máy hiện tại cần cài Git nếu lệnh `git` chưa có. Sau khi cài:

```powershell
git init
git add .
git commit -m "Hoan thien bai kiem tra thuong xuyen 2"
```

Không commit `.env`, API key, mật khẩu SQL Server hoặc file cấu hình local chứa secret. Minh chứng AI được lưu tại `docs/AI_USAGE_EVIDENCE.md`.

## Tài khoản demo

| Vai trò | Username/email | Mật khẩu |
|---|---|---|
| Admin | `admin` / `admin@club.demo` | `Admin@123` |
| Trưởng ban | `leader01` / `leader@club.demo` | `Leader@123` |
| Thành viên | `SV001` / `member@club.demo` | `Member@123` |

Tất cả là dữ liệu giả phục vụ demo.

## AI configuration

Không commit API key. Dùng User Secrets hoặc biến môi trường:

```powershell
dotnet user-secrets init --project src/StudentClubManagement
dotnet user-secrets set "AI_API_KEY" "YOUR_KEY" --project src/StudentClubManagement
dotnet user-secrets set "Ai:BaseUrl" "https://provider.example/v1/chat/completions" --project src/StudentClubManagement
dotnet user-secrets set "Ai:Model" "provider-model" --project src/StudentClubManagement
```

Nếu provider chưa cấu hình, chatbot dùng câu trả lời an toàn từ dữ liệu backend; gợi ý phân công dùng xếp hạng minh bạch theo dữ liệu đã lọc. Lỗi timeout, HTTP và JSON được bắt, ghi log không chứa secret và không làm hỏng module chính.

## Nội dung cần xác nhận

Các điểm Master Prompt đánh dấu chưa chốt không bị biến thành luật chính thức. Đặc biệt: hệ thống không tự tạo tài khoản khi duyệt đơn; điểm demo không phải công thức chính thức; chưa áp dụng ngưỡng/khoảng thời gian AI; email chỉ có adapter in-app fallback; các quyết định AI luôn cần con người chấp nhận.

## Cấu trúc

- `Authorization/`: kiểm tra phạm vi dữ liệu theo người dùng và ban.
- `Controllers/`: mỗi controller nghiệp vụ nằm trong một file riêng.
- `Data/`: `ApplicationDbContext`; dữ liệu mẫu nằm trong `Data/Seed/`.
- `Migrations/`: migration và model snapshot do EF Core quản lý.
- `Models/`: entity được nhóm theo miền `Club`, `Activities`, `Tasks`, `Meetings`, `Points`, `Finance`, `Notifications`, `AI` và `Identity`.
- `Services/Interfaces/`: hợp đồng service; implementation nằm trong `Services/` và `Services/AI/`.
- `ViewModels/`: model dành riêng cho form/trang, nhóm theo module.
- `Views/`: Razor Views theo tên controller; thành phần dùng chung nằm trong `Views/Shared/`.
- `wwwroot/`: CSS, JavaScript và thư viện frontend tĩnh.
- `tests/`: kiểm thử nghiệp vụ với SQLite in-memory.
