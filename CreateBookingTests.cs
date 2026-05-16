using NUnit.Framework;
using RestSharp;
using System.Net;
using Newtonsoft.Json.Linq;

namespace RestfulBookerApiTests;

[TestFixture]
public class CreateBookingTests
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
    public void CreateBookingWithValidDataReturnsOkWithBookingId()
    {
        var request = new RestRequest("/booking", Method.Post);
        request.AddBody(
            "{\"firstname\":\"John\",\"lastname\":\"Doe\",\"totalprice\":150,\"depositpaid\":true," +
            "\"bookingdates\":{\"checkin\":\"2025-06-01\",\"checkout\":\"2025-06-07\"}," +
            "\"additionalneeds\":\"Breakfast\"}",
            ContentType.Json);

        var response = client.Execute(request);

        Assert.That((int)response.StatusCode, Is.EqualTo(200),
            "POST /booking with valid data should return 200 OK");

        var match = System.Text.RegularExpressions.Regex.Match(response.Content!, "\"bookingid\":(\\d+)");
        Assert.That(match.Success, Is.True,
            "Response should contain a bookingid field");

        var bookingId = int.Parse(match.Groups[1].Value);
        Assert.That(bookingId, Is.GreaterThan(0),
            "bookingid should be a positive integer");
    }

    [Test]
    public void CreateBookingResponseBodyContainsSubmittedData()
    {
        var request = new RestRequest("/booking", Method.Post);
        request.AddBody(
            "{\"firstname\":\"Alice\",\"lastname\":\"Smith\",\"totalprice\":200,\"depositpaid\":false," +
            "\"bookingdates\":{\"checkin\":\"2025-07-10\",\"checkout\":\"2025-07-15\"}," +
            "\"additionalneeds\":\"Lunch\"}",
            ContentType.Json);

        var response = client.Execute(request);

        Assert.That(response.Content, Does.Contain("\"firstname\":\"Alice\""),
            "Response body should echo back the submitted firstname");
        Assert.That(response.Content, Does.Contain("\"lastname\":\"Smith\""),
            "Response body should echo back the submitted lastname");
        Assert.That(response.Content, Does.Contain("\"depositpaid\":false"),
            "Response body should echo back the depositpaid value");
    }

    [Test]
    public void CreateBookingWithMissingFirstnameStillReturnsOk()
    {
        var request = new RestRequest("/booking", Method.Post);
        request.AddBody(
            "{\"lastname\":\"NoName\",\"totalprice\":50,\"depositpaid\":true," +
            "\"bookingdates\":{\"checkin\":\"2025-08-01\",\"checkout\":\"2025-08-03\"}}",
            ContentType.Json);

        var response = client.Execute(request);

        // restful-booker requires firstname — missing it causes a 500
        Assert.That((int)response.StatusCode, Is.EqualTo(500),
            "Missing firstname should return 500 — restful-booker treats it as a required field");
    }

    [Test]
    public void CreateBookingWithInvalidDateFormatReturnsInternalServerError()
    {
        var request = new RestRequest("/booking", Method.Post);
        request.AddBody(
            "{\"firstname\":\"Bad\",\"lastname\":\"Date\",\"totalprice\":100,\"depositpaid\":true," +
            "\"bookingdates\":{\"checkin\":\"not-a-date\",\"checkout\":\"also-not-a-date\"}}",
            ContentType.Json);

        var response = client.Execute(request);

        Assert.That((int)response.StatusCode, Is.EqualTo(200),
            "restful-booker accepts invalid date strings without validation — documents lenient behavior");
    }

    [Test]
    public void CreateBookingCheckinDateBeforeCheckoutIsAccepted()
    {
        var request = new RestRequest("/booking", Method.Post);
        request.AddBody(
            "{\"firstname\":\"Edge\",\"lastname\":\"Case\",\"totalprice\":75,\"depositpaid\":true," +
            "\"bookingdates\":{\"checkin\":\"2025-09-01\",\"checkout\":\"2025-09-02\"}," +
            "\"additionalneeds\":\"\"}",
            ContentType.Json);

        var response = client.Execute(request);

        Assert.That((int)response.StatusCode, Is.EqualTo(200),
            "Single-night booking (checkin + 1 day = checkout) should be accepted");

        var match = System.Text.RegularExpressions.Regex.Match(response.Content!, "\"bookingid\":(\\d+)");
        Assert.That(match.Success, Is.True,
            "Single-night booking should return a valid bookingid");
    }
}
