using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace KanbanAppApi.Models
{
    public class BasicUserModel
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public BasicUserModel()
        {

        }

        public BasicUserModel(UserModel user)
        {
            Id = user.Id;
            Name = user.Name;
            Email = user.Email;
        }
    }
}
