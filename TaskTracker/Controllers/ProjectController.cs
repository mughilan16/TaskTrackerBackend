using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace TaskTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectController : Controller
{
    public record ProjectRequest(string Name, string Tag)
    {
        public string Name { get; set; } = Name;
        public string Tag { get; set; } = Tag;
    }

    public record ProjectList(List<Project> Projects)
    {
        public List<Project> Projects { get; set; } = Projects;
    }
    [HttpGet()]
    public ProjectList Get()
    {
        var db = new ProjectDb();
        var projects = db.Get();
        return projects.Count == 0 ? new ProjectList([]) : new ProjectList(projects);
    }

    [HttpPost("new")]
    public IActionResult New(ProjectRequest request)
    {
        var db = new ProjectDb();
        var project = new Project(0, request.Name, request.Tag);
        return Ok(db.Create(project));
    }
}