using System.Text.Json.Serialization;

public record WikiRecentChange(
    [property: JsonPropertyName("$schema")] string Schema,
    [property: JsonPropertyName("meta")] Meta Meta,
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("namespace")] int Namespace,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("title_url")] string TitleUrl,
    [property: JsonPropertyName("comment")] string Comment,
    [property: JsonPropertyName("timestamp")] long Timestamp,
    [property: JsonPropertyName("user")] string User,
    [property: JsonPropertyName("bot")] bool IsBot,
    [property: JsonPropertyName("notify_url")] string NotifyUrl,
    [property: JsonPropertyName("minor")] bool IsMinor,
    [property: JsonPropertyName("patrolled")] bool IsPatrolled,
    [property: JsonPropertyName("length")] WikiLength Length,
    [property: JsonPropertyName("revision")] WikiRevision Revision,
    [property: JsonPropertyName("server_url")] string ServerUrl,
    [property: JsonPropertyName("server_name")] string ServerName,
    [property: JsonPropertyName("server_script_path")] string ServerScriptPath,
    [property: JsonPropertyName("wiki")] string Wiki,
    [property: JsonPropertyName("parsedcomment")] string ParsedComment
);

public record Meta(
    [property: JsonPropertyName("uri")] string Uri,
    [property: JsonPropertyName("request_id")] string RequestId,
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("domain")] string Domain,
    [property: JsonPropertyName("stream")] string Stream,
    [property: JsonPropertyName("dt")] DateTimeOffset DateTime,
    [property: JsonPropertyName("topic")] string Topic,
    [property: JsonPropertyName("partition")] int Partition,
    [property: JsonPropertyName("offset")] long Offset
);

public record WikiLength(
    [property: JsonPropertyName("old")] int? Old,
    [property: JsonPropertyName("new")] int? New
);

public record WikiRevision(
    [property: JsonPropertyName("old")] long? Old,
    [property: JsonPropertyName("new")] long? New
);
