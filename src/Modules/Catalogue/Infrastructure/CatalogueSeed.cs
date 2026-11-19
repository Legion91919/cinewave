using Cinewave.Catalogue.Contracts;
using Cinewave.Catalogue.Domain;

namespace Cinewave.Catalogue.Infrastructure;

/// <summary>Invented demo catalogue. Every title, person and licence here is fictional.</summary>
internal static class CatalogueSeed
{
    public const string DemoStream = "demo/testpattern";

    private static readonly string[] Everywhere = ["NG", "ZA", "KE", "GH", "SN", "GB", "US", "FR"];

    public static void Populate(InMemoryTitleRepository repo, DateTimeOffset now)
    {
        repo.AddCollection(new CatalogueCollection("new", "New this week", 1));
        repo.AddCollection(new CatalogueCollection("festival", "Festival favourites", 2));
        repo.AddCollection(new CatalogueCollection("comedies", "Comedies from Lagos to Lusaka", 3));
        repo.AddCollection(new CatalogueCollection("docs", "Documentaries", 4));
        repo.AddCollection(new CatalogueCollection("music", "Music and rhythm", 5));

        LicenceWindow[] Open(params string[] territories) =>
            territories.Select(t => new LicenceWindow(Territory.Parse(t), now.AddDays(-180), now.AddDays(365))).ToArray();

        void Add(string id, string name, int year, string country, int minutes, string genre, string synopsis, string director,
            int colour, string pattern, string[] collections, LicenceWindow[] windows, bool featured = false)
        {
            string[] palette = ["#7a2e1f", "#a5532a", "#3f5b3a", "#2c4a6b", "#6b3a5c", "#8a6a1f", "#1f5a5a", "#5a2f2f"];
            var art = new TitleArt([palette[colour % 8], palette[(colour + 3) % 8], palette[(colour + 5) % 8]], pattern);
            repo.Add(new Title(id, name, year, country, minutes, genre.Split(", "), synopsis, director, art, DemoStream, collections, featured, windows));
        }

        Add("the-salt-road", "The Salt Road", 2024, "Mali", 112, "Drama", "A caravan guide and a runaway archivist cross the desert with forty camels and one stolen reel of film.", "Aminata Kouyaté", 0, "dunes", ["new", "festival"], Open(Everywhere), featured: true);
        Add("nightbus-to-bulawayo", "Nightbus to Bulawayo", 2022, "Zimbabwe", 98, "Drama, Road movie", "Eleven passengers, one broken headlight and a driver who refuses to stop before dawn.", "Tendai Mlambo", 1, "stripes", ["new", "festival"], Open(Everywhere));
        Add("harmattan-letters", "Harmattan Letters", 2023, "Senegal", 104, "Drama", "Letters from a lighthouse keeper, read aloud on pirate radio, reach the woman he never met.", "Ousmane Faye", 2, "rings", ["festival"], Open(Everywhere));
        Add("red-earth-choir", "Red Earth Choir", 2022, "South Africa", 109, "Music, Drama", "A township choir enters the national finals with no conductor and one borrowed bus.", "Nomvula Dlamini", 3, "grid", ["festival", "music"], Open("ZA", "KE", "GB", "US"));
        Add("baobab-hours", "Baobab Hours", 2021, "Tanzania", 91, "Drama", "The village clock stops at 4:15, and nobody wants it fixed.", "Rehema Mushi", 4, "rings", ["festival"], Open(Everywhere));
        Add("twelve-bridges-of-kinshasa", "Twelve Bridges of Kinshasa", 2024, "DR Congo", 98, "Music, Drama", "A rumba bandleader has one night to cross the city and reunite the band that split twenty years ago.", "Patrice Ilunga", 5, "stripes", ["new", "music"], Open(Everywhere));
        Add("hyena-moon", "Hyena Moon", 2020, "Ethiopia", 102, "Thriller", "A night-shift taxi driver picks up a passenger who knows every turn before she does.", "Selam Bekele", 6, "dunes", ["festival"], Open(Everywhere));
        Add("dune-radio", "Dune Radio", 2023, "Niger", 101, "Documentary", "A teenager runs a radio station from a solar panel, a tent and a lot of nerve.", "Hadiza Moussa", 7, "grid", ["docs"], Open(Everywhere));
        Add("lagos-after-rain", "Lagos After Rain", 2024, "Nigeria", 101, "Comedy", "A wedding planner's worst weekend: three ceremonies, one flooded venue, no generator.", "Chidinma Okafor", 1, "stripes", ["new", "comedies"], Open("NG", "GH", "GB", "US"));
        Add("kente-and-code", "Kente & Code", 2023, "Ghana", 89, "Comedy", "Two weavers launch a pattern-sharing app and accidentally start a fashion war.", "Kwabena Asante", 2, "grid", ["comedies"], Open(Everywhere));
        Add("mama-ndege", "Mama Ndege", 2021, "Kenya", 96, "Comedy, Family", "A grandmother takes flying lessons in secret. Her family finds out at 2,000 feet.", "Wairimu Njoroge", 3, "rings", ["comedies"], Open(Everywhere));
        Add("gold-coast-static", "Gold Coast Static", 2024, "Ghana", 95, "Comedy", "A late-night radio phone-in goes wrong when the whole city calls in at once.", "Esi Mensah", 4, "stripes", ["comedies"],
            [new LicenceWindow(Territory.Parse("NG"), now.AddDays(-200), now.AddDays(-10)), .. Open("GH", "GB", "ZA")]);
        Add("sixteen-kilometres", "Sixteen Kilometres", 2022, "Kenya", 83, "Documentary", "The walk to school, filmed every day for one year.", "Otieno Achieng", 5, "dunes", ["docs"], Open(Everywhere));
        Add("the-last-ferry-to-ilha", "The Last Ferry to Ilha", 2023, "Mozambique", 88, "Documentary", "The final season of a ferry that has crossed the bay since 1961.", "Lurdes Macuácua", 6, "rings", ["docs"], Open(Everywhere));
        Add("tamarind-summer", "Tamarind Summer", 2021, "Senegal", 94, "Drama, Music", "Four cousins, one summer, and a sabar drum that won't stay quiet.", "Mariama Diallo", 7, "grid", ["music"], Open(Everywhere));
        Add("weavers-of-kano", "Weavers of Kano", 2024, "Nigeria", 79, "Documentary", "Three generations at the dye pits, filmed over one harmattan.", "Bello Garba", 0, "stripes", ["docs"],
            [new LicenceWindow(Territory.Parse("NG"), now.AddDays(14), now.AddDays(400)), .. Open("GB", "US")]);
    }
}
