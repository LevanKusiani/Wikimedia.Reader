namespace Wikimedia.Common.Kafka
{
    public class KafkaOptions
    { 
        public string BootstrapServers { get; set; } = "";
        public string SecurityProtocol { get; set; } = "";
        public string ClientId { get; set; } = "";
        public ProducerOptions Producer { get; set; } = new();
        public ConsumerOptions Consumer { get; set; } = new();
        public DeadLetterOptions DLQ { get; set; } = new();
    }

    public class ProducerOptions
    {
        public string Acks { get; set; } = "all";
        public bool EnableIdempotence { get; set; }
        public string CompressionType { get; set; } = "none";
        public int Retries { get; set; }
        public int LingerMs { get; set; }
        public int BatchSize { get; set; }
    }

    public class ConsumerOptions
    {
        public string GroupId { get; set; } = "";
        public string ConsumerTopic { get; set; } = "";
        public string AutoOffsetReset { get; set; } = "latest";
        public bool EnableAutoCommit { get; set; }
    }

    public class DeadLetterOptions
    {
        public string Topic { get; set; } = "";
    }
}
