using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZ_8_Test
{
    public class OrederProcessor
    {
        public interface IDatabase
        {
            bool IsConnected { get; }
            void Connect();
            void Save(Order order);
            Order GetOrder(int id);
        }

        public interface IEmailService
        {
            void SendOrderConfirmation(string customerEmail, int orderId);
        }

        public class Order
        {
            public int Id { get; set; }
            public string CustomerEmail { get; set; }
            public decimal TotalAmount { get; set; }
            public bool IsProcessed { get; set; }
        }

        // ТО ЧТО НУЖНО ТЕСТИРОВАТЬ 
        public class OrderProcessor
        {
            private readonly IDatabase _database;
            private readonly IEmailService _emailService;

            private const decimal EmailNotificationThreshold = 100m;

            public OrderProcessor(IDatabase database, IEmailService emailService)
            {
                _database = database ?? throw new ArgumentNullException(nameof(database));
                _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            }

            public bool ProcessOrder(Order order)
            {
                if (order == null) throw new ArgumentNullException(nameof(order));
                if (order.TotalAmount <= 0) return false;

                EnsureDatabaseConnection();

                try
                {
                    _database.Save(order);

                    if (ShouldSendConfirmationEmail(order.TotalAmount))
                    {
                        _emailService.SendOrderConfirmation(order.CustomerEmail, order.Id);
                    }

                    order.IsProcessed = true;
                    return true;
                }
                catch (Exception)
                {
                    // Здесь предусмотрено место для логирования (ILogger)
                    return false;
                }
            }

            private void EnsureDatabaseConnection()
            {
                if (!_database.IsConnected)
                {
                    _database.Connect();
                }
            }

            private bool ShouldSendConfirmationEmail(decimal amount)
            {
                return amount > EmailNotificationThreshold;
            }
        }
    }
}
