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

    private static void Test(Func<int, (long result, TimeSpan time)> func)
    {
        var (result1, time1) = func(100_000);
        Console.WriteLine($"100.000 элементов. Результат: {result1}, время выполнения: {time1.Ticks} тиков");
        
        var (result2, time2) = func(1_000_000);
        Console.WriteLine($"1.000.000 элементов. Результат: {result2}, время выполнения: {time2.Ticks} тиков");
        
        var (result3, time3) = func(10_000_000);
        Console.WriteLine($"10.000.000 элементов. Результат: {result3}, время выполнения: {time3.Ticks} тиков");
        Console.WriteLine();
    }
    
    private static (long result, TimeSpan time)  ConsistentExecute(int size)
    {
        long result = 0;
        var sw = new Stopwatch();
        var list = GenerateList(size);
        
        sw.Start();
        foreach (var el in list) 
            result += el;
        sw.Stop();
        
        var time = sw.Elapsed;
        return (result, time);
    }
    
    private static (long result, TimeSpan time)  ParallelExecute(int size)
    {
        long result = 0;
        var sw = new Stopwatch();
        var list = GenerateList(size);
        
        object lockObj = new object();
        
        var subList1 = list.GetRange(0, size / 4);
        var subList2 = list.GetRange((size - 1)  / 4, size / 4);
        var subList3 = list.GetRange((size - 1)  / 2, size / 4);
        var subList4 = list.GetRange((size - 1)  / 4 * 3, size / 4);
        
        sw.Start();

        Action<List<int>> action = (List<int> list) =>
        {
            foreach (var el in list)
            {
                result += el;
            }
        };

        Thread t1 = new Thread(() => action(subList1));
        Thread t2 = new Thread(() => action(subList2));
        Thread t3 = new Thread(() => action(subList3));
        Thread t4 = new Thread(() => action(subList4));
        
        t1.Start();
        t2.Start();
        t3.Start();
        t4.Start();
        
        t1.Join();
        t2.Join();
        t3.Join();
        t4.Join();
        sw.Stop();
        
        var time = sw.Elapsed;
        return (result, time);
    }
    
    private static (long result, TimeSpan time)  PLinqExecute(int size)
    {
        long result = 0;
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