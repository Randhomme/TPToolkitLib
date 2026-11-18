using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TPToolkitLib.WorldObject.Definitions;

namespace TPToolkitLib.WorldObject
{
    public class WorldObjectType : IValidatableObject
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "The Type is required, and cannot be an empty string.")]
        public string Type { get; set; } = string.Empty;
        public AiEntityDefinition? AiEntityDefinition { get; set; }
        public RenderEntityDefinition? RenderEntityDefinition { get; set; }
        public PhysicsDefinition? PhysicsDefinition { get; set; }
        public CollisionDefinition? CollisionDefinition { get; set; }
        public CustomInfoDefinition? CustomInfoDefinition { get; set; }

        public override string ToString()
        {
            return Type;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AiEntityDefinition != null)
            {
                try
                {
                    Validator.ValidateObject(AiEntityDefinition, new(AiEntityDefinition));
                }
                catch (Exception ex)
                {
                    return [new($"AiEntityDefinition: {ex.Message}")];
                }
            }
            if (RenderEntityDefinition != null)
            {
                try
                {
                    Validator.ValidateObject(RenderEntityDefinition, new(RenderEntityDefinition));
                }
                catch (Exception ex)
                {
                    return [new($"RenderEntityDefinition: {ex.Message}")];
                }
            }
            if (PhysicsDefinition != null)
            {
                try
                {
                    Validator.ValidateObject(PhysicsDefinition, new(PhysicsDefinition));
                }
                catch (Exception ex)
                {
                    return [new($"PhysicsDefinition: {ex.Message}")];
                }
            }
            if (CollisionDefinition != null)
            {
                try
                {
                    Validator.ValidateObject(CollisionDefinition, new(CollisionDefinition));
                }
                catch (Exception ex)
                {
                    return [new($"CollisionDefinition: {ex.Message}")];
                }
            }
            if (CustomInfoDefinition != null)
            {
                try
                {
                    Validator.ValidateObject(CustomInfoDefinition, new(CustomInfoDefinition));
                }
                catch (Exception ex)
                {
                    return [new($"CustomInfoDefinition: {ex.Message}")];
                }
            }
            return [ValidationResult.Success];
        }
    }
}
