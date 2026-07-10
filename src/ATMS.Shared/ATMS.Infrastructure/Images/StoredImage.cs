namespace ATMS.Infrastructure.Images;

public sealed record StoredImage(
    string RelativePath,
    string Url,
    string ContentType,
    long Size);
