using KanbanAppApi.DataAccess;
using KanbanAppApi.Models;
using MongoDB.Driver;

namespace KanbanAppApi.Services
{
    public class UsersService : IUsersService
    {
        private readonly IDbConnect _databaseConnection;
        public IMongoCollection<UserModel> UsersCollection { get; set; }
        public IMongoCollection<KanbanBoardModel> KanbanBoardsCollection { get; set; }

        public UsersService(IDbConnect databaseConnection)
        {
            _databaseConnection = databaseConnection;
            UsersCollection = _databaseConnection.UsersCollection;
            KanbanBoardsCollection = _databaseConnection.KanbanBoardCollection;
        }

        public async Task<List<UserModel>> GetUsersApartOfBoard(List<string> userIds)
        {
            var filter = Builders<UserModel>.Filter.In(x => x.Id, userIds);

            return await UsersCollection.Find(filter).ToListAsync();
        }

        public async Task AddUser(UserModel user)
        {
            await UsersCollection.InsertOneAsync(user);
        }

        public async Task<UserModel> GetUserByFirebaseId(string firebaseId)
        {
           var user = await UsersCollection.Find(u => u.FirebaseId == firebaseId).FirstOrDefaultAsync();

            return user;
        }

        public async Task<UserModel> GetUser(string userId)
        {
            var user = await UsersCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

            return user;
        }

        public async Task<UserModel> GetUserByEmail(string email)
        {
            return await UsersCollection.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task UpdateUser(UserModel user)
        {
            await UsersCollection.ReplaceOneAsync(u => u.Id == user.Id, user);
        }

        public async Task UpdateManyUsers(List<string> userIds, string boardId)
        {
            var filter = Builders<UserModel>.Filter.In(x => x.Id, userIds);

            var update = Builders<UserModel>.Update.PullFilter(u => u.BoardsApartOf,
                                                b => b.Id == boardId);

            await UsersCollection.UpdateManyAsync(filter, update);
        }

    }
}
