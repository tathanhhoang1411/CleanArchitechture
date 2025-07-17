using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Query.Utilities
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; }

        public ApiResponse()
        {
            Errors = new List<string>();
        }

        public ApiResponse(T data)
        {
            Success = true;
            Message = "Operation completed successfully.";
            Data = data;
            Errors = new List<string>();
        }
        public static ApiResponse<T> CreateErrorResponse(List<string> errors, bool status)
        {
            return new ApiResponse<T>
            {
                Success = status,
                Message = "Operation failed.",
                Data = default,
                Errors = errors
            };
        }
    }
}
