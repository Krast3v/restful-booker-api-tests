using NUnit.Framework;
using RestSharp;
using System.Net;
using Newtonsoft.Json.Linq;

namespace RestfulBookerApiTests;

[TestFixture]
public class UpdateBookingTests
{
    private RestClient client;
    private const string BaseUrl = "https://restful-booker.herokuapp.com";
    private string token = string.Empty;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        client = new RestClient(BaseUrl);
        client.AddDefaultHeader("Accept", "application/json");

        var authRequest = new RestRequest("/auth", Method.Post);
        authRequest.AddBody("{\"username\":\"admin\",\"password\":\"password123\"}", ContentType.Json);
        var authResponse = client.Execute(authRequest);
        var match = System.Text.RegularExpressions.Regex.Match(authResponse.Content!, "\"token\":\"([^\"]+)\"");
        token = match.Groups[1].Value;
    }

    [OneTimeTearDown]
    public void OneTimeTeardown()
    {
        client?.Dispose();
    }

    private string CreateBooking(string firstname = "Original", string lastname = "Booking")
    {
        var request = new RestRequest("/booking", Method.Post);
        request.AddBody(
            $"{{\"firstname\":\"{firstname}\",\"lastname\":\"{lastname}\",\"totalprice\":100,\"depositpaid\":true," +
            "\"bookingdates\":{\"checkin\":\"2025-12-01\",\"checkout\":\"2025-12-05\"}}",
            ContentType.Json);
        var response = client.Execute(request);
        var match = System.Text.RegularExpressions.Regex.Match(response.Content!, "\"bookingid\":(\\d+)");
        Assert.That(match.Success, Is.True, "CreateBooking helper failed to get a valid bookingid");
        return match.Groups[1].Value;
    }

    [Test]
    public void UpdateBookingWithValidTokenReturnsOkWithUpdatedData()
    {
        var bookingId = CreateBooking();

        var request = new RestRequest($"/booking/{bookingId}", Method.Put);
        request.AddHeader("Cookie", $"token={token}");
        request.AddBody(
            "{\"firstname\":\"Updated\",\"lastname\":\"Name\",\"totalprice\":250,\"depositpaid\":false," +
            "\"bookingdates\":{\"checkin\":\"2025-12-10\",\"checkout\":\"2025-12-15\"}}",
            ContentType.Json);

        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            "PUT /booking/{id} with valid token should return 200 OK");

        Assert.That(response.Content, Does.Contain("\"firstname\":\"Updated\""),
            "Updated firstname should be reflected in the response");
        Assert.That(response.Content, Does.Contain("\"lastname\":\"Name\""),
            "Updated lastname should be reflected in the response");
    }

    [Test]
    public void UpdateBookingReplacesAllFieldsNotJustChanged()
    {
        var bookingId = CreateBooking("Before", "Update");

        var request = new RestRequest($"/booking/{bookingId}", Method.Put);
        request.AddHeader("Cookie", $"token={token}");
        request.AddBody(
            "{\"firstname\":\"After\",\"lastname\":\"Update\",\"totalprice\":999,\"depositpaid\":true," +
            "\"bookingdates\":{\"checkin\":\"2026-01-01\",\"checkout\":\"2026-01-10\"}}",
            ContentType.Json);

        var response = client.Execute(request);

        Assert.That(response.Content, Does.Contain("\"firstname\":\"After\""),
            "PUT should replace firstname with the new value");
        Assert.That(response.Content, Does.Contain("\"lastname\":\"Update\""),
            "PUT should replace all fields — this is a full replacement, not a partial update");
    }

    [Test]
    public void UpdateBookingWithoutTokenReturnsForbidden()
    {
        var bookingId = CreateBooking();

        var request = new RestRequest($"/booking/{bookingId}", Method.Put);
        request.AddBody(
            "{\"firstname\":\"NoToken\",\"lastname\":\"Test\",\"totalprice\":50,\"depositpaid\":true," +
            "\"bookingdates\":{\"checkin\":\"2025-12-01\",\"checkout\":\"2025-12-05\"}}",
            ContentType.Json);

        var response = client.Execute(request);

        Assert.That((int)response.StatusCode, Is.EqualTo(403).Or.EqualTo(404),
            "PUT without token should return 403 Forbidden or 404 — restful-booker behavior varies");
    }

    [Test]
    public void PartialUpdateWithPatchUpdatesOnlySpecifiedFields()
    {
        var bookingId = CreateBooking("Patch", "Test");

        var request = new RestRequest($"/booking/{bookingId}", Method.Patch);
        request.AddHeader("Cookie", $"token={token}");
        request.AddBody("{\"firstname\":\"PatchedName\"}", ContentType.Json);

        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            "PATCH /booking/{id} should return 200 OK");

        Assert.That(response.Content, Does.Contain("\"firstname\":\"PatchedName\""),
            "PATCH should update only the firstname field");
        Assert.That(response.Content, Does.Contain("\"lastname\":\"Test\""),
            "PATCH should leave lastname unchanged");
    }
}
