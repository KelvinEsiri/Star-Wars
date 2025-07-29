namespace Star_Wars.Configuration
{
    public class DatabaseSeedingOptions
    {
        public const string SectionName = "DatabaseSeeding";

        /// <summary>
        /// Enable automatic database seeding
        /// </summary>
        public bool EnableAutoSeed { get; set; } = true;

        /// <summary>
        /// Seed database on application startup
        /// </summary>
        public bool SeedOnStartup { get; set; } = true;

        /// <summary>
        /// Force re-seeding even if data already exists
        /// </summary>
        public bool ForceReseed { get; set; } = false;
    }
}
