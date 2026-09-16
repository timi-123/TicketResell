namespace TicketResell.Services;

public static class UploadTypes
{
    private static readonly Dictionary<string, string> TypesByExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg"
    };

    public static bool IsAllowed(string extension)
    {
        return TypesByExtension.ContainsKey(extension);
    }

    public static string ContentTypeFor(string extension)
    {
        return TypesByExtension.TryGetValue(extension, out var contentType) ? contentType : "application/octet-stream";
    }

    public static string AllowedList
    {
        get { return "PDF, PNG and JPG"; }
    }
}
