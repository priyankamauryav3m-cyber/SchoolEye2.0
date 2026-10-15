using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MyApp.Common;

namespace SchoolEye.Tests.TestHelpers;

/// <summary>Helpers to unwrap IActionResult into typed values.</summary>
public static class ResultAssert
{
    public const string SecretExceptionText = "SQL-SECRET: Invalid object name 'dbo.TestTable' at Server=test-db";

    // Unescaped so characters like ' are not hidden as ' and the leak check stays reliable.
    private static readonly JsonSerializerOptions RawJson = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static int StatusOf(IActionResult result) => result switch
    {
        ObjectResult o => o.StatusCode ?? 200,
        StatusCodeResult s => s.StatusCode,
        _ => throw new Xunit.Sdk.XunitException($"Unexpected result type {result.GetType().Name}")
    };

    public static object? ValueOf(IActionResult result)
    {
        var obj = Assert.IsAssignableFrom<ObjectResult>(result);
        return obj.Value;
    }

    /// <summary>Asserts status and unwraps ApiResponse&lt;T&gt;.</summary>
    public static ApiResponse<T> Api<T>(IActionResult result, int expectedStatus)
    {
        Assert.Equal(expectedStatus, StatusOf(result));
        return Assert.IsType<ApiResponse<T>>(ValueOf(result));
    }

    /// <summary>Reads a property from an anonymous response object (e.g. new { message = ... }).</summary>
    public static object? Prop(object? value, string name)
    {
        Assert.NotNull(value);
        var prop = value!.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(prop);
        return prop!.GetValue(value);
    }

    /// <summary>CLAUDE.md §8B: response body must not carry the raw exception text.</summary>
    public static void DoesNotLeak(IActionResult result, string secret = SecretExceptionText)
    {
        var value = (result as ObjectResult)?.Value;
        var body = value switch
        {
            null => string.Empty,
            string s => s,
            _ => JsonSerializer.Serialize(value, RawJson)
        };
        Assert.DoesNotContain(secret, body);
    }
}
