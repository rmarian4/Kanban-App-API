namespace KanbanAppApi.Models
{
    public class CreateUserRequest
    {
        public string name { get; set; }
        public string email { get; set; }
        public string firebaseId {  get; set; }
    }
}
