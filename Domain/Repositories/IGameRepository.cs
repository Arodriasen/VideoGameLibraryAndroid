using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoGameLibraryAndroid.Domain.Entities;

namespace VideoGameLibraryAndroid.Domain.Repositories
{
    public interface IGameRepository : IDisposable
    {
        // Días que un juego permanece en la papelera antes de borrarse para siempre.
        // Usado tanto por PurgeExpiredTrashAsync como por la UI de la papelera (cuenta atrás por juego).
        public const int TrashRetentionDays = 7;

        // Se dispara tras cualquier escritura que cambie de verdad los datos (Add/Update/Delete/
        // Restore/PermanentlyDelete/Import, y Purge solo si borró algo). MainViewModel lo usa para
        // saber si de verdad hace falta recargar la lista al volver a MainPage -- antes recargaba
        // SIEMPRE en cada OnAppearing, lo que reconstruía el CollectionView entero (Clear + Add de
        // cada tarjeta) y perdía la posición de scroll incluso si solo se había entrado a ver la
        // ficha de un juego sin cambiar nada.
        event Action? DataChanged;

        Task<string> GetCollectionNameAsync();
        Task SetCollectionNameAsync(string name);

        Task<List<Game>> GetAllAsync();
        Task<Game?> GetByBarcodeAsync(string barcode);
        Task AddAsync(Game game);
        Task UpdateAsync(Game game);
        Task DeleteAsync(int id);
        Task RestoreAsync(int id);
        Task<List<Game>> GetTrashAsync();
        Task PermanentlyDeleteAsync(int id);
        Task<int> PurgeExpiredTrashAsync(int retentionDays = TrashRetentionDays);
        Task<(int Added, int Duplicates)> ImportAsync(IEnumerable<Game> games);
    }
}
