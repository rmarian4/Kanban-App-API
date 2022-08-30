using KanbanAppApi.DataAccess;
using MongoDB.Driver;
using KanbanAppApi.Models;

namespace KanbanAppApi.Services
{
    public class TasksService : ITasksService
    {
        private readonly IDbConnect _dbConnection;
        public IMongoCollection<TaskModel> TasksCollection { get; set; }

        public TasksService(IDbConnect dbConnection)
        {
            _dbConnection = dbConnection;
            TasksCollection = _dbConnection.TasksCollection;
        }

        public async Task<TaskModel> GetTask(string taskId)
        {
            var task = await TasksCollection.Find(t => t.Id == taskId).FirstOrDefaultAsync();
            
            return task;
        }

        public async Task<List<TaskModel>> GetTasks(List<string> taskIds)
        {
            var filter = Builders<TaskModel>.Filter.In(x => x.Id, taskIds);

            return await TasksCollection.Find(filter).ToListAsync();
        }

        public async Task AddNewTask(KanbanBoardModel board, TaskModel task)
        {
            var client = _dbConnection.Client;
            using var session = await client.StartSessionAsync();

            session.StartTransaction();

            try
            {
                var db = client.GetDatabase(_dbConnection.DbName);
                var taskCollection = db.GetCollection<TaskModel>(_dbConnection.TasksCollectionName);

                await taskCollection.InsertOneAsync(task);

                var boardsCollection = db.GetCollection<KanbanBoardModel>(_dbConnection.KanbanBoardCollectionName);

                if (task.Id is null)
                {
                    throw new Exception();
                }

                board.Tasks.Add(task.Id);

                await boardsCollection.ReplaceOneAsync(b => b.Id == board.Id, board);

                await session.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }

        public async Task UpdateTask(TaskModel updatedTask)
        {
            await TasksCollection.ReplaceOneAsync(t => t.Id == updatedTask.Id, updatedTask);
        }

        public async Task RemoveTask(TaskModel task, KanbanBoardModel board)
        {
            var client = _dbConnection.Client;
            using var session = await client.StartSessionAsync();

            session.StartTransaction();

            try
            {
                var db = client.GetDatabase(_dbConnection.DbName);
                var tasksCollection = db.GetCollection<TaskModel>(_dbConnection.TasksCollectionName);

                await tasksCollection.DeleteOneAsync(t => t.Id == task.Id);

                var boardsCollection = db.GetCollection<KanbanBoardModel>(_dbConnection.KanbanBoardCollectionName);
                List<string> newTaskList = board.Tasks.FindAll(id => id != task.Id);

                board.Tasks = newTaskList;

                await boardsCollection.ReplaceOneAsync(b => b.Id == board.Id, board);

                await session.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }


        public async Task RemoveManyTasks(List<string> taskIds)
        {
            var filter = Builders<TaskModel>.Filter.In(x => x.Id, taskIds);

            await TasksCollection.DeleteManyAsync(filter);

        }
    }
}
