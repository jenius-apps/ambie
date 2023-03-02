using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class CatalogueService : ICatalogueService
{
    // TODO flesh out with cache and repository
    // TODO migrate online sound provider to cache and repo pattern
    public async Task<IReadOnlyList<CatalogueRow>> GetCatalogueRowsAsync()
    {
        await Task.Delay(1);

        return new List<CatalogueRow>()
        {
            new CatalogueRow
            {
                Name = "Water",
                SoundIds = new List<string>()
                {
                    "d8383cc9-69ce-4adf-a02a-d0ba28f99f8e",
                    "d68cb3c9-9bdb-4b9a-922b-10fc790b6b5b",
                    "e729be45-eb21-4ef5-9daf-eecc0c2863be",
                    "eab374d3-92d5-43c9-9e12-b3160405143e",
                    "b1f65785-511b-4dca-bd5d-b996ffb5b645",
                    "c4f13a49-971b-4983-8ba1-a59a10415089"
                }
            },
            new CatalogueRow
            {
                Name = "Binaural beats",
                SoundIds = new List<string>()
                {
                    "5abb5a12-a77d-4287-bd88-76c6e8247493",
                    "5102ab66-7e74-4d2b-b040-a01ef5bea47f",
                    "e807d413-34fa-46e0-bb74-e79198e19122",
                    "1a6ec03a-bb80-42c4-af17-75e8bec658d1",
                    "300beceb-a956-48ad-b896-0364e69e70c4"
                }
            }
        };
    }
}
