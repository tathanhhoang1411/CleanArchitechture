# ====================================================================
# Giai đoạn 1: Build (Sử dụng SDK để biên dịch)
# ====================================================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Đặt biến môi trường và thư mục làm việc
ARG CONFIGURATION=Release
WORKDIR /src

# Sao chép các file .csproj và chạy dotnet restore. 
# Bước này được tối ưu hóa để Docker cache hiệu quả.
# Đường dẫn là tương đối so với Context Build (CleanArchitechture-master/)

# Sao chép file .csproj của tất cả các tầng/layer
COPY ["BE_2911_CleanArchitechture/BE_2911_CleanArchitechture.csproj", "BE_2911_CleanArchitechture/"]
COPY ["CleanArchitecture.Application/CleanArchitecture.Application.csproj", "CleanArchitecture.Application/"]
COPY ["CleanArchitecture.Infrastructure/CleanArchitecture.Infrastructure.csproj", "CleanArchitecture.Infrastructure/"]
COPY ["CleanArchitecture.Entites/CleanArchitecture.Entites.csproj", "CleanArchitecture.Entites/"]

# Khôi phục các gói NuGet (dependencies)
RUN dotnet restore "BE_2911_CleanArchitechture/BE_2911_CleanArchitechture.csproj"

# Sao chép toàn bộ mã nguồn còn lại
COPY . .

# Build ứng dụng
RUN dotnet build "BE_2911_CleanArchitechture/BE_2911_CleanArchitechture.csproj" -c $CONFIGURATION -o /app/build

# ====================================================================
# Giai đoạn 2: Publish (Tạo ra output sẵn sàng chạy)
# ====================================================================
FROM build AS publish
RUN dotnet publish "BE_2911_CleanArchitechture/BE_2911_CleanArchitechture.csproj" -c $CONFIGURATION -o /app/publish /p:UseAppHost=false

# ====================================================================
# Giai đoạn 3: Final (Sử dụng Runtime nhỏ gọn để chạy)
# ====================================================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Sao chép output đã publish từ giai đoạn 'publish'
COPY --from=publish /app/publish .

# Cấu hình mạng
# Mở cổng mặc định 8080.
EXPOSE 8080 
# ASP.NET Core sẽ lắng nghe trên cổng 8080 trong container
ENV ASPNETCORE_URLS=http://+:8080

# Điểm khởi chạy của ứng dụng (Tên file DLL phải trùng với tên project)
ENTRYPOINT ["dotnet", "BE_2911_CleanArchitechture.dll"]