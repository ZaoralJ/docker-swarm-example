﻿namespace MachineSqlDataService
{
    public class Configuration
    {
        public string RabbitMqBrokerName { get; set; }
        public string RabbitMqQueueName { get; set; }
        public string RabbitMqHost { get; set; }
        public string RabbitMqUser { get; set; }
        public string RabbitMqPassword { get; set; }
        public int RabbitMqPrefetchCount { get; set; }
        public string SqlConnectionString { get; set; }
    }
}