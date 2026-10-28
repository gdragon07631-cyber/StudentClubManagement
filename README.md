# Hệ thống quản lý câu lạc bộ sinh viên có tích hợp AI

Đồ án Nhóm 15 xây dựng theo kiến trúc tách biệt **Backend Web API** và **Frontend SPA tĩnh**:
- **Backend (`backend/`)**: ASP.NET Core Web API, C#, Entity Framework Core, ASP.NET Core Identity, xác thực JWT và SQL Server / LocalDB.
- **Frontend (`frontend/`)**: HTML5, CSS3 (Bootstrap 5 + custom CSS), JavaScript thuần (ES Modules) gọi trực tiếp REST API của Backend.
- **Database (`database/`)**: Chứa file script `StudentClubManagement.sql` (đầy đủ cấu trúc bảng, khóa ngoại, ràng buộc và dữ liệu thật) và file sao lưu `StudentClubManagement.bak`.
- **Tài liệu (`docs/`)**: Chứa các tệp tài liệu kế hoạch, đặc tả yêu cầu (`.docx`) và minh chứng sử dụng AI.

## Cấu trúc thư mục

```text
├── backend/                  # Mã nguồn Backend Web API (Controllers, Models, Services, Data, Migrations)
├── frontend/                 # Giao diện người dùng tĩnh (index.html, css/, js/, lib/)
├── database/                 # Database thật: StudentClubManagement.sql và StudentClubManagement.bak
├── docs/                     # Các file tài liệu đọc (.docx, .md)
├── tests/                    # Kiểm thử tự động xUnit
└── StudentClubManagement.slnx
```

## Tài khoản mẫu trong Database

| Vai trò | Tên đăng nhập | Mật khẩu |
|---|---|---|
| Admin / Ban chủ nhiệm | `admin` | `Admin@123` |
| Trưởng câu lạc bộ | `leader01` | `Leader@123` |
| Thành viên | `SV001` | `Member@123` |

## Cài đặt và chạy dự án

### 1. Khởi chạy Backend (tự động migrate & nạp dữ liệu mẫu trên LocalDB)

```powershell
dotnet restore StudentClubManagement.slnx
dotnet run --project backend/StudentClubManagement.Backend.csproj
```

Khi chạy ở cổng mặc định `http://localhost:5100`:
- Các endpoint REST API hoạt động tại `http://localhost:5100/api/...`
- Mở trực tiếp `http://localhost:5100` trên trình duyệt sẽ hiển thị ngay giao diện từ thư mục `frontend/`.
- Ngoài ra có thể mở riêng thư mục `frontend/` bằng VS Code Live Server (`http://localhost:5500`) hoặc `python -m http.server 5296 --directory frontend`.

### 2. Khôi phục từ file Database (`database/`)

Nếu muốn nạp trực tiếp vào SQL Server (SSMS):
- **Cách 1 (Script SQL)**: Mở file `database/StudentClubManagement.sql` trong SQL Server Management Studio (SSMS) và nhấn **Execute (F5)**, hoặc chạy:
  ```powershell
  sqlcmd -S "(localdb)\MSSQLLocalDB" -E -C -i database/StudentClubManagement.sql
  ```
- **Cách 2 (Backup `.bak`)**: Dùng chức năng **Restore Database** trong SSMS với file `database/StudentClubManagement.bak`.

## Kiểm thử (Unit Tests)

```powershell
dotnet test StudentClubManagement.slnx
```
