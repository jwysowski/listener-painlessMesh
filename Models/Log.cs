using System.ComponentModel.DataAnnotations;

namespace Listener.Models
{
    public class Log
    {
        public int Id { get; set; }
        public string NodeId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public double Value { get; set; }
        [DataType(DataType.Date)]
        public DateTime Timestamp { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}