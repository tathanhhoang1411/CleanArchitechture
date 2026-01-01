using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Application.Features.Comments.Commands.Create;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Entites.Enums;
using CleanArchitecture.Entites.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Features.Friends.Commands.Create
{
    public class SendFriendRequestCommand : IRequest<FriendsDto>
    {
        public long senderId { get; set; }
        public long receiverId { get; set; }
        public SendFriendRequestCommand(long senderId, long receiverId)
        {
            this.senderId = senderId;
            this.receiverId = receiverId;
        }
        public class SendFriendRequestCommandHandler : IRequestHandler<SendFriendRequestCommand, FriendsDto>
        {
            private readonly IFriendServices _friendServices;
            private readonly ILogger<SendFriendRequestCommandHandler> _logger;
            public SendFriendRequestCommandHandler(IFriendServices friendServices, ILogger<SendFriendRequestCommandHandler> logger)
            {
                _friendServices = friendServices ?? throw new ArgumentNullException(nameof(friendServices));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }
            public async Task<FriendsDto> Handle(SendFriendRequestCommand sendFriendRequestCommand, CancellationToken cancellationToken)
            {
                FriendsDto friendDto=null;
                try
                {
                    _logger.LogInformation("SendFriendRequestCommand starting ", sendFriendRequestCommand.senderId);
                     friendDto = await _friendServices.CheckExist(sendFriendRequestCommand.senderId, sendFriendRequestCommand.receiverId, cancellationToken);
                    //Nếu có tồn tại lời mời rồi 
                    if (friendDto.ReceiverId > 0 || friendDto.Status==FriendRequestStatus.Accepted || friendDto.Status == FriendRequestStatus.Pending)
                    {
                        return new FriendsDto();
                    }
                    Friend friend = new Friend
                    {
                        SenderId = sendFriendRequestCommand.senderId,
                        ReceiverId = sendFriendRequestCommand.receiverId,
                        RequestedAt = DateTime.UtcNow,
                        ActionedAt = null,
                        Status = Entites.Enums.FriendRequestStatus.Pending
                    };
                    friendDto = await _friendServices.Send(friend, cancellationToken);
                    _logger.LogInformation("Successed!", sendFriendRequestCommand.senderId);
                    return friendDto;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed!", sendFriendRequestCommand.senderId);
                    return friendDto;
                } 
            }
        }
     }
}
