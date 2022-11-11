using System.Threading.Tasks.Dataflow;

namespace Dataflow
{
    public class Pipeline
    {
        private readonly PipelineConfiguration _configuration;

        private TransformBlock<string, string> _readerBlock;
        private TransformBlock<string, FileWithContent> _generatorBlock;
        private ActionBlock<FileWithContent> _writerBlock;


        public Pipeline(PipelineConfiguration configuration)
        {
            _configuration = configuration;

            _readerBlock = new TransformBlock<string, string>(
                async path => await ReadFile(path),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _configuration.MaxReadingTasks });

            _generatorBlock = new TransformBlock<string, FileWithContent>(
                source => ProcessFile(source),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _configuration.MaxProcessingTasks });

            _writerBlock = new ActionBlock<FileWithContent>(
                async fwc => await WriteFile(fwc),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _configuration.MaxWritingTasks });

            _readerBlock.LinkTo(_generatorBlock, new DataflowLinkOptions { PropagateCompletion = true });
            _generatorBlock.LinkTo(_writerBlock, new DataflowLinkOptions { PropagateCompletion = true });
        }

        public async Task PerformProcessing(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                _readerBlock.Post(file);
            }

            _readerBlock.Complete();

            await _writerBlock.Completion;
        }

        private async Task<string> ReadFile(string filePath)
        {
            string result;
            using (var streamReader = new StreamReader(filePath))
            {
                result = await streamReader.ReadToEndAsync();
            }
            return result;
        }

        private FileWithContent ProcessFile(string fileContent)
        {

        }

        private async Task WriteFile(FileWithContent fileWithContent)
        {
            using (var streamWriter = new StreamWriter(fileWithContent.Path))
            {
                await streamWriter.WriteAsync(fileWithContent.Content);
            }
        }
    }
}
