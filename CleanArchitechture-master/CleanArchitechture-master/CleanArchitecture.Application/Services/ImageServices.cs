
using AutoMapper;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Entites.Enums;
using CleanArchitecture.Entites.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace CleanArchitecture.Application.Repository
{
    public class ImageServices : IImageServices
    {
        private readonly ILogger<ImageServices> _logger;
        public ImageServices(ILogger<ImageServices> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userID"> id tài khoản đang thực hiện tính năng</param>
        /// <param name="type">Loại upload (upload ảnh avatar:1, review:2, comment:3)</param>
        /// <param name="Id">Id của review, của comment, của user</param>
        /// <param name="environment"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public  string GetFilePath(long userID, int type, long Id, string webRootPath, CancellationToken cancellationToken=default)
        {
            // Sử dụng WebRootPath để lưu trữ tệp tin công khai/tĩnh
            string baseUploadsFolder = Path.Combine(webRootPath, "Uploads");
            string subFolder;

            switch ((TypeUploadImg)type) // Dùng enum trực tiếp thay vì ép kiểu int
            {
                case TypeUploadImg.Product:
                    // Uploads/Reviews/UserID_123/Review_456
                    subFolder = Path.Combine("Reviews", $"UserID_{userID}", $"Review_{Id}");
                    break;
                case TypeUploadImg.Avatar:
                    subFolder = Path.Combine("Avatars");
                    break;
                case TypeUploadImg.Comment:
                    // Uploads/Comments/UserID_123/Comment_456
                    subFolder = Path.Combine("Comments", $"UserID_{userID}", $"Comment_{Id}");
                    break;
                default:
                    throw new ArgumentException("Invalid flag value", nameof(type));
            }

            // Path.Combine an toàn trên mọi nền tảng
            return Path.Combine(baseUploadsFolder, subFolder);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="userID">Tài khoản thực hiện</param>
        /// <param name="type">Loại upload: avatar:1,review:2, comment:3</param>
        /// <param name="Id">id của user,review, comment,</param>
        /// <param name="email">Dùng trường email để đặt tên cho file ảnh ,Nếu upload ảnh đại diện, còn nếu các loại update khác thì nhập chuoiox rỗng</param>
        /// <param name="environment"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<string>> UploadImage(HttpRequest request,string email, long userID, int type, long Id, string webRootPath, CancellationToken cancellationToken = default)
        {
            try
            {
                List<string> listPublicUrls = new List<string>();
                int temp = 1;

                // 1. Lấy đường dẫn & tạo thư mục chỉ một lần
                string Filepath = GetFilePath(userID, type, Id, webRootPath);
                if (!System.IO.Directory.Exists(Filepath))
                {
                    System.IO.Directory.CreateDirectory(Filepath);
                }

                // Xác định tên thư mục gốc tương đối để trả về URL
                string publicRoot = Filepath.Replace(webRootPath, "").Replace('\\', '/');
                if (publicRoot.EndsWith('/')) publicRoot = publicRoot.TrimEnd('/');

                foreach (IFormFile source in request.Form.Files)
                {
                    if (temp > 5) break; // Dừng sớm

                    // 2. Kiểm tra bảo mật cơ bản
                    if (!source.ContentType.StartsWith("image/")) continue;

                    string fileName = "";
                    switch ((TypeUploadImg)type)
                    {
                        case TypeUploadImg.Product:
                        case TypeUploadImg.Comment:
                            // Giữ tên file theo số thứ tự (hoặc nên dùng GUID)
                            fileName = $"{temp}.jpg";
                            break;
                        case TypeUploadImg.Avatar:
                            fileName = email + ".jpg";
                            break;
                        default:
                            continue;
                    }

                    string imagepath = Path.Combine(Filepath, fileName);

                    // Xóa tệp cũ và lưu tệp mới
                    if (System.IO.File.Exists(imagepath)) System.IO.File.Delete(imagepath);

                    using (FileStream stream = System.IO.File.Create(imagepath))
                    {
                        // 3. Sử dụng await
                        await source.CopyToAsync(stream, cancellationToken);
                    }

                    // 4. Trả về URL công khai
                    listPublicUrls.Add($"{publicRoot}/{fileName}");

                    temp++;
                }
                return listPublicUrls;
            }catch{
                return null;
            }
        }
        public async Task<bool> IsImageExist(string email, int type, string webRootPath, CancellationToken cancellationToken = default)
        {
            try
            {
                // 1. Lấy đường dẫn thư mục dựa trên loại (type)
                // Sử dụng tham số 0 cho userID và Id theo mẫu bạn cung cấp
                string folderPath = GetFilePath(0, type, 0, webRootPath);

                // 2. Tạo tên file theo định dạng {email}.jpg
                string fileName = $"{email}.jpg";

                // 3. Kết hợp thư mục và tên file bằng Path.Combine để đảm bảo an toàn trên mọi hệ điều hành
                string fullPath = Path.Combine(folderPath, fileName);

                // 4. Kiểm tra sự tồn tại của tệp tin vật lý
                // Task.Run được dùng ở đây để thực hiện thao tác I/O đồng bộ trong một luồng bất đồng bộ
                return await Task.Run(() => File.Exists(fullPath), cancellationToken);
            }
            catch
            {
                // Trả về false nếu có bất kỳ lỗi nào xảy ra (ví dụ: lỗi quyền truy cập hoặc đường dẫn không hợp lệ)
                return false;
            }
        }

        public async Task DeleteUploadedFiles(List<string> files, string webRootPath, CancellationToken cancellationToken = default)
        {
            if (files == null || files.Count == 0) return;
            foreach (var entry in files)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (string.IsNullOrWhiteSpace(entry)) continue;

                    string physicalPath = entry;
                    // If entry looks like a public relative URL (/Uploads/...), convert to physical path
                    if (entry.StartsWith("/") || entry.StartsWith("Uploads") || entry.Contains("/Uploads/"))
                    {
                        var rel = entry.TrimStart('/');
                        physicalPath = Path.Combine(webRootPath, rel.Replace('/', Path.DirectorySeparatorChar));
                    }

                    if (File.Exists(physicalPath))
                    {
                        File.Delete(physicalPath);
                        _logger.LogInformation($"Deleted uploaded file: {physicalPath}");
                    }
                    else
                    {
                        _logger.LogInformation($"File to delete not found: {physicalPath}");
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("DeleteUploadedFiles canceled");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to delete uploaded file: {entry}");
                }
            }
        }
    }
}
