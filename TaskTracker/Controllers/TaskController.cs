using Microsoft.AspNetCore.Mvc;

namespace TaskTracker.Controllers
{
    [ApiController]
    [Route("api/task")]
    public class TaskController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public record GetResponse
        {
            public List<Task> Tasks { get; set; }
            public Task? CurrentTask { get; set; }
            public bool IsTaskActive { get; set; }
            public GetResponse(List<Task> tasks, Task? currentTask, bool isTaskActive)
            {
                Tasks = tasks;
                CurrentTask = currentTask;
                IsTaskActive = isTaskActive;
            }
        }
        [HttpGet(Name = "GetTasks")]
        public GetResponse Get()
        {
            var db = new TaskDB();
            var tasks = db.GetTasks();
            var isTaskActive = false;
            Task? currentTask = null;
            if (tasks.Last().StopTime == null)
            {
                currentTask = tasks.Last();
                isTaskActive = true;
                tasks.RemoveAt(tasks.Count-1);
            }
            return new GetResponse(tasks, currentTask, isTaskActive);
        }
   }
}
