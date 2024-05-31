namespace WorkerService1
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        [LogExecutionTimeAspect]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ExecuteLoop(stoppingToken);
        }

        [LogExecutionTimeAspect]
        private static async Task ExecuteLoop(CancellationToken stoppingToken)
        {
            foreach (var i in Enumerable.Range(0, 2000))
            {
                Console.WriteLine("Se escribirá un archivo");

                if (i % 2 == 0)
                {
                    var thread = new Thread(async () => await AppendTextToFile(stoppingToken, i));
                    //await AppendTextToFile(stoppingToken, i);
                    thread.Start();
                    Console.WriteLine(i);
                }
            }

            Console.WriteLine("El proceso terminó");
        }

        [LogExecutionTimeAspect]
        private static async Task AppendTextToFile(CancellationToken stoppingToken, int i)
        {
            await File.AppendAllTextAsync($"Files/pruebaNumerosPares{i}.txt", $"numero {i}", stoppingToken);
        }
    }
}