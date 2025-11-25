# Requirements Document

## Introduction

This document specifies the requirements for an ATS (American Truck Simulator) Performance Tracking System that provides real-time driving performance metrics, historical trip analysis, and visual analytics. The system integrates with existing telemetry infrastructure to calculate and display driving scores, track trip history in a SQLite database, and present performance trends through charts and graphs.

## Glossary

- **Performance Tracking System**: The software component that calculates, displays, and persists driving performance metrics
- **Telemetry Client**: The existing HTTP-based service that retrieves real-time game data from the Funbit telemetry server
- **Smoothness Score**: A percentage metric (0-100%) measuring driving smoothness based on acceleration and braking patterns
- **Fuel Efficiency Score**: A metric measuring miles per gallon (MPG) compared to a 6 MPG baseline
- **Speed Compliance Score**: A percentage metric (0-100%) measuring adherence to speed limits
- **Damage-Free Streak**: A time duration tracking continuous driving without vehicle damage
- **Overall Performance Grade**: A letter grade (A+ to F) representing the average of all performance scores
- **Trip**: A continuous driving session with defined start and end points, persisted in the database
- **Trip Repository**: The data access component managing SQLite database operations for trip records
- **Performance Calculator**: The service component that processes telemetry data and computes performance metrics

## Requirements

### Requirement 1: Real-Time Smoothness Score Calculation

**User Story:** As a truck driver, I want to see my driving smoothness score in real-time, so that I can improve my acceleration and braking technique.

#### Acceptance Criteria

1. WHEN the Performance Tracking System receives telemetry data, THE Performance Calculator SHALL compute the acceleration change by comparing current speed to previous speed
2. IF the acceleration change exceeds 5 MPH per second, THEN THE Performance Calculator SHALL subtract 2 points from the Smoothness Score
3. IF the deceleration exceeds 10 MPH per second, THEN THE Performance Calculator SHALL subtract 5 points from the Smoothness Score
4. THE Performance Tracking System SHALL initialize the Smoothness Score at 100 percent at the start of each calculation cycle
5. THE Performance Tracking System SHALL update the Smoothness Score every second during active driving

### Requirement 2: Real-Time Fuel Efficiency Tracking

**User Story:** As a truck driver, I want to monitor my fuel efficiency in real-time, so that I can optimize my driving for better fuel economy.

#### Acceptance Criteria

1. THE Performance Calculator SHALL compute fuel efficiency by dividing distance traveled in miles by fuel consumed in gallons
2. THE Performance Calculator SHALL calculate the efficiency score as the ratio of current MPG to the 6 MPG baseline multiplied by 100
3. THE Performance Tracking System SHALL display the current MPG value with one decimal precision
4. THE Performance Tracking System SHALL display the session average MPG with one decimal precision
5. THE Performance Tracking System SHALL display the percentage difference from the 6 MPG baseline

### Requirement 3: Real-Time Speed Compliance Monitoring

**User Story:** As a truck driver, I want to track my speed limit compliance, so that I can maintain safe and legal driving speeds.

#### Acceptance Criteria

1. THE Performance Calculator SHALL apply a speed limit of 65 MPH for highway driving conditions
2. THE Performance Calculator SHALL apply a speed limit of 55 MPH for non-highway driving conditions
3. THE Performance Calculator SHALL compute speed compliance as the percentage of time the vehicle speed is at or below the applicable speed limit
4. THE Performance Tracking System SHALL display the speed compliance percentage with zero decimal precision
5. THE Performance Tracking System SHALL convert the speed compliance percentage to a letter grade using the standard grading scale

### Requirement 4: Damage-Free Streak Tracking

**User Story:** As a truck driver, I want to see how long I've driven without damage, so that I can maintain awareness of my safe driving record.

#### Acceptance Criteria

1. THE Performance Tracking System SHALL initialize the Damage-Free Streak to zero at application start
2. WHEN the Performance Calculator detects an increase in vehicle damage, THE Performance Tracking System SHALL reset the Damage-Free Streak to zero
3. WHILE the vehicle damage value remains constant, THE Performance Tracking System SHALL increment the Damage-Free Streak by elapsed time
4. THE Performance Tracking System SHALL display the Damage-Free Streak in hours and minutes format (HH:MM)

### Requirement 5: Overall Performance Grade Calculation

**User Story:** As a truck driver, I want to see an overall performance grade, so that I can quickly assess my driving quality.

#### Acceptance Criteria

1. THE Performance Calculator SHALL compute the Overall Performance Grade by averaging the Smoothness Score, Fuel Efficiency Score, and Speed Compliance Score
2. THE Performance Calculator SHALL convert scores of 95 to 100 percent to grade A+
3. THE Performance Calculator SHALL convert scores of 90 to 94 percent to grade A
4. THE Performance Calculator SHALL convert scores of 85 to 89 percent to grade B+
5. THE Performance Calculator SHALL convert scores below 60 percent to grade F
6. THE Performance Tracking System SHALL display a trend indicator (up arrow, down arrow, or right arrow) comparing current grade to session average

### Requirement 6: Trip Detection and Persistence

**User Story:** As a truck driver, I want my trips to be automatically saved, so that I can review my driving history later.

#### Acceptance Criteria

1. WHEN the vehicle remains stationary for more than 5 minutes, THE Performance Tracking System SHALL mark the current trip as ended
2. WHEN the application closes, THE Performance Tracking System SHALL save the current trip to the database
3. THE Trip Repository SHALL persist trip records with start timestamp, end timestamp, duration in minutes, and distance in miles
4. THE Trip Repository SHALL persist performance metrics including Smoothness Score, Fuel Efficiency MPG, Speed Compliance Percent, and Overall Grade
5. THE Trip Repository SHALL persist additional metrics including average speed and fuel consumed

### Requirement 7: Trip History Display

**User Story:** As a truck driver, I want to view my recent trips, so that I can track my performance over time.

#### Acceptance Criteria

1. THE Performance Tracking System SHALL display the 20 most recent trips in chronological order with newest first
2. THE Performance Tracking System SHALL display trip number, timestamp, duration, distance, MPG, and grade for each trip
3. WHERE the user selects a date range filter, THE Performance Tracking System SHALL display only trips within the specified date range
4. THE Performance Tracking System SHALL calculate and display total number of trips in the database
5. THE Performance Tracking System SHALL calculate and display average grade across all trips

### Requirement 8: Performance Trend Visualization

**User Story:** As a truck driver, I want to see performance trends in a graph, so that I can visualize my improvement over time.

#### Acceptance Criteria

1. THE Performance Tracking System SHALL display a line graph showing Overall Performance Grade for the last 10 trips
2. THE Performance Tracking System SHALL display a bar chart comparing current session metrics to personal averages
3. THE Performance Tracking System SHALL display percentage change indicators for each metric compared to historical average
4. THE Performance Tracking System SHALL update trend visualizations when new trip data is persisted

### Requirement 9: User Interface Layout and Styling

**User Story:** As a truck driver, I want a clean and professional interface, so that I can easily read my performance metrics while driving.

#### Acceptance Criteria

1. THE Performance Tracking System SHALL display current session metrics in card-style containers with white background and subtle shadows
2. THE Performance Tracking System SHALL apply green color (#27AE60) to grades A+ and A
3. THE Performance Tracking System SHALL apply blue color (#3498DB) to grades B+ and B
4. THE Performance Tracking System SHALL apply yellow color (#F39C12) to grade C
5. THE Performance Tracking System SHALL apply red color (#E74C3C) to grades D and F
6. THE Performance Tracking System SHALL display connection status and current speed in the status bar

### Requirement 10: Data Export Capability

**User Story:** As a truck driver, I want to export my trip data to CSV, so that I can analyze it in external tools.

#### Acceptance Criteria

1. WHERE the user selects the export function, THE Performance Tracking System SHALL generate a CSV file containing all trip records
2. THE Performance Tracking System SHALL include column headers for all trip attributes in the CSV file
3. THE Performance Tracking System SHALL format timestamps in ISO 8601 format in the CSV file
4. THE Performance Tracking System SHALL save the CSV file to a user-specified location
