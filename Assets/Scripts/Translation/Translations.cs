using ColdCry.Exception;
using ColdCry.GameObjects;
using ColdCry.Utility;
using System.Collections.Generic;

namespace ColdCry
{
    public static class Translations
    {
        private static Dictionary<string, string> translations = new Dictionary<string, string>();
        private static readonly string FILE_PATH = "Assets/Resources/Translations/";

        /// <summary>
        /// Load all translations from langauge file
        /// </summary>
        /// <param name="language">Langauge that have to be loaded</param>
        public static void LoadTranslations(Language language)
        {
            if (translations == null) {
                translations = new Dictionary<string, string>();
            } else {
                translations.Clear();
            }

            string fileName = FILE_PATH + language.ToString() + ".txt";

            foreach (string line in Files.GetLines( fileName )) {
                string[] splitted = line.Split( '=' );

                if (splitted.Length != 2)
                    throw new TranslationException( "Corrupted translation file, something is missing: "
                        + FILE_PATH + language.ToString() + ".txt" );

                translations.Add( splitted[0], splitted[1] );
            }

            Language = language;
        }

        /// <summary>
        /// Gives text from current loaded file
        /// </summary>
        /// <param name="key">Text to load</param>
        /// <returns>Text from current language</returns>
        public static string Get(string key)
        {
            try {
                return translations[key];
            } catch (KeyNotFoundException) {
                return key;
            }
        }

        public static string GetResource(ResourceType resourceType)
        {
            return Get( "RESOURCE_" + resourceType );
        }

        public static string GetBuilding(BuildingType buildingType)
        {
            return Get( "BUILDING_" + buildingType );
        }

        public static string GetNumber(NumbersNames number)
        {
            return Get( "NUMBER_" + number );
        }

        public static string GetShortenNumber(NumbersNames number)
        {
            return Get( "S_NUMBER_" + number );
        }

        public static Language Language { get; private set; }
    }
}