using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Application.Features.Comments.Commands.Create;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Entites.Enums;
using CleanArchitecture.Entites.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Features.Friends.Commands.Update
{
    public class SetFriendRequestCommand : MediatR.IRequest<FriendsDto>
    {
        public long senderId { get; set; }
        public long receiverId { get; set; }
        public int  status { get; set; }
        public SetFriendRequestCommand(long senderId, long receiverId, int status)
        {
            this.senderId = senderId;
            this.receiverId = receiverId;
            this.status = status;
        }
        public class SetFriendRequestCommandHandler : IRequestHandler<SetFriendRequestCommand, FriendsDto>
        {
            private readonly IFriendServices _friendServices;
            private readonly ILogger<SetFriendRequestCommandHandler> _logger;
            public SetFriendRequestCommandHandler(IFriendServices friendServices, ILogger<SetFriendRequestCommandHandler> logger)
            {
                _friendServices = friendServices ?? throw new ArgumentNullException(nameof(friendServices));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }
            public async Task<FriendsDto> Handle(SetFriendRequestCommand setFriendRequestCommand, CancellationToken cancellationToken)
            {
                FriendsDto friendDto=null;
                try
                {
                    _logger.LogInformation("SetFriendRequestCommand starting for user {UserId}", setFriendRequestCommand.senderId);
                     friendDto = await _friendServices.CheckExist(setFriendRequestCommand.senderId, setFriendRequestCommand.receiverId, cancellationToken);
                    if (friendDto.Status == FriendRequestStatus.Accepted || friendDto.Status == FriendRequestStatus.Rejected) return friendDto;
                    //nếu không  tồn tại lời mời  
                    if (!(friendDto.ReceiverId == setFriendRequestCommand.receiverId && friendDto.Status == FriendRequestStatus.Pending))
                    {
                        return friendDto;
                    }
                    //nếu tồn tại rồi thì cập nhật trạng thái
                    //Lấy lời mời kết bạn
                    Friend aFriendRequest = new Friend();
                    aFriendRequest.ReceiverId = friendDto.ReceiverId;
                    aFriendRequest.SenderId = friendDto.SenderId;
                    friendDto = await _friendServices.Set(aFriendRequest, setFriendRequestCommand.status, cancellationToken);
                    _logger.LogInformation("SendFriendRequestCommand completed for comment {CommentId}", setFriendRequestCommand.senderId);
                    return friendDto;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "SendFriendRequestCommand failed for user {UserId}", setFriendRequestCommand.senderId);
                    return friendDto;
                } 
            }
        }
     }
}
