using System.ComponentModel.DataAnnotations;

namespace TPToolkitLib.Enums
{
    public enum MountType
    {
        [Display(Name = "Light  Mount")]
        LightMount,
        [Display(Name = "Medium Mount")]
        MediumMount,
        [Display(Name = "Heavy  Mount")]
        HeavyMount,
        [Display(Name = "Non-Selectable Special Mount")]
        NonSelectableSpecialMount,
        [Display(Name = "Non-Selectable Light   Mount")]
        NonSelectableLightMount,
        [Display(Name = "Non-Selectable Medium  Mount")]
        NonSelectableMediumMount,
        [Display(Name = "Non-Selectable Heavy   Mount")]
        NonSelectableHeavyMount,
        [Display(Name = "Non-Selectable Dual Light  Mount")]
        NonSelectableDualLightMount,
        [Display(Name = "Non-Selectable Dual Medium Mount")]
        NonSelectableDualMediumMount,
        [Display(Name = "Non-Selectable Dual Heavy  Mount")]
        NonSelectableDualHeavyMount,
        [Display(Name = "Non-Selectable Dual Mega   Mount")]
        NonSelectableDualMegaMount,
    }
}
