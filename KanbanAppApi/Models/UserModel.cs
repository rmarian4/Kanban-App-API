using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbanAppApi.Models
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string FirebaseId { get; set; }
        public string Name { get; set; } 
        public string Email { get; set; } 
        public List<BasicKanbanBoardModel> BoardsApartOf { get; set; } //ids of boards user has been added to
        public List<BasicKanbanBoardModel> BoardsUserHasCreated { get; set; } //ids of boards the user has created
    }
}
