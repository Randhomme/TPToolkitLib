using System.ComponentModel.DataAnnotations;

namespace TPToolkitLib.WorldObject.Enums
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
