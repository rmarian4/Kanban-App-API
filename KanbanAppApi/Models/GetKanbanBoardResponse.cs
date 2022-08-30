using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbanAppApi.Models
{
    public class GetKanbanBoardResponse
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string Title { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public BasicUserModel Owner { get; set; } //user id of person who created the board
        public List<BasicUserModel> UsersAddedToBoard { get; set; } //id's of users who have been added to the board
        public List<string> Statuses { get; set; }
        public List<TaskModel> Tasks { get; set; } //list of Id's corresponding to each task
    }
}
