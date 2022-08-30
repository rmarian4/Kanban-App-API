using MongoDB.Driver;
using KanbanAppApi.Models;
using KanbanAppApi.DataAccess;

namespace KanbanAppApi.Services
{
    public class KanbanBoardService : IKanbanBoardService
    {
        private readonly IDbConnect _databaseConnection;
        public IMongoCollection<KanbanBoardModel> KanbanBoardCollection { get; set; }

        public KanbanBoardService(IDbConnect databaseConnection)
        {
            _databaseConnection = databaseConnection;
            KanbanBoardCollection = _databaseConnection.KanbanBoardCollection;
        }

        public async Task AddNewBoard(KanbanBoardModel kanbanboard, UserModel user)
        {
            var client = _databaseConnection.Client;
            using var session = await client.StartSessionAsync();

            session.StartTransaction();

            try
            {
                var db = client.GetDatabase(_databaseConnection.DbName);
                var boardsCollection = db.GetCollection<KanbanBoardModel>(_databaseConnection.KanbanBoardCollectionName);
                await boardsCollection.InsertOneAsync(kanbanboard);

                var usersCollection = db.GetCollection<UserModel>(_databaseConnection.UsersCollectionName);

                if (kanbanboard.Id is null)
                {
                    throw new Exception();
                }

                user.BoardsUserHasCreated.Add(new BasicKanbanBoardModel(kanbanboard));

                await usersCollection.ReplaceOneAsync(u => u.Id == user.Id, user);

                await session.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                throw;
            }

        }

        public async Task<List<KanbanBoardModel>> GetKanbanBoards()
        {
            return await KanbanBoardCollection.Find(_ => true).ToListAsync();
        }

        public async Task<KanbanBoardModel> GetKanbanBoard(string id)
        {
            var board = await KanbanBoardCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
            return board;
        }


        public async Task UpdateStatuses(KanbanBoardModel updatedBoard)
        {
            await KanbanBoardCollection.ReplaceOneAsync(b => b.Id == updatedBoard.Id, updatedBoard);

        }


        public async Task AddUserToBoard(KanbanBoardModel updatedBoard, UserModel updatedUser)
        {
            var client = _databaseConnection.Client;
            using var session = await client.StartSessionAsync();

            session.StartTransaction();

            try
            {
                var db = client.GetDatabase(_databaseConnection.DbName);

                var boardsCollection = db.GetCollection<KanbanBoardModel>(_databaseConnection.KanbanBoardCollectionName);
                await boardsCollection.ReplaceOneAsync(b => b.Id == updatedBoard.Id, updatedBoard);

                var usersCollection = db.GetCollection<UserModel>(_databaseConnection.UsersCollectionName);
                await usersCollection.ReplaceOneAsync(u => u.Id == updatedUser.Id, updatedUser);

                await session.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                throw;
            }

           
        }

        public async Task RemoveUserFromBoard(KanbanBoardModel board, UserModel user)
        {
            var client = _databaseConnection.Client;
            using var session = await client.StartSessionAsync();

            session.StartTransaction();

            try
            {
                var db = client.GetDatabase(_databaseConnection.DbName);
                var boardCollection = db.GetCollection<KanbanBoardModel>(_databaseConnection.KanbanBoardCollectionName);
                await boardCollection.ReplaceOneAsync(b => b.Id == board.Id, board);

                var usersCollection = db.GetCollection<UserModel>(_databaseConnection.UsersCollectionName);
                await usersCollection.ReplaceOneAsync(u => u.Id == user.Id, user);

                await session.CommitTransactionAsync();
            }
            catch(Exception ex)
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }

        public async Task RemoveBoard(KanbanBoardModel board)
        {
            await KanbanBoardCollection.DeleteOneAsync(b => b.Id == board.Id);
        }


    }
}
