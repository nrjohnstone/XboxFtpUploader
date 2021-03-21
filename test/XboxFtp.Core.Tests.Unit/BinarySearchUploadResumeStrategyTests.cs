using System;
using System.Collections.Generic;
using System.Linq;
using Adapter.Persistence.InMemory;
using FluentAssertions;
using XboxFtp.Core.Entities;
using XboxFtp.Core.Ports.Notification;
using XboxFtp.Core.Ports.Persistence;
using XboxFtp.Core.UseCases;
using Xunit;

namespace XboxFtp.Core.Tests.Unit
{
    public class BinarySearchUploadResumeStrategyTests
    { 
        private readonly IProgressNotifier _notifier;
        private readonly IXboxGameRepository _xboxGameRepository;

        public BinarySearchUploadResumeStrategyTests()
        {
            _notifier = new ProgressNotifierInMemory();
            _xboxGameRepository = new XboxGameRepositoryInMemory(TimeSpan.FromMilliseconds(10));
        }
        
        private BinarySearchUploadResumeStrategy CreateSut(IList<IZipEntry> filesToCheck, string gameName = "TestGame")
        {
            return new BinarySearchUploadResumeStrategy(filesToCheck, _notifier, gameName, _xboxGameRepository);
        }
        
        [Fact]
        public void WhenNoFilesExistOnTargetXbox_ShouldReturnAllFilesAsRemaining()
        {
            IList<IZipEntry> filesToCheck = new List<IZipEntry>();
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile1"});
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile2"});
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile3"});
            
            var sut = CreateSut(filesToCheck);

            // act
            var uploadResumeReport = sut.GetRemainingFiles();
            
            // assert
            uploadResumeReport.RemainingFiles.Count().Should().Be(3);
            uploadResumeReport.RemainingFiles.Select(x => x.FileName).Should().BeEquivalentTo(new[] {"TestFile1", "TestFile2", "TestFile3"});
        }
        
        [Fact]
        public void When50PercentOfFilesExistOnTargetXbox_ShouldReturnOtherFilesAsRemaining()
        {
            IList<IZipEntry> filesToCheck = new List<IZipEntry>();
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile1", UncompressedSize = 4});
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile2", UncompressedSize = 4});
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile3", UncompressedSize = 4});
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile4", UncompressedSize = 4});
            
            var sut = CreateSut(filesToCheck);
            
            _xboxGameRepository.Store("TestGame", "TestFile1", new byte[] { 1,2,3,4});
            _xboxGameRepository.Store("TestGame", "TestFile2", new byte[] { 1,2,3,4});

            // act
            var uploadResumeReport = sut.GetRemainingFiles();
            
            // assert
            uploadResumeReport.RemainingFiles.Count().Should().Be(2);
            uploadResumeReport.RemainingFiles.Select(x => x.FileName).Should().BeEquivalentTo(new[] {"TestFile3", "TestFile4"});
            uploadResumeReport.SizeUploaded.Should().Be(8);
        }
        
        [Fact]
        public void WhenFilesExistOnTargetXbox_ShouldReturnCorrectSizeUploaded()
        {
            IList<IZipEntry> filesToCheck = new List<IZipEntry>();
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile1", UncompressedSize = 4});
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile2", UncompressedSize = 8});
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile3", UncompressedSize = 16});
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile4", UncompressedSize = 32});
            
            var sut = CreateSut(filesToCheck);
            
            _xboxGameRepository.Store("TestGame", "TestFile1", new byte[] { 1,2,3,4});
            _xboxGameRepository.Store("TestGame", "TestFile2", new byte[] { 1,2,3,4,5,6,7,8});

            // act
            var uploadResumeReport = sut.GetRemainingFiles();
            
            // assert
            uploadResumeReport.RemainingFiles.Count().Should().Be(2);
            uploadResumeReport.RemainingFiles.Select(x => x.FileName).Should().BeEquivalentTo(new[] {"TestFile3", "TestFile4"});
            uploadResumeReport.SizeUploaded.Should().Be(12);
        }
        
        [Fact]
        public void WhenAFileIsNotCorrectSize_ShouldResumeAtThatFile()
        {
            IList<IZipEntry> filesToCheck = new List<IZipEntry>();
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile1", UncompressedSize = 4});
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile2", UncompressedSize = 4});
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile3", UncompressedSize = 4});
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile4", UncompressedSize = 4});
            
            var sut = CreateSut(filesToCheck);
            
            _xboxGameRepository.Store("TestGame", "TestFile1", new byte[] { 1,2,3,4});
            _xboxGameRepository.Store("TestGame", "TestFile2", new byte[] { 1,2});

            // act
            var uploadResumeReport = sut.GetRemainingFiles();
            
            // assert
            uploadResumeReport.RemainingFiles.Count().Should().Be(3);
            uploadResumeReport.RemainingFiles.Select(x => x.FileName).Should().BeEquivalentTo(new[] {"TestFile2", "TestFile3", "TestFile4"});
            uploadResumeReport.SizeUploaded.Should().Be(4);
        }
        
        [Fact]
        public void WhenAllFilesExistsOnXbox_ShouldReturnNoRemaingFilesToUpload()
        {
            IList<IZipEntry> filesToCheck = new List<IZipEntry>();
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile1", UncompressedSize = 4});
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile2", UncompressedSize = 4});
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile3", UncompressedSize = 4});
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile4", UncompressedSize = 4});
            
            var sut = CreateSut(filesToCheck);
            
            _xboxGameRepository.Store("TestGame", "TestFile1", new byte[] { 1,2,3,4});
            _xboxGameRepository.Store("TestGame", "TestFile2", new byte[] { 1,2,3,4});
            _xboxGameRepository.Store("TestGame", "TestFile3", new byte[] { 1,2,3,4});
            _xboxGameRepository.Store("TestGame", "TestFile4", new byte[] { 1,2,3,4});

            // act
            var uploadResumeReport = sut.GetRemainingFiles();
            
            // assert
            uploadResumeReport.RemainingFiles.Count().Should().Be(0);
            uploadResumeReport.SizeUploaded.Should().Be(16);
        }
        
        [Theory]
        [InlineData(100, 3)]
        [InlineData(100, 50)]
        [InlineData(100, 49)]
        [InlineData(100, 51)]
        [InlineData(100, 99)]
        public void WhenANumberOfFilesExistsOnXbox_ShouldReturnExpectedNumberOfFilesToUpload(int totalFileCount, int existingFileCount)
        {
            IList<IZipEntry> filesToCheck = new List<IZipEntry>();
            for (int i = 0; i < totalFileCount; i++)
            {
                filesToCheck.Add(new ZipEntryFake() { FileName = $"TestFile{i}", UncompressedSize = 4});    
            }
            
            var sut = CreateSut(filesToCheck);

            for (int i = 0; i < existingFileCount; i++)
            {
                _xboxGameRepository.Store("TestGame", $"TestFile{i}", new byte[] { 1,2,3,4});    
            }
            
            // act
            var uploadResumeReport = sut.GetRemainingFiles();
            
            // assert
            uploadResumeReport.RemainingFiles.Count().Should().Be(totalFileCount - existingFileCount);
            for (int i = 0; i < totalFileCount - existingFileCount; i++)
            {
                string expectedFileName = $"TestFile{i + existingFileCount}";
                uploadResumeReport.RemainingFiles[i].FileName.Should().Be(expectedFileName);
            }
        }
    }
}