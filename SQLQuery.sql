create database EventEaseDB;

use EventEaseDB;

-- Create Venue Table
CREATE TABLE Venue (
    VenueId INT IDENTITY(1,1) PRIMARY KEY,
    VenueName NVARCHAR(100) NOT NULL,
    Location NVARCHAR(200) NOT NULL,
    Capacity INT NOT NULL CHECK (Capacity > 0),
    ImageUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- Create Event Table
CREATE TABLE Event (
    EventId INT IDENTITY(1,1) PRIMARY KEY,
    EventName NVARCHAR(100) NOT NULL,
    EventDate DATETIME2 NOT NULL,
    Description NVARCHAR(500) NULL,
    VenueId INT NULL,
    ImageUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Event_Venue FOREIGN KEY (VenueId) 
        REFERENCES Venue(VenueId) ON DELETE SET NULL
);

-- Create Booking Table
CREATE TABLE Booking (
    BookingId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL,
    VenueId INT NOT NULL,
    BookingDate DATETIME2 NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Booking_Event FOREIGN KEY (EventId) 
        REFERENCES Event(EventId),
    CONSTRAINT FK_Booking_Venue FOREIGN KEY (VenueId) 
        REFERENCES Venue(VenueId),
    CONSTRAINT UQ_Booking_Venue_Date UNIQUE (VenueId, BookingDate)
);


-- Insert Sample Data
INSERT INTO Venue (VenueName, Location, Capacity, ImageUrl) VALUES
('Grand Ballroom', '123 Main St, City Center', 500, 'https://placehold.co/600x400'),
('Conference Hall A', '45 Business Park', 200, 'https://placehold.co/600x400'),
('Garden Pavilion', '789 Park Avenue', 150, 'https://placehold.co/600x400'),
('Executive Boardroom', '45 Business Park', 30, 'https://placehold.co/600x400');

INSERT INTO Event (EventName, EventDate, Description, VenueId, ImageUrl) VALUES
('Annual Tech Conference', '2026-06-15 09:00:00', 'Technology innovation showcase', 1, 'https://placehold.co/600x400'),
('Wedding Reception', '2026-07-20 18:00:00', 'Smith-Johnson wedding', 3, 'https://placehold.co/600x400'),
('Product Launch', '2026-05-10 14:00:00', 'New product reveal', 2, 'https://placehold.co/600x400'),
('Board Meeting', '2026-05-05 10:00:00', 'Quarterly review', 4, 'https://placehold.co/600x400');

INSERT INTO Booking (EventId, VenueId, BookingDate) VALUES
(1, 1, '2026-06-15'),
(2, 3, '2026-07-20'),
(3, 2, '2026-05-10'),
(4, 4, '2026-05-05');

CREATE VIEW vw_BookingDetails AS
SELECT 
    b.BookingId,
    e.EventName,
    e.EventDate,
    v.VenueName,
    v.Location,
    v.Capacity,
    b.BookingDate,
    e.Description AS EventDescription
FROM Booking b
INNER JOIN Event e ON b.EventId = e.EventId
INNER JOIN Venue v ON b.VenueId = v.VenueId;

select * from vw_BookingDetails;