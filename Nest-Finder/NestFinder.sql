-- SQLite Schema for NestFinderDb

-- 1. Users
CREATE TABLE Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FullName TEXT NOT NULL,
    Email TEXT NOT NULL UNIQUE,
    Phone TEXT NOT NULL,
    Username TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    Role TEXT NOT NULL CHECK (Role IN ('Admin', 'Customer')),
    IsActive INTEGER NOT NULL DEFAULT 1,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now', 'localtime'))
);

-- 2. Properties
CREATE TABLE Properties (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    City TEXT NOT NULL DEFAULT '',
    Type TEXT NOT NULL CHECK (Type IN ('Residential', 'Commercial')),
    Price REAL NOT NULL,
    Status TEXT NOT NULL DEFAULT 'Available' CHECK (Status IN ('Available', 'Sold', 'Rented', 'Deleted')),
    Bedrooms INTEGER NOT NULL DEFAULT 0,
    Bathrooms INTEGER NOT NULL DEFAULT 0,
    AreaSqFt INTEGER NOT NULL DEFAULT 0,
    Description TEXT NULL,
    UserId INTEGER NULL REFERENCES Users(Id),
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now', 'localtime'))
);

-- 3. Transactions
CREATE TABLE Transactions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PropertyId INTEGER NOT NULL REFERENCES Properties(Id),
    CustomerId INTEGER NOT NULL REFERENCES Users(Id),
    Type TEXT NOT NULL CHECK (Type IN ('Buy', 'Rent')),
    Amount REAL NOT NULL,
    PaymentMethod TEXT NULL DEFAULT 'Cash',
    Notes TEXT NULL,
    Date TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
    Status TEXT NOT NULL DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Completed', 'Cancelled')),
    PaymentStatus TEXT NOT NULL DEFAULT 'Unpaid'
);

-- 4. PropertyImages
CREATE TABLE PropertyImages (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PropertyId INTEGER NOT NULL REFERENCES Properties(Id) ON DELETE CASCADE,
    ImagePath TEXT NOT NULL
);

-- 5. Favorites
CREATE TABLE Favorites (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NOT NULL REFERENCES Users(Id) ON DELETE CASCADE,
    PropertyId INTEGER NOT NULL REFERENCES Properties(Id) ON DELETE CASCADE,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
    CONSTRAINT UQ_Favorites UNIQUE (UserId, PropertyId)
);

-- 6. Reviews
CREATE TABLE Reviews (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PropertyId INTEGER NOT NULL REFERENCES Properties(Id) ON DELETE CASCADE,
    UserId INTEGER NOT NULL REFERENCES Users(Id),
    Rating INTEGER NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Comment TEXT NULL,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now', 'localtime'))
);

-- 7. Viewings
CREATE TABLE Viewings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    PropertyId INTEGER NOT NULL REFERENCES Properties(Id) ON DELETE CASCADE,
    CustomerId INTEGER NOT NULL REFERENCES Users(Id),
    ViewingTime TEXT NOT NULL,
    Status TEXT NOT NULL DEFAULT 'Scheduled' CHECK (Status IN ('Scheduled', 'Completed', 'Cancelled'))
);

-- SEED DATA
-- admin123 -> 240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9
-- password123 -> ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f

INSERT INTO Users (FullName, Email, Phone, Username, PasswordHash, Role) VALUES
('System Admin',   'admin@nestfinder.com',   '+92-300-1234567', 'admin',    '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'Admin'),
('John Doe',       'john.doe@example.com',   '+92-321-9876543', 'customer', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 'Customer'),
('Jane Smith',     'jane.smith@example.com', '+92-333-5556667', 'john_doe', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 'Customer');

INSERT INTO Properties (Title, City, Type, Price, Status, Bedrooms, Bathrooms, AreaSqFt, Description, UserId) VALUES
('Luxury Beachfront Villa',       'Karachi',    'Residential', 12500000.00, 'Available', 5, 4, 4500, 'Stunning ocean-view villa with private beach access, modern interiors, and lush gardens.', 1),
('Modern City Loft Apartment',    'Lahore',     'Residential', 2800000.00,  'Available', 2, 1, 1200, 'Sleek urban loft in the heart of the city with rooftop terrace and smart home features.', 1),
('Premium Downtown Office Center','Islamabad',  'Commercial',  7500000.00,  'Available', 0, 2, 3200, 'Grade-A office space in prime business district with conference facilities and parking.', 1),
('Spacious Suburban Family Home', 'Rawalpindi', 'Residential', 4200000.00,  'Available', 4, 3, 2800, 'Family-friendly home with large backyard, modern kitchen, and quiet neighborhood.', 1),
('Busy Corner Retail Shop',       'Faisalabad', 'Commercial',  6500000.00,  'Available', 0, 1, 800,  'Prime retail location on busy intersection with high foot traffic and ample frontage.', 1),
('Vintage Victorian Townhouse',   'Multan',     'Residential', 8900000.00,  'Sold',      3, 2, 2200, 'Charming period property with original features, updated amenities, and courtyard.', 1);

INSERT INTO Transactions (PropertyId, CustomerId, Type, Amount, PaymentMethod, Status, PaymentStatus) VALUES
(6, 2, 'Buy',  8900000.00, 'Bank Transfer', 'Completed', 'Paid'),
(2, 3, 'Rent', 45000.00,   'Cash',          'Pending', 'Unpaid');

INSERT INTO Viewings (PropertyId, CustomerId, ViewingTime, Status) VALUES
(1, 2, datetime('now', 'localtime', '+48 hours'),  'Scheduled'),
(3, 3, datetime('now', 'localtime', '+72 hours'),  'Scheduled'),
(4, 2, datetime('now', 'localtime', '+96 hours'),  'Scheduled');

INSERT INTO Reviews (PropertyId, UserId, Rating, Comment) VALUES
(6, 2, 5, 'Absolutely beautiful property! The Victorian architecture is stunning and the modern updates are tasteful.'),
(1, 3, 4, 'Great location and amazing views. Would love to see more photos of the interior.');
