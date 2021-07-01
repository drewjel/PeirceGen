using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;

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
            if (!Directory.Exists(PeirceGen.MonoConfigurationManager.Instance["MatcherPath"]))
                Directory.CreateDirectory(PeirceGen.MonoConfigurationManager.Instance["MatcherPath"]);
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
            GenMatcher.GenFunctionHeader();
            GenMatcher.GenFunctionCpp();
            if (!Directory.Exists(PeirceGen.MonoConfigurationManager.Instance["MatcherPath"] + "ros_matchers"))
                Directory.CreateDirectory(PeirceGen.MonoConfigurationManager.Instance["MatcherPath"] + "ros_matchers");
            System.IO.File.WriteAllText(GenMatcher.GetStatementHeaderLoc(), GenMatcher.StatementHeaderFile);
            System.IO.File.WriteAllText(GenMatcher.GetStatementCPPLoc(), GenMatcher.StatementCppFile);
            System.IO.File.WriteAllText(GenMatcher.GetFunctionHeaderLoc(), GenMatcher.FunctionHeaderFile);
            System.IO.File.WriteAllText(GenMatcher.GetFunctionCPPLoc(), GenMatcher.FunctionCppFile);
        }

        private static string GetFunctionCPPLoc()
        {
            return PeirceGen.MonoConfigurationManager.Instance["MatcherPath"] + "ROS1ProgramMatcher.cpp";
        }

        private static string GetFunctionHeaderLoc()
        {
            return PeirceGen.MonoConfigurationManager.Instance["MatcherPath"] + "ROS1ProgramMatcher.h";
        }

        private static string GetStatementCPPLoc()
        {
            return PeirceGen.MonoConfigurationManager.Instance["MatcherPath"] + "ROSStatementMatcher.cpp";
        }

        private static string GetStatementHeaderLoc()
        {
            return PeirceGen.MonoConfigurationManager.Instance["MatcherPath"] + "ROSStatementMatcher.h";
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

#include ""../maps/ASTToCoords.h""
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

    StatementMatcher
        forStmt_ = forStmt().bind(""ForStmt"");

    StatementMatcher
        tryStmt_ = cxxTryStmt().bind(""TryStmt"");

    StatementMatcher
        cxxMemberCallExpr_ = cxxMemberCallExpr().bind(""CXXMemberCallExpr"");

    //StatementMatcher
    //    functionDecl_ = functionDecl().bind(""FunctionDecl"");

    localFinder_.addMatcher(exprWithCleanups_,this);
    localFinder_.addMatcher(cxxMemberCallExpr_,this);
    localFinder_.addMatcher(decl_, this);
    localFinder_.addMatcher(assign_, this);
    localFinder_.addMatcher(expr_, this);
    localFinder_.addMatcher(ifStmt_,this);
    localFinder_.addMatcher(cmpdStmt_, this);
    localFinder_.addMatcher(returnStmt_, this);
    localFinder_.addMatcher(whileStmt_, this);
    localFinder_.addMatcher(forStmt_, this);
    localFinder_.addMatcher(tryStmt_, this);
    //localFinder_.addMatcher(functionDecl_, this);
    this->childExprStore_ = nullptr;
};

void ROSStatementMatcher::run(const MatchFinder::MatchResult &Result){
    if(this->childExprStore_ != nullptr){
        return;
    }

    const auto declStmt = Result.Nodes.getNodeAs<clang::DeclStmt>(""DeclStmt"");

    const auto assignStmt = Result.Nodes.getNodeAs<clang::Expr>(""Assign"");

    const auto exprStmt = Result.Nodes.getNodeAs<clang::Expr>(""ExprStmt"");

    const auto exprWithCleanupsDiscard = Result.Nodes.getNodeAs<clang::ExprWithCleanups>(""ExprWithCleanupsDiscard"");

    //const auto ifStmt_ = Result.Nodes.getNodeAs<clang::IfStmt>(""IfStmt"");

    const auto cmpdStmt_ = Result.Nodes.getNodeAs<clang::CompoundStmt>(""CompoundStmt"");

    //const auto returnStmt_ = Result.Nodes.getNodeAs<clang::ReturnStmt>(""ReturnStmt"");

    const auto whileStmt_ = Result.Nodes.getNodeAs<clang::WhileStmt>(""WhileStmt"");

    const auto forStmt_ = Result.Nodes.getNodeAs<clang::ForStmt>(""ForStmt"");

    const auto tryStmt_ = Result.Nodes.getNodeAs<clang::CXXTryStmt>(""TryStmt"");

    const auto cxxMemberCallExpr_ = Result.Nodes.getNodeAs<clang::CXXMemberCallExpr>(""CXXMemberCallExpr"");
    
    //const auto functionDecl_ = Result.Nodes.getNodeAs<clang::FunctionDecl>(""FunctionDecl"");

    if(whileStmt_){
        auto wcond = whileStmt_->getCond();
        auto wbody = whileStmt_->getBody();
        
        ROSBooleanMatcher condm{ this->context_, this->interp_};
        condm.setup();
        condm.visit(*wcond);

        if(!condm.getChildExprStore()){
            std::cout<<""Unable to parse While condition!!\n"";
            wcond->dump();
            throw ""Broken Parse"";
        }

        ROSStatementMatcher bodym{ this->context_, this->interp_};
        bodym.setup();
        bodym.visit(*wbody);

        if(!bodym.getChildExprStore()){
            std::cout<<""Unable to parse While block!!\n"";
            wbody->dump();
            throw ""Broken Parse"";
        }

        //this->interp_->mkWHILE_BOOL_EXPR_STMT(whileStmt_, condm.getChildExprStore(), bodym.getChildExprStore());
        interp_->buffer_operand(condm.getChildExprStore());
        interp_->buffer_operand(bodym.getChildExprStore());
        interp_->mkNode(""WHILE_STMT"", whileStmt_, false);
        
        this->childExprStore_ = (clang::Stmt*)whileStmt_;
        return;

    }

    if(forStmt_){
        auto wcond = forStmt_->getCond();
        auto wbody = forStmt_->getBody();
        
        ROSBooleanMatcher condm{ this->context_, this->interp_};
        condm.setup();
        condm.visit(*wcond);

        if(!condm.getChildExprStore()){
            std::cout<<""Unable to parse For condition!!\n"";
            wcond->dump();
            throw ""Broken Parse"";
        }

        ROSStatementMatcher bodym{ this->context_, this->interp_};
        bodym.setup();
        bodym.visit(*wbody);

        if(!bodym.getChildExprStore()){
            std::cout<<""Unable to parse For block!!\n"";
            wbody->dump();
            throw ""Broken Parse"";
        }

        //this->interp_->mkFOR_BOOL_EXPR_STMT(forStmt_, condm.getChildExprStore(), bodym.getChildExprStore());
        interp_->buffer_operand(condm.getChildExprStore());
        interp_->buffer_operand(bodym.getChildExprStore());
        interp_->mkNode(""FOR_STMT"",forStmt_,false); 
        this->childExprStore_ = (clang::Stmt*)forStmt_;
        return;
    }

    //if(functionDecl_){
        

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
        else if (typestr == """ + p_.TypeName + @""" or typestr == ""const " + p_.TypeName + @"""  or typestr == ""class " + p_.TypeName + @"""  or typestr == ""const class " + p_.TypeName + @"""/*typestr.find(""" + p_.TypeName + @""") != string::npos) != string::npos){
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
            else{
                //auto current = st;
                std::vector<std::vector<clang::Stmt*>> stack;
                std::vector<int> recptr;

                /*search up to depth 3 for now. this is not sound, but a sound approach may lead to other issues
                */
                for(auto c1 : st->children()){
                    ROSStatementMatcher i1{this->context_,this->interp_};
                    i1.setup();
                    i1.visit(*c1);
                    if(i1.getChildExprStore()){
                        stmts.push_back(i1.getChildExprStore());
                    }
                    else{
                        for(auto c2 : c1->children()){
                            ROSStatementMatcher i2{this->context_,this->interp_};
                            i2.setup();
                            i2.visit(*c2);
                            if(i2.getChildExprStore()){
                                stmts.push_back(i2.getChildExprStore());
                            }
                            else{
                                for(auto c3 : c2->children()){
                                    ROSStatementMatcher i3{this->context_,this->interp_};
                                    i3.setup();
                                    i3.visit(*c3);
                                    if(i3.getChildExprStore()){
                                        stmts.push_back(i3.getChildExprStore());
                                    }
                                    else{
                                        for(auto c4 : c3->children()){
                                            ROSStatementMatcher i4{this->context_,this->interp_};
                                            i4.setup();
                                            i4.visit(*c4);
                                            if(i4.getChildExprStore()){
                                                stmts.push_back(i4.getChildExprStore());
                                            }
                                            else{
                                                
                                            }
                                        } 
                                    }
                                }
                            }
                        }
                    }
                }

                /*
                restart:
                std::vector<clang::Stmt*> current_stack;
                for(auto c : current->children()) current_stack.push_back(c);
                stack.push_back(current_stack);
                recptr.push_back(0);
                while(!stack.empty()){
                    for(int i = 0; i<stack.back().size();i++){
                        if(recptr.back() > i) continue;
                        auto c = 
                            ROSStatementMatcher inner{this->context_,this->interp_};
                        inner.setup();
                        inner.visit(*c);
                        if(inner.getChildExprStore()){
                            stmts.push_back(inner.getChildExprStore());
                            recptr.back()++;
                        }
                        else if(c->child_begin() != c->child_end()){
                            current = c;
                            goto restart;
                        }
                    }
                }
                */
                    
                    
                
            }
        }
        //this->interp_->mkCOMPOUND_STMT(cmpdStmt_, stmts);
        if(stmts.size()>0){
            interp_->buffer_body(stmts);
            interp_->mkNode(""COMPOUND_STMT"", cmpdStmt_);
            this->childExprStore_ = (clang::Stmt*)cmpdStmt_;
        }
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

            interp_->buffer_operand(condm.getChildExprStore());
            interp_->buffer_operand(thenm.getChildExprStore());
            interp_->buffer_operand(elsem.getChildExprStore());
            interp_->mkNode(""IF_THEN_ELSE_STMT"",ifStmt_,false);
            
            //no need, redundant case
            /*if(auto dc = clang::dyn_cast<clang::IfStmt>(ifStmt_->getElse())){
                interp_->mk" + ifthenelseif.Name + @"(ifStmt_, condm.getChildExprStore(), thenm.getChildExprStore(), elsem.getChildExprStore());
                this->childExprStore_ = (clang::Stmt*)ifStmt_;
                return;
            }
            else{
                interp_->mk" + ifthenelse.Name + @"(ifStmt_, condm.getChildExprStore(), thenm.getChildExprStore(), elsem.getChildExprStore());
                this->childExprStore_ = (clang::Stmt*)ifStmt_;
                return;
            }*/
        }
        else{
            //interp_->mk" + ifthen.Name + @"(ifStmt_, condm.getChildExprStore(), thenm.getChildExprStore());
            interp_->buffer_operand(condm.getChildExprStore());
            interp_->buffer_operand(thenm.getChildExprStore());
            interp_->mkNode(""IF_THEN_ELSE_STMT"",ifStmt_,false);

            this->childExprStore_ = (clang::Stmt*)ifStmt_;
            return;

        }
    }
";
            }) + @"
    auto vec_str = std::string(""std::vector<"");
    if (declStmt)
    {
        if (declStmt->isSingleDecl())
        {
            if (auto vd = clang::dyn_cast<clang::VarDecl>(declStmt->getSingleDecl()))
             {
                auto typestr = ((clang::QualType)vd->getType()).getAsString();
                if(false){}
                else if(typestr.substr(0,vec_str.length())==vec_str){
                    //std::cout<<typestr.substr(vec_str.length(), typestr.length()-vec_str.length()-1)<<""\n"";
                    std::string param_type = typestr.substr(vec_str.length(), typestr.length()-vec_str.length()-1);
                    if(false){}                
"
                +
                Peirce.Join("", ParsePeirce.Instance.MatcherProductions.OrderByDescending(p_ => p_.TypeName.Length),
                    p_ =>
                    {
                        //var withInit = p_.SearchForDecl(true);
                        //var woInit = p_.SearchForDecl(t)
                        //if (p_.SearchForDecl(true) == null)
                        //    return "";
                        return @"
                        else if(" + GoNext.GetTypeMatchCondition("param_type", p_.TypeName) + @"){
                            
                            interp_->mkNode(""IDENT_LIST_" + p_.RefName + @""",vd, true);
                            if (vd->hasInit()){
                                //" + p_.ClassName + @" argm{this->context_,this->interp_};
                                //argm.setup();
                               // argm.visit(*vd->getInit());
                               // auto argstmt = argm.getChildExprStore();
                               //interp_->buffer_operand(argstmt);
                                interp_->buffer_operand(vd);
                                interp_->mkNode(""DECL_LIST_" + p_.RefName + @""",declStmt, false);
                                this->childExprStore_= (clang::Stmt*) declStmt;
                                return;
                            }
                            else{
                                interp_->buffer_operand(vd);
                                interp_->mkNode(""DECL_LIST_" + p_.RefName + @""",declStmt, false);
                                this->childExprStore_ = (clang::Stmt*) declStmt;
                                return;
                            }
                        }
                    ";
                    }) + @"
                }
"
                +
                Peirce.Join("",ParsePeirce.Instance.MatcherProductions.OrderByDescending(p_=>p_.TypeName.Length), 
                    p_=>
                    {
                        //var withInit = p_.SearchForDecl(true);
                        //var woInit = p_.SearchForDecl(t)
                        //if (p_.SearchForDecl(true) == null)
                        //    return "";


                        return @"
                else if (" + GoNext.GetTypeMatchCondition("typestr", p_.TypeName) + @"){
                    //interp_->mk" + @"(vd);
                    interp_->mkNode(""IDENT_" + p_.RefName + @""",vd, true);
                    if (vd->hasInit())
                    {
                        " + p_.ClassName + @" m{ this->context_, this->interp_};
                        m.setup();
                        m.visit((*vd->getInit()));
                        if (m.getChildExprStore())
                        {
                            //interp_->mk" + @"(declStmt, vd, m.getChildExprStore());
                            interp_->buffer_operand(vd);
                            interp_->buffer_operand(m.getChildExprStore());
                            interp_->mkNode(""DECL_INIT_" + p_.RefName + @""", declStmt);
                            this->childExprStore_ =  (clang::Stmt*)declStmt;
                            return;
                        }
                        else
                        {
                            //interp_->mk" + @"(declStmt, vd);
                            interp_->buffer_operand(vd);
                            interp_->mkNode(""DECL_" + p_.RefName + @""", declStmt);
                            this->childExprStore_ =  (clang::Stmt*)declStmt;
                            return;
                        }
                    }
                    else
                    {
                        //interp_->mk" +  @"(declStmt, vd);
                        interp_->buffer_operand(vd);
                        interp_->mkNode(""DECL_" + p_.RefName + @""", declStmt);
                        this->childExprStore_ = (clang::Stmt*)declStmt;
                        return;
                    }
                }
            ";
                    }
                    )
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
                        //if (p_.SearchForDecl(true) == null) return "";

                        return @"
                    else if(" + GoNext.GetTypeMatchCondition("typestr", p_.TypeName) + @"){
                        //interp_->mk" + @"(vd);
                        
                        interp_->mkNode(""IDENT_" + p_.RefName + @""",vd, true);
                        if (vd->hasInit())
                        {
                            " + p_.ClassName + @" m{ this->context_, this->interp_};
                            m.setup();
                            m.visit((*vd->getInit()));
                            if (m.getChildExprStore())
                            {
                                //interp_->mk" + @"(declStmt, vd, m.getChildExprStore());
                                interp_->buffer_operand(vd);
                                interp_->buffer_operand(m.getChildExprStore());
                                interp_->mkNode(""DECL_INIT_" + p_.RefName + @""", declStmt);
                                this->childExprStore_ =  (clang::Stmt*)declStmt;
                            }
                            else
                            {
                                //interp_->mk" + @"(declStmt, vd);
                                interp_->buffer_operand(vd);
                                interp_->mkNode(""DECL_" + p_.RefName + @""", declStmt);
                                this->childExprStore_ =  (clang::Stmt*)declStmt;
                            }
                        }
                        else
                        {
                            //interp_->mk" + @"(declStmt, vd);
                            interp_->buffer_operand(vd);
                            interp_->mkNode(""DECL_" + p_.RefName + @""", declStmt);
                            this->childExprStore_ =  (clang::Stmt*)declStmt;
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
    else if (exprWithCleanupsDiscard)
    {//matches fluff node to discard
        ROSStatementMatcher innerMatcher{ this->context_, this->interp_};
        innerMatcher.setup();
        innerMatcher.visit(*exprWithCleanupsDiscard->getSubExpr());
        if (innerMatcher.getChildExprStore()){
            this->childExprStore_ = const_cast<clang::Stmt*>(innerMatcher.getChildExprStore());
            return;
        }
    }
    else if (cxxMemberCallExpr_)
    {
        auto decl_ = cxxMemberCallExpr_->getMethodDecl();
        if(auto dc = clang::dyn_cast<clang::NamedDecl>(decl_)){
            auto name = dc->getNameAsString();
            auto obj= cxxMemberCallExpr_->getImplicitObjectArgument();
            auto objstr = ((clang::QualType)obj->getType()).getAsString();
            if(objstr.substr(0,vec_str.length())==vec_str and name.find(""push_back"") != string::npos){
                if(auto dc2 = clang::dyn_cast<clang::DeclRefExpr>(obj)){
                    auto objdecl = clang::dyn_cast<clang::VarDecl>(dc2->getDecl());
                    //interp_->buffer_link(objdecl);
                    //interp_->mkNode(""APPEND_LIST_R1"",cxxMemberCallExpr_,false);
                    std::string param_type = objstr.substr(vec_str.length(), objstr.length()-vec_str.length()-1);
                    if(false){}                
"
                +
                Peirce.Join("", ParsePeirce.Instance.MatcherProductions.OrderByDescending(p_ => p_.TypeName.Length),
                    p_ =>
                    {
                        //var withInit = p_.SearchForDecl(true);
                        //var woInit = p_.SearchForDecl(t)
                        //if (p_.SearchForDecl(true) == null)
                        //    return "";
                        return @"
                    else if(" + GoNext.GetTypeMatchCondition("param_type", p_.TypeName) + @"){
                        
                        auto arg_=cxxMemberCallExpr_->getArg(0);
                        " + p_.ClassName + @" argm{this->context_,this->interp_};
                        argm.setup();
                        argm.visit(*arg_);
                        auto argstmt = argm.getChildExprStore();
                        interp_->buffer_link(objdecl);
                        interp_->buffer_operand(argstmt);
                        interp_->mkNode(""APPEND_LIST_" + p_.RefName + @""",cxxMemberCallExpr_, false);
                        this->childExprStore_ = (clang::Stmt*)cxxMemberCallExpr_;
                        return;
                    }
                    ";
                    }) + @"
                }
                else {
                    std::cout<<""Warning : Not a DeclRefExpr"";
                }
            }
        }
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
        if(" + GoNext.GetTypeMatchCondition("typestr", p_.TypeName) + @"){
            " + p_.ClassName + @" m{ this->context_, this->interp_};
            m.setup();
            m.visit(*exprStmt);
            if (m.getChildExprStore()){
                this->childExprStore_ = const_cast<clang::Stmt*>(m.getChildExprStore());
                return;
            }
                
        }";
                    }) + @"
    }
    else if(tryStmt_){
        auto tryBlock = tryStmt_->getTryBlock();
        ROSStatementMatcher innerMatcher{ this->context_, this->interp_};
        innerMatcher.setup();
        innerMatcher.visit(*tryBlock);
        if (innerMatcher.getChildExprStore()){
            this->childExprStore_ = (clang::Stmt*)tryStmt_;//const_cast<clang::Stmt*>(innerMatcher.getChildExprStore());
            interp_->buffer_operand(innerMatcher.getChildExprStore());
            interp_->mkNode(""TRY_STMT"",tryStmt_);//,innerMatcher.getChildExprStore());
            return;
        }
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
#include ""../BaseMatcher.h""
#include ""../Interpretation.h""
/*
See BaseMatcher.h for method details
Starting point entry for matching Clang AST. Searches for main method
*/
class ROS1ProgramMatcher : public BaseMatcher {
public:
    ROS1ProgramMatcher(
        clang::ASTContext* context,
        interp::Interpretation* interp)
        : BaseMatcher(context, interp) { }

        virtual void setup();
        virtual void run(const MatchFinder::MatchResult &Result);
protected:
    
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

#include ""ROS1ProgramMatcher.h""
#include ""ROSStatementMatcher.h""

//#include ""../ASTToCoords.h""

int TUDCount;

using namespace clang::ast_matchers;

void ROS1ProgramMatcher::setup()
    {
        //valid without pointers!
        /*  DeclarationMatcher root =//isMain() <-- certain __important__ matchers like this are missing. find them.
              functionDecl(has(compoundStmt().bind(""MainCompoundStatement"")
              )).bind(""MainCandidate"");
      */
        DeclarationMatcher roott =
            translationUnitDecl().bind(""MainProg"");

        //localFinder_.addMatcher(root, this);
        localFinder_.addMatcher(roott, this);
    };

    /*
    This is a callback method that gets called when Clang matches on a pattern set up in the search method above.
    */
    void ROS1ProgramMatcher::run(const MatchFinder::MatchResult &Result)
    {
        //auto mainCompoundStatement = Result.Nodes.getNodeAs<clang::CompoundStmt>(""MainCompoundStatement"");
        //auto mainCandidate = Result.Nodes.getNodeAs<clang::FunctionDecl>(""MainCandidate"");

        auto tud = Result.Nodes.getNodeAs<clang::TranslationUnitDecl>(""MainProg"");

        auto srcs = this->interp_->getSources();
        /*
            std::cout<<""Sources:\n"";
            for(auto src:srcs)
            {
                std::cout<<src<<""\n"";
            }*/

        if (tud)
        {
            std::cout << ""TranslationUnitDeclCounter:"" << std::to_string(TUDCount++) << ""\n"";
            if (TUDCount > 1)
            {
                std::cout << ""WARNING : UPDATE  LOGIC TO HANDLE MULTIPLE TRANSLATION UNITS."";
                throw ""Bad Code!"";
            }
        }
        std::vector <const clang::FunctionDecl*> globals;
        if (tud)
        {
            //auto tud = clang::TranslationUnitDecl::castFromDeclContext(mainCandidate->getParent());
            auto & srcm = this->context_->getSourceManager();
            for (auto d : tud->decls())
            {

                if (auto fn = clang::dyn_cast<clang::FunctionDecl>(d))
            {
                auto loc = fn->getLocation();

                auto srcloc = srcm.getFileLoc(loc);
                auto locstr = srcloc.printToString(srcm);

                for (auto & src: srcs)
                {
                    if (locstr.find(src) != string::npos)
                    {
                        std::vector <const clang::Stmt*> stmts;
                        if (fn->isMain())
                        {
                            if (auto cmpd = clang::dyn_cast<clang::CompoundStmt>(fn->getBody())){

                                for (auto it = cmpd->body_begin(); it != cmpd->body_end(); it++)
                                {
                                    ROSStatementMatcher rootMatcher{ this->context_, this->interp_};
                                    rootMatcher.setup();
                                    //std::cout<<""dumping\n"";
                                    //(*it)->dump();
                                    //std::cout<<""dumped\n"";
                                    rootMatcher.visit(**it);
                                    if (rootMatcher.getChildExprStore())
                                    {
                                        //std::cout<<""child!!!\n"";
                                        //rootMatcher.getChildExprStore()->dump();
                                        stmts.push_back(rootMatcher.getChildExprStore());
                                    }
                                }
                                this->interp_->buffer_body(stmts);
                                this->interp_->mkNode(""COMPOUND_STMT"", cmpd, false);
                                this->interp_->buffer_body(cmpd);
                                this->interp_->mkNode(""FUNCTION_MAIN"", fn, false);
                                globals.push_back(fn);

                            }
                            else
                            {
                                std::cout << ""Warning : Unable to parse main function? \n"";
                                fn->getBody()->dump();
                            }
                        }
                        else{
                            auto retType = (clang::QualType)fn->getReturnType();
        
                            auto fbody = fn->getBody();
                            /*
                            auto typeDetector = [=](std::string typenm){
                                if(false){return false;}
                        " +
                             Peirce.Join("", ParsePeirce.Instance.MatcherProductions.OrderByDescending(p_ => p_.TypeName.Length), a_ =>
                                "\n\t\t\telse if(" + GoNext.GetTypeMatchCondition("typenm", a_.TypeName) + @"){ return true; }")
                             + @"
                                else { return false;}
                            };*/

                            ROSStatementMatcher bodym{ this->context_, this->interp_};
                            bodym.setup();
                            bodym.visit(*fbody);

                            if(!bodym.getChildExprStore()){
                                std::cout<<""No detected nodes in body of function\n"";
                                return;
                            }

                            std::vector<const clang::ParmVarDecl*> valid_params_;
                            auto params_ = fn->parameters();
                            if(params_.size() > 0){
                                for(auto param_ : params_){
                                    auto typestr = param_->getType().getAsString();
                                    if(false){}
                                 "
                                    +
                                    Peirce.Join("", ParsePeirce.Instance.MatcherProductions.OrderByDescending(p_ => p_.TypeName.Length),
                                        p_ =>
                                        {
                                            return @"
                                    else if(" + GoNext.GetTypeMatchCondition("typestr", p_.TypeName) + @"){
                                        //interp_->mkFunctionParam(""" + p_.RefName + @""", param_);

                                        if(auto dc = clang::dyn_cast<clang::ParmVarDecl>(param_)){
                                            interp_->mkNode(""FUNCTION_PARAM"", param_,false);
                                            valid_params_.push_back(const_cast<clang::ParmVarDecl*>(param_));
                                        }
                                        else
                                        {
                                            std::cout << ""Warning : Param is not a ParmVarDecl\n"";
                                            param_->dump();
                                        }
                                        valid_params_.push_back(param_);
                                    }";
                                        }) + @"
                                }
                            }
                            bool hasReturn = false;
                            auto nodePrefix = std::string("""");
                            auto typenm = retType.getAsString();
                            if(false){}
                        " +
                             Peirce.Join("", ParsePeirce.Instance.MatcherProductions.OrderByDescending(p_ => p_.TypeName.Length), a_ =>
                                "\n\t\t\t\t\t\t\telse if(" + GoNext.GetTypeMatchCondition("typenm", a_.TypeName) + @"){ hasReturn = true; nodePrefix = """ + a_.RefName + @"""; }")
                             + @"
                            else {}
        

                            if(valid_params_.size()>0){
                                interp_->buffer_operands(valid_params_);
            
                            }
                            interp_->buffer_body(bodym.getChildExprStore());
                            if(hasReturn){
                                interp_->mkFunctionWithReturn(nodePrefix, fn);
                            }
                            else{
                                interp_->mkFunction(fn);
                            }
                            globals.push_back(fn);
                                    
                        }
                    }
                }
            }
        }

        //this->interp_->mkSEQ_GLOBALSTMT(tud, globals);
        this->interp_->buffer_body(globals);
        this->interp_->mkNode(""COMPOUND_GLOBAL"", tud, false, true);
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
    this->childExprStore_ = nullptr;
};

void " + this.Production.ClassName + @"::run(const MatchFinder::MatchResult &Result){
    if(this->childExprStore_ != nullptr){
        return;
    }" +
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
            return PeirceGen.MonoConfigurationManager.Instance["MatcherPath"] + Production.ClassName + ".cpp";
        }

        public string GetHeaderLoc()
        {
            return PeirceGen.MonoConfigurationManager.Instance["MatcherPath"] + Production.ClassName + ".h";
        }
    }
}
