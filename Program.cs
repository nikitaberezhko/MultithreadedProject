using System.Diagnostics;

namespace MultithreadedProject;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Выполнение...\n");
        
        Console.WriteLine("Последовательное выполнение");
        Test(ConsistentExecute);

        Console.WriteLine("Параллельное выполнение");
        Test(ParallelExecute);
        
        Console.WriteLine("PLINQ выполнение");
        Test(PLinqExecute);
    }

    private static void Test(Func<int, (int result, TimeSpan time)> func)
    {
        var (result1, time1) = func(100_000);
        Console.WriteLine($"100.000 элементов. Результат: {result1}, время выполнения: {time1.Ticks} тиков");
        
        var (result2, time2) = func(1_000_000);
        Console.WriteLine($"1.000.000 элементов. Результат: {result2}, время выполнения: {time2.Ticks} тиков");
        
        var (result3, time3) = func(10_000_000);
        Console.WriteLine($"10.000.000 элементов. Результат: {result3}, время выполнения: {time3.Ticks} тиков");
        Console.WriteLine();
    }
    
    private static (int result, TimeSpan time)  ConsistentExecute(int size)
    {
        var result = 0;
        var sw = new Stopwatch();
        var list = GenerateList(size);
        
        sw.Start();
        foreach (var el in list) 
            result += el;
        sw.Stop();
        
        var time = sw.Elapsed;
        return (result, time);
    }
    
    private static (int result, TimeSpan time)  ParallelExecute(int size)
    {
        var result = 0;
        var sw = new Stopwatch();
        var list = GenerateList(size);
        
        object lockObj = new object();
        
        sw.Start();
        Parallel.For(0, list.Count, (n) =>
        {
            lock (lockObj) 
                result += list[n];
        });
        sw.Stop();
        
        var time = sw.Elapsed;
        return (result, time);
    }
    
    private static (int result, TimeSpan time)  PLinqExecute(int size)
    {
        var result = 0;
        var sw = new Stopwatch();
        var list = GenerateList(size);
        
        sw.Start();
        var sum = list.AsParallel().Sum();
        sw.Stop();

        result = sum;
        var time = sw.Elapsed;
        return (result, time);
    }
    
    private static List<int> GenerateList(int size)
    {
        var list = new List<int>();
        var random = new Random();
        for (int i = 0; i < size; i++)
        {
            list.Add(random.Next(0, 100));
        }

        return list;
    }
}