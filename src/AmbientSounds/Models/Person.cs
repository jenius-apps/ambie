namespace AmbientSounds.Models
{
    /// <summary>
    /// Class representing a signed in user.
    /// </summary>
    public class Person
    {
        /// <summary>
        /// The person's first name.
        /// </summary>
        public string Firstname { get; set; } = "";

        /// <summary>
        /// The person's email.
        /// </summary>
        public string Email { get; set; } = "";

        /// <summary>
        /// A path to the person's picture.
        /// </summary>
        public string PicturePath { get; set; } = "";
    }
}
