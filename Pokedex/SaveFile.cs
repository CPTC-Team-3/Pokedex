using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pokedex
{

    public class SaveFile
    {
        public int SaveFileId { get; set; }

        public int UserId { get; set; }

        public DateTime SaveDate { get; set; }

        public required string SaveName { get; set; }
    }
}
