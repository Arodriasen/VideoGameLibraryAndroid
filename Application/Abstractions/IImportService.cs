using System.Collections.Generic;
using VideoGameLibraryAndroid.Domain.Entities;

namespace VideoGameLibraryAndroid.Application.Abstractions
{
    public interface IImportService
    {
        List<string> ReadHeaders(string filePath);
        List<Game> ParseFile(string filePath, Dictionary<string, int> mapping);
        List<Game> ParseFile(string filePath);
    }
}
