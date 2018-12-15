using System;

namespace ExecutorService
{

    /// <summary>
    /// 线程过多异常
    /// </summary>
    public class ThreadMaxEception:Exception 
    {
        public ThreadMaxEception(string message)
            : base(message)
        {

        }
        public ThreadMaxEception():base("线程超过运行的最大值")
        {
           
        }
    }
}
