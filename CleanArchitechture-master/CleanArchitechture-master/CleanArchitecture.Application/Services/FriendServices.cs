using AutoMapper;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Repository;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Entites.Enums;
using CleanArchitecture.Entites.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CleanArchitecture.Application.Services
{
    public class FriendServices: IFriendServices
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _cache;
        private readonly IRabbitMQService _rabbitMQ;
        private readonly ILogger<FriendServices> _logger;
        public FriendServices(IReviewRepository reviewRepository, IUnitOfWork unitOfWork, IMapper mapper, IRedisCacheService cache, IRabbitMQService rabbitMQ, ILogger<FriendServices> logger)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _rabbitMQ = rabbitMQ ?? throw new ArgumentNullException(nameof(rabbitMQ));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        public async Task<FriendsDto> Send(Friend friend, CancellationToken cancellationToken )
        {
            if (friend == null) throw new ArgumentNullException(nameof(friend));
            Friend result = null;
            try
            {
                result = await _unitOfWork.Friends.SendRequest(friend, cancellationToken);
                await _unitOfWork.CompleteAsync(cancellationToken);
                // Gửi event sau khi DB đã cập nhật
                _rabbitMQ.Publish($"FriendCreate:{friend.RequestId}");
                return _mapper.Map<FriendsDto>(friend);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Friend_Create error");
                return new FriendsDto();
            }
        }
        public async Task<FriendsDto> CheckExist(long senderId ,long receiverId, CancellationToken cancellationToken )
        {
            if (senderId ==0) throw new ArgumentNullException(nameof(senderId));
            if (receiverId == 0) throw new ArgumentNullException(nameof(receiverId));
            Friend result = null;
            try
            {
                result = await _unitOfWork.Friends.CheckExist(senderId,receiverId, cancellationToken);
                if (result == null)
                {
                    return new FriendsDto();
                }
                return _mapper.Map<FriendsDto>(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Check FriendRequest error");
                return new FriendsDto();
            }
        }
        public async Task<List<FriendsDto>> GetList_SendFriend(int skip, int take, long userId,int status, CancellationToken cancellationToken )
        {
            if (skip < 0) throw new ArgumentNullException(nameof(skip));
            if (take< 0) throw new ArgumentNullException(nameof(take));
            List<Friend> listSendFriend = null;
            try
            {
                listSendFriend = await _unitOfWork.Friends.GetListSendFriend(skip, take,userId, status, cancellationToken);
                if (listSendFriend == null)
                {
                    return new List<FriendsDto>();
                }
                return _mapper.Map<List<FriendsDto>>(listSendFriend);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Check ListSendFriend error");
                return new List<FriendsDto>();
            }
        }
        public async Task<Friend> GetAFriendRequest( long userId, long receiverId, CancellationToken cancellationToken)
        {
            Friend aFriendRequest = new Friend();
            try
            {
                aFriendRequest = await _unitOfWork.Friends.GetAFriendRequest(userId,receiverId, cancellationToken);
                if (aFriendRequest == null)
                {
                    return new Friend();
                }
                return _mapper.Map<Friend>(aFriendRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Check ListSendFriend error");
                return new Friend();
            }
        }
        public async Task<FriendsDto> Set(Friend friend, int status, CancellationToken cancellationToken )
        {
            Friend aFriendRequest = new Friend();
            try
            {
                aFriendRequest = await _unitOfWork.Friends.SetAFriendRequest(friend, status, cancellationToken);
                if (aFriendRequest == null)
                {
                    return new FriendsDto();
                }
                aFriendRequest.Status = (FriendRequestStatus)status;
                aFriendRequest.ActionedAt = DateTime.UtcNow;
                await _unitOfWork.CompleteAsync(cancellationToken);
                return _mapper.Map<FriendsDto>(aFriendRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Accepted/ Rejected error");
                return new FriendsDto();
            }
        }
        public async Task<FriendsDto> Delete(Friend friend, CancellationToken cancellationToken = default)
        {
            try
            {
                bool result = await _unitOfWork.Friends.DelAFriendRequest(friend, cancellationToken);
                if (!result)
                {
                    return new FriendsDto();
                }  
                await _unitOfWork.CompleteAsync(cancellationToken);
                return _mapper.Map<FriendsDto>(friend);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Deleted error");
                return new FriendsDto();
            }
        }
    }
}
