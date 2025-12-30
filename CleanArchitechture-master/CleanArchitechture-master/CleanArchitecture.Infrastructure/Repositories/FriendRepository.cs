
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Entites.Interfaces;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Crypto;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class FriendRepository : IFriendRepository
    {
        private readonly ApplicationContext _userContext;
        private readonly int _maxTake; // Giới hạn configurable

        public FriendRepository(ApplicationContext userContext, IConfiguration configuration)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            // Đọc từ cấu hình, fallback về 1000 nếu không có hoặc không hợp lệ
            var configured = configuration.GetValue<int?>("Paging:MaxTake");
            _maxTake = (configured.HasValue && configured.Value > 0) ? configured.Value : 1000;
        }

        public async Task<Friend> SendRequest(Friend friend, CancellationToken cancellationToken = default)
        {
            try
            {
                await _userContext.Friends.AddAsync(friend, cancellationToken);
                return friend;
            }
            catch
            {
                return new Friend();
            }
        }
        public async Task<Friend> CheckExist(long senderId, long receiverId, CancellationToken cancellationToken = default)
        {
            Friend aFriend = null;
            try
            {
                // Truy vấn chỉ cần kiểm tra một chiều duy nhất (A -> B)
                aFriend = await _userContext.Friends
                    .FirstOrDefaultAsync(p => p.SenderId == senderId && p.ReceiverId == receiverId, cancellationToken);
                return aFriend;
            }
            catch
            {
                return null;
            }
        }
        public async Task<List<Friend>> GetListSendFriend(int skip, int take, long userId,int status, CancellationToken cancellationToken = default)
        {
            List<Friend> listSendFriend = null;
            try
            {
                listSendFriend = await _userContext.Friends
                                .Where(p => (p.SenderId == userId || p.ReceiverId == userId) && (int)p.Status== status)
                                .OrderByDescending(p=>p.RequestedAt)
                                .Skip(skip)
                                .Take(take)
                                .ToListAsync(cancellationToken);
                return listSendFriend;
            }
            catch
            {
                return null;
            }
        }

        public async Task<int> CountFriendsByUser(long userId, int status, CancellationToken cancellationToken = default)
        {
            try
            {
                var count = await _userContext.Friends
                    .Where(f => ((f.SenderId == userId) || (f.ReceiverId == userId)) && (int)f.Status == status)
                    .CountAsync(cancellationToken);
                return count;
            }
            catch
            {
                return 0;
            }
        }
    }
}
