using System;
using System.Threading.Tasks;
using PetaPoco;
using PmSoft.Repositories;

namespace PmSoft.Tasks
{

    public class TaskDetailRepository : Repository<TaskDetail>, ITaskDetailRepository
    {
        /// <summary>
        /// 保存任务状态 
        /// </summary>
        /// <param name="taskDetail"></param>
        public async Task SaveTaskStatusAsync(TaskDetail taskDetail)
        {
            Sql builder = Sql.Builder;
            builder.Append("update Task_Details set LastStart = @0, LastEnd = @1,NextStart = @2,IsRunning = @3 where Id = @4", new object[] { taskDetail.LastStart, taskDetail.LastEnd, taskDetail.NextStart, taskDetail.IsRunning, taskDetail.Id });
            await CreateDAO().ExecuteAsync(builder);
        }
    }
}

