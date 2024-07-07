using System.Data;
using Npgsql;

namespace TaskTracker;

public record Project(int Id, string Name, string Tag)
{
    public int Id { get; set; } = Id;
    public string Name { get; set; }= Name;
    public string Tag { get; set; }= Tag;
}

public class ProjectDb
{
    private readonly NpgsqlConnection _connection = new(ConnectionString);

    private const string ConnectionString =
        "Host=localhost; Port=5432; Database=task-tracker; User Id=root; Password=secret;";

    public Project Create(Project newProject)
    {
        _connection.Open();
        const string query = "INSERT INTO projects(name, tag) VALUES(@name, @tag) RETURNING id";
        var cmd = new NpgsqlCommand(query, _connection);
        cmd.Parameters.Add(new NpgsqlParameter("@name", SqlDbType.Text) { Value = newProject.Name });
        cmd.Parameters.Add(new NpgsqlParameter("@tag", SqlDbType.Int) { Value = newProject.Tag });
        var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            newProject.Id = (int)reader["id"];
        }

        _connection.Close();
        return newProject;
    }

    public List<Project> Get()
    {
        _connection.Open();
        const string query = "SELECT id, name, tag FROM projects";
        var cmd = new NpgsqlCommand(query, _connection);
        var reader = cmd.ExecuteReader();
        List<Project> projects = [];
        while (reader.Read())
        {
            var newProject = new Project((int)reader["id"], (string)reader["name"], (string)reader["tag"] );
            projects.Add(newProject);
        }

        _connection.Close();
        return projects;
    }
}