using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface IFocusNotesService
    {
        Task<string> GetStoredNotesAsync();
        void UpdateNotes(string notes);
        Task SaveNotesToStorageAsync();
    }
}