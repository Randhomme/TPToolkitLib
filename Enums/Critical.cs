using System.ComponentModel.DataAnnotations;

namespace TPToolkitLib.Enums
{
    public enum Critical
    {
        [Display(Name = "Cargo Hold")]
        CargoHold,
        [Display(Name = "Power Transformer Guns")]
        PowerTransformerGuns,
        [Display(Name = "Power Transformer Engines")]
        PowerTranformerEngines,
        [Display(Name = "Engines")]
        Engines,
        [Display(Name = "Rudder Linkage")]
        RudderLinkage,
        [Display(Name = "Spotter Equitment")]
        SpotterEquitment,
        [Display(Name = "MaxVelocity")]
        MaxVelocity,
        [Display(Name = "Munnitions")]
        Munnitions,
        [Display(Name = "Lower Deck Guns")]
        LowerDeckGuns,
        [Display(Name = "Cloaking Generator")]
        CloakingGenerator,
    }
}
