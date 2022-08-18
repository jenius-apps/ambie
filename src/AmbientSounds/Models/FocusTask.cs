using System;

namespace AmbientSounds.Models
{
    public class FocusTask
    {
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Bool indicating if the task has been completed.
        /// </summary>
        public bool Completed { get; set; }

        /// <summary>
        /// Date when the task was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The text for the task. 
        /// </summary>
        public string Text { get; set; } = string.Empty;
    }
}
