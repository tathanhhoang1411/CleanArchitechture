using CleanArchitecture.Application.IRepository;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Query
{
    public class GetAllUserQuery : IRequest<List<UserDto>>
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
        public class GetAllUserQueryHandler : IRequestHandler<GetAllUserQuery, List<UserDto>>
        {

            private readonly IUserServices _userServices;
            public GetAllUserQueryHandler(IUserServices userServices)
            {
                _userServices = userServices;
            }
            public async Task<List<UserDto>> Handle(GetAllUserQuery query, CancellationToken cancellationToken)
            {
                var userList = await _userServices.GetList_Users(query.Skip, query.Take,query.Data);
                return userList ?? new List<UserDto>(); // Trả về danh sách rỗng nếu không có sản phẩm
            }
        }
    }
}
