
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
        public async Task<Friend> SetAFriendRequest(Friend friend, int status, CancellationToken cancellationToken = default)
        {
            try
            {
                Friend aFriendRequest =await _userContext.Friends
                    .FirstOrDefaultAsync(p => p.ReceiverId==friend.ReceiverId && p.SenderId==friend.SenderId, cancellationToken);

                return aFriendRequest;
            }
            catch
            {
                return new Friend();
            }
        }
        public async Task<Friend> CheckExist(long senderId, long receiverId, CancellationToken cancellationToken = default)
        {
            Friend aFriend = new Friend();
            try
            {
                // Truy vấn chỉ cần kiểm tra một chiều duy nhất (A -> B)
                aFriend = await _userContext.Friends.AsNoTracking()
                    .FirstOrDefaultAsync(p => (p.SenderId == senderId && p.ReceiverId == receiverId)||(p.SenderId == receiverId && p.ReceiverId == senderId), cancellationToken);
                return aFriend;
            }
            catch
            {
                return aFriend;
            }
        }
        public async Task<List<Friend>> GetListSendFriend(int skip, int take, long userId,int status, CancellationToken cancellationToken = default)
        {
            List<Friend> listSendFriend = null;
            try
            {
                listSendFriend = await _userContext.Friends
                                .Where(p => (p.SenderId == userId || p.ReceiverId == userId) && (int)p.Status == status)
                                .AsNoTracking()
                                .OrderByDescending(p => p.RequestedAt)
                                .AsNoTracking()
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
        public async Task<Friend> GetAFriendRequest(long userId, long receiverId, CancellationToken cancellationToken = default)
        {
            Friend AFriendRequest = new Friend();
            try
            {
                AFriendRequest = await _userContext.Friends
                                .AsNoTracking()
                                .FirstOrDefaultAsync(p => p.SenderId == receiverId && p.ReceiverId == userId); 
                return AFriendRequest;
            }
            catch
            {
                return AFriendRequest;
            }
        }
        public async Task<bool> DelAFriendRequest(Friend friend, CancellationToken cancellationToken = default)
        {
            try
            {
                // 1. Tìm các bản ghi thỏa mãn điều kiện (Sử dụng IQueryable)
                var requestsToDelete = _userContext.Friends
                    .Where(p => p.ReceiverId == friend.ReceiverId && p.SenderId == friend.SenderId);

                // 2. Kiểm tra nếu không có dữ liệu thì không làm gì cả
                if (!requestsToDelete.Any())
                {
                    return false;
                }

                // 3. Đánh dấu xóa các bản ghi này trong Change Tracker
                // RemoveRange sẽ tự thực hiện câu lệnh SELECT nội bộ trước khi đánh dấu xóa
                _userContext.Friends.RemoveRange(requestsToDelete);

                // LƯU Ý: Vì bạn đang dùng mô hình Unit of Work, việc xóa thực sự xuống Database 
                // chỉ xảy ra khi bạn gọi _unitOfWork.CompleteAsync() ở tầng Service.
                return true;
            }
            catch (Exception ex)
            {
                // Bạn nên dùng _logger ở đây để ghi lại lỗi nếu có
                return false;
            }
        }

        public async Task<int> CountFriendsByUser(long userId, int status, CancellationToken cancellationToken = default)
        {
            try
            {
                var count = await _userContext.Friends
                    .Where(f => ((f.SenderId == userId) || (f.ReceiverId == userId)) && (int)f.Status == status)
                    .AsNoTracking()
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
