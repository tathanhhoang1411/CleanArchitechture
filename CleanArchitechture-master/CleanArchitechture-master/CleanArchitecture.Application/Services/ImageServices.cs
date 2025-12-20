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
        public  string GetFilePath(long userID, int type, long Id, string webRootPath, CancellationToken cancellationToken=default)
        {
            string baseUploadsFolder = Path.Combine(webRootPath, "Uploads");
            string subFolder;

            switch ((TypeUploadImg)type) // Dùng enum trực tiếp thay vì ép kiểu int
            {
                case TypeUploadImg.Product:
                    subFolder = Path.Combine("Reviews", $"UserID_{userID}", $"Review_{Id}");
                    break;
                case TypeUploadImg.Avatar:
                    subFolder = Path.Combine("Avatars");
                    break;
                case TypeUploadImg.Comment:
                    subFolder = Path.Combine("Comments", $"UserID_{userID}", $"Comment_{Id}");
                    break;
                default:
                    throw new ArgumentException("Invalid flag value", nameof(type));
            }

            return Path.Combine(baseUploadsFolder, subFolder);
        }

        public async Task<List<string>> UploadImage(HttpRequest request,string email, long userID, int type, long Id, string webRootPath, CancellationToken cancellationToken = default)
        {
            try
            {
                List<string> listPublicUrls = new List<string>();
                int temp = 1;

                string Filepath = GetFilePath(userID, type, Id, webRootPath);
                if (!System.IO.Directory.Exists(Filepath))
                {
                    System.IO.Directory.CreateDirectory(Filepath);
                }

                string publicRoot = Filepath.Replace(webRootPath, "").Replace('\\', '/');
                if (publicRoot.EndsWith('/')) publicRoot = publicRoot.TrimEnd('/');

                foreach (IFormFile source in request.Form.Files)
                {
                    if (temp > 5) break; // Dừng sớm

                    if (!source.ContentType.StartsWith("image/")) continue;

                    string fileName = "";
                    switch ((TypeUploadImg)type)
                    {
                        case TypeUploadImg.Product:
                        case TypeUploadImg.Comment:
                            fileName = $"{temp}.jpg";
                            break;
                        case TypeUploadImg.Avatar:
                            fileName = email + ".jpg";
                            break;
                        default:
                            continue;
                    }

                    string imagepath = Path.Combine(Filepath, fileName);

                    if (System.IO.File.Exists(imagepath)) System.IO.File.Delete(imagepath);

                    using (FileStream stream = System.IO.File.Create(imagepath))
                    {
                        await source.CopyToAsync(stream, cancellationToken);
                    }

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
                string folderPath = GetFilePath(0, type, 0, webRootPath);
                string fileName = $"{email}.jpg";
                string fullPath = Path.Combine(folderPath, fileName);
                return await Task.Run(() => File.Exists(fullPath), cancellationToken);
            }
            catch
            {
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
