using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories
{
    public class FocusTaskRepository : IFocusTaskRepository
    {
        private const string TasksFilename = "focusTasks.json";
        private readonly IFileWriter _fileWriter;

        public FocusTaskRepository(IFileWriter fileWriter)
        {
            Guard.IsNotNull(fileWriter, nameof(fileWriter));
            _fileWriter = fileWriter;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<FocusTask>> GetTasksAsync()
        {
            var content = await _fileWriter.ReadAsync(TasksFilename);
            if (string.IsNullOrEmpty(content))
            {
                return Array.Empty<FocusTask>();
            }

            try
            {
                var result = JsonSerializer.Deserialize(content, AmbieJsonSerializerContext.Default.FocusTaskArray);
                return result ?? Array.Empty<FocusTask>();
            }
            catch
            {
                return Array.Empty<FocusTask>();
            }
        }

        /// <inheritdoc/>
        public async Task SaveTasksAsync(IEnumerable<FocusTask> tasks)
        {
            if (tasks is null)
            {
                return;
            }

            await _fileWriter.WriteStringAsync(JsonSerializer.Serialize(tasks, AmbieJsonSerializerContext.Default.IEnumerableFocusTask), TasksFilename);
        }
    }
}
