using System;
using System.IO;
using FluentAssertions;
using FluentFTP;
using Moq;
using NUnit.Framework;
using Polly;
using Polly.NoOp;
using Polly.Registry;
using Polly.Retry;

namespace Adapter.Persistence.Ftp.Tests.Unit
{
    public class FtpXboxGameRepositoryTests
    {
        private MoqFtpClientFactory _ftpClientFactory;
        private Mock<IFtpClient> _mockFtpClient;
        private PolicyRegistry _policyRegistry;

        [SetUp]
        public void Setup()
        {
            _mockFtpClient = new Mock<IFtpClient>();
            _ftpClientFactory = new MoqFtpClientFactory(_mockFtpClient);
            _policyRegistry = new PolicyRegistry();
            _policyRegistry.Add("Ftp", Policy.NoOp());
        }

        private FtpXboxGameRepository CreateSut()
        {
            return new FtpXboxGameRepository(_ftpClientFactory, new FtpXboxSettings()
            {
                GameRootDirectory = "F/Games"
            }, _policyRegistry);
        }
        
        [Test]
        public void StoreWithByteArray_WhenPolicyResult_HasFailed_ShouldThrowPersistenceException()
        {
            bool policyExecuted = false;
 
            _policyRegistry = new PolicyRegistry();
            _policyRegistry.Add("Ftp", Policy.Handle<FtpException>((ex) =>
            {
                policyExecuted = true;
                return true;
            }).Retry(0));
            
            _mockFtpClient.Setup(x => x.Connect());
            _mockFtpClient.Setup(x => x.IsConnected).Returns(true);
            _mockFtpClient.Setup(x => x.Upload(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<FtpRemoteExists>(),
                It.IsAny<bool>(), null)).Throws(new FtpException(""));
            
            var sut = CreateSut();
            sut.Connect();

            Action executeStore = () => sut.Store("GameA", "file1.txt", new byte[] {1, 2, 3, 4});
            
            // act
            executeStore.Should().Throw<PersistenceException>();

            // assert
            policyExecuted.Should().BeTrue();
        }
        
        [Test]
        public void StoreWithByteArray_WhenPolicyResult_IsSuccessful_ShouldNotThrow()
        {
            _mockFtpClient.Setup(x => x.Connect());
            _mockFtpClient.Setup(x => x.IsConnected).Returns(true);
            _mockFtpClient.Setup(x => x.Upload(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<FtpRemoteExists>(),
                It.IsAny<bool>(), null)).Returns(FtpStatus.Success);
            
            var sut = CreateSut();
            sut.Connect();
            Action executeStore = () => sut.Store("GameA", "file1.txt", new byte[] {1, 2, 3, 4});
            
            // act
            executeStore.Should().NotThrow();
        }
        
        [Test]
        public void StoreWithStream_WhenPolicyResult_HasFailed_ShouldThrowPersistenceException()
        {
            bool policyExecuted = false;
 
            _policyRegistry = new PolicyRegistry();
            _policyRegistry.Add("Ftp", Policy.Handle<FtpException>((ex) =>
            {
                policyExecuted = true;
                return true;
            }).Retry(0));
            
            _mockFtpClient.Setup(x => x.Connect());
            _mockFtpClient.Setup(x => x.IsConnected).Returns(true);
            _mockFtpClient.Setup(x => x.Upload(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<FtpRemoteExists>(),
                It.IsAny<bool>(), null)).Throws(new FtpException(""));
            
            var sut = CreateSut();
            sut.Connect();

            MemoryStream memoryStream = new MemoryStream(new byte[] { 1,2,3,4});
            Action executeStore = () => sut.Store("GameA", "file1.txt", memoryStream);
            
            // act
            executeStore.Should().Throw<PersistenceException>();

            // assert
            policyExecuted.Should().BeTrue();
        }
    }
}