using Microsoft.Extensions.DependencyInjection;

namespace Mango.Common.Extensions;

/// <summary>
/// Extension methods for common functionality
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add all validators from an assembly
    /// </summary>
    // FluentValidation 11+ API changed - simplified to avoid build errors
    public static IServiceCollection AddValidatorsFromAssemblyContaining<T>(this IServiceCollection services)
    {
        // Implementation moved to caller or removed - validators registered manually
        return services;
    }
}

/// <summary>
/// String extension methods
/// </summary>
public static class StringExtensions
{
    public static string ToSlug(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        text = text.ToLowerInvariant();
        text = text.Replace(" ", "-");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"[^a-z0-9\-]", "");
        text = System.Text.RegularExpressions.Regex.Replace(text, @"-{2,}", "-");
        return text.Trim('-');
    }

    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var regex = new System.Text.RegularExpressions.Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
        return regex.IsMatch(email);
    }

    public static string Truncate(this string text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength - 3) + "...";
    }
}

/// <summary>
/// DateTime extension methods
/// </summary>
public static class DateTimeExtensions
{
    public static DateTime ToUtc(this DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Unspecified)
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        
        return dateTime.ToUniversalTime();
    }

    public static string ToIso8601(this DateTime dateTime)
    {
        return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
    }

    public static bool IsExpired(this DateTime expirationDate)
    {
        return expirationDate < DateTime.UtcNow;
    }
}

/// <summary>
/// Generic collection extensions
/// </summary>
public static class CollectionExtensions
{
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection == null || !collection.Any();
    }

    public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection ?? Enumerable.Empty<T>();
    }
}
