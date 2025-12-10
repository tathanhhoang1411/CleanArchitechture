using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Entites.Enums
{
    public enum FriendRequestStatus
    {
        Pending = 1, // Đang chờ
        Accepted = 2, // Đã chấp nhận (đã là bạn bè)
        Rejected = 3 // Đã từ chối
    }
}
