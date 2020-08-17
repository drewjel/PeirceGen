using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PeirceGen
{
   // public class 

    public class Peirce
    {
        public static string Join<JoinType>(string joinstr, IEnumerable<JoinType> args, Func<JoinType, string> mapper) 
            //where JoinType : new()
        {
            return string.Join(joinstr, args.Select(mapper));
        }

        public Peirce()
        {

            Grammar = new Grammar();
            Spaces = new List<Space>();
            SpaceToObjectMap = new Dictionary<Space, Space.SpaceObject>();
            GrammarRuleToSpaceObjectMap = new Dictionary<Grammar.Production, List<(Space, Space.SpaceObject)>>();
           // SpaceInstances = new List<Space.SpaceInstance>();
            Categories = new List<Space.SpaceCategory>();
            MatcherProductions = new List<MatcherProduction>();
        }

        public Grammar Grammar { get; set; }
        public List<Space> Spaces { get; set; }

       // public List<Space.SpaceInstance> SpaceInstances { get; set; }

        public List<Space.SpaceCategory> Categories { get; set; }

        public Dictionary<Space, Space.SpaceObject> SpaceToObjectMap;

        public Dictionary<Grammar.Production, List<(Space, Space.SpaceObject)>> GrammarRuleToSpaceObjectMap { get; set; }

        public List<MatcherProduction> MatcherProductions;
    }

    public class ParsePeirce
    {
        public static readonly string GrammarFile = Directory.GetParent(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).FullName).FullName + @"\GrammarEmpty";

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

            remaining_config = remaining_config.Skip(next_split + 1).ToList();
            next_split = remaining_config.IndexOf("####");

            var matcherconfig = remaining_config.ToList();

            ParseMatchers.ParseRaw(matcherconfig);
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

                        bool isFuncDeclare = toks[0][0] == 'f';
                        if (isFuncDeclare)
                            toks[0] = toks[0].Substring(1);
                        bool isTransDeclare = toks[0][0] == 't';
                        if (isTransDeclare)
                            toks[0] = toks[0].Substring(1);
                        bool isVarDeclare = toks[0][0] == 'v';
                        if (isVarDeclare)
                            toks[0] = toks[0].Substring(1);

                        var linkSpace = (toks.Count > 1 && toks[1][1] == 's');
                        if(linkSpace)
                        {
                            toks[1] = toks[1][0] + toks[1].Substring(2);
                        }
                        var ct = Grammar.TokenToCaseTypeMap(toks);


                        curCase = new Grammar.Case()
                        {
                            CaseType = ct,
                            Name = //ct == 
                               // Grammar.CaseType.Value ? curProd.Name + (valueCount) : 
                                 //? "IDENT" : 
                                ct == Grammar.CaseType.Pure || ct == Grammar.CaseType.Pure ? curProd.Name + "_" + string.Join("_", toks.Select(t_ => Grammar.TrimProductionType(t_))) :
                                string.Join("_", toks.Select(t_ => Grammar.TrimProductionType(t_))),
                            ProductionRefs = 
                                ct == Grammar.CaseType.Hidden || 
                                ct == Grammar.CaseType.Op || 
                                ct == Grammar.CaseType.ArrayOp ? toks.Skip(1).ToList() : ct == Grammar.CaseType.Inherits || 
                                ct == Grammar.CaseType.Array || 
                                ct == Grammar.CaseType.Pure || 
                               // ct == Grammar.CaseType.Value || 
                                ct == Grammar.CaseType.Passthrough ? toks 
                                    : new List<string>(),
                            Productions = new List<Grammar.Production>(), //fix this.... these need to "resolve" incrementally
                            Description = captionmatch.Groups.Count > 2 ? captionmatch.Groups[3].Value : ""
                            , CoordsToString = (p) => { return "\"Not implemented\";"; },
                             InterpTranslation = (p, s, sp) => { return "\"Not implemented\";"; },
                              Production = curProd,
                              Command = cmd,
                              IsTranslationDeclare = isTransDeclare,
                              IsFuncDeclare = isFuncDeclare,
                               IsVarDeclare = isVarDeclare,
                              LinkSpace = linkSpace,
                              // ValueType = valueType,
                              // ValueCount = valueCount,
                              // ValueDefault = valueDefault
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
                        bool isVarDeclare = fixedline[1] == 'v';
                        if (isVarDeclare)
                            fixedline = fixedline[0] + fixedline.Substring(2);
                        //Console.WriteLine(fixedline);

                        var toksvcheck = fixedline.Replace(" :=", "").Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        var valueType = "";
                        var valueCount = 0;
                        var valueDefault = "";
                        Grammar.ValueContainer vc = default(Grammar.ValueContainer);
                        if (toksvcheck.Count > 1)
                        {
                            var vstr = toksvcheck[1].Substring(toksvcheck[1].IndexOf("(") + 1, toksvcheck[1].Length - toksvcheck[1].IndexOf("(") - 2).Split(',');
                            valueType = vstr[0];
                            valueCount = int.Parse(vstr[1]);
                            if (vstr.Length > 2)
                                valueDefault = vstr[2];

                            vc = new Grammar.ValueContainer() { ValueCount = valueCount, ValueDefault = valueDefault, ValueType = valueType };
                        }

                        curProd = new Grammar.Production()
                        {
                            ProductionType = Grammar.TokenToProductionTypeMap[fixedline[0]],

                            Name = Grammar.TrimProductionType(toksvcheck[0]).Trim(),
                            HasPassthrough = false,
                            Passthrough = default(Grammar.Production),
                             Command = cmd,
                            IsTranslationDeclare = isTransDeclare,
                            IsFuncDeclare = isFuncDeclare,
                             IsVarDeclare = isVarDeclare,
                              Cases = new List<Grammar.Case>(),
                               ValueContainer = vc
                               
                        };

                        Instance.Grammar.Productions.Add(curProd);
                    }
                }
                var t = default(List<Grammar.Production>);
                foreach (var prod in Instance.Grammar.Productions)
                {
                    if(prod.ProductionType == Grammar.ProductionType.Single || prod.ProductionType == Grammar.ProductionType.CaptureSingle)
                    {
                        prod.Name = prod.Name + "_" + prod.Cases[0].Name;
                    }

                    foreach (var pcase in prod.Cases)
                    {
                        //if (pcase.CaseType == Grammar.CaseType.Value)
                        //    continue;

                        foreach (var pref in pcase.ProductionRefs)
                        {
                            t = Instance.Grammar.Productions.Where(p_ => p_.Name == Grammar.TrimProductionType(pref)).ToList();
                            Console.WriteLine(pcase.Name + " " + pref);
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
                Console.WriteLine(ex.StackTrace);
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
                        //EuclideanGeometry, Euclidean,{Dimension=*},Geometric
                        var stripped = Regex.Replace(fixedline, @"\s+", "");
                        var spl = stripped.Split(',');


                        var pattern = Regex.Escape("{") + "(.*)" + Regex.Escape("}");
                        var fieldsmatch = Regex.Match(spl[2], pattern).Groups[1].Value.Split('-').ToList();


                        //var fields = fieldsmatch.Select(f => Enum.Parse(typeof(Space.FieldType), f));


                        if (!Instance.Categories.Any(c => c.Category == spl[1]))
                            Instance.Categories.Add(new Space.SpaceCategory() { Category = spl[1], Objects = new List<Space.SpaceObject>() });
                        sp = new Space()
                        {
                            Name = spl[0],
                            Category = Instance.Categories.Single(c => c.Category == spl[1]),
                            Prefix = spl[3],
                             
                        };

                        if (fieldsmatch.Any(x => x.Contains("Dimension")))
                        {
                            var dimmatch = fieldsmatch.Single(x => x.Contains("Dimension"));
                            var dimval = dimmatch.Split('=')[1];
                            if (dimval == "*")
                            {
                                sp.DimensionType = Space.DimensionType_.ANY;
                            }
                            else
                            {
                                sp.DimensionType = Space.DimensionType_.Fixed;
                                sp.FixedDimension = int.Parse(dimval);
                            }
                        }
                        else
                        {
                            sp.DimensionType = Space.DimensionType_.ANY;
                        }
                        
                        if (fieldsmatch.Any(x => x.Contains("Derived")))
                        {
                            var dimmatch = fieldsmatch.Single(x => x.Contains("Derived"));
                            var dimval = bool.Parse(dimmatch.Split('=')[1]);
                            sp.IsDerived = dimval;
                        }
                        else
                        {
                            sp.IsDerived = false;
                        }

                        //fields.ToList().ForEach(f => sp.FieldMask = sp.FieldMask | (int)f);

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

                        /*
                        //EuclideanGeometry,{geom3d,3}
                        Instance.SpaceInstances.Add(new Space.SpaceInstance()
                        {
                            TypeName = spl[0],
                            InstanceName = spl[1],
                            FieldValues = fields
                        });*/
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

                        var prod = Instance.Grammar.Productions.Single(prod_ => prod_.Name == prodName || 
                            (prod_.ProductionType == Grammar.ProductionType.Single || prod_.ProductionType == Grammar.ProductionType.CaptureSingle) && prod_.Name.Contains(prodName));

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
    
    
        static void MatcherMaps()
        {

        }
    }
}
