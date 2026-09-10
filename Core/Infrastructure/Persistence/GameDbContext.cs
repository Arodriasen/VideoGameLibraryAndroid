using Microsoft.EntityFrameworkCore;
using VideoGameLibraryAndroid.Domain.Entities;

namespace VideoGameLibraryAndroid.Infrastructure.Persistence
{
    public class GameDbContext : DbContext
    {
        private readonly string _connectionString;

        public GameDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<Game> Games { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // EnableRetryOnFailure: Neon (como Azure SQL) puede tener fallos transitorios de
            // conexión mientras "despierta" el cómputo tras un periodo de inactividad.
            options.UseNpgsql(_connectionString, o => o.EnableRetryOnFailure());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Índice único FILTRADO (solo entre juegos activos, "DeletedDate" IS NULL) -- refleja
            // el esquema real de Neon tras la migración FilterBarcodeUniqueIndexByActiveGames del
            // escritorio (dueño del esquema; este proyecto no aplica migraciones, ver comentario
            // en GameRepository). Sin el filtro, un juego en la papelera seguía "ocupando" su
            // código de barras y volver a escanearlo antes de vaciarla o de que caducara (7 días)
            // hacía fallar el guardado con un DbUpdateException, aunque GetByBarcodeAsync (el
            // aviso de "ya lo tienes") sí ignora la papelera.
            modelBuilder.Entity<Game>()
                .HasIndex(g => g.Barcode)
                .IsUnique()
                .HasFilter("\"DeletedDate\" IS NULL");

            // Por defecto, Npgsql mapea DateTime a "timestamp with time zone" y exige Kind=Utc.
            // La app usa DateTime.Now (hora local) para estos dos campos, igual que hacía con
            // SQLite -- se mapean a "timestamp without time zone" para conservar ese comportamiento
            // sin tener que tocar la lógica de negocio (AddedDate/DeletedDate en GameRepository).
            modelBuilder.Entity<Game>().Property(g => g.AddedDate).HasColumnType("timestamp without time zone");
            modelBuilder.Entity<Game>().Property(g => g.DeletedDate).HasColumnType("timestamp without time zone");
        }
    }
}
