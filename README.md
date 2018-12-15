# ExecutorService
超时执行控制

按照Task传递委托，设置超时时间，进行超时返回

示例：
```
 var resut = Executors.Submit(() =>
                    {
                        int tsp = random.Next(1000, 5000);
                        Thread.Sleep(tsp);
                      // Console.WriteLine(tsp/1000);
                    });
                    Console.WriteLine(resut.ErrorMsg);
```
