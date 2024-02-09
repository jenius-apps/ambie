using System.Collections.Generic;
using System.Linq;
using System;

namespace AmbientSounds.Constants
{
    public static class IapConstants
    {
        public const string MsStoreAmbiePlusId = "ambieplus";
        public const string MsStoreAmbiePlusLifetimeId = "lifetimeambieplus";

        public static bool ContainsAmbiePlus(this IReadOnlyList<string> ids) => ids.ContainsId(MsStoreAmbiePlusId);

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

            if (iapId.Split('_') is [string id, string version, ..])
            {
                return int.TryParse(version, out int result)
                    ? (id, result)
                    : (id, 0);
            }

            return (iapId, 0);
        }

        private static bool ContainsId(this IReadOnlyList<string> ids, string id)
        {
            if (ids is null || ids.Count == 0)
            {
                return false;
            }

            return ids.Any(x => x.StartsWith(id, StringComparison.OrdinalIgnoreCase));
        }
    }
}
