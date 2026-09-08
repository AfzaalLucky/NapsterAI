using Microsoft.EntityFrameworkCore;
using NapsterAI.Api.Models.Entities.RealEstate;
using NapsterAI.Api.Services.RealEstate;

namespace NapsterAI.Api.Data;

/// <summary>
/// Development-only idempotent seed, ported from the sample INSERT statements in
/// database\RealEstateDB.sql (5 projects / 17 unit types / 17 inventory rows / 21 amenities)
/// plus the Lookups reference data those enum-like columns exercise. Only runs when the
/// database is empty, so it is safe to call on every startup.
/// </summary>
public static class RealEstateDbInitializer
{
    public static async Task SeedAsync(RealEstateDbContext context)
    {
        // Seeded independently of the rest, so a database that already has Projects (from a
        // Phase 1/2 run before Users existed) still gets its dev login accounts on next startup.
        if (!await context.Users.AnyAsync())
        {
            SeedUsers(context);
            await context.SaveChangesAsync();
        }

        if (await context.Projects.AnyAsync())
        {
            return;
        }

        SeedLookups(context);

        var marinaVista = new Project
        {
            ProjectCode = "PRJ-001", ProjectName = "Marina Vista Towers", Developer = "Skyline Developers",
            ProjectType = "Residential", Status = "Under Construction",
            Description = "A twin-tower waterfront residential development offering studio to 3-bedroom apartments with panoramic marina views.",
            Country = "UAE", City = "Dubai", District = "Dubai Marina", Address = "Marina Walk, Dubai Marina",
            Latitude = 25.076800m, Longitude = 55.130200m,
            TotalBuildings = 2, TotalFloors = 45, TotalUnits = 480,
            LaunchDate = new DateOnly(2023, 2, 1), ConstructionStart = new DateOnly(2023, 6, 1),
            EstimatedCompletion = new DateOnly(2027, 3, 31), HandoverDate = new DateOnly(2027, 6, 30),
            StartingPrice = 850000.00m, MaxPrice = 4500000.00m, Currency = "AED",
            PaymentPlan = "60/40 - 60% during construction, 40% on handover",
            PermitNumber = "DLD-2023-00147", ServiceCharge = 18.50m,
            MasterPlanUrl = "https://example.com/marina-vista/masterplan.pdf",
            BrochureUrl = "https://example.com/marina-vista/brochure.pdf",
            ImageUrl = "https://images.pexels.com/photos/8482510/pexels-photo-8482510.jpeg",
            VideoUrl = "https://example.com/marina-vista/video.mp4",
            ContactPerson = "Ahmed Khalid", ContactPhone = "+971-4-1234567", ContactEmail = "sales@marinavista-example.com",
            IsFeatured = true, IsActive = true, ApprovalStatus = RealEstateApprovalStatuses.Approved, CreatedBy = 1
        };
        var palmGrove = new Project
        {
            ProjectCode = "PRJ-002", ProjectName = "Palm Grove Residences", Developer = "Coastal Living Group",
            ProjectType = "Residential", Status = "Ready",
            Description = "Low-rise garden apartments and townhouses set within landscaped courtyards, close to the beach.",
            Country = "UAE", City = "Dubai", District = "Jumeirah Village Circle", Address = "District 12, JVC",
            Latitude = 25.058300m, Longitude = 55.209800m,
            TotalBuildings = 6, TotalFloors = 8, TotalUnits = 260,
            LaunchDate = new DateOnly(2021, 5, 15), ConstructionStart = new DateOnly(2021, 9, 1),
            EstimatedCompletion = new DateOnly(2024, 12, 31), HandoverDate = new DateOnly(2025, 1, 15),
            StartingPrice = 620000.00m, MaxPrice = 2100000.00m, Currency = "AED",
            PaymentPlan = "20% down payment, 80% on handover",
            PermitNumber = "DLD-2021-00982", ServiceCharge = 12.00m,
            MasterPlanUrl = "https://example.com/palm-grove/masterplan.pdf",
            BrochureUrl = "https://example.com/palm-grove/brochure.pdf",
            ImageUrl = "https://images.pexels.com/photos/221540/pexels-photo-221540.jpeg",
            ContactPerson = "Sara Ibrahim", ContactPhone = "+971-4-2345678", ContactEmail = "sales@palmgrove-example.com",
            IsFeatured = true, IsActive = true, ApprovalStatus = RealEstateApprovalStatuses.Approved, CreatedBy = 1
        };
        var downtownHub = new Project
        {
            ProjectCode = "PRJ-003", ProjectName = "Downtown Business Hub", Developer = "Apex Commercial Properties",
            ProjectType = "Commercial", Status = "Ready",
            Description = "A Grade-A office tower with retail podium, located in the heart of the central business district.",
            Country = "UAE", City = "Dubai", District = "Business Bay", Address = "Sheikh Zayed Road, Business Bay",
            Latitude = 25.187900m, Longitude = 55.264600m,
            TotalBuildings = 1, TotalFloors = 38, TotalUnits = 210,
            LaunchDate = new DateOnly(2019, 1, 10), ConstructionStart = new DateOnly(2019, 5, 1),
            EstimatedCompletion = new DateOnly(2023, 8, 31), HandoverDate = new DateOnly(2023, 9, 30),
            StartingPrice = 1200000.00m, MaxPrice = 9800000.00m, Currency = "AED",
            PaymentPlan = "100% on completion or bank finance",
            PermitNumber = "DLD-2019-00345", ServiceCharge = 25.00m,
            MasterPlanUrl = "https://example.com/downtown-hub/masterplan.pdf",
            BrochureUrl = "https://example.com/downtown-hub/brochure.pdf",
            ImageUrl = "https://images.pexels.com/photos/4469146/pexels-photo-4469146.jpeg",
            VideoUrl = "https://example.com/downtown-hub/video.mp4",
            ContactPerson = "Omar Farouk", ContactPhone = "+971-4-3456789", ContactEmail = "leasing@downtownhub-example.com",
            IsFeatured = false, IsActive = true, ApprovalStatus = RealEstateApprovalStatuses.Approved, CreatedBy = 1
        };
        var emeraldHills = new Project
        {
            ProjectCode = "PRJ-004", ProjectName = "Emerald Hills Villas", Developer = "GreenScape Developments",
            ProjectType = "Villa", Status = "Planning",
            Description = "Gated community of 4-6 bedroom villas surrounding a championship golf course.",
            Country = "UAE", City = "Dubai", District = "Dubai Hills Estate", Address = "Golf Boulevard, Dubai Hills",
            Latitude = 25.106900m, Longitude = 55.244300m,
            TotalBuildings = 0, TotalFloors = 2, TotalUnits = 120,
            LaunchDate = new DateOnly(2026, 1, 1), ConstructionStart = new DateOnly(2026, 6, 1),
            EstimatedCompletion = new DateOnly(2029, 12, 31), HandoverDate = new DateOnly(2030, 3, 31),
            StartingPrice = 3200000.00m, MaxPrice = 15000000.00m, Currency = "AED",
            PaymentPlan = "10% booking, 40% construction-linked, 50% on handover",
            PermitNumber = "DLD-2026-00021", ServiceCharge = 8.00m,
            MasterPlanUrl = "https://example.com/emerald-hills/masterplan.pdf",
            BrochureUrl = "https://example.com/emerald-hills/brochure.pdf",
            ImageUrl = "https://images.pexels.com/photos/15369780/pexels-photo-15369780.jpeg",
            VideoUrl = "https://example.com/emerald-hills/video.mp4",
            ContactPerson = "Layla Hassan", ContactPhone = "+971-4-4567890", ContactEmail = "sales@emeraldhills-example.com",
            IsFeatured = true, IsActive = true, ApprovalStatus = RealEstateApprovalStatuses.PendingReview, CreatedBy = 1
        };
        var riverside = new Project
        {
            ProjectCode = "PRJ-005", ProjectName = "Riverside Mixed-Use Complex", Developer = "Metro Urban Developers",
            ProjectType = "Mixed-Use", Status = "Handover",
            Description = "Integrated development combining residential apartments, retail outlets, and a boutique hotel along the riverfront.",
            Country = "Pakistan", City = "Islamabad", District = "Blue Area", Address = "Jinnah Avenue, Blue Area",
            Latitude = 33.716600m, Longitude = 73.073600m,
            TotalBuildings = 3, TotalFloors = 22, TotalUnits = 340,
            LaunchDate = new DateOnly(2020, 3, 1), ConstructionStart = new DateOnly(2020, 7, 1),
            EstimatedCompletion = new DateOnly(2024, 6, 30), HandoverDate = new DateOnly(2024, 8, 1),
            StartingPrice = 25000000.00m, MaxPrice = 180000000.00m, Currency = "PKR",
            PaymentPlan = "30% down payment, 70% in 24 monthly installments",
            PermitNumber = "CDA-2020-00567", ServiceCharge = 350.00m,
            MasterPlanUrl = "https://example.com/riverside/masterplan.pdf",
            BrochureUrl = "https://example.com/riverside/brochure.pdf",
            ImageUrl = "https://images.pexels.com/photos/8031875/pexels-photo-8031875.jpeg",
            ContactPerson = "Bilal Chaudhry", ContactPhone = "+92-51-1234567", ContactEmail = "info@riverside-example.com",
            IsFeatured = false, IsActive = true, ApprovalStatus = RealEstateApprovalStatuses.Approved, CreatedBy = 1
        };

        context.Projects.AddRange(marinaVista, palmGrove, downtownHub, emeraldHills, riverside);
        await context.SaveChangesAsync();

        context.Locations.AddRange(
            new Location { Country = "UAE", City = "Dubai", District = "Dubai Marina", Latitude = 25.076800m, Longitude = 55.130200m },
            new Location { Country = "UAE", City = "Dubai", District = "Jumeirah Village Circle", Latitude = 25.058300m, Longitude = 55.209800m },
            new Location { Country = "UAE", City = "Dubai", District = "Business Bay", Latitude = 25.187900m, Longitude = 55.264600m },
            new Location { Country = "UAE", City = "Dubai", District = "Dubai Hills Estate", Latitude = 25.106900m, Longitude = 55.244300m },
            new Location { Country = "Pakistan", City = "Islamabad", District = "Blue Area", Latitude = 33.716600m, Longitude = 73.073600m });

        // Marina Vista Towers unit types
        var mvStudio = new UnitType { ProjectId = marinaVista.ProjectId, TypeName = "Studio", Category = "Apartment", Bedrooms = 0, Bathrooms = 1.0m, MinAreaSqFt = 420.00m, MaxAreaSqFt = 480.00m, BasePrice = 850000.00m, PricePerSqFt = 1950.00m, TotalUnitsOfType = 80, AvailableUnitsOfType = 32, FloorPlanUrl = "https://example.com/mv/studio.pdf", Description = "Compact studio with marina view.", CreatedBy = 1 };
        var mv1Br = new UnitType { ProjectId = marinaVista.ProjectId, TypeName = "1 Bedroom", Category = "Apartment", Bedrooms = 1, Bathrooms = 1.5m, MinAreaSqFt = 720.00m, MaxAreaSqFt = 800.00m, BasePrice = 1350000.00m, PricePerSqFt = 1875.00m, TotalUnitsOfType = 140, AvailableUnitsOfType = 55, FloorPlanUrl = "https://example.com/mv/1br.pdf", Description = "Open-plan 1 bedroom with balcony.", CreatedBy = 1 };
        var mv2Br = new UnitType { ProjectId = marinaVista.ProjectId, TypeName = "2 Bedroom", Category = "Apartment", Bedrooms = 2, Bathrooms = 2.5m, MinAreaSqFt = 1100.00m, MaxAreaSqFt = 1250.00m, BasePrice = 2100000.00m, PricePerSqFt = 1909.00m, TotalUnitsOfType = 160, AvailableUnitsOfType = 61, FloorPlanUrl = "https://example.com/mv/2br.pdf", Description = "Spacious 2 bedroom with full marina view.", CreatedBy = 1 };
        var mv3Br = new UnitType { ProjectId = marinaVista.ProjectId, TypeName = "3 Bedroom", Category = "Apartment", Bedrooms = 3, Bathrooms = 3.5m, MinAreaSqFt = 1600.00m, MaxAreaSqFt = 1850.00m, BasePrice = 3400000.00m, PricePerSqFt = 2125.00m, TotalUnitsOfType = 100, AvailableUnitsOfType = 40, FloorPlanUrl = "https://example.com/mv/3br.pdf", Description = "Premium 3 bedroom corner unit.", CreatedBy = 1 };

        // Palm Grove Residences unit types
        var pg1Br = new UnitType { ProjectId = palmGrove.ProjectId, TypeName = "1 Bedroom", Category = "Apartment", Bedrooms = 1, Bathrooms = 1.0m, MinAreaSqFt = 650.00m, MaxAreaSqFt = 700.00m, BasePrice = 620000.00m, PricePerSqFt = 953.00m, TotalUnitsOfType = 60, AvailableUnitsOfType = 12, FloorPlanUrl = "https://example.com/pg/1br.pdf", Description = "Garden-facing 1 bedroom apartment.", CreatedBy = 1 };
        var pg2Br = new UnitType { ProjectId = palmGrove.ProjectId, TypeName = "2 Bedroom", Category = "Apartment", Bedrooms = 2, Bathrooms = 2.0m, MinAreaSqFt = 980.00m, MaxAreaSqFt = 1050.00m, BasePrice = 950000.00m, PricePerSqFt = 969.00m, TotalUnitsOfType = 100, AvailableUnitsOfType = 28, FloorPlanUrl = "https://example.com/pg/2br.pdf", Description = "2 bedroom apartment with courtyard access.", CreatedBy = 1 };
        var pgTh3 = new UnitType { ProjectId = palmGrove.ProjectId, TypeName = "3 Bedroom Townhouse", Category = "Townhouse", Bedrooms = 3, Bathrooms = 3.0m, MinAreaSqFt = 1800.00m, MaxAreaSqFt = 1950.00m, BasePrice = 1850000.00m, PricePerSqFt = 1027.00m, TotalUnitsOfType = 100, AvailableUnitsOfType = 30, FloorPlanUrl = "https://example.com/pg/th3.pdf", Description = "3 bedroom townhouse with private garden.", CreatedBy = 1 };

        // Downtown Business Hub unit types
        var dhSmallOffice = new UnitType { ProjectId = downtownHub.ProjectId, TypeName = "Small Office", Category = "Office", Bathrooms = 1.0m, MinAreaSqFt = 500.00m, MaxAreaSqFt = 700.00m, BasePrice = 1200000.00m, PricePerSqFt = 2000.00m, TotalUnitsOfType = 90, AvailableUnitsOfType = 20, FloorPlanUrl = "https://example.com/dh/small-office.pdf", Description = "Fitted small office suite.", CreatedBy = 1 };
        var dhLargeOffice = new UnitType { ProjectId = downtownHub.ProjectId, TypeName = "Large Office Floor", Category = "Office", Bathrooms = 2.0m, MinAreaSqFt = 3000.00m, MaxAreaSqFt = 4200.00m, BasePrice = 7200000.00m, PricePerSqFt = 2100.00m, TotalUnitsOfType = 30, AvailableUnitsOfType = 6, FloorPlanUrl = "https://example.com/dh/large-office.pdf", Description = "Full-floor office space, shell & core.", CreatedBy = 1 };
        var dhRetail = new UnitType { ProjectId = downtownHub.ProjectId, TypeName = "Retail Unit", Category = "Retail", Bathrooms = 1.0m, MinAreaSqFt = 350.00m, MaxAreaSqFt = 900.00m, BasePrice = 1500000.00m, PricePerSqFt = 3200.00m, TotalUnitsOfType = 90, AvailableUnitsOfType = 18, FloorPlanUrl = "https://example.com/dh/retail.pdf", Description = "Ground floor retail podium unit.", CreatedBy = 1 };

        // Emerald Hills Villas unit types
        var ehVilla4 = new UnitType { ProjectId = emeraldHills.ProjectId, TypeName = "4 Bedroom Villa", Category = "Villa", Bedrooms = 4, Bathrooms = 4.5m, MinAreaSqFt = 3800.00m, MaxAreaSqFt = 4200.00m, BasePrice = 3200000.00m, PricePerSqFt = 800.00m, TotalUnitsOfType = 60, AvailableUnitsOfType = 60, FloorPlanUrl = "https://example.com/eh/villa4.pdf", Description = "Golf-course facing 4 bedroom villa.", CreatedBy = 1 };
        var ehVilla5 = new UnitType { ProjectId = emeraldHills.ProjectId, TypeName = "5 Bedroom Villa", Category = "Villa", Bedrooms = 5, Bathrooms = 5.5m, MinAreaSqFt = 5200.00m, MaxAreaSqFt = 5600.00m, BasePrice = 4800000.00m, PricePerSqFt = 900.00m, TotalUnitsOfType = 40, AvailableUnitsOfType = 40, FloorPlanUrl = "https://example.com/eh/villa5.pdf", Description = "Corner 5 bedroom villa with private pool.", CreatedBy = 1 };
        var ehMansion6 = new UnitType { ProjectId = emeraldHills.ProjectId, TypeName = "6 Bedroom Mansion", Category = "Villa", Bedrooms = 6, Bathrooms = 7.0m, MinAreaSqFt = 7500.00m, MaxAreaSqFt = 8200.00m, BasePrice = 8500000.00m, PricePerSqFt = 1050.00m, TotalUnitsOfType = 20, AvailableUnitsOfType = 20, FloorPlanUrl = "https://example.com/eh/mansion6.pdf", Description = "Flagship 6 bedroom mansion plot.", CreatedBy = 1 };

        // Riverside Mixed-Use Complex unit types
        var rs1Br = new UnitType { ProjectId = riverside.ProjectId, TypeName = "1 Bedroom", Category = "Apartment", Bedrooms = 1, Bathrooms = 1.0m, MinAreaSqFt = 600.00m, MaxAreaSqFt = 650.00m, BasePrice = 25000000.00m, PricePerSqFt = 40000.00m, TotalUnitsOfType = 100, AvailableUnitsOfType = 15, FloorPlanUrl = "https://example.com/rs/1br.pdf", Description = "Riverfront 1 bedroom apartment.", CreatedBy = 1 };
        var rs2Br = new UnitType { ProjectId = riverside.ProjectId, TypeName = "2 Bedroom", Category = "Apartment", Bedrooms = 2, Bathrooms = 2.0m, MinAreaSqFt = 950.00m, MaxAreaSqFt = 1000.00m, BasePrice = 38000000.00m, PricePerSqFt = 39500.00m, TotalUnitsOfType = 140, AvailableUnitsOfType = 22, FloorPlanUrl = "https://example.com/rs/2br.pdf", Description = "Riverfront 2 bedroom apartment.", CreatedBy = 1 };
        var rsRetail = new UnitType { ProjectId = riverside.ProjectId, TypeName = "Retail Shop", Category = "Retail", Bathrooms = 1.0m, MinAreaSqFt = 300.00m, MaxAreaSqFt = 800.00m, BasePrice = 18000000.00m, PricePerSqFt = 45000.00m, TotalUnitsOfType = 60, AvailableUnitsOfType = 9, FloorPlanUrl = "https://example.com/rs/retail.pdf", Description = "Ground floor retail unit facing the boulevard.", CreatedBy = 1 };
        var rsSuite = new UnitType { ProjectId = riverside.ProjectId, TypeName = "Hotel Suite", Category = "Hotel", Bedrooms = 1, Bathrooms = 1.0m, MinAreaSqFt = 400.00m, MaxAreaSqFt = 500.00m, BasePrice = 22000000.00m, PricePerSqFt = 47000.00m, TotalUnitsOfType = 40, AvailableUnitsOfType = 5, FloorPlanUrl = "https://example.com/rs/suite.pdf", Description = "Boutique hotel investment suite.", CreatedBy = 1 };

        context.UnitTypes.AddRange(
            mvStudio, mv1Br, mv2Br, mv3Br,
            pg1Br, pg2Br, pgTh3,
            dhSmallOffice, dhLargeOffice, dhRetail,
            ehVilla4, ehVilla5, ehMansion6,
            rs1Br, rs2Br, rsRetail, rsSuite);
        await context.SaveChangesAsync();

        context.Inventory.AddRange(
            // Marina Vista Towers
            new Inventory { ProjectId = marinaVista.ProjectId, UnitTypeId = mvStudio.UnitTypeId, UnitNumber = "MV-A-0501", BuildingTower = "Tower A", FloorNumber = 5, ViewType = "Marina View", AreaSqFt = 455.00m, Bedrooms = 0, Bathrooms = 1.0m, ParkingSpaces = 1, HasBalcony = true, FurnishingStatus = "Unfurnished", ListPrice = 887250.00m, PricePerSqFt = 1950.00m, Status = RealEstateInventoryStatuses.Available, ListingDate = new DateOnly(2024, 1, 10), AgentName = "Fatima Noor", AgentContact = "+971-50-1112233", Notes = "Bright studio, high demand.", ApprovalStatus = RealEstateApprovalStatuses.Approved, CreatedBy = 1 },
            new Inventory { ProjectId = marinaVista.ProjectId, UnitTypeId = mv1Br.UnitTypeId, UnitNumber = "MV-A-1203", BuildingTower = "Tower A", FloorNumber = 12, ViewType = "Marina View", AreaSqFt = 765.00m, Bedrooms = 1, Bathrooms = 1.5m, ParkingSpaces = 1, HasBalcony = true, FurnishingStatus = "Semi-Furnished", ListPrice = 1434375.00m, PricePerSqFt = 1875.00m, Status = RealEstateInventoryStatuses.Reserved, ListingDate = new DateOnly(2024, 1, 15), BuyerTenantName = "Hassan Rahim", AgentName = "Fatima Noor", AgentContact = "+971-50-1112233", Notes = "Reserved pending final payment.", ApprovalStatus = RealEstateApprovalStatuses.Approved, CreatedBy = 1 },
            new Inventory { ProjectId = marinaVista.ProjectId, UnitTypeId = mv2Br.UnitTypeId, UnitNumber = "MV-B-2004", BuildingTower = "Tower B", FloorNumber = 20, ViewType = "Sea & Marina View", AreaSqFt = 1180.00m, Bedrooms = 2, Bathrooms = 2.5m, ParkingSpaces = 2, HasBalcony = true, FurnishingStatus = "Unfurnished", ListPrice = 2252620.00m, PricePerSqFt = 1909.00m, Status = RealEstateInventoryStatuses.Sold, ListingDate = new DateOnly(2023, 11, 5), SoldOrLeasedDate = new DateOnly(2024, 2, 20), BuyerTenantName = "Mei Ling Tan", AgentName = "Karim Aziz", AgentContact = "+971-50-2223344", Notes = "Sold to overseas investor.", ApprovalStatus = RealEstateApprovalStatuses.Approved, CreatedBy = 1 },
            new Inventory { ProjectId = marinaVista.ProjectId, UnitTypeId = mv3Br.UnitTypeId, UnitNumber = "MV-B-3001", BuildingTower = "Tower B", FloorNumber = 30, ViewType = "Full Marina View", AreaSqFt = 1720.00m, Bedrooms = 3, Bathrooms = 3.5m, ParkingSpaces = 2, HasBalcony = true, FurnishingStatus = "Furnished", ListPrice = 3655000.00m, PricePerSqFt = 2125.00m, Status = RealEstateInventoryStatuses.Available, ListingDate = new DateOnly(2024, 2, 1), AgentName = "Karim Aziz", AgentContact = "+971-50-2223344", Notes = "Top-floor premium corner unit.", ApprovalStatus = RealEstateApprovalStatuses.Approved, CreatedBy = 1 },

            // Palm Grove Residences
            new Inventory { ProjectId = palmGrove.ProjectId, UnitTypeId = pg1Br.UnitTypeId, UnitNumber = "PG-C-102", BuildingTower = "Cluster C", FloorNumber = 1, ViewType = "Courtyard View", AreaSqFt = 675.00m, Bedrooms = 1, Bathrooms = 1.0m, ParkingSpaces = 1, HasBalcony = true, FurnishingStatus = "Unfurnished", ListPrice = 643275.00m, PricePerSqFt = 953.00m, Status = RealEstateInventoryStatuses.Sold, ListingDate = new DateOnly(2023, 6, 1), SoldOrLeasedDate = new DateOnly(2023, 9, 12), BuyerTenantName = "Yousef Al Amin", AgentName = "Nadia Saeed", AgentContact = "+971-50-3334455", ApprovalStatus = RealEstateApprovalStatuses.Approved, CreatedBy = 1 },
            new Inventory { ProjectId = palmGrove.ProjectId, UnitTypeId = pg2Br.UnitTypeId, UnitNumber = "PG-D-208", BuildingTower = "Cluster D", FloorNumber = 2, ViewType = "Garden View", AreaSqFt = 1015.00m, Bedrooms = 2, Bathrooms = 2.0m, ParkingSpaces = 1, HasBalcony = false, FurnishingStatus = "Unfurnished", ListPrice = 983535.00m, PricePerSqFt = 969.00m, Status = RealEstateInventoryStatuses.Available, ListingDate = new DateOnly(2024, 1, 20), AgentName = "Nadia Saeed", AgentContact = "+971-50-3334455", ApprovalStatus = RealEstateApprovalStatuses.Approved, CreatedBy = 1 },
            new Inventory { ProjectId = palmGrove.ProjectId, UnitTypeId = pgTh3.UnitTypeId, UnitNumber = "PG-TH-014", BuildingTower = "Townhouse Row 1", FloorNumber = 0, ViewType = "Park View", AreaSqFt = 1875.00m, Bedrooms = 3, Bathrooms = 3.0m, ParkingSpaces = 2, HasBalcony = false, FurnishingStatus = "Unfurnished", ListPrice = 1925625.00m, PricePerSqFt = 1027.00m, Status = RealEstateInventoryStatuses.Blocked, ListingDate = new DateOnly(2024, 2, 5), AgentName = "Nadia Saeed", AgentContact = "+971-50-3334455", Notes = "Blocked for corporate client review.", ApprovalStatus = RealEstateApprovalStatuses.Approved, CreatedBy = 1 },

            // Downtown Business Hub
            new Inventory { ProjectId = downtownHub.ProjectId, UnitTypeId = dhSmallOffice.UnitTypeId, UnitNumber = "DH-15-A", BuildingTower = "Main Tower", FloorNumber = 15, ViewType = "City Skyline", AreaSqFt = 610.00m, Bathrooms = 1.0m, ParkingSpaces = 1, HasBalcony = false, FurnishingStatus = "Unfurnished", ListPrice = 1220000.00m, PricePerSqFt = 2000.00m, Status = RealEstateInventoryStatuses.Leased, ListingDate = new DateOnly(2023, 9, 1), SoldOrLeasedDate = new DateOnly(2023, 10, 1), BuyerTenantName = "BrightTech FZ LLC", AgentName = "Tariq Salman", AgentContact = "+971-50-4445566", Notes = "Leased on 3-year contract.", ApprovalStatus = RealEstateApprovalStatuses.Approved, CreatedBy = 1 },
            new Inventory { ProjectId = downtownHub.ProjectId, UnitTypeId = dhLargeOffice.UnitTypeId, UnitNumber = "DH-25-FULL", BuildingTower = "Main Tower", FloorNumber = 25, ViewType = "City Skyline", AreaSqFt = 3600.00m, Bathrooms = 2.0m, ParkingSpaces = 8, HasBalcony = false, FurnishingStatus = "Shell & Core", ListPrice = 7560000.00m, PricePerSqFt = 2100.00m, Status = RealEstateInventoryStatuses.Available, ListingDate = new DateOnly(2023, 12, 1), AgentName = "Tariq Salman", AgentContact = "+971-50-4445566", Notes = "Full floor available for fit-out.", ApprovalStatus = RealEstateApprovalStatuses.Approved, CreatedBy = 1 },
            new Inventory { ProjectId = downtownHub.ProjectId, UnitTypeId = dhRetail.UnitTypeId, UnitNumber = "DH-G-07", BuildingTower = "Retail Podium", FloorNumber = 0, ViewType = "Street Front", AreaSqFt = 480.00m, Bathrooms = 1.0m, ParkingSpaces = 0, HasBalcony = false, FurnishingStatus = "Shell & Core", ListPrice = 1536000.00m, PricePerSqFt = 3200.00m, Status = RealEstateInventoryStatuses.Available, ListingDate = new DateOnly(2024, 1, 5), AgentName = "Tariq Salman", AgentContact = "+971-50-4445566", Notes = "Corner retail unit, high footfall.", ApprovalStatus = RealEstateApprovalStatuses.Approved, CreatedBy = 1 },

            // Emerald Hills Villas
            new Inventory { ProjectId = emeraldHills.ProjectId, UnitTypeId = ehVilla4.UnitTypeId, UnitNumber = "EH-V-023", ViewType = "Golf Course View", AreaSqFt = 4050.00m, Bedrooms = 4, Bathrooms = 4.5m, ParkingSpaces = 3, HasBalcony = false, FurnishingStatus = "Unfurnished", ListPrice = 3240000.00m, PricePerSqFt = 800.00m, Status = RealEstateInventoryStatuses.Available, ListingDate = new DateOnly(2024, 3, 1), AgentName = "Rania Fadel", AgentContact = "+971-50-5556677", Notes = "Off-plan, under construction.", ApprovalStatus = RealEstateApprovalStatuses.PendingReview, CreatedBy = 1 },
            new Inventory { ProjectId = emeraldHills.ProjectId, UnitTypeId = ehVilla5.UnitTypeId, UnitNumber = "EH-V-045", ViewType = "Golf Course View", AreaSqFt = 5400.00m, Bedrooms = 5, Bathrooms = 5.5m, ParkingSpaces = 3, HasBalcony = false, FurnishingStatus = "Unfurnished", ListPrice = 4860000.00m, PricePerSqFt = 900.00m, Status = RealEstateInventoryStatuses.Reserved, ListingDate = new DateOnly(2024, 3, 10), BuyerTenantName = "David O'Connor", AgentName = "Rania Fadel", AgentContact = "+971-50-5556677", Notes = "Reserved with 10% booking fee.", ApprovalStatus = RealEstateApprovalStatuses.PendingReview, CreatedBy = 1 },
            new Inventory { ProjectId = emeraldHills.ProjectId, UnitTypeId = ehMansion6.UnitTypeId, UnitNumber = "EH-V-001", ViewType = "Panoramic Golf View", AreaSqFt = 7850.00m, Bedrooms = 6, Bathrooms = 7.0m, ParkingSpaces = 4, HasBalcony = false, FurnishingStatus = "Unfurnished", ListPrice = 8842500.00m, PricePerSqFt = 1050.00m, Status = RealEstateInventoryStatuses.Available, ListingDate = new DateOnly(2024, 3, 15), AgentName = "Rania Fadel", AgentContact = "+971-50-5556677", Notes = "Flagship plot, corner position.", ApprovalStatus = RealEstateApprovalStatuses.PendingReview, CreatedBy = 1 },

            // Riverside Mixed-Use Complex
            new Inventory { ProjectId = riverside.ProjectId, UnitTypeId = rs1Br.UnitTypeId, UnitNumber = "RS-A-0602", BuildingTower = "Tower A", FloorNumber = 6, ViewType = "River View", AreaSqFt = 620.00m, Bedrooms = 1, Bathrooms = 1.0m, ParkingSpaces = 1, HasBalcony = true, FurnishingStatus = "Unfurnished", ListPrice = 24800000.00m, PricePerSqFt = 40000.00m, Status = RealEstateInventoryStatuses.Sold, ListingDate = new DateOnly(2023, 10, 1), SoldOrLeasedDate = new DateOnly(2024, 1, 18), BuyerTenantName = "Ayesha Malik", AgentName = "Usman Tariq", AgentContact = "+92-300-1112233", ApprovalStatus = RealEstateApprovalStatuses.Approved, CreatedBy = 1 },
            new Inventory { ProjectId = riverside.ProjectId, UnitTypeId = rs2Br.UnitTypeId, UnitNumber = "RS-A-1105", BuildingTower = "Tower A", FloorNumber = 11, ViewType = "River View", AreaSqFt = 975.00m, Bedrooms = 2, Bathrooms = 2.0m, ParkingSpaces = 1, HasBalcony = true, FurnishingStatus = "Semi-Furnished", ListPrice = 38512500.00m, PricePerSqFt = 39500.00m, Status = RealEstateInventoryStatuses.Available, ListingDate = new DateOnly(2024, 1, 25), AgentName = "Usman Tariq", AgentContact = "+92-300-1112233", ApprovalStatus = RealEstateApprovalStatuses.Approved, CreatedBy = 1 },
            new Inventory { ProjectId = riverside.ProjectId, UnitTypeId = rsRetail.UnitTypeId, UnitNumber = "RS-G-11", BuildingTower = "Retail Podium", FloorNumber = 0, ViewType = "Boulevard Front", AreaSqFt = 520.00m, Bathrooms = 1.0m, ParkingSpaces = 0, HasBalcony = false, FurnishingStatus = "Shell & Core", ListPrice = 23400000.00m, PricePerSqFt = 45000.00m, Status = RealEstateInventoryStatuses.Available, ListingDate = new DateOnly(2024, 2, 1), AgentName = "Usman Tariq", AgentContact = "+92-300-1112233", Notes = "Prime boulevard-facing shop.", ApprovalStatus = RealEstateApprovalStatuses.Approved, CreatedBy = 1 },
            new Inventory { ProjectId = riverside.ProjectId, UnitTypeId = rsSuite.UnitTypeId, UnitNumber = "RS-H-302", BuildingTower = "Hotel Tower", FloorNumber = 3, ViewType = "City View", AreaSqFt = 450.00m, Bedrooms = 1, Bathrooms = 1.0m, ParkingSpaces = 0, HasBalcony = false, FurnishingStatus = "Furnished", ListPrice = 21150000.00m, PricePerSqFt = 47000.00m, Status = RealEstateInventoryStatuses.Available, ListingDate = new DateOnly(2024, 2, 10), AgentName = "Usman Tariq", AgentContact = "+92-300-1112233", Notes = "Managed hotel suite with rental pool option.", ApprovalStatus = RealEstateApprovalStatuses.Approved, CreatedBy = 1 });

        context.Amenities.AddRange(
            // Marina Vista Towers
            new Amenity { ProjectId = marinaVista.ProjectId, AmenityName = "Infinity Swimming Pool", Category = "Recreational", Description = "Rooftop infinity pool overlooking the marina.", IconUrl = "https://example.com/icons/pool.svg", ImageUrl = "https://example.com/img/mv-pool.jpg", IsHighlighted = true, DisplayOrder = 1, CreatedBy = 1 },
            new Amenity { ProjectId = marinaVista.ProjectId, AmenityName = "Fully Equipped Gym", Category = "Wellness", Description = "24/7 gym with cardio and free-weight zones.", IconUrl = "https://example.com/icons/gym.svg", ImageUrl = "https://example.com/img/mv-gym.jpg", IsHighlighted = true, DisplayOrder = 2, CreatedBy = 1 },
            new Amenity { ProjectId = marinaVista.ProjectId, AmenityName = "24/7 Security & CCTV", Category = "Security", Description = "Round-the-clock security personnel and CCTV monitoring.", IconUrl = "https://example.com/icons/security.svg", DisplayOrder = 3, CreatedBy = 1 },
            new Amenity { ProjectId = marinaVista.ProjectId, AmenityName = "Kids Play Area", Category = "Recreational", Description = "Dedicated indoor and outdoor play area for children.", IconUrl = "https://example.com/icons/kids.svg", DisplayOrder = 4, CreatedBy = 1 },
            new Amenity { ProjectId = marinaVista.ProjectId, AmenityName = "Covered Parking", Category = "Convenience", Description = "Allocated covered parking bays for residents.", IconUrl = "https://example.com/icons/parking.svg", DisplayOrder = 5, CreatedBy = 1 },

            // Palm Grove Residences
            new Amenity { ProjectId = palmGrove.ProjectId, AmenityName = "Landscaped Courtyard", Category = "Recreational", Description = "Central landscaped courtyard with seating areas.", IconUrl = "https://example.com/icons/garden.svg", ImageUrl = "https://example.com/img/pg-courtyard.jpg", IsHighlighted = true, DisplayOrder = 1, CreatedBy = 1 },
            new Amenity { ProjectId = palmGrove.ProjectId, AmenityName = "Community Pool", Category = "Recreational", Description = "Shared community swimming pool.", IconUrl = "https://example.com/icons/pool.svg", IsHighlighted = true, DisplayOrder = 2, CreatedBy = 1 },
            new Amenity { ProjectId = palmGrove.ProjectId, AmenityName = "Jogging Track", Category = "Wellness", Description = "1.2 km jogging and cycling track around the community.", IconUrl = "https://example.com/icons/jog.svg", DisplayOrder = 3, CreatedBy = 1 },
            new Amenity { ProjectId = palmGrove.ProjectId, AmenityName = "Retail Plaza", Category = "Convenience", Description = "On-site retail plaza with supermarket and cafes.", IconUrl = "https://example.com/icons/retail.svg", DisplayOrder = 4, CreatedBy = 1 },

            // Downtown Business Hub
            new Amenity { ProjectId = downtownHub.ProjectId, AmenityName = "High-Speed Elevators", Category = "Convenience", Description = "Destination-controlled high-speed elevator system.", IconUrl = "https://example.com/icons/elevator.svg", DisplayOrder = 1, CreatedBy = 1 },
            new Amenity { ProjectId = downtownHub.ProjectId, AmenityName = "Conference & Business Center", Category = "Business", Description = "Shared conference rooms and business center facilities.", IconUrl = "https://example.com/icons/business.svg", ImageUrl = "https://example.com/img/dh-conference.jpg", IsHighlighted = true, DisplayOrder = 2, CreatedBy = 1 },
            new Amenity { ProjectId = downtownHub.ProjectId, AmenityName = "Basement Parking", Category = "Convenience", Description = "Multi-level basement parking for tenants and visitors.", IconUrl = "https://example.com/icons/parking.svg", DisplayOrder = 3, CreatedBy = 1 },
            new Amenity { ProjectId = downtownHub.ProjectId, AmenityName = "Food Court", Category = "Convenience", Description = "Ground floor food court with multiple dining options.", IconUrl = "https://example.com/icons/food.svg", DisplayOrder = 4, CreatedBy = 1 },

            // Emerald Hills Villas
            new Amenity { ProjectId = emeraldHills.ProjectId, AmenityName = "Championship Golf Course", Category = "Recreational", Description = "18-hole championship golf course within the community.", IconUrl = "https://example.com/icons/golf.svg", ImageUrl = "https://example.com/img/eh-golf.jpg", IsHighlighted = true, DisplayOrder = 1, CreatedBy = 1 },
            new Amenity { ProjectId = emeraldHills.ProjectId, AmenityName = "Private Villa Pools", Category = "Recreational", Description = "Option for private swimming pool per villa.", IconUrl = "https://example.com/icons/pool.svg", IsHighlighted = true, DisplayOrder = 2, CreatedBy = 1 },
            new Amenity { ProjectId = emeraldHills.ProjectId, AmenityName = "Equestrian Club", Category = "Recreational", Description = "On-site equestrian club and riding trails.", IconUrl = "https://example.com/icons/horse.svg", DisplayOrder = 3, CreatedBy = 1 },
            new Amenity { ProjectId = emeraldHills.ProjectId, AmenityName = "Gated Community with Security", Category = "Security", Description = "24/7 gated access with dedicated security patrols.", IconUrl = "https://example.com/icons/security.svg", DisplayOrder = 4, CreatedBy = 1 },
            new Amenity { ProjectId = emeraldHills.ProjectId, AmenityName = "Clubhouse", Category = "Recreational", Description = "Community clubhouse with dining and event spaces.", IconUrl = "https://example.com/icons/clubhouse.svg", DisplayOrder = 5, CreatedBy = 1 },

            // Riverside Mixed-Use Complex
            new Amenity { ProjectId = riverside.ProjectId, AmenityName = "Riverside Promenade", Category = "Recreational", Description = "Landscaped promenade along the riverfront.", IconUrl = "https://example.com/icons/promenade.svg", ImageUrl = "https://images.pexels.com/photos/8469931/pexels-photo-8469931.jpeg", IsHighlighted = true, DisplayOrder = 1, CreatedBy = 1 },
            new Amenity { ProjectId = riverside.ProjectId, AmenityName = "Boutique Hotel & Spa", Category = "Wellness", Description = "On-site boutique hotel with spa and wellness center.", IconUrl = "https://example.com/icons/spa.svg", IsHighlighted = true, DisplayOrder = 2, CreatedBy = 1 },
            new Amenity { ProjectId = riverside.ProjectId, AmenityName = "Cinema & Entertainment Zone", Category = "Recreational", Description = "Multiplex cinema and family entertainment zone.", IconUrl = "https://example.com/icons/cinema.svg", DisplayOrder = 3, CreatedBy = 1 },
            new Amenity { ProjectId = riverside.ProjectId, AmenityName = "Multi-Level Parking", Category = "Convenience", Description = "Dedicated multi-level parking for residents, retail, and hotel guests.", IconUrl = "https://example.com/icons/parking.svg", DisplayOrder = 4, CreatedBy = 1 });

        // Sample media gallery rows (the legacy scalar ImageUrl/VideoUrl above stay populated too, for compatibility).
        context.Media.AddRange(
            new Media { EntityType = RealEstateMediaEntityTypes.Project, EntityId = marinaVista.ProjectId, MediaType = RealEstateMediaTypes.Image, Url = marinaVista.ImageUrl!, DisplayOrder = 1, IsPrimary = true },
            new Media { EntityType = RealEstateMediaEntityTypes.Project, EntityId = marinaVista.ProjectId, MediaType = RealEstateMediaTypes.Video, Url = marinaVista.VideoUrl!, DisplayOrder = 2, IsPrimary = false },
            new Media { EntityType = RealEstateMediaEntityTypes.Project, EntityId = palmGrove.ProjectId, MediaType = RealEstateMediaTypes.Image, Url = palmGrove.ImageUrl!, DisplayOrder = 1, IsPrimary = true });

        await context.SaveChangesAsync();
    }

    /// <summary>DEV-ONLY seeded credentials, clearly fake - never use these in a real deployment.</summary>
    private static void SeedUsers(RealEstateDbContext context)
    {
        context.Users.AddRange(
            new User { Email = "admin@realestate.local", PasswordHash = PasswordHasher.Hash("Admin123!"), Role = RealEstateRoles.Admin },
            new User { Email = "agent@realestate.local", PasswordHash = PasswordHasher.Hash("Agent123!"), Role = RealEstateRoles.Agent });
    }

    private static void SeedLookups(RealEstateDbContext context)
    {
        var lookups = new List<Lookup>();
        void Add(string type, params string[] codes)
        {
            for (var i = 0; i < codes.Length; i++)
            {
                lookups.Add(new Lookup { LookupType = type, Code = codes[i], DisplayName = codes[i], DisplayOrder = i + 1 });
            }
        }

        Add(RealEstateLookupTypes.ProjectType, "Residential", "Commercial", "Mixed-Use", "Villa", "Retail");
        Add(RealEstateLookupTypes.ProjectStatus, "Planning", "Under Construction", "Ready", "Handover", "Sold Out");
        Add(RealEstateLookupTypes.InventoryStatus, RealEstateInventoryStatuses.All);
        Add(RealEstateLookupTypes.UnitCategory, "Apartment", "Villa", "Townhouse", "Duplex", "Penthouse", "Office", "Retail", "Hotel");
        Add(RealEstateLookupTypes.FurnishingStatus, "Furnished", "Semi-Furnished", "Unfurnished", "Shell & Core");
        Add(RealEstateLookupTypes.AmenityCategory, "Recreational", "Security", "Wellness", "Convenience", "Business");
        Add(RealEstateLookupTypes.ViewType, "Sea View", "Marina View", "City View", "Garden View", "Pool View", "Golf Course View", "River View", "Courtyard View", "Street Front");

        context.Lookups.AddRange(lookups);
    }
}
