using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PZ_8_Test.OrederProcessor;
using Xunit;

namespace PZ_8_Test
{
    public class OrderProcessorTests
    {
        [Fact]
        public void ProcessOrder_ReturnsTrue_WhenOrderIsValid()
        {
            var dbMock = new Mock<IDatabase>();
            dbMock.Setup(db => db.IsConnected).Returns(true);

            var emailMock = new Mock<IEmailService>();

            var processor = new OrderProcessor(dbMock.Object, emailMock.Object);

            var order = new Order { Id = 1, CustomerEmail = "test@mail.com", TotalAmount = 50 };

            var result = processor.ProcessOrder(order);

            Assert.True(result);
            Assert.True(order.IsProcessed);
            dbMock.Verify(db => db.Save(order), Times.Once);
        }

        
        [Fact]
        public void ProcessOrder_ConnectsToDatabase_WhenNotConnected()
        {
            var dbMock = new Mock<IDatabase>();
            dbMock.Setup(db => db.IsConnected).Returns(false);

            var emailMock = new Mock<IEmailService>();

            var processor = new OrderProcessor(dbMock.Object, emailMock.Object);

            var order = new Order { Id = 2, CustomerEmail = "a@mail.com", TotalAmount = 30 };

            processor.ProcessOrder(order);

            dbMock.Verify(db => db.Connect(), Times.Once);
        }

        
        [Fact]
        public void ProcessOrder_ReturnsFalse_WhenTotalAmountIsZero()
        {
            var dbMock = new Mock<IDatabase>();
            var emailMock = new Mock<IEmailService>();

            var processor = new OrderProcessor(dbMock.Object, emailMock.Object);

            var order = new Order { Id = 3, TotalAmount = 0 };

            var result = processor.ProcessOrder(order);

            Assert.False(result);
            Assert.False(order.IsProcessed);
            dbMock.Verify(db => db.Save(It.IsAny<Order>()), Times.Never);
        }

       
        [Fact]
        public void ProcessOrder_ThrowsException_WhenOrderIsNull()
        {
            var dbMock = new Mock<IDatabase>();
            var emailMock = new Mock<IEmailService>();
            var processor = new OrderProcessor(dbMock.Object, emailMock.Object);

            Assert.Throws<ArgumentNullException>(() => processor.ProcessOrder(null));
        }

        
        [Fact]
        public void ProcessOrder_SendsEmail_WhenAmountIsGreaterThan100()
        {
            var dbMock = new Mock<IDatabase>();
            dbMock.Setup(db => db.IsConnected).Returns(true);

            var emailMock = new Mock<IEmailService>();

            var processor = new OrderProcessor(dbMock.Object, emailMock.Object);

            var order = new Order
            {
                Id = 5,
                CustomerEmail = "buyer@mail.com",
                TotalAmount = 150
            };

            processor.ProcessOrder(order);

            emailMock.Verify(e => e.SendOrderConfirmation(order.CustomerEmail, order.Id), Times.Once);
        }

        
       
        [Fact]
        public void ProcessOrder_DoesNotSendEmail_WhenAmountIs100()
        {
            var dbMock = new Mock<IDatabase>();
            dbMock.Setup(db => db.IsConnected).Returns(true);

            var emailMock = new Mock<IEmailService>();

            var processor = new OrderProcessor(dbMock.Object, emailMock.Object);

            var order = new Order
            {
                Id = 6,
                CustomerEmail = "no@mail.com",
                TotalAmount = 100
            };

            processor.ProcessOrder(order);

            emailMock.Verify(e => e.SendOrderConfirmation(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

       
        [Fact]
        public void ProcessOrder_ReturnsFalse_WhenDatabaseThrows()
        {
            var dbMock = new Mock<IDatabase>();
            dbMock.Setup(db => db.IsConnected).Returns(true);
            dbMock.Setup(db => db.Save(It.IsAny<Order>())).Throws(new Exception("DB error"));

            var emailMock = new Mock<IEmailService>();

            var processor = new OrderProcessor(dbMock.Object, emailMock.Object);

            var order = new Order { Id = 7, TotalAmount = 50 };

            var result = processor.ProcessOrder(order);

            Assert.False(result);
            Assert.False(order.IsProcessed);
            emailMock.Verify(e => e.SendOrderConfirmation(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

       
        [Fact]
        public void Constructor_Throws_WhenDatabaseIsNull()
        {
            var emailMock = new Mock<IEmailService>();

            Assert.Throws<ArgumentNullException>(() => new OrderProcessor(null, emailMock.Object));
        }

        [Fact]
        public void Constructor_Throws_WhenEmailServiceIsNull()
        {
            var dbMock = new Mock<IDatabase>();

            Assert.Throws<ArgumentNullException>(() => new OrderProcessor(dbMock.Object, null));
        }
    }
}
