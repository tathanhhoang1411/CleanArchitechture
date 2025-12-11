using CleanArchitecture.Entites.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Dtos
{
    public class FriendsDto
    {
        public long SenderId { get; set; } 
        public long ReceiverId { get; set; } 
        public FriendRequestStatus Status { get; set; } 
        public DateTime RequestedAt { get; set; } 
        public DateTime? ActionedAt { get; set; }
    }
}
