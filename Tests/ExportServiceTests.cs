using ClosedXML.Excel;
using VideoGameLibraryAndroid.Domain.Entities;
using VideoGameLibraryAndroid.Infrastructure.Files;

namespace VideoGameLibraryAndroid.Tests
{
    public class ExportServiceTests : IDisposable
    {
        private readonly List<string> _tempFiles = new();
        private readonly ExportService _service = new();

        private string TempPath(string extension)
        {
            var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.{extension}");
            _tempFiles.Add(path);
            return path;
        }

        public void Dispose()
        {
            foreach (var f in _tempFiles)
                if (File.Exists(f)) File.Delete(f);
        }

        private static Game NewGame(string title, string platform = "PC", string? barcode = null,
            int? year = null, int rating = 0, DateTime? addedDate = null) =>
            new()
            {
                Title = title,
                Platform = platform,
                Barcode = barcode,
                Year = year,
                Rating = rating,
                AddedDate = addedDate ?? DateTime.Now
            };

        [Fact]
        public void ExportToCsv_escribe_cabecera_y_una_fila_por_juego()
        {
            var path = TempPath("csv");
            var games = new[] { NewGame("Celeste", "Nintendo Switch", barcode: "111", year: 2018, rating: 5) };

            _service.ExportToCsv(games, path);

            var lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);
            Assert.Equal("sep=;", lines[0]);
            Assert.Contains("Código de Barras;Título;Plataforma", lines[1]);
            Assert.Contains("\"Celeste\"", lines[2]);
            Assert.Contains("\"Nintendo Switch\"", lines[2]);
        }

        [Fact]
        public void ExportToCsv_escapa_comillas_dobles_en_los_campos()
        {
            var path = TempPath("csv");
            var games = new[] { NewGame("El juego \"definitivo\"") };

            _service.ExportToCsv(games, path);

            var content = File.ReadAllText(path, System.Text.Encoding.UTF8);
            Assert.Contains("El juego \"\"definitivo\"\"", content);
        }

        [Fact]
        public void ExportToCsv_sin_puntuacion_deja_el_campo_vacio()
        {
            var path = TempPath("csv");
            var games = new[] { NewGame("Juego sin puntuar", rating: 0) };

            _service.ExportToCsv(games, path);

            var line = File.ReadAllLines(path, System.Text.Encoding.UTF8)[2];
            var fields = line.Split(';');
            Assert.Equal("\"\"", fields[7]); // columna Puntuación
        }

        [Fact]
        public void ExportToCsv_lista_vacia_solo_escribe_las_dos_lineas_de_cabecera()
        {
            var path = TempPath("csv");

            _service.ExportToCsv(Array.Empty<Game>(), path);

            var lines = File.ReadAllLines(path, System.Text.Encoding.UTF8);
            Assert.Equal(2, lines.Length);
        }

        [Fact]
        public void ExportToExcel_escribe_cabecera_y_una_fila_por_juego()
        {
            var path = TempPath("xlsx");
            var games = new[] { NewGame("Hollow Knight", "PC", barcode: "222", year: 2017, rating: 4) };

            _service.ExportToExcel(games, path);

            using var workbook = new XLWorkbook(path);
            var sheet = workbook.Worksheets.First();
            Assert.Equal("Título", sheet.Cell(1, 2).GetString());
            Assert.Equal("Hollow Knight", sheet.Cell(2, 2).GetString());
            Assert.Equal("PC", sheet.Cell(2, 3).GetString());
            Assert.Equal(2017, sheet.Cell(2, 7).GetValue<int>());
            Assert.Equal(4, sheet.Cell(2, 8).GetValue<int>());
        }

        [Fact]
        public void ExportToExcel_sin_año_ni_puntuacion_deja_las_celdas_vacias()
        {
            var path = TempPath("xlsx");
            var games = new[] { NewGame("Juego incompleto") };

            _service.ExportToExcel(games, path);

            using var workbook = new XLWorkbook(path);
            var sheet = workbook.Worksheets.First();
            Assert.True(sheet.Cell(2, 7).IsEmpty()); // Año
            Assert.True(sheet.Cell(2, 8).IsEmpty()); // Puntuación
        }

        [Fact]
        public void Exportar_a_CSV_y_reimportar_conserva_los_datos_principales()
        {
            var csvPath = TempPath("csv");
            var original = NewGame("Super Mario Odyssey", "Nintendo Switch", barcode: "333", year: 2017, rating: 5);

            _service.ExportToCsv(new[] { original }, csvPath);
            var reimported = Assert.Single(new ImportService().ParseFile(csvPath));

            Assert.Equal(original.Title, reimported.Title);
            Assert.Equal(original.Platform, reimported.Platform);
            Assert.Equal(original.Barcode, reimported.Barcode);
            Assert.Equal(original.Year, reimported.Year);
            Assert.Equal(original.Rating, reimported.Rating);
        }
    }
}
