using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Query.Utilities
{
    public class ApiRequest<T>
    {
        public int Skip { get; set; }  // Thuộc tính kiểu int đầu tiên
        public int Take { get; set; } // Thuộc tính kiểu int thứ hai
        public T RequestData { get; set; }       // Thuộc tính kiểu class chưa biết trước

        public ApiRequest() { }

        public ApiRequest(int skipParameter, int takeParameter, T requestData)
        {
            Skip = skipParameter;
            Take = takeParameter;
            RequestData = requestData;
        }
    }
}
