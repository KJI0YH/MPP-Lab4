namespace Dataflow
{
    public class Executor
    {
        public static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Invalid number of parameters\n" +
                    "Usage: <source files separated with \"|\"> <output directory>" +
                    "[max reading tasks] [max processing tasks] [max writing tasks]");

                return;
            }

            var sourceFiles = args[0].Split('|');
            var existingFiles = new List<string>();
            foreach (var file in sourceFiles)
            {
                if (File.Exists(file))
                {
                    existingFiles.Add(file);
                }
                else
                {
                    Console.Error.WriteLine($"File {file} does not exists or you do not have permissions to open it. It would be removed form generating");
                }
            }
            var outputDir = args[1];

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            int maxReadingTasks = 0;
            int maxProcessingTasks = 0;
            int maxWritingTasks = 0;

            if (args.Length > 2)
            {
                if (!int.TryParse(args[2], out maxReadingTasks))
                {
                    Console.Error.WriteLine($"Invalid max reading tasks number. Expected integer, got {args[2]}");
                    return;
                }
            }

            if (args.Length > 3)
            {
                if (!int.TryParse(args[3], out maxProcessingTasks))
                {
                    Console.Error.WriteLine($"Invalid max processing tasks number. Expected integer, got {args[3]}");
                    return;
                }
            }

            if (args.Length > 4)
            {
                if (!int.TryParse(args[4], out maxWritingTasks))
                {
                    Console.Error.WriteLine($"Invalid max writing tasks number. Expected integer, got {args[4]}");
                    return;
                }
            }


            PipelineConfiguration config = new PipelineConfiguration(
                maxReadingTasks == 0 ? PipelineConfiguration.DefaultReadingTasks : maxReadingTasks,
                maxProcessingTasks == 0 ? PipelineConfiguration.DefaultProcessigTasks : maxProcessingTasks,
                maxWritingTasks == 0 ? PipelineConfiguration.DefaultWritingTasks : maxWritingTasks
            );
            Pipeline pipeline = new Pipeline(config, outputDir);

            await pipeline.PerformProcessing(existingFiles);
        }
    }
}
