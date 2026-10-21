using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPToolkitLib.WorldObject.Definitions.AiEntityFactories
{
    public class DragonAI : AiEntityDefinition
    {
        public float DefaultSightRange { get; set; }
        public DragonAI(string type) : base(type)
        {
            
        }
    }
}
