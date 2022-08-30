using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbanAppApi.Models
{
    public class CreateTaskRequest
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string BoardId { get; set; }
        public string? UserAssignedToTask { get; set; }

        public string Title { get; set; }
        
        public string Description { get; set; }
        
        public List<string> SubTasks { get; set; }
        
        public string Status { get; set; }
    }
}
