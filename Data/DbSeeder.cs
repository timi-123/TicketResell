using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicketResell.Models;
using TicketResell.Services;

namespace TicketResell.Data;

public static class DbSeeder
{
    public const string AdminRole = "Admin";

    public static async Task SeedIdentityAsync(RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        if (!await roleManager.RoleExistsAsync(AdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(AdminRole));
        }

        var email = configuration["Admin:Email"] ?? "admin@ticketresell.local";
        var password = configuration["Admin:Password"] ?? "Admin123!";

        var admin = await userManager.FindByEmailAsync(email);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = "Site",
                LastName = "Admin",
                Address = "Head office",
                City = "Skopje"
            };

            var result = await userManager.CreateAsync(admin, password);
            if (!result.Succeeded)
            {
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(admin, AdminRole))
        {
            await userManager.AddToRoleAsync(admin, AdminRole);
        }
    }

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var existingCategories = await context.Categories.ToDictionaryAsync(c => c.Name);
        var existingVenues = await context.Venues.ToDictionaryAsync(v => v.Name);
        var existingEventsByTitle = await context.Events.ToDictionaryAsync(e => e.Title);

        Category GetOrAddCategory(string name, string description)
        {
            if (existingCategories.TryGetValue(name, out var category))
            {
                return category;
            }

            var created = new Category { Name = name, Description = description };
            existingCategories[name] = created;
            return created;
        }

        Venue GetOrAddVenue(string name, string address, string city, int capacity)
        {
            if (existingVenues.TryGetValue(name, out var venue))
            {
                return venue;
            }

            var created = new Venue { Name = name, Address = address, City = city, Capacity = capacity };
            existingVenues[name] = created;
            return created;
        }

        var concerts = GetOrAddCategory("Concert", "Live music, festivals and tours");
        var sports = GetOrAddCategory("Sports", "Football, basketball and handball fixtures");
        var theatre = GetOrAddCategory("Theatre", "Plays, opera and stand-up comedy");
        var festivals = GetOrAddCategory("Festival", "Multi-day music and film festivals");

        var arena = GetOrAddVenue("Toshe Proeski Arena", "Bulevar Ilinden 1", "Skopje", 33000);
        var sportsCentre = GetOrAddVenue("Boris Trajkovski Sports Centre", "Bulevar Treta Makedonska Brigada 62", "Skopje", 6500);
        var nationalTheatre = GetOrAddVenue("Macedonian National Theatre", "Kej Dimitar Vlahov 3", "Skopje", 850);
        var universalHall = GetOrAddVenue("Univerzalna Sala", "Bulevar Partizanski Odredi 5", "Skopje", 1500);
        var ohridAmphitheatre = GetOrAddVenue("Ohrid Antique Theatre", "Tsar Samuil 92", "Ohrid", 2500);
        var bitolaCultureCentre = GetOrAddVenue("Bitola Culture Centre", "Marshal Tito 46", "Bitola", 700);
        var tetovoHall = GetOrAddVenue("Tetovo Sports Hall", "Ilinden bb", "Tetovo", 3200);
        var prilepHall = GetOrAddVenue("Prilep Sports Hall", "Aleksandar Makedonski 14", "Prilep", 2000);
        var strumicaArena = GetOrAddVenue("Strumica Arena", "Goce Delcev 30", "Strumica", 4000);

        var today = DateTime.Today;

        var candidateEvents = new[]
        {
            new Event
            {
                Title = "Bjelo Dugme Tribute Night",
                Description = "Full-band tribute running through four decades of hits.",
                StartsAt = today.AddDays(21).AddHours(20),
                Venue = arena,
                Category = concerts
            },
            new Event
            {
                Title = "Summer Rock Festival - Day 1",
                Description = "Six bands across two stages, gates open at 16:00.",
                StartsAt = today.AddDays(45).AddHours(16),
                Venue = arena,
                Category = festivals
            },
            new Event
            {
                Title = "Summer Rock Festival - Day 2",
                Description = "Headline set plus late-night DJ programme.",
                StartsAt = today.AddDays(46).AddHours(16),
                Venue = arena,
                Category = festivals
            },
            new Event
            {
                Title = "Vardar vs Pelister",
                Description = "Handball league fixture, round 14.",
                StartsAt = today.AddDays(10).AddHours(19),
                Venue = sportsCentre,
                Category = sports
            },
            new Event
            {
                Title = "MZT Skopje vs Rabotnicki",
                Description = "Basketball derby, expected sell-out.",
                StartsAt = today.AddDays(17).AddHours(18),
                Venue = sportsCentre,
                Category = sports
            },
            new Event
            {
                Title = "National Cup Final",
                Description = "Football cup final with open seating.",
                StartsAt = today.AddDays(60).AddHours(17),
                Venue = arena,
                Category = sports
            },
            new Event
            {
                Title = "Hamlet",
                Description = "Classic staging in Macedonian with English subtitles.",
                StartsAt = today.AddDays(14).AddHours(19).AddMinutes(30),
                Venue = nationalTheatre,
                Category = theatre
            },
            new Event
            {
                Title = "Stand-Up Comedy Gala",
                Description = "Five comedians, one night, strictly 18+.",
                StartsAt = today.AddDays(30).AddHours(21),
                Venue = nationalTheatre,
                Category = theatre
            },
            new Event
            {
                Title = "Kiril Dzajkovski Live",
                Description = "Electronic set with a string section and guest vocalists.",
                StartsAt = today.AddDays(5).AddHours(21),
                Venue = universalHall,
                Category = concerts
            },
            new Event
            {
                Title = "Philharmonic: Beethoven Cycle",
                Description = "Symphonies five and seven in one evening.",
                StartsAt = today.AddDays(12).AddHours(20),
                Venue = universalHall,
                Category = concerts
            },
            new Event
            {
                Title = "Ohrid Summer Festival: Opening Night",
                Description = "Opening concert in the antique theatre above the lake.",
                StartsAt = today.AddDays(26).AddHours(21),
                Venue = ohridAmphitheatre,
                Category = festivals
            },
            new Event
            {
                Title = "Ohrid Summer Festival: Chamber Evening",
                Description = "String quartet programme, seating unreserved.",
                StartsAt = today.AddDays(28).AddHours(20).AddMinutes(30),
                Venue = ohridAmphitheatre,
                Category = festivals
            },
            new Event
            {
                Title = "Lake Stage Jazz Night",
                Description = "Trio sets running until midnight.",
                StartsAt = today.AddDays(38).AddHours(21),
                Venue = ohridAmphitheatre,
                Category = concerts
            },
            new Event
            {
                Title = "Bitola Film Festival: Gala Screening",
                Description = "Opening film with the director in attendance.",
                StartsAt = today.AddDays(8).AddHours(19),
                Venue = bitolaCultureCentre,
                Category = festivals
            },
            new Event
            {
                Title = "Pelister vs Vardar",
                Description = "Return leg of the handball fixture.",
                StartsAt = today.AddDays(24).AddHours(18),
                Venue = bitolaCultureCentre,
                Category = sports
            },
            new Event
            {
                Title = "The Cherry Orchard",
                Description = "Chekhov in a new translation, two intervals.",
                StartsAt = today.AddDays(19).AddHours(19),
                Venue = bitolaCultureCentre,
                Category = theatre
            },
            new Event
            {
                Title = "Shkendija vs Rabotnicki",
                Description = "League fixture, away end sold separately.",
                StartsAt = today.AddDays(7).AddHours(17),
                Venue = tetovoHall,
                Category = sports
            },
            new Event
            {
                Title = "Tetovo Winter Cup Final",
                Description = "Regional basketball final.",
                StartsAt = today.AddDays(52).AddHours(19),
                Venue = tetovoHall,
                Category = sports
            },
            new Event
            {
                Title = "Balkan Brass Night",
                Description = "Three brass orchestras, standing floor and seated tiers.",
                StartsAt = today.AddDays(33).AddHours(20),
                Venue = tetovoHall,
                Category = concerts
            },
            new Event
            {
                Title = "New Year Concert",
                Description = "Waltzes and marches to close the year.",
                StartsAt = today.AddDays(88).AddHours(19),
                Venue = universalHall,
                Category = concerts
            },
            new Event
            {
                Title = "Prilep Blues Festival",
                Description = "Three stages of blues and soul across one weekend.",
                StartsAt = today.AddDays(15).AddHours(20),
                Venue = prilepHall,
                Category = festivals
            },
            new Event
            {
                Title = "Pelister Cup Qualifier",
                Description = "Regional qualifier, winner advances to the national round.",
                StartsAt = today.AddDays(9).AddHours(18),
                Venue = prilepHall,
                Category = sports
            },
            new Event
            {
                Title = "Twelfth Night",
                Description = "Shakespeare's comedy, performed in modern dress.",
                StartsAt = today.AddDays(22).AddHours(19).AddMinutes(30),
                Venue = bitolaCultureCentre,
                Category = theatre
            },
            new Event
            {
                Title = "Strumica Summer Concert",
                Description = "Open-air concert to open the summer season.",
                StartsAt = today.AddDays(18).AddHours(20).AddMinutes(30),
                Venue = strumicaArena,
                Category = concerts
            },
            new Event
            {
                Title = "Strumica Carnival Music Night",
                Description = "A night of brass, folk and carnival favourites.",
                StartsAt = today.AddDays(40).AddHours(21),
                Venue = strumicaArena,
                Category = festivals
            },
            new Event
            {
                Title = "Rabotnicki vs Shkendija",
                Description = "Away leg of the league fixture.",
                StartsAt = today.AddDays(32).AddHours(18),
                Venue = strumicaArena,
                Category = sports
            },
            new Event
            {
                Title = "An Evening of Chopin",
                Description = "Solo piano recital, all-Chopin programme.",
                StartsAt = today.AddDays(25).AddHours(20),
                Venue = universalHall,
                Category = concerts
            },
            new Event
            {
                Title = "The Seagull",
                Description = "Chekhov's tragicomedy in a new staging.",
                StartsAt = today.AddDays(35).AddHours(19),
                Venue = nationalTheatre,
                Category = theatre
            },
            new Event
            {
                Title = "Macedonian Idol Live Final",
                Description = "The televised singing final, live in the arena.",
                StartsAt = today.AddDays(42).AddHours(20),
                Venue = arena,
                Category = concerts
            },
            new Event
            {
                Title = "Balkan Handball Championship: Semi-Final",
                Description = "Regional semi-final, winner plays for the title.",
                StartsAt = today.AddDays(48).AddHours(18).AddMinutes(30),
                Venue = sportsCentre,
                Category = sports
            },
            new Event
            {
                Title = "Balkan Handball Championship: Final",
                Description = "Regional final, two days after the semi-final.",
                StartsAt = today.AddDays(50).AddHours(18).AddMinutes(30),
                Venue = sportsCentre,
                Category = sports
            },
            new Event
            {
                Title = "Ohrid Summer Festival: Closing Gala",
                Description = "Closing night of the festival, full orchestra and choir.",
                StartsAt = today.AddDays(55).AddHours(21),
                Venue = ohridAmphitheatre,
                Category = festivals
            },
            new Event
            {
                Title = "Jazz Under the Stars",
                Description = "Quartet set on the lakeside stage.",
                StartsAt = today.AddDays(60).AddHours(21),
                Venue = ohridAmphitheatre,
                Category = concerts
            },
            new Event
            {
                Title = "A Midsummer Night's Dream",
                Description = "Shakespeare's comedy performed in the round.",
                StartsAt = today.AddDays(65).AddHours(19).AddMinutes(30),
                Venue = nationalTheatre,
                Category = theatre
            },
            new Event
            {
                Title = "Comedy Night: New Voices",
                Description = "Six new stand-up acts, hosted by a local favourite.",
                StartsAt = today.AddDays(70).AddHours(21),
                Venue = bitolaCultureCentre,
                Category = theatre
            },
            new Event
            {
                Title = "Tetovo Food & Music Festival",
                Description = "Local food stalls plus live folk and pop acts.",
                StartsAt = today.AddDays(75).AddHours(17),
                Venue = tetovoHall,
                Category = festivals
            },
            new Event
            {
                Title = "Vardar vs MZT Skopje",
                Description = "City derby, basketball league fixture.",
                StartsAt = today.AddDays(80).AddHours(19),
                Venue = sportsCentre,
                Category = sports
            },
            new Event
            {
                Title = "Prilep Basketball Cup",
                Description = "Regional cup, group stage opener.",
                StartsAt = today.AddDays(85).AddHours(18),
                Venue = prilepHall,
                Category = sports
            },
            new Event
            {
                Title = "Skopje Opera Gala",
                Description = "Highlights from Verdi and Puccini, full orchestra.",
                StartsAt = today.AddDays(90).AddHours(20),
                Venue = nationalTheatre,
                Category = theatre
            },
            new Event
            {
                Title = "Winter Rock Fest",
                Description = "Four local bands, one night, indoor stage.",
                StartsAt = today.AddDays(95).AddHours(19),
                Venue = arena,
                Category = festivals
            },
            new Event
            {
                Title = "Strumica Theatre Nights: Macbeth",
                Description = "Touring production, one night only.",
                StartsAt = today.AddDays(100).AddHours(19).AddMinutes(30),
                Venue = strumicaArena,
                Category = theatre
            },
            new Event
            {
                Title = "New Year's Eve Concert Spectacular",
                Description = "Full orchestra countdown concert with fireworks after.",
                StartsAt = today.AddDays(110).AddHours(22),
                Venue = arena,
                Category = concerts
            },
            new Event
            {
                Title = "Chamber Strings Recital",
                Description = "String quartet, programme announced on the night.",
                StartsAt = today.AddDays(115).AddHours(20),
                Venue = universalHall,
                Category = concerts
            },
            new Event
            {
                Title = "Ohrid Winter Jazz",
                Description = "Indoor winter edition of the summer jazz series.",
                StartsAt = today.AddDays(120).AddHours(21),
                Venue = ohridAmphitheatre,
                Category = concerts
            },
            new Event
            {
                Title = "Bitola Regional Handball Cup",
                Description = "Regional cup, final group match.",
                StartsAt = today.AddDays(125).AddHours(18),
                Venue = bitolaCultureCentre,
                Category = sports
            },
            new Event
            {
                Title = "Tetovo Basketball Derby",
                Description = "Local derby, always a sell-out.",
                StartsAt = today.AddDays(130).AddHours(19),
                Venue = tetovoHall,
                Category = sports
            },
            new Event
            {
                Title = "Spring Comedy Tour: Skopje Night",
                Description = "Touring stand-up bill, five acts.",
                StartsAt = today.AddDays(135).AddHours(20),
                Venue = nationalTheatre,
                Category = theatre
            },
            new Event
            {
                Title = "Prilep Spring Festival",
                Description = "Opening weekend of the spring festival season.",
                StartsAt = today.AddDays(140).AddHours(18),
                Venue = prilepHall,
                Category = festivals
            },
            new Event
            {
                Title = "Strumica Football Cup Final",
                Description = "Regional cup final, gates open two hours early.",
                StartsAt = today.AddDays(145).AddHours(18),
                Venue = strumicaArena,
                Category = sports
            },
            new Event
            {
                Title = "Skopje Chamber Orchestra: Spring Concert",
                Description = "Season-closing concert, programme includes Vivaldi.",
                StartsAt = today.AddDays(150).AddHours(20),
                Venue = universalHall,
                Category = concerts
            }
        };

        var now = DateTime.Now;
        var newEvents = new List<Event>();
        var changed = false;

        foreach (var candidate in candidateEvents)
        {
            if (!existingEventsByTitle.TryGetValue(candidate.Title, out var existing))
            {
                newEvents.Add(candidate);
                continue;
            }

            if (existing.StartsAt <= now)
            {
                existing.StartsAt = candidate.StartsAt;
                changed = true;
            }
        }

        if (newEvents.Count > 0)
        {
            context.Events.AddRange(newEvents);
            changed = true;
        }

        if (changed)
        {
            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedDemoDataAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        IWebHostEnvironment environment)
    {
        const string sellerAEmail = "sasha@demo.local";

        if (await userManager.FindByEmailAsync(sellerAEmail) != null)
        {
            return;
        }

        async Task<ApplicationUser> CreateDemoUserAsync(string email, string firstName, string lastName,
            string address, string city)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                Address = address,
                City = city
            };

            await userManager.CreateAsync(user, "Demo123!");
            return user;
        }

        var sellerA = await CreateDemoUserAsync(sellerAEmail, "Sasha", "Nikolov", "Partizanska 5", "Skopje");
        var sellerB = await CreateDemoUserAsync("elena@demo.local", "Elena", "Trajkovska", "Marshal Tito 22", "Bitola");
        var buyerA = await CreateDemoUserAsync("marko@demo.local", "Marko", "Ilievski", "Kliment Ohridski 8", "Ohrid");
        var buyerB = await CreateDemoUserAsync("bea@demo.local", "Bea", "Stojanova", "Goce Delcev 3", "Tetovo");

        Event? FindEvent(string title)
        {
            return context.Events.Local.FirstOrDefault(e => e.Title == title)
                   ?? context.Events.FirstOrDefault(e => e.Title == title);
        }

        var bjeloDugme = FindEvent("Bjelo Dugme Tribute Night")!;
        var summerRock = FindEvent("Summer Rock Festival - Day 1")!;
        var vardarPelister = FindEvent("Vardar vs Pelister")!;
        var mztRabotnicki = FindEvent("MZT Skopje vs Rabotnicki")!;
        var hamlet = FindEvent("Hamlet")!;
        var standUp = FindEvent("Stand-Up Comedy Gala")!;
        var kiril = FindEvent("Kiril Dzajkovski Live")!;
        var philharmonic = FindEvent("Philharmonic: Beethoven Cycle")!;
        var ohridOpening = FindEvent("Ohrid Summer Festival: Opening Night")!;
        var lakeJazz = FindEvent("Lake Stage Jazz Night")!;
        var bitolaFilm = FindEvent("Bitola Film Festival: Gala Screening")!;
        var cherryOrchard = FindEvent("The Cherry Orchard")!;
        var shkendija = FindEvent("Shkendija vs Rabotnicki")!;
        var balkanBrass = FindEvent("Balkan Brass Night")!;
        var newYear = FindEvent("New Year Concert")!;
        var prilepBlues = FindEvent("Prilep Blues Festival")!;
        var strumicaConcert = FindEvent("Strumica Summer Concert")!;
        var chopin = FindEvent("An Evening of Chopin")!;

        TicketListing MakeListing(Event listingEvent, ApplicationUser seller, decimal price, int quantity,
            string? section, string? row, ListingStatus status = ListingStatus.Active)
        {
            var listing = new TicketListing
            {
                EventId = listingEvent.Id,
                Event = listingEvent,
                SellerId = seller.Id,
                Seller = seller,
                PricePerTicket = price,
                Quantity = quantity,
                Section = section,
                Row = row,
                Status = status,
                CreatedAt = DateTime.Now.AddDays(-3)
            };

            context.TicketListings.Add(listing);
            return listing;
        }

        var lBjeloSasha = MakeListing(bjeloDugme, sellerA, 28.00m, 4, "Floor", null);
        var lBjeloElena = MakeListing(bjeloDugme, sellerB, 35.00m, 1, "A", "3");
        MakeListing(summerRock, sellerA, 45.00m, 3, null, null);
        var lVardarSasha = MakeListing(vardarPelister, sellerA, 15.00m, 3, "B", "10");
        MakeListing(vardarPelister, sellerB, 22.00m, 2, "VIP", "1");
        var lMztElena = MakeListing(mztRabotnicki, sellerB, 18.00m, 3, null, null);
        var lMztSoldOut = MakeListing(mztRabotnicki, sellerA, 19.00m, 0, null, null, ListingStatus.Sold);
        var lHamletSasha = MakeListing(hamlet, sellerA, 12.00m, 2, "Stalls", "F");
        MakeListing(hamlet, sellerB, 20.00m, 1, "Balcony", "A");
        var lStandUpSasha = MakeListing(standUp, sellerA, 25.00m, 2, null, null);
        var lKirilElena = MakeListing(kiril, sellerB, 30.00m, 1, "Floor", null);
        MakeListing(philharmonic, sellerA, 22.00m, 4, null, null);
        MakeListing(ohridOpening, sellerA, 40.00m, 4, null, null);
        MakeListing(ohridOpening, sellerB, 55.00m, 2, "VIP", null);
        MakeListing(lakeJazz, sellerA, 32.00m, 3, null, null);
        MakeListing(bitolaFilm, sellerB, 18.00m, 5, null, null);
        MakeListing(cherryOrchard, sellerA, 14.00m, 2, "Stalls", "C");
        MakeListing(shkendija, sellerB, 20.00m, 4, "C", "2");
        MakeListing(balkanBrass, sellerA, 24.00m, 3, null, null);
        MakeListing(balkanBrass, sellerB, 30.00m, 1, "Front Row", null);
        MakeListing(newYear, sellerA, 60.00m, 2, null, null);
        MakeListing(prilepBlues, sellerB, 22.00m, 3, null, null);
        MakeListing(strumicaConcert, sellerA, 27.00m, 2, null, null);
        MakeListing(chopin, sellerB, 35.00m, 1, null, null);

        await context.SaveChangesAsync();

        Order MakeOrder(TicketListing listing, ApplicationUser buyer, int quantity, OrderStatus status,
            DateTime orderedAt, DateTime? paidAt = null, DateTime? deliveredAt = null, DateTime? refundedAt = null,
            string? paymentReference = null)
        {
            var subtotal = quantity * listing.PricePerTicket;
            var order = new Order
            {
                BuyerId = buyer.Id,
                Buyer = buyer,
                TicketListingId = listing.Id,
                TicketListing = listing,
                Quantity = quantity,
                SubtotalPrice = subtotal,
                BuyerFee = FeeSchedule.BuyerFee(subtotal),
                SellerCommission = FeeSchedule.SellerCommission(subtotal),
                TotalPrice = subtotal + FeeSchedule.BuyerFee(subtotal),
                OrderedAt = orderedAt,
                PaidAt = paidAt,
                DeliveredAt = deliveredAt,
                RefundedAt = refundedAt,
                PaymentReference = paymentReference,
                OrderStatus = status
            };

            context.Orders.Add(order);
            return order;
        }

        var now = DateTime.Now;

        MakeOrder(lBjeloSasha, buyerA, 2, OrderStatus.Paid, now.AddDays(-2), paidAt: now.AddDays(-2),
            paymentReference: "SIM-DEMO000001");
        MakeOrder(lBjeloElena, buyerB, 1, OrderStatus.Pending, now.AddHours(-1));
        var deliveredOrder = MakeOrder(lVardarSasha, buyerA, 3, OrderStatus.Delivered, now.AddDays(-5),
            paidAt: now.AddDays(-5), deliveredAt: now.AddDays(-4), paymentReference: "SIM-DEMO000003");
        MakeOrder(lHamletSasha, buyerB, 2, OrderStatus.Refunded, now.AddDays(-6), paidAt: now.AddDays(-6),
            refundedAt: now.AddDays(-3), paymentReference: "SIM-DEMO000004");
        MakeOrder(lMztElena, buyerA, 1, OrderStatus.Cancelled, now.AddDays(-2));
        MakeOrder(lKirilElena, buyerB, 1, OrderStatus.Paid, now.AddDays(-1), paidAt: now.AddDays(-1),
            paymentReference: "SIM-DEMO000006");
        MakeOrder(lMztSoldOut, buyerB, 1, OrderStatus.Paid, now.AddDays(-1), paidAt: now.AddDays(-1),
            paymentReference: "SIM-DEMO000007");
        MakeOrder(lStandUpSasha, buyerA, 1, OrderStatus.Pending, now.AddMinutes(-30));

        await context.SaveChangesAsync();

        var ticketsFolder = Path.Combine(environment.ContentRootPath, "App_Data", "tickets");
        Directory.CreateDirectory(ticketsFolder);

        var storedName = $"{Guid.NewGuid():N}.png";
        var placeholderPng = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");
        await File.WriteAllBytesAsync(Path.Combine(ticketsFolder, storedName), placeholderPng);

        context.TicketFiles.Add(new TicketFile
        {
            OrderId = deliveredOrder.Id,
            FileName = "e-ticket.png",
            StoredName = storedName,
            ContentType = "image/png",
            SizeBytes = placeholderPng.Length,
            UploadedAt = deliveredOrder.DeliveredAt ?? now
        });

        await context.SaveChangesAsync();
    }

    public static async Task SeedOrdersForUserAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        IWebHostEnvironment environment, string buyerEmail)
    {
        var buyer = await userManager.FindByEmailAsync(buyerEmail);
        if (buyer == null)
        {
            return;
        }

        var alreadySeeded = await context.Orders
            .AnyAsync(o => o.BuyerId == buyer.Id && o.PaymentReference != null && o.PaymentReference.StartsWith("SIM-TIMON"));
        if (alreadySeeded)
        {
            return;
        }

        var sellerA = await userManager.FindByEmailAsync("sasha@demo.local");
        var sellerB = await userManager.FindByEmailAsync("elena@demo.local");
        if (sellerA == null || sellerB == null)
        {
            return;
        }

        var twelfthNight = await context.Events.FirstOrDefaultAsync(e => e.Title == "Twelfth Night");
        var idolFinal = await context.Events.FirstOrDefaultAsync(e => e.Title == "Macedonian Idol Live Final");
        var chamberEvening = await context.Events.FirstOrDefaultAsync(e => e.Title == "Ohrid Summer Festival: Chamber Evening");
        if (twelfthNight == null || idolFinal == null || chamberEvening == null)
        {
            return;
        }

        var now = DateTime.Now;

        var paidListing = new TicketListing
        {
            EventId = twelfthNight.Id,
            SellerId = sellerB.Id,
            PricePerTicket = 16.00m,
            Quantity = 3,
            Section = "Balcony",
            Row = "B",
            Status = ListingStatus.Active,
            CreatedAt = now.AddDays(-3)
        };

        var pendingListing = new TicketListing
        {
            EventId = idolFinal.Id,
            SellerId = sellerA.Id,
            PricePerTicket = 50.00m,
            Quantity = 0,
            Section = "Floor",
            Row = null,
            Status = ListingStatus.Sold,
            CreatedAt = now.AddDays(-1)
        };

        var deliveredListing = new TicketListing
        {
            EventId = chamberEvening.Id,
            SellerId = sellerB.Id,
            PricePerTicket = 45.00m,
            Quantity = 0,
            Section = null,
            Row = null,
            Status = ListingStatus.Sold,
            CreatedAt = now.AddDays(-8)
        };

        context.TicketListings.AddRange(paidListing, pendingListing, deliveredListing);
        await context.SaveChangesAsync();

        var paidSubtotal = 2 * paidListing.PricePerTicket;
        var paidOrder = new Order
        {
            BuyerId = buyer.Id,
            TicketListingId = paidListing.Id,
            Quantity = 2,
            SubtotalPrice = paidSubtotal,
            BuyerFee = FeeSchedule.BuyerFee(paidSubtotal),
            SellerCommission = FeeSchedule.SellerCommission(paidSubtotal),
            TotalPrice = paidSubtotal + FeeSchedule.BuyerFee(paidSubtotal),
            OrderedAt = now.AddDays(-2),
            PaidAt = now.AddDays(-2),
            PaymentReference = "SIM-TIMON000001",
            OrderStatus = OrderStatus.Paid
        };

        var pendingSubtotal = 1 * pendingListing.PricePerTicket;
        var pendingOrder = new Order
        {
            BuyerId = buyer.Id,
            TicketListingId = pendingListing.Id,
            Quantity = 1,
            SubtotalPrice = pendingSubtotal,
            BuyerFee = FeeSchedule.BuyerFee(pendingSubtotal),
            SellerCommission = FeeSchedule.SellerCommission(pendingSubtotal),
            TotalPrice = pendingSubtotal + FeeSchedule.BuyerFee(pendingSubtotal),
            OrderedAt = now.AddHours(-2),
            OrderStatus = OrderStatus.Pending
        };

        var deliveredSubtotal = 1 * deliveredListing.PricePerTicket;
        var deliveredOrder = new Order
        {
            BuyerId = buyer.Id,
            TicketListingId = deliveredListing.Id,
            Quantity = 1,
            SubtotalPrice = deliveredSubtotal,
            BuyerFee = FeeSchedule.BuyerFee(deliveredSubtotal),
            SellerCommission = FeeSchedule.SellerCommission(deliveredSubtotal),
            TotalPrice = deliveredSubtotal + FeeSchedule.BuyerFee(deliveredSubtotal),
            OrderedAt = now.AddDays(-8),
            PaidAt = now.AddDays(-8),
            DeliveredAt = now.AddDays(-7),
            PaymentReference = "SIM-TIMON000003",
            OrderStatus = OrderStatus.Delivered
        };

        context.Orders.AddRange(paidOrder, pendingOrder, deliveredOrder);
        await context.SaveChangesAsync();

        var ticketsFolder = Path.Combine(environment.ContentRootPath, "App_Data", "tickets");
        Directory.CreateDirectory(ticketsFolder);

        var storedName = $"{Guid.NewGuid():N}.png";
        var placeholderPng = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");
        await File.WriteAllBytesAsync(Path.Combine(ticketsFolder, storedName), placeholderPng);

        context.TicketFiles.Add(new TicketFile
        {
            OrderId = deliveredOrder.Id,
            FileName = "e-ticket.png",
            StoredName = storedName,
            ContentType = "image/png",
            SizeBytes = placeholderPng.Length,
            UploadedAt = deliveredOrder.DeliveredAt ?? now
        });

        await context.SaveChangesAsync();
    }
}
