namespace Wikimedia.Common.Extensions
{
    public static class WikiRecentChangeExtensions
    {
        public static WikiRecentChange Deserialize(string json)
        {
            var result = System.Text.Json.JsonSerializer.Deserialize<WikiRecentChange>(json)
                ?? throw new InvalidOperationException("Deserialization resulted in null");

            if (string.IsNullOrWhiteSpace(result.ServerName) || string.IsNullOrWhiteSpace(result.Title))
            {
                throw new InvalidOperationException("Deserialized object has invalid ServerName or Title");
            }

            return result;
        }
    }

}
