-- PostgreSQL Database Initialization Script

-- Create Owners table
CREATE TABLE Owners (
    ID SERIAL PRIMARY KEY,
    Nick VARCHAR(255) UNIQUE NOT NULL
);

-- Create MapData table
CREATE TABLE MapData (
    MapID SERIAL PRIMARY KEY,
    MapName VARCHAR(255) UNIQUE NOT NULL,
    FileName VARCHAR(255) UNIQUE NOT NULL,
    OwnerID INTEGER NOT NULL REFERENCES Owners(ID) ON DELETE CASCADE,
    Publicity BOOLEAN DEFAULT FALSE
);

-- Create a View for easier MapEntity mapping
CREATE VIEW MapsWithOwners AS
SELECT 
    m.MapID,
    m.MapName,
    m.FileName,
    m.Publicity,
    o.ID AS OwnerID,
    o.Nick AS OwnerNick
FROM MapData m
JOIN Owners o ON m.OwnerID = o.ID;
