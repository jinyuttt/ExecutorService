
using System.ComponentModel;

namespace ExecutorService
{
    public class TaskResult<T>
    {

        /// <summary>
        /// 返回结果
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        /// 执行结果
        /// </summary>
        public ErrorCode ResultCode { get; set; }

        /// <summary>
        /// 执行描述
        /// </summary>
        public string ErrorMsg { get { return ResultCode.FetchDescription(); } }

        /// <summary>
        /// 其它信息
        /// </summary>
        public string ResultMsg { get; set; }
    }

    /// <summary>
    /// 执行错误
    /// </summary>
    public enum ErrorCode
    {
        [Description("成功")]
        sucess,
        [Description("执行超时")]
        timeout,
        [Description("异常")]
        exception
    }
}
