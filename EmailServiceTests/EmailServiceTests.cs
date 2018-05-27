using System;
using EmailService.Contracts;
using EmailService.Service;
using Microsoft.Extensions.Logging;
using MockEmailClient;
using Moq;
using Xunit;

namespace EmailServiceTests
{
    public class EmailingServiceTests
    {
        private readonly EmailingService _sut;
        private readonly Mock<IEmailClient> _mockClient;
        private readonly Mock<ILogger<EmailingService>> _mockLogger;

        public EmailingServiceTests()
        {
            _mockClient = new Mock<IEmailClient>();
            _mockLogger = new Mock<ILogger<EmailingService>>();
            _sut = new EmailingService(_mockClient.Object, _mockLogger.Object);
        }

        [Fact]
        public void Should_Send_Emails_to_Email_Client()
        {
            var email = new Email { To = "George", Body = "Very Important!" };

            _sut.SendEmail(email);
            _sut.SendEmail(email);
            _sut.SendEmail(email);
            _sut.SendEmail(email);

            _mockClient.Verify(call => call.SendEmail(email.To, email.Body), Times.Exactly(4));
        }

        [Fact]
        public void Should_Handle_SendEmail_Failure()
        {
            var email = new Email { To = "George", Body = "Very Important!" };
            _mockClient.Setup(call => call.SendEmail(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception("Sending error"));

            var result = _sut.SendEmail(email);

            Assert.Equal("Failure.", result);
        }

        //TODO: More tests!

        [Fact]
        public void Should_Handle_Connection_Failure()
        {
            var email = new Email { To = "George", Body = "Very Important!" };
            _mockClient.Setup(call => call.SendEmail(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception("Connection Failed"));

            var result = _sut.SendEmail(email);

            Assert.Equal("Failure.", result);
        }

    }
}
