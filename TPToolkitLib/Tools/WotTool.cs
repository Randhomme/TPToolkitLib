using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TPToolkitLib.Enums;
using TPToolkitLib.Exceptions;
using TPToolkitLib.Interfaces;
using TPToolkitLib.Utils;
using TPToolkitLib.WorldObject;
using TPToolkitLib.WorldObject.Definitions;
using TPToolkitLib.WorldObject.Definitions.AiEntityFactories;
using TPToolkitLib.WorldObject.Definitions.CollisionFactories;
using TPToolkitLib.WorldObject.Definitions.CustomInfoFactories;
using TPToolkitLib.WorldObject.Definitions.CustomInfoFactories.Classes;
using TPToolkitLib.WorldObject.Definitions.PhysicsFactories;
using TPToolkitLib.WorldObject.Definitions.RenderEntityFactories;
using TPToolkitLib.WorldObject.Definitions.RenderEntityFactories.RenderEntityFactoryClasses;
using TPToolkitLib.WorldObject.Definitions.RenderEntityFactories.RenderEntityFactoryClasses.MeshAttributes;

namespace TPToolkitLib.Tools
{
    public static class WotTool
    {
        private static readonly IList<IDependencyResolvable> dependencyResolvables = [];

        #region Public methods

        public static WorldObjectType WorldObjectFromWot(string wotFilePath, bool ignoreFormatError)
        {
            return ReadWot(wotFilePath, ignoreFormatError);
        }

        public static void WorldObjectToWot(WorldObjectType worldObjectType, string wotFilePath)
        {
            WriteWot(worldObjectType, wotFilePath);
        }

        #endregion

        #region Internal methods

        internal static void ResolveDependencies()
        {
            for (int i = 0; i < dependencyResolvables.Count; i++)
            {
                dependencyResolvables[i].ResolveDependency();
            }
            dependencyResolvables.Clear();
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

                wotReader.ReadLine(); // Definition String 'COLLISIONDEFINITION'
                line = wotReader.ReadLine().Substring(18).Trim('\''); // EntityType
                if (!string.IsNullOrEmpty(line))
                {
                    if (!string.Equals(line, collisionDefinitionString, StringComparison.Ordinal))
                    {
                        throw new TPException($"EntityType '{line}' for 'COLLISIONDEFINITION' is invalid. Make sure it matches the Type of the world object.");
                    }

                    // If not empty and valid, check the factory type
                    var factoryType = wotReader.ReadLine().Substring(19).Trim('\'');

                    ReadCollisionDefinition(wotReader, wot, factoryType, collisionDefinitionString, ignoreFormatError);
                }

                wotReader.ReadLine(); // Definition String 'CUSTOMINFODEFINITION'
                line = wotReader.ReadLine().Substring(18).Trim('\''); // EntityType
                if (!string.IsNullOrEmpty(line))
                {
                    if (!string.Equals(line, customInfoDefinitionString, StringComparison.Ordinal))
                    {
                        throw new TPException($"EntityType '{line}' for 'CUSTOMINFODEFINITION' is invalid. Make sure it matches the Type of the world object.");
                    }

                    // If not empty and valid, check the factory type
                    var factoryType = wotReader.ReadLine().Substring(19).Trim('\'');

                    ReadCustomInfoDefinition(wotReader, wot, factoryType, customInfoDefinitionString, ignoreFormatError);
                }
            }
            return wot;
        }

        private static void WriteWot(WorldObjectType worldObjectType, string wotFilePath)
        {
            using(var wotWriter = new StreamWriter(File.Open(wotFilePath, FileMode.Create)))
            {
                wotWriter.WriteLine($"Type String '{worldObjectType.Type}'");
                wotWriter.WriteSection("ConstData", (lvl) =>
                {
                    wotWriter.WriteLineLevel($"HasPhysics String '{worldObjectType.PhysicsDefinition?.Type}'", lvl);
                    wotWriter.WriteLineLevel($"HasCollision String '{worldObjectType.CollisionDefinition?.Type}'", lvl);
                    wotWriter.WriteLineLevel($"HasRender String '{worldObjectType.RenderEntityDefinition?.Type}'", lvl);
                    wotWriter.WriteLineLevel($"HasAI String '{worldObjectType.AiEntityDefinition?.Type}'", lvl);
                    wotWriter.WriteLineLevel($"HasCustomInfo String '{worldObjectType.CustomInfoDefinition?.Type}'", lvl);
                });
                wotWriter.WriteLine("Definition String 'AIENTITYDEFINITION'");
                WriteAIEntityDefinition(wotWriter, worldObjectType.AiEntityDefinition);
                wotWriter.WriteLine("Definition String 'RENDERENTITYDEFINITION'");
                WriteRenderEntityDefinition(wotWriter, worldObjectType.RenderEntityDefinition);
                wotWriter.WriteLine("Definition String 'PHYSICSDEFINITION'");
                WritePhysicsDefinition(wotWriter, worldObjectType.PhysicsDefinition);
                wotWriter.WriteLine("Definition String 'COLLISIONDEFINITION'");
                WriteCollisionDefinition(wotWriter, worldObjectType.CollisionDefinition);
                wotWriter.WriteLine("Definition String 'CUSTOMINFODEFINITION'");
                WriteCustomInfoDefinition(wotWriter, worldObjectType.CustomInfoDefinition);
            }
        }

        #endregion

        #region Read definitions

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

            wot.PhysicsDefinition = factoryType switch
            {
                "DragonPhysics" => ReadDragonPhysics(wotReader, physicsDefinitionString, ignoreFormatError),
                "Whale Physics" => ReadWhalePhysics(wotReader, physicsDefinitionString, ignoreFormatError),
                "SpaceObjectPhysics" => ReadSpaceObjectPhysics(wotReader, physicsDefinitionString, ignoreFormatError),
                "ProjectilePhysics" => ReadProjectilePhysics(wotReader, physicsDefinitionString, ignoreFormatError),
                "MinePhysics" => ReadMinePhysics(wotReader, physicsDefinitionString, ignoreFormatError),
                "TorpedoPhysics" => ReadTorpedoPhysics(wotReader, physicsDefinitionString, ignoreFormatError),
                "ShipDemo" => ReadShipDemo(wotReader, physicsDefinitionString, ignoreFormatError),
                _ => throw new TPException($"FactoryType '{factoryType}' for 'PHYSICSDEFINITION' is invalid."),
            };
            wotReader.ReadLine(); // End section
        }

        private static void ReadCollisionDefinition(StreamReader wotReader, WorldObjectType wot, string factoryType, string collisionDefinitionString, bool ignoreFormatError)
        {
            wotReader.ReadLine(); // ContData
            wotReader.ReadLine(); // Start section

            wot.CollisionDefinition = factoryType switch
            {
                "BoundingRenderEntity" => ReadBoundingRenderEntity(wotReader, collisionDefinitionString, ignoreFormatError),
                "Point" => ReadPoint(wotReader, collisionDefinitionString, ignoreFormatError),
                "BoundingSphere" => ReadBoundingSphere(wotReader, collisionDefinitionString, ignoreFormatError),
                _ => throw new TPException($"FactoryType '{factoryType}' for 'COLLISIONDEFINITION' is invalid."),
            };
            wotReader.ReadLine(); // End section
        }

        private static void ReadCustomInfoDefinition(StreamReader wotReader, WorldObjectType wot, string factoryType, string customInfoDefinitionString, bool ignoreFormatError)
        {
            wotReader.ReadLine(); // ContData
            wotReader.ReadLine(); // Start section

            switch (factoryType)
            {
                case "DragonCustomInfoFactory":
                    wot.CustomInfoDefinition = ReadDragonCustomInfoFactory(wotReader, customInfoDefinitionString, ignoreFormatError);
                    break;
                case "SpaceAnimalCustomInfoFactory":
                    wot.CustomInfoDefinition = ReadSpaceAnimalCustomInfoFactory(wotReader, customInfoDefinitionString, ignoreFormatError);
                    break;
                case "AsteroidCustomInfoFactory":
                    var asteroidCustomInfoFactory = ReadAsteroidCustomInfoFactory(wotReader, customInfoDefinitionString, ignoreFormatError);
                    wot.CustomInfoDefinition = asteroidCustomInfoFactory;
                    dependencyResolvables.Add(asteroidCustomInfoFactory);
                    break;
                case "IslandCustomInfoFactory":
                    wot.CustomInfoDefinition = ReadIslandCustomInfoFactory(wotReader, customInfoDefinitionString, ignoreFormatError);
                    break;
                case "BulletCustomInfoFactory":
                    wot.CustomInfoDefinition = ReadBulletCustomInfoFactory(wotReader, customInfoDefinitionString, ignoreFormatError);
                    break;
                case "StarMortarCustomInfoFactory":
                    wot.CustomInfoDefinition = ReadStarMortarCustomInfoFactory(wotReader, customInfoDefinitionString, ignoreFormatError);
                    break;
                case "EnergyNetCustomInfoFactory":
                    wot.CustomInfoDefinition = ReadEnergyNetCustomInfoFactory(wotReader, customInfoDefinitionString, ignoreFormatError);
                    break;
                case "GrapplingHarpoonCustomInfoFactory":
                    wot.CustomInfoDefinition = ReadGrapplingHarpoonCustomInfoFactory(wotReader, customInfoDefinitionString, ignoreFormatError);
                    break;
                case "GravChargeCustomInfoFactory":
                    wot.CustomInfoDefinition = ReadGravChargeCustomInfoFactory(wotReader, customInfoDefinitionString, ignoreFormatError);
                    break;
                case "NovaMortarCustomInfoFactory":
                    wot.CustomInfoDefinition = ReadNovaMortarCustomInfoFactory(wotReader, customInfoDefinitionString, ignoreFormatError);
                    break;
                case "MineCustomInfoFactory":
                    wot.CustomInfoDefinition = ReadMineCustomInfoFactory(wotReader, customInfoDefinitionString, ignoreFormatError);
                    break;
                case "TorpedoCustomInfoFactory":
                    wot.CustomInfoDefinition = ReadTorpedoCustomInfoFactory(wotReader, customInfoDefinitionString, ignoreFormatError);
                    break;
                case "CrewCustomInfoFactory":
                    wot.CustomInfoDefinition = ReadCrewCustomInfoFactory(wotReader, customInfoDefinitionString, ignoreFormatError);
                    break;
                case "GunCustomInfoFactory":
                    wot.CustomInfoDefinition = ReadGunCustomInfoFactory(wotReader, customInfoDefinitionString, ignoreFormatError);
                    break;
                case "ShipCustomInfoFactory":
                    wot.CustomInfoDefinition = ReadShipCustomInfoFactory(wotReader, customInfoDefinitionString, ignoreFormatError);
                    break;
                case "ShipDebrisCustomInfoFactory":
                    wot.CustomInfoDefinition = ReadShipDebrisCustomInfoFactory(wotReader, customInfoDefinitionString, ignoreFormatError);
                    break;
                case "BlackHoleCustomInfoFactory":
                    wot.CustomInfoDefinition = ReadBlackHoleCustomInfoFactory(wotReader, customInfoDefinitionString, ignoreFormatError);
                    break;
                case "EtheriumCurrentCustomInfoFactory":
                    wot.CustomInfoDefinition = ReadEtheriumCurrentCustomInfoFactory(wotReader, customInfoDefinitionString, ignoreFormatError);
                    break;
                case "NebulaCustomInfoFactory":
                    wot.CustomInfoDefinition = ReadNebulaCustomInfoFactory(wotReader, customInfoDefinitionString, ignoreFormatError);
                    break;
                default:
                    throw new TPException($"FactoryType '{factoryType}' for 'CUSTOMINFODEFINITION' is invalid.");
            }
            wotReader.ReadLine(); // End section
        }

        #endregion

        #region Read AI factories

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

        #endregion

        #region Read Render factories

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
                AnimationName = wotReader.ReadString("AssociationName String "),
            };
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

        #region Read Physics factories

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

        #region Read Collision factories

        private static BoundingRenderEntity ReadBoundingRenderEntity(StreamReader wotReader, string collisionDefinitionString, bool ignoreFormatError)
        {
            var boundingRenderEntity = new BoundingRenderEntity(collisionDefinitionString);
            var detectionTypeString = wotReader.ReadString("DetectionType String ");
            if (EnumExtensions.TryGetValueFromDisplayName<DetectionType>(detectionTypeString, out var detectionType))
                boundingRenderEntity.DetectionType = detectionType;
            else
                throw new TPException($"The DetectionType value '{detectionTypeString}' in the COLLISIONDEFINITION factory '{collisionDefinitionString}' is invalid.");
            var responseTypeString = wotReader.ReadString("ResponseType String ");
            if (EnumExtensions.TryGetValueFromDisplayName<ResponseType>(responseTypeString, out var responseType))
                boundingRenderEntity.ResponseType = responseType;
            else
                throw new TPException($"The ResponseType value '{responseTypeString}' in the COLLISIONDEFINITION factory '{collisionDefinitionString}' is invalid.");
            boundingRenderEntity.UserDefinedSphereSize = wotReader.ReadAndParseBool("User defined sphere size Bool ", ignoreFormatError);
            if (boundingRenderEntity.UserDefinedSphereSize)
            {
                boundingRenderEntity.LocalPosition = wotReader.ReadAndParseVector3("LocalPosition Vector3", ignoreFormatError);
                boundingRenderEntity.Radius = wotReader.ReadAndParseFloat("Radius Float ", ignoreFormatError);
            }
            boundingRenderEntity.UserDefinedBoundingBoxExtents = wotReader.ReadAndParseBool("UserDefinedBoundingBoxExtents Bool ", ignoreFormatError);
            if (boundingRenderEntity.UserDefinedBoundingBoxExtents)
            {
                boundingRenderEntity.MinExtents = wotReader.ReadAndParseVector3("MinExtents Vector3", ignoreFormatError);
                boundingRenderEntity.MaxExtents = wotReader.ReadAndParseVector3("MaxExtents Vector3", ignoreFormatError);
            }
            return boundingRenderEntity;
        }

        private static Point ReadPoint(StreamReader wotReader, string collisionDefinitionString, bool ignoreFormatError)
        {
            var point = new Point(collisionDefinitionString);
            var detectionTypeString = wotReader.ReadString("DetectionType String ");
            if (EnumExtensions.TryGetValueFromDisplayName<DetectionType>(detectionTypeString, out var detectionType))
                point.DetectionType = detectionType;
            else
                throw new TPException($"The DetectionType value '{detectionTypeString}' in the COLLISIONDEFINITION factory '{collisionDefinitionString}' is invalid.");
            var responseTypeString = wotReader.ReadString("ResponseType String ");
            if (EnumExtensions.TryGetValueFromDisplayName<ResponseType>(responseTypeString, out var responseType))
                point.ResponseType = responseType;
            else
                throw new TPException($"The ResponseType value '{responseTypeString}' in the COLLISIONDEFINITION factory '{collisionDefinitionString}' is invalid.");
            return point;
        }

        private static BoundingSphere ReadBoundingSphere(StreamReader wotReader, string collisionDefinitionString, bool ignoreFormatError)
        {
            var boundingSphere = new BoundingSphere(collisionDefinitionString);
            var detectionTypeString = wotReader.ReadString("DetectionType String ");
            if (EnumExtensions.TryGetValueFromDisplayName<DetectionType>(detectionTypeString, out var detectionType))
                boundingSphere.DetectionType = detectionType;
            else
                throw new TPException($"The DetectionType value '{detectionTypeString}' in the COLLISIONDEFINITION factory '{collisionDefinitionString}' is invalid.");
            var responseTypeString = wotReader.ReadString("ResponseType String ");
            if (EnumExtensions.TryGetValueFromDisplayName<ResponseType>(responseTypeString, out var responseType))
                boundingSphere.ResponseType = responseType;
            else
                throw new TPException($"The ResponseType value '{responseTypeString}' in the COLLISIONDEFINITION factory '{collisionDefinitionString}' is invalid.");
            boundingSphere.UserDefinedSphereSize = wotReader.ReadAndParseBool("User defined sphere size Bool ", ignoreFormatError);
            if (boundingSphere.UserDefinedSphereSize)
            {
                boundingSphere.LocalPosition = wotReader.ReadAndParseVector3("LocalPosition Vector3", ignoreFormatError);
                boundingSphere.Radius = wotReader.ReadAndParseFloat("Radius Float ", ignoreFormatError);
            }
            return boundingSphere;
        }

        #endregion

        #region Read CustomInfo factories

        private static DragonCustomInfoFactory ReadDragonCustomInfoFactory(StreamReader wotReader, string customInfoDefinitionString, bool ignoreFormatError)
        {
            var dragonCustomInfoFactory = new DragonCustomInfoFactory(customInfoDefinitionString);
            wotReader.ReadLine(); // GUIInfo Chunk
            wotReader.ReadLine(); // Start section
            dragonCustomInfoFactory.SelectedIndicatorPercentageOfRadius = wotReader.ReadAndParseFloat("Selected Indicator Percentage Of Radius Float ", ignoreFormatError);
            dragonCustomInfoFactory.DistancetoStartSpherePicking = wotReader.ReadAndParseFloat("Distance to Start Sphere Picking Float ", ignoreFormatError);
            wotReader.ReadLine(); // End section
            dragonCustomInfoFactory.ShortestTimeBetweenSounds = wotReader.ReadAndParseFloat("ShortestTimeBetweenSounds Float ", ignoreFormatError);
            dragonCustomInfoFactory.LongestTimeBetweenSounds = wotReader.ReadAndParseFloat("LongestTimeBetweenSounds Float ", ignoreFormatError);
            dragonCustomInfoFactory.Sound_0 = wotReader.ReadString("Sound_0 String ");
            dragonCustomInfoFactory.Sound_1 = wotReader.ReadString("Sound_1 String ");
            dragonCustomInfoFactory.Sound_2 = wotReader.ReadString("Sound_2 String ");
            dragonCustomInfoFactory.Sound_3 = wotReader.ReadString("Sound_3 String ");
            dragonCustomInfoFactory.Sound_4 = wotReader.ReadString("Sound_4 String ");
            dragonCustomInfoFactory.Sound_5 = wotReader.ReadString("Sound_5 String ");
            dragonCustomInfoFactory.Sound_6 = wotReader.ReadString("Sound_6 String ");
            dragonCustomInfoFactory.Sound_7 = wotReader.ReadString("Sound_7 String ");
            dragonCustomInfoFactory.Sound_8 = wotReader.ReadString("Sound_8 String ");
            dragonCustomInfoFactory.Sound_9 = wotReader.ReadString("Sound_9 String ");
            dragonCustomInfoFactory.ReportSpotting = wotReader.ReadAndParseBool("Report spotting Bool ", ignoreFormatError);
            if (dragonCustomInfoFactory.ReportSpotting)
            {
                var reportTypeString = wotReader.ReadString("Report type String ");
                if (EnumExtensions.TryGetValueFromDisplayName<GenericDialogSound>(reportTypeString, out var reportType))
                    dragonCustomInfoFactory.ReportType = reportType;
                else
                    throw new TPException($"The Report type value '{reportTypeString}' in the CUSTOMINFODEFINITION factory '{customInfoDefinitionString}' is invalid.");
            }
            dragonCustomInfoFactory.SoundDistance = wotReader.ReadAndParseFloat("Sound Distance Float ", ignoreFormatError);
            return dragonCustomInfoFactory;
        }

        private static SpaceAnimalCustomInfoFactory ReadSpaceAnimalCustomInfoFactory(StreamReader wotReader, string customInfoDefinitionString, bool ignoreFormatError)
        {
            var spaceAnimalCustomInfoFactory = new SpaceAnimalCustomInfoFactory(customInfoDefinitionString);
            wotReader.ReadLine(); // GUIInfo Chunk
            wotReader.ReadLine(); // Start section
            spaceAnimalCustomInfoFactory.SelectedIndicatorPercentageOfRadius = wotReader.ReadAndParseFloat("Selected Indicator Percentage Of Radius Float ", ignoreFormatError);
            spaceAnimalCustomInfoFactory.DistancetoStartSpherePicking = wotReader.ReadAndParseFloat("Distance to Start Sphere Picking Float ", ignoreFormatError);
            wotReader.ReadLine(); // End section
            spaceAnimalCustomInfoFactory.ShortestTimeBetweenSounds = wotReader.ReadAndParseFloat("ShortestTimeBetweenSounds Float ", ignoreFormatError);
            spaceAnimalCustomInfoFactory.LongestTimeBetweenSounds = wotReader.ReadAndParseFloat("LongestTimeBetweenSounds Float ", ignoreFormatError);
            spaceAnimalCustomInfoFactory.Sound_0 = wotReader.ReadString("Sound_0 String ");
            spaceAnimalCustomInfoFactory.Sound_1 = wotReader.ReadString("Sound_1 String ");
            spaceAnimalCustomInfoFactory.Sound_2 = wotReader.ReadString("Sound_2 String ");
            spaceAnimalCustomInfoFactory.Sound_3 = wotReader.ReadString("Sound_3 String ");
            spaceAnimalCustomInfoFactory.Sound_4 = wotReader.ReadString("Sound_4 String ");
            spaceAnimalCustomInfoFactory.Sound_5 = wotReader.ReadString("Sound_5 String ");
            spaceAnimalCustomInfoFactory.Sound_6 = wotReader.ReadString("Sound_6 String ");
            spaceAnimalCustomInfoFactory.Sound_7 = wotReader.ReadString("Sound_7 String ");
            spaceAnimalCustomInfoFactory.Sound_8 = wotReader.ReadString("Sound_8 String ");
            spaceAnimalCustomInfoFactory.Sound_9 = wotReader.ReadString("Sound_9 String ");
            spaceAnimalCustomInfoFactory.ReportSpotting = wotReader.ReadAndParseBool("Report spotting Bool ", ignoreFormatError);
            if (spaceAnimalCustomInfoFactory.ReportSpotting)
            {
                var reportTypeString = wotReader.ReadString("Report type String ");
                if (EnumExtensions.TryGetValueFromDisplayName<GenericDialogSound>(reportTypeString, out var reportType))
                    spaceAnimalCustomInfoFactory.ReportType = reportType;
                else
                    throw new TPException($"The Report type value '{reportTypeString}' in the CUSTOMINFODEFINITION factory '{customInfoDefinitionString}' is invalid.");
            }
            spaceAnimalCustomInfoFactory.SoundDistance = wotReader.ReadAndParseFloat("Sound Distance Float ", ignoreFormatError);
            return spaceAnimalCustomInfoFactory;
        }

        private static AsteroidCustomInfoFactory ReadAsteroidCustomInfoFactory(StreamReader wotReader, string customInfoDefinitionString, bool ignoreFormatError)
        {
            var asteroidCustomInfoFactory = new AsteroidCustomInfoFactory(customInfoDefinitionString)
            {
                MaxHitPoints = wotReader.ReadAndParseInt("Max Hit Points Int ", ignoreFormatError),
                WorldObjectToCreateUponDeath0Name = wotReader.ReadString("WorldObject To Create Upon Death 0 String "),
                WorldObjectToCreateUponDeath1Name = wotReader.ReadString("WorldObject To Create Upon Death 1 String "),
                WorldObjectToCreateUponDeath2Name = wotReader.ReadString("WorldObject To Create Upon Death 2 String "),
                WorldObjectToCreateUponDeath3Name = wotReader.ReadString("WorldObject To Create Upon Death 3 String "),
                WorldObjectToCreateUponDeath4Name = wotReader.ReadString("WorldObject To Create Upon Death 4 String "),
                WorldObjectToCreateUponDeath5Name = wotReader.ReadString("WorldObject To Create Upon Death 5 String "),
                WorldObjectToCreateUponDeath6Name = wotReader.ReadString("WorldObject To Create Upon Death 6 String "),
                WorldObjectToCreateUponDeath7Name = wotReader.ReadString("WorldObject To Create Upon Death 7 String "),
                WorldObjectToCreateUponDeath8Name = wotReader.ReadString("WorldObject To Create Upon Death 8 String "),
                WorldObjectToCreateUponDeath9Name = wotReader.ReadString("WorldObject To Create Upon Death 9 String "),
                ExplosionEffect = wotReader.ReadString("ExplosionEffect String "),
                ReportSpotting = wotReader.ReadAndParseBool("Report Spotting Bool ", ignoreFormatError),
            };
            return asteroidCustomInfoFactory;
        }

        private static IslandCustomInfoFactory ReadIslandCustomInfoFactory(StreamReader wotReader, string customInfoDefinitionString, bool ignoreFormatError)
        {
            var islandCustomInfoFactory = new IslandCustomInfoFactory(customInfoDefinitionString)
            {
                OcclusionPolygonPoint00 = wotReader.ReadAndParseVector3("Occlusion Polygon Point # 00 Vector3", ignoreFormatError),
                OcclusionPolygonPoint01 = wotReader.ReadAndParseVector3("Occlusion Polygon Point # 01 Vector3", ignoreFormatError),
                OcclusionPolygonPoint02 = wotReader.ReadAndParseVector3("Occlusion Polygon Point # 02 Vector3", ignoreFormatError),
                OcclusionPolygonPoint03 = wotReader.ReadAndParseVector3("Occlusion Polygon Point # 03 Vector3", ignoreFormatError),
                OcclusionPolygonPoint04 = wotReader.ReadAndParseVector3("Occlusion Polygon Point # 04 Vector3", ignoreFormatError),
                OcclusionPolygonPoint05 = wotReader.ReadAndParseVector3("Occlusion Polygon Point # 05 Vector3", ignoreFormatError),
                OcclusionPolygonPoint06 = wotReader.ReadAndParseVector3("Occlusion Polygon Point # 06 Vector3", ignoreFormatError),
                OcclusionPolygonPoint07 = wotReader.ReadAndParseVector3("Occlusion Polygon Point # 07 Vector3", ignoreFormatError),
                OcclusionPolygonPoint08 = wotReader.ReadAndParseVector3("Occlusion Polygon Point # 08 Vector3", ignoreFormatError),
                OcclusionPolygonPoint09 = wotReader.ReadAndParseVector3("Occlusion Polygon Point # 09 Vector3", ignoreFormatError),
                OcclusionPolygonPointCount = wotReader.ReadAndParseInt("Occlusion Polygon Point Count Int ", ignoreFormatError),
            };
            var raceString = wotReader.ReadString("Race String ");
            if (EnumExtensions.TryGetValueFromDisplayName<Race>(raceString, out var race))
                islandCustomInfoFactory.Race = race;
            else
                throw new TPException($"The Race value '{raceString}' in the CUSTOMINFODEFINITION factory '{customInfoDefinitionString}' is invalid.");
            islandCustomInfoFactory.HasAmbientSound = wotReader.ReadAndParseBool("HasAmbientSound Bool ", ignoreFormatError);
            if (islandCustomInfoFactory.HasAmbientSound)
            {
                islandCustomInfoFactory.AmbientSoundMaxDistance = wotReader.ReadAndParseFloat("AmbientSoundMaxDistance Float ", ignoreFormatError);
                islandCustomInfoFactory.AmbientSoundName = wotReader.ReadString("AmbientSoundName String ");
            }
            islandCustomInfoFactory.CoreDamageSectionMaxHitPoints = wotReader.ReadAndParseInt("Core Damage Section Max HitPoints Int ", ignoreFormatError);
            islandCustomInfoFactory.AnnounceSighting = wotReader.ReadAndParseBool("AnnounceSighting Bool ", ignoreFormatError);
            return islandCustomInfoFactory;
        }

        private static BulletCustomInfoFactory ReadBulletCustomInfoFactory(StreamReader wotReader, string customInfoDefinitionString, bool ignoreFormatError)
        {
            return ReadBulletCustomInfoFactory<BulletCustomInfoFactory>(wotReader, customInfoDefinitionString, ignoreFormatError);
        }

        private static StarMortarCustomInfoFactory ReadStarMortarCustomInfoFactory(StreamReader wotReader, string customInfoDefinitionString, bool ignoreFormatError)
        {
            var magnitude = wotReader.ReadAndParseFloat("Magnitude Float ", ignoreFormatError);
            var maxRadius = wotReader.ReadAndParseFloat("MaxRadius Float ", ignoreFormatError);
            var duration = wotReader.ReadAndParseFloat("Duration Float ", ignoreFormatError);
            var starMortarCustomInfoFactory = ReadBulletCustomInfoFactory<StarMortarCustomInfoFactory>(wotReader, customInfoDefinitionString, ignoreFormatError);
            starMortarCustomInfoFactory.Magnitude = magnitude;
            starMortarCustomInfoFactory.MaxRadius = maxRadius;
            starMortarCustomInfoFactory.Duration = duration;
            return starMortarCustomInfoFactory;
        }
        
        private static EnergyNetCustomInfoFactory ReadEnergyNetCustomInfoFactory(StreamReader wotReader, string customInfoDefinitionString, bool ignoreFormatError)
        {
            var duration = wotReader.ReadAndParseFloat("Duration Float ", ignoreFormatError);
            var percentageOfEnergyDissipated = wotReader.ReadAndParseFloat("PercentageOfEnergyDissipated Float ", ignoreFormatError);
            var energyNetCustomInfoFactory = ReadBulletCustomInfoFactory<EnergyNetCustomInfoFactory>(wotReader, customInfoDefinitionString, ignoreFormatError);
            energyNetCustomInfoFactory.Duration = duration;
            energyNetCustomInfoFactory.PercentageOfEnergyDissipated = percentageOfEnergyDissipated;
            energyNetCustomInfoFactory.Duration = duration;
            return energyNetCustomInfoFactory;
        }

        private static GrapplingHarpoonCustomInfoFactory ReadGrapplingHarpoonCustomInfoFactory(StreamReader wotReader, string customInfoDefinitionString, bool ignoreFormatError)
        {
            return ReadBulletCustomInfoFactory<GrapplingHarpoonCustomInfoFactory>(wotReader, customInfoDefinitionString, ignoreFormatError);
        }
        
        private static GravChargeCustomInfoFactory ReadGravChargeCustomInfoFactory(StreamReader wotReader, string customInfoDefinitionString, bool ignoreFormatError)
        {
            var magnitude = wotReader.ReadAndParseFloat("Magnitude Float ", ignoreFormatError);
            var radius = wotReader.ReadAndParseFloat("Radius Float ", ignoreFormatError);
            var duration = wotReader.ReadAndParseFloat("Duration Float ", ignoreFormatError);
            var gravChargeCustomInfoFactory = ReadBulletCustomInfoFactory<GravChargeCustomInfoFactory>(wotReader, customInfoDefinitionString, ignoreFormatError, false);
            gravChargeCustomInfoFactory.Magnitude = magnitude;
            gravChargeCustomInfoFactory.Radius = radius;
            gravChargeCustomInfoFactory.Duration = duration;
            return gravChargeCustomInfoFactory;
        }

        private static NovaMortarCustomInfoFactory ReadNovaMortarCustomInfoFactory(StreamReader wotReader, string customInfoDefinitionString, bool ignoreFormatError)
        {
            return ReadBulletCustomInfoFactory<NovaMortarCustomInfoFactory>(wotReader, customInfoDefinitionString, ignoreFormatError, false);
        }

        private static MineCustomInfoFactory ReadMineCustomInfoFactory(StreamReader wotReader, string customInfoDefinitionString, bool ignoreFormatError)
        {
            return ReadBulletCustomInfoFactory<MineCustomInfoFactory>(wotReader, customInfoDefinitionString, ignoreFormatError);
        }

        private static TorpedoCustomInfoFactory ReadTorpedoCustomInfoFactory(StreamReader wotReader, string customInfoDefinitionString, bool ignoreFormatError)
        {
            return ReadBulletCustomInfoFactory<TorpedoCustomInfoFactory>(wotReader, customInfoDefinitionString, ignoreFormatError);
        }

        private static CrewCustomInfoFactory ReadCrewCustomInfoFactory(StreamReader wotReader, string customInfoDefinitionString, bool ignoreFormatError)
        {
            return new CrewCustomInfoFactory(customInfoDefinitionString)
            {
                CrewRace = wotReader.ReadAndParseEnum<Race>("Crew Race String ", ignoreFormatError),
                PointValue = wotReader.ReadAndParseInt("Point Value Int ", ignoreFormatError),
                CrewNameStringID = wotReader.ReadString("Crew Name String ID String "),
                LeadershipSkillActive = wotReader.ReadAndParseBool("Leadership Skill Active Bool ", ignoreFormatError),
                LeadershipSkillAbilityValue = wotReader.ReadAndParseInt("Leadership Skill Ability Value Int ", ignoreFormatError),
                NavigationSkillActive = wotReader.ReadAndParseBool("Navigation Skill Active Bool ", ignoreFormatError),
                NavigationSkillAbilityValue = wotReader.ReadAndParseInt("Navigation Skill Ability Value Int ", ignoreFormatError),
                SpottingSkillActive = wotReader.ReadAndParseBool("Spotting Skill Active Bool ", ignoreFormatError),
                SpottingSkillAbilityValue = wotReader.ReadAndParseInt("Spotting Skill Ability Value Int ", ignoreFormatError),
                EngineeringSkillActive = wotReader.ReadAndParseBool("Engineering Skill Active Bool ", ignoreFormatError),
                EngineeringSkillAbilityValue = wotReader.ReadAndParseInt("Engineering Skill Ability Value Int ", ignoreFormatError),
                RiggingSkillActive = wotReader.ReadAndParseBool("Rigging Skill Active Bool ", ignoreFormatError),
                RiggingSkillAbilityValue = wotReader.ReadAndParseInt("Rigging Skill Ability Value Int ", ignoreFormatError),
                CombatSkillActive = wotReader.ReadAndParseBool("Combat Skill Active Bool ", ignoreFormatError),
                CombatSkillAbilityValue = wotReader.ReadAndParseInt("Combat Skill Ability Value Int ", ignoreFormatError),
                GunnerySkillActive = wotReader.ReadAndParseBool("Gunnery Skill Active Bool ", ignoreFormatError),
                GunnerySkillAbilityValue = wotReader.ReadAndParseInt("Gunnery Skill Ability Value Int ", ignoreFormatError),
                TalkingHeadTexture = wotReader.ReadString("Talking Head Texture String "),
                Species = wotReader.ReadAndParseEnum<Species>("Species String ", ignoreFormatError),
            };
        }

        private static GunCustomInfoFactory ReadGunCustomInfoFactory(StreamReader wotReader, string customInfoDefinitionString, bool ignoreFormatError)
        {
            return new GunCustomInfoFactory(customInfoDefinitionString)
            {
                BulletTypeName_PrimaryString = wotReader.ReadString("BulletTypeName_Primary String "),
                BulletTypeName_SecondaryString = wotReader.ReadString("BulletTypeName_Secondary String "),
                SoundName = wotReader.ReadString("SoundName String "),
                MuzzleFlashEffect = wotReader.ReadString("MuzzleFlashEffect String "),
                MuzzleSpeed = wotReader.ReadAndParseFloat("MuzzleSpeed Float ", ignoreFormatError),
                DuringBurstReloadTime = wotReader.ReadAndParseFloat("DuringBurstReloadTime Float ", ignoreFormatError),
                BulletsPerBurst = wotReader.ReadAndParseInt("BulletsPerBurst Int ", ignoreFormatError),
                TimeBetweenBursts = wotReader.ReadAndParseFloat("TimeBetweenBursts Float ", ignoreFormatError),
                CalculateAccuracyDeviationDuringBurst = wotReader.ReadAndParseBool("CalculateAccuracyDeviationDuringBurst Bool ", ignoreFormatError),
                WeaponBarIconTexture = wotReader.ReadString("WeaponBar Icon Texture String "),
                VerticalRotationSpeed = wotReader.ReadAndParseFloat("VerticalRotationSpeed Float ", ignoreFormatError),
                HorizontalRotationSpeed = wotReader.ReadAndParseFloat("HorizontalRotationSpeed Float ", ignoreFormatError),
                VerticalMinAngle = wotReader.ReadAndParseFloat("VerticalMinAngle Float ", ignoreFormatError),
                VerticalMaxAngle = wotReader.ReadAndParseFloat("VerticalMaxAngle Float ", ignoreFormatError),
                LobAngle = wotReader.ReadAndParseFloat("LobAngle Float ", ignoreFormatError),
                MaximumRange_Range = wotReader.ReadAndParseFloat("MaximumRange_Range Float ", ignoreFormatError),
                LongRange_Range = wotReader.ReadAndParseFloat("LongRange_Range Float ", ignoreFormatError),
                EffectiveRange_Range = wotReader.ReadAndParseFloat("EffectiveRange_Range Float ", ignoreFormatError),
                BulletAffectedByGravity = wotReader.ReadAndParseBool("BulletAffectedByGravity Bool ", ignoreFormatError),
                MaximumRange_AccuracyDeviation = wotReader.ReadAndParseFloat("MaximumRange_AccuracyDeviation Float ", ignoreFormatError),
                LongRange_AccuracyDeviation = wotReader.ReadAndParseFloat("LongRange_AccuracyDeviation Float ", ignoreFormatError),
                EffectiveRange_AccuracyDeviation = wotReader.ReadAndParseFloat("EffectiveRange_AccuracyDeviation Float ", ignoreFormatError),
                MaximumRange_RicochetFactor = wotReader.ReadAndParseFloat("MaximumRange_RicochetFactor Float ", ignoreFormatError),
                LongRange_RicochetFactor = wotReader.ReadAndParseFloat("LongRange_RicochetFactor Float ", ignoreFormatError),
                EffectiveRange_RicochetFactor = wotReader.ReadAndParseFloat("EffectiveRange_RicochetFactor Float ", ignoreFormatError),
                VictoryPointCost = wotReader.ReadAndParseInt("Victory Point Cost Int ", ignoreFormatError),
                GunNameStringID = wotReader.ReadString("Gun Name String ID String "),
                MountType = wotReader.ReadAndParseEnum<MountType>("Mount Type String ", ignoreFormatError),
                ExclusionRace = wotReader.ReadAndParseEnum<Race>("Exclusion Race String ", ignoreFormatError),
                CanBeFiredWhileCloaked = wotReader.ReadAndParseBool("Can Be Fired While Cloaked Bool ", ignoreFormatError),
                FiringCrewAlert = wotReader.ReadAndParseEnum<GenericDialogSound>("Firing Crew Alert String ", ignoreFormatError),
                SoundMaxDistance = wotReader.ReadAndParseFloat("SoundMaxDistance Float ", ignoreFormatError),
                SoundVolume = wotReader.ReadAndParseFloat("SoundVolume Float ", ignoreFormatError),
            };
        }

        private static ShipCustomInfoFactory ReadShipCustomInfoFactory(StreamReader wotReader, string customInfoDefinitionString, bool ignoreFormatError)
        {
            wotReader.ReadLine(); // 00000003 GUIInfo Chunk
            wotReader.ReadLine(); // Start section
            var shipCustomInfoFactory = new ShipCustomInfoFactory(customInfoDefinitionString)
            {
                ShipBarIconTexture = wotReader.ReadString("ShipBar Icon Texture String "),
                SelectedIndicatorPercentageOfRadius = wotReader.ReadAndParseFloat("Selected Indicator Percentage Of Radius Float ", ignoreFormatError),
                DistanceToStartSpherePicking = wotReader.ReadAndParseFloat("Distance to Start Sphere Picking Float ", ignoreFormatError),
            };
            wotReader.ReadLine(); // End section
            shipCustomInfoFactory.DisplayableShipnameStringID = wotReader.ReadString("Displayable Shipname String ID String ");
            shipCustomInfoFactory.ShipRace = wotReader.ReadAndParseEnum<Race>("Ship Race String ", ignoreFormatError);
            shipCustomInfoFactory.IsTender = wotReader.ReadAndParseBool("Is Tender Bool ", ignoreFormatError);
            shipCustomInfoFactory.NumberOfLifeboats = wotReader.ReadAndParseInt("Number of lifeboats Int ", ignoreFormatError);
            shipCustomInfoFactory.IsLifeboat = wotReader.ReadAndParseBool("Is Lifeboat Bool ", ignoreFormatError);
            shipCustomInfoFactory.IsCloakable = wotReader.ReadAndParseBool("Is Cloakable Bool ", ignoreFormatError);
            shipCustomInfoFactory.ShipSize = wotReader.ReadAndParseInt("Ship Size Int ", ignoreFormatError);
            shipCustomInfoFactory.CoreDamageSectionMaxHitPoints = wotReader.ReadAndParseInt("Core Damage Section Max HitPoints Int ", ignoreFormatError);
            shipCustomInfoFactory.ExplosionEffectName = wotReader.ReadString("Explosion Effect Name String ");
            shipCustomInfoFactory.EngineType = wotReader.ReadAndParseEnum<EngineType>("EngineType String ", ignoreFormatError);
            shipCustomInfoFactory.EngineSoundNameEmergency = wotReader.ReadString("EngineSoundName Emergency String ");
            shipCustomInfoFactory.EngineSoundNameFull = wotReader.ReadString("EngineSoundName Full String ");
            shipCustomInfoFactory.EngineSoundNameHalf = wotReader.ReadString("EngineSoundName Half String ");
            shipCustomInfoFactory.VictoryPointCost = wotReader.ReadAndParseInt("Victory Point Cost Int ", ignoreFormatError);
            shipCustomInfoFactory.IsAvailableInMultiplayer = wotReader.ReadAndParseBool("Available in Multiplayer? Bool ", ignoreFormatError);
            shipCustomInfoFactory.AvailableUniqueShipNameID00 = wotReader.ReadString("Available Unique Ship Name ID 00 String ");
            shipCustomInfoFactory.AvailableUniqueShipNameID01 = wotReader.ReadString("Available Unique Ship Name ID 01 String ");
            shipCustomInfoFactory.AvailableUniqueShipNameID02 = wotReader.ReadString("Available Unique Ship Name ID 02 String ");
            shipCustomInfoFactory.AvailableUniqueShipNameID03 = wotReader.ReadString("Available Unique Ship Name ID 03 String ");
            shipCustomInfoFactory.AvailableUniqueShipNameID04 = wotReader.ReadString("Available Unique Ship Name ID 04 String ");
            shipCustomInfoFactory.AvailableUniqueShipNameID05 = wotReader.ReadString("Available Unique Ship Name ID 05 String ");
            shipCustomInfoFactory.AvailableUniqueShipNameID06 = wotReader.ReadString("Available Unique Ship Name ID 06 String ");
            shipCustomInfoFactory.AvailableUniqueShipNameID07 = wotReader.ReadString("Available Unique Ship Name ID 07 String ");
            shipCustomInfoFactory.AvailableUniqueShipNameID08 = wotReader.ReadString("Available Unique Ship Name ID 08 String ");
            shipCustomInfoFactory.AvailableUniqueShipNameID09 = wotReader.ReadString("Available Unique Ship Name ID 09 String ");
            shipCustomInfoFactory.AvailableUniqueShipNameID10 = wotReader.ReadString("Available Unique Ship Name ID 10 String ");
            shipCustomInfoFactory.AvailableUniqueShipNameID11 = wotReader.ReadString("Available Unique Ship Name ID 11 String ");
            shipCustomInfoFactory.MaxNumberOfGunners = wotReader.ReadAndParseInt("Max number of Gunners Int ", ignoreFormatError);
            shipCustomInfoFactory.MaxNumberOfCaptains = wotReader.ReadAndParseInt("Max number of Captains Int ", ignoreFormatError);
            shipCustomInfoFactory.MaxNumberOfFirstMates = wotReader.ReadAndParseInt("Max number of First Mates Int ", ignoreFormatError);
            shipCustomInfoFactory.MaxNumberOfNavigators = wotReader.ReadAndParseInt("Max number of Navigators Int ", ignoreFormatError);
            shipCustomInfoFactory.MaxNumberOfEngineers = wotReader.ReadAndParseInt("Max number of Engineers Int ", ignoreFormatError);
            shipCustomInfoFactory.MaxNumberOfRiggers = wotReader.ReadAndParseInt("Max number of Riggers Int ", ignoreFormatError);
            shipCustomInfoFactory.MaxNumberOfFighters = wotReader.ReadAndParseInt("Max number of Fighters Int ", ignoreFormatError);
            shipCustomInfoFactory.MaxNumberOfLookouts = wotReader.ReadAndParseInt("Max number of Lookouts Int ", ignoreFormatError);
            shipCustomInfoFactory.RepairEffectName = wotReader.ReadString("Repair Effect Name String ");
            return shipCustomInfoFactory;
        }

        private static ShipDebrisCustomInfoFactory ReadShipDebrisCustomInfoFactory(StreamReader wotReader, string customInfoDefinitionString, bool ignoreFormatError)
        {
            return new ShipDebrisCustomInfoFactory(customInfoDefinitionString)
            {
                LifeTimeMin = wotReader.ReadAndParseFloat("LifeTimeMin Float ", ignoreFormatError),
                LifeTimeMax = wotReader.ReadAndParseFloat("LifeTimeMax Float ", ignoreFormatError),
            };
        }

        private static BlackHoleCustomInfoFactory ReadBlackHoleCustomInfoFactory(StreamReader wotReader, string customInfoDefinitionString, bool ignoreFormatError)
        {
            return new BlackHoleCustomInfoFactory(customInfoDefinitionString)
            {
                Magnitude = wotReader.ReadAndParseFloat("Magnitude Float ", ignoreFormatError),
                Radius = wotReader.ReadAndParseFloat("Radius Float ", ignoreFormatError),
                VortexEffectName = wotReader.ReadString("VortexEffectName String "),
                AmbientSoundMaxDistance = wotReader.ReadAndParseFloat("AmbientSoundMaxDistance Float ", ignoreFormatError),
                AmbientSoundName = wotReader.ReadString("AmbientSoundName String "),
            };
        }

        private static EtheriumCurrentCustomInfoFactory ReadEtheriumCurrentCustomInfoFactory(StreamReader wotReader, string customInfoDefinitionString, bool ignoreFormatError)
        {
            return new EtheriumCurrentCustomInfoFactory(customInfoDefinitionString)
            {
                Magnitude = wotReader.ReadAndParseFloat("Magnitude Float ", ignoreFormatError),
                Radius = wotReader.ReadAndParseFloat("Radius Float ", ignoreFormatError),
            };
        }

        private static NebulaCustomInfoFactory ReadNebulaCustomInfoFactory(StreamReader wotReader, string customInfoDefinitionString, bool ignoreFormatError)
        {
            var nebulaCustomInfoFactory = new NebulaCustomInfoFactory(customInfoDefinitionString)
            {
                MinimumLightningSpawnDistanceFromShips = wotReader.ReadAndParseFloat("Minimum Lightning Spawn Distance From Ships Float ", ignoreFormatError),
                MinimumLightningBoltLength = wotReader.ReadAndParseFloat("Minimum Lightning Bolt Length Float ", ignoreFormatError),
                MaximumLightningSpawnPointZAmplitude = wotReader.ReadAndParseFloat("Maximum Lightning Spawn Point Z Amplitude Float ", ignoreFormatError),
                MeteorHitChanceModifier = wotReader.ReadAndParseFloat("Meteor Hit Chance Modifier Float ", ignoreFormatError),
            };
            wotReader.ReadLine(); // 00000048 Lightning Damage Potential
            wotReader.ReadLine(); // Start section
            ReadDamagePotential(wotReader, nebulaCustomInfoFactory.LightningDamagePotential, ignoreFormatError);
            wotReader.ReadLine(); // End section
            wotReader.ReadLine(); // 00000048 Wind Damage Potential
            wotReader.ReadLine(); // Start section
            ReadDamagePotential(wotReader, nebulaCustomInfoFactory.WindDamagePotential, ignoreFormatError);
            wotReader.ReadLine(); // End section
            wotReader.ReadLine(); // 00000048 Metor Damage Potential
            wotReader.ReadLine(); // Start section
            ReadDamagePotential(wotReader, nebulaCustomInfoFactory.MeteorDamagePotential, ignoreFormatError);
            wotReader.ReadLine(); // End section
            nebulaCustomInfoFactory.NebulaSoundName = wotReader.ReadString("NebulaSoundName String ");
            nebulaCustomInfoFactory.SolarStormSoundName = wotReader.ReadString("SolarStormSoundName String ");
            nebulaCustomInfoFactory.MeteorShowerSoundName = wotReader.ReadString("MeteorShowerSoundName String ");

            return nebulaCustomInfoFactory;
        }

        private static T ReadBulletCustomInfoFactory<T>(StreamReader wotReader, string customInfoDefinitionString, bool ignoreFormatError, bool hasLifetime = true) where T : BulletCustomInfoFactory
        {
            var bulletCustomInfoFactory = (T)Activator.CreateInstance(typeof(T), customInfoDefinitionString);
            bulletCustomInfoFactory.HitEffect = wotReader.ReadString("HitEffect String ");
            bulletCustomInfoFactory.BulletEffect = wotReader.ReadString("BulletEffect String ");
            bulletCustomInfoFactory.DamageHitPoints = wotReader.ReadAndParseInt("Damage HitPoints Int ", ignoreFormatError);
            var decalDamageSizeString = wotReader.ReadString("Decal Damage Size String ");
            if (EnumExtensions.TryGetValueFromDisplayName<DecalDamageSize>(decalDamageSizeString, out var decalDamageSize))
                bulletCustomInfoFactory.DecalDamageSize = decalDamageSize;
            else
                throw new TPException($"The Decal Damage Size value '{decalDamageSizeString}' in the CUSTOMINFODEFINITION factory '{customInfoDefinitionString}' is invalid.");

            ReadDamagePotential(wotReader, bulletCustomInfoFactory.DamagePotential, ignoreFormatError);

            bulletCustomInfoFactory.ImpactSoundMaxDistance = wotReader.ReadAndParseFloat("Impact Sound Max Distance Float ", ignoreFormatError);
            bulletCustomInfoFactory.ImpactSoundVolume = wotReader.ReadAndParseFloat("Impact Sound Volume Float ", ignoreFormatError);
            bulletCustomInfoFactory.TravelSoundMaxDistance = wotReader.ReadAndParseFloat("Travel Sound Max Distance Float ", ignoreFormatError);
            bulletCustomInfoFactory.TravelSoundVolume = wotReader.ReadAndParseFloat("Travel Sound Volume Float ", ignoreFormatError);

            bulletCustomInfoFactory.RicochetSoundHullWood = wotReader.ReadString("RicochetSound: Hull Wood String ");
            wotReader.ReadLine(); // 00000005 ImpactSoundsHull Wood
            wotReader.ReadLine(); // Start section
            bulletCustomInfoFactory.ImpactSound0HullWood = wotReader.ReadString("Sound_0 String ");
            bulletCustomInfoFactory.ImpactSound1HullWood = wotReader.ReadString("Sound_1 String ");
            bulletCustomInfoFactory.ImpactSound2HullWood = wotReader.ReadString("Sound_2 String ");
            bulletCustomInfoFactory.ImpactSound3HullWood = wotReader.ReadString("Sound_3 String ");
            bulletCustomInfoFactory.ImpactSound4HullWood = wotReader.ReadString("Sound_4 String ");
            wotReader.ReadLine(); // End section

            bulletCustomInfoFactory.RicochetSoundHullReInforcedWood = wotReader.ReadString("RicochetSound: Hull ReInforced Wood String ");
            wotReader.ReadLine(); // 00000005 ImpactSoundsHull Wood
            wotReader.ReadLine(); // Start section
            bulletCustomInfoFactory.ImpactSound0HullReInforcedWood = wotReader.ReadString("Sound_0 String ");
            bulletCustomInfoFactory.ImpactSound1HullReInforcedWood = wotReader.ReadString("Sound_1 String ");
            bulletCustomInfoFactory.ImpactSound2HullReInforcedWood = wotReader.ReadString("Sound_2 String ");
            bulletCustomInfoFactory.ImpactSound3HullReInforcedWood = wotReader.ReadString("Sound_3 String ");
            bulletCustomInfoFactory.ImpactSound4HullReInforcedWood = wotReader.ReadString("Sound_4 String ");
            wotReader.ReadLine(); // End section

            bulletCustomInfoFactory.RicochetSoundHullIron = wotReader.ReadString("RicochetSound: Hull Iron String ");
            wotReader.ReadLine(); // 00000005 ImpactSoundsHull Wood
            wotReader.ReadLine(); // Start section
            bulletCustomInfoFactory.ImpactSound0HullIron = wotReader.ReadString("Sound_0 String ");
            bulletCustomInfoFactory.ImpactSound1HullIron = wotReader.ReadString("Sound_1 String ");
            bulletCustomInfoFactory.ImpactSound2HullIron = wotReader.ReadString("Sound_2 String ");
            bulletCustomInfoFactory.ImpactSound3HullIron = wotReader.ReadString("Sound_3 String ");
            bulletCustomInfoFactory.ImpactSound4HullIron = wotReader.ReadString("Sound_4 String ");
            wotReader.ReadLine(); // End section

            bulletCustomInfoFactory.RicochetSoundSailCloth = wotReader.ReadString("RicochetSound: Sail Cloth String ");
            wotReader.ReadLine(); // 00000005 ImpactSoundsHull Wood
            wotReader.ReadLine(); // Start section
            bulletCustomInfoFactory.ImpactSound0SailCloth = wotReader.ReadString("Sound_0 String ");
            bulletCustomInfoFactory.ImpactSound1SailCloth = wotReader.ReadString("Sound_1 String ");
            bulletCustomInfoFactory.ImpactSound2SailCloth = wotReader.ReadString("Sound_2 String ");
            bulletCustomInfoFactory.ImpactSound3SailCloth = wotReader.ReadString("Sound_3 String ");
            bulletCustomInfoFactory.ImpactSound4SailCloth = wotReader.ReadString("Sound_4 String ");
            wotReader.ReadLine(); // End section

            bulletCustomInfoFactory.RicochetSoundWallStone = wotReader.ReadString("RicochetSound: Wall Stone String ");
            wotReader.ReadLine(); // 00000005 ImpactSoundsHull Wood
            wotReader.ReadLine(); // Start section
            bulletCustomInfoFactory.ImpactSound0WallStone = wotReader.ReadString("Sound_0 String ");
            bulletCustomInfoFactory.ImpactSound1WallStone = wotReader.ReadString("Sound_1 String ");
            bulletCustomInfoFactory.ImpactSound2WallStone = wotReader.ReadString("Sound_2 String ");
            bulletCustomInfoFactory.ImpactSound3WallStone = wotReader.ReadString("Sound_3 String ");
            bulletCustomInfoFactory.ImpactSound4WallStone = wotReader.ReadString("Sound_4 String ");
            wotReader.ReadLine(); // End section

            bulletCustomInfoFactory.RicochetSoundDragonScale = wotReader.ReadString("RicochetSound: Dragon Scale String ");
            wotReader.ReadLine(); // 00000005 ImpactSoundsHull Wood
            wotReader.ReadLine(); // Start section
            bulletCustomInfoFactory.ImpactSound0DragonScale = wotReader.ReadString("Sound_0 String ");
            bulletCustomInfoFactory.ImpactSound1DragonScale = wotReader.ReadString("Sound_1 String ");
            bulletCustomInfoFactory.ImpactSound2DragonScale = wotReader.ReadString("Sound_2 String ");
            bulletCustomInfoFactory.ImpactSound3DragonScale = wotReader.ReadString("Sound_3 String ");
            bulletCustomInfoFactory.ImpactSound4DragonScale = wotReader.ReadString("Sound_4 String ");
            wotReader.ReadLine(); // End section

            bulletCustomInfoFactory.TravelSound = wotReader.ReadString("Travel Sound String ");
            if (hasLifetime)
                bulletCustomInfoFactory.Lifetime = wotReader.ReadAndParseFloat("Lifetime Float ", ignoreFormatError);

            return bulletCustomInfoFactory;
        }

        private static void ReadDamagePotential(StreamReader wotReader, DamagePotential damagePotential, bool ignoreFormatError)
        {
            wotReader.ReadLine(); // #
            wotReader.ReadLine(); // # Hull Wood DamagePotential
            wotReader.ReadLine(); // #
            damagePotential.ChanceOfCriticalDamageWood = wotReader.ReadAndParseFloat("Chance of Critical Damage: Wood Float ", ignoreFormatError);
            damagePotential.ChanceOfFireWood = wotReader.ReadAndParseFloat("Chance of Fire: Wood Float ", ignoreFormatError);
            damagePotential.InitialFireStrengthWood = wotReader.ReadAndParseFloat("Initial Fire Strength: Wood Float ", ignoreFormatError);
            damagePotential.DamageLowerBoundWood = wotReader.ReadAndParseFloat("Damage Lower Bound: Wood Float ", ignoreFormatError);
            damagePotential.DamageUpperBoundWood = wotReader.ReadAndParseFloat("Damage Upper Bound: Wood Float ", ignoreFormatError);
            wotReader.ReadLine(); // #
            wotReader.ReadLine(); // # Hull ReInforced Wood DamagePotential
            wotReader.ReadLine(); // #
            damagePotential.ChanceOfCriticalDamageReinforced = wotReader.ReadAndParseFloat("Chance of Critical Damage: Reinforced Float ", ignoreFormatError);
            damagePotential.ChanceOfFireReinforced = wotReader.ReadAndParseFloat("Chance of Fire: Reinforced Float ", ignoreFormatError);
            damagePotential.InitialFireStrengthReinforced = wotReader.ReadAndParseFloat("Initial Fire Strength: Reinforced Float ", ignoreFormatError);
            damagePotential.DamageLowerBoundReinforced = wotReader.ReadAndParseFloat("Damage Lower Bound: Reinforced Float ", ignoreFormatError);
            damagePotential.DamageUpperBoundReinforced = wotReader.ReadAndParseFloat("Damage Upper Bound: Reinforced Float ", ignoreFormatError);
            wotReader.ReadLine(); // #
            wotReader.ReadLine(); // # Hull Iron DamagePotential
            wotReader.ReadLine(); // #
            damagePotential.ChanceOfCriticalDamageIron = wotReader.ReadAndParseFloat("Chance of Critical Damage: Iron Float ", ignoreFormatError);
            damagePotential.ChanceOfFireIron = wotReader.ReadAndParseFloat("Chance of Fire: Iron Float ", ignoreFormatError);
            damagePotential.InitialFireStrengthIron = wotReader.ReadAndParseFloat("Initial Fire Strength: Iron Float ", ignoreFormatError);
            damagePotential.DamageLowerBoundIron = wotReader.ReadAndParseFloat("Damage Lower Bound: Iron Float ", ignoreFormatError);
            damagePotential.DamageUpperBoundIron = wotReader.ReadAndParseFloat("Damage Upper Bound: Iron Float ", ignoreFormatError);
            wotReader.ReadLine(); // #
            wotReader.ReadLine(); // # Sail Cloth DamagePotential
            wotReader.ReadLine(); // #
            damagePotential.ChanceOfCriticalDamageCloth = wotReader.ReadAndParseFloat("Chance of Critical Damage: Cloth Float ", ignoreFormatError);
            damagePotential.ChanceOfFireCloth = wotReader.ReadAndParseFloat("Chance of Fire: Cloth Float ", ignoreFormatError);
            damagePotential.InitialFireStrengthCloth = wotReader.ReadAndParseFloat("Initial Fire Strength: Cloth Float ", ignoreFormatError);
            damagePotential.DamageLowerBoundCloth = wotReader.ReadAndParseFloat("Damage Lower Bound: Cloth Float ", ignoreFormatError);
            damagePotential.DamageUpperBoundCloth = wotReader.ReadAndParseFloat("Damage Upper Bound: Cloth Float ", ignoreFormatError);
            wotReader.ReadLine(); // #
            wotReader.ReadLine(); // # Wall Stone DamagePotential
            wotReader.ReadLine(); // #
            damagePotential.ChanceOfCriticalDamageStone = wotReader.ReadAndParseFloat("Chance of Critical Damage: Stone Float ", ignoreFormatError);
            damagePotential.ChanceOfFireStone = wotReader.ReadAndParseFloat("Chance of Fire: Stone Float ", ignoreFormatError);
            damagePotential.InitialFireStrengthStone = wotReader.ReadAndParseFloat("Initial Fire Strength: Stone Float ", ignoreFormatError);
            damagePotential.DamageLowerBoundStone = wotReader.ReadAndParseFloat("Damage Lower Bound: Stone Float ", ignoreFormatError);
            damagePotential.DamageUpperBoundStone = wotReader.ReadAndParseFloat("Damage Upper Bound: Stone Float ", ignoreFormatError);
            wotReader.ReadLine(); // #
            wotReader.ReadLine(); // # Dragon Scale DamagePotential
            wotReader.ReadLine(); // #
            damagePotential.ChanceOfCriticalDamageDragon = wotReader.ReadAndParseFloat("Chance of Critical Damage: Dragon Float ", ignoreFormatError);
            damagePotential.ChanceOfFireDragon = wotReader.ReadAndParseFloat("Chance of Fire: Dragon Float ", ignoreFormatError);
            damagePotential.InitialFireStrengthDragon = wotReader.ReadAndParseFloat("Initial Fire Strength: Dragon Float ", ignoreFormatError);
            damagePotential.DamageLowerBoundDragon = wotReader.ReadAndParseFloat("Damage Lower Bound: Dragon Float ", ignoreFormatError);
            damagePotential.DamageUpperBoundDragon = wotReader.ReadAndParseFloat("Damage Upper Bound: Dragon Float ", ignoreFormatError);
        }

        #endregion

        #region Write definitions

        private static void WriteAIEntityDefinition(StreamWriter wotWriter, AiEntityDefinition? aiEntityDefinition)
        {
            if (aiEntityDefinition != null)
            {
                wotWriter.WriteLine($"EntityType String '{aiEntityDefinition.Type}'");
                switch (aiEntityDefinition)
                {
                    case DragonAI dragonAI:
                        WriteDragonAI(wotWriter, dragonAI);
                        break;
                    case SpaceAnimalAI spaceAnimalAI:
                        WriteSpaceAnimalAI(wotWriter, spaceAnimalAI);
                        break;
                    case SpaceObjectAI spaceObjectAI:
                        WriteSpaceObjectAI(wotWriter, spaceObjectAI);
                        break;
                    case IslandAI islandAI:
                        WriteIslandAI(wotWriter, islandAI);
                        break;
                    case MineAI mineAI:
                        WriteMineAI(wotWriter, mineAI);
                        break;
                    case GunAI gunAI:
                        WriteGunAI(wotWriter, gunAI);
                        break;
                    case VolcanoAI volcanoAI:
                        WriteVolcanoAI(wotWriter, volcanoAI);
                        break;
                    case ShipAI shipAI:
                        WriteShipAI(wotWriter, shipAI);
                        break;
                    default:
                        throw new TPException($"AIENTITYDEFINITION '{aiEntityDefinition.GetType().Name}' is not a valid factory.");
                }
            }
            else
                wotWriter.WriteLine("EntityType String ''");
        }

        private static void WriteRenderEntityDefinition(StreamWriter wotWriter, RenderEntityDefinition? renderEntityDefinition)
        {
            if (renderEntityDefinition != null)
            {
                wotWriter.WriteLine($"EntityType String '{renderEntityDefinition.Type}'");
                switch (renderEntityDefinition)
                {
                    case RenderEntityFactory renderEntityFactory:
                        WriteRenderEntityFactory(wotWriter, renderEntityFactory);
                        break;
                    default:
                        throw new TPException($"RENDERENTITYDEFINITION '{renderEntityDefinition.GetType().Name}' is not a valid factory.");
                }
            }
            else
                wotWriter.WriteLine("EntityType String ''");
        }

        private static void WritePhysicsDefinition(StreamWriter wotWriter, PhysicsDefinition? physicsDefinition)
        {
            if (physicsDefinition != null)
            {
                wotWriter.WriteLine($"EntityType String '{physicsDefinition.Type}'");
                switch (physicsDefinition)
                {
                    case DragonPhysics dragonPhysics:
                        WriteDragonPhysics(wotWriter, dragonPhysics);
                        break;
                    case WhalePhysics whalePhysics:
                        WriteWhalePhysics(wotWriter, whalePhysics);
                        break;
                    case SpaceObjectPhysics spaceObjectPhysics:
                        WriteSpaceObjectPhysics(wotWriter, spaceObjectPhysics);
                        break;
                    case ProjectilePhysics projectilePhysics:
                        WriteProjectilePhysics(wotWriter, projectilePhysics);
                        break;
                    case MinePhysics minePhysics:
                        WriteMinePhysics(wotWriter, minePhysics);
                        break;
                    case TorpedoPhysics torpedoPhysics:
                        WriteTorpedoPhysics(wotWriter, torpedoPhysics);
                        break;
                    case ShipDemo shipDemo:
                        WriteShipDemo(wotWriter, shipDemo);
                        break;
                    default:
                        throw new TPException($"PHYSICSDEFINITION '{physicsDefinition.GetType().Name}' is not a valid factory.");
                }
            }
            else
                wotWriter.WriteLine("EntityType String ''");
        }

        private static void WriteCollisionDefinition(StreamWriter wotWriter, CollisionDefinition? collisionDefinition)
        {
            if (collisionDefinition != null)
            {
                wotWriter.WriteLine($"EntityType String '{collisionDefinition.Type}'");
                switch (collisionDefinition)
                {
                    case BoundingRenderEntity boundingRenderEntity:
                        WriteBoundingRenderEntity(wotWriter, boundingRenderEntity);
                        break;
                    case Point point:
                        WritePoint(wotWriter, point);
                        break;
                    case BoundingSphere boundingSphere:
                        WriteBoundingSphere(wotWriter, boundingSphere);
                        break;
                    default:
                        throw new TPException($"COLLISIONDEFINITION '{collisionDefinition.GetType().Name}' is not a valid factory.");
                }
            }
            else
                wotWriter.WriteLine("EntityType String ''");
        }

        private static void WriteCustomInfoDefinition(StreamWriter wotWriter, CustomInfoDefinition? customInfoDefinition)
        {
            if (customInfoDefinition != null)
            {
                wotWriter.WriteLine($"EntityType String '{customInfoDefinition.Type}'");
                switch (customInfoDefinition)
                {
                    case DragonCustomInfoFactory dragonCustomInfoFactory:
                        WriteDragonCustomInfoFactory(wotWriter, dragonCustomInfoFactory);
                        break;
                    case SpaceAnimalCustomInfoFactory spaceAnimalCustomInfoFactory:
                        WriteSpaceAnimalCustomInfoFactory(wotWriter, spaceAnimalCustomInfoFactory);
                        break;
                    case AsteroidCustomInfoFactory asteroidCustomInfoFactory:
                        WriteAsteroidCustomInfoFactory(wotWriter, asteroidCustomInfoFactory);
                        break;
                    case IslandCustomInfoFactory islandCustomInfoFactory:
                        WriteIslandCustomInfoFactory(wotWriter, islandCustomInfoFactory);
                        break;
                    case EnergyNetCustomInfoFactory energyNetCustomInfoFactory:
                        WriteEnergyNetCustomInfoFactory(wotWriter, energyNetCustomInfoFactory);
                        break;
                    case GrapplingHarpoonCustomInfoFactory grapplingHarpoonCustomInfoFactory:
                        WriteGrapplingHarpoonCustomInfoFactory(wotWriter, grapplingHarpoonCustomInfoFactory);
                        break;
                    case GravChargeCustomInfoFactory gravChargeCustomInfoFactory:
                        WriteGravChargeCustomInfoFactory(wotWriter, gravChargeCustomInfoFactory);
                        break;
                    case NovaMortarCustomInfoFactory novaMortarCustomInfoFactory:
                        WriteNovaMortarCustomInfoFactory(wotWriter, novaMortarCustomInfoFactory);
                        break;
                    case MineCustomInfoFactory mineCustomInfoFactory:
                        WriteMineCustomInfoFactory(wotWriter, mineCustomInfoFactory);
                        break;
                    case StarMortarCustomInfoFactory starMortarCustomInfoFactory:
                        WriteStarMortarCustomInfoFactory(wotWriter, starMortarCustomInfoFactory);
                        break;
                    case TorpedoCustomInfoFactory torpedoCustomInfoFactory:
                        WriteTorpedoCustomInfoFactory(wotWriter, torpedoCustomInfoFactory);
                        break;
                    case BulletCustomInfoFactory bulletCustomInfoFactory:
                        WriteBulletCustomInfoFactory(wotWriter, bulletCustomInfoFactory);
                        break;
                    case CrewCustomInfoFactory crewCustomInfoFactory:
                        WriteCrewCustomInfoFactory(wotWriter, crewCustomInfoFactory);
                        break;
                    case GunCustomInfoFactory gunCustomInfoFactory:
                        WriteGunCustomInfoFactory(wotWriter, gunCustomInfoFactory);
                        break;
                    case ShipCustomInfoFactory shipCustomInfoFactory:
                        WriteShipCustomInfoFactory(wotWriter, shipCustomInfoFactory);
                        break;
                    case ShipDebrisCustomInfoFactory shipDebrisCustomInfoFactory:
                        WriteShipDebrisCustomInfoFactory(wotWriter, shipDebrisCustomInfoFactory);
                        break;
                    case BlackHoleCustomInfoFactory blackHoleCustomInfoFactory:
                        WriteBlackHoleCustomInfoFactory(wotWriter, blackHoleCustomInfoFactory);
                        break;
                    case EtheriumCurrentCustomInfoFactory etheriumCurrentCustomInfoFactory:
                        WriteEtheriumCurrentCustomInfoFactory(wotWriter, etheriumCurrentCustomInfoFactory);
                        break;
                    case NebulaCustomInfoFactory nebulaCustomInfoFactory:
                        WriteNebulaCustomInfoFactory(wotWriter, nebulaCustomInfoFactory);
                        break;
                    default:
                        throw new TPException($"CUSTOMINFODEFINITION '{customInfoDefinition.GetType().Name}' is not a valid factory.");
                }
            }
            else
                wotWriter.WriteLine("EntityType String ''");
        }

        #endregion

        #region Write AIEntity factories

        private static void WriteDragonAI(StreamWriter wotWriter, DragonAI dragonAI)
        {
            wotWriter.WriteLine($"FactoryType String 'DragonAI'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"Default SightRange Float {dragonAI.DefaultSightRange:F6}", lvl);
            });
        }

        private static void WriteSpaceAnimalAI(StreamWriter wotWriter, SpaceAnimalAI spaceAnimalAI)
        {
            wotWriter.WriteLine($"FactoryType String 'SpaceAnimalAI'");
            wotWriter.WriteSection("ConstData");
        }

        private static void WriteSpaceObjectAI(StreamWriter wotWriter, SpaceObjectAI spaceObjectAI)
        {
            wotWriter.WriteLine($"FactoryType String 'SpaceObjectAI'");
            wotWriter.WriteSection("ConstData");
        }

        private static void WriteIslandAI(StreamWriter wotWriter, IslandAI islandAI)
        {
            wotWriter.WriteLine($"FactoryType String 'IslandAI'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"Default SightRange Float {islandAI.DefaultSightRange:F6}", lvl);
            });
        }

        private static void WriteMineAI(StreamWriter wotWriter, MineAI mineAI)
        {
            wotWriter.WriteLine($"FactoryType String 'MineAI'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"Default SightRange Float {mineAI.DefaultSightRange:F6}", lvl);
            });
        }

        private static void WriteGunAI(StreamWriter wotWriter, GunAI gunAI)
        {
            wotWriter.WriteLine($"FactoryType String 'GunAI'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"Is Lobbing Gun Bool {gunAI.IsLobbingGun}", lvl);
                wotWriter.WriteLineLevel($"Is MineLaying Gun Bool {gunAI.IsMineLayingGun}", lvl);
                wotWriter.WriteLineLevel($"Is TorpedoLauncher Gun Bool {gunAI.IsTorpedoLauncherGun}", lvl);
                wotWriter.WriteLineLevel($"Is PointDefense Gun Bool {gunAI.IsPointDefenseGun}", lvl);
            });
        }

        private static void WriteVolcanoAI(StreamWriter wotWriter, VolcanoAI volcanoAI)
        {
            wotWriter.WriteLine($"FactoryType String 'VolcanoAI'");
            wotWriter.WriteSection("ConstData");
        }

        private static void WriteShipAI(StreamWriter wotWriter, ShipAI shipAI)
        {
            wotWriter.WriteLine($"FactoryType String 'ShipAI'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"Default SightRange Float {shipAI.DefaultSightRange:F6}", lvl);
            });
        }

        #endregion

        #region Write RenderEntity factories

        private static void WriteRenderEntityFactory(StreamWriter wotWriter, RenderEntityFactory renderEntityFactory)
        {
            wotWriter.WriteLine($"FactoryType String 'RenderEntityFactory'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"Distant Bool {renderEntityFactory.Distant}", lvl);
                wotWriter.WriteLineLevel($"Mesh Scene Name String '{renderEntityFactory.MeshSceneName}'", lvl);
                wotWriter.WriteLineLevel($"Render Effect String '{renderEntityFactory.RenderEffectName}'", lvl);
                wotWriter.WriteLineLevel($"Visible Bool {renderEntityFactory.Visible}", lvl);
                WriteMeshAttributeManager(wotWriter, renderEntityFactory.MeshAttributes, lvl);
                wotWriter.WriteLineLevel($"ROTATIONSPEED Float {renderEntityFactory.RotationSpeed:F6}", lvl);
            });
        }

        private static void WriteMeshAttributeManager(StreamWriter wotWriter, IList<MeshAttribute> meshAttributes, int level)
        {
            wotWriter.WriteSection("Mesh Attribute Manager", (lvl) =>
            {
                wotWriter.WriteLineLevel($"MeshAttributes - Size Int {meshAttributes.Count}", lvl);
                for (int i = 0; i < meshAttributes.Count; i++)
                {
                    WriteMeshAttribute(wotWriter, meshAttributes[i], lvl);
                }
            }, level);
        }

        private static void WriteMeshAttribute(StreamWriter wotWriter, MeshAttribute meshAttribute, int level)
        {
            wotWriter.WriteSection("MeshAttributes - Element", (lvl) =>
            {
                wotWriter.WriteLineLevel($"MeshName String '{meshAttribute.MeshName}'", lvl);
                wotWriter.WriteLineLevel($"MeshAttribute - Size Int {meshAttribute.MeshSubAttributes.Count}", lvl);
                for (int i = 0; i < meshAttribute.MeshSubAttributes.Count; i++)
                {
                    WriteMeshSubAttribute(wotWriter, meshAttribute.MeshSubAttributes[i], lvl);
                }
            }, level);
        }

        private static void WriteMeshSubAttribute(StreamWriter wotWriter, MeshSubAttribute meshSubAttribute, int level)
        {
            wotWriter.WriteSection("MeshAttribute - Element", (lvl) =>
            {
                switch (meshSubAttribute)
                {
                    case GunPlacement gunPlacement:
                        WriteGunPlacement(wotWriter, gunPlacement, lvl);
                        break;
                    case DamageSection damageSection:
                        WriteDamageSection(wotWriter, damageSection, lvl);
                        break;
                    case MeshHitPoints meshHitPoints:
                        WriteMeshHitPoints(wotWriter, meshHitPoints, lvl);
                        break;
                    case DockPoint dockPoint:
                        WriteDockPoint(wotWriter, dockPoint, lvl);
                        break;
                    case FlagAttachmentPoint flagAttachmentPoint:
                        WriteFlagAttachmentPoint(wotWriter, flagAttachmentPoint, lvl);
                        break;
                    case PlayEffect playEffect:
                        WritePlayEffect(wotWriter, playEffect, lvl);
                        break;
                    case BoardingEffectPoint boardingEffectPoint:
                        WriteBoardingEffectPoint(wotWriter, boardingEffectPoint, lvl);
                        break;
                    case VolcanoSmoke volcanoSmoke:
                        WriteVolcanoSmoke(wotWriter, volcanoSmoke, lvl);
                        break;
                    case EnginePortPlacement enginePortPlacement:
                        WriteEnginePortPlacement(wotWriter, enginePortPlacement, lvl);
                        break;
                    case GunMuzzlePlacement gunMuzzlePlacement:
                        WriteGunMuzzlePlacement(wotWriter, gunMuzzlePlacement, lvl);
                        break;
                    case GunVerticalPivotPlacement gunVerticalPivotPlacement:
                        WriteGunVerticalPivotPlacement(wotWriter, gunVerticalPivotPlacement, lvl);
                        break;
                    case WakePlacement wakePlacement:
                        WriteWakePlacement(wotWriter, wakePlacement, lvl);
                        break;
                    case ToweePoint toweePoint:
                        WriteToweePoint(wotWriter, toweePoint, lvl);
                        break;
                    case TowerPoint towerPoint:
                        WriteTowerPoint(wotWriter, towerPoint, lvl);
                        break;
                    case TorpedoHomingPoint torpedoHomingPoint:
                        WriteTorpedoHomingPoint(wotWriter, torpedoHomingPoint, lvl);
                        break;
                    case CloakingEffectPoint cloakingEffectPoint:
                        WriteCloakingEffectPoint(wotWriter, cloakingEffectPoint, lvl);
                        break;
                    case FullyCloakedEffectPoint fullyCloakedEffectPoint:
                        WriteFullyCloakedEffectPoint(wotWriter, fullyCloakedEffectPoint, lvl);
                        break;
                    default:
                        throw new TPException($"'{meshSubAttribute.GetType().Name}' is not a valid mesh attribute.");
                }
            }, level);
        }

        private static void WriteGunPlacement(StreamWriter wotWriter, GunPlacement gunPlacement, int level)
        {
            wotWriter.WriteLineLevel("AttributeName String 'GunPlacement'", level);
            wotWriter.WriteLineLevel($"DescriptorName String '{gunPlacement.DescriptorName}'", level);
            wotWriter.WriteLineLevel($"AssociationName String '{gunPlacement.GunName},{gunPlacement.MaxRotation},{gunPlacement.Bank.GetName()}'", level);
        }

        private static void WriteDamageSection(StreamWriter wotWriter, DamageSection damageSection, int level)
        {
            wotWriter.WriteLineLevel("AttributeName String 'DamageSection'", level);
            wotWriter.WriteLineLevel($"DescriptorName String '{damageSection.DescriptorName}'", level);
            wotWriter.WriteLevel($"AssociationName String '", level);
            wotWriter.Write($"vitalToShip={(damageSection.VitalToShip ? '1' : '0')}");
            wotWriter.Write($",vitalToMaxVelocity={(damageSection.VitalToMaxVelocity ? '1' : '0')}");
            wotWriter.Write($",vitalToManeuverability={(damageSection.VitalToManeuverability ? '1' : '0')}");
            wotWriter.Write($",vitalToMission={(damageSection.VitalToMission ? '1' : '0')}");
            wotWriter.Write($",swappable={(damageSection.Swappable ? '1' : '0')}");
            wotWriter.Write($",material={damageSection.Material.GetName()}");
            wotWriter.Write($",hitpoints={damageSection.Hitpoints}");
            if (damageSection.VitalSectionCoreDamagePercent > 0)
                wotWriter.Write($",vitalSectionCoreDamagePercent={damageSection.VitalSectionCoreDamagePercent}");
            var criticals = string.Join("+", damageSection.Criticals.Select(x => x.GetName()));
            if (!string.IsNullOrEmpty(criticals))
                wotWriter.Write($",criticals={criticals}");
            var adjacents = string.Join("+", damageSection.Adjacents);
            if (!string.IsNullOrEmpty(adjacents))
                wotWriter.Write($",adjacent={adjacents}");
            if (!string.IsNullOrEmpty(damageSection.DebrisMeshScene))
                wotWriter.Write($",debrisMeshScene={damageSection.DebrisMeshScene}");
            if (!string.IsNullOrEmpty(damageSection.DebrisFx))
                wotWriter.Write($",debrisFx={damageSection.DebrisFx}");
            for (int i = 0; i < damageSection.DestructFxs.Count; i++)
            {
                var destructFx = damageSection.DestructFxs[i];
                wotWriter.Write($",destructFx={{ {destructFx.Effect} {destructFx.A:F6} {destructFx.B:F6} ( {destructFx.X:F6} {destructFx.Y:F6} {destructFx.Z:F6} ) }}");
            }
            wotWriter.WriteLineLevel("'", 0);
        }

        private static void WriteMeshHitPoints(StreamWriter wotWriter, MeshHitPoints meshHitPoints, int level)
        {
            wotWriter.WriteLineLevel("AttributeName String 'MeshHitPoints'", level);
            wotWriter.WriteLineLevel($"DescriptorName String '{meshHitPoints.DescriptorName}'", level);
            wotWriter.WriteLineLevel($"AssociationName String '{meshHitPoints.AssociationName}'", level);
        }

        private static void WriteDockPoint(StreamWriter wotWriter, DockPoint dockPoint, int level)
        {
            wotWriter.WriteLineLevel("AttributeName String 'DockPoint'", level);
            wotWriter.WriteLineLevel($"DescriptorName String '{dockPoint.DescriptorName}'", level);
            wotWriter.WriteLineLevel($"AssociationName String '{dockPoint.AnimationName}'", level);
        }

        private static void WriteFlagAttachmentPoint(StreamWriter wotWriter, FlagAttachmentPoint flagAttachmentPoint, int level)
        {
            wotWriter.WriteLineLevel("AttributeName String 'FlagAttachmentPoint'", level);
            wotWriter.WriteLineLevel($"DescriptorName String '{flagAttachmentPoint.DescriptorName}'", level);
            wotWriter.WriteLineLevel($"AssociationName String ''", level);
        }

        private static void WritePlayEffect(StreamWriter wotWriter, PlayEffect playEffect, int level)
        {
            wotWriter.WriteLineLevel("AttributeName String 'PlayEffect'", level);
            wotWriter.WriteLineLevel($"DescriptorName String '{playEffect.DescriptorName}'", level);
            wotWriter.WriteLineLevel($"AssociationName String '{playEffect.EffectName}'", level);
        }

        private static void WriteBoardingEffectPoint(StreamWriter wotWriter, BoardingEffectPoint boardingEffectPoint, int level)
        {
            wotWriter.WriteLineLevel("AttributeName String 'BoardingEffectPoint'", level);
            wotWriter.WriteLineLevel($"DescriptorName String '{boardingEffectPoint.DescriptorName}'", level);
            wotWriter.WriteLineLevel($"AssociationName String ''", level);
        }

        private static void WriteVolcanoSmoke(StreamWriter wotWriter, VolcanoSmoke volcanoSmoke, int level)
        {
            wotWriter.WriteLineLevel("AttributeName String 'VolcanoSmoke'", level);
            wotWriter.WriteLineLevel($"DescriptorName String '{volcanoSmoke.DescriptorName}'", level);
            wotWriter.WriteLineLevel($"AssociationName String ''", level);
        }

        private static void WriteEnginePortPlacement(StreamWriter wotWriter, EnginePortPlacement enginePortPlacement, int level)
        {
            wotWriter.WriteLineLevel("AttributeName String 'EnginePortPlacement'", level);
            wotWriter.WriteLineLevel($"DescriptorName String '{enginePortPlacement.DescriptorName}'", level);
            wotWriter.WriteLineLevel($"AssociationName String '{enginePortPlacement.EffectName}'", level);
        }

        private static void WriteGunMuzzlePlacement(StreamWriter wotWriter, GunMuzzlePlacement gunMuzzlePlacement, int level)
        {
            wotWriter.WriteLineLevel("AttributeName String 'GunMuzzlePlacement'", level);
            wotWriter.WriteLineLevel($"DescriptorName String '{gunMuzzlePlacement.DescriptorName}'", level);
            wotWriter.WriteLineLevel($"AssociationName String ''", level);
        }

        private static void WriteGunVerticalPivotPlacement(StreamWriter wotWriter, GunVerticalPivotPlacement gunVerticalPivotPlacement, int level)
        {
            wotWriter.WriteLineLevel("AttributeName String 'GunVerticalPivotPlacement'", level);
            wotWriter.WriteLineLevel($"DescriptorName String '{gunVerticalPivotPlacement.DescriptorName}'", level);
            wotWriter.WriteLineLevel($"AssociationName String ''", level);
        }

        private static void WriteWakePlacement(StreamWriter wotWriter, WakePlacement wakePlacement, int level)
        {
            wotWriter.WriteLineLevel("AttributeName String 'WakePlacement'", level);
            wotWriter.WriteLineLevel($"DescriptorName String '{wakePlacement.DescriptorName}'", level);
            wotWriter.WriteLineLevel($"AssociationName String '{wakePlacement.EffectName}'", level);
        }

        private static void WriteToweePoint(StreamWriter wotWriter, ToweePoint toweePoint, int level)
        {
            wotWriter.WriteLineLevel("AttributeName String 'ToweePoint'", level);
            wotWriter.WriteLineLevel($"DescriptorName String '{toweePoint.DescriptorName}'", level);
            wotWriter.WriteLineLevel($"AssociationName String ''", level);
        }

        private static void WriteTowerPoint(StreamWriter wotWriter, TowerPoint towerPoint, int level)
        {
            wotWriter.WriteLineLevel("AttributeName String 'TowerPoint'", level);
            wotWriter.WriteLineLevel($"DescriptorName String '{towerPoint.DescriptorName}'", level);
            wotWriter.WriteLineLevel($"AssociationName String ''", level);
        }

        private static void WriteTorpedoHomingPoint(StreamWriter wotWriter, TorpedoHomingPoint torpedoHomingPoint, int level)
        {
            wotWriter.WriteLineLevel("AttributeName String 'TorpedoHomingPoint'", level);
            wotWriter.WriteLineLevel($"DescriptorName String '{torpedoHomingPoint.DescriptorName}'", level);
            wotWriter.WriteLineLevel($"AssociationName String ''", level);
        }

        private static void WriteCloakingEffectPoint(StreamWriter wotWriter, CloakingEffectPoint cloakingEffectPoint, int level)
        {
            wotWriter.WriteLineLevel("AttributeName String 'CloakingEffectPoint'", level);
            wotWriter.WriteLineLevel($"DescriptorName String '{cloakingEffectPoint.DescriptorName}'", level);
            wotWriter.WriteLineLevel($"AssociationName String ''", level);
        }

        private static void WriteFullyCloakedEffectPoint(StreamWriter wotWriter, FullyCloakedEffectPoint fullyCloakedEffectPoint, int level)
        {
            wotWriter.WriteLineLevel("AttributeName String 'FullyCloakedEffectPoint'", level);
            wotWriter.WriteLineLevel($"DescriptorName String '{fullyCloakedEffectPoint.DescriptorName}'", level);
            wotWriter.WriteLineLevel($"AssociationName String ''", level);
        }

        #endregion

        #region Write Physics factories

        private static void WriteDragonPhysics(StreamWriter wotWriter, DragonPhysics dragonPhysics)
        {
            wotWriter.WriteLine($"FactoryType String 'DragonPhysics'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"CenterOfMass Vector3( {dragonPhysics.CenterOfMass.X:F6}, {dragonPhysics.CenterOfMass.Y:F6}, {dragonPhysics.CenterOfMass.Z:F6} )", lvl);
                wotWriter.WriteLineLevel($"Mass Float {dragonPhysics.Mass:F6}", lvl);
                wotWriter.WriteLineLevel($"MaxThrust Float {dragonPhysics.MaxThrust:F6}", lvl);
                wotWriter.WriteLineLevel($"MaxSpeed Float {dragonPhysics.MaxSpeed:F6}", lvl);
                wotWriter.WriteLineLevel($"RotationalFriction Float {dragonPhysics.RotationalFriction:F6}", lvl);
                wotWriter.WriteLineLevel($"MaxAngularAcceleration Float {dragonPhysics.MaxAngularAcceleration:F6}", lvl);
            });
        }

        private static void WriteWhalePhysics(StreamWriter wotWriter, WhalePhysics whalePhysics)
        {
            wotWriter.WriteLine($"FactoryType String 'Whale Physics'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"CenterOfMass Vector3( {whalePhysics.CenterOfMass.X:F6}, {whalePhysics.CenterOfMass.Y:F6}, {whalePhysics.CenterOfMass.Z:F6} )", lvl);
                wotWriter.WriteLineLevel($"Mass Float {whalePhysics.Mass:F6}", lvl);
                wotWriter.WriteLineLevel($"MaxThrust Float {whalePhysics.MaxThrust:F6}", lvl);
                wotWriter.WriteLineLevel($"MaxSpeed Float {whalePhysics.MaxSpeed:F6}", lvl);
                wotWriter.WriteLineLevel($"RotationalFriction Float {whalePhysics.RotationalFriction:F6}", lvl);
                wotWriter.WriteLineLevel($"MaxAngularAcceleration Float {whalePhysics.MaxAngularAcceleration:F6}", lvl);
                wotWriter.WriteLineLevel($"Max Dive Pitch Float {whalePhysics.MaxDivePitch:F6}", lvl);
                wotWriter.WriteLineLevel($"Max Climb Pitch Float {whalePhysics.MaxClimbPitch:F6}", lvl);
                wotWriter.WriteLineLevel($"Dive Time Float {whalePhysics.DiveTime:F6}", lvl);
            });
        }

        private static void WriteSpaceObjectPhysics(StreamWriter wotWriter, SpaceObjectPhysics spaceObjectPhysics)
        {
            wotWriter.WriteLine($"FactoryType String 'SpaceObjectPhysics'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"CenterOfMass Vector3( {spaceObjectPhysics.CenterOfMass.X:F6}, {spaceObjectPhysics.CenterOfMass.Y:F6}, {spaceObjectPhysics.CenterOfMass.Z:F6} )", lvl);
                wotWriter.WriteLineLevel($"Mass Float {spaceObjectPhysics.Mass:F6}", lvl);
                wotWriter.WriteLineLevel($"Acceleration Float {spaceObjectPhysics.Acceleration:F6}", lvl);
            });
        }

        private static void WriteProjectilePhysics(StreamWriter wotWriter, ProjectilePhysics projectilePhysics)
        {
            wotWriter.WriteLine($"FactoryType String 'ProjectilePhysics'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"CenterOfMass Vector3( {projectilePhysics.CenterOfMass.X:F6}, {projectilePhysics.CenterOfMass.Y:F6}, {projectilePhysics.CenterOfMass.Z:F6} )", lvl);
                wotWriter.WriteLineLevel($"Mass Float {projectilePhysics.Mass:F6}", lvl);
            });
        }

        private static void WriteMinePhysics(StreamWriter wotWriter, MinePhysics minePhysics)
        {
            wotWriter.WriteLine($"FactoryType String 'MinePhysics'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"CenterOfMass Vector3( {minePhysics.CenterOfMass.X:F6}, {minePhysics.CenterOfMass.Y:F6}, {minePhysics.CenterOfMass.Z:F6} )", lvl);
                wotWriter.WriteLineLevel($"Mass Float {minePhysics.Mass:F6}", lvl);
                wotWriter.WriteLineLevel($"Maximum speed Float {minePhysics.MaximumSpeed:F6}", lvl);
            });
        }

        private static void WriteTorpedoPhysics(StreamWriter wotWriter, TorpedoPhysics torpedoPhysics)
        {
            wotWriter.WriteLine($"FactoryType String 'TorpedoPhysics'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"CenterOfMass Vector3( {torpedoPhysics.CenterOfMass.X:F6}, {torpedoPhysics.CenterOfMass.Y:F6}, {torpedoPhysics.CenterOfMass.Z:F6} )", lvl);
                wotWriter.WriteLineLevel($"Mass Float {torpedoPhysics.Mass:F6}", lvl);
                wotWriter.WriteLineLevel($"MaxThrust Float {torpedoPhysics.MaxThrust:F6}", lvl);
                wotWriter.WriteLineLevel($"MaxSpeed Float {torpedoPhysics.MaxSpeed:F6}", lvl);
                wotWriter.WriteLineLevel($"RotationalFriction Float {torpedoPhysics.RotationalFriction:F6}", lvl);
                wotWriter.WriteLineLevel($"MaxAngularAcceleration Float {torpedoPhysics.MaxAngularAcceleration:F6}", lvl);
            });
        }

        private static void WriteShipDemo(StreamWriter wotWriter, ShipDemo shipDemo)
        {
            wotWriter.WriteLine($"FactoryType String 'ShipDemo'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"CenterOfMass Vector3( {shipDemo.CenterOfMass.X:F6}, {shipDemo.CenterOfMass.Y:F6}, {shipDemo.CenterOfMass.Z:F6} )", lvl);
                wotWriter.WriteLineLevel($"Mass Float {shipDemo.Mass:F6}", lvl);
                wotWriter.WriteLineLevel($"MaxThrust Float {shipDemo.MaxThrust:F6}", lvl);
                wotWriter.WriteLineLevel($"MaxSpeed Float {shipDemo.MaxSpeed:F6}", lvl);
                wotWriter.WriteLineLevel($"RotationalFriction Float {shipDemo.RotationalFriction:F6}", lvl);
                wotWriter.WriteLineLevel($"MaxAngularAcceleration Float {shipDemo.MaxAngularAcceleration:F6}", lvl);
            });
        }

        #endregion

        #region Write Collision factories

        private static void WriteBoundingRenderEntity(StreamWriter wotWriter, BoundingRenderEntity boundingRenderEntity)
        {
            wotWriter.WriteLine($"FactoryType String 'BoundingRenderEntity'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"DetectionType String '{boundingRenderEntity.DetectionType.GetName()}'", lvl);
                wotWriter.WriteLineLevel($"ResponseType String '{boundingRenderEntity.ResponseType.GetName()}'", lvl);
                wotWriter.WriteLineLevel($"User defined sphere size Bool {boundingRenderEntity.UserDefinedSphereSize}", lvl);
                if (boundingRenderEntity.UserDefinedSphereSize)
                {
                    wotWriter.WriteLineLevel($"LocalPosition Vector3( {boundingRenderEntity.LocalPosition.X:F6}, {boundingRenderEntity.LocalPosition.Y:F6}, {boundingRenderEntity.LocalPosition.Z:F6} )", lvl);
                    wotWriter.WriteLineLevel($"Radius Float {boundingRenderEntity.Radius:F6}", lvl);
                }
                wotWriter.WriteLineLevel($"UserDefinedBoundingBoxExtents Bool {boundingRenderEntity.UserDefinedBoundingBoxExtents}", lvl);
                if (boundingRenderEntity.UserDefinedBoundingBoxExtents)
                {
                    wotWriter.WriteLineLevel($"MinExtents Vector3( {boundingRenderEntity.MinExtents.X:F6}, {boundingRenderEntity.MinExtents.Y:F6}, {boundingRenderEntity.MinExtents.Z:F6} )", lvl);
                    wotWriter.WriteLineLevel($"MaxExtents Vector3( {boundingRenderEntity.MaxExtents.X:F6}, {boundingRenderEntity.MaxExtents.Y:F6}, {boundingRenderEntity.MaxExtents.Z:F6} )", lvl);
                }
            });
        }

        private static void WritePoint(StreamWriter wotWriter, Point point)
        {
            wotWriter.WriteLine($"FactoryType String 'Point'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"DetectionType String '{point.DetectionType.GetName()}'", lvl);
                wotWriter.WriteLineLevel($"ResponseType String '{point.ResponseType.GetName()}'", lvl);
            });
        }

        private static void WriteBoundingSphere(StreamWriter wotWriter, BoundingSphere boundingSphere)
        {
            wotWriter.WriteLine($"FactoryType String 'BoundingSphere'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"DetectionType String '{boundingSphere.DetectionType.GetName()}'", lvl);
                wotWriter.WriteLineLevel($"ResponseType String '{boundingSphere.ResponseType.GetName()}'", lvl);
                wotWriter.WriteLineLevel($"User defined sphere size Bool {boundingSphere.UserDefinedSphereSize}", lvl);
                if (boundingSphere.UserDefinedSphereSize)
                {
                    wotWriter.WriteLineLevel($"LocalPosition Vector3( {boundingSphere.LocalPosition.X:F6}, {boundingSphere.LocalPosition.Y:F6}, {boundingSphere.LocalPosition.Z:F6} )", lvl);
                    wotWriter.WriteLineLevel($"Radius Float {boundingSphere.Radius:F6}", lvl);
                }
            });
        }

        #endregion

        #region Write CustomInfo factories

        private static void WriteDragonCustomInfoFactory(StreamWriter wotWriter, DragonCustomInfoFactory dragonCustomInfoFactory)
        {
            wotWriter.WriteLine($"FactoryType String 'DragonCustomInfoFactory'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteSection("GUIInfo Chunk", (lvl1) =>
                {
                    wotWriter.WriteLineLevel($"Selected Indicator Percentage Of Radius Float {dragonCustomInfoFactory.SelectedIndicatorPercentageOfRadius:F6}", lvl1);
                    wotWriter.WriteLineLevel($"Distance to Start Sphere Picking Float {dragonCustomInfoFactory.DistancetoStartSpherePicking:F6}", lvl1);
                }, lvl);
                wotWriter.WriteLineLevel($"ShortestTimeBetweenSounds Float {dragonCustomInfoFactory.ShortestTimeBetweenSounds:F6}", lvl);
                wotWriter.WriteLineLevel($"LongestTimeBetweenSounds Float {dragonCustomInfoFactory.LongestTimeBetweenSounds:F6}", lvl);
                wotWriter.WriteLineLevel($"Sound_0 String '{dragonCustomInfoFactory.Sound_0}'", lvl);
                wotWriter.WriteLineLevel($"Sound_1 String '{dragonCustomInfoFactory.Sound_1}'", lvl);
                wotWriter.WriteLineLevel($"Sound_2 String '{dragonCustomInfoFactory.Sound_2}'", lvl);
                wotWriter.WriteLineLevel($"Sound_3 String '{dragonCustomInfoFactory.Sound_3}'", lvl);
                wotWriter.WriteLineLevel($"Sound_4 String '{dragonCustomInfoFactory.Sound_4}'", lvl);
                wotWriter.WriteLineLevel($"Sound_5 String '{dragonCustomInfoFactory.Sound_5}'", lvl);
                wotWriter.WriteLineLevel($"Sound_6 String '{dragonCustomInfoFactory.Sound_6}'", lvl);
                wotWriter.WriteLineLevel($"Sound_7 String '{dragonCustomInfoFactory.Sound_7}'", lvl);
                wotWriter.WriteLineLevel($"Sound_8 String '{dragonCustomInfoFactory.Sound_8}'", lvl);
                wotWriter.WriteLineLevel($"Sound_9 String '{dragonCustomInfoFactory.Sound_9}'", lvl);
                wotWriter.WriteLineLevel($"Report spotting Bool {dragonCustomInfoFactory.ReportSpotting}", lvl);
                if (dragonCustomInfoFactory.ReportSpotting)
                    wotWriter.WriteLineLevel($"Report type String '{dragonCustomInfoFactory.ReportType.GetName()}'", lvl);
                wotWriter.WriteLineLevel($"Sound Distance Float {dragonCustomInfoFactory.SoundDistance:F6}", lvl);
            });
        }

        private static void WriteSpaceAnimalCustomInfoFactory(StreamWriter wotWriter, SpaceAnimalCustomInfoFactory spaceAnimalCustomInfoFactory)
        {
            wotWriter.WriteLine($"FactoryType String 'SpaceAnimalCustomInfoFactory'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteSection("GUIInfo Chunk", (lvl1) =>
                {
                    wotWriter.WriteLineLevel($"Selected Indicator Percentage Of Radius Float {spaceAnimalCustomInfoFactory.SelectedIndicatorPercentageOfRadius:F6}", lvl1);
                    wotWriter.WriteLineLevel($"Distance to Start Sphere Picking Float {spaceAnimalCustomInfoFactory.DistancetoStartSpherePicking:F6}", lvl1);
                }, lvl);
                wotWriter.WriteLineLevel($"ShortestTimeBetweenSounds Float {spaceAnimalCustomInfoFactory.ShortestTimeBetweenSounds:F6}", lvl);
                wotWriter.WriteLineLevel($"LongestTimeBetweenSounds Float {spaceAnimalCustomInfoFactory.LongestTimeBetweenSounds:F6}", lvl);
                wotWriter.WriteLineLevel($"Sound_0 String '{spaceAnimalCustomInfoFactory.Sound_0}'", lvl);
                wotWriter.WriteLineLevel($"Sound_1 String '{spaceAnimalCustomInfoFactory.Sound_1}'", lvl);
                wotWriter.WriteLineLevel($"Sound_2 String '{spaceAnimalCustomInfoFactory.Sound_2}'", lvl);
                wotWriter.WriteLineLevel($"Sound_3 String '{spaceAnimalCustomInfoFactory.Sound_3}'", lvl);
                wotWriter.WriteLineLevel($"Sound_4 String '{spaceAnimalCustomInfoFactory.Sound_4}'", lvl);
                wotWriter.WriteLineLevel($"Sound_5 String '{spaceAnimalCustomInfoFactory.Sound_5}'", lvl);
                wotWriter.WriteLineLevel($"Sound_6 String '{spaceAnimalCustomInfoFactory.Sound_6}'", lvl);
                wotWriter.WriteLineLevel($"Sound_7 String '{spaceAnimalCustomInfoFactory.Sound_7}'", lvl);
                wotWriter.WriteLineLevel($"Sound_8 String '{spaceAnimalCustomInfoFactory.Sound_8}'", lvl);
                wotWriter.WriteLineLevel($"Sound_9 String '{spaceAnimalCustomInfoFactory.Sound_9}'", lvl);
                wotWriter.WriteLineLevel($"Report spotting Bool {spaceAnimalCustomInfoFactory.ReportSpotting}", lvl);
                if (spaceAnimalCustomInfoFactory.ReportSpotting)
                    wotWriter.WriteLineLevel($"Report type String '{spaceAnimalCustomInfoFactory.ReportType.GetName()}'", lvl);
                wotWriter.WriteLineLevel($"Sound Distance Float {spaceAnimalCustomInfoFactory.SoundDistance:F6}", lvl);
            });
        }

        private static void WriteAsteroidCustomInfoFactory(StreamWriter wotWriter, AsteroidCustomInfoFactory asteroidCustomInfoFactory)
        {
            wotWriter.WriteLine($"FactoryType String 'AsteroidCustomInfoFactory'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"Max Hit Points Int {asteroidCustomInfoFactory.MaxHitPoints}", lvl);
                wotWriter.WriteLineLevel($"WorldObject To Create Upon Death 0 String '{asteroidCustomInfoFactory.WorldObjectToCreateUponDeath0Name}'", lvl);
                wotWriter.WriteLineLevel($"WorldObject To Create Upon Death 1 String '{asteroidCustomInfoFactory.WorldObjectToCreateUponDeath1Name}'", lvl);
                wotWriter.WriteLineLevel($"WorldObject To Create Upon Death 2 String '{asteroidCustomInfoFactory.WorldObjectToCreateUponDeath2Name}'", lvl);
                wotWriter.WriteLineLevel($"WorldObject To Create Upon Death 3 String '{asteroidCustomInfoFactory.WorldObjectToCreateUponDeath3Name}'", lvl);
                wotWriter.WriteLineLevel($"WorldObject To Create Upon Death 4 String '{asteroidCustomInfoFactory.WorldObjectToCreateUponDeath4Name}'", lvl);
                wotWriter.WriteLineLevel($"WorldObject To Create Upon Death 5 String '{asteroidCustomInfoFactory.WorldObjectToCreateUponDeath5Name}'", lvl);
                wotWriter.WriteLineLevel($"WorldObject To Create Upon Death 6 String '{asteroidCustomInfoFactory.WorldObjectToCreateUponDeath6Name}'", lvl);
                wotWriter.WriteLineLevel($"WorldObject To Create Upon Death 7 String '{asteroidCustomInfoFactory.WorldObjectToCreateUponDeath7Name}'", lvl);
                wotWriter.WriteLineLevel($"WorldObject To Create Upon Death 8 String '{asteroidCustomInfoFactory.WorldObjectToCreateUponDeath8Name}'", lvl);
                wotWriter.WriteLineLevel($"WorldObject To Create Upon Death 9 String '{asteroidCustomInfoFactory.WorldObjectToCreateUponDeath9Name}'", lvl);
                wotWriter.WriteLineLevel($"ExplosionEffect String '{asteroidCustomInfoFactory.ExplosionEffect}'", lvl);
                wotWriter.WriteLineLevel($"Report Spotting Bool {asteroidCustomInfoFactory.ReportSpotting}", lvl);
            });
        }

        private static void WriteIslandCustomInfoFactory(StreamWriter wotWriter, IslandCustomInfoFactory islandCustomInfoFactory)
        {
            wotWriter.WriteLine($"FactoryType String 'IslandCustomInfoFactory'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"Occlusion Polygon Point # 00 Vector3( {islandCustomInfoFactory.OcclusionPolygonPoint00.X:F6}, {islandCustomInfoFactory.OcclusionPolygonPoint00.Y:F6}, {islandCustomInfoFactory.OcclusionPolygonPoint00.Z:F6} )", lvl);
                wotWriter.WriteLineLevel($"Occlusion Polygon Point # 01 Vector3( {islandCustomInfoFactory.OcclusionPolygonPoint01.X:F6}, {islandCustomInfoFactory.OcclusionPolygonPoint01.Y:F6}, {islandCustomInfoFactory.OcclusionPolygonPoint01.Z:F6} )", lvl);
                wotWriter.WriteLineLevel($"Occlusion Polygon Point # 02 Vector3( {islandCustomInfoFactory.OcclusionPolygonPoint02.X:F6}, {islandCustomInfoFactory.OcclusionPolygonPoint02.Y:F6}, {islandCustomInfoFactory.OcclusionPolygonPoint02.Z:F6} )", lvl);
                wotWriter.WriteLineLevel($"Occlusion Polygon Point # 03 Vector3( {islandCustomInfoFactory.OcclusionPolygonPoint03.X:F6}, {islandCustomInfoFactory.OcclusionPolygonPoint03.Y:F6}, {islandCustomInfoFactory.OcclusionPolygonPoint03.Z:F6} )", lvl);
                wotWriter.WriteLineLevel($"Occlusion Polygon Point # 04 Vector3( {islandCustomInfoFactory.OcclusionPolygonPoint04.X:F6}, {islandCustomInfoFactory.OcclusionPolygonPoint04.Y:F6}, {islandCustomInfoFactory.OcclusionPolygonPoint04.Z:F6} )", lvl);
                wotWriter.WriteLineLevel($"Occlusion Polygon Point # 05 Vector3( {islandCustomInfoFactory.OcclusionPolygonPoint05.X:F6}, {islandCustomInfoFactory.OcclusionPolygonPoint05.Y:F6}, {islandCustomInfoFactory.OcclusionPolygonPoint05.Z:F6} )", lvl);
                wotWriter.WriteLineLevel($"Occlusion Polygon Point # 06 Vector3( {islandCustomInfoFactory.OcclusionPolygonPoint06.X:F6}, {islandCustomInfoFactory.OcclusionPolygonPoint06.Y:F6}, {islandCustomInfoFactory.OcclusionPolygonPoint06.Z:F6} )", lvl);
                wotWriter.WriteLineLevel($"Occlusion Polygon Point # 07 Vector3( {islandCustomInfoFactory.OcclusionPolygonPoint07.X:F6}, {islandCustomInfoFactory.OcclusionPolygonPoint07.Y:F6}, {islandCustomInfoFactory.OcclusionPolygonPoint07.Z:F6} )", lvl);
                wotWriter.WriteLineLevel($"Occlusion Polygon Point # 08 Vector3( {islandCustomInfoFactory.OcclusionPolygonPoint08.X:F6}, {islandCustomInfoFactory.OcclusionPolygonPoint08.Y:F6}, {islandCustomInfoFactory.OcclusionPolygonPoint08.Z:F6} )", lvl);
                wotWriter.WriteLineLevel($"Occlusion Polygon Point # 09 Vector3( {islandCustomInfoFactory.OcclusionPolygonPoint09.X:F6}, {islandCustomInfoFactory.OcclusionPolygonPoint09.Y:F6}, {islandCustomInfoFactory.OcclusionPolygonPoint09.Z:F6} )", lvl);
                wotWriter.WriteLineLevel($"Occlusion Polygon Point Count Int {islandCustomInfoFactory.OcclusionPolygonPointCount}", lvl);
                wotWriter.WriteLineLevel($"Race String '{islandCustomInfoFactory.Race.GetName()}'", lvl);
                wotWriter.WriteLineLevel($"HasAmbientSound Bool {islandCustomInfoFactory.HasAmbientSound}", lvl);
                if (islandCustomInfoFactory.HasAmbientSound)
                {
                    wotWriter.WriteLineLevel($"AmbientSoundMaxDistance Float {islandCustomInfoFactory.AmbientSoundMaxDistance:F6}", lvl);
                    wotWriter.WriteLineLevel($"AmbientSoundName String '{islandCustomInfoFactory.AmbientSoundName}'", lvl);
                }
                wotWriter.WriteLineLevel($"Core Damage Section Max HitPoints Int {islandCustomInfoFactory.CoreDamageSectionMaxHitPoints}", lvl);
                wotWriter.WriteLineLevel($"AnnounceSighting Bool {islandCustomInfoFactory.AnnounceSighting}", lvl);
            });
        }

        private static void WriteBulletCustomInfoFactory(StreamWriter wotWriter, BulletCustomInfoFactory bulletCustomInfoFactory)
        {
            wotWriter.WriteLine($"FactoryType String 'BulletCustomInfoFactory'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                WriteBulletCustomInfoFactoryProperties(wotWriter, bulletCustomInfoFactory, lvl);
            });
        }
        
        private static void WriteStarMortarCustomInfoFactory(StreamWriter wotWriter, StarMortarCustomInfoFactory starMortarCustomInfoFactory)
        {
            wotWriter.WriteLine($"FactoryType String 'StarMortarCustomInfoFactory'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"Magnitude Float {starMortarCustomInfoFactory.Magnitude:F6}", lvl);
                wotWriter.WriteLineLevel($"MaxRadius Float {starMortarCustomInfoFactory.MaxRadius:F6}", lvl);
                wotWriter.WriteLineLevel($"Duration Float {starMortarCustomInfoFactory.Duration:F6}", lvl);
                WriteBulletCustomInfoFactoryProperties(wotWriter, starMortarCustomInfoFactory, lvl);
            });
        }

        private static void WriteEnergyNetCustomInfoFactory(StreamWriter wotWriter, EnergyNetCustomInfoFactory energyNetCustomInfoFactory)
        {
            wotWriter.WriteLine($"FactoryType String 'EnergyNetCustomInfoFactory'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"Duration Float {energyNetCustomInfoFactory.Duration:F6}", lvl);
                wotWriter.WriteLineLevel($"PercentageOfEnergyDissipated Float {energyNetCustomInfoFactory.PercentageOfEnergyDissipated:F6}", lvl);
                WriteBulletCustomInfoFactoryProperties(wotWriter, energyNetCustomInfoFactory, lvl);
            });
        }

        private static void WriteGrapplingHarpoonCustomInfoFactory(StreamWriter wotWriter, GrapplingHarpoonCustomInfoFactory grapplingHarpoonCustomInfoFactory)
        {
            wotWriter.WriteLine($"FactoryType String 'GrapplingHarpoonCustomInfoFactory'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                WriteBulletCustomInfoFactoryProperties(wotWriter, grapplingHarpoonCustomInfoFactory, lvl);
            });
        }

        private static void WriteGravChargeCustomInfoFactory(StreamWriter wotWriter, GravChargeCustomInfoFactory gravChargeCustomInfoFactory)
        {
            wotWriter.WriteLine($"FactoryType String 'GravChargeCustomInfoFactory'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"Magnitude Float {gravChargeCustomInfoFactory.Magnitude:F6}", lvl);
                wotWriter.WriteLineLevel($"Radius Float {gravChargeCustomInfoFactory.Radius:F6}", lvl);
                wotWriter.WriteLineLevel($"Duration Float {gravChargeCustomInfoFactory.Duration:F6}", lvl);
                WriteBulletCustomInfoFactoryProperties(wotWriter, gravChargeCustomInfoFactory, lvl);
            });
        }

        private static void WriteNovaMortarCustomInfoFactory(StreamWriter wotWriter, NovaMortarCustomInfoFactory novaMortarCustomInfoFactory)
        {
            wotWriter.WriteLine($"FactoryType String 'NovaMortarCustomInfoFactory'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                WriteBulletCustomInfoFactoryProperties(wotWriter, novaMortarCustomInfoFactory, lvl);
            });
        }

        private static void WriteMineCustomInfoFactory(StreamWriter wotWriter, MineCustomInfoFactory mineCustomInfoFactory)
        {
            wotWriter.WriteLine($"FactoryType String 'MineCustomInfoFactory'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                WriteBulletCustomInfoFactoryProperties(wotWriter, mineCustomInfoFactory, lvl);
            });
        }

        private static void WriteTorpedoCustomInfoFactory(StreamWriter wotWriter, TorpedoCustomInfoFactory torpedoCustomInfoFactory)
        {
            wotWriter.WriteLine($"FactoryType String 'TorpedoCustomInfoFactory'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                WriteBulletCustomInfoFactoryProperties(wotWriter, torpedoCustomInfoFactory, lvl);
            });
        }

        private static void WriteBulletCustomInfoFactoryProperties(StreamWriter wotWriter, BulletCustomInfoFactory bulletCustomInfoFactory, int level)
        {
            wotWriter.WriteLineLevel($"HitEffect String '{bulletCustomInfoFactory.HitEffect}'", level);
            wotWriter.WriteLineLevel($"BulletEffect String '{bulletCustomInfoFactory.BulletEffect}'", level);
            wotWriter.WriteLineLevel($"Damage HitPoints Int {bulletCustomInfoFactory.DamageHitPoints}", level);
            wotWriter.WriteLineLevel($"Decal Damage Size String '{bulletCustomInfoFactory.DecalDamageSize.GetName()}'", level);
            WriteDamagePotential(wotWriter, bulletCustomInfoFactory.DamagePotential, level);
            wotWriter.WriteLineLevel($"Impact Sound Max Distance Float {bulletCustomInfoFactory.ImpactSoundMaxDistance:F6}", level);
            wotWriter.WriteLineLevel($"Impact Sound Volume Float {bulletCustomInfoFactory.ImpactSoundVolume:F6}", level);
            wotWriter.WriteLineLevel($"Travel Sound Max Distance Float {bulletCustomInfoFactory.TravelSoundMaxDistance:F6}", level);
            wotWriter.WriteLineLevel($"Travel Sound Volume Float {bulletCustomInfoFactory.TravelSoundVolume:F6}", level);
            wotWriter.WriteLineLevel($"RicochetSound: Hull Wood String '{bulletCustomInfoFactory.RicochetSoundHullWood}'", level);
            wotWriter.WriteSection("ImpactSoundsHull Wood", (lvl) =>
            {
                wotWriter.WriteLineLevel($"Sound_0 String '{bulletCustomInfoFactory.ImpactSound0HullWood}'", lvl);
                wotWriter.WriteLineLevel($"Sound_1 String '{bulletCustomInfoFactory.ImpactSound1HullWood}'", lvl);
                wotWriter.WriteLineLevel($"Sound_2 String '{bulletCustomInfoFactory.ImpactSound2HullWood}'", lvl);
                wotWriter.WriteLineLevel($"Sound_3 String '{bulletCustomInfoFactory.ImpactSound3HullWood}'", lvl);
                wotWriter.WriteLineLevel($"Sound_4 String '{bulletCustomInfoFactory.ImpactSound4HullWood}'", lvl);
            }, level);
            wotWriter.WriteLineLevel($"RicochetSound: Hull ReInforced Wood String '{bulletCustomInfoFactory.RicochetSoundHullReInforcedWood}'", level);
            wotWriter.WriteSection("ImpactSoundsHull ReInforced Wood", (lvl) =>
            {
                wotWriter.WriteLineLevel($"Sound_0 String '{bulletCustomInfoFactory.ImpactSound0HullReInforcedWood}'", lvl);
                wotWriter.WriteLineLevel($"Sound_1 String '{bulletCustomInfoFactory.ImpactSound1HullReInforcedWood}'", lvl);
                wotWriter.WriteLineLevel($"Sound_2 String '{bulletCustomInfoFactory.ImpactSound2HullReInforcedWood}'", lvl);
                wotWriter.WriteLineLevel($"Sound_3 String '{bulletCustomInfoFactory.ImpactSound3HullReInforcedWood}'", lvl);
                wotWriter.WriteLineLevel($"Sound_4 String '{bulletCustomInfoFactory.ImpactSound4HullReInforcedWood}'", lvl);
            }, level);
            wotWriter.WriteLineLevel($"RicochetSound: Hull Iron String '{bulletCustomInfoFactory.RicochetSoundHullIron}'", level);
            wotWriter.WriteSection("ImpactSoundsHull Iron", (lvl) =>
            {
                wotWriter.WriteLineLevel($"Sound_0 String '{bulletCustomInfoFactory.ImpactSound0HullIron}'", lvl);
                wotWriter.WriteLineLevel($"Sound_1 String '{bulletCustomInfoFactory.ImpactSound1HullIron}'", lvl);
                wotWriter.WriteLineLevel($"Sound_2 String '{bulletCustomInfoFactory.ImpactSound2HullIron}'", lvl);
                wotWriter.WriteLineLevel($"Sound_3 String '{bulletCustomInfoFactory.ImpactSound3HullIron}'", lvl);
                wotWriter.WriteLineLevel($"Sound_4 String '{bulletCustomInfoFactory.ImpactSound4HullIron}'", lvl);
            }, level);
            wotWriter.WriteLineLevel($"RicochetSound: Sail Cloth String '{bulletCustomInfoFactory.RicochetSoundSailCloth}'", level);
            wotWriter.WriteSection("ImpactSoundsSail Cloth", (lvl) =>
            {
                wotWriter.WriteLineLevel($"Sound_0 String '{bulletCustomInfoFactory.ImpactSound0SailCloth}'", lvl);
                wotWriter.WriteLineLevel($"Sound_1 String '{bulletCustomInfoFactory.ImpactSound1SailCloth}'", lvl);
                wotWriter.WriteLineLevel($"Sound_2 String '{bulletCustomInfoFactory.ImpactSound2SailCloth}'", lvl);
                wotWriter.WriteLineLevel($"Sound_3 String '{bulletCustomInfoFactory.ImpactSound3SailCloth}'", lvl);
                wotWriter.WriteLineLevel($"Sound_4 String '{bulletCustomInfoFactory.ImpactSound4SailCloth}'", lvl);
            }, level);
            wotWriter.WriteLineLevel($"RicochetSound: Wall Stone String '{bulletCustomInfoFactory.RicochetSoundWallStone}'", level);
            wotWriter.WriteSection("ImpactSoundsWall Stone", (lvl) =>
            {
                wotWriter.WriteLineLevel($"Sound_0 String '{bulletCustomInfoFactory.ImpactSound0WallStone}'", lvl);
                wotWriter.WriteLineLevel($"Sound_1 String '{bulletCustomInfoFactory.ImpactSound1WallStone}'", lvl);
                wotWriter.WriteLineLevel($"Sound_2 String '{bulletCustomInfoFactory.ImpactSound2WallStone}'", lvl);
                wotWriter.WriteLineLevel($"Sound_3 String '{bulletCustomInfoFactory.ImpactSound3WallStone}'", lvl);
                wotWriter.WriteLineLevel($"Sound_4 String '{bulletCustomInfoFactory.ImpactSound4WallStone}'", lvl);
            }, level);
            wotWriter.WriteLineLevel($"RicochetSound: Dragon Scale String '{bulletCustomInfoFactory.RicochetSoundDragonScale}'", level);
            wotWriter.WriteSection("ImpactSoundsDragon Scale", (lvl) =>
            {
                wotWriter.WriteLineLevel($"Sound_0 String '{bulletCustomInfoFactory.ImpactSound0DragonScale}'", lvl);
                wotWriter.WriteLineLevel($"Sound_1 String '{bulletCustomInfoFactory.ImpactSound1DragonScale}'", lvl);
                wotWriter.WriteLineLevel($"Sound_2 String '{bulletCustomInfoFactory.ImpactSound2DragonScale}'", lvl);
                wotWriter.WriteLineLevel($"Sound_3 String '{bulletCustomInfoFactory.ImpactSound3DragonScale}'", lvl);
                wotWriter.WriteLineLevel($"Sound_4 String '{bulletCustomInfoFactory.ImpactSound4DragonScale}'", lvl);
            }, level);
            wotWriter.WriteLineLevel($"Travel Sound String '{bulletCustomInfoFactory.TravelSound}'", level);
            if (bulletCustomInfoFactory.Lifetime >= 0)
                wotWriter.WriteLineLevel($"Lifetime Float {bulletCustomInfoFactory.Lifetime:F6}", level);
        }

        private static void WriteDamagePotential(StreamWriter wotWriter, DamagePotential damagePotential, int level)
        {
            wotWriter.WriteLineLevel("#", level);
            wotWriter.WriteLineLevel("# Hull Wood DamagePotential", level);
            wotWriter.WriteLineLevel("#", level);
            wotWriter.WriteLineLevel($"Chance of Critical Damage: Wood Float {damagePotential.ChanceOfCriticalDamageWood:F6}", level);
            wotWriter.WriteLineLevel($"Chance of Fire: Wood Float {damagePotential.ChanceOfFireWood:F6}", level);
            wotWriter.WriteLineLevel($"Initial Fire Strength: Wood Float {damagePotential.InitialFireStrengthWood:F6}", level);
            wotWriter.WriteLineLevel($"Damage Lower Bound: Wood Float {damagePotential.DamageLowerBoundWood:F6}", level);
            wotWriter.WriteLineLevel($"Damage Upper Bound: Wood Float {damagePotential.DamageUpperBoundWood:F6}", level);
            wotWriter.WriteLineLevel("#", level);
            wotWriter.WriteLineLevel("# Hull ReInforced Wood DamagePotential", level);
            wotWriter.WriteLineLevel("#", level);
            wotWriter.WriteLineLevel($"Chance of Critical Damage: Reinforced Float {damagePotential.ChanceOfCriticalDamageReinforced:F6}", level);
            wotWriter.WriteLineLevel($"Chance of Fire: Reinforced Float {damagePotential.ChanceOfFireReinforced:F6}", level);
            wotWriter.WriteLineLevel($"Initial Fire Strength: Reinforced Float {damagePotential.InitialFireStrengthReinforced:F6}", level);
            wotWriter.WriteLineLevel($"Damage Lower Bound: Reinforced Float {damagePotential.DamageLowerBoundReinforced:F6}", level);
            wotWriter.WriteLineLevel($"Damage Upper Bound: Reinforced Float {damagePotential.DamageUpperBoundReinforced:F6}", level);
            wotWriter.WriteLineLevel("#", level);
            wotWriter.WriteLineLevel("# Hull Iron DamagePotential", level);
            wotWriter.WriteLineLevel("#", level);
            wotWriter.WriteLineLevel($"Chance of Critical Damage: Iron Float {damagePotential.ChanceOfCriticalDamageIron:F6}", level);
            wotWriter.WriteLineLevel($"Chance of Fire: Iron Float {damagePotential.ChanceOfFireIron:F6}", level);
            wotWriter.WriteLineLevel($"Initial Fire Strength: Iron Float {damagePotential.InitialFireStrengthIron:F6}", level);
            wotWriter.WriteLineLevel($"Damage Lower Bound: Iron Float {damagePotential.DamageLowerBoundIron:F6}", level);
            wotWriter.WriteLineLevel($"Damage Upper Bound: Iron Float {damagePotential.DamageUpperBoundIron:F6}", level);
            wotWriter.WriteLineLevel("#", level);
            wotWriter.WriteLineLevel("# Sail Cloth DamagePotential", level);
            wotWriter.WriteLineLevel("#", level);
            wotWriter.WriteLineLevel($"Chance of Critical Damage: Cloth Float {damagePotential.ChanceOfCriticalDamageCloth:F6}", level);
            wotWriter.WriteLineLevel($"Chance of Fire: Cloth Float {damagePotential.ChanceOfFireCloth:F6}", level);
            wotWriter.WriteLineLevel($"Initial Fire Strength: Cloth Float {damagePotential.InitialFireStrengthCloth:F6}", level);
            wotWriter.WriteLineLevel($"Damage Lower Bound: Cloth Float {damagePotential.DamageLowerBoundCloth:F6}", level);
            wotWriter.WriteLineLevel($"Damage Upper Bound: Cloth Float {damagePotential.DamageUpperBoundCloth:F6}", level);
            wotWriter.WriteLineLevel("#", level);
            wotWriter.WriteLineLevel("# Wall Stone DamagePotential", level);
            wotWriter.WriteLineLevel("#", level);
            wotWriter.WriteLineLevel($"Chance of Critical Damage: Stone Float {damagePotential.ChanceOfCriticalDamageStone:F6}", level);
            wotWriter.WriteLineLevel($"Chance of Fire: Stone Float {damagePotential.ChanceOfFireStone:F6}", level);
            wotWriter.WriteLineLevel($"Initial Fire Strength: Stone Float {damagePotential.InitialFireStrengthStone:F6}", level);
            wotWriter.WriteLineLevel($"Damage Lower Bound: Stone Float {damagePotential.DamageLowerBoundStone:F6}", level);
            wotWriter.WriteLineLevel($"Damage Upper Bound: Stone Float {damagePotential.DamageUpperBoundStone:F6}", level);
            wotWriter.WriteLineLevel("#", level);
            wotWriter.WriteLineLevel("# Dragon Scale DamagePotential", level);
            wotWriter.WriteLineLevel("#", level);
            wotWriter.WriteLineLevel($"Chance of Critical Damage: Dragon Float {damagePotential.ChanceOfCriticalDamageDragon:F6}", level);
            wotWriter.WriteLineLevel($"Chance of Fire: Dragon Float {damagePotential.ChanceOfFireDragon:F6}", level);
            wotWriter.WriteLineLevel($"Initial Fire Strength: Dragon Float {damagePotential.InitialFireStrengthDragon:F6}", level);
            wotWriter.WriteLineLevel($"Damage Lower Bound: Dragon Float {damagePotential.DamageLowerBoundDragon:F6}", level);
            wotWriter.WriteLineLevel($"Damage Upper Bound: Dragon Float {damagePotential.DamageUpperBoundDragon:F6}", level);
        }
        
        private static void WriteCrewCustomInfoFactory(StreamWriter wotWriter, CrewCustomInfoFactory crewCustomInfoFactory)
        {
            wotWriter.WriteLine($"FactoryType String 'CrewCustomInfoFactory'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"Crew Race String '{crewCustomInfoFactory.CrewRace.GetName()}'", lvl);
                wotWriter.WriteLineLevel($"Point Value Int {crewCustomInfoFactory.PointValue}", lvl);
                wotWriter.WriteLineLevel($"Crew Name String ID String '{crewCustomInfoFactory.CrewNameStringID}'", lvl);
                wotWriter.WriteLineLevel($"Leadership Skill Active Bool {crewCustomInfoFactory.LeadershipSkillActive}", lvl);
                wotWriter.WriteLineLevel($"Leadership Skill Ability Value Int {crewCustomInfoFactory.LeadershipSkillAbilityValue}", lvl);
                wotWriter.WriteLineLevel($"Navigation Skill Active Bool {crewCustomInfoFactory.NavigationSkillActive}", lvl);
                wotWriter.WriteLineLevel($"Navigation Skill Ability Value Int {crewCustomInfoFactory.NavigationSkillAbilityValue}", lvl);
                wotWriter.WriteLineLevel($"Spotting Skill Active Bool {crewCustomInfoFactory.SpottingSkillActive}", lvl);
                wotWriter.WriteLineLevel($"Spotting Skill Ability Value Int {crewCustomInfoFactory.SpottingSkillAbilityValue}", lvl);
                wotWriter.WriteLineLevel($"Engineering Skill Active Bool {crewCustomInfoFactory.EngineeringSkillActive}", lvl);
                wotWriter.WriteLineLevel($"Engineering Skill Ability Value Int {crewCustomInfoFactory.EngineeringSkillAbilityValue}", lvl);
                wotWriter.WriteLineLevel($"Rigging Skill Active Bool {crewCustomInfoFactory.RiggingSkillActive}", lvl);
                wotWriter.WriteLineLevel($"Rigging Skill Ability Value Int {crewCustomInfoFactory.RiggingSkillAbilityValue}", lvl);
                wotWriter.WriteLineLevel($"Combat Skill Active Bool {crewCustomInfoFactory.CombatSkillActive}", lvl);
                wotWriter.WriteLineLevel($"Combat Skill Ability Value Int {crewCustomInfoFactory.CombatSkillAbilityValue}", lvl);
                wotWriter.WriteLineLevel($"Gunnery Skill Active Bool {crewCustomInfoFactory.GunnerySkillActive}", lvl);
                wotWriter.WriteLineLevel($"Gunnery Skill Ability Value Int {crewCustomInfoFactory.GunnerySkillAbilityValue}", lvl);
                wotWriter.WriteLineLevel($"Talking Head Texture String '{crewCustomInfoFactory.TalkingHeadTexture}'", lvl);
                wotWriter.WriteLineLevel($"Species String '{crewCustomInfoFactory.Species.GetName()}'", lvl);
            });
        }
        
        private static void WriteGunCustomInfoFactory(StreamWriter wotWriter, GunCustomInfoFactory gunCustomInfoFactory)
        {
            wotWriter.WriteLine($"FactoryType String 'GunCustomInfoFactory'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"BulletTypeName_Primary String '{gunCustomInfoFactory.BulletTypeName_PrimaryString}'", lvl);
                wotWriter.WriteLineLevel($"BulletTypeName_Secondary String '{gunCustomInfoFactory.BulletTypeName_SecondaryString}'", lvl);
                wotWriter.WriteLineLevel($"SoundName String '{gunCustomInfoFactory.SoundName}'", lvl);
                wotWriter.WriteLineLevel($"MuzzleFlashEffect String '{gunCustomInfoFactory.MuzzleFlashEffect}'", lvl);
                wotWriter.WriteLineLevel($"MuzzleSpeed Float {gunCustomInfoFactory.MuzzleSpeed:F6}", lvl);
                wotWriter.WriteLineLevel($"DuringBurstReloadTime Float {gunCustomInfoFactory.DuringBurstReloadTime:F6}", lvl);
                wotWriter.WriteLineLevel($"BulletsPerBurst Int {gunCustomInfoFactory.BulletsPerBurst}", lvl);
                wotWriter.WriteLineLevel($"TimeBetweenBursts Float {gunCustomInfoFactory.TimeBetweenBursts:F6}", lvl);
                wotWriter.WriteLineLevel($"CalculateAccuracyDeviationDuringBurst Bool {gunCustomInfoFactory.CalculateAccuracyDeviationDuringBurst}", lvl);
                wotWriter.WriteLineLevel($"WeaponBar Icon Texture String '{gunCustomInfoFactory.WeaponBarIconTexture}'", lvl);
                wotWriter.WriteLineLevel($"VerticalRotationSpeed Float {gunCustomInfoFactory.VerticalRotationSpeed:F6}", lvl);
                wotWriter.WriteLineLevel($"HorizontalRotationSpeed Float {gunCustomInfoFactory.HorizontalRotationSpeed:F6}", lvl);
                wotWriter.WriteLineLevel($"VerticalMinAngle Float {gunCustomInfoFactory.VerticalMinAngle:F6}", lvl);
                wotWriter.WriteLineLevel($"VerticalMaxAngle Float {gunCustomInfoFactory.VerticalMaxAngle:F6}", lvl);
                wotWriter.WriteLineLevel($"LobAngle Float {gunCustomInfoFactory.LobAngle:F6}", lvl);
                wotWriter.WriteLineLevel($"MaximumRange_Range Float {gunCustomInfoFactory.MaximumRange_Range:F6}", lvl);
                wotWriter.WriteLineLevel($"LongRange_Range Float {gunCustomInfoFactory.LongRange_Range:F6}", lvl);
                wotWriter.WriteLineLevel($"EffectiveRange_Range Float {gunCustomInfoFactory.EffectiveRange_Range:F6}", lvl);
                wotWriter.WriteLineLevel($"BulletAffectedByGravity Bool {gunCustomInfoFactory.BulletAffectedByGravity}", lvl);
                wotWriter.WriteLineLevel($"MaximumRange_AccuracyDeviation Float {gunCustomInfoFactory.MaximumRange_AccuracyDeviation:F6}", lvl);
                wotWriter.WriteLineLevel($"LongRange_AccuracyDeviation Float {gunCustomInfoFactory.LongRange_AccuracyDeviation:F6}", lvl);
                wotWriter.WriteLineLevel($"EffectiveRange_AccuracyDeviation Float {gunCustomInfoFactory.EffectiveRange_AccuracyDeviation:F6}", lvl);
                wotWriter.WriteLineLevel($"MaximumRange_RicochetFactor Float {gunCustomInfoFactory.MaximumRange_RicochetFactor:F6}", lvl);
                wotWriter.WriteLineLevel($"LongRange_RicochetFactor Float {gunCustomInfoFactory.LongRange_RicochetFactor:F6}", lvl);
                wotWriter.WriteLineLevel($"EffectiveRange_RicochetFactor Float {gunCustomInfoFactory.EffectiveRange_RicochetFactor:F6}", lvl);
                wotWriter.WriteLineLevel($"Victory Point Cost Int {gunCustomInfoFactory.VictoryPointCost}", lvl);
                wotWriter.WriteLineLevel($"Gun Name String ID String '{gunCustomInfoFactory.GunNameStringID}'", lvl);
                wotWriter.WriteLineLevel($"Mount Type String '{gunCustomInfoFactory.MountType.GetName()}'", lvl);
                wotWriter.WriteLineLevel($"Exclusion Race String '{gunCustomInfoFactory.ExclusionRace.GetName()}'", lvl);
                wotWriter.WriteLineLevel($"Can Be Fired While Cloaked Bool {gunCustomInfoFactory.CanBeFiredWhileCloaked}", lvl);
                wotWriter.WriteLineLevel($"Firing Crew Alert String '{gunCustomInfoFactory.FiringCrewAlert.GetName()}'", lvl);
                wotWriter.WriteLineLevel($"SoundMaxDistance Float {gunCustomInfoFactory.SoundMaxDistance:F6}", lvl);
                wotWriter.WriteLineLevel($"SoundVolume Float {gunCustomInfoFactory.SoundVolume:F6}", lvl);
            });
        }

        private static void WriteShipCustomInfoFactory(StreamWriter wotWriter, ShipCustomInfoFactory shipCustomInfoFactory)
        {
            wotWriter.WriteLine($"FactoryType String 'ShipCustomInfoFactory'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteSection("GUIInfo Chunk", (lvl1) =>
                {
                    wotWriter.WriteLineLevel($"ShipBar Icon Texture String '{shipCustomInfoFactory.ShipBarIconTexture}'", lvl1);
                    wotWriter.WriteLineLevel($"Selected Indicator Percentage Of Radius Float {shipCustomInfoFactory.SelectedIndicatorPercentageOfRadius:F6}", lvl1);
                    wotWriter.WriteLineLevel($"Distance to Start Sphere Picking Float {shipCustomInfoFactory.DistanceToStartSpherePicking:F6}", lvl1);
                }, lvl);
                wotWriter.WriteLineLevel($"Displayable Shipname String ID String '{shipCustomInfoFactory.DisplayableShipnameStringID}'", lvl);
                wotWriter.WriteLineLevel($"Ship Race String '{shipCustomInfoFactory.ShipRace.GetName()}'", lvl);
                wotWriter.WriteLineLevel($"Is Tender Bool {shipCustomInfoFactory.IsTender}", lvl);
                wotWriter.WriteLineLevel($"Number of lifeboats Int {shipCustomInfoFactory.NumberOfLifeboats}", lvl);
                wotWriter.WriteLineLevel($"Is Lifeboat Bool {shipCustomInfoFactory.IsLifeboat}", lvl);
                wotWriter.WriteLineLevel($"Is Cloakable Bool {shipCustomInfoFactory.IsCloakable}", lvl);
                wotWriter.WriteLineLevel($"Ship Size Int {shipCustomInfoFactory.ShipSize}", lvl);
                wotWriter.WriteLineLevel($"Core Damage Section Max HitPoints Int {shipCustomInfoFactory.CoreDamageSectionMaxHitPoints}", lvl);
                wotWriter.WriteLineLevel($"Explosion Effect Name String '{shipCustomInfoFactory.ExplosionEffectName}'", lvl);
                wotWriter.WriteLineLevel($"EngineType String '{shipCustomInfoFactory.EngineType.GetName()}'", lvl);
                wotWriter.WriteLineLevel($"EngineSoundName Emergency String '{shipCustomInfoFactory.EngineSoundNameEmergency}'", lvl);
                wotWriter.WriteLineLevel($"EngineSoundName Full String '{shipCustomInfoFactory.EngineSoundNameFull}'", lvl);
                wotWriter.WriteLineLevel($"EngineSoundName Half String '{shipCustomInfoFactory.EngineSoundNameHalf}'", lvl);
                wotWriter.WriteLineLevel($"Victory Point Cost Int {shipCustomInfoFactory.VictoryPointCost}", lvl);
                wotWriter.WriteLineLevel($"Available in Multiplayer? Bool {shipCustomInfoFactory.IsAvailableInMultiplayer}", lvl);
                wotWriter.WriteLineLevel($"Available Unique Ship Name ID 00 String '{shipCustomInfoFactory.AvailableUniqueShipNameID00}'", lvl);
                wotWriter.WriteLineLevel($"Available Unique Ship Name ID 01 String '{shipCustomInfoFactory.AvailableUniqueShipNameID01}'", lvl);
                wotWriter.WriteLineLevel($"Available Unique Ship Name ID 02 String '{shipCustomInfoFactory.AvailableUniqueShipNameID02}'", lvl);
                wotWriter.WriteLineLevel($"Available Unique Ship Name ID 03 String '{shipCustomInfoFactory.AvailableUniqueShipNameID03}'", lvl);
                wotWriter.WriteLineLevel($"Available Unique Ship Name ID 04 String '{shipCustomInfoFactory.AvailableUniqueShipNameID04}'", lvl);
                wotWriter.WriteLineLevel($"Available Unique Ship Name ID 05 String '{shipCustomInfoFactory.AvailableUniqueShipNameID05}'", lvl);
                wotWriter.WriteLineLevel($"Available Unique Ship Name ID 06 String '{shipCustomInfoFactory.AvailableUniqueShipNameID06}'", lvl);
                wotWriter.WriteLineLevel($"Available Unique Ship Name ID 07 String '{shipCustomInfoFactory.AvailableUniqueShipNameID07}'", lvl);
                wotWriter.WriteLineLevel($"Available Unique Ship Name ID 08 String '{shipCustomInfoFactory.AvailableUniqueShipNameID08}'", lvl);
                wotWriter.WriteLineLevel($"Available Unique Ship Name ID 09 String '{shipCustomInfoFactory.AvailableUniqueShipNameID09}'", lvl);
                wotWriter.WriteLineLevel($"Available Unique Ship Name ID 10 String '{shipCustomInfoFactory.AvailableUniqueShipNameID10}'", lvl);
                wotWriter.WriteLineLevel($"Available Unique Ship Name ID 11 String '{shipCustomInfoFactory.AvailableUniqueShipNameID11}'", lvl);
                wotWriter.WriteLineLevel($"Max number of Gunners Int {shipCustomInfoFactory.MaxNumberOfGunners}", lvl);
                wotWriter.WriteLineLevel($"Max number of Captains Int {shipCustomInfoFactory.MaxNumberOfCaptains}", lvl);
                wotWriter.WriteLineLevel($"Max number of First Mates Int {shipCustomInfoFactory.MaxNumberOfFirstMates}", lvl);
                wotWriter.WriteLineLevel($"Max number of Navigators Int {shipCustomInfoFactory.MaxNumberOfNavigators}", lvl);
                wotWriter.WriteLineLevel($"Max number of Engineers Int {shipCustomInfoFactory.MaxNumberOfEngineers}", lvl);
                wotWriter.WriteLineLevel($"Max number of Riggers Int {shipCustomInfoFactory.MaxNumberOfRiggers}", lvl);
                wotWriter.WriteLineLevel($"Max number of Fighters Int {shipCustomInfoFactory.MaxNumberOfFighters}", lvl);
                wotWriter.WriteLineLevel($"Max number of Lookouts Int {shipCustomInfoFactory.MaxNumberOfLookouts}", lvl);
                wotWriter.WriteLineLevel($"Repair Effect Name String '{shipCustomInfoFactory.RepairEffectName}'", lvl);
            });
        }

        private static void WriteShipDebrisCustomInfoFactory(StreamWriter wotWriter, ShipDebrisCustomInfoFactory shipDebrisCustomInfoFactory)
        {
            wotWriter.WriteLine($"FactoryType String 'ShipDebrisCustomInfoFactory'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"LifeTimeMin Float {shipDebrisCustomInfoFactory.LifeTimeMin:F6}", lvl);
                wotWriter.WriteLineLevel($"LifeTimeMax Float {shipDebrisCustomInfoFactory.LifeTimeMax:F6}", lvl);
            });
        }

        private static void WriteBlackHoleCustomInfoFactory(StreamWriter wotWriter, BlackHoleCustomInfoFactory blackHoleCustomInfoFactory)
        {
            wotWriter.WriteLine($"FactoryType String 'BlackHoleCustomInfoFactory'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"Magnitude Float {blackHoleCustomInfoFactory.Magnitude:F6}", lvl);
                wotWriter.WriteLineLevel($"Radius Float {blackHoleCustomInfoFactory.Radius:F6}", lvl);
                wotWriter.WriteLineLevel($"VortexEffectName String '{blackHoleCustomInfoFactory.VortexEffectName}'", lvl);
                wotWriter.WriteLineLevel($"AmbientSoundMaxDistance Float {blackHoleCustomInfoFactory.AmbientSoundMaxDistance:F6}", lvl);
                wotWriter.WriteLineLevel($"AmbientSoundName String '{blackHoleCustomInfoFactory.AmbientSoundName}'", lvl);
            });
        }

        private static void WriteEtheriumCurrentCustomInfoFactory(StreamWriter wotWriter, EtheriumCurrentCustomInfoFactory etheriumCurrentCustomInfoFactory)
        {
            wotWriter.WriteLine($"FactoryType String 'EtheriumCurrentCustomInfoFactory'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"Magnitude Float {etheriumCurrentCustomInfoFactory.Magnitude:F6}", lvl);
                wotWriter.WriteLineLevel($"Radius Float {etheriumCurrentCustomInfoFactory.Radius:F6}", lvl);
            });
        }

        private static void WriteNebulaCustomInfoFactory(StreamWriter wotWriter, NebulaCustomInfoFactory nebulaCustomInfoFactory)
        {
            wotWriter.WriteLine($"FactoryType String 'NebulaCustomInfoFactory'");
            wotWriter.WriteSection("ConstData", (lvl) =>
            {
                wotWriter.WriteLineLevel($"Minimum Lightning Spawn Distance From Ships Float {nebulaCustomInfoFactory.MinimumLightningSpawnDistanceFromShips:F6}", lvl);
                wotWriter.WriteLineLevel($"Minimum Lightning Bolt Length Float {nebulaCustomInfoFactory.MinimumLightningBoltLength:F6}", lvl);
                wotWriter.WriteLineLevel($"Maximum Lightning Spawn Point Z Amplitude Float {nebulaCustomInfoFactory.MaximumLightningSpawnPointZAmplitude:F6}", lvl);
                wotWriter.WriteLineLevel($"Meteor Hit Chance Modifier Float {nebulaCustomInfoFactory.MeteorHitChanceModifier:F6}", lvl);
                wotWriter.WriteSection("Lightning Damage Potential", (lvl1) =>
                {
                    WriteDamagePotential(wotWriter, nebulaCustomInfoFactory.LightningDamagePotential, lvl1);
                }, lvl);
                wotWriter.WriteSection("Wind Damage Potential", (lvl1) =>
                {
                    WriteDamagePotential(wotWriter, nebulaCustomInfoFactory.WindDamagePotential, lvl1);
                }, lvl);
                wotWriter.WriteSection("Meteor Damage Potential", (lvl1) =>
                {
                    WriteDamagePotential(wotWriter, nebulaCustomInfoFactory.MeteorDamagePotential, lvl1);
                }, lvl);
                wotWriter.WriteLineLevel($"NebulaSoundName String '{nebulaCustomInfoFactory.NebulaSoundName}'", lvl);
                wotWriter.WriteLineLevel($"SolarStormSoundName String '{nebulaCustomInfoFactory.SolarStormSoundName}'", lvl);
                wotWriter.WriteLineLevel($"MeteorShowerSoundName String '{nebulaCustomInfoFactory.MeteorShowerSoundName}'", lvl);
            });
        }

        #endregion
    }
}
