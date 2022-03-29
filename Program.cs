using MQTTnet;
using MQTTnet.Server;
using MQTTnet.Client;
using MQTTnet.Client.Options;

var factory = new MqttFactory();

var serverOptions = new MqttServerOptionsBuilder()
    .WithDefaultEndpointPort(1883)
    .WithClientId("TestServer")
    .Build();

var clientOptions = new MqttClientOptionsBuilder()
    .WithClientId("TestClient")
    .WithTcpServer("localhost", 1883)
    .Build();

var server = factory.CreateMqttServer();
await server.StartAsync(serverOptions);

var client = factory.CreateMqttClient();

client.UseApplicationMessageReceivedHandler(msg =>
{
    var payloadText = System.Text.Encoding.UTF8.GetString(
        msg?.ApplicationMessage?.Payload ?? Array.Empty<byte>());

    Console.WriteLine($"Received msg: {payloadText}");
});

await client.ConnectAsync(clientOptions, CancellationToken.None);

await client.SubscribeAsync(
    new MqttTopicFilter
    {
        Topic = "gate/alive"
    },
    new MqttTopicFilter
    {
        Topic = "gate/temperature"
    },
    new MqttTopicFilter
    {
        Topic = "gate/humidity"
    }
);

Console.WriteLine("Press any key to stop the server.");
Console.ReadLine();

await server.StopAsync();
