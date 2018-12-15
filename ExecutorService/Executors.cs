#region << 版 本 注 释 >>
/*----------------------------------------------------------------
* 项目名称 ：ExecutorTask
* 项目描述 ：
* 类 名 称 ：Executors
* 类 描 述 ：
* 命名空间 ：ExecutorTask
* CLR 版本 ：4.0.30319.42000
* 作    者 ：jinyu
* 创建时间 ：2018
* 更新时间 ：2018
* 版 本 号 ：v1.0.0.0
*******************************************************************
* Copyright @ jinyu 2018. All rights reserved.
*******************************************************************
//----------------------------------------------------------------*/
#endregion


using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ExecutorService
{
    /* ============================================================================== 
    * 功能描述：Executors 超时执行；Execute方法执行不统计，submit方法内部记录超时的任务
    * 创 建 者：jinyu 
    * 修 改 者：jinyu 
    * 创建日期：2018 
    * 修改日期：2018 
    * ==============================================================================*/

    /// <summary>
    /// Execute方法直接执行
    /// submit方法内部记录超时的任务，会按照设置的最大线程数提示异常
    /// </summary>
    public class Executors
    {

       /// <summary>
       /// 当前设置的超时
       /// </summary>
        public  static int TimeOut = 1000;
        private static DateTime cur = DateTime.Now;//移除
        private static ConcurrentDictionary<int, DateTime> dicTimeOut = new ConcurrentDictionary<int, DateTime>();

        private static int maxThreadWaitNum = 10 * Environment.ProcessorCount;

        /// <summary>
        /// 最大线程数量
        /// 默认：CPU线程10倍
        /// </summary>
        public static int MaxThreadWaitNum { get { return maxThreadWaitNum; } set { maxThreadWaitNum = value; } }

        private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(MaxThreadWaitNum);
       
        #region 等待机制

        public static TaskResult<VoidNull> Execute(Action<object> action, object state, int timeOut = 0)
        {
            timeOut = timeOut > 0 ? timeOut : TimeOut;
            TaskResult<VoidNull> taskResult = new TaskResult<VoidNull>();
            try
            {
                Task result = Task.Factory.StartNew(action, state);
                if (result.Wait(timeOut))
                {
                    taskResult.ResultCode = ErrorCode.sucess;
                }
                else
                {
                    taskResult.ResultCode = ErrorCode.timeout;
                }
            }
            catch(Exception ex)
            {
                taskResult.ResultCode = ErrorCode.exception;
                taskResult.ResultMsg = ex.Message;
            }
            return taskResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="timeOut"></param>
        public static TaskResult<VoidNull> Execute(Action action,int timeOut = 0)
        {
            timeOut = timeOut > 0 ? timeOut : TimeOut;
            TaskResult<VoidNull> taskResult = new TaskResult<VoidNull>();
            try
            {
                Task result = Task.Factory.StartNew(action);
                if (result.Wait(timeOut))
                {
                    taskResult.ResultCode = ErrorCode.sucess;
                }
                else
                {
                    taskResult.ResultCode = ErrorCode.timeout;
                }
            }
            catch(Exception ex)
            {
                taskResult.ResultCode = ErrorCode.exception;
                taskResult.ResultMsg = ex.Message;
            }
            return taskResult;
        }

        public static TaskResult<TResult> Execute<TResult>(Func<object, TResult> function, object state, int timeOut = 0)
        {
            timeOut = timeOut > 0 ? timeOut : TimeOut;
            TaskResult<TResult> taskResult = new TaskResult<TResult>();
            try
            {
                Task<TResult> result = Task.Factory.StartNew(function, state);
                if (result.Wait(timeOut))
                {
                    taskResult.ResultCode = ErrorCode.sucess;
                    taskResult.Result = result.Result;
                }
                else
                {
                    taskResult.ResultCode = ErrorCode.timeout;
                }
            }catch(Exception ex)
            {
                taskResult.ResultCode = ErrorCode.exception;
                taskResult.ResultMsg = ex.Message;
            }
            return taskResult;
        }

        public static TaskResult<TResult> Execute<TResult>(Func<TResult> function, int timeOut = 0)
        {
            timeOut = timeOut > 0 ? timeOut : TimeOut;
            TaskResult<TResult> taskResult = new TaskResult<TResult>();
            try
            {
                Task<TResult> result = Task.Factory.StartNew(function);
                if (result.Wait(timeOut))
                {
                    taskResult.ResultCode = ErrorCode.sucess;
                    taskResult.Result = result.Result;
                }
                else
                {
                    taskResult.ResultCode = ErrorCode.timeout;
                }
            }catch(Exception ex)
            {
                taskResult.ResultCode = ErrorCode.exception;
                taskResult.ResultMsg = ex.Message;
            }
            return taskResult;
        }

        #endregion

        #region  阻塞机制

       /// <summary>
       /// 超时执行
       /// 超时默认0表示使用当前设置5000毫秒
       /// </summary>
       /// <param name="action">任务</param>
       /// <param name="state">参数</param>
       /// <param name="timeOut">超时时间（毫秒）</param>
       /// <returns></returns>
        public static TaskResult<VoidNull> Submit(Action<object> action, object state, int timeOut = 0)
        {
            timeOut = timeOut > 0 ? timeOut : TimeOut;
            TaskResult<VoidNull> taskResult = new TaskResult<VoidNull>() { ResultCode = ErrorCode.sucess };
            ManualResetEventSlim eventSlim = new ManualResetEventSlim(false);
          
            var task = Task.Factory.StartNew((slim) =>
              {
                  if(!semaphoreSlim.Wait(TimeOut))
                  {
                      ProcessError();
                  }
                  ManualResetEventSlim manual = slim as ManualResetEventSlim;
                  int id = -1;
                  try
                  {
                      CancellationTokenSource cancellation = new CancellationTokenSource(timeOut);
                      var cancell = cancellation.Token.Register(() =>
                      {
                          //经测试，这种方式比直接eventSlim.Wait(timeout)更加合理
                          //启动线程需要时间，执行时间这种方式计算更加合理
                          taskResult.ResultCode = ErrorCode.timeout;
                          manual.Set();
                          dicTimeOut[id] = DateTime.Now;
                      });
                      Task result = Task.Factory.StartNew(action, state,cancellation.Token);
                      id = result.Id;
                      result.Wait();
                      cancell.Dispose();
                  }
                  catch (Exception ex)
                  {
                      taskResult.ResultCode = ErrorCode.exception;
                      taskResult.ResultMsg = ex.Message;
                  }
                  finally
                  {
                      manual.Set();
                      semaphoreSlim.Release();
                      dicTimeOut.TryRemove(id, out cur);
                  }
              }, eventSlim);
            eventSlim.Wait();
            return taskResult;
        }

        public static TaskResult<VoidNull> Submit(Action action, int timeOut = 0)
        {
            timeOut = timeOut > 0 ? timeOut : TimeOut;
            TaskResult<VoidNull> taskResult = new TaskResult<VoidNull>() { ResultCode = ErrorCode.sucess };
            ManualResetEventSlim eventSlim = new ManualResetEventSlim(false);
            
            var task= Task.Factory.StartNew((slim) =>
            {
                if (!semaphoreSlim.Wait(TimeOut/2))
                {
                    ProcessError();
                }
                ManualResetEventSlim manual = slim as ManualResetEventSlim;
                int id = -1;
                try
                {
                    CancellationTokenSource cancellation = new CancellationTokenSource(timeOut);
                    var cancell = cancellation.Token.Register(() =>
                     {
                         //经测试，这种方式比直接eventSlim.Wait(timeout)更加合理
                         //启动线程需要时间，执行时间这种方式计算更加合理
                         taskResult.ResultCode = ErrorCode.timeout;
                         manual.Set();
                         dicTimeOut[id] = DateTime.Now;
                     });
                    Task result = Task.Factory.StartNew(action,cancellation.Token);
                    id = result.Id;
                    result.Wait();
                    cancell.Dispose();
                }
                catch (Exception ex)
                {
                    taskResult.ResultCode = ErrorCode.exception;
                    taskResult.ResultMsg = ex.Message;
                }
                finally
                {
                    manual.Set();
                    semaphoreSlim.Release();
                    dicTimeOut.TryRemove(id, out cur);
                }
            }, eventSlim);
            eventSlim.Wait();
            return taskResult;
        }

        public static TaskResult<TResult> Submit<TResult>(Func<object, TResult> function, object state, int timeOut = 0)
        {
            timeOut = timeOut > 0 ? timeOut : TimeOut;
            TaskResult<TResult> taskResult = new TaskResult<TResult>() { ResultCode = ErrorCode.sucess };
            ManualResetEventSlim eventSlim = new ManualResetEventSlim(false);
          
           var task= Task.Factory.StartNew((slim) =>
              {
                  if (!semaphoreSlim.Wait(TimeOut/4))
                  {

                      ProcessError();
                  }
                  ManualResetEventSlim manual = slim as ManualResetEventSlim;
                  int id = -1;
                  try
                  {
                      CancellationTokenSource cancellation = new CancellationTokenSource(timeOut);
                      var cancell = cancellation.Token.Register(() =>
                      {
                          //经测试，这种方式比直接eventSlim.Wait(timeout)更加合理
                          //启动线程需要时间，执行时间这种方式计算更加合理
                          taskResult.ResultCode = ErrorCode.timeout;
                          manual.Set();
                          dicTimeOut[id] = DateTime.Now;
                      });
                      Task<TResult> result = Task.Factory.StartNew(function, state,cancellation.Token);
                      id = result.Id;
                      taskResult.Result= result.Result;
                      cancell.Dispose();
                  }
                  catch (Exception ex)
                  {
                      taskResult.ResultCode = ErrorCode.exception;
                      taskResult.ResultMsg = ex.Message;
                  }
                  finally
                  {
                      manual.Set();
                      semaphoreSlim.Release();
                      dicTimeOut.TryRemove(id, out cur);
                  }
              },eventSlim);
            eventSlim.Wait();
            return taskResult;
             
        }

        public static TaskResult<TResult> Submit<TResult>(Func<TResult> function, int timeOut = 0)
        {
            timeOut = timeOut > 0 ? timeOut : TimeOut;
            TaskResult<TResult> taskResult = new TaskResult<TResult>() { ResultCode = ErrorCode.sucess };
            ManualResetEventSlim eventSlim = new ManualResetEventSlim(false);
          
            var task = Task.Factory.StartNew((slim) =>
              {
                  if (!semaphoreSlim.Wait(TimeOut))
                  {

                      ProcessError();
                  }
                  ManualResetEventSlim manual = slim as ManualResetEventSlim;
                  int id = -1;
                  try
                  {
                      CancellationTokenSource cancellation = new CancellationTokenSource(timeOut);
                      var cancell = cancellation.Token.Register(() =>
                      {
                          //经测试，这种方式比直接eventSlim.Wait(timeout)更加合理
                          //启动线程需要时间，执行时间这种方式计算更加合理
                          taskResult.ResultCode = ErrorCode.timeout;
                          manual.Set();
                          dicTimeOut[id] = DateTime.Now;
                      });
                      Task<TResult> result = Task.Factory.StartNew(function,cancellation.Token);
                      id = result.Id;
                      taskResult.Result = result.Result;
                      cancell.Dispose();
                  }
                  catch (Exception ex)
                  {
                      taskResult.ResultCode = ErrorCode.exception;
                      taskResult.ResultMsg = ex.Message;
                  }
                  finally
                  {
                      manual.Set();
                      semaphoreSlim.Release();
                      dicTimeOut.TryRemove(id, out cur);
                  }
              }, eventSlim);
            eventSlim.Wait();
            return taskResult;
        }
       

        /// <summary>
        /// 抛出错误
        /// </summary>
        public static  void ProcessError()
        {
          throw new ThreadMaxEception();
        }

        
      
        #endregion
    }
}
