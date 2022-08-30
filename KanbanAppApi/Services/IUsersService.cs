using KanbanAppApi.Models;
using MongoDB.Driver;

namespace KanbanAppApi.Services
{
    public interface IUsersService
    {
        IMongoCollection<UserModel> UsersCollection { get; set; }

        Task AddUser(UserModel user);
        Task<UserModel> GetUserByFirebaseId(string firebaseId);
        Task<UserModel> GetUser(string userId);
        Task<UserModel> GetUserByEmail(string email);
        Task<List<UserModel>> GetUsersApartOfBoard(List<string> userIds);
        Task UpdateUser(UserModel user);
        Task UpdateManyUsers(List<string> userIds, string boardId);
    }
}