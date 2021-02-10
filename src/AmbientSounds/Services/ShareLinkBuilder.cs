using Microsoft.Toolkit.Diagnostics;
using System.Collections.Generic;
using System;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Class for building the share sounds link.
    /// </summary>
    public class ShareLinkBuilder : IShareLinkBuilder
    {
        private readonly IMixMediaPlayerService _player;

        public ShareLinkBuilder(
            IMixMediaPlayerService mixMediaPlayerService)
        {
            Guard.IsNotNull(mixMediaPlayerService, nameof(mixMediaPlayerService));
            _player = mixMediaPlayerService;
        }

        /// <inheritdoc/>
        public string GetLink()
        {
            IList<string> soundIds = _player.GetActiveIds();
            var encodedIds = new string[soundIds.Count];
            for (int i = 0; i < soundIds.Count; i++)
            {
                encodedIds[i] = GuidEncoder.Encode(soundIds[i]);
            }
            return $"ambie://play?sounds={string.Join(",", encodedIds)}";
        }
    }


    public static class GuidEncoder
    {
        public static string Encode(string guidText)
        {
            Guid guid = new Guid(guidText);
            return Encode(guid);
        }

        public static string Encode(Guid guid)
        {
            string enc = Convert.ToBase64String(guid.ToByteArray());
            enc = enc.Replace("/", "_");
            enc = enc.Replace("+", "-");
            return enc.Substring(0, 22);
        }

        public static Guid Decode(string encoded)
        {
            encoded = encoded.Replace("_", "/");
            encoded = encoded.Replace("-", "+");
            byte[] buffer = Convert.FromBase64String(encoded + "==");
            return new Guid(buffer);
        }
    }
}
