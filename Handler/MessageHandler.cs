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
            await SaveLog(msg);
        }

        private async Task SaveLog(MqttApplicationMessageReceivedEventArgs msg)
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
            await SaveNode(log);
        }

        private async Task SaveNode(Log log)
        {
            bool notInList = false;
            var node = _context.Node.FirstOrDefault(n => n.NodeId == log.NodeId);
            if (node == null)
            {
                node = new Node();
                notInList = true;
            }

            node.NodeId = log.NodeId;
            if (log.Message[1] == '0')
                node.Temperature = log.Value;
            else
                node.Humidity = log.Value;

            node.LastUpdate = log.Timestamp;
            node.Arch = false;

            if (notInList)
                _context.Node.Add(node);

            await _context.SaveChangesAsync();
        }
    }
}