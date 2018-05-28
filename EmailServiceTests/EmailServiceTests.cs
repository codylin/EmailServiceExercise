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
        //Close method should propagate error up to caller
        [Fact]
        public void Should_Replicate_Connection_Failure()
        {
            var email = new Email { To = "George", Body = "Very Important!" };
            _mockClient.Setup(call => call.SendEmail(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Connection Failed"));

            var ex = Record.Exception(() => _sut.SendEmailHelper(email));
            Assert.NotNull(ex);
            Assert.IsType<Exception>(ex);
            Assert.Equal("Connection Failed", ex.Message);
        }
        
        //Close method should propagate error up to caller
        [Fact]
        public void Should_Replicate_Closing_Failure()
        {
            _mockClient.Setup(call => call.Close())
                .Throws(new Exception("Unexpected Error"));

            var ex = Record.Exception(() => _sut.Close());
            Assert.NotNull(ex);
            Assert.IsType<Exception>(ex);
            Assert.Equal("Unexpected Error", ex.Message);
        }
        //Testing different Paths for SendEmail
        //SendEmail should return fail if connection failed even after retrying
        [Fact]
        public void Should_Handle_Connetion_Failed_After_Retry()
        {
            var email = new Email { To = "George", Body = "Very Important!" };
            _mockClient.Setup(call => call.SendEmail(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Connection Failed"));
            
            var result = _sut.SendEmail(email);

            Assert.Equal("Failure.", result);
        }

        //SendEmail should retry 6 times before failing (hardcoded)
        [Fact]
        public void Should_Retry_Some_Amount_of_Times()
        {
            var email = new Email { To = "George", Body = "Very Important!" };
            _mockClient.Setup(call => call.SendEmail(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Connection Failed"));

            var result = _sut.SendEmail(email);
            // first try + 6 retries
            _mockClient.Verify(call => call.SendEmail(email.To, email.Body), Times.Exactly(7));
            Assert.Equal("Failure.", result);
        }
        //SendEmail should return fail for unexpected error
        [Fact]
        public void Should_Handle_Unexpected_Error()
        {
            var email = new Email { To = "George", Body = "Very Important!" };
            _mockClient.Setup(call => call.Close())
                .Throws(new Exception("Unexpected Error"));

            var result = _sut.SendEmail(email);

            Assert.Equal("Failure.", result);
        }
    }
}
