using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using KanbanAppApi.DataAccess;
using KanbanAppApi.Models;
using MongoDB.Driver;
using KanbanAppApi.Services;
using FirebaseAdmin.Auth;

namespace KanbanAppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KanbanAppController : ControllerBase
    {
        private readonly IUsersService _usersService;
        private readonly IKanbanBoardService _kanbanBoardService;
        private readonly ITasksService _tasksService;

        public KanbanAppController(IUsersService usersService, IKanbanBoardService kanbanBoardService, ITasksService tasksService)
        {
            _usersService = usersService;
            _kanbanBoardService = kanbanBoardService;
            _tasksService = tasksService;
        }

        private async Task<FirebaseToken> verifyAccessToken(string authorization)
        {
            if (authorization == null || !authorization.StartsWith("Bearer")) {
                throw new ArgumentException("Invalid Authorization");
            }

            string accessToken = authorization.Substring("Bearer ".Length).Trim();
            FirebaseToken decodedToken;

            try
            {
                decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(accessToken);
            } 
            catch (Exception e)
            {
                throw new ArgumentException("Invalid Authorization", e);
            }
            
            return decodedToken;
        }

        [HttpPost("users")]
        public async Task<ActionResult> AddUser([FromBody] CreateUserRequest createUserRequest)
        {
            UserModel newUser = new()
            {
                FirebaseId = createUserRequest.firebaseId,
                Name = createUserRequest.name,
                Email = createUserRequest.email,
                BoardsApartOf = new List<BasicKanbanBoardModel>(),
                BoardsUserHasCreated = new List<BasicKanbanBoardModel>()
            };

            await _usersService.AddUser(newUser);

            return CreatedAtAction(nameof(GetUser), new { userId = newUser.Id }, newUser);
        }

        [HttpGet("users/{firebaseId}")]
        public async Task<ActionResult<UserModel>> LogUserIn(string firebaseId)
        {
            //when user logs in  only firebaseId will be available, so fetch the user based on firebaseId
            var user = await _usersService.GetUserByFirebaseId(firebaseId);

            if(user == null)
            {
                return NotFound($"user with firebase id of {firebaseId} not found");
            }

            return Ok(user);
        }

        [HttpGet("users/{userId:length(24)}")]
        public async Task<ActionResult<UserModel>> GetUser([FromHeader] string authorization,string userId)
        {

            try
            {
                await verifyAccessToken(authorization);
            }
            catch (ArgumentException e)
            {
                Console.Write(e.Message);
                return Unauthorized();
            }

            var user = await _usersService.GetUser(userId);

            if(user == null)
            {
                return NotFound($"user with id {userId} not found");
            }

            return Ok(user);
        }


        [HttpGet("boards")]
        public async Task<ActionResult<List<KanbanBoardModel>>> GetKanbanBoards([FromHeader] string authorization)
        {
            try
            {
                await verifyAccessToken(authorization);
            }
            catch (ArgumentException e)
            {
                Console.Write(e.Message);
                return Unauthorized();
            }

            var result = await _kanbanBoardService.GetKanbanBoards();

            return Ok(result);
        }

        [HttpGet("boards/{id:length(24)}")]
        public async Task<ActionResult<GetKanbanBoardResponse>> GetKanbanBoard([FromHeader] string authorization, string id)
        {
            FirebaseToken firebaseToken;
            try
            {
                firebaseToken = await verifyAccessToken(authorization);
            }
            catch (ArgumentException e)
            {
                Console.Write(e.Message);
                return Unauthorized();
            }

            var board = await _kanbanBoardService.GetKanbanBoard(id);
            var user = await _usersService.GetUserByFirebaseId(firebaseToken.Uid);

            if(board is null)
            {
                return NotFound($"Kanban board with id {id} was not found");
            }

            if(!board.Owner.Equals(user.Id) && !board.UsersAddedToBoard.Contains(user.Id))
            {
                return StatusCode(403);
            }

            var tasks = await _tasksService.GetTasks(board.Tasks);
            var users = await _usersService.GetUsersApartOfBoard(board.UsersAddedToBoard);
            var owner = await _usersService.GetUser(board.Owner);

            var basicUsers = users.Select(u => new BasicUserModel(u)).ToList(); //convert list of users to BasicUserModel
            var basicOwner = new BasicUserModel(owner); //convert owner to basicUserModel

            GetKanbanBoardResponse response = new()
            {
                Id = board.Id,
                Title = board.Title,
                Owner = basicOwner,
                UsersAddedToBoard = basicUsers,
                Statuses = board.Statuses,
                Tasks = tasks,
            };

            return Ok(response);
        }

        [HttpPost("boards")]
        public async Task<ActionResult> AddNewKanbanBoard([FromHeader] string authorization, [FromBody] CreateKanbanBoardRequest request)
        {
            FirebaseToken firebaseToken;
            try
            {
                firebaseToken = await verifyAccessToken(authorization);
            }
            catch (ArgumentException e)
            {
                Console.Write(e.Message);
                return Unauthorized();
            }

            var user = await _usersService.GetUserByFirebaseId(firebaseToken.Uid);

            if(user is null)
            {
                return NotFound($"User with id {firebaseToken.Uid} was not found");
            }

            KanbanBoardModel newBoard = new()
            {
                Title = request.Title,
                Owner = user.Id,
                UsersAddedToBoard = new List<string>(),
                Statuses = new List<string>(),
                Tasks = new List<string>()
            };

            await _kanbanBoardService.AddNewBoard(newBoard, user);

            return CreatedAtAction(nameof(GetKanbanBoard), new {id = newBoard.Id}, newBoard);
        }

        [HttpPut("boards/{boardId:length(24)}/status")]
        public async Task<ActionResult> AddNewStatus([FromHeader] string authorization, string boardId, [FromBody] StatusRequest request)
        {
            FirebaseToken firebaseToken;
            try
            {
                firebaseToken = await verifyAccessToken(authorization);
            }
            catch (ArgumentException e)
            {
                Console.Write(e.Message);
                return Unauthorized();
            }

            var board = await _kanbanBoardService.GetKanbanBoard(boardId);
            var user = await _usersService.GetUserByFirebaseId(firebaseToken.Uid);

            if(board is null)
            {
                return NotFound($"Board with id {boardId} was not found");
            }

            if(!board.Owner.Equals(user.Id) && !board.UsersAddedToBoard.Contains(user.Id))
            {
                return StatusCode(403);
            }

            if(board.Statuses.Contains(request.Status))
            {
                return BadRequest($"The board already contains the {request.Status} status");
            }

            board.Statuses.Add(request.Status);
            await _kanbanBoardService.UpdateStatuses(board);

            return NoContent();
        }

        [HttpDelete("boards/{boardId:length(24)}/status")]
        public async Task<ActionResult> RemoveStatus([FromHeader] string authorization,string boardId, [FromBody] StatusRequest request)
        {
            FirebaseToken firebaseToken;
            try
            {
                firebaseToken = await verifyAccessToken(authorization);
            }
            catch (ArgumentException e)
            {
                Console.Write(e.Message);
                return Unauthorized();
            }

            var board = await _kanbanBoardService.GetKanbanBoard(boardId);
            var user = await _usersService.GetUserByFirebaseId(firebaseToken.Uid);

            if(board is null)
            {
                return NotFound($"Board with Id {boardId} not found");
            }

            if (!board.Owner.Equals(user.Id) && !board.UsersAddedToBoard.Contains(user.Id))
            {
                return StatusCode(403);
            }

            foreach(var id in board.Tasks)
            {
                var task = await _tasksService.GetTask(id);

                if(task.Status.Equals(request.Status))
                {
                    return BadRequest($"There are still tasks that have a the {request.Status} status");
                }
            }

            board.Statuses.Remove(request.Status);

            await _kanbanBoardService.UpdateStatuses(board);

            return NoContent();
        }

        [HttpPut("boards/{boardId:length(24)}/users")]
        public async Task<ActionResult> AddUserToBoard([FromHeader] string authorization, string boardId, [FromBody] AddUserToBoardRequest request)
        {
            FirebaseToken firebaseToken;
            try
            {
                firebaseToken = await verifyAccessToken(authorization);
            }
            catch (ArgumentException e)
            {
                Console.Write(e.Message);
                return Unauthorized();
            }

            var user = await _usersService.GetUserByFirebaseId(firebaseToken.Uid);
            var boardToAddUserTo = await _kanbanBoardService.GetKanbanBoard(boardId);
            var userToAddBoardTo = await _usersService.GetUserByEmail(request.UserEmail);

            if(boardToAddUserTo is null || userToAddBoardTo is null) 
            {
                return NotFound($"Kanban board with id {boardId} or user with id {request.UserEmail} was not found");
            }

            if(!boardToAddUserTo.Owner.Equals(user.Id))
            {
                return StatusCode(403);
            }

            boardToAddUserTo.UsersAddedToBoard.Add(userToAddBoardTo.Id);
            userToAddBoardTo.BoardsApartOf.Add(new BasicKanbanBoardModel(boardToAddUserTo));

            await _kanbanBoardService.AddUserToBoard(boardToAddUserTo, userToAddBoardTo);

            return NoContent();
        }

        [HttpDelete("boards/{boardId:length(24)}/users")]
        public async Task<ActionResult> RemoveUsersFromBoard([FromHeader] string authorization, string boardId, [FromBody] RemoveUserFromBoardRequest request)
        {
            FirebaseToken firebaseToken;
            try
            {
                firebaseToken = await verifyAccessToken(authorization);
            }
            catch (ArgumentException e)
            {
                Console.Write(e.Message);
                return Unauthorized();
            }

            var board = await _kanbanBoardService.GetKanbanBoard(boardId);
            var owner = await _usersService.GetUserByFirebaseId(firebaseToken.Uid);

            if(board is null)
            {
                return NotFound($"board with id {boardId} not found");
            }

            if(!board.Owner.Equals(owner.Id))
            {
                return StatusCode(403);
            }

            foreach(var id in request.UserIds)
            {
                var user = await _usersService.GetUser(id);
                board.UsersAddedToBoard = board.UsersAddedToBoard.FindAll(x => x != id);
                user.BoardsApartOf = user.BoardsApartOf.FindAll(x => x.Id != boardId);

                await _kanbanBoardService.RemoveUserFromBoard(board, user);
            }

            return NoContent();
        }

        [HttpDelete("boards/{boardId:length(24)}")]
        public async Task<ActionResult> RemoveBoard([FromHeader] string authorization, string boardId)
        {
            FirebaseToken firebaseToken;

            try
            {
                firebaseToken = await verifyAccessToken(authorization);
            }
            catch (ArgumentException e)
            {
                Console.Write(e.Message);
                return Unauthorized();
            }

            var board = await _kanbanBoardService.GetKanbanBoard(boardId);
            var user = await _usersService.GetUserByFirebaseId(firebaseToken.Uid);

            if(board is null)
            {
                return NotFound($"Board with id {boardId} not found");
            }

            if(!board.Owner.Equals(user.Id))
            {
                return StatusCode(403);
            }

            var owner = await _usersService.GetUser(board.Owner);

            owner.BoardsUserHasCreated = owner.BoardsUserHasCreated.FindAll(x => x.Id != boardId);

            await _usersService.UpdateUser(owner);

            await _tasksService.RemoveManyTasks(board.Tasks);

            await _usersService.UpdateManyUsers(board.UsersAddedToBoard, boardId);

            await _kanbanBoardService.RemoveBoard(board);

            return NoContent();
        }

        [HttpGet("tasks/{taskId:length(24)}")]
        public async Task<ActionResult<TaskModel>> GetTask(string taskId)
        {
            var task = await _tasksService.GetTask(taskId);

            if(task is null)
            {
                return NotFound($"Task with id {taskId} not found");
            }

            return Ok(task);
        }

        [HttpPost("tasks")]
        public async Task<ActionResult> AddNewTask([FromHeader] string authorization, [FromBody] CreateTaskRequest request)
        {
            FirebaseToken firebaseToken;

            try
            {
               firebaseToken = await verifyAccessToken(authorization);
            }
            catch (ArgumentException e)
            {
                Console.Write(e.Message);
                return Unauthorized();
            }

            var board = await _kanbanBoardService.GetKanbanBoard(request.BoardId);
            var userMakingRequest = await _usersService.GetUserByFirebaseId(firebaseToken.Uid);

            if(board is null)
            {
                return NotFound($"Kanban board with id {request.BoardId} not found");
            }

            if (!board.Owner.Equals(userMakingRequest.Id) && !board.UsersAddedToBoard.Contains(userMakingRequest.Id))
            {
                return StatusCode(403);
            }

            TaskModel newTask = new()
            {
                Title = request.Title,
                Description = request.Description,
                Status = request.Status,
                SubTasks = new List<SubTaskModel>()
            };

            if(!board.Statuses.Contains(request.Status))
            {
                return BadRequest($"Board does not contain {request.Status} status");
            }

            if (request.UserAssignedToTask is not null)
            {
                var user = await _usersService.GetUser(request.UserAssignedToTask);

                if (user is null)
                {
                    return NotFound($"User with id {request.UserAssignedToTask} not found");
                }

                //check if the user assigned to the task is still apart of the board
                if (!board.UsersAddedToBoard.Contains(user.Id) && !board.Owner.Equals(user.Id))
                {
                    return BadRequest("User not apart of board");
                }

                newTask.PersonAssignedToTask = new BasicUserModel(user);
            }

            foreach(var subTask in request.SubTasks)
            {
                SubTaskModel newSubTask = new()
                {
                    IsCompleted = false,
                    Description = subTask
                };

                newTask.SubTasks.Add(newSubTask);
            }

            await _tasksService.AddNewTask(board, newTask);

            return CreatedAtAction(nameof(GetTask), new {taskId = newTask.Id}, newTask);
        }

        [HttpPut("tasks")]
        public async Task<ActionResult> UpdateTask([FromHeader] string authorization, [FromBody] UpdateTaskRequest request)
        {
            FirebaseToken firebaseToken;

            try
            {
                firebaseToken = await verifyAccessToken(authorization);
            }
            catch (ArgumentException e)
            {
                Console.Write(e.Message);
                return Unauthorized();
            }

            var user = await _usersService.GetUserByFirebaseId(firebaseToken.Uid);
            var task = await _tasksService.GetTask(request.UpdatedTask.Id); //check if task has not been deleted

            if(task is null)
            {
                return BadRequest("The task has been deleted");
            }

            var board = await _kanbanBoardService.GetKanbanBoard(request.BoardId);

            if (!board.Owner.Equals(user.Id) && !board.UsersAddedToBoard.Contains(user.Id))
            {
                return StatusCode(403);
            }

            //check if the user assigned to task is still apart of the board
            if(request.UpdatedTask.PersonAssignedToTask is not null 
                && !board.UsersAddedToBoard.Contains(request.UpdatedTask.PersonAssignedToTask.Id) 
                && !board.Owner.Equals(request.UpdatedTask.PersonAssignedToTask.Id))
            {
                return BadRequest("User not apart of board");
            }

            await _tasksService.UpdateTask(request.UpdatedTask);
            return NoContent();
        }

       

        [HttpDelete("boards/{boardId:length(24)}/tasks/{taskId:length(24)}")]
        public async Task<ActionResult> DeleteTask([FromHeader] string authorization, string taskId, string boardId)
        {
            FirebaseToken firebaseToken;

            try
            {
                firebaseToken = await verifyAccessToken(authorization);
            }
            catch (ArgumentException e)
            {
                Console.Write(e.Message);
                return Unauthorized();
            }

            var user = await _usersService.GetUserByFirebaseId(firebaseToken.Uid);
            var task = await _tasksService.GetTask(taskId);
            var board = await _kanbanBoardService.GetKanbanBoard(boardId);

            if(task is null || board is null)
            {
                return NotFound($"Task with id {taskId} or board with id {boardId} not found");
            }

            if (!board.Owner.Equals(user.Id) && !board.UsersAddedToBoard.Contains(user.Id))
            {
                return StatusCode(403);
            }

            await _tasksService.RemoveTask(task, board);

            return NoContent();
        }
    }
}
