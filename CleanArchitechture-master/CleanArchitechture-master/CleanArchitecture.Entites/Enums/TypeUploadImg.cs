using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Entites.Enums
{
    /// <summary>
    /// 1: ảnh đại diện, 2: ảnh sản phẩm, 3: ảnh bình luận
    /// </summary>
    public enum TypeUploadImg
    {
        Avatar = 1, 
        Product = 2, 
        Comment = 3,
        Chat = 4
    }
}
