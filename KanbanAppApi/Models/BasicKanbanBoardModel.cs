using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbanAppApi.Models
{
    public class BasicKanbanBoardModel
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Title { get; set; }

        public BasicKanbanBoardModel()
        {

        }

        public BasicKanbanBoardModel(KanbanBoardModel kanbanBoard)
        {
            Id = kanbanBoard.Id;
            Title = kanbanBoard.Title;
        }
    }
}
