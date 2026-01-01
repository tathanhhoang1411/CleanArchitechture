using CleanArchitecture.Entites.Entites;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Entites.Interfaces
{
    public interface IFriendRepository
    {
        Task<Friend> SendRequest(Friend friend ,CancellationToken cancellationToken = default);
        Task<Friend> CheckExist(long senderId,long receiverId, CancellationToken cancellationToken = default);
        Task<List<Friend>> GetListSendFriend(int skip,int take,long userId,int status , CancellationToken cancellationToken = default);
        Task<Friend> GetAFriendRequest(long userId, long receiverId, CancellationToken cancellationToken = default);

        // Count friendships where the user is either sender or receiver and status matches
        Task<int> CountFriendsByUser(long userId, int status, CancellationToken cancellationToken = default);
        Task<Friend> SetAFriendRequest(Friend friend, int status, CancellationToken cancellationToken = default);
        Task<bool> DelAFriendRequest(Friend friend, CancellationToken cancellationToken = default);
    }
}
