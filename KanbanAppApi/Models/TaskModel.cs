using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbanAppApi.Models
{
    public class TaskModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        //[BsonRepresentation(BsonType.ObjectId)]
        //public string BoardId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public List<SubTaskModel> SubTasks { get; set; }
        public string Status{ get; set; }
        public BasicUserModel? PersonAssignedToTask { get; set; } //id of user assigned to task
    }
}
