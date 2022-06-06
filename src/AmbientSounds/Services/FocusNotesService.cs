using Microsoft.Toolkit.Diagnostics;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public class FocusNotesService : IFocusNotesService
    {
        private const string NotesFileName = "focusNotes.txt";
        private readonly IFileWriter _fileWriter;
        private string _notes = string.Empty;

        public FocusNotesService(IFileWriter fileWriter)
        {
            Guard.IsNotNull(fileWriter, nameof(fileWriter));

            _fileWriter = fileWriter;
        }

        public Task<string> GetStoredNotesAsync()
        {
            return _fileWriter.ReadAsync(NotesFileName);
        }

        public void UpdateNotes(string notes)
        {
            _notes = notes;
        }

        public Task SaveNotesToStorageAsync()
        {
            return _fileWriter.WriteStringAsync(_notes, NotesFileName);
        }
    }
}
