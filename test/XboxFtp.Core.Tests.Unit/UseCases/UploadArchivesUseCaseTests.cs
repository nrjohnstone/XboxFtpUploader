using Adapter.Persistence.InMemory;
using System;
using System.Collections.Generic;
using System.Text;
using XboxFtp.Core.Entities;
using XboxFtp.Core.Ports.Notification;
using XboxFtp.Core.UseCases;
using Xunit;

namespace XboxFtp.Core.Tests.Unit.UseCases
{
    public class UploadArchivesUseCaseTests
    {
        private XboxGameRepositoryFactory _xboxGameRepositoryFactory;
        private ProgressNotifierInMemory _notifier;
        private Dictionary<string, long> _data;
        private ZipFileProcessorFake _zipFileProcessor;

        public UploadArchivesUseCaseTests()
        {
            _data = new Dictionary<string, long>();
            _xboxGameRepositoryFactory = new XboxGameRepositoryFactory(_data, TimeSpan.FromMilliseconds(5));
            _notifier = new ProgressNotifierInMemory();
            _zipFileProcessor = new ZipFileProcessorFake();
        }

        [Fact]
        public void Should()
        {
            var sut = CreateSut();
            IZipFile zipFileFake = new ZipFileFake(new List<IZipEntry>() { new ZipEntryFake() }); 
            _zipFileProcessor.SetZipFile(zipFileFake);

            sut.Execute(new List<string> { "C:/GameFolder/Game1" });
        }

        private UploadArchivesUseCase CreateSut()
        {
            return new UploadArchivesUseCase(_xboxGameRepositoryFactory, _notifier, _zipFileProcessor);
        }
    }
}
