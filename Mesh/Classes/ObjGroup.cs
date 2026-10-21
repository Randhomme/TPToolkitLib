using System.Collections.Generic;

namespace TPToolkitLib.Mesh.Classes
{
    public class ObjGroup
    {
        public string GroupName { get; set; }
        public IList<ObjMaterialGroup> MaterialGroups { get; }
        public ObjGroup(string groupName)
        {
            GroupName = groupName;
            MaterialGroups = [];
        }
        public override string ToString()
        {
            return GroupName;
        }
    }
}
