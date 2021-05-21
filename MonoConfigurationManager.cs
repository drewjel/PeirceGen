using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeirceGen
{
    public class MonoConfigurationManager
    {
        public static readonly MonoConfigurationManager Instance = new MonoConfigurationManager();
        /*
        public static MonoConfigurationManager()
        {

        }
        
    <add key="GrammarPath" value="C:\Users\msfti\source\repos\givemeros\PeirceGen\"/>
    <add key="GenPath" value="C:\Users\msfti\source\repos\givemeros\PeirceGen\symlinkme\"/>
    <add key="MatcherPath" value="C:\Users\msfti\source\repos\givemeros\PeirceGen\symlinkme\ros_matchers\"/>
             
            { "GrammarPath", @"/peirce/gen/Grammars/" },
            { "GenPath", @"/peirce/gen/symlinkme/"},
            { "MatcherPath", @"/peirce/gen/symlinkme/ros_matchers/"}
            { "GrammarPath", @"C:\Users\msfti\source\repos\givemeros\PeirceGen\" },
            { "GenPath", @"C:\Users\msfti\source\repos\givemeros\PeirceGen\symlinkme\"},
            { "MatcherPath", @"C:\Users\msfti\source\repos\givemeros\PeirceGen\symlinkme\ros_matchers\"}
             */

        public Dictionary<string, string> config = new Dictionary<string, string>()
        {
            { "GrammarPath", @"/peirce/gen/Grammars/" },
            { "GenPath", @"/peirce/gen/symlinkme/"},
            { "MatcherPath", @"/peirce/gen/symlinkme/ros_matchers/"}
           // { "GrammarPath", @"C:\Users\msfti\source\repos\givemeros\PeirceGen\Grammars\" },
           // { "GenPath", @"C:\Users\msfti\source\repos\givemeros\PeirceGen\symlinkme\"},
            //{ "MatcherPath", @"C:\Users\msfti\source\repos\givemeros\PeirceGen\symlinkme\ros_matchers\"}
        };

        public string this[string key]
        {
            get { return config[key]; }
        }
    }
}
