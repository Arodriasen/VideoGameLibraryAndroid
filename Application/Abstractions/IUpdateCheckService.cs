using System.Threading.Tasks;

namespace VideoGameLibraryAndroid.Application.Abstractions
{
    public record UpdateInfo(string Version, string Url);

    public interface IUpdateCheckService
    {
        Task<UpdateInfo?> CheckForUpdateAsync();
    }
}
