using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TPToolkitLib.MeshScene.Classes;

namespace TPToolkitLib.WorldObject.Definitions.RenderEntityFactories.RenderEntityFactoryClasses
{
    public class MeshAttribute : IValidatableObject
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "The MeshName is required, and cannot be an empty string.")]
        public string MeshName { get; set; }
        /// <summary>
        /// The MsbElement that corresponds to this MeshAttribute's MeshName. This is resolved after the MsbScene is loaded and the MeshName is matched to an element in the scene.
        /// </summary>
        public MsbElement? Mesh
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                if (value != null)
                    MeshName = value.Name;
                else
                    MeshName = string.Empty;
            }
        }
        public IList<MeshSubAttribute> MeshSubAttributes { get; } = [];
        public MeshAttribute(string meshName)
        {
            MeshName = meshName;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            int i = 0;
            try
            {
                for (i = 0; i < MeshSubAttributes.Count; i++)
                {
                    var meshSubAttribute = MeshSubAttributes[i];
                    Validator.ValidateObject(meshSubAttribute, new(meshSubAttribute), true);
                }
            }
            catch (Exception ex)
            {
                return [new($"MeshSubAttribute {i + 1}: {ex.Message}")];
            }
            return [ValidationResult.Success];
        }
    }
}
