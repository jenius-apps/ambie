using System.Collections.Generic;
using System.Linq;
using System;

namespace AmbientSounds.Constants
{
    public static class IapConstants
    {
        public const string MsStoreAmbiePlusId = "ambieplus";
        public const string MsStoreFreeRotationId = "free";

        public static bool ContainsFreeId(this IReadOnlyList<string> ids) => ids.ContainsId(MsStoreFreeRotationId);

        public static bool ContainsAmbiePlus(this IReadOnlyList<string> ids) => ids.ContainsId(MsStoreAmbiePlusId);

        private static bool ContainsId(this IReadOnlyList<string> ids, string id)
        {
            if (ids is null || ids.Count == 0)
            {
                return false;
            }

            return ids.Any(x => x.StartsWith(id, StringComparison.OrdinalIgnoreCase));
        }

        public static bool ContainsAmbiePlus(this string id)
        {
            if (id is null)
            {
                return false;
            }

            return id.StartsWith(MsStoreAmbiePlusId);
        }

        public static (string, int) SplitIdAndVersion(this string iapId)
        {
            if (string.IsNullOrEmpty(iapId))
            {
                return (string.Empty, 0);
            }

            var split = iapId.Split('_');
            if (split.Length <= 1)
            {
                return (iapId, 0);
            }
            else
            {
                return int.TryParse(split[1], out int result)
                    ? (split[0], result)
                    : (split[0], 0);
            }
        }
    }
}
