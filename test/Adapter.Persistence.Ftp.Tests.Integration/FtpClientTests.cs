using System;
using System.IO;
using FluentAssertions;
using FluentFTP;
using NUnit.Framework;

namespace Adapter.Persistence.Ftp.Tests.Integration
{
    public class FtpClientTests
    {
        private FtpClient _ftpClient;

        [SetUp]
        public void Setup()
        {
            _ftpClient = CreateFtpClient();
            _ftpClient.Connect();
        }

        private FtpClient CreateFtpClient()
        {
            return new FtpClient(host:  Global.Settings.XboxFtpHost, user: Global.Settings.XboxFtpUser, pass: Global.Settings.XboxFtpPassword);
        }

        [Test]
        public void Connect_WhenPasswordIsIncorrect_ShouldThrow()
        {
            FtpXboxSettings settings = new FtpXboxSettings()
            {
                Host = Global.Settings.XboxFtpHost,
                User = Global.Settings.XboxFtpUser,
                Password = "incorrectPassword",
                GameRootDirectory = "Games"
            };
            
            var sut = new FtpXboxGameRepository(new FtpClientFactory(settings), settings);
            
            // act            
            Action connectWithIncorrectPassword = () => sut.Connect();

            // assert
            connectWithIncorrectPassword.Should().Throw<FtpException>();
        }
        
        [Test]
        public void Connect_WhenUserIsIncorrect_ShouldThrow()
        {
            FtpXboxSettings settings = new FtpXboxSettings()
            {
                Host = Global.Settings.XboxFtpHost,
                User = "incorrect_user",
                Password = Global.Settings.XboxFtpPassword,
                GameRootDirectory = "Games"
            };
            
            var sut = new FtpXboxGameRepository(new FtpClientFactory(settings), settings);
            
            // act            
            Action connectWithIncorrectUser = () => sut.Connect();

            // assert
            connectWithIncorrectUser.Should().Throw<FtpException>();
        }
        
        [Test]
        public void Connect_WhenCredentialsAreValid_ShouldConnect()
        {
            FtpXboxSettings settings = new FtpXboxSettings()
            {
                Host = Global.Settings.XboxFtpHost,
                User =  Global.Settings.XboxFtpUser,
                Password = Global.Settings.XboxFtpPassword,
                GameRootDirectory = "Games"
            };
            
            var sut = new FtpXboxGameRepository(new FtpClientFactory(settings), settings);
            
            // act            
            Action connectWithValidCredentials = () => sut.Connect();

            // assert
            connectWithValidCredentials.Should().NotThrow<Exception>();
        }
        
        [Test]
        public void Disconnect_WhenConnected_ShouldNotThrow()
        {
            FtpXboxSettings settings = new FtpXboxSettings()
            {
                Host = Global.Settings.XboxFtpHost,
                User =  Global.Settings.XboxFtpUser,
                Password = Global.Settings.XboxFtpPassword,
                GameRootDirectory = "Games"
            };
            
            var sut = new FtpXboxGameRepository(new FtpClientFactory(settings), settings);
            sut.Connect();
            
            // act            
            Action disconnect = () => sut.Disconnect();

            // assert
            disconnect.Should().NotThrow<Exception>();
        }
        
        [Test]
        public void Exists_WhenPathDoesNotExist_ShouldReturnFalse()
        {
            FtpXboxSettings settings = new FtpXboxSettings()
            {
                Host = Global.Settings.XboxFtpHost,
                User =  Global.Settings.XboxFtpUser,
                Password = Global.Settings.XboxFtpPassword,
                GameRootDirectory = "Games"
            };
            
            var sut = new FtpXboxGameRepository(new FtpClientFactory(settings), settings);
            sut.Connect();
            
            // act            
            var exists = sut.Exists("someGameNameThatDoesNotExist", "filePathThatDoesNotExist.txt", 5000);

            // assert
            exists.Should().BeFalse();
        }
        
        [Test]
        public void Exists_WhenGameFolderExists_ButPathDoesNot_ShouldReturnFalse()
        {
            FtpXboxSettings settings = new FtpXboxSettings()
            {
                Host = Global.Settings.XboxFtpHost,
                User =  Global.Settings.XboxFtpUser,
                Password = Global.Settings.XboxFtpPassword,
                GameRootDirectory = "Games"
            };
            
            var sut = new FtpXboxGameRepository(new FtpClientFactory(settings), settings);
            sut.Connect();
            
            // act            
            var exists = sut.Exists("GameA", "fileThatDoesNotExist.txt", 5000);

            // assert
            exists.Should().BeFalse();
        }
        
        [Test]
        public void Exists_WhenGameFolderExistsAndFileExists_ButSizeIsDifferent_ShouldReturnFalse()
        {
            FtpXboxSettings settings = new FtpXboxSettings()
            {
                Host = Global.Settings.XboxFtpHost,
                User =  Global.Settings.XboxFtpUser,
                Password = Global.Settings.XboxFtpPassword,
                GameRootDirectory = "Games"
            };
            
            var sut = new FtpXboxGameRepository(new FtpClientFactory(settings), settings);
            sut.Connect();
            
            // act            
            var exists = sut.Exists("GameA", "SampleFile.txt", 20);

            // assert
            exists.Should().BeFalse();
        }
        
        [Test]
        public void Exists_WhenGameFolderExistsAndFileExistsAndSizeIsEqual_ShouldReturnTrue()
        {
            FtpXboxSettings settings = new FtpXboxSettings()
            {
                Host = Global.Settings.XboxFtpHost,
                User =  Global.Settings.XboxFtpUser,
                Password = Global.Settings.XboxFtpPassword,
                GameRootDirectory = "/F/Games"
            };
            
            var sut = new FtpXboxGameRepository(new FtpClientFactory(settings), settings);
            sut.Connect();
            
            // act            
            var exists = sut.Exists("GameA", "SampleFile.txt", 17);

            // assert
            exists.Should().BeTrue();
        }

        [Test]
        public void CreateDirectory_ShouldCreateDirectory()
        {
            FtpXboxSettings settings = new FtpXboxSettings()
            {
                Host = Global.Settings.XboxFtpHost,
                User =  Global.Settings.XboxFtpUser,
                Password = Global.Settings.XboxFtpPassword,
                GameRootDirectory = "/F/Games"
            };
            
            var sut = new FtpXboxGameRepository(new FtpClientFactory(settings), settings);
            sut.Connect();
            
            // act
            sut.CreateDirectory("/F/Games/GameB");
            
            // assert
            bool exists = _ftpClient.DirectoryExists("/F/Games/GameB");
            exists.Should().BeTrue();
        }
        
        [Test]
        public void CreateGame_ShouldCreateDirectoryUnderGameRootDirectory()
        {
            FtpXboxSettings settings = new FtpXboxSettings()
            {
                Host = Global.Settings.XboxFtpHost,
                User =  Global.Settings.XboxFtpUser,
                Password = Global.Settings.XboxFtpPassword,
                GameRootDirectory = "/F/Games"
            };
            
            var sut = new FtpXboxGameRepository(new FtpClientFactory(settings), settings);
            sut.Connect();
            
            // act
            sut.CreateGame("GameC");
            
            // assert
            bool exists = _ftpClient.DirectoryExists("/F/Games/GameC");
            exists.Should().BeTrue();
        }
        
        [Test]
        public void Store_WhenProvidingAByteArray_ShouldStoreFileCorrectly()
        {
            FtpXboxSettings settings = new FtpXboxSettings()
            {
                Host = Global.Settings.XboxFtpHost,
                User =  Global.Settings.XboxFtpUser,
                Password = Global.Settings.XboxFtpPassword,
                GameRootDirectory = "/F/Games"
            };
            
            var sut = new FtpXboxGameRepository(new FtpClientFactory(settings), settings);
            sut.Connect();
            byte[] data = File.ReadAllBytes("SampleFileB.txt");
            
            // act
            sut.Store("GameA", "SampleFileB.txt", data);
            
            // assert
            bool exists = _ftpClient.FileExists("/F/Games/GameA/SampleFileB.txt");
            exists.Should().BeTrue();
            FtpListItem fileInfo = _ftpClient.GetObjectInfo("/F/Games/GameA/SampleFileB.txt");
            fileInfo.Size.Should().Be(31);
            MemoryStream memoryStream = new MemoryStream();
            _ftpClient.Download(memoryStream, "/F/Games/GameA/SampleFileB.txt");
            memoryStream.Length.Should().Be(31);
            var actualData = memoryStream.ToArray();
            actualData.Should().BeEquivalentTo(data);
        }
        
        [Test]
        public void Store_WhenProvidingAStrean_ShouldStoreFileCorrectly()
        {
            FtpXboxSettings settings = new FtpXboxSettings()
            {
                Host = Global.Settings.XboxFtpHost,
                User =  Global.Settings.XboxFtpUser,
                Password = Global.Settings.XboxFtpPassword,
                GameRootDirectory = "/F/Games"
            };
            
            var sut = new FtpXboxGameRepository(new FtpClientFactory(settings), settings);
            sut.Connect();
            MemoryStream data = new MemoryStream();
            data.Write(File.ReadAllBytes("SampleFileC.txt"));
            
            // act
            sut.Store("GameA", "SampleFileC.txt", data);
            
            // assert
            bool exists = _ftpClient.FileExists("/F/Games/GameA/SampleFileC.txt");
            exists.Should().BeTrue();
            FtpListItem fileInfo = _ftpClient.GetObjectInfo("/F/Games/GameA/SampleFileC.txt");
            fileInfo.Size.Should().Be(42);
            MemoryStream memoryStream = new MemoryStream();
            _ftpClient.Download(memoryStream, "/F/Games/GameA/SampleFileC.txt");
            memoryStream.Length.Should().Be(42);
            var actualData = memoryStream.ToArray();
            actualData.Should().BeEquivalentTo(data.ToArray());
        }
    }
}