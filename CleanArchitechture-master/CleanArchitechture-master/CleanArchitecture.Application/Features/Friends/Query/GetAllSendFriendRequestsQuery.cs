using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Dtos;
using MediatR;

namespace CleanArchitecture.Application.Features.Friends.Query
{
    public class GetAllSendFriendRequestsQuery : IRequest<List<FriendsDto>>
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public int Status { get; set; }
        public long UserID { get; set; }

        public GetAllSendFriendRequestsQuery(int skip, int take, long userID,int status)
        {
            Skip = skip;
            Take = take;
            Status = status;
            UserID = userID;
        }
    }

    public class GetAllSendFriendRequestsQueryHandler : IRequestHandler<GetAllSendFriendRequestsQuery, List<FriendsDto>>
    {
        private readonly IFriendServices _friendServices;

        public GetAllSendFriendRequestsQueryHandler(IFriendServices productServices)
        {
            _friendServices = productServices;
        }

        public async Task<List<FriendsDto>> Handle(GetAllSendFriendRequestsQuery query, CancellationToken cancellationToken)
        {
            // Lấy danh sách đã gửi kết bạn
            List<FriendsDto> ListSendFriend = await _friendServices.GetList_SendFriend(query.Skip, query.Take, query.UserID, query.Status, cancellationToken);
            return ListSendFriend ?? new List<FriendsDto>(); // Trả về danh sách rỗng nếu không có sản phẩm
        }
    }
}