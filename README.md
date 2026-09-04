# RaceDay Event Management System

## Project Overview

RaceDay is a full-stack web-based event management system designed for South African road running, walking and cycling events.

The system is intended to allow event organisers to manage events, categories, participant enrolments and results, while participants can create accounts, browse available events, enter event categories and track their own results.

The project is being developed in multiple parts. Part 1 focuses on system planning and database design, Part 2 focuses on the REST API and database integration, and Part 3 focuses on the MVC web application and additional cloud functionality.

---

## User Roles

### Organiser

Organisers are responsible for managing race events and participant information.

Organiser functionality includes:

- Create events
- Edit events
- Delete events
- Manage event categories
- View event enrolments
- Capture participant results
- Update participant results
- Manage event locations

### Participant

Participants use the system to enter and track their participation in events.

Participant functionality includes:

- Create an account
- Log in
- Manage their profile
- Browse available events
- View event details
- View event categories
- Enter an event category
- View their own enrolments
- Cancel their own enrolments
- View their own results

---

## Part 1 – System Planning and Database Design

Part 1 establishes the foundation for the RaceDay system before API development begins.

The Part 1 documentation includes:

- Entity Relationship Diagram (ERD)
- REST API endpoint plan
- SQL Server database creation script
- Database seed data
- GitHub repository structure
- GitHub Actions workflow planning

The database design contains the following main entities:

- Users
- Events
- Categories
- Enrolments
- Results
- EventLocations

### Documentation

The Part 1 documentation is available in the `/docs` folder.

- `RaceDay_ERD.png` – Entity Relationship Diagram
- `RaceDay_ERD.drawio` – Editable ERD source file
- `API_Endpoint_Plan.md` – Planned REST API endpoints
- `RaceDay_Database.sql` – SQL Server database creation and seed script

---

## Part 2 – REST API

Part 2 will implement the planned REST API using C#.

The API will provide functionality for:

- Authentication
- User profiles
- Event management
- Category management
- Event enrolments
- Participant results
- Event locations

The API will connect to the RaceDay SQL Server database and will be tested before being integrated with the MVC application.

---

## Part 3 – MVC Web Application

Part 3 will provide the user-facing web application.

The MVC application will consume the REST API and provide separate functionality for organisers and participants.

Additional planned functionality includes:

- Event browsing
- Event registration
- Participant dashboards
- Organiser management features
- Event location information
- Weather information
- Azure Blob Storage integration
- Docker containerisation

---

## Database Design

The RaceDay database uses relational tables connected through primary and foreign keys.

The main relationships include:

- One organiser can manage many events.
- One location can be associated with many events.
- One event can contain many categories.
- One participant can have many enrolments.
- One event can have many enrolments.
- One category can have many enrolments.
- One enrolment can have zero or one result.

The `Enrolments` entity connects participants with event categories.

---

## API Design

The planned API follows REST conventions.

The API endpoint plan covers:

- Authentication
- User profiles
- Events
- Categories
- Enrolments
- Results
- Event locations

Detailed routes, HTTP methods, request bodies, expected responses and role requirements are documented in:

`docs/API_Endpoint_Plan.md`

---

## GitHub and Version Control

GitHub is used for source control and project submission.

The project uses meaningful commits to record development progress.

All project source files and required documentation will be committed and pushed to the repository.

---

## Continuous Integration

GitHub Actions will be used to validate the repository structure and required project files.

The workflow will verify that required documentation and project components are present in the repository.

### CI/CD Screenshot

The successful GitHub Actions workflow result will be added here before final submission.

`[GitHub Actions screenshot to be added]`

---

## Testing

Testing will be performed throughout development to verify that the RaceDay system functions correctly.

Testing will include:

- Authentication testing
- API endpoint testing
- Database operations
- Event management
- Category management
- Enrolment functionality
- Result management
- MVC integration
- Cloud storage functionality
- Application deployment

---

## CI/CD Validation

GitHub Actions is used to validate the RaceDay Part 1 repository structure and required documentation.

The workflow checks that the required Part 1 documentation is present, including the `/docs` folder and its required files. A successful green check confirms that the validation workflow completed successfully.

### GitHub Actions Validation Result

![GitHub Actions Validation](docs/GitHub_Actions.png)

The successful workflow run demonstrates that the RaceDay repository passed the Part 1 validation checks on the `main` branch.

---

## Part 1 Documentation

The following planning and database documentation has been completed for Part 1:

- **ERD:** `docs/RaceDay_ERD.png`
- **ERD Source:** `docs/RaceDay_ERD.drawio`
- **API Endpoint Plan:** `docs/API_Endpoint_Plan.md`
- **SQL Server Database Script:** `docs/RaceDay_Database.sql`

These documents provide the database structure, API planning and SQL Server implementation required before development of the REST API in Part 2.

---

## YouTube Demonstration

Part 1 demonstration video:

**YouTube Link:** [To be added before submission]

The video will provide a voice-over demonstration of the completed Part 1 planning documentation, database design, API endpoint plan and GitHub Actions validation.

## Project Structure

```text
RaceDay/
│
├── .github/
│   └── workflows/
│
├── docs/
│   ├── API_Endpoint_Plan.md
│   ├── RaceDay_Database.sql
│   ├── RaceDay_ERD.drawio
│   └── RaceDay_ERD.png
│
├── README.md
│
└── [Application source files will be added in Parts 2 and 3]