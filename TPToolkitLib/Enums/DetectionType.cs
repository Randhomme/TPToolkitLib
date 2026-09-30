using System.ComponentModel.DataAnnotations;

namespace TPToolkitLib.Enums
{
    public enum DetectionType
    {
        BoundingRenderEntity,
        Null,
        OBBTree,
        Point,
        [Display(Name = "Defensive WeaponFire")]
        DefensiveWeaponFire,
        [Display(Name = "Attackable WeaponFire")]
        AttackableWeaponFire,
    }
}
