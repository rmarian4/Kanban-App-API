namespace KanbanAppApi.Models
{
    public class UpdateTaskRequest
    {
        public TaskModel UpdatedTask { get; set; }
        public string BoardId{ get; set; }
    }
}
