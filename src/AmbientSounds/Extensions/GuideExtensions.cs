using AmbientSounds.Models;
using System.Linq;

namespace AmbientSounds.Extensions;

public static class GuideExtensions
{
    public static string UpperCaseCulture(this Guide guide)
    {
        return guide.Culture.ToUpper();
    }
}
