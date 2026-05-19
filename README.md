# Restful Booker API Tests

Automated API test suite for [restful-booker.herokuapp.com](https://restful-booker.herokuapp.com) — a hotel booking API built for QA practice.

## Tech Stack

- **C# / .NET 10**
- **NUnit** — test runner
- **RestSharp 114** — HTTP client
- **Newtonsoft.Json** — JSON parsing

## Test Coverage

| File | Endpoint | Tests |
|---|---|---|
| `GetBookingsTests.cs` | GET /booking | 5 |
| `CreateBookingTests.cs` | POST /booking | 5 |
| `AuthenticationTests.cs` | POST /auth | 5 |
| `DeleteBookingTests.cs` | DELETE /booking/{id} | 4 |
| `UpdateBookingTests.cs` | PUT + PATCH /booking/{id} | 4 |

**Total: 23 tests**

## What Is Tested

- Status codes (200, 201, 403, 404, 500)
- Response body correctness
- Authentication flow — token generation and usage
- CRUD operations — Create, Read, Update, Delete
- Error handling — missing fields, invalid data, missing token
- PUT vs PATCH behavior — full replace vs partial update

## Notes on restful-booker

This API has intentional quirks useful for QA practice:

- Returns `200` on successful booking creation (not `201`)
- Returns `NaN` for numeric fields in some responses — raw string assertions used where needed
- Some booking IDs return `418` — tests create fresh bookings to guarantee valid IDs
- Missing required fields (e.g. `firstname`) return `500` instead of `400`

## Run Tests

```bash
dotnet test
```
