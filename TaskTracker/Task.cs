using Npgsql;
using System.Numerics;
namespace TaskTracker
{
    public record Task
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan? StopTime { get; set; }
        public int? Total { get; set; }
        public string Tag { get; set; }
        public Task(
          int id,
          string message,
          DateTime date,
          TimeSpan start_time,
          TimeSpan? stop_time,
          int? total,
          string tag
        )
        {
            Id = id;
            Message = message;
            Date = date;
            StartTime = start_time;
            StopTime = stop_time;
            Total = total;
            Tag = tag;
        }
        override public string ToString()
        {
            return $"{Id} {Date.ToString("dd/MM/yyyy")} {StartTime.ToString()} {StopTime.ToString()} {Total} {Tag} {Message}";
        }
    }

    public class TaskDB
    {

        private NpgsqlConnection _connection;
        private readonly string CONNECTION_STRING =
          "Host=localhost; Port=5432; Database=task-tracker; User Id=root; Password=secret;";

        public TaskDB()
        {
            _connection = new NpgsqlConnection(CONNECTION_STRING);
        }

        public List<Task> GetTasks()
        {
            _connection.Open();
            var query = "SELECT * FROM tasks";
            using NpgsqlCommand cmd = new NpgsqlCommand(query, _connection);

            using NpgsqlDataReader reader = cmd.ExecuteReader();
            List<Task> tasks = new List<Task>();

            while (reader.Read())
            {
                TimeSpan? stopTime = null;
                int? total =  null;
                if (!DBNull.Value.Equals(reader["stop_time"]))
                {
                    stopTime = (TimeSpan)reader["stop_time"];
                }
                if (!DBNull.Value.Equals(reader["total"]))
                {
                    total = (int)reader["total"];
                }
                var newTask = new Task(
                  (int)reader["id"],
                  (string)reader["message"],
                  (DateTime)reader["date"],
                  (TimeSpan)reader["start_time"],
                  stopTime,
                  total,
                  (string)reader["tag"]
                );
                tasks.Add(newTask);
            }
            _connection.Close();
            return tasks;
        }
    }
}
