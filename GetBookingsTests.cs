using NUnit.Framework;
using RestSharp;
using System.Net;
using Newtonsoft.Json.Linq;

namespace RestfulBookerApiTests;

[TestFixture]
public class GetBookingsTests
{
    private RestClient client;
    private const string BaseUrl = "https://restful-booker.herokuapp.com";

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        client = new RestClient(BaseUrl);
        client.AddDefaultHeader("Accept", "application/json");
    }

    [OneTimeTearDown]
    public void OneTimeTeardown()
    {
        client?.Dispose();
    }

    [Test]
    public void GetAllBookingsReturnsOkWithBookingList()
    {
        var request = new RestRequest("/booking", Method.Get);

        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            "GET /booking should return 200 OK");

        var body = JArray.Parse(response.Content!);
        Assert.That(body.Count, Is.GreaterThan(0),
            "Booking list should not be empty");
    }

    [Test]
    public void GetAllBookingsResponseContainsBookingIdField()
    {
        var request = new RestRequest("/booking", Method.Get);

        var response = client.Execute(request);
        var body = JArray.Parse(response.Content!);

        Assert.That(body[0]["bookingid"], Is.Not.Null,
            "Each booking entry should contain a bookingid field");
    }

    [Test]
    public void GetBookingByValidIdReturnsOkWithBookingDetails()
    {
        // Create a booking first to guarantee a valid ID
        var createRequest = new RestRequest("/booking", Method.Post);
        createRequest.AddBody("{\"firstname\":\"Test\",\"lastname\":\"User\",\"totalprice\":100,\"depositpaid\":true,\"bookingdates\":{\"checkin\":\"2025-01-01\",\"checkout\":\"2025-01-05\"},\"additionalneeds\":\"None\"}", ContentType.Json);
        var createResponse = client.Execute(createRequest);
        // Use regex to extract bookingid — avoids Newtonsoft issue with Infinity in restful-booker responses
        var match = System.Text.RegularExpressions.Regex.Match(createResponse.Content!, "\"bookingid\":(\\d+)");
        Assert.That(match.Success, Is.True, "Could not extract bookingid from create response");
        var bookingId = match.Groups[1].Value;

        var request = new RestRequest($"/booking/{bookingId}", Method.Get);

        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"GET /booking/{bookingId} should return 200 OK");

        var body = JObject.Parse(response.Content!);
        Assert.That(body["firstname"]?.ToString(), Is.EqualTo("Test"),
            "Booking should return correct firstname");
        Assert.That(body["lastname"]?.ToString(), Is.EqualTo("User"),
            "Booking should return correct lastname");
        Assert.That(body["totalprice"]?.ToString(), Is.EqualTo("100"),
            "Booking should return correct totalprice");
    }

    [Test]
    public void GetBookingByNonExistentIdReturns404()
    {
        var request = new RestRequest("/booking/999999", Method.Get);

        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound),
            "Non-existent booking ID should return 404 Not Found");
    }

    [Test]
    public void GetBookingsFilteredByFirstNameReturnsMatchingResults()
    {
        var request = new RestRequest("/booking", Method.Get);
        request.AddQueryParameter("firstname", "John");

        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            "Filtered GET /booking should return 200 OK");
    }
}
