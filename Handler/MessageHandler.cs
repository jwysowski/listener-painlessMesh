using MQTTnet;
using Listener.Models;

namespace MessageHandler
{
    public class Handler
    {
        private readonly ListenerContext _context;

        public Handler(ListenerContext context)
        {
            _context = context;
        }
        public async Task Handle(MqttApplicationMessageReceivedEventArgs msg)
        {
            var log = new Log();

            log.Message = System.Text.Encoding.UTF8.GetString(
                msg?.ApplicationMessage?.Payload ?? Array.Empty<byte>());

            Console.WriteLine($"Received msg: {log.Message}");

            if (log.Message[0] != ':')
                return;

            log.NodeId = log.Message.Substring(7, 10);
            log.Type = log.Message[1] == '0' ? "temperature" : "humidity";
            log.Value = Convert.ToDouble(log.Message.Substring(2, 5));
            log.Timestamp = System.DateTime.Now;

            _context.Log.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}