using ExecutorService;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Random random = new Random(DateTime.Now.Millisecond);
                Parallel.For(0, 500, (i) =>
                {
                    var resut = Executors.Submit(() =>
                    {
                        int tsp = random.Next(1000, 5000);
                        Thread.Sleep(tsp);
                      // Console.WriteLine(tsp/1000);
                    });
                    Console.WriteLine(resut.ErrorMsg);
                });
              
            
           while(true)
            {
                Thread.Sleep(random.Next(5000));
            }
           // Console.Read();
        }
    }
}
