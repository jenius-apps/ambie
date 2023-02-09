using CommunityToolkit.Diagnostics;
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

        public async Task<string> GetStoredNotesAsync()
        {
            if (string.IsNullOrEmpty(_notes))
            {
                _notes = await _fileWriter.ReadAsync(NotesFileName);
            }

            return _notes;
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
