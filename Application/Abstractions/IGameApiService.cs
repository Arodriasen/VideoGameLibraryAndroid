using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VideoGameLibraryAndroid.Domain.Entities;

namespace VideoGameLibraryAndroid.Application.Abstractions
{
    public interface IGameApiService
    {
        void UpdateKeys(string scanDexToken, string igdbClientId, string igdbClientSecret,
                         string rawgApiKey, string theGamesDbApiKey);

        Task<List<Game>> SearchCandidatesByBarcodeAsync(string rawBarcode, CancellationToken ct = default);
        Task<List<Game>> SearchByNameCandidatesAsync(string name, CancellationToken ct = default);
        Task<byte[]?> DownloadCoverAsync(string url, CancellationToken ct = default);
    }
}
