using SwarmUI.Core;
using SwarmUI.Utils;

namespace Spoomples.Extensions.WildcardImporter
{
    public class WildcardImporterExtension : Extension
    {
        public override void OnPreInit()
        {
            Logs.Debug("WildcardImporter Extension started.");
            ScriptFiles.Add("Assets/wildcard_importer.js");
            ScriptFiles.Add("Assets/dropzone-min.js");
            StyleSheetFiles.Add("Assets/dropzone.css");
        }

        public override void OnInit()
        {
            WildcardImporterAPI.Register();
        }
    }
}
