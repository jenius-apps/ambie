using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface ISearchService
    {
        Task<IReadOnlyList<Sound>> SearchByNameAsync(string name);
    }
}