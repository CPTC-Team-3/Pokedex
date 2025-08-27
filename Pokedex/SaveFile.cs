using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokedex
{
/// <summary>
/// Represents a saved file associated with a user, including metadata such as the save date and name.
/// </summary>
/// <remarks>This class is typically used to store and retrieve information about user-specific saved files. Each
/// instance uniquely identifies a saved file through its <see cref="SaveFileId"/>.</remarks>
    public class SaveFile
    {
        public int SaveFileId { get; set; }

        public int UserId { get; set; }

        public DateTime SaveDate { get; set; }

        public required string SaveName { get; set; }
    }
}
