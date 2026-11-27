using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using TPToolkitLib.Interfaces;
using TPToolkitLib.MeshScene.Classes;
using TPToolkitLib.Tools;
using TPToolkitLib.WorldObject.Enums;

namespace TPToolkitLib.WorldObject.Definitions.RenderEntityFactories.RenderEntityFactoryClasses.MeshAttributes
{
    public class DamageSection : MeshSubAttribute, IDependencyResolvable
    {
        public bool VitalToShip { get; set; }
        public bool VitalToMaxVelocity { get; set; }
        public bool VitalToManeuverability { get; set; }
        public bool VitalToMission { get; set; }
        public bool Swappable { get; set; }
        public Material Material { get; set; }
        public int Hitpoints { get; set; }
        public int VitalSectionCoreDamagePercent { get; set; }
        public IList<Critical> Criticals { get; } = [];
        public IList<string> Adjacents { get; } = [];
        /// <summary>
        /// Resolved by RenderEntityFactory
        /// </summary>
        public IList<MsbMesh> AdjacentElements { get; } = [];
        public string DebrisMeshScene { get; set; } = string.Empty;
        public MsbScene? DebrisMsbScene
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                if (value != null)
                    DebrisMeshScene = value.Name;
                else
                    DebrisMeshScene = string.Empty;
            }
        }
        [CustomValidation(typeof(DamageSection), nameof(ValidateDebrisFx))] // Should not be empty with a debrisMeshScene
        public string DebrisFx { get; set; } = string.Empty; // case sensitive
        public IList<DestructFx> DestructFxs { get; } = [];

        public static ValidationResult ValidateDebrisFx(object value, ValidationContext context)
        {
            if (context.ObjectInstance is DamageSection typedInstance)
            {
                if (string.IsNullOrWhiteSpace(typedInstance.DebrisFx) && (!string.IsNullOrWhiteSpace(typedInstance.DebrisMeshScene) || typedInstance.DebrisMsbScene != null))
                    return new("DebrisFx is required with a DebrisMeshScene.");
                else
                    return ValidationResult.Success;
            }
            else
                return new("DamageSection is invalid.");
        }

        public void ResolveDependency()
        {
            DebrisMsbScene = TPGameTool.MeshScenes.FirstOrDefault((ms) => ms.Name.Equals(DebrisMeshScene, StringComparison.OrdinalIgnoreCase));
        }
    }

    public class DestructFx
    {
        public string Effect { get; set; } = "none";
        public float Time { get; set; }
        public float Value { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}
