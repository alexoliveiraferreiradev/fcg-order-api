using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fcg.Orders.Domain.ReadModels
{
    public class GameSnapshot
    {
        public Guid GameId { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public DateTime LastSyncedAt { get; private set; }

        public void Apply(string name, string description, DateTime occuredAt)
        {
            if (occuredAt < LastSyncedAt) return;
            Name = name;
            Description = description;
            LastSyncedAt = occuredAt;
        }
    }
}
