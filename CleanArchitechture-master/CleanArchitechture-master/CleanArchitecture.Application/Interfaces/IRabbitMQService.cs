using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Interfaces
{
    public interface IRabbitMQService
    {
        /// <summary>
        /// Xuất bản (Publish) một tin nhắn đến RabbitMQ queue.
        /// </summary>
        /// <param name="message">Nội dung tin nhắn dưới dạng chuỗi.</param>
        void Publish(string message);

        /// <summary>
        /// Đăng ký (Subscribe) để nhận tin nhắn từ RabbitMQ queue.
        /// </summary>
        /// <param name="onMessage">Action được thực thi khi nhận được tin nhắn.</param>
        void Subscribe(Action<string> onMessage);
    }
}
