using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeirceGen.Generators
{
    public class GenMatcher //: GenBase
    {
        public MatcherProduction Production { get; set; }

        public string HeaderFile { get; protected set; }

        public string CppFile { get; protected set; }

        public GenMatcher(MatcherProduction prod)
        {
            this.Production = prod;

            GenHeader();
            GenCpp();
            if (!Directory.Exists(Directory.GetParent(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).FullName).FullName + @"\symlinkme\ros_matchers"))
                Directory.CreateDirectory(Directory.GetParent(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).FullName).FullName + @"\symlinkme\ros_matchers");
            System.IO.File.WriteAllText(this.GetHeaderLoc(), this.HeaderFile);
            System.IO.File.WriteAllText(this.GetCPPLoc(), this.CppFile);
        }

        public static string StatementHeaderFile;
        public static string StatementCppFile;
        public static string FunctionHeaderFile;
        public static string FunctionCppFile;

        public static void GenTopLevelMatchers()
        {
            GenMatcher.GenStatementHeader();
            GenMatcher.GenStatementCpp();
            //GenMatcher.GenFunctionHeader();
           // GenMatcher.GenFunctionCpp();
            if (!Directory.Exists(Directory.GetParent(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).FullName).FullName + @"\symlinkme\ros_matchers"))
                Directory.CreateDirectory(Directory.GetParent(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).FullName).FullName + @"\symlinkme\ros_matchers");
            System.IO.File.WriteAllText(GenMatcher.GetStatementHeaderLoc(), GenMatcher.StatementHeaderFile);
            System.IO.File.WriteAllText(GenMatcher.GetStatementCPPLoc(), GenMatcher.StatementCppFile);
            System.IO.File.WriteAllText(GenMatcher.GetFunctionHeaderLoc(), GenMatcher.FunctionHeaderFile);
            System.IO.File.WriteAllText(GenMatcher.GetFunctionCPPLoc(), GenMatcher.FunctionCppFile);
        }

        private static string GetFunctionCPPLoc()
        {
            return Directory.GetParent(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).FullName).FullName + @"\symlinkme\ros_matchers\ROSFunctionMatcher.cpp";
        }

        private static string GetFunctionHeaderLoc()
        {
            return Directory.GetParent(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).FullName).FullName + @"\symlinkme\ros_matchers\ROSFunctionMatcher.h";
        }

        private static string GetStatementCPPLoc()
        {
            return Directory.GetParent(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).FullName).FullName + @"\symlinkme\ros_matchers\ROSStatementMatcher.cpp";
        }

        private static string GetStatementHeaderLoc()
        {
            return Directory.GetParent(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).FullName).FullName + @"\symlinkme\ros_matchers\ROSStatementMatcher.h";
        }

        public static void GenStatementHeader()
        {
            GenMatcher.StatementHeaderFile = @"
#ifndef rosstatement
#define rosstatement
#include ""../BaseMatcher.h""
#include ""../Interpretation.h""

/*
See BaseMatcher.h for method details
Searches for relevant statements including physical calculations, generally inside a ""compound"" statement in a code block
*/

class ROSStatementMatcher : public BaseMatcher {
public:
    ROSStatementMatcher(clang::ASTContext* context, interp::Interpretation* interp) : BaseMatcher(context, interp) { }
        virtual void setup();
        virtual void run(const MatchFinder::MatchResult &Result);

};

#endif
";
        }

        public static void GenStatementCpp()
        {
            GenMatcher.StatementCppFile = @"
#include ""clang/ASTMatchers/ASTMatchFinder.h""
#include ""clang/ASTMatchers/ASTMatchers.h""
#include <vector>

#include ""../Interpretation.h""

#include ""ROSStatementMatcher.h""
"
+
Peirce.Join("\n",ParsePeirce.Instance.MatcherProductions,p_=>"#include \"" + p_.ClassName+".h\"")
+
@"

#include <string>


#include <iostream>

#include <memory>

#include ""../ASTToCoords.h""
/*
This manages all statements in Clang.
*/


void ROSStatementMatcher::setup(){

    StatementMatcher exprWithCleanups_ =
        exprWithCleanups(has(expr().bind(""UsefulExpr""))).bind(""ExprWithCleanupsDiscard"");//fluff node to discard

    StatementMatcher
        decl_ = declStmt().bind(""DeclStmt"");
    StatementMatcher
        assign_ = anyOf(
            cxxOperatorCallExpr(
                hasOverloadedOperatorName(""="")).bind(""Assign""),
            binaryOperator(
                hasOperatorName(""="")).bind(""Assign"")
        );

    StatementMatcher
        ifStmt_ = ifStmt().bind(""IfStmt"");

    StatementMatcher
        cmpdStmt_ = compoundStmt().bind(""CompoundStmt"");

    StatementMatcher
        expr_ = expr().bind(""ExprStmt"");

    StatementMatcher
        returnStmt_ = returnStmt().bind(""ReturnStmt"");

    StatementMatcher 
        whileStmt_ = whileStmt().bind(""WhileStmt"");

    localFinder_.addMatcher(decl_, this);
    localFinder_.addMatcher(assign_, this);
    localFinder_.addMatcher(expr_, this);
    localFinder_.addMatcher(ifStmt_,this);
    localFinder_.addMatcher(cmpdStmt_, this);
    localFinder_.addMatcher(returnStmt_, this);
    localFinder_.addMatcher(whileStmt_, this);
};

void ROSStatementMatcher::run(const MatchFinder::MatchResult &Result){

    this->childExprStore_ = nullptr;

    const auto declStmt = Result.Nodes.getNodeAs<clang::DeclStmt>(""DeclStmt"");

    const auto assignStmt = Result.Nodes.getNodeAs<clang::Expr>(""Assign"");

    const auto exprStmt = Result.Nodes.getNodeAs<clang::Expr>(""ExprStmt"");

    const auto exprWithCleanupsDiscard = Result.Nodes.getNodeAs<clang::ExprWithCleanups>(""ExprWithCleanupsDiscard"");

    const auto ifStmt_ = Result.Nodes.getNodeAs<clang::IfStmt>(""IfStmt"");

    const auto cmpdStmt_ = Result.Nodes.getNodeAs<clang::CompoundStmt>(""CompoundStmt"");

    const auto returnStmt_ = Result.Nodes.getNodeAs<clang::ReturnStmt>(""ReturnStmt"");

    const auto whileStmt_ = Result.Nodes.getNodeAs<clang::WhileStmt>(""WhileStmt"");

    /*
        if(declStmt)
            declStmt->dump();
        else if(assignStmt)
            assignStmt->dump();
        else if(exprStmt)
            exprStmt->dump();
        */
    /*
    if(whileStmt_){
        auto wcond = whileStmt_->getCond();
        auto wbody = whileStmt_->getBody();
        
        ROSBooleanMatcher condm{ this->context_, this->interp_};
        condm.setup();
        condm.visit(*wcond);

        if(!condm.getChildExprStore()){
            std::cout<<""Unable to parse If condition!!\n"";
            wcond->dump();
            throw ""Broken Parse"";
        }

        ROSStatementMatcher bodym{ this->context_, this->interp_};
        bodym.setup();
        bodym.visit(*wbody);

        if(!bodym.getChildExprStore()){
            std::cout<<""Unable to parse If block!!\n"";
            wbody->dump();
            throw ""Broken Parse"";
        }

        this->interp_->mkWHILE_BOOL_EXPR_STMT(whileStmt_, condm.getChildExprStore(), bodym.getChildExprStore());
        this->childExprStore_ = (clang::Stmt*)whileStmt_;
        return;

    }*/

    /*
    if(returnStmt_){
        auto _expr = returnStmt_->getRetValue();
        auto typestr = ((clang::QualType)_expr->getType()).getAsString();
        if(false){}
        "
                +
                Peirce.Join("", ParsePeirce.Instance.MatcherProductions.OrderByDescending(p_ => p_.TypeName.Length),
                    p_ =>
                    {
                        //var withInit = p_.SearchForDecl(true);
                        //var woInit = p_.SearchForDecl(t)

                        return @"
        else if (typestr.find(""" + p_.TypeName + @""") != string::npos){
            " + p_.ClassName + @" m{ this->context_, this->interp_};
            m.setup();
            m.visit(*_expr);
            if(m.getChildExprStore()){
                this->childExprStore_ = (clang::Stmt*)_expr;
            }
            return;
        }
            ";
                    })
                + @"
    }*/

    if(cmpdStmt_){
        std::vector<const clang::Stmt*> stmts;

        for(auto st : cmpdStmt_->body()){
            ROSStatementMatcher stmti{this->context_,this->interp_};
            stmti.setup();
            stmti.visit(*st);
            if(stmti.getChildExprStore()){
                stmts.push_back(stmti.getChildExprStore());
            }
        }
        this->interp_->mkCOMPOUND_STMT(cmpdStmt_, stmts);
        this->childExprStore_ = (clang::Stmt*)cmpdStmt_;
        return;
        
    }

    " + Peirce.Join("",new string[] { "hi" },(prod) =>
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
                var ifProd = ParsePeirce.Instance.Grammar.Productions.SingleOrDefault(p_ => p_.Name.StartsWith("IF"));

                if (ifProd == null)
                    return "";
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

        if(ifStmt_->getElse()){
            auto elses = ifStmt_->getElse();

            ROSStatementMatcher elsem{ this->context_, this->interp_};
            elsem.setup();
            elsem.visit(*elses);
            if(!elsem.getChildExprStore()){
                std::cout<<""Unable to parse Then block!!\n"";
                elses->dump();
                throw ""Broken Parse"";
            }
            if(auto dc = clang::dyn_cast<clang::IfStmt>(ifStmt_->getElse())){
                interp_->mk" + ifthenelseif.Name + @"(ifStmt_, condm.getChildExprStore(), thenm.getChildExprStore(), elsem.getChildExprStore());
                this->childExprStore_ = (clang::Stmt*)ifStmt_;
                return;
            }
            else{
                interp_->mk" + ifthenelse.Name + @"(ifStmt_, condm.getChildExprStore(), thenm.getChildExprStore(), elsem.getChildExprStore());
                this->childExprStore_ = (clang::Stmt*)ifStmt_;
                return;
            }
        }
        else{
            interp_->mk" + ifthen.Name + @"(ifStmt_, condm.getChildExprStore(), thenm.getChildExprStore());
            this->childExprStore_ = (clang::Stmt*)ifStmt_;
            return;

        }
    }
";
            }) + @"
    if (declStmt)
    {
        if (declStmt->isSingleDecl())
        {
            if (auto vd = clang::dyn_cast<clang::VarDecl>(declStmt->getSingleDecl()))
             {
                auto typestr = ((clang::QualType)vd->getType()).getAsString();
                if(false){}
"
                +
                Peirce.Join("",ParsePeirce.Instance.MatcherProductions.OrderByDescending(p_=>p_.TypeName.Length), 
                    p_=>
                    {
                       //var withInit = p_.SearchForDecl(true);
                        //var woInit = p_.SearchForDecl(t)

                        return @"
                else if (typestr.find(""" + p_.TypeName + @""") != string::npos){
                    interp_->mk" + p_.SearchForIdent().Production.Name + @"(vd);
                    if (vd->hasInit())
                    {
                        " + p_.ClassName + @" m{ this->context_, this->interp_};
                        m.setup();
                        m.visit((*vd->getInit()));
                        if (m.getChildExprStore())
                        {
                            interp_->mk" + p_.SearchForDecl(true).Name + @"(declStmt, vd, m.getChildExprStore());
                            this->childExprStore_ =  (clang::Stmt*)declStmt;
                            return;
                        }
                        else
                        {
                            interp_->mk" + p_.SearchForDecl(false).Name + @"(declStmt, vd);
                            this->childExprStore_ =  (clang::Stmt*)declStmt;
                            return;
                        }
                    }
                    else
                    {
                        interp_->mk" + p_.SearchForDecl(false).Name + @"(declStmt, vd);
                        this->childExprStore_ = (clang::Stmt*)declStmt;
                        return;
                    }
                }
            ";
                    })
                + @"
            }
        }
        else
        {
            bool anyfound = false;
            for (auto it = declStmt->decl_begin(); it != declStmt->decl_end(); it++)
            {
                if (auto vd = clang::dyn_cast<clang::VarDecl>(declStmt->getSingleDecl()))
                {
                    auto typestr = ((clang::QualType)vd->getType()).getAsString();
                    if(false){}
                " 
                +
                Peirce.Join("",ParsePeirce.Instance.MatcherProductions.OrderByDescending(p_ => p_.TypeName.Length), 
                    p_=>
                    {
                        return @"
                    else if(typestr.find(""" + p_.TypeName + @""") != string::npos){
                        interp_->mk" + p_.SearchForIdent().Production.Name + @"(vd);
                        if (vd->hasInit())
                        {
                            " + p_.ClassName + @" m{ this->context_, this->interp_};
                            m.setup();
                            m.visit((*vd->getInit()));
                            if (m.getChildExprStore())
                            {
                                interp_->mk" + p_.SearchForDecl(true).Name + @"(declStmt, vd, m.getChildExprStore());
                            }
                            else
                            {
                                interp_->mk" + p_.SearchForDecl(false).Name + @"(declStmt, vd);
                            }
                        }
                        else
                        {
                            interp_->mk" + p_.SearchForDecl(false).Name + @"(declStmt, vd);
                        }
                        anyfound = true;
                    }";
                    })
                + @"
                }
            }
            if (anyfound)
            {
                this->childExprStore_ = const_cast<clang::DeclStmt*>(declStmt);
                return;
            }
        }
    }
    else if (assignStmt)
    {
        //not implemented!!
    }
    else if (exprStmt)
    {
        auto typestr = ((clang::QualType)exprStmt->getType()).getAsString();
        "
                +
                Peirce.Join("", ParsePeirce.Instance.MatcherProductions.OrderByDescending(p_ => p_.TypeName.Length),
                    p_ =>
                    {
                        return @"
        if(typestr.find(""" + p_.TypeName + @""") != string::npos){
            " + p_.ClassName + @" m{ this->context_, this->interp_};
            m.setup();
            m.visit(*exprStmt);
            if (m.getChildExprStore())
                this->childExprStore_ = const_cast<clang::Stmt*>(m.getChildExprStore());
                return;
                
        }";
                    }) + @"
    }


    else if (exprWithCleanupsDiscard)
    {//matches fluff node to discard
        ROSStatementMatcher innerMatcher{ this->context_, this->interp_};
        innerMatcher.setup();
        innerMatcher.visit(*exprWithCleanupsDiscard->getSubExpr());
        if (innerMatcher.getChildExprStore())
            this->childExprStore_ = const_cast<clang::Stmt*>(innerMatcher.getChildExprStore());
            return;
    }
    else
    {
        //log error
    }

};

";
        }

        public static void GenFunctionHeader()
        {
            GenMatcher.FunctionHeaderFile = @"
#include "".. / BaseMatcher.h""
#include ""../Interpretation.h""
/*
See BaseMatcher.h for method details
Starting point entry for matching Clang AST. Searches for main method
*/
class ROSFunctionMatcher : public BaseMatcher {
public:
    ROSFunctionMatcher(clang::ASTContext* context, interp::Interpretation* interp) : BaseMatcher(context, interp) { }

        virtual void setup();
        virtual void run(const MatchFinder::MatchResult &Result);

};
";
        }

        public static void GenFunctionCpp()
        {
            GenMatcher.FunctionCppFile = @"

#include ""clang/ASTMatchers/ASTMatchFinder.h""
#include ""clang/ASTMatchers/ASTMatchers.h""
#include ""clang/AST/Decl.h""
#include <vector>
#include <iostream>

#include ""ROSFunctionMatcher.h""
#include ""ROSStatementMatcher.h""

#include ""../ASTToCoords.h""


using namespace clang::ast_matchers;

void ROSFunctionMatcher::setup()
    {
        DeclarationMatcher root =
            functionDecl(has(compoundStmt().bind(""MainCompoundStatement"")
            )).bind(""MainCandidate"");

        localFinder_.addMatcher(root, this);
    };

    /*
    This is a callback method that gets called when Clang matches on a pattern set up in the search method above.
    */
    void ROSFunctionMatcher::run(const MatchFinder::MatchResult &Result)
    {
        auto mainCompoundStatement = Result.Nodes.getNodeAs<clang::CompoundStmt>(""MainCompoundStatement"");
        auto mainCandidate = Result.Nodes.getNodeAs<clang::FunctionDecl>(""MainCandidate"");

        if (mainCandidate->isMain())
        {

            // stmts gets converted into a SEQ construct in lang.
            std::vector <const clang::Stmt*> stmts;

            //visit each statement in the main procedure
            for (auto it = mainCompoundStatement->body_begin(); it != mainCompoundStatement->body_end(); it++)
            {
                ROSStatementMatcher rootMatcher{ this->context_, this->interp_};
                rootMatcher.setup();
                rootMatcher.visit(**it);
                auto h = *it;

                if (rootMatcher.getChildExprStore())
                {
                    stmts.push_back(rootMatcher.getChildExprStore());
                }
                if (auto dc = clang::dyn_cast<clang::DeclStmt>(h))
            {
                auto decl = dc->getSingleDecl();
                if (auto ddc = clang::dyn_cast<clang::VarDecl>(decl))
                {
                    //  ddc->getType()->dump();
                }
            }
        }

        this->interp_->mkCOMPOUND_STMT(mainCompoundStatement, stmts);
        this->interp_->mkMAIN_STMT(mainCandidate, mainCompoundStatement);
        auto tud = clang::TranslationUnitDecl::castFromDeclContext(mainCandidate->getParent());
        std::vector <const clang::FunctionDecl*> globals;
        globals.push_back(mainCandidate);
        this->interp_->mkSEQ_GLOBALSTMT(tud, globals);
    }
};

";
        }


        public void GenCpp()
        {
            var file = @"
#include ""clang/ASTMatchers/ASTMatchFinder.h""
#include ""clang/ASTMatchers/ASTMatchers.h""
"
+
this.Production.GetIncludes()
+
@"


#include <string>
#include <unordered_map>
#include <functional>


void " + this.Production.ClassName + @"::setup(){"/* +
    Peirce.Join("\n",
        this.Production.Cases,
        pcase => "\n\t\tStatementMatcher " +
            pcase.LocalName + "=" + pcase.BuildMatcher(this.Production)
                + ";\n\t\tlocalFinder_.addMatcher(" + pcase.LocalName + ",this);")

*/
+ 
Peirce.Join("", new List<MatcherProduction>() { this.Production }.Select(cowboycode =>
{
    var distinctcases = this.Production.Cases.GroupBy(pcase => pcase.ClangName).Select(grp => grp.First()).ToList();



    // Peirce.Join("\n\t", distinctcases, pcase => "\n\tauto " + pcase.LocalName + " = Result.Nodes.getNodeAs<clang::" + pcase.ClangName + ">(\"" + pcase.ClangName + "\");");


    return Peirce.Join("\n\t", distinctcases, pcase => "\n\t\tStatementMatcher " +
            pcase.LocalName + "=" + pcase.BuildMatcher(this.Production)
                + ";\n\t\tlocalFinder_.addMatcher(" + pcase.LocalName + ",this);");
}).ToList(), p => p)
+
@"
};

void " + this.Production.ClassName + @"::run(const MatchFinder::MatchResult &Result){" +
Peirce.Join("", new List<MatcherProduction>() { this.Production }.Select(cowboycode =>
{
    var distinctcases = this.Production.Cases.GroupBy(pcase => pcase.ClangName).Select(grp => grp.First()).ToList();



   // Peirce.Join("\n\t", distinctcases, pcase => "\n\tauto " + pcase.LocalName + " = Result.Nodes.getNodeAs<clang::" + pcase.ClangName + ">(\"" + pcase.ClangName + "\");");


    return Peirce.Join("\n\t",distinctcases, pcase => "\n\tauto " + pcase.LocalName + " = Result.Nodes.getNodeAs<clang::" + pcase.ClangName + ">(\"" + pcase.ClangName + "\");");
}).ToList(), p => p)
+
@"
    std::unordered_map<std::string,std::function<bool(std::string)>> arg_decay_exist_predicates;
    std::unordered_map<std::string,std::function<std::string(std::string)>> arg_decay_match_predicates;
    this->childExprStore_ = nullptr;
"
+
Peirce.Join("", new List<MatcherProduction>() { this.Production }.Select(cowboycode =>
 {
     var distinctcases = this.Production.Cases.GroupBy(pcase => pcase.ClangName).Select(grp => grp.First()).ToList();



     Peirce.Join("\n\t", distinctcases, pcase => pcase.BuildCallbackHandler(this.Production));


     return Peirce.Join("\n\t", this.Production.Cases, pcase => pcase.BuildCallbackHandler(this.Production));
 }).ToList(), p => p)
            +""
       + @"


};

";
            this.CppFile = file;
        }

        public void GenHeader()
        {
            var file = @"
#ifndef " + this.Production.ClassName + @"guard
#define " + this.Production.ClassName + @"guard
#include ""../BaseMatcher.h""
#include ""../Interpretation.h""


class " + this.Production.ClassName + @" : public BaseMatcher {
public:
    " + this.Production.ClassName + @"(clang::ASTContext* context, interp::Interpretation* interp) : BaseMatcher(context, interp) { }
        virtual void setup();
        virtual void run(const MatchFinder::MatchResult &Result);

};

#endif";


    this.HeaderFile = file;
}

        public string GetCPPLoc()
        {
            return Directory.GetParent(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).FullName).FullName + @"\symlinkme\ros_matchers\" + Production.ClassName + ".cpp"; 
        }

        public string GetHeaderLoc()
        {
            return Directory.GetParent(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).FullName).FullName + @"\symlinkme\ros_matchers\" + Production.ClassName + ".h";
        }
    }
}
