# RaceDay API

[![Part 2 API CI](https://github.com/ST10484115/RaceDay/actions/workflows/part2-ci.yml/badge.svg?branch=part2-api)](https://github.com/ST10484115/RaceDay/actions/workflows/part2-ci.yml)

## 1. Project Overview

RaceDay is a RESTful API for managing running, walking and cycling events. It supports user registration, authentication, event management, event locations, categories, participant enrolments and race results.

The API uses role-based authorisation to distinguish between Organisers and Participants. Entity Framework Core manages database access, while SQL Server stores application data.

## 2. Technologies Used

- C#
- ASP.NET Core Web API
- .NET 10
- Entity Framework Core
- SQL Server
- Cookie-based authentication
- Role-based authorisation
- Swagger / OpenAPI
- xUnit automated testing
- SQLite in-memory database for integration tests
- GitHub Actions for continuous integration

## 3. Main Features

- Register and authenticate users.
- Hash passwords before storing them.
- Retrieve and update the authenticated user's profile.
- Create, view, update and delete events, subject to authorisation rules.
- Manage event locations and categories.
- Allow participants to enrol in eligible events.
- Record and update race results.
- Restrict access according to the user's role and resource ownership.
- Validate request data and handle common errors.
- Automatically build the API and run tests through GitHub Actions.

## 4. Project Structure

```text
RaceDay/
├── .github/
│   └── workflows/
│       ├── part2-ci.yml
│       └── validate.yml
├── RaceDay.API/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Models/
│   ├── Properties/
│   ├── Program.cs
│   └── appsettings.json
├── RaceDay.API.Tests/
│   ├── AuthIntegrationTests.cs
│   ├── CustomWebApplicationFactory.cs
│   └── UnitTest1.cs
├── docs/
├── .gitignore
├── README.md
└── RaceDay.slnx
```

The API project contains the application code, controllers, data-access configuration, models and DTOs. The test project contains unit and integration tests. The `docs` directory contains supporting database design and project documentation.

## 5. Requirements

Install the following before running the application:

- Visual Studio with the .NET 10 SDK.
- SQL Server LocalDB or a compatible SQL Server instance.
- Git, if cloning the repository.

The development configuration uses Windows SQL Server LocalDB. Other environments may require a different connection string.

## 6. Database Configuration

RaceDay uses Entity Framework Core Code First to define the database structure through C# models and migrations.

The development connection string in `appsettings.json` is configured for SQL Server LocalDB:

```json
{
  "ConnectionStrings": {
    "RaceDayConnection": "Server=(localdb)\\MSSQLLocalDB;Database=RaceDay;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

Do not commit passwords, API keys or other secrets. Use an appropriate configuration provider for production environments.

### Create or update the database

Open a terminal in the repository root and run:

```powershell
dotnet restore RaceDay.slnx
dotnet ef database update --project RaceDay.API --startup-project RaceDay.API
```

The second command applies the existing Entity Framework Core migrations to the configured database.

If the `dotnet ef` command is unavailable, install the command-line tool:

```powershell
dotnet tool install --global dotnet-ef
```

If the tool is already installed, ensure its version is compatible with the project's Entity Framework Core version.

## 7. Running the API

From the repository root, run:

```powershell
dotnet run --project RaceDay.API
```

Swagger UI is enabled in the development environment. Open the Swagger URL displayed in the application output, normally similar to:

`https://localhost:<port>/swagger`

The actual port and protocol depend on the active launch profile.

## 8. Authentication and Authorisation

The API supports two roles:

- **Organiser:** manages events, categories and locations, and records results for events they organise.
- **Participant:** views available events, enrols in eligible events and accesses their own enrolments and results.

Registration and login are handled by the authentication controller. Passwords are hashed using ASP.NET Core password-hashing functionality instead of being stored as plain text.

Successful login establishes an authentication cookie. Protected endpoints require authentication, and role-restricted endpoints check the user's assigned role. Resource ownership is also checked where applicable.

For a production system, Organiser registration should be protected by an approval or invitation process.

## 9. API Endpoint Reference

Routes are relative to the API base URL. Protected endpoints require a valid authentication cookie and, where specified, the appropriate role.

### Authentication

| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/api/Auth/register` | Register a user |
| POST | `/api/Auth/login` | Authenticate a user |
| GET | `/api/Auth/me` | Retrieve the authenticated user's profile |
| POST | `/api/Auth/logout` | Sign out the authenticated user |

### Users

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/Users/me` | Retrieve the current user's profile |
| PUT | `/api/Users/me` | Update the current user's profile |

### Events

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/Events` | List events |
| GET | `/api/Events/{id}` | Retrieve an event |
| GET | `/api/Events/mine` | List events organised by the current Organiser |
| POST | `/api/Events` | Create an event |
| PUT | `/api/Events/{id}` | Update an event |
| DELETE | `/api/Events/{id}` | Delete an event when permitted |

### Locations

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/Locations` | List locations |
| GET | `/api/Locations/{id}` | Retrieve a location |
| POST | `/api/Locations` | Create a location |
| PUT | `/api/Locations/{id}` | Update a location |
| DELETE | `/api/Locations/{id}` | Delete a location when permitted |

### Categories

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/Categories/event/{eventId}` | List categories for an event |
| GET | `/api/Categories/{id}` | Retrieve a category |
| POST | `/api/Categories/event/{eventId}` | Add a category to an event |
| PUT | `/api/Categories/{id}` | Update a category |
| DELETE | `/api/Categories/{id}` | Delete a category when permitted |

### Enrolments

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/Enrolments` | List enrolments visible to the current user |
| GET | `/api/Enrolments/{id}` | Retrieve an enrolment |
| POST | `/api/Enrolments` | Enrol the authenticated Participant |
| DELETE | `/api/Enrolments/{id}` | Cancel an enrolment when permitted |

### Results

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/Results` | List results visible to the current user |
| GET | `/api/Results/{id}` | Retrieve a result |
| POST | `/api/Results/{enrolmentId}` | Record a result for an enrolment |
| PUT | `/api/Results/{id}` | Update a result |

Swagger provides the request schemas and response information for exploring the endpoints.

## 10. Running the Tests

From the repository root, run:

```powershell
dotnet test RaceDay.slnx
```

The test project includes DTO validation and integration tests covering registration, login, unauthorised access, role restrictions, event and category operations, enrolments and result recording.

Integration tests use an in-memory SQLite database so they can run without connecting to the development SQL Server database.

The latest locally confirmed test run reported 19 passed, 0 failed and 0 skipped.

## 11. Continuous Integration

The Part 2 workflow is located at:

`.github/workflows/part2-ci.yml`

It runs when changes are pushed to `part2-api` and when pull requests target that branch. The workflow restores dependencies, builds the solution in Release configuration and runs the automated tests.

The latest verified GitHub Actions run completed successfully.

The original Part 1 validation workflow remains in the repository.

## 12. Database Design Documentation

Supporting database design and project documentation are maintained in the `docs` directory. These artefacts document the database structure, entity relationships and implementation planning for RaceDay.

## 13. Project Status

The Part 2 API build and automated test workflow has been verified as successful on GitHub Actions. The latest locally confirmed test run reported 19 passing tests.

Remaining technical checks and submission requirements should be reviewed before the project is considered complete.
