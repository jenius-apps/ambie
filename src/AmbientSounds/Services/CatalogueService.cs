using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class CatalogueService : ICatalogueService
{
    public async Task<IReadOnlyList<CatalogueRow>> GetCatalogueRowsAsync()
    {
        await Task.Delay(1);

        return new List<CatalogueRow>()
        {
            new CatalogueRow
            {
                Name = "Featured",
                SoundIds = new List<string>()
                {
                    "test"
                }
            },
            new CatalogueRow
            {
                Name = "Featured",
                SoundIds = new List<string>()
                {
                    "test"
                }
            },
            new CatalogueRow
            {
                Name = "Featured",
                SoundIds = new List<string>()
                {
                    "test"
                }
            },
            new CatalogueRow
            {
                Name = "Featured",
                SoundIds = new List<string>()
                {
                    "test"
                }
            },
        };
    }
}
