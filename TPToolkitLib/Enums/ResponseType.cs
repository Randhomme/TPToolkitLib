using System.ComponentModel.DataAnnotations;

namespace TPToolkitLib.Enums
{
    public enum ResponseType
    {
        [Display(Name = "Static Object")]
        StaticObject,
        Ship,
        Null,
        [Display(Name = "Space Object")]
        SpaceObject,
        Asteroid,
        Bullet,
        [Display(Name = "Defensive WeaponFire")]
        DefensiveWeaponFire,
        [Display(Name = "Attackable WeaponFire")]
        AttackableWeaponFire,
        Lifeboat,
    }
}
