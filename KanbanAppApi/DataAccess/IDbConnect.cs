using KanbanAppApi.Models;
using MongoDB.Driver;

namespace KanbanAppApi.DataAccess
{
    public interface IDbConnect
    {
        MongoClient Client { get; }
        string DbName { get; }
        IMongoCollection<KanbanBoardModel> KanbanBoardCollection { get; set; }
        string KanbanBoardCollectionName { get; }
        IMongoCollection<UserModel> UsersCollection { get; set; }
        string UsersCollectionName { get; }
        IMongoCollection<TaskModel> TasksCollection { get; set; }   
        string TasksCollectionName { get; }
    }
}