/* ============================================================================
   REAL ESTATE INDUSTRY DATABASE
   Platform : Microsoft SQL Server
   Notes    : - No foreign key constraints (by design)
              - All business fields are NULLable
              - Standard audit columns on every table:
                  CreatedBy    INT NULL
                  CreatedDate  DATETIME NULL DEFAULT GETDATE()
                  ModifiedBy   INT NULL
                  ModifiedDate DATETIME NULL
   ============================================================================ */

IF DB_ID('RealEstateDB') IS NULL
BEGIN
    CREATE DATABASE RealEstateDB;
END
GO

USE RealEstateDB;
GO

/* ============================================================================
   DROP EXISTING TABLES (clean re-run)
   ============================================================================ */
IF OBJECT_ID('dbo.Inventory', 'U') IS NOT NULL DROP TABLE dbo.Inventory;
IF OBJECT_ID('dbo.UnitTypes', 'U') IS NOT NULL DROP TABLE dbo.UnitTypes;
IF OBJECT_ID('dbo.Amenities', 'U') IS NOT NULL DROP TABLE dbo.Amenities;
IF OBJECT_ID('dbo.Projects', 'U')  IS NOT NULL DROP TABLE dbo.Projects;
GO

/* ============================================================================
   TABLE: Projects
   Master table for real estate developments
   ============================================================================ */
CREATE TABLE dbo.Projects
(
    ProjectID INT IDENTITY(1,1) PRIMARY KEY,
    ProjectCode NVARCHAR(50) NULL,
    ProjectName NVARCHAR(200) NULL,
    Developer NVARCHAR(200) NULL,
    ProjectType NVARCHAR(50) NULL,          -- Residential / Commercial / Mixed-Use / Villa / Retail
    Status NVARCHAR(50) NULL,               -- Planning / Under Construction / Ready / Handover / Sold Out
    Description NVARCHAR(MAX) NULL,
    Country NVARCHAR(100) NULL,
    City NVARCHAR(100) NULL,
    District NVARCHAR(150) NULL,
    Address NVARCHAR(300) NULL,
    Latitude DECIMAL(9,6) NULL,
    Longitude DECIMAL(9,6) NULL,
    TotalBuildings INT NULL,
    TotalFloors INT NULL,
    TotalUnits INT NULL,
    LaunchDate DATE NULL,
    ConstructionStart DATE NULL,
    EstimatedCompletion DATE NULL,
    HandoverDate DATE NULL,
    StartingPrice DECIMAL(18,2) NULL,
    MaxPrice DECIMAL(18,2) NULL,
    Currency NVARCHAR(10) NULL,
    PaymentPlan NVARCHAR(500) NULL,
    PermitNumber NVARCHAR(100) NULL,        -- RERA / DLD permit etc.
    ServiceCharge DECIMAL(10,2) NULL,       -- per sq.ft / sq.m per year
    MasterPlanURL NVARCHAR(500) NULL,
    BrochureURL NVARCHAR(500) NULL,
    ImageURL NVARCHAR(500) NULL,
    VideoURL NVARCHAR(500) NULL,
    ContactPerson NVARCHAR(150) NULL,
    ContactPhone NVARCHAR(50) NULL,
    ContactEmail NVARCHAR(150) NULL,
    IsFeatured BIT NULL,
    IsActive BIT NULL,
    CreatedBy INT NULL,
    CreatedDate DATETIME NULL DEFAULT GETDATE(),
    ModifiedBy INT NULL,
    ModifiedDate DATETIME NULL
);
GO

/* ============================================================================
   TABLE: UnitTypes
   Defines the unit "floor plan" categories offered within a project
   (Studio, 1BR, 2BR, Penthouse, etc.)
   ProjectID is a plain INT reference to Projects.ProjectID - no FK constraint
   ============================================================================ */
CREATE TABLE dbo.UnitTypes
(
    UnitTypeID INT IDENTITY(1,1) PRIMARY KEY,
    ProjectID INT NULL,                     -- references Projects.ProjectID (no FK)
    TypeName NVARCHAR(100) NULL,            -- Studio / 1 Bedroom / 2 Bedroom / Penthouse / Townhouse
    Category NVARCHAR(50) NULL,             -- Apartment / Villa / Townhouse / Duplex / Penthouse
    Bedrooms INT NULL,
    Bathrooms DECIMAL(3,1) NULL,
    MinAreaSqFt DECIMAL(10,2) NULL,
    MaxAreaSqFt DECIMAL(10,2) NULL,
    BasePrice DECIMAL(18,2) NULL,
    PricePerSqFt DECIMAL(10,2) NULL,
    TotalUnitsOfType INT NULL,
    AvailableUnitsOfType INT NULL,
    FloorPlanURL NVARCHAR(500) NULL,
    Description NVARCHAR(1000) NULL,
    CreatedBy INT NULL,
    CreatedDate DATETIME NULL DEFAULT GETDATE(),
    ModifiedBy INT NULL,
    ModifiedDate DATETIME NULL
);
GO

/* ============================================================================
   TABLE: Inventory
   Individual sellable / leasable units - the actual saleable stock
   ProjectID / UnitTypeID are plain INT references - no FK constraints
   ============================================================================ */
CREATE TABLE dbo.Inventory
(
    InventoryID INT IDENTITY(1,1) PRIMARY KEY,
    ProjectID INT NULL,                     -- references Projects.ProjectID (no FK)
    UnitTypeID INT NULL,                    -- references UnitTypes.UnitTypeID (no FK)
    UnitNumber NVARCHAR(50) NULL,
    BuildingTower NVARCHAR(100) NULL,
    FloorNumber INT NULL,
    ViewType NVARCHAR(100) NULL,            -- Sea View / City View / Garden View / Pool View
    AreaSqFt DECIMAL(10,2) NULL,
    Bedrooms INT NULL,
    Bathrooms DECIMAL(3,1) NULL,
    ParkingSpaces INT NULL,
    HasBalcony BIT NULL,
    FurnishingStatus NVARCHAR(50) NULL,     -- Furnished / Semi-Furnished / Unfurnished
    ListPrice DECIMAL(18,2) NULL,
    PricePerSqFt DECIMAL(10,2) NULL,
    Status NVARCHAR(50) NULL,               -- Available / Reserved / Sold / Blocked / Leased
    ListingDate DATE NULL,
    SoldOrLeasedDate DATE NULL,
    BuyerTenantName NVARCHAR(150) NULL,
    AgentName NVARCHAR(150) NULL,
    AgentContact NVARCHAR(50) NULL,
    Notes NVARCHAR(1000) NULL,
    CreatedBy INT NULL,
    CreatedDate DATETIME NULL DEFAULT GETDATE(),
    ModifiedBy INT NULL,
    ModifiedDate DATETIME NULL
);
GO

/* ============================================================================
   TABLE: Amenities
   Amenities / facilities offered per project
   ProjectID is a plain INT reference to Projects.ProjectID - no FK constraint
   ============================================================================ */
CREATE TABLE dbo.Amenities
(
    AmenityID INT IDENTITY(1,1) PRIMARY KEY,
    ProjectID INT NULL,                     -- references Projects.ProjectID (no FK)
    AmenityName NVARCHAR(150) NULL,
    Category NVARCHAR(50) NULL,             -- Recreational / Security / Wellness / Convenience / Business
    Description NVARCHAR(1000) NULL,
    IconURL NVARCHAR(500) NULL,
    ImageURL NVARCHAR(500) NULL,
    IsHighlighted BIT NULL,
    DisplayOrder INT NULL,
    CreatedBy INT NULL,
    CreatedDate DATETIME NULL DEFAULT GETDATE(),
    ModifiedBy INT NULL,
    ModifiedDate DATETIME NULL
);
GO

/* ============================================================================
   SAMPLE DATA
   ============================================================================ */

/* ----------------------------------------------------------------------------
   1. PROJECTS  (5 sample projects)
   ---------------------------------------------------------------------------- */
INSERT INTO dbo.Projects
(ProjectCode, ProjectName, Developer, ProjectType, Status, Description, Country, City, District, Address,
 Latitude, Longitude, TotalBuildings, TotalFloors, TotalUnits, LaunchDate, ConstructionStart, EstimatedCompletion,
 HandoverDate, StartingPrice, MaxPrice, Currency, PaymentPlan, PermitNumber, ServiceCharge,
 MasterPlanURL, BrochureURL, ImageURL, VideoURL, ContactPerson, ContactPhone, ContactEmail,
 IsFeatured, IsActive, CreatedBy)
VALUES
('PRJ-001', 'Marina Vista Towers', 'Skyline Developers', 'Residential', 'Under Construction',
 'A twin-tower waterfront residential development offering studio to 3-bedroom apartments with panoramic marina views.',
 'UAE', 'Dubai', 'Dubai Marina', 'Marina Walk, Dubai Marina',
 25.076800, 55.130200, 2, 45, 480, '2023-02-01', '2023-06-01', '2027-03-31',
 '2027-06-30', 850000.00, 4500000.00, 'AED', '60/40 - 60% during construction, 40% on handover',
 'DLD-2023-00147', 18.50,
 'https://example.com/marina-vista/masterplan.pdf', 'https://example.com/marina-vista/brochure.pdf',
 'https://images.pexels.com/photos/8469931/pexels-photo-8469931.jpeg', 'https://example.com/marina-vista/video.mp4',
 'Ahmed Khalid', '+971-4-1234567', 'sales@marinavista-example.com', 1, 1, 1),

('PRJ-002', 'Palm Grove Residences', 'Coastal Living Group', 'Residential', 'Ready',
 'Low-rise garden apartments and townhouses set within landscaped courtyards, close to the beach.',
 'UAE', 'Dubai', 'Jumeirah Village Circle', 'District 12, JVC',
 25.058300, 55.209800, 6, 8, 260, '2021-05-15', '2021-09-01', '2024-12-31',
 '2025-01-15', 620000.00, 2100000.00, 'AED', '20% down payment, 80% on handover',
 'DLD-2021-00982', 12.00,
 'https://example.com/palm-grove/masterplan.pdf', 'https://example.com/palm-grove/brochure.pdf',
 'https://images.pexels.com/photos/221540/pexels-photo-221540.jpeg', NULL,
 'Sara Ibrahim', '+971-4-2345678', 'sales@palmgrove-example.com', 1, 1, 1),

('PRJ-003', 'Downtown Business Hub', 'Apex Commercial Properties', 'Commercial', 'Ready',
 'A Grade-A office tower with retail podium, located in the heart of the central business district.',
 'UAE', 'Dubai', 'Business Bay', 'Sheikh Zayed Road, Business Bay',
 25.187900, 55.264600, 1, 38, 210, '2019-01-10', '2019-05-01', '2023-08-31',
 '2023-09-30', 1200000.00, 9800000.00, 'AED', '100% on completion or bank finance',
 'DLD-2019-00345', 25.00,
 'https://example.com/downtown-hub/masterplan.pdf', 'https://example.com/downtown-hub/brochure.pdf',
 'https://images.pexels.com/photos/4469146/pexels-photo-4469146.jpeg', 'https://example.com/downtown-hub/video.mp4',
 'Omar Farouk', '+971-4-3456789', 'leasing@downtownhub-example.com', 0, 1, 1),

('PRJ-004', 'Emerald Hills Villas', 'GreenScape Developments', 'Villa', 'Planning',
 'Gated community of 4-6 bedroom villas surrounding a championship golf course.',
 'UAE', 'Dubai', 'Dubai Hills Estate', 'Golf Boulevard, Dubai Hills',
 25.106900, 55.244300, 0, 2, 120, '2026-01-01', '2026-06-01', '2029-12-31',
 '2030-03-31', 3200000.00, 15000000.00, 'AED', '10% booking, 40% construction-linked, 50% on handover',
 'DLD-2026-00021', 8.00,
 'https://example.com/emerald-hills/masterplan.pdf', 'https://example.com/emerald-hills/brochure.pdf',
 'https://images.pexels.com/photos/15369780/pexels-photo-15369780.jpeg', 'https://example.com/emerald-hills/video.mp4',
 'Layla Hassan', '+971-4-4567890', 'sales@emeraldhills-example.com', 1, 1, 1),

('PRJ-005', 'Riverside Mixed-Use Complex', 'Metro Urban Developers', 'Mixed-Use', 'Handover',
 'Integrated development combining residential apartments, retail outlets, and a boutique hotel along the riverfront.',
 'Pakistan', 'Islamabad', 'Blue Area', 'Jinnah Avenue, Blue Area',
 33.716600, 73.073600, 3, 22, 340, '2020-03-01', '2020-07-01', '2024-06-30',
 '2024-08-01', 25000000.00, 180000000.00, 'PKR', '30% down payment, 70% in 24 monthly installments',
 'CDA-2020-00567', 350.00,
 'https://example.com/riverside/masterplan.pdf', 'https://example.com/riverside/brochure.pdf',
 'https://images.pexels.com/photos/8031875/pexels-photo-8031875.jpeg', NULL,
 'Bilal Chaudhry', '+92-51-1234567', 'info@riverside-example.com', 0, 1, 1);
GO

/* ----------------------------------------------------------------------------
   2. UNIT TYPES  (per project)
   ---------------------------------------------------------------------------- */
INSERT INTO dbo.UnitTypes
(ProjectID, TypeName, Category, Bedrooms, Bathrooms, MinAreaSqFt, MaxAreaSqFt, BasePrice, PricePerSqFt,
 TotalUnitsOfType, AvailableUnitsOfType, FloorPlanURL, Description, CreatedBy)
VALUES
-- Marina Vista Towers (ProjectID 1)
(1, 'Studio', 'Apartment', 0, 1.0, 420.00, 480.00, 850000.00, 1950.00, 80, 32, 'https://example.com/mv/studio.pdf', 'Compact studio with marina view.', 1),
(1, '1 Bedroom', 'Apartment', 1, 1.5, 720.00, 800.00, 1350000.00, 1875.00, 140, 55, 'https://example.com/mv/1br.pdf', 'Open-plan 1 bedroom with balcony.', 1),
(1, '2 Bedroom', 'Apartment', 2, 2.5, 1100.00, 1250.00, 2100000.00, 1909.00, 160, 61, 'https://example.com/mv/2br.pdf', 'Spacious 2 bedroom with full marina view.', 1),
(1, '3 Bedroom', 'Apartment', 3, 3.5, 1600.00, 1850.00, 3400000.00, 2125.00, 100, 40, 'https://example.com/mv/3br.pdf', 'Premium 3 bedroom corner unit.', 1),

-- Palm Grove Residences (ProjectID 2)
(2, '1 Bedroom', 'Apartment', 1, 1.0, 650.00, 700.00, 620000.00, 953.00, 60, 12, 'https://example.com/pg/1br.pdf', 'Garden-facing 1 bedroom apartment.', 1),
(2, '2 Bedroom', 'Apartment', 2, 2.0, 980.00, 1050.00, 950000.00, 969.00, 100, 28, 'https://example.com/pg/2br.pdf', '2 bedroom apartment with courtyard access.', 1),
(2, '3 Bedroom Townhouse', 'Townhouse', 3, 3.0, 1800.00, 1950.00, 1850000.00, 1027.00, 100, 30, 'https://example.com/pg/th3.pdf', '3 bedroom townhouse with private garden.', 1),

-- Downtown Business Hub (ProjectID 3)
(3, 'Small Office', 'Office', NULL, 1.0, 500.00, 700.00, 1200000.00, 2000.00, 90, 20, 'https://example.com/dh/small-office.pdf', 'Fitted small office suite.', 1),
(3, 'Large Office Floor', 'Office', NULL, 2.0, 3000.00, 4200.00, 7200000.00, 2100.00, 30, 6, 'https://example.com/dh/large-office.pdf', 'Full-floor office space, shell & core.', 1),
(3, 'Retail Unit', 'Retail', NULL, 1.0, 350.00, 900.00, 1500000.00, 3200.00, 90, 18, 'https://example.com/dh/retail.pdf', 'Ground floor retail podium unit.', 1),

-- Emerald Hills Villas (ProjectID 4)
(4, '4 Bedroom Villa', 'Villa', 4, 4.5, 3800.00, 4200.00, 3200000.00, 800.00, 60, 60, 'https://example.com/eh/villa4.pdf', 'Golf-course facing 4 bedroom villa.', 1),
(4, '5 Bedroom Villa', 'Villa', 5, 5.5, 5200.00, 5600.00, 4800000.00, 900.00, 40, 40, 'https://example.com/eh/villa5.pdf', 'Corner 5 bedroom villa with private pool.', 1),
(4, '6 Bedroom Mansion', 'Villa', 6, 7.0, 7500.00, 8200.00, 8500000.00, 1050.00, 20, 20, 'https://example.com/eh/mansion6.pdf', 'Flagship 6 bedroom mansion plot.', 1),

-- Riverside Mixed-Use Complex (ProjectID 5)
(5, '1 Bedroom', 'Apartment', 1, 1.0, 600.00, 650.00, 25000000.00, 40000.00, 100, 15, 'https://example.com/rs/1br.pdf', 'Riverfront 1 bedroom apartment.', 1),
(5, '2 Bedroom', 'Apartment', 2, 2.0, 950.00, 1000.00, 38000000.00, 39500.00, 140, 22, 'https://example.com/rs/2br.pdf', 'Riverfront 2 bedroom apartment.', 1),
(5, 'Retail Shop', 'Retail', NULL, 1.0, 300.00, 800.00, 18000000.00, 45000.00, 60, 9, 'https://example.com/rs/retail.pdf', 'Ground floor retail unit facing the boulevard.', 1),
(5, 'Hotel Suite', 'Hotel', 1, 1.0, 400.00, 500.00, 22000000.00, 47000.00, 40, 5, 'https://example.com/rs/suite.pdf', 'Boutique hotel investment suite.', 1);
GO

/* ----------------------------------------------------------------------------
   3. INVENTORY  (sample individual units per project/unit type)
   ---------------------------------------------------------------------------- */
INSERT INTO dbo.Inventory
(ProjectID, UnitTypeID, UnitNumber, BuildingTower, FloorNumber, ViewType, AreaSqFt, Bedrooms, Bathrooms,
 ParkingSpaces, HasBalcony, FurnishingStatus, ListPrice, PricePerSqFt, Status, ListingDate, SoldOrLeasedDate,
 BuyerTenantName, AgentName, AgentContact, Notes, CreatedBy)
VALUES
-- Marina Vista Towers
(1, 1, 'MV-A-0501', 'Tower A', 5, 'Marina View', 455.00, 0, 1.0, 1, 1, 'Unfurnished', 887250.00, 1950.00, 'Available', '2024-01-10', NULL, NULL, 'Fatima Noor', '+971-50-1112233', 'Bright studio, high demand.', 1),
(1, 2, 'MV-A-1203', 'Tower A', 12, 'Marina View', 765.00, 1, 1.5, 1, 1, 'Semi-Furnished', 1434375.00, 1875.00, 'Reserved', '2024-01-15', NULL, 'Hassan Rahim', 'Fatima Noor', '+971-50-1112233', 'Reserved pending final payment.', 1),
(1, 3, 'MV-B-2004', 'Tower B', 20, 'Sea & Marina View', 1180.00, 2, 2.5, 2, 1, 'Unfurnished', 2252620.00, 1909.00, 'Sold', '2023-11-05', '2024-02-20', 'Mei Ling Tan', 'Karim Aziz', '+971-50-2223344', 'Sold to overseas investor.', 1),
(1, 4, 'MV-B-3001', 'Tower B', 30, 'Full Marina View', 1720.00, 3, 3.5, 2, 1, 'Furnished', 3655000.00, 2125.00, 'Available', '2024-02-01', NULL, NULL, 'Karim Aziz', '+971-50-2223344', 'Top-floor premium corner unit.', 1),

-- Palm Grove Residences
(2, 5, 'PG-C-102', 'Cluster C', 1, 'Courtyard View', 675.00, 1, 1.0, 1, 1, 'Unfurnished', 643275.00, 953.00, 'Sold', '2023-06-01', '2023-09-12', 'Yousef Al Amin', 'Nadia Saeed', '+971-50-3334455', NULL, 1),
(2, 6, 'PG-D-208', 'Cluster D', 2, 'Garden View', 1015.00, 2, 2.0, 1, 0, 'Unfurnished', 983535.00, 969.00, 'Available', '2024-01-20', NULL, NULL, 'Nadia Saeed', '+971-50-3334455', NULL, 1),
(2, 7, 'PG-TH-014', 'Townhouse Row 1', 0, 'Park View', 1875.00, 3, 3.0, 2, 0, 'Unfurnished', 1925625.00, 1027.00, 'Blocked', '2024-02-05', NULL, NULL, 'Nadia Saeed', '+971-50-3334455', 'Blocked for corporate client review.', 1),

-- Downtown Business Hub
(3, 8, 'DH-15-A', 'Main Tower', 15, 'City Skyline', 610.00, NULL, 1.0, 1, 0, 'Unfurnished', 1220000.00, 2000.00, 'Leased', '2023-09-01', '2023-10-01', 'BrightTech FZ LLC', 'Tariq Salman', '+971-50-4445566', 'Leased on 3-year contract.', 1),
(3, 9, 'DH-25-FULL', 'Main Tower', 25, 'City Skyline', 3600.00, NULL, 2.0, 8, 0, 'Shell & Core', 7560000.00, 2100.00, 'Available', '2023-12-01', NULL, NULL, 'Tariq Salman', '+971-50-4445566', 'Full floor available for fit-out.', 1),
(3, 10, 'DH-G-07', 'Retail Podium', 0, 'Street Front', 480.00, NULL, 1.0, 0, 0, 'Shell & Core', 1536000.00, 3200.00, 'Available', '2024-01-05', NULL, NULL, 'Tariq Salman', '+971-50-4445566', 'Corner retail unit, high footfall.', 1),

-- Emerald Hills Villas
(4, 11, 'EH-V-023', NULL, NULL, 'Golf Course View', 4050.00, 4, 4.5, 3, 0, 'Unfurnished', 3240000.00, 800.00, 'Available', '2024-03-01', NULL, NULL, 'Rania Fadel', '+971-50-5556677', 'Off-plan, under construction.', 1),
(4, 12, 'EH-V-045', NULL, NULL, 'Golf Course View', 5400.00, 5, 5.5, 3, 0, 'Unfurnished', 4860000.00, 900.00, 'Reserved', '2024-03-10', NULL, 'David O''Connor', 'Rania Fadel', '+971-50-5556677', 'Reserved with 10% booking fee.', 1),
(4, 13, 'EH-V-001', NULL, NULL, 'Panoramic Golf View', 7850.00, 6, 7.0, 4, 0, 'Unfurnished', 8842500.00, 1050.00, 'Available', '2024-03-15', NULL, NULL, 'Rania Fadel', '+971-50-5556677', 'Flagship plot, corner position.', 1),

-- Riverside Mixed-Use Complex
(5, 14, 'RS-A-0602', 'Tower A', 6, 'River View', 620.00, 1, 1.0, 1, 1, 'Unfurnished', 24800000.00, 40000.00, 'Sold', '2023-10-01', '2024-01-18', 'Ayesha Malik', 'Usman Tariq', '+92-300-1112233', NULL, 1),
(5, 15, 'RS-A-1105', 'Tower A', 11, 'River View', 975.00, 2, 2.0, 1, 1, 'Semi-Furnished', 38512500.00, 39500.00, 'Available', '2024-01-25', NULL, NULL, 'Usman Tariq', '+92-300-1112233', NULL, 1),
(5, 16, 'RS-G-11', 'Retail Podium', 0, 'Boulevard Front', 520.00, NULL, 1.0, 0, 0, 'Shell & Core', 23400000.00, 45000.00, 'Available', '2024-02-01', NULL, NULL, 'Usman Tariq', '+92-300-1112233', 'Prime boulevard-facing shop.', 1),
(5, 17, 'RS-H-302', 'Hotel Tower', 3, 'City View', 450.00, 1, 1.0, 0, 0, 'Furnished', 21150000.00, 47000.00, 'Available', '2024-02-10', NULL, NULL, 'Usman Tariq', '+92-300-1112233', 'Managed hotel suite with rental pool option.', 1);
GO

/* ----------------------------------------------------------------------------
   4. AMENITIES  (per project)
   ---------------------------------------------------------------------------- */
INSERT INTO dbo.Amenities
(ProjectID, AmenityName, Category, Description, IconURL, ImageURL, IsHighlighted, DisplayOrder, CreatedBy)
VALUES
-- Marina Vista Towers
(1, 'Infinity Swimming Pool', 'Recreational', 'Rooftop infinity pool overlooking the marina.', 'https://example.com/icons/pool.svg', 'https://example.com/img/mv-pool.jpg', 1, 1, 1),
(1, 'Fully Equipped Gym', 'Wellness', '24/7 gym with cardio and free-weight zones.', 'https://example.com/icons/gym.svg', 'https://example.com/img/mv-gym.jpg', 1, 2, 1),
(1, '24/7 Security & CCTV', 'Security', 'Round-the-clock security personnel and CCTV monitoring.', 'https://example.com/icons/security.svg', NULL, 0, 3, 1),
(1, 'Kids Play Area', 'Recreational', 'Dedicated indoor and outdoor play area for children.', 'https://example.com/icons/kids.svg', NULL, 0, 4, 1),
(1, 'Covered Parking', 'Convenience', 'Allocated covered parking bays for residents.', 'https://example.com/icons/parking.svg', NULL, 0, 5, 1),

-- Palm Grove Residences
(2, 'Landscaped Courtyard', 'Recreational', 'Central landscaped courtyard with seating areas.', 'https://example.com/icons/garden.svg', 'https://example.com/img/pg-courtyard.jpg', 1, 1, 1),
(2, 'Community Pool', 'Recreational', 'Shared community swimming pool.', 'https://example.com/icons/pool.svg', NULL, 1, 2, 1),
(2, 'Jogging Track', 'Wellness', '1.2 km jogging and cycling track around the community.', 'https://example.com/icons/jog.svg', NULL, 0, 3, 1),
(2, 'Retail Plaza', 'Convenience', 'On-site retail plaza with supermarket and cafes.', 'https://example.com/icons/retail.svg', NULL, 0, 4, 1),

-- Downtown Business Hub
(3, 'High-Speed Elevators', 'Convenience', 'Destination-controlled high-speed elevator system.', 'https://example.com/icons/elevator.svg', NULL, 0, 1, 1),
(3, 'Conference & Business Center', 'Business', 'Shared conference rooms and business center facilities.', 'https://example.com/icons/business.svg', 'https://example.com/img/dh-conference.jpg', 1, 2, 1),
(3, 'Basement Parking', 'Convenience', 'Multi-level basement parking for tenants and visitors.', 'https://example.com/icons/parking.svg', NULL, 0, 3, 1),
(3, 'Food Court', 'Convenience', 'Ground floor food court with multiple dining options.', 'https://example.com/icons/food.svg', NULL, 0, 4, 1),

-- Emerald Hills Villas
(4, 'Championship Golf Course', 'Recreational', '18-hole championship golf course within the community.', 'https://example.com/icons/golf.svg', 'https://example.com/img/eh-golf.jpg', 1, 1, 1),
(4, 'Private Villa Pools', 'Recreational', 'Option for private swimming pool per villa.', 'https://example.com/icons/pool.svg', NULL, 1, 2, 1),
(4, 'Equestrian Club', 'Recreational', 'On-site equestrian club and riding trails.', 'https://example.com/icons/horse.svg', NULL, 0, 3, 1),
(4, 'Gated Community with Security', 'Security', '24/7 gated access with dedicated security patrols.', 'https://example.com/icons/security.svg', NULL, 0, 4, 1),
(4, 'Clubhouse', 'Recreational', 'Community clubhouse with dining and event spaces.', 'https://example.com/icons/clubhouse.svg', NULL, 0, 5, 1),

-- Riverside Mixed-Use Complex
(5, 'Riverside Promenade', 'Recreational', 'Landscaped promenade along the riverfront.', 'https://example.com/icons/promenade.svg', 'https://example.com/img/rs-promenade.jpg', 1, 1, 1),
(5, 'Boutique Hotel & Spa', 'Wellness', 'On-site boutique hotel with spa and wellness center.', 'https://example.com/icons/spa.svg', NULL, 1, 2, 1),
(5, 'Cinema & Entertainment Zone', 'Recreational', 'Multiplex cinema and family entertainment zone.', 'https://example.com/icons/cinema.svg', NULL, 0, 3, 1),
(5, 'Multi-Level Parking', 'Convenience', 'Dedicated multi-level parking for residents, retail, and hotel guests.', 'https://example.com/icons/parking.svg', NULL, 0, 4, 1);
GO

/* ============================================================================
   QUICK VERIFICATION QUERIES (optional - comment out if not needed)
   ============================================================================ */
-- SELECT * FROM dbo.Projects;
-- SELECT * FROM dbo.UnitTypes;
-- SELECT * FROM dbo.Inventory;
-- SELECT * FROM dbo.Amenities;

PRINT 'RealEstateDB schema and sample data created successfully.';
GO
