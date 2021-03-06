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
    public class SequentialUploadResumeStrategyTests
    {
        private readonly IProgressNotifier _notifier;
        private readonly IXboxGameRepository _xboxGameRepository;

        public SequentialUploadResumeStrategyTests()
        {
            _notifier = new ProgressNotifierInMemory();
            _xboxGameRepository = new XboxGameRepositoryInMemory(TimeSpan.FromMilliseconds(10));
        }
        
        private SequentialUploadResumeStrategy CreateSut(IList<IZipEntry> filesToCheck, string gameName = "TestGame")
        {
            return new SequentialUploadResumeStrategy(filesToCheck, _notifier, gameName, _xboxGameRepository);
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
        public void WhenFirstFileExistOnTargetXbox_AndIsTheSameSize_ShouldRemoveFileFromFilesAsRemaining()
        {
            IList<IZipEntry> filesToCheck = new List<IZipEntry>();
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile1", UncompressedSize = 4});
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile2", UncompressedSize = 8});
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile3", UncompressedSize = 16});
            
            var sut = CreateSut(filesToCheck);
            _xboxGameRepository.Store("TestGame", "TestFile1", new byte[] { 1,2,3,4});
            
            // act
            var uploadResumeReport = sut.GetRemainingFiles();
            
            // assert
            uploadResumeReport.RemainingFiles.Count().Should().Be(2);
            uploadResumeReport.RemainingFiles.Should().NotContain(x => x.FileName == "TestFile1");
            uploadResumeReport.RemainingFiles.Select(x => x.FileName).Should().BeEquivalentTo(new[] {"TestFile2", "TestFile3"});
            uploadResumeReport.SizeUploaded.Should().Be(4);
        }
        
        [Fact]
        public void WhenFirstFileExistOnTargetXbox_AndIsNotTheSameSize_ShouldIncludeFileFromFilesAsRemaining()
        {
            IList<IZipEntry> filesToCheck = new List<IZipEntry>();
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile1", UncompressedSize = 8});
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile2", UncompressedSize = 4});
            filesToCheck.Add(new ZipEntryFake() { FileName = "TestFile3", UncompressedSize = 4});
            
            var sut = CreateSut(filesToCheck);
            _xboxGameRepository.Store("TestGame", "TestFile1", new byte[] { 1,2,3,4});
            
            // act
            var uploadResumeReport = sut.GetRemainingFiles();
            
            // assert
            uploadResumeReport.RemainingFiles.Count().Should().Be(3);
            uploadResumeReport.RemainingFiles.Select(x => x.FileName).Should().BeEquivalentTo(new[] {"TestFile1", "TestFile2", "TestFile3"});
            uploadResumeReport.SizeUploaded.Should().Be(0);
        }
    }
}
