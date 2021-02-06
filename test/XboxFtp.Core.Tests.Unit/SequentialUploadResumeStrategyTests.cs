using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Adapter.Persistence.InMemory;
using FluentAssertions;
using Ionic.Zip;
using XboxFtp.Core.Entities;
using XboxFtp.Core.Ports.Notification;
using XboxFtp.Core.Ports.Persistence;
using XboxFtp.Core.UseCases;
using Xunit;

namespace XboxFtp.Core.Tests.Unit
{
    internal class ZipFileFake : IZipEntry
    {
        public string FileName { get; set; }
        public long UncompressedSize { get; set; }
        
        public void Extract(string baseDirectory, ExtractExistingFileAction extractExistingFileAction)
        {
        }

        public Stream OpenReader()
        {
            return new MemoryStream();
        }
    }
    
    public class SequentialUploadResumeStrategyTests
    {
        private readonly IProgressNotifier _notifier;
        private readonly IXboxGameRepository _xboxGameRepository;

        public SequentialUploadResumeStrategyTests()
        {
            _notifier = new ProgressNotifierInMemory();
            _xboxGameRepository = new XboxGameRepositoryInMemory(TimeSpan.FromMilliseconds(10));
        }
        
        private SequentialUploadResumeStrategy CreateSut(Queue<IZipEntry> filesToCheck, string gameName = "TestGame")
        {
            return new SequentialUploadResumeStrategy(filesToCheck, _notifier, gameName, _xboxGameRepository);
        }
        
        [Fact]
        public void WhenNoFilesExistOnTargetXbox_ShouldReturnAllFilesAsRemaining()
        {
            Queue<IZipEntry> filesToCheck = new Queue<IZipEntry>();
            filesToCheck.Enqueue(new ZipFileFake() { FileName = "TestFile1"});
            filesToCheck.Enqueue(new ZipFileFake() { FileName = "TestFile2"});
            filesToCheck.Enqueue(new ZipFileFake() { FileName = "TestFile3"});
            
            var sut = CreateSut(filesToCheck);

            // act
            var remainingFiles = sut.GetRemainingFiles();
            
            // assert
            remainingFiles.Count().Should().Be(3);
            remainingFiles.Select(x => x.FileName).Should().BeEquivalentTo(new[] {"TestFile1", "TestFile2", "TestFile3"});
        }
        
        [Fact]
        public void WhenFirstFileExistOnTargetXbox_AndIsTheSameSize_ShouldRemoveFileFromFilesAsRemaining()
        {
            Queue<IZipEntry> filesToCheck = new Queue<IZipEntry>();
            filesToCheck.Enqueue(new ZipFileFake() { FileName = "TestFile1", UncompressedSize = 4});
            filesToCheck.Enqueue(new ZipFileFake() { FileName = "TestFile2", UncompressedSize = 4});
            filesToCheck.Enqueue(new ZipFileFake() { FileName = "TestFile3", UncompressedSize = 4});
            
            var sut = CreateSut(filesToCheck);
            _xboxGameRepository.Store("TestGame", "TestFile1", new byte[] { 1,2,3,4});
            
            // act
            var remainingFiles = sut.GetRemainingFiles();
            
            // assert
            remainingFiles.Count().Should().Be(2);
            remainingFiles.Should().NotContain(x => x.FileName == "TestFile1");
            remainingFiles.Select(x => x.FileName).Should().BeEquivalentTo(new[] {"TestFile2", "TestFile3"});
        }
        
        [Fact]
        public void WhenFirstFileExistOnTargetXbox_AndIsNotTheSameSize_ShouldIncludeFileFromFilesAsRemaining()
        {
            Queue<IZipEntry> filesToCheck = new Queue<IZipEntry>();
            filesToCheck.Enqueue(new ZipFileFake() { FileName = "TestFile1", UncompressedSize = 8});
            filesToCheck.Enqueue(new ZipFileFake() { FileName = "TestFile2", UncompressedSize = 4});
            filesToCheck.Enqueue(new ZipFileFake() { FileName = "TestFile3", UncompressedSize = 4});
            
            var sut = CreateSut(filesToCheck);
            _xboxGameRepository.Store("TestGame", "TestFile1", new byte[] { 1,2,3,4});
            
            // act
            var remainingFiles = sut.GetRemainingFiles();
            
            // assert
            remainingFiles.Count().Should().Be(3);
            remainingFiles.Select(x => x.FileName).Should().BeEquivalentTo(new[] {"TestFile1", "TestFile2", "TestFile3"});
        }
    }
}
