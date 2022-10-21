using System.Collections.Generic;
using System.Linq;

namespace AmbientSounds.Constants
{
    public class IapConstants
    {
        public const string MsStoreAmbiePlusId = "ambieplus";

        public static bool ContainsAmbiePlus(IReadOnlyList<string> ids)
        {
            if (ids is null || ids.Count == 0)
            {
                return false;
            }

            return ids.Any(x => x.StartsWith(MsStoreAmbiePlusId));
        }
    }
}
