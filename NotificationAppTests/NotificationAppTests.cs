using Moq;
using NUnit.Framework;
using NotificationApp.Entities;
using NotificationApp.Interfaces;
using NotificationApp.Services;
using System;
using System.ComponentModel.DataAnnotations;

namespace NotificationApp.Tests
{
    public class NotificationAppTests
    {
        private Mock<IUserRepository> _mockUserRepo;
        private Mock<INotifier> _mockNotifier;
        private NotificationService _notificationService;

        [SetUp]
        public void Setup()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockNotifier = new Mock<INotifier>();
            _notificationService = new NotificationService(_mockUserRepo.Object, _mockNotifier.Object);
        }

        [Test]
        public void NotifyUser_WithValidActiveUser_CallsSend()
        {
            // Arrange
            var user = new User { Id = 1, Email = "user@example.com", IsActive = true };

            // Act
            _mockUserRepo.Setup(repo => repo.GetUserById(1)).Returns(user);

            _notificationService.NotifyUser(1, "Hello!");

            // Assert: Verify that Send was called once with correct parameters
            _mockNotifier.Verify(n => n.Send("user@example.com", "Hello!"), Times.Once);

        }

        [Test]
        public void NotifyUser_WithInactiveUser_ThrowsInvalidOperationException()
        {
            // Arrange
            var user = new User { Id = 1, Email = "user@example.com", IsActive = false };
            _mockUserRepo.Setup(repo => repo.GetUserById(1)).Returns(user);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _notificationService.NotifyUser(1, "Hello!"));
            _mockNotifier.Verify(n => n.Send(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        }

        [Test]
        public void NotifyUser_WithNonExistentUser_ThrowsArgumentException()
        {
            // Arrange
            _mockUserRepo.Setup(repo => repo.GetUserById(It.IsAny<int>())).Returns((User)null);

            Assert.Throws<ArgumentException>(() => _notificationService.NotifyUser(1, "Hello!"));
            // Act & Assert

        }
        [Test]
        public void NotifyUser_WithEmptyMessage_ThrowsArgumentException()
        {
            //Arrange
            var user = new User { Id = 1, Email = "user@example.com", IsActive = true };
            _mockUserRepo.Setup(repo => repo.GetUserById(4)).Returns(user);

            //Act & Assert
            Assert.Throws<ArgumentException>(() => _notificationService.NotifyUser(4, ""));

        }
        [TestCase(1, "user1@example.com", true, "First message")]
        [TestCase(2, "user2@example.com", true, "Second message")]
        [TestCase(3, "user3@example.com", false, null)]

        public void NotifyUser_WithDifferentUsers_ShouldBehaveCorrectly(int userId, string email, bool isActive, string message)
        {
            var user = new User { Id = userId, Email = email, IsActive = isActive};
            _mockUserRepo.Setup(repo => repo.GetUserById(userId)).Returns(user);

            if (!isActive)
            {
                Assert.Throws<InvalidOperationException>(() => _notificationService.NotifyUser(userId, "Test message"));
            }
            else 
            {
                _notificationService.NotifyUser(userId, message);

                _mockNotifier.Verify(n => n.Send(email, message), Times.Once);

            }

        }
    }
}

