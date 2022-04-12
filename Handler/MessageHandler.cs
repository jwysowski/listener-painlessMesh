using MQTTnet;
using Listener.Models;
using MessageTypes;

namespace MessageHandler
{
    public class Handler
    {
        private readonly ListenerContext _context;
        private readonly MessageTypesLUT _typesLUT;
        private bool _isCommand;

        public Handler(ListenerContext context)
        {
            _context = context;
            _typesLUT = new MessageTypesLUT();
            _isCommand = false;
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
            log.Type = _typesLUT.GetMessageType(log.Message[1]);
            if (log.Type != "Temperature" && log.Type != "Humidity")
                _isCommand = true;

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
            if (!_isCommand)
            {
                if (_typesLUT.SetTemperature(log.Message[1]))
                    node.Temperature = log.Value;
                else
                    node.Humidity = log.Value;

                node.TargetTemperature = null;
                node.TargetHumidity = null;
            }
            else
            {
                if (_typesLUT.SetTemperature(log.Message[1]))
                    node.TargetTemperature = log.Value;
                else
                    node.TargetHumidity = log.Value;
            }

            node.LastUpdate = log.Timestamp;
            node.InManualMode = _typesLUT.GetMode(log.Message[1]);

            // node.TargetTemperature = node.InManualMode ? ;
            node.TargetHumidity = null;

            node.Arch = false;

            if (notInList)
                _context.Node.Add(node);

            await _context.SaveChangesAsync();
        }
    }
}