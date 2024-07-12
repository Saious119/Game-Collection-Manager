using GameCollectionManagerAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCollectionManagerAPI.Services
{
    public interface IMetaCritic_Services
    {
        public Task<MetacriticData> getMetaCriticInfo(string gameName, string platform);
    }
}
