using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PeirceGen
{
    public class MatcherProduction
    {
        public string ClassName { get; set; }
        public string TypeName { get; set; }

        public List<MatcherCase> Cases { get; set; }

        public Grammar.Production GrammarType { get; set; }

        public List<string> RawCases { get; set; }

        public string InheritStr { get; set; }
        public List<MatcherProduction> InheritGroup { get; set; }

        public Grammar.Production DefaultProduction { get; set; }
        public Grammar.Case DefaultCase { get; set; }

        public string GetIncludes()
        {
            var allprods = new List<MatcherProduction>(this.InheritGroup);
            try
            {
                this.Cases.ForEach(c =>
                {
                    //Console.WriteLine(c.ClangName + " " + Peirce.Join(" ",c.Args, a=>a.ClassName));

                    allprods.AddRange((c.Args??new List<MatcherProduction>()).SelectMany(a => a.InheritGroup));
                });
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            allprods = allprods.Distinct().ToList();

            return Peirce.Join("", allprods, p_ => "\n#include \"" + p_.ClassName + ".h\"");
        }

        public Grammar.Case SearchForDecl(bool withInit)
        {
            var declareProd = ParsePeirce.Instance.Grammar.Productions.Single(p_=>p_.Name.ToLower().Contains("declare"));

            var withInitProd = declareProd.Cases.Single(c_ => c_.Productions.Exists(p__ => p__ == this.GrammarType));

            if (withInit)
            {
                return withInitProd;
            }
            else
            {
                var ret = declareProd.Cases.Single(c_ => c_.Productions.Count == 1 && c_.Productions[0] == withInitProd.Productions[0]);

                return ret;
            }
        }

        public Grammar.Case SearchForAssign()
        {
            return default(Grammar.Case);
        }

        public Grammar.Case SearchForIdent()
        {
            return SearchForDecl(false).Productions[0].Cases[0];
        }

        public static Grammar.Production FindProduction(MatcherProduction production, string query)
        {
            var gramprod = default(Grammar.Production);
            var toks = query.Split('.');
            try
            {

                gramprod = 
                    toks[0] == "$" ? production.GrammarType :
                    toks[0] == "DEFAULT" ? production.DefaultCase.Production : 
                    ParsePeirce.Instance.Grammar.Productions.Single(p_ => p_.Name == toks[0]);

                return gramprod;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public static Grammar.Case FindCase(MatcherProduction production, string query, List<MatcherProduction> args = null)
        {
            var toks = query.Split('.');

            var gramprod = toks[0] == "$" ?
                production.GrammarType : ParsePeirce.Instance.Grammar.Productions.Single(p_ => p_.Name == toks[0]);
            int result;
            int i = 0;
            var test = default(Grammar.Case);
            if (toks[1] == "?")
            {
                try
                {
                    return gramprod.Cases.Single(pcase => args.Count == pcase.Productions.Count && (i = 0) == 0// && string.IsNullOrEmpty(pcase.ValueType)
                        && args.TrueForAll(arg => arg.GrammarType == pcase.Productions[i++]));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else if(int.TryParse(toks[1], out result))
            {
                return gramprod.Cases[result];
            }
            else
            {
                try
                {
                    test = gramprod.Cases.First(pcase => pcase.Name.StartsWith(toks[1] + "_"));

                    return gramprod.Cases.Single(pcase => pcase.Name.StartsWith(toks[1] + "_") &&args.Count == pcase.Productions.Count &&(i=0)==0 
                        && args.TrueForAll(arg => arg.GrammarType == pcase.Productions[i++]));
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }


            return null;
        }

        public class MatcherCase
        {
            public string ClangName { get; set; }

            public string LocalName { get; set; }

            public Grammar.Production TargetGrammarProduction { get; set; }
            public Grammar.Case TargetGrammarCase { get; set; }

            public Func<MatcherProduction, string> BuildCallbackHandler { get; set; }
            public Func<MatcherProduction, string> BuildMatcher { get; set; }

            public List<MatcherProduction> Args;
            // public Func<MatcherProduction, string> 

            public override bool Equals(object obj)
            {
                var mc = (MatcherCase)obj;

                return this.ClangName == mc.ClangName && this.Args.Count == mc.Args.Count &&
                    Enumerable.Range(0, this.Args.Count).ToList().TrueForAll(i => this.Args[i] == mc.Args[i]);
            }
        }

       

        public static Func<MatcherCase, string, string> WrapIf = (pcase, body) =>
        {
            return @"if(" + pcase.LocalName + @"){"
+
body
+ @"
    }";
        };

        public static List<MatcherCase> BuildMatcherCaseFromRegister(MatcherProduction production, string raw, List<MatcherProduction> allProductions)
        {
            /*
             * 
		MemberExpr(tf::Vector3).normalized:$.UNOP
		--MemberExpr(tf::Vector3).lerp:+. !!NOT SUPPORTED YET
		OperatorCallExpr(tf::Vector3,tf::Vector3).*:$.MUL
             * */
            var retval = new List<MatcherCase>();
            try
            {

                var asttype = raw.Split('@')[0];
                var grammartype = raw.Split('@')[1];

                var asttoks = asttype.Split('.');
                var grammartoks = grammartype.Split('.');

                //Console.WriteLine(raw + " " + grammartype);

                var targetProd = MatcherProduction.FindProduction(production, grammartype);

                if (asttoks[0].Contains("CXXMemberCallExpr"))
                {
                    var args = asttoks[0].Substring(asttoks[0].IndexOf('(') + 1, asttoks[0].Length - asttoks[0].IndexOf('(') - 2).Split(',');

                    var numargs = args.Length;

                    var prodArgs = args.Select(a => allProductions.Single(p_ => p_.TypeName == a)).ToList();
                    var targetCase = MatcherProduction.FindCase(production, grammartype, prodArgs);

                    retval.Add(new MatcherCase()
                    {
                        Args = prodArgs,
                        TargetGrammarCase = targetCase,
                        TargetGrammarProduction = targetProd,

                        ClangName = "CXXMemberCallExpr",
                        LocalName = "cxxMemberCallExpr_",
                        BuildMatcher = (prod) =>
                        {
                            return "cxxMemberCallExpr().bind(\"CXXMemberCallExpr\")";
                        },
                        BuildCallbackHandler = (prod) =>
                        {

                            int i = 0, j = 0, k = 0,
                                l = 0, m = 0, n = 0, o = 0, x = 0, y = 0, z = 0, q = 0;
                            var prodexistpreds = string.Join("",
                                prodArgs.Select(a => "\n\targ_decay_exist_predicates[\"" + raw + a.TypeName + @"""] = [=](std::string typenm){
        if(false){return false;}" +
        Peirce.Join("", a.InheritGroup, a_ => "\n\t\telse if(typenm.find(\"" + a_.TypeName + "\") != string::npos){ return true; }")
        + @"
    };"));
                            return prodexistpreds + @"
    if(cxxMemberCallExpr_){
        auto decl_ = cxxMemberCallExpr_->getMethodDecl();
        if(auto dc = clang::dyn_cast<clang::NamedDecl>(decl_)){
            auto name = dc->getNameAsString();
            

            if(name.find(""" + asttoks[1] + @""") != string::npos){"
    +
    Peirce.Join("", prodArgs, p_ => l == 0 ? @"
                auto arg" + l++ + @" = cxxMemberCallExpr_->getImplicitObjectArgument();
                auto arg0str = ((clang::QualType)arg0->getType()).getAsString();
                " :
        @"
                auto arg" + l + @"=cxxMemberCallExpr_->getArg(" + l++ + @"-1);
                auto arg" + ++q + "str = ((clang::QualType)arg" + q + "->getType()).getAsString();\n") +
                Peirce.Join("", prodArgs, p_ => @"
                clang::Stmt* arg" + i++ + "stmt;\n")
    +
    @"            
                if (" + Peirce.Join(@" and 
                    ", prodArgs, p_ => "arg_decay_exist_predicates[\"" + raw + p_.TypeName + @"""](arg" + x++ + "str)") + @"){" +
     Peirce.Join("", prodArgs, p_ => {
         var retstr = @"
                    if(false){" + m + @";}
                    "
+ Peirce.Join(@"
                    ", p_.InheritGroup, p__ => "else if(arg" + y + @"str.find(""" + p__.TypeName + @""")!=string::npos){
                        "
+
p__.ClassName + @" arg" + m + @"m{this->context_,this->interp_};
                        arg" + m + @"m.setup();
                        arg" + m + @"m.visit(*arg" + m + @");
                        arg" + m + @"stmt = arg" + m + @"m.getChildExprStore();
                    }");

         m++;
         y++;
         return retstr;
     })
     + @"
                    if(" +
     Peirce.Join(@" and 
                        ", prodArgs, p_ => "arg" + n++ + "stmt")
     + @"){
                        interp_->mk" + targetCase.Name + "(cxxMemberCallExpr_," + Peirce.Join(",", prodArgs, p_ => "arg" + o++ + "stmt") + @");
                        this->childExprStore_ = (clang::Stmt*)cxxMemberCallExpr_;
                        return;
                    }
            
                }
            }
        }
    }
";
                        }
                    });
                }
                else if (asttoks[0].Contains("BinaryOperator"))
                {
                    var args = asttoks[0].Substring(asttoks[0].IndexOf('(') + 1, asttoks[0].Length - asttoks[0].IndexOf('(') -2).Split(',');

                    var numargs = args.Length;

                    var prodArgs = args.Select(a => allProductions.Single(p_ => p_.TypeName == a)).ToList();
                    var targetCase = MatcherProduction.FindCase(production, grammartype, prodArgs);


                    retval.Add(new MatcherCase()
                    {
                        Args = prodArgs,
                        TargetGrammarCase = targetCase,
                        TargetGrammarProduction = targetProd,

                        ClangName = "BinaryOperator",
                        LocalName = "binaryOperator_",
                        BuildMatcher = (prod) =>
                        {
                            return "binaryOperator().bind(\"BinaryOperator\")";
                        },
                        BuildCallbackHandler = (prod) =>
                        {
                            int i = 0, j = 0, k = 0,
                                l = 0, m = 0, n = 0, o = 0, x = 0, y = 0, z = 0;
                            var prodexistpreds = string.Join("",
                                prodArgs.Select(a => "\n\targ_decay_exist_predicates[\"" + raw + a.TypeName + @"""] = [=](std::string typenm){
    if(false){return false;}" +
        Peirce.Join("", a.InheritGroup, a_ => "\n\t\telse if(typenm.find(\"" + a_.TypeName + "\") != string::npos){ return true; }")
        + @"
    };"));

                            return prodexistpreds + @"
    if(binaryOperator_){
        auto bostr = binaryOperator_->getOpcodeStr().str();
        auto lhs = binaryOperator_->getLHS();
        auto rhs = binaryOperator_->getRHS();
        clang::Stmt* lhsstmt;
        clang::Stmt* rhsstmt;
            

        if(bostr.find(""" + asttoks[1] + @""") != string::npos){
            auto lhs = binaryOperator_->getLHS();
            auto lhsstr = ((clang::QualType)lhs->getType()).getAsString();
            auto rhs = binaryOperator_->getRHS();
            auto rhsstr = ((clang::QualType)rhs->getType()).getAsString();
            clang::Stmt* lhsresult = nullptr;
            clang::Stmt* rhsresult = nullptr;
            if(false){}
            "
         /*+
         Peirce.Join("\n", prodArgs, p_ => {
             var retstr =
             @"
            if(false){" + m + ";}" + Peirce.Join(@"
            ", p_.InheritGroup, p__ => @"
            else if(arg" + y + @"str.find(""" + p__.TypeName + @""") != string::npos){
            "
          +
          p__.ClassName + @" 
                arg" + m + @"m{this->context_,this->interp_};
                arg" + m + @"m.setup();
                arg" + m + @"m.visit(*arg" + m + @");
                arg" + m + @"stmt = arg" + m + @"m.getChildExprStore();
            }");
             y++;
             m++;
             return retstr;
         })*/
         +
        Peirce.Join(@"
            ", new List<MatcherProduction>( prodArgs[0].InheritGroup ), p_ => @"else if(lhsstr.find(""" + p_.TypeName + @""") != string::npos){
                " + p_.ClassName + @" lhsm{this->context_,this->interp_};
                lhsm.setup();
                lhsm.visit(*lhs);
                lhsresult = lhsm.getChildExprStore();
                            
            }")
        +
        @"
            if(false){}
            "
        +
        Peirce.Join("", new List<MatcherProduction>( prodArgs[1].InheritGroup), p_ => @"
            else if(rhsstr.find(""" + p_.TypeName + @""") != string::npos){
                " + p_.ClassName + @" rhsm{this->context_,this->interp_};
                rhsm.setup();
                rhsm.visit(*rhs);
                rhsresult = rhsm.getChildExprStore();
                            
            }")


     + @"
            if(lhsresult and rhsresult){
                interp_->mk" + targetCase.Name + @"(binaryOperator_,lhsresult, rhsresult);
                this->childExprStore_ = (clang::Stmt*)binaryOperator_;
                return;
            }
        }
    }
";
                        }
                    });
                }
                else if (asttoks[0].Contains("CXXOperatorCallExpr"))
                {
                    var args = asttoks[0].Substring(asttoks[0].IndexOf('(') + 1, asttoks[0].Length - asttoks[0].IndexOf('(') - 2).Split(',');

                    var numargs = args.Length;

                    var prodArgs = args.Select(a => allProductions.Single(p_ => p_.TypeName == a)).ToList();
                    var targetCase = MatcherProduction.FindCase(production, grammartype, prodArgs);


                    retval.Add(new MatcherCase()
                    {
                        Args = prodArgs,
                        TargetGrammarCase = targetCase,
                        TargetGrammarProduction = targetProd,

                        ClangName = "CXXOperatorCallExpr",
                        LocalName = "cxxOperatorCallExpr_",
                        BuildMatcher = (prod) =>
                        {
                            return "cxxOperatorCallExpr().bind(\"CXXOperatorCallExpr\")";
                        },
                        BuildCallbackHandler = (prod) =>
                        {
                            int i = 0, j = 0, k = 0,
                                l = 0, m = 0, n = 0, o = 0, x = 0, y = 0, z = 0, q = 0;
                            var prodexistpreds = string.Join("",
                                   prodArgs.Select(a => "\n\targ_decay_exist_predicates[\"" + raw + a.TypeName + @"""] = [=](std::string typenm){
        if(false){ return false;}" +
           Peirce.Join("", a.InheritGroup, a_ => "\n\t\telse if(typenm.find(\"" + a_.TypeName + "\") != string::npos){ return true; }")
           + @"
    };"));
                            return prodexistpreds + @"
    if(cxxOperatorCallExpr_){
        auto decl_ = cxxOperatorCallExpr_->getCalleeDecl();
        if(auto dc = clang::dyn_cast<clang::NamedDecl>(decl_)){
            auto name = dc->getNameAsString();

            if(name.find(""" + asttoks[1] + @""") != string::npos){"
 +
 Peirce.Join("", prodArgs, p_ => @"
                auto arg" + l + "=cxxOperatorCallExpr_->getArg(" + l++ + @");
                auto arg" + q + "str = ((clang::QualType)arg" + q++ + "->getType()).getAsString();\n") +
             Peirce.Join("", prodArgs, p_ => @"
                clang::Stmt* arg" + i++ + "stmt;\n")
 + @"              
                if (" + Peirce.Join(@" and 
                    ", prodArgs, p_ => "arg_decay_exist_predicates[\"" + raw + p_.TypeName + @"""](arg" + x++ + "str)") + @"){" +
     Peirce.Join("", prodArgs, p_ => {
         var retstr = @"
                    if(false){" + m + @";}
                    "
                   + Peirce.Join(@"
                    ", p_.InheritGroup, p__ =>
                   {
                       return "else if(arg" + y + @"str.find(""" + p__.TypeName + @""") != string::npos){
            "
           + @"
                        " +
           p__.ClassName + @" arg" + m + @"m{this->context_,this->interp_};
                        arg" + m + @"m.setup();
                        arg" + m + @"m.visit(*arg" + m + @");
                        arg" + m + @"stmt = arg" + m + @"m.getChildExprStore();
                    }";
                   });
         y++;
         m++;
         return retstr;
     })
        + @"
                    if(" +
        Peirce.Join(" and ", prodArgs, p_ => "arg" + n++ + "stmt")
        + @"){
                        interp_->mk" + targetCase.Name + "(cxxOperatorCallExpr_," + Peirce.Join(",", prodArgs, p_ => "arg" + o++ + "stmt") + @");
                        
                        this->childExprStore_ = (clang::Stmt*)cxxOperatorCallExpr_;
                        return;
                    }
            
                }
            }
        }
    }
";
                           }
                    });
                }
                else if (asttoks[0].Contains("CXXConstructExpr"))
                {
                    //
                    //CXXConstructExpr(tfScalar, tfScalar, tfScalar, tfScalar, tfScalar, tfScalar, tfScalar, tfScalar, tfScalar)
                    //:REALMATRIX3_LITERAL.?
                    //                var numargs = asttoks[0]
                    var args = asttoks[0].Substring(asttoks[0].IndexOf('(') + 1, asttoks[0].Length - asttoks[0].IndexOf('(') - 2).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    var numargs = args.Length;

                    var prodArgs = args.Select(a => allProductions.Single(p_ => p_.TypeName == a)).ToList();
                    var targetCase = MatcherProduction.FindCase(production, grammartype, prodArgs);

                    //var switchers =
                    retval.Add(new MatcherCase()
                    {
                        ClangName = "CXXConstructExpr",
                        LocalName = "cxxConstructExpr_",
                        Args = prodArgs,
                        TargetGrammarCase = targetCase,
                        TargetGrammarProduction = targetProd,
                        BuildMatcher = (prod) => "cxxConstructExpr().bind(\"CXXConstructExpr\")",

                        BuildCallbackHandler = (prod) =>
                         {

                             int i = 0, j = 0, k = 0,
                                 l = 0, m = 0, n = 0, o = 0, x = 0, y = 0, z = 0, q = 0;


                             var prodexistpreds = string.Join("",
                                 prodArgs.Select(a => "\n\targ_decay_exist_predicates[\"" + raw + a.TypeName + @"""] = [=](std::string typenm){
        if(false){return false;}
    " +
         Peirce.Join("", a.InheritGroup, a_ => "\n\t\telse if(typenm.find(\"" + a_.TypeName + "\") != string::npos){ return true; }")
         + @"
    };"));
                             var prodmatchpreds = string.Join("",
                                 prodArgs.Select(a => "\n\targ_decay_match_predicates[\"" + raw + ++j + @"""] = [=](std::string typenm){
    if(false){
        
    }
};"));
                        //first declare arg results
                        //then , for each arg
                        //
                        //if all children exists, declare a match in interp, or else throw a matcher wartning -invalid logic

                        return prodexistpreds + @"
    if(cxxConstructExpr_ and cxxConstructExpr_->getNumArgs() == " + prodArgs.Count + @"){" +
     Peirce.Join("", prodArgs, p_ => @"
        clang::Stmt* arg" + i++ + "stmt;\n")
     +
     Peirce.Join("", prodArgs, p_ => @"
        auto arg" + l + "=cxxConstructExpr_->getArg(" + l++ + @");
        auto arg" + q + "str = ((clang::QualType)arg" + q++ + "->getType()).getAsString();\n")
     + @"
        if(true " + (prodArgs.Count > 0 ? " and " : "") +Peirce.Join(@" and 
            ", prodArgs, p_ => "arg_decay_exist_predicates[\"" + raw + p_.TypeName + @"""](arg" + x++ + "str)") + @"){
            "+
         Peirce.Join("\n", prodArgs, p_ => {
             var retstr =
             @"
            if(false){" + m + ";}" + Peirce.Join(@"
            ", p_.InheritGroup, p__ => @"
            else if(arg" + y + @"str.find(""" + p__.TypeName + @""") != string::npos){
            "
          +
          p__.ClassName + @" 
                arg" + m + @"m{this->context_,this->interp_};
                arg" + m + @"m.setup();
                arg" + m + @"m.visit(*arg" + m + @");
                arg" + m + @"stmt = arg" + m + @"m.getChildExprStore();
            }");
             y++;
             m++;
             return retstr;
         })
         + @"
            if(true " + (prodArgs.Count > 0 ? " and " : "") +
             Peirce.Join(" and ", prodArgs, p_ => "arg" + n++ + "stmt")
             + @"){
                interp_->mk" + targetCase.Name + "(cxxConstructExpr_" + (prodArgs.Count > 0 ? " , " : "") + Peirce.Join(",", prodArgs, p_ => "arg" + o++ + "stmt") + @");
                this->childExprStore_ = (clang::Stmt*)cxxConstructExpr_;
                return;
            }
        }
    }";
                         }
                    });
                    retval.Add(new MatcherCase()
                    {
                        ClangName = "CXXTemporaryObjectExpr",
                        LocalName = "cxxTemporaryObjectExpr_",
                        Args = prodArgs,
                        TargetGrammarCase = targetCase,
                        TargetGrammarProduction = targetProd,
                        BuildMatcher = (prod) => "cxxTemporaryObjectExpr().bind(\"CXXTemporaryObjectExpr\")",

                        BuildCallbackHandler = (prod) =>
                        {


                            int i = 0, j = 0, k = 0,
                                l = 0, m = 0, n = 0, o = 0, x = 0, y = 0, z = 0, q = 0;


                            var prodexistpreds = string.Join("",
                                prodArgs.Select(a => "\n\targ_decay_exist_predicates[\"" + raw + a.TypeName + @"""] = [=](std::string typenm){
        if(false){return false;}" +
            Peirce.Join("", a.InheritGroup, a_ => "\n\t\telse if(typenm.find(\"" + a_.TypeName + "\") != string::npos){ return true; }")
            + @"
    };"));
                            var prodmatchpreds = string.Join("",
                                prodArgs.Select(a => "\n\targ_decay_match_predicates[\"" + raw + ++j + @"""] = [=](std::string typenm){
    if(false){
        
    }
};"));
                        //first declare arg results
                        //then , for each arg
                        //
                        //if all children exists, declare a match in interp, or else throw a matcher wartning -invalid logic

                        return prodexistpreds + @"
    if(cxxTemporaryObjectExpr_ and cxxTemporaryObjectExpr_->getNumArgs() == " + prodArgs.Count + @"){" +
     Peirce.Join("", prodArgs, p_ => @"
        clang::Stmt* arg" + i++ + "stmt;\n")
     +
     Peirce.Join("", prodArgs, p_ => @"
        auto arg" + l + "=cxxTemporaryObjectExpr_->getArg(" + l++ + @");
        auto arg" + q + "str = ((clang::QualType)arg" + q++ + "->getType()).getAsString();\n")
     + @"
        if(true " + (prodArgs.Count > 0 ? " and " : "") + Peirce.Join(@" and 
            ", prodArgs, p_ => "arg_decay_exist_predicates[\"" + raw + p_.TypeName + @"""](arg" + x++ + "str)") + @"){
            " +
         Peirce.Join("\n", prodArgs, p_ => {
             var retstr =
             @"
            if(false){" + m + ";}" + Peirce.Join(@"
            ", p_.InheritGroup, p__ => @"
            else if(arg" + y + @"str.find(""" + p__.TypeName + @""") != string::npos){
            "
          +
          p__.ClassName + @" 
                arg" + m + @"m{this->context_,this->interp_};
                arg" + m + @"m.setup();
                arg" + m + @"m.visit(*arg" + m + @");
                arg" + m + @"stmt = arg" + m + @"m.getChildExprStore();
            }");
             y++;
             m++;
             return retstr;
         })
         + @"
            if(true " + (prodArgs.Count > 0 ? " and " : "") +
             Peirce.Join(" and ", prodArgs, p_ => "arg" + n++ + "stmt")
             + @"){
                interp_->mk" + targetCase.Name + "(cxxTemporaryObjectExpr_" + (prodArgs.Count > 0 ? " , " : "") + Peirce.Join(",", prodArgs, p_ => "arg" + o++ + "stmt") + @");
                this->childExprStore_ = (clang::Stmt*)cxxTemporaryObjectExpr_;
                return;
            }
        }
    }";
                        }
                    });
                    /*retval.Add(new MatcherCase()
                    {
                         Args = new List<MatcherProduction>(),
                          BuildCallbackHandler = (prod) => {
                            return "";
                        },
                           BuildMatcher = (prod) =>
                           {
                               return "";
                           },
                            ClangName = "",
                             LocalName = "",
                              TargetGrammarCase = null,

                         
                    });*/
                }
                else if (asttoks[0].Contains("CXXBoolLiteralExpr"))
                {
                    retval.Add(
                        new MatcherCase()
                        {
                            ClangName = "CXXBoolLiteralExpr",
                            LocalName = "cxxBoolLiteralExpr_",
                            Args = new List<MatcherProduction>(),
                            BuildMatcher = (prod) => "cxxBoolLiteral().bind(\"CXXBoolLiteralExpr\")",
                            BuildCallbackHandler = (prod) =>
                            {
                                var prodexistpreds =
                                   Peirce.Join("",
                                   new List<MatcherProduction>() { prod },
                                   a => "\n\targ_decay_exist_predicates[\"cxxBoolLiteralExpr__" + a.TypeName + @"""] = [=](std::string typenm){
        if(false){return false;}" +
           Peirce.Join("", a.InheritGroup, a_ => "\n\t\telse if(typenm.find(\"" + a_.TypeName + "\") != string::npos){ return true; }")
           + @"
    };");
                                int y = 0, m = 0;
                                return prodexistpreds + @"
    if (cxxBoolLiteralExpr_)
    {
        interp_->mkBOOL_LIT(cxxBoolLiteralExpr_);
        this->childExprStore_ = (clang::Stmt*)cxxBoolLiteralExpr_;
        return;
    }";
                            }
                        }
                    );
                }
                else if (asttoks[0].Contains("IntegerLiteral"))
                {
                    retval.Add(
                        new MatcherCase()
                        {
                            ClangName = "IntegerLiteral",
                            LocalName = "integerLiteral_",
                            Args = new List<MatcherProduction>(),
                            BuildMatcher = (prod) => "integerLiteral().bind(\"IntegerLiteral\")",
                            BuildCallbackHandler = (prod) =>
                            {
                                var prodexistpreds =
                                   Peirce.Join("",
                                   new List<MatcherProduction>() { prod },
                                   a => "\n\targ_decay_exist_predicates[\"integerLiteral__" + a.TypeName + @"""] = [=](std::string typenm){
        if(false){return false;}" +
           Peirce.Join("", a.InheritGroup, a_ => "\n\t\telse if(typenm.find(\"" + a_.TypeName + "\") != string::npos){ return true; }")
           + @"
    };");
                                int y = 0, m = 0;
                                return prodexistpreds + @"
    if (integerLiteral_)
    {
        this->interp_->mkINT_LIT(integerLiteral_);
        this->childExprStore_ = (clang::Stmt*)integerLiteral_;
        if(this->childExprStore_){}
        else{
            std::cout<<""WARNING: Capture Escaping! Dump :\n"";
            integerLiteral_->dump();
        }
            return;
    }";
                            }
                        }
                    );
                }
                else if (asttoks[0].Contains("FloatingLiteral"))
                {
                    retval.Add(
                        new MatcherCase()
                        {
                            ClangName = "FloatingLiteral",
                            LocalName = "floatLiteral_",
                            Args = new List<MatcherProduction>(),
                            BuildMatcher = (prod) => "floatLiteral().bind(\"FloatingLiteral\")",
                            BuildCallbackHandler = (prod) =>
                            {
                                var prodexistpreds =
                                   Peirce.Join("",
                                   new List<MatcherProduction>() { prod },
                                   a => "\n\targ_decay_exist_predicates[\"floatLiteral_" + a.TypeName + @"""] = [=](std::string typenm){
        if(false){return false;}" +
           Peirce.Join("", a.InheritGroup, a_ => "\n\t\telse if(typenm.find(\"" + a_.TypeName + "\") != string::npos){ return true; }")
           + @"
    };");
                                int y = 0, m = 0;
                                return prodexistpreds + @"
    if (floatLiteral_)
    {
        this->interp_->mkINT_LIT(floatLiteral_);
        this->childExprStore_ = (clang::Stmt*)floatLiteral_;
        if(this->childExprStore_){}
        else{
            std::cout<<""WARNING: Capture Escaping! Dump :\n"";
            floatLiteral_->dump();
        }
            return;
    }";
                            }
                        }
                    );
                }
                else
                {
                    Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!");
                    Console.WriteLine("!!Did Not Recognize!!");
                    Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!");
                }

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            return retval;
        }


        public static List<MatcherCase> BuildDefaults(MatcherProduction production, Grammar.Case defaultcase = null)
        {
            var retval = new List<MatcherCase>()
            {
                new MatcherCase()
                    {
                        ClangName = "CXXConstructExpr",
                        LocalName = "cxxConstructExpr_",
                        Args = new List<MatcherProduction>(),
                        TargetGrammarCase = null,
                        TargetGrammarProduction = null,
                        BuildMatcher = (prod) => "cxxConstructExpr().bind(\"CXXConstructExpr\")",
                         BuildCallbackHandler = (prod) =>
                         {
                             return @"
    if(cxxConstructExpr_){
        auto decl_ = cxxConstructExpr_->getConstructor();
        if(decl_->isCopyOrMoveConstructor())
        {
            " + prod.ClassName + @" pm{context_, interp_};
            pm.setup();
            pm.visit(**cxxConstructExpr_->getArgs());
            this->childExprStore_ = pm.getChildExprStore();
            if(this->childExprStore_){}
    " + (defaultcase != null ?@"
            else{
                this->childExprStore_ = (clang::Stmt*)cxxBindTemporaryExpr_;
                interp_->mk" + defaultcase.Name + @"((clang::Stmt*)cxxBindTemporaryExpr_);
            }
        }
    }
" : @"
            else{
                std::cout<<""WARNING: Capture Escaping! Dump : \n"";
                cxxConstructExpr_->dump();
            }
            return;
        }
    }
");
                         }
                },
                new MatcherCase()
                {
                    ClangName = "MemberExpr",
                    LocalName = "memberExpr_",
                    Args = new List<MatcherProduction>(),
                     TargetGrammarCase = null,
                     TargetGrammarProduction = null,
                        BuildMatcher = (prod) => "memberExpr().bind(\"MemberExpr\")",
                     BuildCallbackHandler = (prod) =>
                     {
                         var prodexistpreds =
                            Peirce.Join("",
                            new List<MatcherProduction>(){prod},
                            a => "\n\targ_decay_exist_predicates[\"memberExpr_"  + a.TypeName + @"""] = [=](std::string typenm){
    if(false){return false;}" +
    Peirce.Join("", a.InheritGroup, a_ => "\n\t\telse if(typenm.find(\"" + a_.TypeName + "\") != string::npos){ return true; }")
    + @"
    };");
                         int y = 0, m = 0;
                         return prodexistpreds+@"
    if(memberExpr_){
        auto inner = memberExpr_->getBase();
        auto typestr = ((clang::QualType)inner->getType()).getAsString();
        if(false){}
        "+ Peirce.Join("\n\t\t", prod.InheritGroup, p__ => @"else if(typestr.find("""+p__.TypeName+@""") != string::npos){
            "
      +
      p__.ClassName + @" innerm{this->context_,this->interp_};
            innerm.setup();
            innerm.visit(*inner);
            this->childExprStore_ = (clang::Stmt*)innerm.getChildExprStore();
            return;
        }")+@"

    }
";
                     }
                },

                new MatcherCase()
                {
                    ClangName = "ImplicitCastExpr",
                    LocalName = "implicitCastExpr_",
                     Args = new List<MatcherProduction>(),
                      TargetGrammarCase = null,
                       TargetGrammarProduction = null,
                        BuildMatcher = (prod) => "implicitCastExpr().bind(\"ImplicitCastExpr\")",
                        BuildCallbackHandler = (prod) =>
                        {var prodexistpreds =
                            Peirce.Join("",
                            new List<MatcherProduction>(){prod},
                            a => "\n\targ_decay_exist_predicates[\"implicitCastExpr_"  + a.TypeName + @"""] = [=](std::string typenm){
        if(false){return false; }" +
    Peirce.Join("", a.InheritGroup, a_ => "\n\t\telse if(typenm.find(\"" + a_.TypeName + "\") != string::npos){ return true; }")
    + @"
    };");
                         int y = 0, m = 0;
                         return prodexistpreds+@"

    if (implicitCastExpr_)
    {
        auto inner = implicitCastExpr_->getSubExpr();
        auto typestr = inner->getType().getAsString();

        if(false){}
        "+ Peirce.Join("\n\t\t", prod.InheritGroup, p__ => @"else if(typestr.find("""+p__.TypeName+@""") != string::npos){
            "
      +
      p__.ClassName + @" innerm{this->context_,this->interp_};
            innerm.setup();
            innerm.visit(*inner);
            this->childExprStore_ = (clang::Stmt*)innerm.getChildExprStore();
            return;
        }") + (defaultcase != null ?@"
        else{
            this->childExprStore_ = (clang::Stmt*)implicitCastExpr_;
            interp_->mk" + defaultcase.Name + @"((clang::Stmt*)implicitCastExpr_);
            return;
        }
    }
" : @"
        else{
            std::cout<<""WARNING: Capture Escaping! Dump : \n"";
            implicitCastExpr_->dump();
        }
            return;

    }");
                        }
                },
                new MatcherCase()
                {
                     ClangName = "CXXBindTemporaryExpr",
                      LocalName = "cxxBindTemporaryExpr_",
                       Args = new List<MatcherProduction>(),
                        BuildMatcher = (prod) => "cxxBindTemporaryExpr().bind(\"CXXBindTemporaryExpr\")",
                         BuildCallbackHandler = (prod) =>
                         {var prodexistpreds =
                            Peirce.Join("",
                            new List<MatcherProduction>(){prod},
                            a => "\n\targ_decay_exist_predicates[\"cxxBindTemporaryExpr_"  + a.TypeName + @"""] = [=](std::string typenm){
        if(false){ return false; }" +
    Peirce.Join("", a.InheritGroup, a_ => "\n\t\telse if(typenm.find(\"" + a_.TypeName + "\") != string::npos){ return true; }")
    + @"
    };");
                         int y = 0, m = 0;
                         return prodexistpreds+@"
    if (cxxBindTemporaryExpr_)
    {
        " + prod.ClassName + @" exprMatcher{ context_, interp_};
        exprMatcher.setup();
        exprMatcher.visit(*cxxBindTemporaryExpr_->getSubExpr());
        this->childExprStore_ = (clang::Stmt*)exprMatcher.getChildExprStore();
        if(this->childExprStore_){}
    " + (defaultcase != null ?@"
        else{
            this->childExprStore_ = (clang::Stmt*)cxxBindTemporaryExpr_;
            interp_->mk" + defaultcase.Name + @"((clang::Stmt*)cxxBindTemporaryExpr_);
            return;
        }
    }
" : @"
        else{
            std::cout<<""WARNING: Capture Escaping! Dump : \n"";
            cxxBindTemporaryExpr_->dump();
        }
            return;

    }");
                         }


                },
                new MatcherCase()
                {
                    ClangName = "MaterializeTemporaryExpr",
                    LocalName = "materializeTemporaryExpr_",
                     Args = new List<MatcherProduction>(),
                      BuildMatcher = (prod) => "materializeTemporaryExpr().bind(\"MaterializeTemporaryExpr\")",
                      BuildCallbackHandler = (prod) =>
                      {var prodexistpreds =
                            Peirce.Join("",
                            new List<MatcherProduction>(){prod},
                            a => "\n\targ_decay_exist_predicates[\"materializeTemporaryExpr_"  + a.TypeName + @"""] = [=](std::string typenm){
        if(false){return false;}" +
    Peirce.Join("", a.InheritGroup, a_ => "\n\t\telse if(typenm.find(\"" + a_.TypeName + "\") != string::npos){ return true; }")
    + @"
    };");
                         int y = 0, m = 0;
                         return prodexistpreds+@"
    if (materializeTemporaryExpr_)
        {
            " + prod.ClassName + @" exprMatcher{ context_, interp_};
            exprMatcher.setup();
            exprMatcher.visit(*materializeTemporaryExpr_->GetTemporaryExpr());
            this->childExprStore_ = (clang::Stmt*)exprMatcher.getChildExprStore();
        
            if(this->childExprStore_){}
        " + (defaultcase != null ?@"
            else{
                this->childExprStore_ = (clang::Stmt*)materializeTemporaryExpr_;
                interp_->mk" + defaultcase.Name + @"((clang::Stmt*)materializeTemporaryExpr_);
                return;
            }
        }
" : @"
            else{
                std::cout<<""WARNING: Capture Escaping! Dump : \n"";
                materializeTemporaryExpr_->dump();
            }
            return;

    }");
                      }

                },
                new MatcherCase()
                {
                    ClangName = "ParenExpr",
                    LocalName = "parenExpr_",
                    Args = new List<MatcherProduction>(),
                     BuildMatcher = (prod) => "parenExpr().bind(\"ParenExpr\")",
                     BuildCallbackHandler = (prod) =>
                     {
                         var prodexistpreds =
                            Peirce.Join("",
                            new List<MatcherProduction>(){prod},
                            a => "\n\targ_decay_exist_predicates[\"parenExpr_"  + a.TypeName + @"""] = [=](std::string typenm){
        if(false){return false;}" +
    Peirce.Join("", a.InheritGroup, a_ => "\n\t\telse if(typenm.find(\"" + a_.TypeName + "\") != string::npos){ return true; }")
    + @"
    };");
                         int y = 0, m = 0;
                         return prodexistpreds+@"
    if (parenExpr_)
    {
        " + prod.ClassName + @" inner{ context_, interp_};
        inner.setup();
        inner.visit(*parenExpr_->getSubExpr());
        this->childExprStore_ = (clang::Stmt*)inner.getChildExprStore();
        if(this->childExprStore_){}
        else{
            std::cout<<""WARNING: Capture Escaping! Dump :\n"";
            parenExpr_->dump();
        }
        return;
    }";
                     },


                },
                new MatcherCase()
                {
                    ClangName = "ExprWithCleanups",
                    LocalName = "exprWithCleanups_",
                    Args = new List<MatcherProduction>(),
                     
                     BuildMatcher = (prod) => "exprWithCleanups().bind(\"ExprWithCleanups\")",
                     BuildCallbackHandler = (prod) =>
                     {
                        return @"
    if (exprWithCleanups_)
        {
            " + prod.ClassName + @" exprMatcher{ context_, interp_};
            exprMatcher.setup();
            exprMatcher.visit(*exprWithCleanups_->getSubExpr());
            this->childExprStore_ = (clang::Stmt*)exprMatcher.getChildExprStore();
        
            if(this->childExprStore_){}
        " + (defaultcase != null ?@"
            else{
                this->childExprStore_ = (clang::Stmt*)exprWithCleanups_;
                interp_->mk" + defaultcase.Name + @"((clang::Stmt*)exprWithCleanups_);
                return;
            }
        }
    " : @"
            else{
                std::cout<<""WARNING: Capture Escaping! Dump : \n"";
                exprWithCleanups_->dump();
            }

    }");               }

                },
                new MatcherCase()
                {
                     ClangName = "DeclRefExpr",
                      LocalName = "declRefExpr_",
                      Args = new List<MatcherProduction>(),
                       BuildMatcher = (prod) => "declRefExpr().bind(\"DeclRefExpr\")",
                       BuildCallbackHandler = (prod) =>
                       {
                           var refexpr = prod.GrammarType.Cases.Single(pcase => pcase.Name.Contains("VAR"));

                           return @"
    if(declRefExpr_){
        if(auto dc = clang::dyn_cast<clang::VarDecl>(declRefExpr_->getDecl())){
            interp_->mk" + refexpr.Name + @"(declRefExpr_, dc);
            this->childExprStore_ = (clang::Stmt*)declRefExpr_;
            return;

        }
    }
";
                       }
                },
                new MatcherCase()
                {
                    ClangName = "CallExpr",
                    LocalName = "callExpr_",
                    Args = new List<MatcherProduction>(),
                    BuildMatcher = (prod) => "callExpr().bind(\"CallExpr\")",
                    BuildCallbackHandler = (prod) =>
                    {
                        var callexpr = prod.GrammarType.Cases.Single(pcase => pcase.Name.Contains("CALL"));

                        return @"
    if(callExpr_){
        if(auto dc = clang::dyn_cast<clang::FunctionDecl>(callExpr_->getCalleeDecl())){
            interp_->mk" + callexpr.Name + @"(callExpr_);//, dc);
            this->childExprStore_ = (clang::Stmt*)callExpr_;
            return;
        }
    }
";
                    }
                }/*,
                new MatcherCase()
                {
                    ClangName = "ReturnStmt",
                    LocalName = "returnStmt_",
                    Args = new List<MatcherProduction>(),

                     BuildMatcher = (prod) => "exprWithCleanups().bind(\"ExprWithCleanups\")",
                     BuildCallbackHandler = (prod) =>
                     {
                        return @"
    if (returnStmt_)
        {
            " + prod.ClassName + @" exprMatcher{ context_, interp_};
            exprMatcher.setup();
            exprMatcher.visit(*returnStmt_->getSubExpr());
            this->childExprStore_ = (clang::Stmt*)exprMatcher.getChildExprStore();
        
            if(this->childExprStore_){}
        " + (defaultcase != null ?@"
            else{
                this->childExprStore_ = (clang::Stmt*)returnStmt_;
                interp_->mk" + defaultcase.Name + @"((clang::Stmt*)returnStmt_);
                return;
            }
        }
    " : @"
            else{
                std::cout<<""WARNING: Capture Escaping! Dump : \n"";
                returnStmt_->dump();
            }

    }");               }
                }*/
               
            };
            return retval;
        }

        public static MatcherCase GetIfCase()
        {
            return new MatcherCase()
            {
                ClangName = "IfStmt",
                LocalName = "ifStmt_",
                Args = new List<MatcherProduction>(),
                BuildMatcher = (prod) => "ifStmt().bind(\"ifStmt\"",
                BuildCallbackHandler = (prod) =>
                {
                    //var callexpr = prod.GrammarType.Cases.Single(pcase => pcase.Name.Contains("CALL"));

                    /*
                     * 
                     * bool 	hasInitStorage () const
                        True if this IfStmt has the storage for an init statement. More...

                    bool 	hasVarStorage () const
                        True if this IfStmt has storage for a variable declaration. More...

                    bool 	hasElseStorage () const
                        True if this IfStmt has storage for an else statement. More...

                    Expr * 	getCond ()

                    const Expr * 	getCond () const

                    void 	setCond (Expr *Cond)

                    Stmt * 	getThen ()

                    const Stmt * 	getThen () const

                    void 	setThen (Stmt *Then)

                    Stmt * 	getElse ()

                    const Stmt * 	getElse () const
                     * 
                     * */
                    var ifProd = ParsePeirce.Instance.Grammar.Productions.Single(p_ => p_.Name.StartsWith("IF"));
                    /*
                     * _IFCOND :=
IFTHEN +BOOL_EXPR +STMT | ~An If-Then Statement
IFTHENELSEIF +BOOL_EXPR +STMT +IFCOND | ~An If-Then-Else-If Statement 
IFTHENELSE +BOOL_EXPR +STMT +STMT ~An If-Then-Else Statement

                     * */
                    var ifthen = ifProd.Cases.Single(c => c.Productions.Count == 2);
                    var ifthenelseif = ifProd.Cases.Single(c => c.Productions.Count > 2 && c.Productions[2].Name.StartsWith("IF"));
                    var ifthenelse = ifProd.Cases.Single(c => c.Productions.Count > 2 && c.Productions[2].Name.StartsWith("STMT"));

                    return @"
    if(ifStmt_){
        auto cond = ifStmt_->getCond();
        auto thens = ifStmt_->getThen();

        ROSBooleanMatcher condm{ this->context_, this->interp_};
        condm.setup();
        condm.visit(*cond);

        if(!condm.getChildExprStore()){
            std::cout<<""Unable to parse If condition!!\n"";
            cond->dump();
            throw ""Broken Parse"";
        }

        ROSStatementMatcher thenm{ this->context_, this->interp_};
        thenm.setup();
        thenm.visit(*thens);

        if(!thenm.getChildExprStore()){
            std::cout<<""Unable to parse If block!!\n"";
            thens->dump();
            throw ""Broken Parse"";
        }

        if(ifStmt_->hasElseStorage()){
            auto elses = ifStmt_->getElse();

            ROSStatementMatcher elsem{ this->context_, this->interp_};
            elsem.setup();
            elsem.visit(*elses);
            if(!elsem.getChildExprStore(){
                std::cout<<""Unable to parse Then block!!\n"";
                elsem->dump();
                throw ""Broken Parse"";
            }
            if(auto dc = clang::dyn_cast<clang::IfStmt>(ifStmt_->getElse()){
                interp_->mk" + ifthenelseif.Name + @"(ifStmt_, condm.getChildExprStore(), thenm.getChildExprStore(), elsem.getChildExprStore());
                
                return;
            }
            else{
                interp_->mk" + ifthenelse.Name + @"(ifStmt_, condm.getChildExprStore(), thenm.getChildExprStore(), elsem.getChildExprStore());
            }
        }
        else{
            interp_->mk" + ifthen.Name + @"(ifStmt_, condm.getChildExprStore(), thenm.getChildExprStore());

        }
    }
";
                }
            };
        }
    }

    public class MatcherCase
    {
        public string RawString { get; set; }

        public string Name { get; set; }

        public Grammar.Production SpecificGrammarProduction { get; set; }
    }

    public static class ParseMatchers
    {

        static bool isComment(string line)
        {
            return line.Length == 0 || (line[0] == '-' && line[1] == '-');
        }

        static bool enterBlock(string line)
        {
            return line.Trim() == "{";

        }
        static bool exitBlock(string line)
        {
            return line.Trim() == "}";
        }
        static bool enterCapture(string line)
        {
            return line.Trim() == "Capture:";
        }

        static bool enterDiscard(string line)
        {
            return line.Trim() == "Discard:";
        }

        public static List<MatcherProduction> ParseRaw(List<string> rawMatchers)
        {
            List<MatcherProduction> retval = new List<MatcherProduction>();
            try
            {
                int globalstate = 0, prodstate = 0;
                int
                    prebase = 0,
                    enterbase = 1,
                    inbase = 2,
                    outbase = 3,
                    nonprod = 4,
                    enterprod = 5,
                    inprod = 6;
                int
                    unk = 0,
                    entercapture = 1,
                    atcapture = 2,
                    enterdiscard = 3,
                    atdiscard = 4;


                var currentprod = new MatcherProduction();

                foreach (var rawLine in rawMatchers)
                {
                    var line = rawLine.Trim();

                    if (isComment(line))
                        continue;

                    if (globalstate == prebase)
                    {
                        if (line.ToLower().Contains("base"))
                        {
                            globalstate = enterbase;
                        }
                    }
                    else if (globalstate == enterbase)
                    {
                        if (enterBlock(line))
                        {
                            globalstate = inbase;
                        }

                    }
                    else if (globalstate == inbase)
                    {
                        if (prodstate == unk)
                        {
                            if (enterCapture(line))
                            {
                                prodstate = entercapture;
                            }
                            else if (enterDiscard(line))
                            {
                                prodstate = enterdiscard;
                            }
                            else if (exitBlock(line))
                            {
                                globalstate = nonprod;
                            }
                        }
                        else if (prodstate == entercapture)
                        {
                            if (enterBlock(line))
                            {
                                prodstate = atcapture;
                            }
                        }
                        else if (prodstate == atcapture)
                        {
                            if (exitBlock(line))
                                prodstate = unk;
                        }
                        else if (prodstate == enterdiscard)
                        {
                            if (enterBlock(line))
                                prodstate = atdiscard;
                            else
                            {

                            }
                        }
                        else if (prodstate == atdiscard)
                        {
                            if (exitBlock(line))
                                prodstate = unk;
                            else
                            {

                            }
                        }
                    }
                    else if (globalstate == nonprod)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            //ROSTFScalarMatcher tfScalar REAL1_EXPR

                            var toks = line.Split(' ');

                            var cn = "";
                            var inherits = "";
                            if (toks[0].Contains("~"))
                            {
                                cn = toks[0].Split('~')[0];
                                inherits = toks[0].Split('~')[1];
                            }
                            else
                            {

                            }
                            //Console.WriteLine(line);
                            try
                            {
                                currentprod = new MatcherProduction()
                                {
                                    TypeName = toks[0].Contains("~") ? cn : toks[0],
                                    InheritStr = toks[0].Contains("~") ? inherits : null,
                                    ClassName = toks[1],
                                    GrammarType = ParsePeirce.Instance.Grammar.Productions.Single(p_ => p_.Name == toks[2]),
                                    Cases = new List<MatcherProduction.MatcherCase>(),
                                    InheritGroup = new List<MatcherProduction>(),
                                    RawCases = new List<string>()
                                    ,DefaultCase =
                                        toks.Count() > 3 ?
                                          ParsePeirce.Instance.Grammar
                                            .Productions.Single(p_ => p_.Name == toks[3].Split('.')[0])
                                            .Cases[int.Parse(toks[3].Split('.')[1])] : null
                                };
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine(ex.StackTrace);
                            }

                            retval.Add(currentprod);


                            if (toks.Count() > 3)
                            {
                                Console.WriteLine(line);
                                var defaultcase = toks[3].Split('.');
                                var defaultprod = ParsePeirce.Instance.Grammar.Productions.Single(p_ => p_.Name == defaultcase[0]);
                                currentprod.Cases.AddRange(MatcherProduction.BuildDefaults(currentprod, defaultprod.Cases[int.Parse(defaultcase[1])]));
                            }
                            else
                                currentprod.Cases.AddRange(MatcherProduction.BuildDefaults(currentprod));
                            globalstate = enterprod;
                            prodstate = unk;
                        }
                    }
                    else if(globalstate == enterprod)
                    {
                        if(enterBlock(line))
                        {
                            globalstate = inprod;
                        }
                    }
                    else if (globalstate == inprod)
                    {
                        if (prodstate == unk)
                        {
                            if (enterCapture(line))
                            {
                                prodstate = entercapture;
                            }
                            else if (enterDiscard(line))
                            {
                                prodstate = enterdiscard;
                            }
                            else if (exitBlock(line))
                            {
                                currentprod = null;
                                globalstate = nonprod;
                                prodstate = unk;
                            }
                        }
                        else if (prodstate == entercapture)
                        {
                            if (enterBlock(line))
                            {
                                prodstate = atcapture;
                            }
                        }
                        else if (prodstate == atcapture)
                        {
                            if (exitBlock(line))
                                prodstate = unk;
                            else if (!string.IsNullOrEmpty(line))
                            {
                                Console.WriteLine(line);
                                currentprod.RawCases.Add(line);
                                //currentprod.Cases.AddRange(MatcherProduction.BuildMatcherCaseFromRegister(currentprod, line));
                            }
                        }
                        else if (prodstate == enterdiscard)
                        {
                            if (enterBlock(line))
                                prodstate = atdiscard;
                            else
                            {

                            }
                        }
                        else if (prodstate == atdiscard)
                        {
                            if (exitBlock(line))
                                prodstate = unk;
                            else if (!string.IsNullOrEmpty(line))
                            {
                                //
                                Console.WriteLine(line);
                                currentprod.RawCases.Add(line);
                                //currentprod.Cases.AddRange(MatcherProduction.BuildMatcherCaseFromRegister(currentprod, line));
                            }
                        }
                    }
                }

                //propagate inheritance groups

                foreach (var prod in retval.Where(p_ => p_.InheritStr != null))
                {
                    var inherits = retval.Single(p_ => p_.TypeName == prod.InheritStr);

                    inherits.InheritGroup.Add(prod);
                    prod.InheritGroup = inherits.InheritGroup;
                }

                foreach (var prod in retval.Where(p_ => p_.InheritStr == null))
                {
                    prod.InheritGroup.Add(prod);

                    prod.InheritGroup = prod.InheritGroup.OrderByDescending(p => p.TypeName.Length).ToList();
                }

                foreach (var prod in retval)
                {
                    //   MatcherProduction.BuildDefaults(prod, )
                    var splCases = prod.Cases;
                    prod.Cases = new List<MatcherProduction.MatcherCase>();
                    prod.RawCases.ForEach(str =>
                    {
                        var matches = MatcherProduction.BuildMatcherCaseFromRegister(prod, str, retval);
                        prod.Cases.AddRange(matches);
                        retval.Where(p_ => p_.InheritStr == prod.TypeName).ToList().ForEach(p_ => p_.Cases.AddRange(matches));
                    });
                    prod.Cases = prod.Cases.OrderByDescending(x => x.ClangName)
                       // .ThenByDescending(x => x.Args.Count > 0)
                        .ThenByDescending(x => (x.Args.Count > 0 ? x.Args[0].TypeName.Length : 0)).ToList();//quick and dirty "temporary fix" 
                    splCases.AddRange(prod.Cases);
                    prod.Cases = splCases;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }


            ParsePeirce.Instance.MatcherProductions = retval;
            return retval;
        }
    }
}
