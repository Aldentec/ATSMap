-- ATS Performance Tracking Database Schema
-- SQLite database for storing trip history and performance metrics

-- Trips table: stores completed driving sessions with performance data
CREATE TABLE IF NOT EXISTS Trips (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    StartTime TEXT NOT NULL,
    EndTime TEXT NOT NULL,
    DurationMinutes REAL NOT NULL,
    DistanceMiles REAL NOT NULL,
    SmoothnessScore REAL NOT NULL,
    FuelEfficiencyMPG REAL NOT NULL,
    SpeedCompliancePercent REAL NOT NULL,
    SafetyScore REAL NOT NULL DEFAULT 100.0,
    OverallGrade TEXT NOT NULL,
    AverageSpeed REAL NOT NULL,
    FuelConsumed REAL NOT NULL
);

-- Index on StartTime for efficient date range queries and sorting
CREATE INDEX IF NOT EXISTS idx_trips_starttime ON Trips(StartTime DESC);

-- Index on OverallGrade for grade-based filtering
CREATE INDEX IF NOT EXISTS idx_trips_grade ON Trips(OverallGrade);
