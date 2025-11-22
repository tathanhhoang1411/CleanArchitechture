using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Dtos;
using MediatR;

namespace CleanArchitecture.Application.Features.Users.Query
{
    public class GetAllUserQuery : IRequest<List<UsersDto>>
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public string Data { get; set; }
        public GetAllUserQuery(int skip, int take, string data)
        {
            Skip = skip;
            Take = take;
            Data = data;
        }
        public class GetAllUserQueryHandler : IRequestHandler<GetAllUserQuery, List<UsersDto>>
        {

            private readonly IUserServices _userServices;
            public GetAllUserQueryHandler(IUserServices userServices)
            {
                _userServices = userServices;
            }
            public async Task<List<UsersDto>> Handle(GetAllUserQuery query, CancellationToken cancellationToken)
            {
                var userList = await _userServices.GetList_Users(query.Skip, query.Take,query.Data);
                return userList ?? new List<UsersDto>(); // Trả về danh sách rỗng nếu không có sản phẩm
            }
        }
    }
}
