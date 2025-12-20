
using CleanArchitecture.Entites.Entites;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Interfaces
{
    public interface IImageServices
    {
         public string GetFilePath(long userID, int type, long Id, string webRootPath, CancellationToken cancellationToken = default);
        public Task<List<string>> UploadImage(HttpRequest request,string email, long userID, int type, long Id, string webRootPath, CancellationToken cancellationToken = default);
        public Task<bool> IsImageExist(string email,int type, string webRootPath, CancellationToken cancellationToken = default);
        // Delete uploaded files given their public/relative URLs or physical paths; webRootPath is used to resolve relative URLs
        public Task DeleteUploadedFiles(List<string> files, string webRootPath, CancellationToken cancellationToken = default);

    }
}
