using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PeirceGen
{
    public class Grammar
    {
        //public string 

        public class Command
        {
            public string Production { get; set; }
            public string Case { get; set; }

            public string NameSuffix { get; set; }

            public string ToLeanConstructor() { return this.Production + '.' + this.Case;  }
        }

        public List<Production> Productions = new List<Production>();

        public static Dictionary<char, ProductionType> TokenToProductionTypeMap = new Dictionary<char, ProductionType>()
        {
            { '+', ProductionType.Capture },
            { '_', ProductionType.Throw },
            { '=', ProductionType.Passthrough },
            { '*', ProductionType.Array },
            { '1', ProductionType.Single },
            { '>', ProductionType.Inherits },
            { '.', ProductionType.Hidden }
        };


        public enum ProductionType
        {
            Capture,
            Throw,
            Passthrough,
            Inherits,
            Array,
            Single,
            Hidden
        }

        public enum CaseType
        { 
            Ident,
            Real,
            Op,
            Pure,
            Passthrough,
            Array,
            ArrayOp,
            Inherits,
            Hidden
        }

        public static CaseType TokenToCaseTypeMap(List<string> toks)
        {
            return
                toks.Count > 1 && toks[1][0] == '.' ? CaseType.Hidden :
                toks.Count > 0 && toks[0][0] == '>' ? CaseType.Inherits :
                toks.Count > 0 && toks[0][0] == '*' ? CaseType.Array :
                toks.Count > 1 && toks[1][0] == '*' ? CaseType.ArrayOp :
                toks[0] == "IDENT" ? CaseType.Ident :
                toks[0] == "FLOAT" ? CaseType.Real :
                toks.Count > 0 && TokenToProductionTypeMap.Keys.Contains(toks[0][0]) && TokenToProductionTypeMap[toks[0][0]] == ProductionType.Passthrough ? CaseType.Passthrough :
                toks.Count > 1 && TokenToProductionTypeMap.Keys.Contains(toks[0][0]) ? CaseType.Pure :
                CaseType.Op;
        }

        public static bool IsProduction(string candidate)
        {
            return TokenToProductionTypeMap.Keys.Contains(candidate[0]);
        }

        public static string TrimProductionType(string candidate)
        {
            return IsProduction(candidate) ? candidate.Substring(1) : candidate;
        }

        public class Production
        {
            public List<Case> Cases = new List<Case>();
            public ProductionType ProductionType;
            public string Name { get; set; }

            public string Description { get; set; }

            public bool HasPassthrough { get; set; }
            public Production Passthrough;
            public bool HasInherits { get; set; }
            public Production Inherits { get; set; }


            public Command Command { get; set; }

            public bool IsTranslationDeclare { get; set; }
            public bool IsVarDeclare { get; set; }
            public bool IsFuncDeclare { get; set; }
        }

        public class Case
        {
            public CaseType CaseType;
            public string Name { get; set; }

            public string Description { get; set; }

            public List<Production> Productions = new List<Production>();
            public List<string> ProductionRefs = new List<string>();

            public Func<Production, string> CoordsToString { get; set; }

            public Func<Production, Space, Space.SpaceObject, Space.SpaceInstance, string> InterpTranslation { get; set; }

            public Command Command { get; set; }

            public bool IsTranslationDeclare { get; set; }
            public bool IsVarDeclare { get; set; }
            public bool IsFuncDeclare { get; set; }

            public bool LinkSpace { get; set; }

            public void ParseCoordsToString(bool fromCommand)
            {
                /*
                 * std::string file_id_;
                    std::string file_name_;
                    std::string file_path_;

                    std::string name_; //only used for Decl. possibly subclass this, or else this property is unused elsewhere

                    int begin_line_no_;
                    int begin_col_no_;
                    int end_line_no_;
                    int end_col_no_;

                 * */
                var retval = "std::string(\"\")";
                if (true)
                    retval += @"+" + @" ""COMMAND.B.L""+ std::to_string(state_->begin_line_no_) + ""C"" + std::to_string(state_->begin_col_no_) + "".E.L"" + std::to_string(state_->end_line_no_) + ""C"" + std::to_string(state_->end_col_no_)";
                this.CoordsToString = (prod) => { return @"(this->getIndex() > 0 ? ""INDEX""+std::to_string(this->getIndex())+""."":"""")+"+"\"" + (prod.Passthrough != null ? prod.Passthrough.Name : prod.Name).Replace("_", ".") + "\" + " + retval; };
            }

            public void ParseCoordsToString(string toParse)
            {
                /*
                 * std::string file_id_;
                    std::string file_name_;
                    std::string file_path_;

                    std::string name_; //only used for Decl. possibly subclass this, or else this property is unused elsewhere

                    int begin_line_no_;
                    int begin_col_no_;
                    int end_line_no_;
                    int end_col_no_;

                 * */
                var retval = "std::string(\"\")";
                if (toParse.Contains("$NAME"))
                    retval += @" + state_->name_";
                if (toParse.Contains("$LOC"))
                    retval += @"+" + @" "".B.L""+ std::to_string(state_->begin_line_no_) + ""C"" + std::to_string(state_->begin_col_no_) + "".E.L"" + std::to_string(state_->end_line_no_) + ""C"" + std::to_string(state_->end_col_no_)";
                this.CoordsToString = (prod) => { return @"(this->getIndex() > 0 ? ""INDEX""+std::to_string(this->getIndex())+""."":"""")+" + "\"" + (prod.Passthrough != null ? prod.Passthrough.Name : prod.Name).Replace("_", ".") + "\" + " + retval; };
            }

            public void ParseInterpTranslation(bool fromCommand)
            {
                this.InterpTranslation = (prod, sp, spobj, spinst) =>
                {
                    int i = 0;
                    var retval = @"
            auto case_coords = dynamic_cast<coords::" + (prod.Passthrough != null ? prod.Passthrough.Name : prod.Name) + @"*>(this->coords_);";
                    if(this.Command is Grammar.Command && prod.Command is Grammar.Command)
                    {
                        retval += @"
            retval += ""def " + @""" + case_coords->toString() + """ + this.Command.NameSuffix + "" + @" : " + this.Command.Production + " := " + this.Command.ToLeanConstructor() + @" " + @""" + " + (this.Productions.Count > 0 ? string.Join(" + ", (this.Productions.Select(p_ => "operand_" + ++i + "->coords_->toString()"))) : "\"\"") + @";
";
                        retval += @"
            retval += ""def " + (prod.Passthrough != null ? prod.Passthrough.Name : prod.Name) + @""" + case_coords->toString() + """ + "" + @" : " + prod.Command.Production + " := " + prod.Command.ToLeanConstructor() + @" " + @""" + case_coords->toString() + """ + this.Command.NameSuffix + "\";";

                    }
                    else if (this.Command is Grammar.Command)
                    {
                        retval += @"
            retval += ""def " + @""" + case_coords->toString() + """ + "" + @" : " + this.Command.Production + " := " + this.Command.ToLeanConstructor() + @" " + @""" + " + (this.Productions.Count > 0 ? string.Join(" + ", (this.Productions.Select(p_ => "operand_" + ++i + "->coords_->toString()"))) : "\"\"") + @";
";
                    }
                    else if (prod.Command is Grammar.Command)
                    {
                        retval += @"
            retval += ""def " + (prod.Passthrough != null ? prod.Passthrough.Name : prod.Name) + @""" + case_coords->toString() + """ + "" + @" : " + prod.Command.Production + " := " + prod.Command.ToLeanConstructor() + @" " + @""" + " + (this.Productions.Count > 0 ? string.Join(" + ", (this.Productions.Select(p_ => "operand_" + ++i + "->coords_->toString()"))) : "\"\"") + @";
";

                    }
                    return retval;
                };
            }

            public void ParseInterpTranslation(string toParse)
            {
                /*{1=2,Command}*/
                toParse = toParse.Replace("<", "").Replace(">", "");

                var spl = toParse.Split(',');

                var parsed = Regex.Match(spl[0], @"(?:(\d*)([^0-9]*))*");

                this.InterpTranslation = (prod, sp, spobj, spinst) =>
                {
                    var defaultStr = (sp == default(Space) || spobj == default(Space.SpaceObject)) ? "_" : sp.Prefix + spobj.Name + "Default";
                    var hasDefault = spl[1].Contains("D");
                    var hasIndex = spl[1].Contains("I");
                    int i = 0;
                    var retval = "";
                    if (sp == default(Space) || spobj == default(Space.SpaceObject))
                    {
                        retval = @"
            auto case_coords = dynamic_cast<coords::" + (prod.Passthrough != null ? prod.Passthrough.Name : prod.Name) + @"*>(this->coords_);
            retval += ""def " + @""" + case_coords->toString() + "" : ^ := ""
             + " + (hasDefault ? "\" " + spl[1][0] + defaultStr + " \"" :
                            hasIndex ? "\"" + spl[1][0] + "\"" + "+ std::to_string(++GLOBAL_INDEX)" :
                                            spl[0] == "I" ?
                                                string.Join("+ \"" + spl[1] + "\" +", this.Productions.Select(p_ => "\"(" + "\" + operand_" + ++i + "->coords_->toString() + \")\"")) :
                                            spl[0] == "B" ? "+ \"(" + spl[1][0] + "(\"+this->operand_1->coords_->toString()+\"))"+spl[1][1]+"(" + spl[1][2]+"(\"+this->operand_2->coords_->toString()+\"))\";" :
                                            spl[0] == "A" ?
                                                string.Join(" ", this.Productions.Select(p_ => "\" (" + "\" + operand_" + ++i + "->coords_->toString() + \") + \"")) + " + \"" + spl[1] + "\"" :
                                                "\"" + spl[1] + "\" " + string.Join(" ", this.Productions.Select(p_ => "+ \"("  + "\" + operand_" + ++i + "->coords_->toString() + \")\""))) + @";
            //return retval;
    ";
                    }
                    else
                    {
                        retval = @"
            auto case_coords = dynamic_cast<coords::" + (prod.Passthrough != null ? prod.Passthrough.Name : prod.Name) + @"*>(this->coords_);
            retval += ""def " + @""" + case_coords->toString() + "" : " + sp.Prefix + spobj.Name + spl[2] + @" "" +  "" := ""
             + " + (hasDefault ? "\" " + spl[1][0] + "(" + defaultStr + " (Eval"+ sp.Prefix +"SpaceExpression " + spinst.FieldValues[0] + "sp)) \"" :
                            hasIndex ? "\" " + spl[1][0] + "\"" + "+ std::to_string(++GLOBAL_INDEX)" :
                                            (spl[0] == "I" ?
                                                string.Join("+ \"" + spl[1] + "\" +", this.Productions.Select(p_ => "\"(" + "\" + operand_" + ++i + "->coords_->toString() + \") \"")) :
                                            spl[0] == "A" ?
                                                string.Join(" ", this.Productions.Select(p_ => "\"(" + "\" + operand_" + ++i + "->coords_->toString() + \") \" + ")) + "\" " + spl[1] + " \"" :
                                                "\" " + spl[1] + "\" " + string.Join(" ", this.Productions.Select(p_ => "+ \"(" + "\" + operand_" + ++i + "->coords_->toString() + \")\"")))) + @";
            //return retval;
    ";
                    }
                    if (this.Command is Grammar.Command && prod.Command is Grammar.Command)
                    {
                        retval += @"
            retval += ""def " + @""" + case_coords->toString() + """ + this.Command.NameSuffix + "" + @" : " + this.Command.Production + " := " + this.Command.ToLeanConstructor() + @" " + @""" + " + (this.Productions.Count > 0 ? string.Join(" + ", (this.Productions.Select(p_ => "operand_" + ++i + "->coords_->toString()"))) : "\"\"") + @";
";
                        retval += @"
            retval += ""def " + (prod.Passthrough != null ? prod.Passthrough.Name : prod.Name) + @""" + case_coords->toString() + """ + "" + @" : " + prod.Command.Production + " := " + prod.Command.ToLeanConstructor() + @" " + @""" + case_coords->toString() + """ + this.Command.NameSuffix + "\";";

                    }
                    else if (this.Command is Grammar.Command)
                    {
                        retval += @"
            retval += ""def " + @""" + case_coords->toString() + """ + "" + @" : " + this.Command.Production + " := " + this.Command.ToLeanConstructor() + @" " + (prod.Passthrough != null ? prod.Passthrough.Name : prod.Name) + @""" + case_coords->toString();
";
                    }
                    else if(prod.Command is Grammar.Command)
                    {
                        retval += @"
            retval += ""def " + @""" + case_coords->toString() + """ + "" + @" : " + prod.Command.Production + " := " + prod.Command.ToLeanConstructor() + @" " + (prod.Passthrough != null ? prod.Passthrough.Name : prod.Name) + @""" + case_coords->toString();
";

                    }


                    return retval;
                };
            }
        }

    }

    /*
     * 
--Define instantiable spaces

{	
	EuclideanGeometry, Euclidean,{Name,Dimension},Geometric
	ClassicalTime, Affine, {Name},Time
	ClassicalVelocity, Vector, {Name,Dimension},Velocity
},
--Define instances
{
	EuclideanGeometry,{geom3d,3}
	ClassicalTime,{time},
	ClassicalVelocity,{vel,3}
}
     * 
     * */

    public class Space
    {
        public string Name { get; set; }
        public SpaceCategory Category { get; set; }

        public enum FieldType
        {
            Name = 1,
            Dimension = 2
        }

        public int FieldMask { get; set; }

        public bool MaskContains(FieldType ft)
        {
            return (this.FieldMask & (int)ft) == (int)ft;
        }

        public string Prefix { get; set; }

        public SpaceCategory Inherits { get; set; }


        public class SpaceCategory
        {
            public string Category { get; set; }

            public List<SpaceObject> Objects = new List<SpaceObject>();

            public void Add(SpaceObject o)
            {
                if (!Objects.Any(o_ => o_.Name == o.Name))
                    this.Objects.Add(o);
            }

        }

        public class SpaceObject
        {
            public string Name { get; set; }

            public bool HasFrame { get; set; }

            public bool IsTransform { get; set; }

            public bool IsMap { get; set; }
        }

        public static void PropagateInheritance(List<Space> curSpaces, List<Space> allSpaces)
        {
            foreach(var sp in curSpaces)
            {
                if (sp.Inherits != default(SpaceCategory))
                    sp.Inherits.Objects.ForEach(o_ => sp.Category.Add(o_));
                PropagateInheritance(allSpaces.Where(sp_ => sp_.Inherits == sp.Category).ToList(), allSpaces);
            }
        }

        public static Dictionary<Space, SpaceObject> RetrieveInheritedObjects(Space sp, SpaceObject obj, List<Space> allSpaces)
        {
            Dictionary<Space, SpaceObject> curLevel = new Dictionary<Space, SpaceObject>();

            curLevel[sp] = obj;

            foreach(var child in allSpaces.Where(sp_ => sp_.Inherits == sp.Category))
            {
                curLevel.Add(child, child.Category.Objects.Single(ob_ => ob_ == obj));

                var next = RetrieveInheritedObjects(child, obj, allSpaces);

                next.Keys.ToList().ForEach(key => curLevel[key] = next[key] );
            }

            return curLevel;
        }

        public class SpaceInstance
        {
            public string TypeName { get; set; }
            public string InstanceName { get; set; }
            public List<string> FieldValues = new List<string>();
        }
    }

    public class Peirce
    {
        public Peirce()
        {

            Grammar = new Grammar();
            Spaces = new List<Space>();
            SpaceToObjectMap = new Dictionary<Space, Space.SpaceObject>();
            GrammarRuleToSpaceObjectMap = new Dictionary<Grammar.Production, List<(Space, Space.SpaceObject)>>();
            SpaceInstances = new List<Space.SpaceInstance>();
            Categories = new List<Space.SpaceCategory>();
        }

        public Grammar Grammar { get; set; }
        public List<Space> Spaces { get; set; }

        public List<Space.SpaceInstance> SpaceInstances { get; set; }

        public List<Space.SpaceCategory> Categories { get; set; }

        public Dictionary<Space, Space.SpaceObject> SpaceToObjectMap;

        public Dictionary<Grammar.Production, List<(Space, Space.SpaceObject)>> GrammarRuleToSpaceObjectMap { get; set; }
    }

    public class ParsePeirce
    {
        public const string GrammarFile = "C:\\Users\\msfti\\source\\repos\\givemeros\\PeirceGen\\Grammar";

        public static readonly Peirce Instance = new Peirce();
        
        /*{
            Grammar = new Grammar()
            {
                Productions = new List<Grammar.Production>()
                {
                     new Grammar.Production()
                     {

                     },
                     new Grammar.Production()
                     {

                     }
                }

            }
        };*/
        /*{
            Grammar = new Grammar(),
            Spaces = new List<Space>(),
            SpaceToObjectMap = new Dictionary<Space, Space.SpaceObject>(),
            GrammarRuleToSpaceObjectMap = new Dictionary<Grammar.Case, Space.SpaceObject>()
        };*/

        static ParsePeirce()
        {
            /*
             * {
            ,
             Grammar = new Grammar()
             {
                  Productions = new List<Grammar.Production>()
                  {

                  }
             },
              GrammarRuleToSpaceObjectMap = new Dictionary<Grammar.Production, List<(Space, Space.SpaceObject)>>()
              {

              },
               SpaceInstances = new List<Space.SpaceInstance>()
               {

               },
                Spaces = new List<Space>()
                {
                     new Space()
                     {
                          Categor
                     }
                },
                 SpaceToObjectMap = new Dictionary<Space, Space.SpaceObject>()
                 {

                 }
        };
             * */
             /*
            var euclideanCat = new Space.SpaceCategory()
            {
                Category = "Euclidean",
                Objects = new List<Space.SpaceObject>()
                     {
                       new Space.SpaceObject(){ Name = "Rotation"},
                       new Space.SpaceObject(){ Name = "Orientation"},
                       new Space.SpaceObject(){ Name = "Angle"},
                       new Space.SpaceObject(){ Name = "FrameChange"},
                       new Space.SpaceObject(){ Name = "Point"},
                       new Space.SpaceObject(){ Name = "HomogeneousPoint"},
                       new Space.SpaceObject(){ Name = "Vector"},
                       new Space.SpaceObject(){ Name = "Scalar"},
                       new Space.SpaceObject(){ Name = "BasisChange"},
                       new Space.SpaceObject(){ Name = "Scaling"},
                       new Space.SpaceObject(){ Name = "Shear" }
                     }
            };

            var affineCat = new Space.SpaceCategory()
            {
                Category = "Affine",
                Objects = new List<Space.SpaceObject>()
                     {
                         new Space.SpaceObject(){ Name = "FrameChange" },
                         new Space.SpaceObject(){ Name = "Point"},
                         new Space.SpaceObject(){ Name = "HomogeneousPoint"},
                         new Space.SpaceObject(){ Name = "Vector"},
                         new Space.SpaceObject(){ Name = "Scalar"},
                         new Space.SpaceObject(){ Name = "BasisChange"},
                         new Space.SpaceObject(){ Name = "Scaling"},
                         new Space.SpaceObject(){ Name = "Shear"}

                     }
            };

            var vectorCat = new Space.SpaceCategory()
            {
                Category = "Vector",
                Objects = new List<Space.SpaceObject>()
                    {
                         new Space.SpaceObject(){ Name = "Vector"},
                         new Space.SpaceObject(){ Name = "Scalar"},
                         new Space.SpaceObject(){ Name = "BasisChange"},
                         new Space.SpaceObject(){ Name = "Scaling"},
                         new Space.SpaceObject(){ Name = "Shear"}
                    }
            };

            ParsePeirce.Instance.Categories = new List<Space.SpaceCategory>()
            {
                euclideanCat,
                affineCat,
                vectorCat
            };

            var euclideanSpace = new Space()
            { 
                Category = euclideanCat,
                FieldMask = 3, 
                Inherits = affineCat, 
                Name = "EuclideanGeometry",
                Prefix = "EuclideanGeometry"
            };

            var timeSpace = new Space()
            {
                Category = affineCat,
                FieldMask = 1,
                Inherits = vectorCat,
                Name = "ClassicalTime",
                Prefix = "Time"
            };

            var velSpace = new Space()
            {
                Category = vectorCat,
                FieldMask = 3,
                Inherits = null,
                Name = "ClassicalVelocity",
                Prefix = "Velocity"
            };

            ParsePeirce.Instance.Spaces = new List<Space>()
            {
                euclideanSpace,
                timeSpace, 
                velSpace
            };

            ParsePeirce.Instance.SpaceInstances = new List<Space.SpaceInstance>()
            {
                 new Space.SpaceInstance()
                 {
                      InstanceName = "geom3d",
                       TypeName = "EuclideanGeometry",
                        FieldValues = new List<string>(){"worldGeometry", "3"}
                 },
                 new Space.SpaceInstance()
                 {
                     InstanceName = "time",
                      TypeName = "ClassicalTime",
                       FieldValues = new List<string>(){"worldTime"}
                 },
                 new Space.SpaceInstance()
                 {
                     InstanceName = "vel",
                      TypeName = "ClassicalVelocity",
                       FieldValues = new List<string>(){"worldVelocity", "3"}
                 }
            };

            var STMT = new Grammar.Production()
            {

            };

            var IFCOND = new Grammar.Production()
            {

            };

            var EXPR = new Grammar.Production()
            {

            };

            var ASSIGNMENT = new Grammar.Production()
            {

            };

            var DECLARE = new Grammar.Production()
            {

            };

            var REAL1_EXPR = new Grammar.Production()
            {

            };

            var REAL3_EXPR = new Grammar.Production()
            {

            };

            var REAL4_EXPR = new Grammar.Production()
            {

            };

            var REALMATRIX_EXPR = new Grammar.Production()
            {

            };

            var REAL1_VAR = new Grammar.Production()
            {

            };

            var REAL3_VAR = new Grammar.Production()
            {

            };

            var REAL4_VAR = new Grammar.Production()
            {

            };

            var REALMATRIX_VAR = new Grammar.Production()
            {

            };

            var REAL1_LITERAL = new Grammar.Production()
            {

            };

            var REAL3_LITERAL = new Grammar.Production()
            {

            };

            var REAL4_LITERAL = new Grammar.Production()
            {

            };

            var REALMATRIX_LITERAL = new Grammar.Production()
            {

            };

            ParsePeirce.Instance.Grammar = new Grammar()
            {
                Productions = new List<Grammar.Production>()
                {

                }
            }
            */

            /*
             * --Major sections are: 
             * Grammar, 
             * Spaces, 
             * Space Objects & Operations, 
             * Grammar Rule->Object+Operation Map, 
             * (Unimplemented yet) AST+Annotation->DSL Map, 
             * (Unimplemented yet) AST->Default Object/Operation Map
             * 
             * */

            var remaining_config = File.ReadAllLines(GrammarFile).ToList();

            var next_split = remaining_config.IndexOf("####");

            var grammar = remaining_config.Take(next_split).ToList();


            ParsePeirce.ParseGrammar(grammar);

            remaining_config = remaining_config.Skip(next_split + 1).ToList();

            next_split = remaining_config.IndexOf("####");

            var spaces = remaining_config.Take(next_split).ToList();

            ParsePeirce.ParseSpaces(spaces);


            remaining_config = remaining_config.Skip(next_split + 1).ToList();
            next_split = remaining_config.IndexOf("####");

            var objects = remaining_config.Take(next_split).ToList();

            ParsePeirce.ParseSpaceObjects(objects);

            remaining_config = remaining_config.Skip(next_split + 1).ToList();
            next_split = remaining_config.IndexOf("####");

            var grammar_to_object_map = remaining_config.Take(next_split).ToList();

            ParsePeirce.ParseGrammarToSpaceObjectMap(grammar_to_object_map);


            remaining_config = remaining_config.Skip(next_split + 1).ToList();
            next_split = remaining_config.IndexOf("####");

            var ast_annotation_to_dsl_map = remaining_config.Take(next_split).ToList();

            remaining_config = remaining_config.Skip(next_split + 1).ToList();
            next_split = remaining_config.IndexOf("####");

            var ast_to_default_domain_object_map = remaining_config.ToList();
        }
    

        static bool isComment(string line)
        {
            return line.Length == 0 || ( line[0] == '-' && line[1] == '-');
        }

        static void ParseGrammar(List<string> grammar)
        {
            var curProd = new Grammar.Production(); curProd = null;
            var curCase = new Grammar.Case(); curCase = null;
            try
            {
                foreach (var line in grammar)
                {
                    var fixedline = line.Length > 0 && line[0] == '\t' ? line.Substring(1) : line;


                    if (isComment(fixedline))
                    {
                        continue;
                    }
                    else if (string.IsNullOrEmpty(fixedline) && curProd != null)
                    {
                        curProd = null;
                    }
                    else if (string.IsNullOrEmpty(fixedline) && curProd == null)
                        continue;
                    else if (curCase == null && curProd != null)
                    {
                        var captionmatch = Regex.Match(fixedline, @"([^~]*)((?:~)((\w|\s)*))?");


                        var toks = captionmatch.Groups[1].Value.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList().Select(tok_ => tok_.Trim()).ToList();

                        bool isLast = !toks.Contains("|");

                        var coordsandinterp = toks.Where(tok_ => tok_.Contains("<") && tok_.Contains(">")).ToList();

                        var commandwrapper = toks.Where(tok_ => tok_[0] == '@' && tok_[tok_.Length - 1] == '@').Select(tok_ => tok_.Substring(1, tok_.Length - 2)).ToList();

                        var cmd = commandwrapper.Count == 0 ? default(Grammar.Command) : new Grammar.Command()
                        { Production = commandwrapper[0].Split(',')[0], Case = commandwrapper[0].Split(',')[1], NameSuffix = commandwrapper[0].Split(',')[2] };




                        toks = toks
                            .Where(tok_ => tok_ != "|" && !(tok_.Contains("<") && tok_.Contains(">")))
                            .Where(tok_ => !(tok_[0] == '@' && tok_[tok_.Length-1] == '@'))
                            .ToList();

                        /*
                         * +EXPR :=
	                            +REAL1_EXPR |
	                            +REAL3_EXPR |
	                            +REAL4_EXPR |
	                            +MATRIX_EXPR
                         * */
                        var ct = Grammar.TokenToCaseTypeMap(toks);

                        bool isFuncDeclare = toks[0][0] == 'f';
                        if (isFuncDeclare)
                            toks[0] = toks[0].Substring(1);
                        bool isTransDeclare = toks[0][0] == 't';
                        if (isTransDeclare)
                            toks[0] = toks[0].Substring(1);
                   


                        var linkSpace = (toks.Count > 1 && toks[1][1] == 's');
                        if(linkSpace)
                        {
                            toks[1] = toks[1][0] + toks[1].Substring(2);
                        }

                        curCase = new Grammar.Case()
                        {
                            CaseType = ct,
                            Name = ct == 
                                Grammar.CaseType.Real ? curProd.Name + (toks.Count) : 
                                ct == Grammar.CaseType.Ident ? "IDENT" : 
                                ct == Grammar.CaseType.Pure || ct == Grammar.CaseType.Pure ? curProd.Name + "_" + string.Join("_", toks.Select(t_ => Grammar.TrimProductionType(t_))) :
                                string.Join("_", toks.Select(t_ => Grammar.TrimProductionType(t_))),
                            ProductionRefs = ct == Grammar.CaseType.Hidden || ct == Grammar.CaseType.Op || ct == Grammar.CaseType.ArrayOp ? toks.Skip(1).ToList() : ct == Grammar.CaseType.Inherits || ct == Grammar.CaseType.Array || ct == Grammar.CaseType.Pure || ct == Grammar.CaseType.Real || ct == Grammar.CaseType.Passthrough ? toks : new List<string>(),
                            Productions = new List<Grammar.Production>(), //fix this.... these need to "resolve" incrementally
                            Description = captionmatch.Groups.Count > 2 ? captionmatch.Groups[3].Value : ""
                            , CoordsToString = (p) => { return "\"Not implemented\";"; },
                             InterpTranslation = (p, s, sp, spi) => { return "\"Not implemented\";"; },
                              Command = cmd,
                              IsTranslationDeclare = isTransDeclare,
                              IsFuncDeclare = isFuncDeclare,
                              LinkSpace = linkSpace
                               
                        };

                        if (coordsandinterp.Count == 2) {
                            curCase.ParseCoordsToString(coordsandinterp[0]);
                            curCase.ParseInterpTranslation(coordsandinterp[1]);
                        }
                        else if (cmd is Grammar.Command || curProd.Command is Grammar.Command)
                        {
                            curCase.ParseCoordsToString(true);
                            curCase.ParseInterpTranslation(true);
                        }
                        curProd.Cases.Add(curCase);

                        curCase = null;

                        if (isLast)
                            curProd = null;
                    }
                    else if (curProd == null)
                    {
                        var toks = fixedline.Split(' ').Where(tok_=>tok_.Length>1);
                        var commandwrapper = toks.Where(tok_ => tok_[0] == '@' && tok_[tok_.Length - 1] == '@').Select(tok_ => tok_.Substring(1, tok_.Length - 2)).ToList();

                        var cmd = commandwrapper.Count == 0 ? default(Grammar.Command) : new Grammar.Command()
                        { Production = commandwrapper[0].Split(',')[0], Case = commandwrapper[0].Split(',')[1], NameSuffix = commandwrapper[0].Split(',')[2] };

                        if (cmd is Grammar.Command)
                            fixedline = fixedline.Replace('@' + commandwrapper[0] + '@', "");

                        var last = fixedline.IndexOf(" :=");

                        bool isDeclare = (fixedline[1] == '#');
                        if (isDeclare)
                            fixedline = fixedline[0] + fixedline.Substring(2);

                        bool isFuncDeclare = fixedline[1] == 'f';
                        if (isFuncDeclare)
                            fixedline = fixedline[0] + fixedline.Substring(2);
                        bool isTransDeclare = fixedline[1] == 't';
                        if (isTransDeclare)
                            fixedline = fixedline[0] + fixedline.Substring(2);

                        curProd = new Grammar.Production()
                        {
                            ProductionType = Grammar.TokenToProductionTypeMap[fixedline[0]],

                            Name = Grammar.TrimProductionType(fixedline.Substring(0, last)).Trim(),
                            HasPassthrough = false,
                            Passthrough = default(Grammar.Production),
                             Command = cmd,
                            IsTranslationDeclare = isTransDeclare,
                            IsFuncDeclare = isFuncDeclare
                        };

                        Instance.Grammar.Productions.Add(curProd);
                    }
                }
                var t = default(List<Grammar.Production>);
                foreach (var prod in Instance.Grammar.Productions)
                {
                    if(prod.ProductionType == Grammar.ProductionType.Single)
                    {
                        prod.Name = prod.Name + "_" + prod.Cases[0].Name;
                    }

                    foreach (var pcase in prod.Cases)
                    {
                        if (pcase.CaseType == Grammar.CaseType.Real)
                            continue;

                        foreach (var pref in pcase.ProductionRefs)
                        {
                            t = Instance.Grammar.Productions.Where(p_ => p_.Name == Grammar.TrimProductionType(pref)).ToList();

                            pcase.Productions.Add(Instance.Grammar.Productions.Single(p_ => p_.Name == Grammar.TrimProductionType(pref)));
                        }

                        if (pcase.CaseType == Grammar.CaseType.Passthrough)
                        {
                            prod.HasPassthrough = true;
                            pcase.Productions[0].Passthrough = prod;
                        }
                        if(pcase.CaseType == Grammar.CaseType.Inherits)
                        {
                            prod.HasInherits = true;

                            pcase.Productions[0].Inherits = prod;
                            //pcase.CaseType = Grammar.CaseType.Pure;
                        }
                    }
                }
            }
            catch(Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        static void ParseSpaces(List<string> spaces)
        {
            /*
             * 
             * {
	                EuclideanGeometry, Euclidean,{Name,Dimension},Geometric
	                ClassicalTime, Affine, {Name},Time
	                ClassicalVelocity, Vector, {Name,Dimension},Velocity
                },
                --Define instances
                {
	                EuclideanGeometry,{geom3d,3}
	                ClassicalTime,{time},
	                ClassicalVelocity,{vel,3}
                }

             * */


            var sp = default(Space);

            bool inSpace = false;
            bool inInstance = false;

            Func<string, bool> begin = (ln) => ln.Trim() == "{";
            Func<string, bool> end = (ln) => ln.Trim() == "}";

            int
                preSpace = 1,
                space = 2,
                exSpace = 3,
                instance = 4;

            int status = 1;
            try
            {
                foreach (var line in spaces)
                {
                    var fixedline = line.Length > 0 && line[0] == '\t' ? line.Substring(1) : line;


                    if (isComment(fixedline))
                        continue;
                    else if (status == preSpace && !begin(fixedline))
                        continue;
                    else if (status == preSpace && begin(fixedline))
                        status = space;
                    else if (status == space && end(fixedline))
                        status = exSpace;
                    else if (status == space && !string.IsNullOrEmpty(fixedline))
                    {
                        //EuclideanGeometry, Euclidean,{Name,Dimension},Geometric
                        var stripped = Regex.Replace(fixedline, @"\s+", "");
                        var spl = stripped.Split(',');


                        var pattern = Regex.Escape("{") + "(.*)" + Regex.Escape("}");
                        var fieldsmatch = Regex.Match(spl[2], pattern).Groups[1].Value.Split('-').ToList();
                        var fields = fieldsmatch.Select(f => Enum.Parse(typeof(Space.FieldType), f));


                        if (!Instance.Categories.Any(c => c.Category == spl[1]))
                            Instance.Categories.Add(new Space.SpaceCategory() { Category = spl[1], Objects = new List<Space.SpaceObject>() });
                        sp = new Space()
                        {
                            Name = spl[0],
                            Category = Instance.Categories.Single(c => c.Category == spl[1]),
                            Prefix = spl[3],
                             
                        };

                        fields.ToList().ForEach(f => sp.FieldMask = sp.FieldMask | (int)f);

                        Instance.Spaces.Add(sp);

                    }
                    else if (status == exSpace && !begin(fixedline))
                        continue;
                    else if (status == exSpace && begin(fixedline))
                        status = instance;
                    else if (status == instance && !end(fixedline))
                    {
                        var stripped = Regex.Replace(fixedline, @"\s+", "");
                        var spl = stripped.Split(',');


                        var pattern = Regex.Escape("{") + "(.*)" + Regex.Escape("}");
                        
                        var fieldsmatch = Regex.Match(spl[2], pattern).Groups[1].Value.Split('-').ToList();
                        var fields = fieldsmatch;// fieldsmatch.Select(f => Enum.Parse(typeof(Space.FieldType), f));


                        //EuclideanGeometry,{geom3d,3}
                        Instance.SpaceInstances.Add(new Space.SpaceInstance()
                        {
                            TypeName = spl[0],
                            InstanceName = spl[1],
                            FieldValues = fields
                        });
                    }
                    else if (status == instance && end(fixedline))
                        break;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void ParseSpaceObjects(List<string> spaceobjects)
        {
            /*
             * 
                Vector={Vector,Scalar,BasisChange,Scaling,Shear}
                Affine={FrameChange,Point,HomogenousPoint},Vector
                Euclidean={Rotation,Orientation,Angle},Affine

             * 
             * */
            try
            {
                foreach (var lin in spaceobjects)
                {

                    var line = lin.Length > 0 && lin[0] == '\t' ? lin.Substring(1) : lin;


                    if (isComment(line))
                        continue;
                    else
                    {
                        var pattern = @"(\w*)=" + Regex.Escape("{") + "(.*)" + Regex.Escape("}") + @"(?:,(\w*))*";

                        var matches = Regex.Match(line, pattern);

                        var captures = matches.Groups;

                        var spaceCat = captures[1].Value;

                        var spaces = Instance.Spaces.Where(sp_ => sp_.Category.Category == spaceCat).ToList();


                        foreach (var sp in spaces)
                        {
                            var inherits =
                                    captures.Count > 3 && !string.IsNullOrEmpty(captures[3].Value) ?
                                    Instance.Categories.Single(sp_ => sp_.Category == captures[3].Value) : default(Space.SpaceCategory);
                            //var inherits = Instance.Categories.Single(c => c.Category == inheritName);


                            captures[2].Value.Split(',').ToList().ForEach(o_ =>
                            {
                                bool hasFrame = o_[0] == 'f';
                                var trunc = o_;
                                if (hasFrame)
                                    trunc = o_.Substring(1);
                                bool isMap = o_[0] == 'm';
                                if (isMap)
                                    trunc = o_.Substring(1);
                                bool isTransformation = o_[0] == 't';
                                if(isTransformation)
                                    trunc = o_.Substring(1);


                                sp.Category.Add(new Space.SpaceObject()
                                {
                                    Name = trunc,
                                    HasFrame = hasFrame, 
                                    IsMap = isMap,
                                    IsTransform = isTransformation
                                });
                            });

                            sp.Inherits = inherits;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            var initial = Instance.Spaces.Where(s_ => s_.Inherits == default(Space.SpaceCategory)).ToList();

            Space.PropagateInheritance(initial, Instance.Spaces);
        }

        static void ParseGrammarToSpaceObjectMap(List<string> grammartospaceobjects)
        {
            /*
            {
                REAL1_EXPR ={ Euclidean.Angle,Vector.Scalar}
                REAL3_EXPR ={ Euclidean.Rotation,Euclidean.Orientation,Vector.Vector,Affine.Point}
                MATRIX_EXPR ={ Vector.Scaling,Vector.Shear,Vector.BasisChange, Affine.FrameChange, Euclidean.Rotation}
                REAL4_EXPR ={ Euclidean.Rotation,Euclidean.Orientation,Affine.HomogenousPoint}
            }*/
            Func<string, bool> begin = (ln) => ln.Trim() == "{";
            Func<string, bool> end = (ln) => ln.Trim() == "}";

            int
                prematch = 1,
                matched = 2,
                exSpace = 3,
                instance = 4;

            int current = prematch;

            try
            {
                foreach (var lin in grammartospaceobjects)
                {

                    var line = lin.Length > 0 && lin[0] == '\t' ? lin.Substring(1) : lin;


                    if (isComment(line))
                        continue;
                    else if (current == prematch && begin(line))
                    {
                        current = matched;
                    }
                    else if (current == matched && !string.IsNullOrEmpty(line) && !end(line))
                    {
                        var pattern = Regex.Escape("{") + "(.*)" + Regex.Escape("}") + @"(?:,(\w*))*";

                        var stripped = Regex.Replace(line, @"\s+", "");

                        var prodName = Grammar.TrimProductionType(stripped.Split('=')[0]);

                        var prod = Instance.Grammar.Productions.Single(prod_ => prod_.Name == prodName || (prod_.ProductionType == Grammar.ProductionType.Single) && prod_.Name.Contains(prodName));

                        var match = Regex.Match(stripped.Split('=')[1], pattern).Groups[1].Value;

                        foreach (var objType in match.Split(','))
                        {
                            var spaceCat = objType.Split('.')[0];
                            var objName = objType.Split('.')[1];

                            var matchedSpaces = Instance.Spaces.Where(sp_ => sp_.Category.Category == spaceCat).ToList();

                            foreach (var sp in matchedSpaces)
                            {
                                var obj = sp.Category.Objects.Single(obj_ => obj_.Name == objName);


                                var spaceObjDict = Space.RetrieveInheritedObjects(sp, obj, Instance.Spaces);
                                Instance.GrammarRuleToSpaceObjectMap[prod] = Instance.GrammarRuleToSpaceObjectMap.ContainsKey(prod) ? Instance.GrammarRuleToSpaceObjectMap[prod]: new List<(Space, Space.SpaceObject)>();
                                spaceObjDict.Keys.ToList().ForEach(key => Instance.GrammarRuleToSpaceObjectMap[prod].Add((key, spaceObjDict[key])));
                            }
                        }
                    }
                    else if (current == matched && end(line))
                        break;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            //var initial = Instance.Spaces.Where(s_ => s_.Inherits == default(Space.SpaceCategory)).ToList();

           // Space.PropagateInheritance(initial, Instance.Spaces);
        }
    }
}
