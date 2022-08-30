using KanbanAppApi.Models;
using MongoDB.Driver;

namespace KanbanAppApi.Services
{
    public interface ITasksService
    {
        IMongoCollection<TaskModel> TasksCollection { get; set; }

        Task AddNewTask(KanbanBoardModel board, TaskModel task);
        Task<TaskModel> GetTask(string taskId);
        Task<List<TaskModel>> GetTasks(List<string> taskIds);
        Task RemoveTask(TaskModel task, KanbanBoardModel board);
        Task UpdateTask(TaskModel updatedTask);
        Task RemoveManyTasks(List<string> taskIds);
    }
}