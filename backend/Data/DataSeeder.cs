using HongarijePlanner.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace HongarijePlanner.Api.Data;

public static class DataSeeder
{
    private const string AdminName = "Yana";
    private const string HouseLink = "https://www.interhome.be/rental/9fef3e704583bf14177d5eae79bb0efd";

    // Called once at startup — seeds info cards, shared packing, and the admin user.
    public static async Task SeedGlobalDataAsync(AppDbContext db)
    {
        await SeedAdminUserAsync(db);
        await SeedInfoAsync(db);
        await SeedSharedPackingAsync(db);
    }

    private static async Task SeedAdminUserAsync(AppDbContext db)
    {
        var admin = await db.Users.FindAsync(AdminName);
        if (admin is null)
        {
            db.Users.Add(new User { Name = AdminName, IsAdmin = true });
            await db.SaveChangesAsync();
        }
        else if (!admin.IsAdmin)
        {
            admin.IsAdmin = true;
            await db.SaveChangesAsync();
        }
    }

    // Called when a user logs in for the first time — seeds their personal packing list.
    public static async Task SeedPackingForUserAsync(AppDbContext db, string userName)
    {
        if (await db.PackingItems.AnyAsync(p => p.Owner == userName && p.IsDefault))
            return;

        db.PackingItems.AddRange(DefaultPackingItems.Select(item => new PackingItem
        {
            Id    = NewId(),
            Title    = item.Title,
            Category = item.Category,
            Owner    = userName,
            IsPacked = false,
            IsDefault = true,
        }));

        await db.SaveChangesAsync();
    }

    private static async Task SeedInfoAsync(AppDbContext db)
    {
        if (await db.InfoItems.AnyAsync()) return;

        db.InfoItems.AddRange(
            new InfoItem
            {
                Id       = NewId(),
                Category = "Verblijf",
                Title    = "Appartement Moni 1",
                Body     = "Balatonszemes, 7 40, 8636 Hongarije\n\n" +
                           "Verblijf: 8 nachten\n" +
                           "Inchecken: 22-7-2026 vanaf 16u (woensdag)\n" +
                           "Uitchecken: 30-7-2026 ten laatste 10u (donderdag)\n\n" +
                           "Reserveringsnummer: 606026101376\n" +
                           "WiFi-wachtwoord: xxx",
                Link     = HouseLink,
                Special  = "maps",
            },
            new InfoItem
            {
                Id       = NewId(),
                Category = "Vignetten",
                Title    = "Tolwegen & vignetten",
                Special  = "vignets",
            },
            new InfoItem
            {
                Id       = NewId(),
                Category = "Valuta",
                Title    = "Euro ↔ HUF calculator",
                Special  = "currency",
            }
        );

        await db.SaveChangesAsync();
    }

    private static async Task SeedSharedPackingAsync(AppDbContext db)
    {
        if (await db.SharedPackingItems.AnyAsync()) return;

        string[] titles = ["Wasrek", "Wasknijpers", "Wasmiddel", "Allesreiniger",
                           "Sponzen", "Diepvrieszakjes", "Aluminiumfolie", "Plakband", "Schaar"];

        db.SharedPackingItems.AddRange(titles.Select(title => new SharedPackingItem
        {
            Id      = NewId(),
            Title   = title,
            AddedBy = AdminName,
        }));

        await db.SaveChangesAsync();
    }

    private static string NewId() => Guid.NewGuid().ToString("N")[..8];

    private static readonly (string Title, string Category)[] DefaultPackingItems =
    [
        ("Oplader gsm",                    "Essentieel"),
        ("Powerbank",                      "Essentieel"),
        ("Zonnebril",                      "Essentieel"),
        ("Zonnebrandcrème",                "Essentieel"),
        ("Portfeuille",                    "Essentieel"),
        ("T-shirts",                       "Kleding"),
        ("Broeken / shorts",               "Kleding"),
        ("Trui",                           "Kleding"),
        ("Regenjas",                       "Kleding"),
        ("Ondergoed",                      "Kleding"),
        ("Sokken",                         "Kleding"),
        ("Comfortabele wandelschoenen",    "Kleding"),
        ("Sandalen of slippers",           "Kleding"),
        ("Pyjama",                         "Kleding"),
        ("Zwemkledij",                     "Kleding"),
        ("Petje of zonnehoed",             "Kleding"),
        ("Identiteitskaart",               "Documenten"),
        ("Europese ziekteverzekeringskaart", "Documenten"),
        ("Vignet aangevraagd",             "Documenten"),
        ("Rijbewijs",                      "Documenten"),
        ("Boek of e-reader",               "Spullen & activiteiten"),
        ("Kaartspel of reisspelletje",     "Spullen & activiteiten"),
        ("Oortjes of koptelefoon",         "Spullen & activiteiten"),
        ("Persoonlijke medicatie",         "Apotheek & gezondheid"),
        ("Pijnstillers",                   "Apotheek & gezondheid"),
        ("Pleisters",                      "Apotheek & gezondheid"),
        ("Muggenspray",                    "Apotheek & gezondheid"),
        ("Aftersun",                       "Apotheek & gezondheid"),
        ("Maandverband / tampons",         "Apotheek & gezondheid"),
        ("Snacks voor onderweg",           "Overige"),
        ("Herbruikbare fles",              "Overige"),
        ("Slaapmasker",                    "Overige"),
        ("Badhanddoeken",                  "Overige"),
        ("Strandlaken",                    "Overige"),
    ];
}
