using NUnit.Framework;
using RestSharp;
using System.Net;

namespace RestfulBookerApiTests;

[TestFixture]
public class DeleteBookingTests
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

    private string CreateBooking()
    {
        var request = new RestRequest("/booking", Method.Post);
        request.AddBody(
            "{\"firstname\":\"Delete\",\"lastname\":\"Test\",\"totalprice\":100,\"depositpaid\":true," +
            "\"bookingdates\":{\"checkin\":\"2025-11-01\",\"checkout\":\"2025-11-05\"}}",
            ContentType.Json);
        var response = client.Execute(request);
        var match = System.Text.RegularExpressions.Regex.Match(response.Content!, "\"bookingid\":(\\d+)");
        return match.Groups[1].Value;
    }

    [Test]
    public void DeleteBookingWithValidTokenReturnsCreated()
    {
        var bookingId = CreateBooking();

        var request = new RestRequest($"/booking/{bookingId}", Method.Delete);
        request.AddHeader("Cookie", $"token={token}");

        var response = client.Execute(request);

        Assert.That((int)response.StatusCode, Is.EqualTo(201),
            "DELETE /booking/{id} with valid token should return 201 Created");
    }

    [Test]
    public void DeletedBookingIsNoLongerRetrievable()
    {
        var bookingId = CreateBooking();

        var deleteRequest = new RestRequest($"/booking/{bookingId}", Method.Delete);
        deleteRequest.AddHeader("Cookie", $"token={token}");
        client.Execute(deleteRequest);

        var getRequest = new RestRequest($"/booking/{bookingId}", Method.Get);
        var getResponse = client.Execute(getRequest);

        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound),
            "Deleted booking should return 404 when accessed again");
    }

    [Test]
    public void DeleteBookingWithoutTokenReturnsMethodNotAllowed()
    {
        var bookingId = CreateBooking();

        var request = new RestRequest($"/booking/{bookingId}", Method.Delete);

        var response = client.Execute(request);

        Assert.That((int)response.StatusCode, Is.EqualTo(403),
            "DELETE without token should return 403 Forbidden");
    }

    [Test]
    public void DeleteNonExistentBookingReturnsMethodNotAllowed()
    {
        var request = new RestRequest("/booking/999999", Method.Delete);
        request.AddHeader("Cookie", $"token={token}");

        var response = client.Execute(request);

        Assert.That((int)response.StatusCode, Is.EqualTo(405),
            "DELETE on non-existent booking should return 405 Method Not Allowed");
    }
}
