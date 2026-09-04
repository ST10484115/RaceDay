# RaceDay API Endpoint Plan

## 1. Overview

RaceDay is a web-based event management system designed for the South African road
running, walking and cycling community. The API will provide functionality for
authentication, user profiles, events, event categories, participant enrolments
and race results.

The API will be implemented in Part 2 using ASP.NET Core Web API and SQL Server.

The system supports two main roles:

- **Organiser** - creates, edits and deletes events, manages categories,
  captures participant results and views event enrolments.
- **Participant** - creates an account, browses events, enters events by
  selecting a category, views their own enrolments and tracks personal results.

---

## 2. Authentication

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| POST | `/api/auth/register` | Creates a new participant or organiser account. | Public | FirstName, LastName, Email, Password, Role | 201 Created with user details |
| POST | `/api/auth/login` | Authenticates an existing user. | Public | Email, Password | 200 OK with authentication result/token |

---

## 3. User Profile

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/users/{id}` | Retrieves a user's profile. | Authenticated User | None | 200 OK with profile details |
| PUT | `/api/users/{id}` | Updates the user's profile information. | Authenticated User | FirstName, LastName, Email | 200 OK with updated profile |

---

## 4. Events

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/events` | Returns all upcoming and available events. | Public | None | 200 OK with event list |
| GET | `/api/events/{id}` | Returns details for a specific event. | Public | None | 200 OK with event details |
| POST | `/api/events` | Creates a new event. | Organiser | EventName, Description, EventDate, LocationId, Status | 201 Created with new event |
| PUT | `/api/events/{id}` | Updates an existing event. | Organiser | EventName, Description, EventDate, LocationId, Status | 200 OK with updated event |
| DELETE | `/api/events/{id}` | Deletes an event. | Organiser | None | 204 No Content |

---

## 5. Event Categories

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/events/{eventId}/categories` | Returns all categories belonging to an event. | Public | None | 200 OK with category list |
| GET | `/api/categories/{id}` | Returns details for one category. | Public | None | 200 OK with category details |
| POST | `/api/events/{eventId}/categories` | Creates a category for an event. | Organiser | CategoryName, DistanceKm, EntryFee | 201 Created with new category |
| PUT | `/api/categories/{id}` | Updates an existing category. | Organiser | CategoryName, DistanceKm, EntryFee | 200 OK with updated category |
| DELETE | `/api/categories/{id}` | Deletes an event category. | Organiser | None | 204 No Content |

---

## 6. Event Enrolments

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/enrolments` | Returns enrolments belonging to the authenticated participant. Organisers can use this endpoint to view enrolments for their events. | Participant / Organiser | None | 200 OK with enrolment list |
| GET | `/api/enrolments/{id}` | Returns details for a specific enrolment. | Participant / Organiser | None | 200 OK with enrolment details |
| POST | `/api/enrolments` | Enrols a participant into an event category. | Participant | ParticipantId, EventId, CategoryId | 201 Created with enrolment |
| DELETE | `/api/enrolments/{id}` | Cancels or removes an enrolment. | Participant | None | 204 No Content |

---

## 7. Results

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/results` | Returns results relevant to the authenticated user. Organisers can access results for their events. | Participant / Organiser | None | 200 OK with result list |
| GET | `/api/results/{id}` | Returns details for a specific result. | Participant / Organiser | None | 200 OK with result details |
| POST | `/api/results` | Records a participant's result after an event. | Organiser | EnrolmentId, FinishTime, Position, ResultStatus | 201 Created with result |
| PUT | `/api/results/{id}` | Updates an existing participant result. | Organiser | FinishTime, Position, ResultStatus | 200 OK with updated result |

---

## 8. Event Locations

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|---|---|---|---|---|---|
| GET | `/api/locations/{id}` | Returns information about an event location. | Public | None | 200 OK with location details |
| POST | `/api/locations` | Creates a new event location. | Organiser | LocationName, Address, City, Province, Latitude, Longitude | 201 Created with location |
| PUT | `/api/locations/{id}` | Updates an event location. | Organiser | LocationName, Address, City, Province, Latitude, Longitude | 200 OK with updated location |
| DELETE | `/api/locations/{id}` | Deletes an event location. | Organiser | None | 204 No Content |

---

## 9. Endpoint Summary

The planned API provides functionality for:

1. User registration and login.
2. User profile management.
3. Event browsing and management.
4. Event category management.
5. Participant enrolments.
6. Participant result tracking.
7. Event result management by organisers.
8. Event location management.

The API will use HTTP status codes to communicate the result of each request.
Authentication and role-based authorization will be enforced at API level in
Part 2.

---

## 10. Role Access Summary

### Public

- Register
- Login
- Browse events
- View event details
- View event categories
- View category details
- View locations

### Organiser

- Create events
- Edit events
- Delete events
- Create categories
- Edit categories
- Delete categories
- View event enrolments
- Capture participant results
- Update results
- Manage event locations

### Participant

- Register
- Login
- Manage their profile
- Browse events
- View event details
- Enrol in an event category
- View their own enrolments
- Cancel their own enrolments
- View their own results

---

## 11. Relationship With Database Design

The planned API corresponds with the entities in the RaceDay database:

- `Users` supports authentication and user profiles.
- `Events` supports event management.
- `Categories` supports event categories.
- `Enrolments` records participant entries.
- `Results` records participant performance.
- `EventLocations` stores event location information.

The API implementation in Part 2 should follow this endpoint plan. Any
significant changes to the planned routes or functionality should be documented
and explained in the project README.