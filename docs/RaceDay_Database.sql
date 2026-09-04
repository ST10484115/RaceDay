/*
    RaceDay Database
    PROG6212 Portfolio of Evidence - Part 1

    This script creates the RaceDay database schema and inserts
    sample data for testing.
*/

CREATE DATABASE RaceDay;
GO

USE RaceDay;
GO

-- Drop existing tables if the script is being re-run.
DROP TABLE IF EXISTS Results;
DROP TABLE IF EXISTS Enrolments;
DROP TABLE IF EXISTS Categories;
DROP TABLE IF EXISTS Events;
DROP TABLE IF EXISTS EventLocations;
DROP TABLE IF EXISTS Users;
GO

-- =========================================================
-- USERS
-- Stores both Organisers and Participants.
-- =========================================================

CREATE TABLE Users
(
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Role VARCHAR(20) NOT NULL
        CONSTRAINT CK_Users_Role
        CHECK (Role IN ('Organiser', 'Participant')),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- =========================================================
-- EVENT LOCATIONS
-- Stores the location of each event.
-- =========================================================

CREATE TABLE EventLocations
(
    LocationId INT IDENTITY(1,1) PRIMARY KEY,
    LocationName VARCHAR(100) NOT NULL,
    Address VARCHAR(255) NOT NULL,
    City VARCHAR(100) NOT NULL,
    Province VARCHAR(100) NOT NULL,
    Latitude DECIMAL(9,6) NULL,
    Longitude DECIMAL(9,6) NULL
);
GO

-- =========================================================
-- EVENTS
-- Stores events created by Organisers.
-- =========================================================

CREATE TABLE Events
(
    EventId INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserId INT NOT NULL,
    EventName VARCHAR(100) NOT NULL,
    Description VARCHAR(500) NULL,
    EventDate DATE NOT NULL,
    LocationId INT NOT NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'Upcoming'
        CONSTRAINT CK_Events_Status
        CHECK (Status IN ('Upcoming', 'Open', 'Completed', 'Cancelled')),
    
    CONSTRAINT FK_Events_Organiser
        FOREIGN KEY (OrganiserId)
        REFERENCES Users(UserId),

    CONSTRAINT FK_Events_Location
        FOREIGN KEY (LocationId)
        REFERENCES EventLocations(LocationId)
);
GO

-- =========================================================
-- CATEGORIES
-- Categories belonging to an event.
-- =========================================================

CREATE TABLE Categories
(
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL,
    CategoryName VARCHAR(100) NOT NULL,
    DistanceKm DECIMAL(6,2) NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL DEFAULT 0.00,

    CONSTRAINT FK_Categories_Event
        FOREIGN KEY (EventId)
        REFERENCES Events(EventId),

    CONSTRAINT CK_Categories_Distance
        CHECK (DistanceKm > 0),

    CONSTRAINT CK_Categories_EntryFee
        CHECK (EntryFee >= 0)
);
GO

-- =========================================================
-- ENROLMENTS
-- Records participants entering event categories.
-- =========================================================

CREATE TABLE Enrolments
(
    EnrolmentId INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantId INT NOT NULL,
    EventId INT NOT NULL,
    CategoryId INT NOT NULL,
    EnrolmentDate DATE NOT NULL DEFAULT GETDATE(),
    Status VARCHAR(20) NOT NULL DEFAULT 'Confirmed'
        CONSTRAINT CK_Enrolments_Status
        CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled')),

    CONSTRAINT FK_Enrolments_Participant
        FOREIGN KEY (ParticipantId)
        REFERENCES Users(UserId),

    CONSTRAINT FK_Enrolments_Event
        FOREIGN KEY (EventId)
        REFERENCES Events(EventId),

    CONSTRAINT FK_Enrolments_Category
        FOREIGN KEY (CategoryId)
        REFERENCES Categories(CategoryId),

    CONSTRAINT UQ_Enrolments_Participant_Category
        UNIQUE (ParticipantId, CategoryId)
);
GO

-- =========================================================
-- RESULTS
-- Stores participant race results.
-- =========================================================

CREATE TABLE Results
(
    ResultId INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentId INT NOT NULL UNIQUE,
    FinishTime TIME NULL,
    Position INT NULL,
    ResultStatus VARCHAR(20) NOT NULL DEFAULT 'Finished'
        CONSTRAINT CK_Results_Status
        CHECK (ResultStatus IN ('Finished', 'DNF', 'DNS')),

    RecordedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Results_Enrolment
        FOREIGN KEY (EnrolmentId)
        REFERENCES Enrolments(EnrolmentId),

    CONSTRAINT CK_Results_Position
        CHECK (Position IS NULL OR Position > 0)
);
GO

-- =========================================================
-- SAMPLE USERS
-- 2 Organisers and 2 Participants as required.
-- =========================================================

INSERT INTO Users
    (FirstName, LastName, Email, PasswordHash, Role)
VALUES
    ('Nomsa', 'Mthembu', 'nomsa@raceday.co.za', 'HASH_NOMSA', 'Organiser'),
    ('Thabo', 'Jacobs', 'thabo@raceday.co.za', 'HASH_THABO', 'Organiser'),
    ('Sibusiso', 'Khumalo', 'sibusiso@example.com', 'HASH_SIBUSISO', 'Participant'),
    ('Lerato', 'Mokoena', 'lerato@example.com', 'HASH_LERATO', 'Participant');
GO

-- =========================================================
-- SAMPLE LOCATIONS
-- =========================================================

INSERT INTO EventLocations
    (LocationName, Address, City, Province, Latitude, Longitude)
VALUES
    ('Nelson Mandela Bay Stadium', '70 Prince Alfred Road', 'Gqeberha', 'Eastern Cape', -33.950000, 25.600000),
    ('Durban Moses Mabhida Stadium', '44 Isaiah Ntshangase Road', 'Durban', 'KwaZulu-Natal', -29.825000, 31.030000),
    ('Cape Town Stadium', 'Fritz Sonnenberg Road', 'Cape Town', 'Western Cape', -33.903000, 18.412000);
GO

-- =========================================================
-- SAMPLE EVENTS
-- 3 events as required.
-- =========================================================

INSERT INTO Events
    (OrganiserId, EventName, Description, EventDate, LocationId, Status)
VALUES
    (
        1,
        'Bay City Run',
        'A community road running event in Gqeberha.',
        '2026-10-18',
        1,
        'Open'
    ),
    (
        2,
        'Durban Coastal Challenge',
        'A scenic road running event along the Durban coast.',
        '2026-11-08',
        2,
        'Upcoming'
    ),
    (
        1,
        'Cape Town Cycle Classic',
        'A road cycling event around Cape Town.',
        '2026-12-06',
        3,
        'Upcoming'
    );
GO

-- =========================================================
-- SAMPLE CATEGORIES
-- Categories for each event.
-- =========================================================

INSERT INTO Categories
    (EventId, CategoryName, DistanceKm, EntryFee)
VALUES
    (1, '5KM Fun Run', 5.00, 100.00),
    (1, '10KM Road Run', 10.00, 180.00),
    (1, '21KM Half Marathon', 21.10, 300.00),

    (2, '10KM Coastal Run', 10.00, 200.00),
    (2, '21KM Half Marathon', 21.10, 350.00),

    (3, '40KM Cycle', 40.00, 250.00),
    (3, '80KM Cycle', 80.00, 400.00);
GO

-- =========================================================
-- SAMPLE ENROLMENTS
-- =========================================================

INSERT INTO Enrolments
    (ParticipantId, EventId, CategoryId, Status)
VALUES
    (3, 1, 2, 'Confirmed'),
    (4, 1, 3, 'Confirmed'),
    (3, 2, 4, 'Confirmed'),
    (4, 3, 6, 'Confirmed');
GO

-- =========================================================
-- SAMPLE RESULTS
-- =========================================================

INSERT INTO Results
    (EnrolmentId, FinishTime, Position, ResultStatus)
VALUES
    (1, '00:52:34', 18, 'Finished'),
    (2, '01:48:21', 7, 'Finished');
GO

-- =========================================================
-- Verification queries
-- =========================================================

SELECT * FROM Users;
SELECT * FROM EventLocations;
SELECT * FROM Events;
SELECT * FROM Categories;
SELECT * FROM Enrolments;
SELECT * FROM Results;
GO