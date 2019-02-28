﻿using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace Mqtt.CommonLib
{
    public class MqttDoubleChannelClientAsync
    {
        private readonly IMqttClient _client = new MqttFactory().CreateMqttClient();
        private readonly List<string> _messages = new List<string>();

        public MqttDoubleChannelClientAsync()
        {
            _client.ApplicationMessageReceived += (s, e) =>
            {
                _messages.Add(Encoding.UTF8.GetString(e.ApplicationMessage.Payload) + " | " + e.ApplicationMessage.Topic);
            };
            
            //TODO maybe put automatic sub to an opposing channel for receiving while publishing on another channel (2channels total)
            //_client.Connected += async (s, e) =>
            //{
            //    await _client.SubscribeAsync(new TopicFilterBuilder().WithTopic("t").Build());
            //};
        }

        private async Task<MqttClientConnectResult> Connect(string ip, int port)
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(ip, port)
                .Build();

            return await _client.ConnectAsync(options);
        }

        private async void Subscribe(string topic)
        {
            await _client.SubscribeAsync(new TopicFilterBuilder().WithTopic(topic).Build());
        }

        private async void Publish(string topic, string message)
        {
            var mqttApplicationMessageBuilder = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .WithExactlyOnceQoS()
                .WithRetainFlag()
                .Build();

            await _client.PublishAsync(mqttApplicationMessageBuilder);
        }

        private string GetAllMessages()
        {
            var stringBuilder = new StringBuilder();

            foreach (var m in _messages)
            {
                stringBuilder.AppendLine(m + " | ");
                stringBuilder.AppendLine();
            }

            return stringBuilder.ToString();
        }

        public static async void SendMessage(string ip, int port, string topic, string message)
        {
            var client = new MqttDoubleChannelClientAsync();

            await client.Connect(ip, port);
            client.Publish(topic, message);
        }

        public static async Task<string> GetMessages(string ip, int port, string topic)
        {
            var client = new MqttDoubleChannelClientAsync();

            await client.Connect(ip, port);
            client.Subscribe(topic);

            return client.GetAllMessages();
        }
    }
}