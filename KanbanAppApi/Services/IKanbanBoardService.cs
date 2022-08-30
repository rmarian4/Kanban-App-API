using KanbanAppApi.Models;
using MongoDB.Driver;

namespace KanbanAppApi.Services
{
    public interface IKanbanBoardService
    {
        IMongoCollection<KanbanBoardModel> KanbanBoardCollection { get; set; }

        Task AddNewBoard(KanbanBoardModel kanbanboard, UserModel user);
        Task UpdateStatuses(KanbanBoardModel board);
        Task AddUserToBoard(KanbanBoardModel updatedBoard, UserModel updatedUser);
        Task<KanbanBoardModel> GetKanbanBoard(string id);
        Task<List<KanbanBoardModel>> GetKanbanBoards();
        Task RemoveUserFromBoard(KanbanBoardModel board, UserModel user);
        Task RemoveBoard(KanbanBoardModel board);
    }
}