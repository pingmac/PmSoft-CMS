using System.Threading.Tasks;

namespace PmSoft.Tasks
{
    /// <summary>
    /// 用于注册任务的接口 -- 所有任务都需要实现此接口
    /// </summary>
    public interface ITask
    {
        /// <summary>
        /// 执行任务的方法 
        /// </summary>
        /// <param name="taskDetail"></param>
        Task ExecuteAsync(TaskDetail taskDetail = null);
    }
}

