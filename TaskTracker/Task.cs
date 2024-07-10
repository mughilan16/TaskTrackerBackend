using Npgsql;
using System.Data;
using System.Numerics;
using Microsoft.AspNetCore.Http.HttpResults;

namespace TaskTracker
{
    public record Task(
        int Id,
        string Message,
        DateTime Date,
        DateTime StartTime,
        DateTime? StopTime,
        int? Total,
        int? ProjectId,
        string Tag)
    {
        public int Id { get; set; } = Id;
        public string Message { get; set; } = Message;
        public DateTime Date { get; set; } = Date;
        public DateTime StartTime { get; set; } = StartTime;
        public DateTime? StopTime { get; set; } = StopTime;
        public int? Total { get; set; } = Total;
        public string Tag { get; set; } = Tag;
        public int? ProjectId { get; set; } = ProjectId;

        public override string ToString()
        {
            return $"{Id} {Date:dd/MM/yyyy} {StartTime:hh:mm:ss)} {StopTime:hh:mm:ss} {Total} {Tag} {Message}";
        }
    }

    public class TaskDb
    {
        private readonly NpgsqlConnection _connection = new(ConnectionString);
        private const string ConnectionString = "Host=localhost; Port=5432; Database=task-tracker; User Id=root; Password=secret;";

        public Task Create(Task newTask)
        {
            if (this.IsTaskActive()) { throw new Exception("Already Task Active"); }
            _connection.Open();
            if (newTask.ProjectId == 0)
            {
                const string query = "INSERT INTO tasks_test(message, date, start_time, tag, project) VALUES(@message, @date, @start_time, @tag, @project) RETURNING id";
                var cmd = new NpgsqlCommand(query, _connection);
                cmd.Parameters.Add(new NpgsqlParameter("@message", SqlDbType.Text) { Value = newTask.Message });
                cmd.Parameters.Add(new NpgsqlParameter("@date", SqlDbType.DateTime) { Value = newTask.Date});
                cmd.Parameters.Add(new NpgsqlParameter("@start_time", SqlDbType.DateTime) { Value = newTask.StartTime });
                cmd.Parameters.Add(new NpgsqlParameter("@tag", SqlDbType.Text) { Value = newTask.Tag});
                cmd.Parameters.Add(new NpgsqlParameter("@project", SqlDbType.Int) { Value = newTask.ProjectId });
                var reader = cmd.ExecuteReader();
                if (reader.Read()) { newTask.Id = (int) reader["id"]; }
            }
            else
            {
                const string query = "INSERT INTO tasks_test(message, date, start_time, tag) VALUES(@message, @date, @start_time, @tag) RETURNING id";
                var cmd = new NpgsqlCommand(query, _connection);
                cmd.Parameters.Add(new NpgsqlParameter("@message", SqlDbType.Text) { Value = newTask.Message });
                cmd.Parameters.Add(new NpgsqlParameter("@date", SqlDbType.DateTime) { Value = newTask.Date});
                cmd.Parameters.Add(new NpgsqlParameter("@start_time", SqlDbType.DateTime) { Value = newTask.StartTime });
                cmd.Parameters.Add(new NpgsqlParameter("@tag", SqlDbType.Text) { Value = newTask.Tag});
                var reader = cmd.ExecuteReader();
                if (reader.Read()) { newTask.Id = (int) reader["id"]; }
            }
            return newTask;
        }

        
        public List<Task> GetTasks()
        {
            _connection.Open();
            const string query = "SELECT * FROM tasks_test ORDER BY start_time ASC";
            NpgsqlCommand cmd = new(query, _connection);

            var reader = cmd.ExecuteReader();
            List<Task> tasks = [];

            while (reader.Read())
            {
                DateTime? stopTime = null;
                int? total =  null;
                int? project = null;
                if (!DBNull.Value.Equals(reader["stop_time"]))
                {
                    stopTime = (DateTime)reader["stop_time"];
                }
                if (!DBNull.Value.Equals(reader["total"]))
                {
                    total = (int)reader["total"];
                }

                if (!DBNull.Value.Equals(reader["project"]))
                {
                    project = (int)reader["project"];
                }
                var newTask = new Task(
                  (int)reader["id"],
                  (string)reader["message"],
                  (DateTime)reader["date"],
                  (DateTime)reader["start_time"],
                  stopTime,
                  total,
                  project,
                  (string)reader["tag"]
                );
                tasks.Add(newTask);
            }
            _connection.Close();
            return tasks;
        }

        public Task CompleteTask()
        {
            if (!this.IsTaskActive()) { throw new Exception("No Task Active"); }
            const string query = "SELECT * FROM tasks_test WHERE stop_time IS NULL LIMIT 1";
            _connection.Open();
            using NpgsqlCommand cmd = new(query, _connection);
            var reader = cmd.ExecuteReader();
            if (!reader.Read()) throw new Exception("No Task Currently Running");
            var startTime = (DateTime) reader["start_time"];
            var message = (string)reader["message"];
            var id = (int)reader["id"];
            var tag = (string)reader["tag"];
            var projectId = (int)reader["project"];
            var date = (DateTime)reader["date"];
            _connection.Close();
            var stopTime = DateTime.Now;
            var total = (int) stopTime.Subtract(startTime).TotalMinutes;
            const string updateQuery = "UPDATE tasks_test SET stop_time = @stop_time, total = @total WHERE stop_time IS NULL";
            _connection.Open();
            var updateCmd = new NpgsqlCommand(updateQuery, _connection);
            updateCmd.Parameters.Add(new NpgsqlParameter("@stop_time", SqlDbType.DateTime) { Value = stopTime });
            updateCmd.Parameters.Add(new NpgsqlParameter("@total", SqlDbType.Int) { Value = total});
            updateCmd.ExecuteReader();
            _connection.Close();
            return new Task(id, message, date, startTime, stopTime, total, projectId, tag);
        }

        private bool IsTaskActive()
        {
            const string query = "SELECT * from tasks_test WHERE stop_time IS NULL LIMIT 1";
            _connection.Open();
            NpgsqlCommand  cmd = new(query, _connection);
            var reader = cmd.ExecuteReader();
            var isActive = reader.Read();
            _connection.Close();
            return isActive;
        }
        public List<Task> GetTaskForWorkMonth()
        {
            _connection.Open();
            const string query = "SELECT * FROM tasks_test";
            NpgsqlCommand cmd = new(query, _connection);

            var reader = cmd.ExecuteReader();
            List<Task> tasks = [];

            while (reader.Read())
            {
                DateTime? stopTime = null;
                int? total =  null;
                int? projectId = null;
                if (!DBNull.Value.Equals(reader["stop_time"]))
                {
                    stopTime = (DateTime)reader["stop_time"];
                }
                if (!DBNull.Value.Equals(reader["total"]))
                {
                    total = (int)reader["total"];
                }
                if (!DBNull.Value.Equals(reader["total"]))
                {
                    projectId = (int)reader["total"];
                }
                var newTask = new Task(
                  (int)reader["id"],
                  (string)reader["message"],
                  (DateTime)reader["date"],
                  (DateTime)reader["start_time"],
                  stopTime,
                  total,
                  projectId,
                  (string)reader["tag"]
                );
                tasks.Add(newTask);
            }
            _connection.Close();
            return tasks;
        }
    }
}
