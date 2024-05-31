using AspectInjector.Broker;
using System.Diagnostics;
using Serilog;

namespace WorkerService1
{
    [Aspect(Scope.Global)]
    [Injection(typeof(LogExecutionTimeAspect))]
    public class LogExecutionTimeAspect : Attribute
    {
        private Stopwatch stopwatch;
        private long startMemory;
        private long startCpuTime;
        private PerformanceCounter diskTimeCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
        private PerformanceCounter cpuTimeCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private PerformanceCounter diskReadTime = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
        private PerformanceCounter diskWriteTime = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
        private int startThreadCount;

        [Advice(Kind.Before, Targets = Target.Method)]
        public void OnEntry([Argument(Source.Name)] string name)
        {
            // Guarda el tiempo de inicio, la memoria utilizada y el tiempo de CPU en las variables de seguimiento
            stopwatch = Stopwatch.StartNew();
            startMemory = GC.GetTotalMemory(false);
            startCpuTime = Process.GetCurrentProcess().TotalProcessorTime.Ticks;
            diskTimeCounter.NextValue();
            cpuTimeCounter.NextValue();
            diskReadTime.NextValue();
            diskWriteTime.NextValue();
            startThreadCount = Process.GetCurrentProcess().Threads.Count;
        }

        [Advice(Kind.After, Targets = Target.Method)]
        public void OnExit([Argument(Source.Name)] string name)
        {
            // Obtiene el tiempo transcurrido, la memoria utilizada y el tiempo de CPU y los registra
            stopwatch.Stop();
            var elapsedMemory = (GC.GetTotalMemory(false) - startMemory) / (1024 * 1024); // Convertir a megabytes
            var elapsedCpuTime = (Process.GetCurrentProcess().TotalProcessorTime.Ticks - startCpuTime) / TimeSpan.TicksPerSecond; // Convertir a segundos
            var elapsedSeconds = stopwatch.Elapsed.TotalSeconds; // Convertir a segundos
            var diskTime = diskTimeCounter.NextValue();
            var cpuTime = cpuTimeCounter.NextValue();
            var diskRead = diskReadTime.NextValue() / (1024 * 1024);
            var diskWrite = diskWriteTime.NextValue() / (1024 * 1024);
            var threadCount = Process.GetCurrentProcess().Threads.Count - startThreadCount;
            Log.Information(",{MethodName}, {Time}, {Memory}, {CpuTime}, {diskTime}, {diskRead}, {diskWrite}, {CpuPercent}, {ThreadCountValue}",
                name, elapsedSeconds, elapsedMemory, elapsedCpuTime, diskTime, diskRead, diskWrite, cpuTime, threadCount);
        }
    }
}