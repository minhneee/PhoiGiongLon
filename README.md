# Swine Breeding Manager

Ứng dụng quản lý nông trại chăn nuôi lợn được xây dựng bằng ASP.NET Core MVC.

## 📋 Mục đích

Swine Breeding Manager là một hệ thống quản lý toàn diện cho các nông trại chăn nuôi lợi, cung cấp các tính năng:

- **Quản lý lợi**: Theo dõi thông tin chi tiết về từng con lợi (tag number, avatar, sức khỏe)
- **Quản lý chuồng**: Tổ chức và quản lý các chuồng chăn nuôi
- **Quản lý sinh sản**: Theo dõi các bản ghi sinh sản và phối giống
- **Quản lý sức khỏe**: Ghi lại thông tin sức khỏe và cân nặng của lợi
- **Quản lý bán hàng**: Theo dõi các bản ghi bán và giá bán
- **Quản lý người dùng**: Kiểm soát quyền truy cập
- **Báo cáo**: Xem các báo cáo chi tiết về hoạt động nông trại
- **Phân tích dòng máu**: Xem thông tin phả hệ lợi

## 🛠️ Công nghệ sử dụng

- **Framework**: ASP.NET Core MVC
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Xác thực**: ASP.NET Core Identity
- **UI**: Razor Views, Bootstrap

## 📁 Cấu trúc dự án

```
SwineBreedingManager/
├── Areas/                 # Identity areas
├── Controllers/           # MVC controllers
├── Models/               # Data models
├── Views/                # Razor views
├── Services/             # Business logic services
├── Data/                 # Database context và seeding
├── Migrations/           # EF Core migrations
├── Filters/              # Custom filters
├── Properties/           # Cấu hình launch
└── wwwroot/             # Static files
```

## 🚀 Cách chạy

### Yêu cầu
- .NET SDK 6.0 hoặc cao hơn
- SQL Server

### Thiết lập
1. Clone dự án
2. Cập nhật connection string trong `appsettings.json`
3. Chạy migrations:
   ```bash
   dotnet ef database update
   ```
4. Chạy ứng dụng:
   ```bash
   dotnet run
   ```

Ứng dụng sẽ khởi chạy tại `https://localhost:7000`

## 📝 Cấu hình

### appsettings.json
- Cấu hình connection string
- Cấu hình logging
- Cấu hình ứng dụng khác

### appsettings.Development.json
- Cấu hình cho môi trường phát triển

## 🔐 Xác thực và Phân quyền

Ứng dụng sử dụng ASP.NET Core Identity để quản lý người dùng và phân quyền dựa trên các trang (PagePermission).

## 📚 Các Models chính

- **Pig**: Thông tin lợi
- **Pen**: Chuồng chăn nuôi
- **BreedingRecord**: Bản ghi sinh sản
- **SaleRecord**: Bản ghi bán hàng
- **PigHealth**: Thông tin sức khỏe lợi
- **PagePermission**: Phân quyền truy cập trang

## 🔗 Các Services

- **BreedingService**: Xử lý logic sinh sản
- **GenealogyService**: Xử lý logic phả hệ

## 📊 Cơ sở dữ liệu

Dự án sử dụng Entity Framework Core migrations để quản lý schema:
- `InitialCreate`: Tạo các bảng cơ bản
- `AddAvatarToPig`: Thêm hỗ trợ avatar cho lợi
- `AddPenManagement`: Thêm quản lý chuồng
- `AddWeightAndSales`: Thêm quản lý cân nặng và bán hàng
- `AddPenIdToBreedingRecord`: Liên kết chuồng với bản ghi sinh sản
- `AddUniqueTagNumber`: Thêm unique constraint cho tag number
- `AddPagePermissions`: Thêm hệ thống phân quyền trang

## 📧 Liên hệ

Nếu có câu hỏi hoặc góp ý, vui lòng liên hệ qua email hoặc tạo issue.

---

**Lần cập nhật cuối**: Tháng 3, 2026
