using Moq;
using NUnit.Framework;
using NotificationApp.Entities;
using NotificationApp.Interfaces;
using NotificationApp.Services;
using System;

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


    }
}
