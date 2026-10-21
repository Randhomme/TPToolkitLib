using System.ComponentModel.DataAnnotations;

namespace TPToolkitLib.Enums
{
    /// <summary>
    /// Represents the material used by a DamageSection in a world object file.
    /// </summary>
    public enum Material
    {
        [Display(Name = "Hull Wood")]
        HullWood,
        [Display(Name = "Dragon Scale")]
        DragonScale,
        [Display(Name = "Hull ReInforced Wood")]
        HullReInforcedWood,
        [Display(Name = "Wall Stone")]
        WallStone,
        [Display(Name = "Sail Cloth")]
        SailCloth,
        [Display(Name = "Hull Iron")]
        HullIron,
    }
}
