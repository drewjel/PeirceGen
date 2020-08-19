using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using System.Configuration;

namespace PeirceGen
{

    public partial class Grammar
    {
        //public string 

        public class Command
        {
            public string Production { get; set; }
            public string Case { get; set; }

            public string NameSuffix { get; set; }

            public string ToLeanConstructor() { return this.Production + '.' + this.Case; }
        }

        public List<Production> Productions = new List<Production>();

        public static Dictionary<char, ProductionType> TokenToProductionTypeMap = new Dictionary<char, ProductionType>()
        {
            { '+', ProductionType.Capture },
            { '_', ProductionType.Throw },
            { '=', ProductionType.Passthrough },
            { '*', ProductionType.Array },
            { '1', ProductionType.Single },
            { 'i', ProductionType.CaptureSingle },
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
            Hidden,
            Single,
            CaptureSingle,
            Value
        }

        public enum CaseType
        {
           // Value,
            Op,
            Pure,
            Passthrough,
            Array,
            ArrayOp,
            Inherits,
            Hidden,
            Ident
        }


        public static CaseType TokenToCaseTypeMap(List<string> toks)
        {
            return
                toks.Count > 1 && toks[1][0] == '.' ? CaseType.Hidden :
                toks.Count > 0 && toks[0][0] == '>' ? CaseType.Inherits :
                toks.Count > 0 && toks[0][0] == '*' ? CaseType.Array :
                toks.Count > 1 && toks[1][0] == '*' ? CaseType.ArrayOp :
                toks[0] == "IDENT" ? CaseType.Ident :
                //toks[0].Contains("VALUE") ? CaseType.Value :
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


        public class ValueContainer
        {
            public string ValueType { get; set; }
            public int ValueCount { get; set; }

            public string ValueDefault { get; set; }
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

            public Production GetTopPassthrough()
            {
                var current = this;

                while (current.Passthrough != null)
                    current = current.Passthrough;
                return current;
            }

            public ValueContainer ValueContainer { get; set; }

            public bool HasValueContainer()
            {
                return this.GetPriorityValueContainer() is ValueContainer;
            }

            public bool InheritsContainer()
            {
                return (this.Passthrough is Grammar.Production && this.Passthrough.ValueContainer is ValueContainer) ||
                   (this.Inherits is Grammar.Production && this.Inherits.ValueContainer is ValueContainer);
            }

            public ValueContainer GetPriorityValueContainer()
            {
                return
                    this.Passthrough is Grammar.Production && this.Passthrough.ValueContainer is ValueContainer ? this.Passthrough.ValueContainer :
                    this.Inherits is Grammar.Production && this.Inherits.ValueContainer is ValueContainer ? this.Inherits.ValueContainer :
                    this.ValueContainer is ValueContainer ? this.ValueContainer :
                    default(ValueContainer);
            }


        }

        public class Case
        {
            public CaseType CaseType;
            public string Name { get; set; }

            public string Description { get; set; }

            public List<Production> Productions = new List<Production>();
            public List<string> ProductionRefs = new List<string>();

            public Func<Production, string> CoordsToString { get; set; }

            public Func<Production, Space, Space.SpaceObject, string> InterpTranslation { get; set; }

            public Command Command { get; set; }

            public bool IsTranslationDeclare { get; set; }
            public bool IsVarDeclare { get; set; }
            public bool IsFuncDeclare { get; set; }

            public bool LinkSpace { get; set; }

            public Production Production;

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
                this.CoordsToString = (prod) => { return @"(this->getIndex() > 0 ? ""INDEX""+std::to_string(this->getIndex())+""."":"""")+" + "\"" + (prod.Passthrough != null ? prod.Passthrough.Name : prod.Name).Replace("_", ".") + "\" + " + retval; };
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
                this.InterpTranslation = (prod, sp, spobj) =>
                {
                    int i = 0;
                    var retval = @"
            auto case_coords = dynamic_cast<coords::" + (prod.Passthrough != null ? prod.Passthrough.Name : prod.Name) + @"*>(this->coords_);";
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

                //var parsed = Regex.Match(spl[0], @"(?:(\d*)([^0-9]*))*");

                this.InterpTranslation = (prod, sp, spobj) =>
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
                                            spl[0] == "B" ? "+ \"(" + spl[1][0] + "(\"+this->operand_1->coords_->toString()+\"))" + spl[1][1] + "(" + spl[1][2] + "(\"+this->operand_2->coords_->toString()+\"))\";" :
                                            spl[0] == "A" ?
                                                string.Join(" ", this.Productions.Select(p_ => "\" (" + "\" + operand_" + ++i + "->coords_->toString() + \") + \"")) + " + \"" + spl[1] + "\"" :
                                                "\"" + spl[1] + "\" " + string.Join(" ", this.Productions.Select(p_ => "+ \"(" + "\" + operand_" + ++i + "->coords_->toString() + \")\""))) + @";
            //return retval;
    ";
                    }
                    else
                    {
                        retval = @"
            auto case_coords = dynamic_cast<coords::" + (prod.Passthrough != null ? prod.Passthrough.Name : prod.Name) + @"*>(this->coords_);
            retval += ""def " + @""" + case_coords->toString() + "" : " + sp.Prefix + spobj.Name + spl[2] + @" "" +  "" := ""
             + " + (hasDefault ? "\" " + spl[1][0] + "(" + defaultStr + " (Eval" + sp.Prefix + "SpaceExpression0 sp)) \"" :
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
                    else if (prod.Command is Grammar.Command)
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
        /*
        public enum FieldType
        {
            Name = 1,
            Dimension = 2
        }*/
        public enum DimensionType_
        {
            ANY,
            Fixed
        }

        public DimensionType_ DimensionType { get; set; }

        public int FixedDimension { get; set; }

        public string Prefix { get; set; }

        public SpaceCategory Inherits { get; set; }


        public bool IsDerived { get; set; }


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
            foreach (var sp in curSpaces)
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

            foreach (var child in allSpaces.Where(sp_ => sp_.Inherits == sp.Category))
            {
                curLevel.Add(child, child.Category.Objects.Single(ob_ => ob_ == obj));

                var next = RetrieveInheritedObjects(child, obj, allSpaces);

                next.Keys.ToList().ForEach(key => curLevel[key] = next[key]);
            }

            return curLevel;
        }
        /*
        public class SpaceInstance
        {
            public string TypeName { get; set; }
            public string InstanceName { get; set; }
            public List<string> FieldValues = new List<string>();
        }*/
    }
}
