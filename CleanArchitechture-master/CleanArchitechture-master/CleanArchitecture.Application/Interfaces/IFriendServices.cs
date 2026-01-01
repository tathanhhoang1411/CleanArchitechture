using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Entites.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Interfaces
{
    public interface IFriendServices
    {
        Task<FriendsDto> Send(Friend friend, CancellationToken cancellationToken = default);
        Task<FriendsDto> CheckExist(long senderId,long receiverId, CancellationToken cancellationToken = default);
        Task<List<FriendsDto>> GetList_SendFriend(int skip,int take, long userId,int status, CancellationToken cancellationToken = default);
        Task<Friend> GetAFriendRequest( long userId,long receiverId, CancellationToken cancellationToken = default);
        Task<FriendsDto> Set(Friend friend,int status,CancellationToken cancellationToken = default);
        Task<FriendsDto> Delete(Friend friend, CancellationToken cancellationToken = default);
    }
}
