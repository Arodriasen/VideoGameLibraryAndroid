using System.Collections.Generic;
using VideoGameLibraryAndroid.Domain.Entities;

namespace VideoGameLibraryAndroid.Application.Abstractions
{
    public interface IExportService
    {
        void ExportToExcel(IEnumerable<Game> games, string filePath);
        void ExportToCsv(IEnumerable<Game> games, string filePath);
    }
}
