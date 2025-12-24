using CleanArchitecture.Entites.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CleanArchitecture.Entites.Interfaces
{
    public interface IFriendRepository
    {
        Task<Friend> SendRequest(Friend friend ,CancellationToken cancellationToken = default);
        Task<Friend> CheckExist(long senderId,long receiverId, CancellationToken cancellationToken = default);
        Task<List<Friend>> GetListSendFriend(int skip,int take,long userId,int status , CancellationToken cancellationToken = default);

        // Count friendships where the user is either sender or receiver and status matches
        Task<int> CountFriendsByUser(long userId, int status, CancellationToken cancellationToken = default);
    }
}
