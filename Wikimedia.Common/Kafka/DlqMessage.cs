namespace Wikimedia.Common.Kafka
{
    public class DlqMessage
    {
        public string Source { get; init; } = "";
        public string Reason { get; init; } = "";
        public string Error { get; init; } = "";
        public string OriginalPayload { get; init; } = "";
        public DateTime FailedAtUtc { get; init; }
        public string Topic { get; init; } = "";
        public int? Partition { get; init; }
        public long? Offset { get; init; }
    }
}
