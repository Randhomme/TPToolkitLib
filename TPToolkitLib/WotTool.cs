using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPToolkitLib.Exceptions;
using TPToolkitLib.Utils;
using TPToolkitLib.WorldObject;
using TPToolkitLib.WorldObject.Definitions.AiEntityFactories;

namespace TPToolkitLib
{
    public static class WotTool
    {
        public static WorldObjectType WorldObjectFromWot(string wotFilePath, bool ignoreFormatError)
        {
            return ReadWot(wotFilePath, ignoreFormatError);
        }

        private static WorldObjectType ReadWot(string wotFilePath, bool ignoreFormatError)
        {
            var wot = new WorldObjectType();
            using(var wotReader = new StreamReader(File.OpenRead(wotFilePath)))
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
                        throw new TPException($"EntityType '{line}' of AIENTITYDEFINITION is invalid. Make sure it matches the Type of the world object.");
                    }

                    // If not empty and valid, check the factory type
                    var factoryType = wotReader.ReadLine().Substring(19).Trim('\'');

                    ReadAiEntityDefinition(wotReader, wot, factoryType, aiEntityDefinitionString, ignoreFormatError);
                }
            }
            return wot;
        }

        private static void ReadAiEntityDefinition(StreamReader wotReader, WorldObjectType wot, string factoryType, string aiEntityDefinitionString, bool ignoreFormatError)
        {
            wotReader.ReadLine(); // ContData
            wotReader.ReadLine(); // Start section

            switch (factoryType)
            {
                case "DragonAI":
                    var dragonAI = new DragonAI(aiEntityDefinitionString)
                    {
                        DefaultSightRange = wotReader.ReadAndParseFloat("Default SightRange Float ", ignoreFormatError)
                    };
                    wot.AiEntityDefinition = dragonAI;
                    break;
                case "IslandAI":
                    var islandAI = new IslandAI(aiEntityDefinitionString)
                    {
                        DefaultSightRange = wotReader.ReadAndParseFloat("Default SightRange Float ", ignoreFormatError)
                    };
                    wot.AiEntityDefinition = islandAI;
                    break;
                case "SpaceAnimalAI":
                    var spaceAnimalAI = new SpaceAnimalAI(aiEntityDefinitionString);
                    wot.AiEntityDefinition = spaceAnimalAI;
                    break;
                case "SpaceObjectAI":
                    var spaceObjectAI = new SpaceObjectAI(aiEntityDefinitionString);
                    wot.AiEntityDefinition = spaceObjectAI;
                    break;
                case "MineAI":
                    var mineAI = new MineAI(aiEntityDefinitionString)
                    {
                        DefaultSightRange = wotReader.ReadAndParseFloat("Default SightRange Float ", ignoreFormatError)
                    };
                    wot.AiEntityDefinition = mineAI;
                    break;
                case "GunAI":
                    var gunAI = new GunAI(aiEntityDefinitionString)
                    {
                        IsLobbingGun = wotReader.ReadAndParseBool("Is Lobbing Gun Bool ", ignoreFormatError),
                        IsMineLayingGun = wotReader.ReadAndParseBool("Is MineLaying Gun Bool ", ignoreFormatError),
                        IsTorpedoLauncherGun = wotReader.ReadAndParseBool("Is TorpedoLauncher Gun Bool ", ignoreFormatError),
                        IsPointDefenseGun = wotReader.ReadAndParseBool("Is PointDefense Gun Bool ", ignoreFormatError)
                    };
                    wot.AiEntityDefinition = gunAI;
                    break;
                case "VolcanoAI":
                    var volcanoAI = new VolcanoAI(aiEntityDefinitionString);
                    wot.AiEntityDefinition = volcanoAI;
                    break;
                case "ShipAI":
                    var shipAI = new ShipAI(aiEntityDefinitionString)
                    {
                        DefaultSightRange = wotReader.ReadAndParseFloat("Default SightRange Float ", ignoreFormatError)
                    };
                    wot.AiEntityDefinition = shipAI;
                    break;
                default:
                    throw new TPException($"FactoryType '{factoryType}' of AIENTITYDEFINITION is invalid.");
            }

            wotReader.ReadLine(); // End section
        }
    }
}
