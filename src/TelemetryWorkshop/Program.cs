using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

// Tracing is configured by the OpenTelemetry Operator, not here. The
// instrumentation.opentelemetry.io/inject-dotnet annotation on the deployment
// auto-instruments ASP.NET Core and exports OTLP/HTTP to the collector sidecar,
// so all this app needs to do is decorate the Activity the operator created.

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Results.Text("200 OK", "text/plain"));

app.MapPost("/api/check", async (HttpRequest request) =>
{
    var span = Activity.Current;

    // Parse the body by hand. Binding to a typed parameter would make ASP.NET
    // Core answer malformed JSON with its own 400, and this workshop depends on
    // a bad payload producing a 500.
    JsonNode? body = null;
    try
    {
        body = await JsonNode.ParseAsync(request.Body);
    }
    catch (JsonException)
    {
        // Leave body null and fall through to the 500 below.
    }

    if (body is not JsonObject data
        || !data.TryGetPropertyValue("amount", out var amountNode)
        || amountNode is null)
    {
        span?.SetTag("http.status_code", 500);
        span?.SetTag("error.message", "'amount' key missing in JSON body");
        return Results.Problem(detail: "'amount' key missing in JSON body", statusCode: 500);
    }

    if (!TryGetAmount(amountNode, out var amt))
    {
        span?.SetTag("http.status_code", 500);
        span?.SetTag("error.message", "Invalid amount format");
        return Results.Problem(detail: "Sending ERROR 500", statusCode: 500);
    }

    if (amt == 1337)
    {
        var egg = Encoding.UTF8.GetString(Convert.FromBase64String(
            "SWYgeW91IGNhbiBwcm9kdWNlIHRoaXMgbWVzc2FnZSBpbiBKYWVnZXIsIHlvdSd2ZSBmb3VuZCB0aGUgZWFzdGVyIGVnZwo="));
        span?.SetTag("http.status_code", 503);
        span?.SetTag("error.message", egg);
        return Results.Problem(detail: "Sending ERROR 503", statusCode: 503);
    }

    if (amt < 25000)
    {
        span?.SetTag("http.status_code", 500);
        span?.SetTag("error.message", "Amount below minimum threshold");
        return Results.Problem(detail: "Sending ERROR 500", statusCode: 500);
    }

    return Results.Json(new { message = "SUCCESS VALID", data });
});

app.Run();

// Accepts a JSON number or a numeric string, matching what float() allowed in
// the Python version of this workshop.
static bool TryGetAmount(JsonNode node, out double value)
{
    value = 0;

    if (node is not JsonValue jsonValue)
    {
        return false;
    }

    if (jsonValue.TryGetValue<double>(out var number))
    {
        value = number;
        return true;
    }

    return jsonValue.TryGetValue<string>(out var text)
           && double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
}
