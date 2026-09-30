using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPToolkitLib.Enums;
using TPToolkitLib.Exceptions;
using TPToolkitLib.Interfaces;
using TPToolkitLib.MeshScene.Classes;
using TPToolkitLib.Utils;
using TPToolkitLib.WorldObject;
using TPToolkitLib.WorldObject.Definitions.AiEntityFactories;
using TPToolkitLib.WorldObject.Definitions.PhysicsFactories;
using TPToolkitLib.WorldObject.Definitions.RenderEntityFactories;
using TPToolkitLib.WorldObject.Definitions.RenderEntityFactories.RenderEntityFactoryClasses;
using TPToolkitLib.WorldObject.Definitions.RenderEntityFactories.RenderEntityFactoryClasses.MeshAttributes;

namespace TPToolkitLib
{
    public static class WotTool
    {
        private static readonly IList<IDependencyResolvable> dependencyResolvables = [];

        #region Public methods

        public static WorldObjectType WorldObjectFromWot(string wotFilePath, bool ignoreFormatError)
        {
            return ReadWot(wotFilePath, ignoreFormatError);
        }

        public static void WorldObjectToWot(string wotFilePath)
        {

        }

        #endregion

        #region Private methods

        private static WorldObjectType ReadWot(string wotFilePath, bool ignoreFormatError)
        {
            var wot = new WorldObjectType();
            using (var wotReader = new StreamReader(File.OpenRead(wotFilePath)))
            {
                string physicsDefinitionString, collisionDefinitionString, renderEntityDefinitionString, aiEntityDefinitionString, customInfoDefinitionString;
                physicsDefinitionString = collisionDefinitionString = renderEntityDefinitionString = aiEntityDefinitionString = customInfoDefinitionString = string.Empty;

                // Type String
                var line = wotReader.ReadLine().Substring(12).Trim('\'');
                if (string.IsNullOrEmpty(line))
                    throw new TPException("The world object type string should not be empty.");
                wot.Type = line;

                wotReader.ReadLine(); // ContData
                wotReader.ReadLine(); // Start section

                // 'Has' stuff, and for logic purpose, they need to match the type or be empty
                //HasPhysics String
                line = wotReader.ReadLine().Trim().Substring(18).Trim('\'');
                if (!string.IsNullOrEmpty(line)) // string empty means no definition
                {
                    if (!string.Equals(line, wot.Type, StringComparison.Ordinal))
                    {
                        throw new TPException($"HasPhysics '{line}' is invalid. Make sure it matches the Type of the world object.");
                    }
                    physicsDefinitionString = line;
                }

                //HasCollision String
                line = wotReader.ReadLine().Trim().Substring(20).Trim('\'');
                if (!string.IsNullOrEmpty(line)) // string empty means no definition
                {
                    if (!string.Equals(line, wot.Type, StringComparison.Ordinal))
                    {
                        throw new TPException($"HasCollision '{line}' is invalid. Make sure it matches the Type of the world object.");
                    }
                    collisionDefinitionString = line;
                }

                //HasRender String
                line = wotReader.ReadLine().Trim().Substring(17).Trim('\'');
                if (!string.IsNullOrEmpty(line)) // string empty means no definition
                {
                    if (!string.Equals(line, wot.Type, StringComparison.Ordinal))
                    {
                        throw new TPException($"HasRender '{line}' is invalid. Make sure it matches the Type of the world object.");
                    }
                    renderEntityDefinitionString = line;
                }

                //HasAI String
                line = wotReader.ReadLine().Trim().Substring(13).Trim('\'');
                if (!string.IsNullOrEmpty(line)) // string empty means no definition
                {
                    if (!string.Equals(line, wot.Type, StringComparison.Ordinal))
                    {
                        throw new TPException($"HasAI '{line}' is invalid. Make sure it matches the Type of the world object.");
                    }
                    aiEntityDefinitionString = line;
                }

                //HasCustomInfo String
                line = wotReader.ReadLine().Trim().Substring(21).Trim('\'');
                if (!string.IsNullOrEmpty(line)) // string empty means no definition
                {
                    if (!string.Equals(line, wot.Type, StringComparison.Ordinal))
                    {
                        throw new TPException($"HasCustomInfo '{line}' is invalid. Make sure it matches the Type of the world object.");
                    }
                    customInfoDefinitionString = line;
                }

                wotReader.ReadLine(); // End section

                wotReader.ReadLine(); // Definition String 'AIENTITYDEFINITION'
                line = wotReader.ReadLine().Substring(18).Trim('\''); // EntityType
                if (!string.IsNullOrEmpty(line))
                {
                    if (!string.Equals(line, aiEntityDefinitionString, StringComparison.Ordinal))
                    {
                        throw new TPException($"EntityType '{line}' for 'AIENTITYDEFINITION' is invalid. Make sure it matches the Type of the world object.");
                    }

                    // If not empty and valid, check the factory type
                    var factoryType = wotReader.ReadLine().Substring(19).Trim('\'');

                    ReadAiEntityDefinition(wotReader, wot, factoryType, aiEntityDefinitionString, ignoreFormatError);
                }

                wotReader.ReadLine(); // Definition String 'RENDERENTITYDEFINITION'
                line = wotReader.ReadLine().Substring(18).Trim('\''); // EntityType
                if (!string.IsNullOrEmpty(line))
                {
                    if (!string.Equals(line, renderEntityDefinitionString, StringComparison.Ordinal))
                    {
                        throw new TPException($"EntityType '{line}' for 'RENDERENTITYDEFINITION' is invalid. Make sure it matches the Type of the world object.");
                    }

                    // If not empty and valid, check the factory type
                    var factoryType = wotReader.ReadLine().Substring(19).Trim('\'');

                    ReadRenderEntityDefinition(wotReader, wot, factoryType, renderEntityDefinitionString, ignoreFormatError);
                }

                wotReader.ReadLine(); // Definition String 'PHYSICSDEFINITION'
                line = wotReader.ReadLine().Substring(18).Trim('\''); // EntityType
                if (!string.IsNullOrEmpty(line))
                {
                    if (!string.Equals(line, physicsDefinitionString, StringComparison.Ordinal))
                    {
                        throw new TPException($"EntityType '{line}' for 'PHYSICSDEFINITION' is invalid. Make sure it matches the Type of the world object.");
                    }

                    // If not empty and valid, check the factory type
                    var factoryType = wotReader.ReadLine().Substring(19).Trim('\'');

                    ReadPhysicsDefinition(wotReader, wot, factoryType, physicsDefinitionString, ignoreFormatError);
                }
            }
            return wot;
        }

        #endregion

        #region Definitions

        private static void ReadAiEntityDefinition(StreamReader wotReader, WorldObjectType wot, string factoryType, string aiEntityDefinitionString, bool ignoreFormatError)
        {
            wotReader.ReadLine(); // ContData
            wotReader.ReadLine(); // Start section

            wot.AiEntityDefinition = factoryType switch
            {
                "DragonAI" => ReadDragonAI(wotReader, aiEntityDefinitionString, ignoreFormatError),
                "IslandAI" => ReadIslandAI(wotReader, aiEntityDefinitionString, ignoreFormatError),
                "SpaceAnimalAI" => ReadSpaceAnimalAI(aiEntityDefinitionString),
                "SpaceObjectAI" => ReadSpaceObjectAI(aiEntityDefinitionString),
                "MineAI" => ReadMineAI(wotReader, aiEntityDefinitionString, ignoreFormatError),
                "GunAI" => ReadGunAI(wotReader, aiEntityDefinitionString, ignoreFormatError),
                "VolcanoAI" => ReadVolcanoAI(aiEntityDefinitionString),
                "ShipAI" => ReadShipAI(wotReader, aiEntityDefinitionString, ignoreFormatError),
                _ => throw new TPException($"FactoryType '{factoryType}' for 'AIENTITYDEFINITION' is invalid."),
            };
            wotReader.ReadLine(); // End section
        }

        private static void ReadRenderEntityDefinition(StreamReader wotReader, WorldObjectType wot, string factoryType, string renderEntityDefinitionString, bool ignoreFormatError)
        {
            wotReader.ReadLine(); // ContData
            wotReader.ReadLine(); // Start section

            switch (factoryType)
            {
                case "RenderEntityFactory":
                    var renderEntityFactory = ReadRenderEntityFactory(wotReader, renderEntityDefinitionString, ignoreFormatError);
                    wot.RenderEntityDefinition = renderEntityFactory;
                    dependencyResolvables.Add(renderEntityFactory);
                    break;
                default:
                    throw new TPException($"FactoryType '{factoryType}' for 'RENDERENTITYDEFINITION' is invalid.");
            }

            wotReader.ReadLine(); // End section
        }

        private static void ReadPhysicsDefinition(StreamReader wotReader, WorldObjectType wot, string factoryType, string physicsDefinitionString, bool ignoreFormatError)
        {
            wotReader.ReadLine(); // ContData
            wotReader.ReadLine(); // Start section

            switch (factoryType)
            {
                case "DragonPhysics":
                    wot.PhysicsDefinition = ReadDragonPhysics(wotReader, physicsDefinitionString, ignoreFormatError);
                    break;
                case "Whale Physics":
                    wot.PhysicsDefinition = ReadWhalePhysics(wotReader, physicsDefinitionString, ignoreFormatError);
                    break;
                case "SpaceObjectPhysics":
                    wot.PhysicsDefinition = ReadSpaceObjectPhysics(wotReader, physicsDefinitionString, ignoreFormatError);
                    break;
                case "ProjectilePhysics":
                    wot.PhysicsDefinition = ReadProjectilePhysics(wotReader, physicsDefinitionString, ignoreFormatError);
                    break;
                case "MinePhysics":
                    wot.PhysicsDefinition = ReadMinePhysics(wotReader, physicsDefinitionString, ignoreFormatError);
                    break;
                case "TorpedoPhysics":
                    wot.PhysicsDefinition = ReadTorpedoPhysics(wotReader, physicsDefinitionString, ignoreFormatError);
                    break;
                case "ShipDemo":
                    wot.PhysicsDefinition = ReadShipDemo(wotReader, physicsDefinitionString, ignoreFormatError);
                    break;
                default:
                    throw new TPException($"FactoryType '{factoryType}' for 'PHYSICSDEFINITION' is invalid.");
            }

            wotReader.ReadLine(); // End section
        }

        #endregion

        #region Factories

        private static DragonAI ReadDragonAI(StreamReader wotReader, string aiEntityDefinitionString, bool ignoreFormatError)
        {
            return new DragonAI(aiEntityDefinitionString)
            {
                DefaultSightRange = wotReader.ReadAndParseFloat("Default SightRange Float ", ignoreFormatError)
            };
        }

        private static IslandAI ReadIslandAI(StreamReader wotReader, string aiEntityDefinitionString, bool ignoreFormatError)
        {
            return new IslandAI(aiEntityDefinitionString)
            {
                DefaultSightRange = wotReader.ReadAndParseFloat("Default SightRange Float ", ignoreFormatError)
            };
        }

        private static SpaceAnimalAI ReadSpaceAnimalAI(string aiEntityDefinitionString)
        {
            return new SpaceAnimalAI(aiEntityDefinitionString);
        }

        private static SpaceObjectAI ReadSpaceObjectAI(string aiEntityDefinitionString)
        {
            return new SpaceObjectAI(aiEntityDefinitionString);
        }

        private static MineAI ReadMineAI(StreamReader wotReader, string aiEntityDefinitionString, bool ignoreFormatError)
        {
            return new MineAI(aiEntityDefinitionString)
            {
                DefaultSightRange = wotReader.ReadAndParseFloat("Default SightRange Float ", ignoreFormatError)
            };
        }

        private static GunAI ReadGunAI(StreamReader wotReader, string aiEntityDefinitionString, bool ignoreFormatError)
        {
            return new GunAI(aiEntityDefinitionString)
            {
                IsLobbingGun = wotReader.ReadAndParseBool("Is Lobbing Gun Bool ", ignoreFormatError),
                IsMineLayingGun = wotReader.ReadAndParseBool("Is MineLaying Gun Bool ", ignoreFormatError),
                IsTorpedoLauncherGun = wotReader.ReadAndParseBool("Is TorpedoLauncher Gun Bool ", ignoreFormatError),
                IsPointDefenseGun = wotReader.ReadAndParseBool("Is PointDefense Gun Bool ", ignoreFormatError)
            };
        }

        private static VolcanoAI ReadVolcanoAI(string aiEntityDefinitionString)
        {
            return new VolcanoAI(aiEntityDefinitionString);
        }

        private static ShipAI ReadShipAI(StreamReader wotReader, string aiEntityDefinitionString, bool ignoreFormatError)
        {
            return new ShipAI(aiEntityDefinitionString)
            {
                DefaultSightRange = wotReader.ReadAndParseFloat("Default SightRange Float ", ignoreFormatError)
            };
        }

        private static RenderEntityFactory ReadRenderEntityFactory(StreamReader wotReader, string renderEntityDefinitionString, bool ignoreFormatError)
        {
            var renderEntityFactory = new RenderEntityFactory(renderEntityDefinitionString)
            {
                Distant = wotReader.ReadAndParseBool("Distant Bool ", ignoreFormatError),
                MeshSceneName = wotReader.ReadString("Mesh Scene Name String "),
                RenderEffectName = wotReader.ReadString("Render Effect String "),
                Visible = wotReader.ReadAndParseBool("Visible Bool "),
            };

            ReadMeshAttributeManager(wotReader, renderEntityFactory.MeshAttributes);

            renderEntityFactory.RotationSpeed = wotReader.ReadAndParseFloat("ROTATIONSPEED Float ");

            return renderEntityFactory;
        }

        private static DragonPhysics ReadDragonPhysics(StreamReader wotReader, string physicsDefinitionString, bool ignoreFormatError)
        {
            return new DragonPhysics(physicsDefinitionString)
            {
                CenterOfMass = wotReader.ReadAndParseVector3("CenterOfMass Vector3", ignoreFormatError),
                Mass = wotReader.ReadAndParseFloat("Mass Float ", ignoreFormatError),
                MaxThrust = wotReader.ReadAndParseFloat("MaxThrust Float ", ignoreFormatError),
                MaxSpeed = wotReader.ReadAndParseFloat("MaxSpeed Float ", ignoreFormatError),
                RotationalFriction = wotReader.ReadAndParseFloat("RotationalFriction Float ", ignoreFormatError),
                MaxAngularAcceleration = wotReader.ReadAndParseFloat("MaxAngularAcceleration Float ", ignoreFormatError),
            };
        }

        private static SpaceObjectPhysics ReadSpaceObjectPhysics(StreamReader wotReader, string physicsDefinitionString, bool ignoreFormatError)
        {
            return new SpaceObjectPhysics(physicsDefinitionString)
            {
                CenterOfMass = wotReader.ReadAndParseVector3("CenterOfMass Vector3", ignoreFormatError),
                Mass = wotReader.ReadAndParseFloat("Mass Float ", ignoreFormatError),
                Acceleration = wotReader.ReadAndParseFloat("Acceleration Float ", ignoreFormatError),
            };
        }

        private static WhalePhysics ReadWhalePhysics(StreamReader wotReader, string physicsDefinitionString, bool ignoreFormatError)
        {
            return new WhalePhysics(physicsDefinitionString)
            {
                CenterOfMass = wotReader.ReadAndParseVector3("CenterOfMass Vector3", ignoreFormatError),
                Mass = wotReader.ReadAndParseFloat("Mass Float ", ignoreFormatError),
                MaxThrust = wotReader.ReadAndParseFloat("MaxThrust Float ", ignoreFormatError),
                MaxSpeed = wotReader.ReadAndParseFloat("MaxSpeed Float ", ignoreFormatError),
                RotationalFriction = wotReader.ReadAndParseFloat("RotationalFriction Float ", ignoreFormatError),
                MaxAngularAcceleration = wotReader.ReadAndParseFloat("MaxAngularAcceleration Float ", ignoreFormatError),
                MaxDivePitch = wotReader.ReadAndParseFloat("Max Dive Pitch Float ", ignoreFormatError),
                MaxClimbPitch = wotReader.ReadAndParseFloat("Max Climb Pitch Float ", ignoreFormatError),
                DiveTime = wotReader.ReadAndParseFloat("Dive Time Float ", ignoreFormatError),
            };
        }

        private static ProjectilePhysics ReadProjectilePhysics(StreamReader wotReader, string physicsDefinitionString, bool ignoreFormatError)
        {
            return new ProjectilePhysics(physicsDefinitionString)
            {
                CenterOfMass = wotReader.ReadAndParseVector3("CenterOfMass Vector3", ignoreFormatError),
                Mass = wotReader.ReadAndParseFloat("Mass Float ", ignoreFormatError),
            };
        }

        private static MinePhysics ReadMinePhysics(StreamReader wotReader, string physicsDefinitionString, bool ignoreFormatError)
        {
            return new MinePhysics(physicsDefinitionString)
            {
                CenterOfMass = wotReader.ReadAndParseVector3("CenterOfMass Vector3", ignoreFormatError),
                Mass = wotReader.ReadAndParseFloat("Mass Float ", ignoreFormatError),
                MaximumSpeed = wotReader.ReadAndParseFloat("Maximum speed Float ", ignoreFormatError),
            };
        }

        private static TorpedoPhysics ReadTorpedoPhysics(StreamReader wotReader, string physicsDefinitionString, bool ignoreFormatError)
        {
            return new TorpedoPhysics(physicsDefinitionString)
            {
                CenterOfMass = wotReader.ReadAndParseVector3("CenterOfMass Vector3", ignoreFormatError),
                Mass = wotReader.ReadAndParseFloat("Mass Float ", ignoreFormatError),
                MaxThrust = wotReader.ReadAndParseFloat("MaxThrust Float ", ignoreFormatError),
                MaxSpeed = wotReader.ReadAndParseFloat("MaxSpeed Float ", ignoreFormatError),
                RotationalFriction = wotReader.ReadAndParseFloat("RotationalFriction Float ", ignoreFormatError),
                MaxAngularAcceleration = wotReader.ReadAndParseFloat("MaxAngularAcceleration Float ", ignoreFormatError),
            };
        }

        private static ShipDemo ReadShipDemo(StreamReader wotReader, string physicsDefinitionString, bool ignoreFormatError)
        {
            return new ShipDemo(physicsDefinitionString)
            {
                CenterOfMass = wotReader.ReadAndParseVector3("CenterOfMass Vector3", ignoreFormatError),
                Mass = wotReader.ReadAndParseFloat("Mass Float ", ignoreFormatError),
                MaxThrust = wotReader.ReadAndParseFloat("MaxThrust Float ", ignoreFormatError),
                MaxSpeed = wotReader.ReadAndParseFloat("MaxSpeed Float ", ignoreFormatError),
                RotationalFriction = wotReader.ReadAndParseFloat("RotationalFriction Float ", ignoreFormatError),
                MaxAngularAcceleration = wotReader.ReadAndParseFloat("MaxAngularAcceleration Float ", ignoreFormatError),
            };
        }

        #endregion

        #region RenderEntityFactory specifics

        private static void ReadMeshAttributeManager(StreamReader wotReader, IList<MeshAttribute> meshAttributes)
        {
            wotReader.ReadLine(); // Mesh Attribute Manager
            wotReader.ReadLine(); // Start section
            var meshAttributesSize = wotReader.ReadAndParseInt("MeshAttributes - Size Int "); // MeshAttributes - Size Int
            for (int i = 0; i < meshAttributesSize; i++)
            {
                wotReader.ReadLine(); // MeshAttributes - Element
                wotReader.ReadLine(); // Start section
                var meshName = wotReader.ReadString("MeshName String "); // MeshName String

                if (string.IsNullOrEmpty(meshName))
                    throw new TPException($"MeshName of attribute {i} cannot be empty.");

                var meshAttribute = new MeshAttribute() { MeshName = meshName };

                ReadMeshAttribute(wotReader, meshAttribute);

                wotReader.ReadLine(); // End section

                meshAttributes.Add(meshAttribute);
            }
            wotReader.ReadLine(); // End section
        }

        private static void ReadMeshAttribute(StreamReader wotReader, MeshAttribute meshAttribute)
        {
            var meshSubAttributeSize = wotReader.ReadAndParseInt("MeshAttribute - Size Int "); // MeshAttribute - Size Int;
            for (int i = 0; i < meshSubAttributeSize; i++)
            {
                wotReader.ReadLine(); // MeshAttribute - Element
                wotReader.ReadLine(); // Start section
                var attributeName = wotReader.ReadString("AttributeName String ");
                switch (attributeName)
                {
                    case "GunPlacement":
                        var gunPlacement = ReadGunPlacement(wotReader, meshAttribute);
                        meshAttribute.MeshSubAttributes.Add(gunPlacement);
                        dependencyResolvables.Add(gunPlacement);
                        break;
                    case "DamageSection":
                        var damageSection = ReadDamageSection(wotReader, meshAttribute);
                        meshAttribute.MeshSubAttributes.Add(damageSection);
                        break;
                    case "MeshHitPoints":
                        var meshHitPoints = ReadMeshHitPoints(wotReader);
                        meshAttribute.MeshSubAttributes.Add(meshHitPoints);
                        dependencyResolvables.Add(meshHitPoints);
                        break;
                    case "DockPoint":
                        var dockPoint = ReadDockPoint(wotReader);
                        meshAttribute.MeshSubAttributes.Add(dockPoint);
                        break;
                    case "FlagAttachmentPoint":
                        var flagAttachmentPoint = ReadFlagAttachmentPoint(wotReader);
                        meshAttribute.MeshSubAttributes.Add(flagAttachmentPoint);
                        break;
                    case "PlayEffect":
                        var playEffect = ReadPlayEffect(wotReader);
                        meshAttribute.MeshSubAttributes.Add(playEffect);
                        break;
                    case "BoardingEffectPoint":
                        var boardingEffectPoint = ReadBoardingEffectPoint(wotReader);
                        meshAttribute.MeshSubAttributes.Add(boardingEffectPoint);
                        break;
                    case "VolcanoSmoke":
                        var volcanoSmoke = ReadVolcanoSmoke(wotReader);
                        meshAttribute.MeshSubAttributes.Add(volcanoSmoke);
                        break;
                    case "EnginePortPlacement":
                        var enginePortPlacement = ReadEnginePortPlacement(wotReader);
                        meshAttribute.MeshSubAttributes.Add(enginePortPlacement);
                        break;
                    case "GunMuzzlePlacement":
                        var gunMuzzlePlacement = ReadGunMuzzlePlacement(wotReader);
                        meshAttribute.MeshSubAttributes.Add(gunMuzzlePlacement);
                        break;
                    case "GunVerticalPivotPlacement":
                        var gunVerticalPivotPlacement = ReadGunVerticalPivotPlacement(wotReader);
                        meshAttribute.MeshSubAttributes.Add(gunVerticalPivotPlacement);
                        break;
                    case "WakePlacement":
                        var wakePlacement = ReadWakePlacement(wotReader);
                        meshAttribute.MeshSubAttributes.Add(wakePlacement);
                        break;
                    case "ToweePoint":
                        var toweePoint = ReadToweePoint(wotReader);
                        meshAttribute.MeshSubAttributes.Add(toweePoint);
                        break;
                    case "TowerPoint":
                        var towerPoint = ReadTowerPoint(wotReader);
                        meshAttribute.MeshSubAttributes.Add(towerPoint);
                        break;
                    case "TorpedoHomingPoint":
                        var torpedoHomingPoint = ReadTorpedoHomingPoint(wotReader);
                        meshAttribute.MeshSubAttributes.Add(torpedoHomingPoint);
                        break;
                    case "CloakingEffectPoint":
                        var cloakingEffectPoint = ReadCloakingEffectPoint(wotReader);
                        meshAttribute.MeshSubAttributes.Add(cloakingEffectPoint);
                        break;
                    case "FullyCloakedEffectPoint":
                        var fullyCloakedEffectPoint = ReadFullyCloakedEffectPoint(wotReader);
                        meshAttribute.MeshSubAttributes.Add(fullyCloakedEffectPoint);
                        break;
                    default:
                        throw new TPException($"The AttributeName '{attributeName}' of MeshAttribute '{meshAttribute.MeshName}' is invalid.");
                }
                wotReader.ReadLine(); // End section
            }
        }

        private static GunPlacement ReadGunPlacement(StreamReader wotReader, MeshAttribute meshAttribute)
        {
            var gunPlacement = new GunPlacement()
            {
                DescriptorName = wotReader.ReadString("DescriptorName String ")
            };
            var associationNameString = wotReader.ReadString("AssociationName String "); // Gun, Rotation, Bank
            var gunPlacementValues = associationNameString.Split(',');
            if (gunPlacementValues.Length != 3)
                throw new TPException($"The GunPlacement attribute of MeshAttribute '{meshAttribute.MeshName}' must have exactly 3 values in the AssociationName string, but found {gunPlacementValues.Length}.");
            gunPlacement.GunName = gunPlacementValues[0];
            gunPlacement.MaxRotation = float.Parse(gunPlacementValues[1]);
            if (Enum.TryParse<Bank>(gunPlacementValues[2], out var bank))
                gunPlacement.Bank = bank;
            else
                throw new TPException($"The Bank value '{gunPlacementValues[2]}' in the GunPlacement attribute of MeshAttribute '{meshAttribute.MeshName}' is invalid.");
            return gunPlacement;
        }

        private static DamageSection ReadDamageSection(StreamReader wotReader, MeshAttribute meshAttribute)
        {
            var damageSection = new DamageSection()
            {
                DescriptorName = wotReader.ReadString("DescriptorName String ")
            };
            var associationNameString = wotReader.ReadString("AssociationName String ");
            var associations = associationNameString.Split(',');
            var hasMaterial = false;
            for (int i = 0; i < associations.Length; i++)
            {
                var association = associations[i].Trim();
                var keyVal = association.Split('=');
                if (keyVal.Length != 2)
                    throw new TPException($"The AssociationName string in the DamageSection attribute of MeshAttribute '{meshAttribute.MeshName}' is invalid. Each association must be in the format 'key=value'.");
                var key = keyVal[0];
                var val = keyVal[1];
                switch (key)
                {
                    case "vitalToShip":
                        if (val == "0")
                            damageSection.VitalToShip = false;
                        else if (val == "1")
                            damageSection.VitalToShip = true;
                        else
                            throw new TPException($"The vitalToShip value '{val}' in the DamageSection attribute of MeshAttribute '{meshAttribute.MeshName}' is invalid.");
                        break;
                    case "vitalToMaxVelocity":
                        if (val == "0")
                            damageSection.VitalToMaxVelocity = false;
                        else if (val == "1")
                            damageSection.VitalToMaxVelocity = true;
                        else
                            throw new TPException($"The vitalToMaxVelocity value '{val}' in the DamageSection attribute of MeshAttribute '{meshAttribute.MeshName}' is invalid.");
                        break;
                    case "vitalToManeuverability":
                        if (val == "0")
                            damageSection.VitalToManeuverability = false;
                        else if (val == "1")
                            damageSection.VitalToManeuverability = true;
                        else
                            throw new TPException($"The vitalToManeuverability value '{val}' in the DamageSection attribute of MeshAttribute '{meshAttribute.MeshName}' is invalid.");
                        break;
                    case "vitalToMission":
                        if (val == "0")
                            damageSection.VitalToMission = false;
                        else if (val == "1")
                            damageSection.VitalToMission = true;
                        else
                            throw new TPException($"The vitalToMission value '{val}' in the DamageSection attribute of MeshAttribute '{meshAttribute.MeshName}' is invalid.");
                        break;
                    case "swappable":
                        if (val == "0")
                            damageSection.Swappable = false;
                        else if (val == "1")
                            damageSection.Swappable = true;
                        else
                            throw new TPException($"The swappable value '{val}' in the DamageSection attribute of MeshAttribute '{meshAttribute.MeshName}' is invalid.");
                        break;
                    case "material":
                        if (EnumExtensions.TryGetValueFromDisplayName<Material>(val, out var material))
                        {
                            damageSection.Material = material;
                            hasMaterial = true;
                        }
                        else
                        {
                            throw new TPException($"The material value '{val}' in the DamageSection attribute of MeshAttribute '{meshAttribute.MeshName}' is invalid.");
                        }
                        break;
                    case "hitpoints":
                        if (int.TryParse(val, out var hitpoints))
                            damageSection.Hitpoints = hitpoints;
                        else
                            throw new TPException($"The hitpoints value '{val}' in the DamageSection attribute of MeshAttribute '{meshAttribute.MeshName}' is invalid.");
                        break;
                    case "vitalSectionCoreDamagePercent":
                        if (int.TryParse(val, out var vitalSectionCoreDamagePercent))
                            damageSection.VitalSectionCoreDamagePercent = vitalSectionCoreDamagePercent;
                        else
                            throw new TPException($"The vitalSectionCoreDamagePercent value '{val}' in the DamageSection attribute of MeshAttribute '{meshAttribute.MeshName}' is invalid.");
                        break;
                    case "criticals":
                        var criticalsStrs = val.Split(['+'], StringSplitOptions.RemoveEmptyEntries);
                        for (int j = 0; j < criticalsStrs.Length; j++)
                        {
                            var criticalStr = criticalsStrs[j];
                            if (EnumExtensions.TryGetValueFromDisplayName<Critical>(criticalStr, out var critical))
                            {
                                if (!damageSection.Criticals.Contains(critical))
                                    damageSection.Criticals.Add(critical);
                            }
                            else
                                throw new TPException($"The critical value '{criticalStr}' in the DamageSection attribute of MeshAttribute '{meshAttribute.MeshName}' is invalid.");
                        }
                        break;
                    case "adjacent":
                        var adjacentsStrs = val.Split(['+'], StringSplitOptions.RemoveEmptyEntries);
                        for (int j = 0; j < adjacentsStrs.Length; j++)
                        {
                            var adjacentStr = adjacentsStrs[j];
                            if (!damageSection.Adjacents.Contains(adjacentStr))
                                damageSection.Adjacents.Add(adjacentStr);
                        }
                        break;
                    case "debrisMeshScene":
                        damageSection.DebrisMeshScene = val;
                        break;
                    case "debrisFx":
                        damageSection.DebrisFx = val;
                        break;
                    case "destructFx":
                        var destructFxStr = val.Trim('{', '}', ' '); // Format of DestructFx is 'Effect A B ( X Y Z )'
                        var destructFxParts = destructFxStr.Split([' '], 4, StringSplitOptions.RemoveEmptyEntries);
                        var destructFx = new DestructFx() { Effect = destructFxParts[0] };
                        if (destructFxParts.Length > 1 && float.TryParse(destructFxParts[1], out var a))
                            destructFx.A = a;
                        else
                            throw new TPException($"The destructFx value '{destructFxStr}' in the DamageSection attribute of MeshAttribute '{meshAttribute.MeshName}' is invalid.");
                        if (destructFxParts.Length > 2 && float.TryParse(destructFxParts[2], out var b))
                            destructFx.B = b;
                        else
                            throw new TPException($"The destructFx value '{destructFxStr}' in the DamageSection attribute of MeshAttribute '{meshAttribute.MeshName}' is invalid.");
                        if (destructFxParts.Length > 3)
                        {
                            var xyzStr = destructFxParts[3].Trim('(', ')', ' ');
                            var xyzParts = xyzStr.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                            if (xyzParts.Length != 3)
                                throw new TPException($"The destructFx value '{destructFxStr}' in the DamageSection attribute of MeshAttribute '{meshAttribute.MeshName}' is invalid.");
                            if (float.TryParse(xyzParts[0], out var x))
                                destructFx.X = x;
                            else
                                throw new TPException($"The destructFx value '{destructFxStr}' in the DamageSection attribute of MeshAttribute '{meshAttribute.MeshName}' is invalid.");
                            if (float.TryParse(xyzParts[1], out var y))
                                destructFx.Y = y;
                            else
                                throw new TPException($"The destructFx value '{destructFxStr}' in the DamageSection attribute of MeshAttribute '{meshAttribute.MeshName}' is invalid.");
                            if (float.TryParse(xyzParts[2], out var z))
                                destructFx.Z = z;
                            else
                                throw new TPException($"The destructFx value '{destructFxStr}' in the DamageSection attribute of MeshAttribute '{meshAttribute.MeshName}' is invalid.");
                        }
                        damageSection.DestructFxs.Add(destructFx);
                        break;
                    default:
                        throw new TPException($"The key '{key}' in the AssociationName string of the DamageSection attribute of MeshAttribute '{meshAttribute.MeshName}' is invalid.");
                }
            }
            if (!hasMaterial)
            {
                throw new TPException($"The DamageSection attribute of MeshAttribute '{meshAttribute.MeshName}' must have a 'material' association in the AssociationName string.");
            }
            return damageSection;
        }

        private static MeshHitPoints ReadMeshHitPoints(StreamReader wotReader)
        {
            var meshHitPoints = new MeshHitPoints()
            {
                DescriptorName = wotReader.ReadString("DescriptorName String "),
                AssociationName = wotReader.ReadString("AssociationName String ")
            };
            return meshHitPoints;
        }

        private static DockPoint ReadDockPoint(StreamReader wotReader)
        {
            var dockPoint = new DockPoint()
            {
                DescriptorName = wotReader.ReadString("DescriptorName String "),
            };
            wotReader.ReadLine(); // Skip AssociationName String
            return dockPoint;
        }

        private static FlagAttachmentPoint ReadFlagAttachmentPoint(StreamReader wotReader)
        {
            var flagAttachmentPoint = new FlagAttachmentPoint()
            {
                DescriptorName = wotReader.ReadString("DescriptorName String "),
            };
            wotReader.ReadLine(); // Skip AssociationName String
            return flagAttachmentPoint;
        }

        private static PlayEffect ReadPlayEffect(StreamReader wotReader)
        {
            var playEffect = new PlayEffect()
            {
                DescriptorName = wotReader.ReadString("DescriptorName String "),
                EffectName = wotReader.ReadString("AssociationName String ")
            };
            return playEffect;
        }

        private static BoardingEffectPoint ReadBoardingEffectPoint(StreamReader wotReader)
        {
            var boardingEffectPoint = new BoardingEffectPoint()
            {
                DescriptorName = wotReader.ReadString("DescriptorName String "),
            };
            wotReader.ReadLine(); // Skip AssociationName String
            return boardingEffectPoint;
        }

        private static VolcanoSmoke ReadVolcanoSmoke(StreamReader wotReader)
        {
            var volcanoSmoke = new VolcanoSmoke()
            {
                DescriptorName = wotReader.ReadString("DescriptorName String "),
            };
            wotReader.ReadLine(); // Skip AssociationName String
            return volcanoSmoke;
        }

        private static EnginePortPlacement ReadEnginePortPlacement(StreamReader wotReader)
        {
            var enginePortPlacement = new EnginePortPlacement()
            {
                DescriptorName = wotReader.ReadString("DescriptorName String "),
                EffectName = wotReader.ReadString("AssociationName String ")
            };
            return enginePortPlacement;
        }

        private static GunMuzzlePlacement ReadGunMuzzlePlacement(StreamReader wotReader)
        {
            var gunMuzzlePlacement = new GunMuzzlePlacement()
            {
                DescriptorName = wotReader.ReadString("DescriptorName String "),
            };
            wotReader.ReadLine(); // Skip AssociationName String
            return gunMuzzlePlacement;
        }

        private static GunVerticalPivotPlacement ReadGunVerticalPivotPlacement(StreamReader wotReader)
        {
            var gunVerticalPivotPlacement = new GunVerticalPivotPlacement()
            {
                DescriptorName = wotReader.ReadString("DescriptorName String "),
            };
            wotReader.ReadLine(); // Skip AssociationName String
            return gunVerticalPivotPlacement;
        }

        private static WakePlacement ReadWakePlacement(StreamReader wotReader)
        {
            var wakePlacement = new WakePlacement()
            {
                DescriptorName = wotReader.ReadString("DescriptorName String "),
                EffectName = wotReader.ReadString("AssociationName String ")
            };
            return wakePlacement;
        }

        private static ToweePoint ReadToweePoint(StreamReader wotReader)
        {
            var toweePoint = new ToweePoint()
            {
                DescriptorName = wotReader.ReadString("DescriptorName String "),
            };
            wotReader.ReadLine(); // Skip AssociationName String
            return toweePoint;
        }

        private static TowerPoint ReadTowerPoint(StreamReader wotReader)
        {
            var towerPoint = new TowerPoint()
            {
                DescriptorName = wotReader.ReadString("DescriptorName String "),
            };
            wotReader.ReadLine(); // Skip AssociationName String
            return towerPoint;
        }

        private static TorpedoHomingPoint ReadTorpedoHomingPoint(StreamReader wotReader)
        {
            var torpedoHomingPoint = new TorpedoHomingPoint()
            {
                DescriptorName = wotReader.ReadString("DescriptorName String "),
            };
            wotReader.ReadLine(); // Skip AssociationName String
            return torpedoHomingPoint;
        }

        private static CloakingEffectPoint ReadCloakingEffectPoint(StreamReader wotReader)
        {
            var cloakingEffectPoint = new CloakingEffectPoint()
            {
                DescriptorName = wotReader.ReadString("DescriptorName String "),
            };
            wotReader.ReadLine(); // Skip AssociationName String
            return cloakingEffectPoint;
        }

        private static FullyCloakedEffectPoint ReadFullyCloakedEffectPoint(StreamReader wotReader)
        {
            var fullyCloakedEffectPoint = new FullyCloakedEffectPoint()
            {
                DescriptorName = wotReader.ReadString("DescriptorName String "),
            };
            wotReader.ReadLine(); // Skip AssociationName String
            return fullyCloakedEffectPoint;
        }

        #endregion
    }
}
