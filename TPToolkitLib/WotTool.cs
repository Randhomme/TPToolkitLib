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
using TPToolkitLib.WorldObject.Definitions.CollisionFactories;
using TPToolkitLib.WorldObject.Definitions.CustomInfoFactories;
using TPToolkitLib.WorldObject.Definitions.CustomInfoFactories.Classes;
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
                HitPoints = wotReader.ReadAndParseInt("Max Hit Points Int ", ignoreFormatError),
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
