global using static SZones.Translations;

namespace SZones;

#region Translations for Main
partial class ZoneManager
{
    public override TranslationList DefaultTranslations => DefaultTranslationList;

    public new string Translate(string key, params object[] args) => base.Translate(key.Trim(TranslationKeyTrimCharacters), args);
}
#endregion
// Don't rename this class, it will be used in analyzer.
public static partial class Translations
{
    #region Base
    public static char[] TranslationKeyTrimCharacters = new[] { '_' };
    /// <summary>
    /// Retrieves values from <see cref="Translations"/> type. [Only "<see langword="public"/> <see langword="static"/> <see langword="readonly"/> <see langword="string"/>" or 
    /// "<see langword="public"/> <see langword="const"/> <see langword="string"/>" fields]
    /// </summary>
    public static TranslationList DefaultTranslationList
    {
        get
        {
            var translations = new TranslationList();
            translations.AddRange(
            typeof(Translations).GetFields(BindingFlags.Static | BindingFlags.Public)
            .Where(x => (x.IsStatic && x.IsInitOnly) || x.IsLiteral)
            .Select(x =>
                new TranslationListEntry(x.Name.Trim(TranslationKeyTrimCharacters), (x.IsLiteral ? x.GetRawConstantValue() : x.GetValue(null)).ToString())
            ));
            return translations;
        }
    }
    /// <summary>
    /// This method is important for analyzer!
    /// </summary>
    public static string Translate(string translationKey, params object[] arguments) => ZoneManager.Instance.Translate(translationKey, arguments);
    #endregion
    // You can write static/const string fields(translations) here. By default _ will be trimmed
    public const string
        Create = "Zone {0} created.",
        Delete = "Zone {0} deleted.",
        Node = "Added node for zone {0}.",
        Teleport = "Teleported to zone {0}.",
        NotFound = "Zone {0} not found.",
        Help = "/{0} [{1}] [zone] |[type] [size]|";
}