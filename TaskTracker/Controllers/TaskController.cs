using Microsoft.AspNetCore.Mvc;

namespace TaskTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : Controller
    {
        public record GetResponse(List<Task> Tasks, Task? CurrentTask)
        {
            public List<Task> Tasks { get; set; } = Tasks;
            public Task? CurrentTask { get; set; } = CurrentTask;
        }

        public record PostRequest(string Message, string Tag,int Project)
        {
            public string Message { get; set; } = Message;
            public string Tag { get; set; } = Tag;
            public int Project { get; set; } = Project;
        }

        [HttpGet()]
        public GetResponse Get()
        {
            var db = new TaskDb();
            var tasks = db.GetTasks();
            if (tasks.Count == 0) return new GetResponse([], null);
            if (tasks.Last().StopTime != null) return new GetResponse(tasks, null);
            var currentTask = tasks.Last();
            tasks.RemoveAt(tasks.Count-1);
            return tasks.Count == 0 ? new GetResponse([], currentTask) : new GetResponse(tasks, currentTask);
        }

        [HttpPost("create")]
        public IActionResult Create(PostRequest request)
        {
            var db = new TaskDb();
            var newTask = new Task(0, Message: request.Message, Tag: request.Tag, Date: DateTime.Now.Date, StartTime: DateTime.Now, StopTime:null, Total:0, ProjectId: request.Project);
            return Ok(db.Create(newTask));
        }

        [HttpPatch("complete")]
        public IActionResult Patch()
        {
            var db = new TaskDb();
            return Ok(db.CompleteTask());
        }
   }
}
