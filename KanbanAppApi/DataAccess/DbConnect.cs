using KanbanAppApi.Models;
using MongoDB.Driver;

namespace KanbanAppApi.DataAccess
{
    public class DbConnect : IDbConnect
    {
        private readonly IConfiguration config;
        private readonly IMongoDatabase db;
        private string connectionId = "MongoDB";

        public string DbName { get; private set; }
        public string KanbanBoardCollectionName { get; private set; } = "kanbanboards";
        public string UsersCollectionName { get; private set; } = "users";
        public string TasksCollectionName { get; private set; } = "tasks";
        public MongoClient Client { get; private set; }

        public IMongoCollection<KanbanBoardModel> KanbanBoardCollection { get; set; }
        public IMongoCollection<UserModel> UsersCollection { get; set; }
        public IMongoCollection<TaskModel> TasksCollection { get; set; }

        public DbConnect(IConfiguration config)
        {
            this.config = config;
            var connectionString = config.GetConnectionString(connectionId);
            Client = new MongoClient(connectionString);
            DbName = config["DatabaseName"];
            this.db = Client.GetDatabase(DbName);

            KanbanBoardCollection = db.GetCollection<KanbanBoardModel>(KanbanBoardCollectionName);
            UsersCollection = db.GetCollection<UserModel>(UsersCollectionName);
            TasksCollection = db.GetCollection<TaskModel>(TasksCollectionName);
        }
    }
}
