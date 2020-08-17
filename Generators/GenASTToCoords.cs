using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeirceGen.Generators
{
    public class GenASTToCoords : GenBase
    {

        public override string GetCPPLoc()
        {
            return "/peirce/PeirceGen/ASTToCoords.cpp";
        }

        public override string GetHeaderLoc()
        {
            return "/peirce/PeirceGen/ASTToCoords.h";
        }

        public override void GenCpp()
        {
            var header = @"
#include ""ASTToCoords.h""
#include <g3log/g3log.hpp>

#include <iostream>
#include <exception>
#include <memory>
#include <string>
#include <memory>

#include ""llvm/Support/Casting.h""
/*
Create Coords object for given AST node and update AST-to_Coords
mappings. Currently this means just the ast2coords unorderedmaps,
one for Clang AST objects inheriting from Stmt, and one for Clang
AST objects inheriting from Decl. We maintain both forward and
backwards maps. See AST.h for the translations.
*/
using namespace ast2coords;

void ASTToCoords::setASTState(coords::Coords* coords, clang::Stmt* stmt, clang::ASTContext* c)
{
    auto range = stmt->getSourceRange();
    auto begin = c->getFullLoc(range.getBegin());
    auto end = c->getFullLoc(range.getEnd());

    coords->state_ = new coords::ASTState(
        """",
        """",
        """",
        (clang::dyn_cast<clang::DeclRefExpr>(stmt)) ? (clang::dyn_cast<clang::DeclRefExpr>(stmt))->getDecl()->getNameAsString() : """",
        begin.getSpellingLineNumber(),
        begin.getSpellingColumnNumber(),
        end.getSpellingLineNumber(),
        end.getSpellingColumnNumber()
    );
    /*
    coords->state_.file_id_ = new std::string("""");
    coords->state_.file_name_ = """";
    coords->state_.file_path_ = """";

    coords->state_.name_ = 
        ((clang::DeclRefExpr*) stmt) ? ((clang::DeclRefExpr*) stmt)->getDecl()->getNameAsString() : """";


    coords->state_.begin_line_no_ = begin.getSpellingLineNumber();
    coords->state_.begin_col_no_ = begin.getSpellingColumnNumber();
    coords->state_.end_line_no_ = end.getSpellingLineNumber();
    coords->state_.end_col_no_ = end.getSpellingColumnNumber();
    */
}

void ASTToCoords::setASTState(coords::Coords* coords, clang::Decl* decl, clang::ASTContext* c)
{
    auto range = decl->getSourceRange();
    auto begin = c->getFullLoc(range.getBegin());
    auto end = c->getFullLoc(range.getEnd());

    coords->state_ = new coords::ASTState(
        """",
        """",
        """",
        (clang::dyn_cast<clang::NamedDecl>(decl)) ? (clang::dyn_cast<clang::NamedDecl>(decl))->getNameAsString() : """",
        begin.getSpellingLineNumber(),
        begin.getSpellingColumnNumber(),
        end.getSpellingLineNumber(),
        end.getSpellingColumnNumber()
    );
    /*
    coords->state_.file_id_ = """";
    coords->state_.file_name_ = """";
    coords->state_.file_path_ = """";

    coords->state_.name_ = ((clang::NamedDecl*) decl) ? ((clang::NamedDecl*) decl)->getNameAsString() : """";

    coords->state_.begin_line_no_ = begin.getSpellingLineNumber();
    coords->state_.begin_col_no_ = begin.getSpellingColumnNumber();
    coords->state_.end_line_no_ = end.getSpellingLineNumber();
    coords->state_.end_col_no_ = end.getSpellingColumnNumber();
    */
}

ASTToCoords::ASTToCoords() {
   this->stmt_coords = new std::unordered_map<const clang::Stmt*, coords::Coords*>();
   this->decl_coords = new std::unordered_map<const clang::Decl*, coords::Coords*>();
   this->coords_stmt = new std::unordered_map<coords::Coords*,const clang::Stmt*>();
   this->coords_decl = new std::unordered_map<coords::Coords*,const clang::Decl*>();
}

";

            var file = header;
            foreach(var prod in ParsePeirce.Instance.Grammar.Productions)
            {
                foreach(var pcase in prod.Cases)
                {
                    if (pcase.CaseType == Grammar.CaseType.Passthrough || pcase.CaseType == Grammar.CaseType.Inherits)
                        continue;

                    int i = 0, j = 0;

                    if (prod.ProductionType == Grammar.ProductionType.Single || prod.ProductionType == Grammar.ProductionType.CaptureSingle)
                    {
                        var mkStr = @"
coords::" + prod.Name + @"* ASTToCoords::mk" + prod.Name + "(const ast::" + prod.Name + "* ast, clang::ASTContext* c" + (pcase.Productions.Count > 0 ? "," +
            string.Join(",", pcase.Productions.Select(p_ => "coords::" + p_.Name + "* operand" + ++i)) : "") +
                            (prod.HasValueContainer() ? "," +
                        Peirce.Join(",", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => "std::shared_ptr<" + prod.GetPriorityValueContainer().ValueType + "> value" + v) : "") + @"){
    coords::" + prod.Name + @"* coord = new coords::" + prod.Name + @"(" + string.Join(",", pcase.Productions.Select(p_ => "operand" + ++j)) 
    + (prod.HasValueContainer() ? (pcase.Productions.Count > 0 ? "," : "") +
                        Peirce.Join(",", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => " value" + v) : "") 
+ @");
    ast::" + prod.Name + "* unconst_ast = const_cast<ast::" + prod.Name + @"*>(ast);" +
(prod.HasValueContainer() ? Peirce.Join("", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => "\n\t//coord->setValue(value" + v + "," + v + ");") : "")
+ @"


    if (auto dc = clang::dyn_cast<clang::NamedDecl>(unconst_ast)){
        clang::NamedDecl* unconst_dc = const_cast<clang::NamedDecl*>(dc);
        setASTState(coord, unconst_dc, c);
        overrideDecl2Coords(dc, coord);     // Use Clang canonical addresses? 
        overrideCoords2Decl(coord, dc);     // Use Clang canonical addresses?
    }
    /*if (auto dc = clang::dyn_cast<clang::Stmt>(unconst_ast)){
        clang::Stmt* unconst_dc = const_cast<clang::Stmt*>(dc);
        setASTState(coord, unconst_dc, c);
        overrideStmt2Coords(dc, coord);     // Use Clang canonical addresses? 
        overrideCoords2Stmt(coord, dc);     // Use Clang canonical addresses?  
    }*/
    return coord;
}
";
                        file += mkStr;
                        break;
                    }
                    else {
                        switch (pcase.CaseType)
                        {
                            case Grammar.CaseType.Ident:
                                {
                                    break;
                                }
                            case Grammar.CaseType.Op:
                            case Grammar.CaseType.Hidden:
                            case Grammar.CaseType.Pure:
                                {

                                    if (!(pcase.IsFuncDeclare || pcase.IsTranslationDeclare || pcase.IsVarDeclare))
                                    {
                                        var mkStr = @"
coords::" + pcase.Name + @"* ASTToCoords::mk" + pcase.Name + "(const ast::" + pcase.Name + "* ast, clang::ASTContext* c" + (pcase.Productions.Count > 0 ? "," +
                            string.Join(",", pcase.Productions.Select(p_ => "coords::" + p_.Name + "* operand" + ++i)) : "") +
                            (prod.HasValueContainer() ? "," +
                        Peirce.Join(",", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => "std::shared_ptr<" + prod.GetPriorityValueContainer().ValueType + "> value" + v) : "") + @"){
    coords::" + pcase.Name + @"* coord = new coords::" + pcase.Name + @"(" + string.Join(",", pcase.Productions.Select(p_ => "operand" + ++j))
    + (prod.HasValueContainer() ? (pcase.Productions.Count > 0 ? "," : "") +
                        Peirce.Join(",", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v =>  " value" + v) : "") + @");
    ast::" + pcase.Name + "* unconst_ast = const_cast<ast::" + pcase.Name + @"*>(ast);"
+
(prod.HasValueContainer() ? Peirce.Join("", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => "\n\t//coord->setValue(value" + v + "," + v + ");") : "")

+ @"
    /*if (auto dc = clang::dyn_cast<clang::NamedDecl>(unconst_ast)){
        clang::NamedDecl* unconst_dc = const_cast<clang::NamedDecl*>(dc);
        setASTState(coord, unconst_dc, c);
        overrideDecl2Coords(dc, coord);     // Use Clang canonical addresses? 
        overrideCoords2Decl(coord, dc);     // Use Clang canonical addresses?
    }*/
    if (auto dc = clang::dyn_cast<clang::Stmt>(unconst_ast)){
        clang::Stmt* unconst_dc = const_cast<clang::Stmt*>(dc);
        setASTState(coord, unconst_dc, c);
        overrideStmt2Coords(dc, coord);     // Use Clang canonical addresses? 
        overrideCoords2Stmt(coord, dc);     // Use Clang canonical addresses?  
    }
    return coord;
}
";
                                        file += mkStr;
                                    }
                                    else
                                    {

                                        var mkStr = @"
coords::" + pcase.Name + @"* ASTToCoords::mk" + pcase.Name + "(const ast::" + pcase.Name + "* ast, clang::ASTContext* c" + (pcase.Productions.Count > 0 ? "," +
                            string.Join(",", pcase.Productions.Select(p_ => "coords::" + p_.Name + "* operand" + ++i)) : "")  +
                            (prod.HasValueContainer() ? "," +
                        Peirce.Join(",", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => "std::shared_ptr<" + prod.GetPriorityValueContainer().ValueType + "> value" + v) : "") + @"){
    coords::" + pcase.Name + @"* coord = new coords::" + pcase.Name + @"(" + string.Join(",", pcase.Productions.Select(p_ => "operand" + ++j))
    + (prod.HasValueContainer() ? "," +
                        Peirce.Join(",", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => " value" + v) : "") + @");
    ast::" + pcase.Name + "* unconst_ast = const_cast<ast::" + pcase.Name + @"*>(ast);"
+
(prod.HasValueContainer() ? Peirce.Join("", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => "\n\t//coord->setValue(value" + v + ","+v+");") : "")

+@"
 
    if (auto dc = clang::dyn_cast<clang::NamedDecl>(unconst_ast)){
        clang::NamedDecl* unconst_dc = const_cast<clang::NamedDecl*>(dc);
        setASTState(coord, unconst_dc, c);
        overrideDecl2Coords(dc, coord);     // Use Clang canonical addresses? 
        overrideCoords2Decl(coord, dc);     // Use Clang canonical addresses?
    }
    /*if (auto dc = clang::dyn_cast<clang::Stmt>(unconst_ast)){
        clang::Stmt* unconst_dc = const_cast<clang::Stmt*>(dc);
        setASTState(coord, unconst_dc, c);
        overrideStmt2Coords(dc, coord);     // Use Clang canonical addresses? 
        overrideCoords2Stmt(coord, dc);     // Use Clang canonical addresses?  
    }*/
    return coord;
}
";
                                        file += mkStr;
                                    }
                                    break;
                                }
                            /*case Grammar.CaseType.Pure://fix this!!
                                {
                                    if (!(pcase.IsFuncDeclare || pcase.IsTranslationDeclare || pcase.IsVarDeclare))
                                    {
                                        var mkStr = @"
coords::" + pcase.Name + @"* ASTToCoords::mk" + pcase.Name + "(const ast::" + pcase.Name + "* ast, clang::ASTContext* c" + (pcase.Productions.Count > 0 ? "," +
                            string.Join(",", pcase.Productions.Select(p_ => "coords::" + p_.Name + "* operand" + ++i)) : "") + @"){
    coords::" + pcase.Name + @"* coord = new coords::" + pcase.Name + @"(" + string.Join(",", pcase.Productions.Select(p_ => "operand" + ++j)) + @");
    ast::" + pcase.Name + "* unconst_ast = const_cast<ast::" + pcase.Name + @"*>(ast);
 
    /*if (auto dc = clang::dyn_cast<clang::NamedDecl>(unconst_ast)){
        clang::NamedDecl* unconst_dc = const_cast<clang::NamedDecl*>(dc);
        setASTState(coord, unconst_dc, c);
        overrideDecl2Coords(dc, coord);     // Use Clang canonical addresses? 
        overrideCoords2Decl(coord, dc);     // Use Clang canonical addresses?
    }
    if (auto dc = clang::dyn_cast<clang::Stmt>(unconst_ast)){
        clang::Stmt* unconst_dc = const_cast<clang::Stmt*>(dc);
        setASTState(coord, unconst_dc, c);
        overrideStmt2Coords(dc, coord);     // Use Clang canonical addresses? 
        overrideCoords2Stmt(coord, dc);     // Use Clang canonical addresses?  
    }
    return coord;
}
";
                                        file += mkStr;
                                    }
                                    else
                                    {

                                        var mkStr = @"
coords::" + pcase.Name + @"* ASTToCoords::mk" + pcase.Name + "(const ast::" + pcase.Name + "* ast, clang::ASTContext* c" + (pcase.Productions.Count > 0 ? "," +
                            string.Join(",", pcase.Productions.Select(p_ => "coords::" + p_.Name + "* operand" + ++i)) : "") + @"){
    coords::" + pcase.Name + @"* coord = new coords::" + pcase.Name + @"(" + string.Join(",", pcase.Productions.Select(p_ => "operand" + ++j)) + @");
    ast::" + pcase.Name + "* unconst_ast = const_cast<ast::" + pcase.Name + @"*>(ast);
 
    if (auto dc = clang::dyn_cast<clang::NamedDecl>(unconst_ast)){
        clang::NamedDecl* unconst_dc = const_cast<clang::NamedDecl*>(dc);
        setASTState(coord, unconst_dc, c);
        overrideDecl2Coords(dc, coord);     // Use Clang canonical addresses? 
        overrideCoords2Decl(coord, dc);     // Use Clang canonical addresses?
    }
    /*if (auto dc = clang::dyn_cast<clang::Stmt>(unconst_ast)){
        clang::Stmt* unconst_dc = const_cast<clang::Stmt*>(dc);
        setASTState(coord, unconst_dc, c);
        overrideStmt2Coords(dc, coord);     // Use Clang canonical addresses? 
        overrideCoords2Stmt(coord, dc);     // Use Clang canonical addresses?  
    }
    return coord;
}
";
                                        file += mkStr;
                                    }
                                    break;
                                }*/
                            case Grammar.CaseType.ArrayOp:
                                {

                                    if(!(pcase.IsFuncDeclare || pcase.IsTranslationDeclare || pcase.IsVarDeclare))
                                    {
                                        var mkStr = @"
coords::" + pcase.Name + @"* ASTToCoords::mk" + pcase.Name + "(const ast::" + pcase.Name + "* ast, clang::ASTContext* c, std::vector<coords::" + pcase.Productions[0].Name + @"*> operands ){
    coords::" + pcase.Name + @"* coord = new coords::" + pcase.Name + @"(operands);
    ast::" + pcase.Name + "* unconst_ast = const_cast<ast::" + pcase.Name + @"*>(ast);

    /*if (auto dc = clang::dyn_cast<clang::NamedDecl>(unconst_ast)){
        clang::NamedDecl* unconst_dc = const_cast<clang::NamedDecl*>(dc);
        setASTState(coord, unconst_dc, c);
        overrideDecl2Coords(dc, coord);     // Use Clang canonical addresses? 
        overrideCoords2Decl(coord, dc);     // Use Clang canonical addresses?
    }*/
    if (auto dc = clang::dyn_cast<clang::Stmt>(unconst_ast)){
        clang::Stmt* unconst_dc = const_cast<clang::Stmt*>(dc);
        setASTState(coord, unconst_dc, c);
        overrideStmt2Coords(dc, coord);     // Use Clang canonical addresses? 
        overrideCoords2Stmt(coord, dc);     // Use Clang canonical addresses?  
    }
    return coord;
}
";
                                        file += mkStr;
                                    }
                                    else if (pcase.IsTranslationDeclare)
                                    {

                                        var mkStr = @"
coords::" + pcase.Name + @"* ASTToCoords::mk" + pcase.Name + "(const ast::" + pcase.Name + "* ast, clang::ASTContext* c, std::vector<coords::" + pcase.Productions[0].Name + @"*> operands ){
    coords::" + pcase.Name + @"* coord = new coords::" + pcase.Name + @"(operands);
    ast::" + pcase.Name + "* unconst_ast = const_cast<ast::" + pcase.Name + @"*>(ast);

    coord->state_ = new coords::ASTState(
        """",
        """",
        """",
        """",
        0,
        0,
        0,
        0
    );

    return coord;
}
";
                                        file += mkStr;
                                    }
                                    else
                                    {
                                        var mkStr = @"
coords::" + pcase.Name + @"* ASTToCoords::mk" + pcase.Name + "(const ast::" + pcase.Name + "* ast, clang::ASTContext* c, std::vector<coords::" + pcase.Productions[0].Name + @"*> operands ){
    coords::" + pcase.Name + @"* coord = new coords::" + pcase.Name + @"(operands);
    ast::" + pcase.Name + "* unconst_ast = const_cast<ast::" + pcase.Name + @"*>(ast);

    if (auto dc = clang::dyn_cast<clang::NamedDecl>(unconst_ast)){
        clang::NamedDecl* unconst_dc = const_cast<clang::NamedDecl*>(dc);
        setASTState(coord, unconst_dc, c);
        overrideDecl2Coords(dc, coord);     // Use Clang canonical addresses? 
        overrideCoords2Decl(coord, dc);     // Use Clang canonical addresses?
    }
    /*if (auto dc = clang::dyn_cast<clang::Stmt>(unconst_ast)){
        clang::Stmt* unconst_dc = const_cast<clang::Stmt*>(dc);
        setASTState(coord, unconst_dc, c);
        overrideStmt2Coords(dc, coord);     // Use Clang canonical addresses? 
        overrideCoords2Stmt(coord, dc);     // Use Clang canonical addresses?  
    }*/
    return coord;
}
";
                                        file += mkStr;
                                    }

                                    break;

                                }
                           /* case Grammar.CaseType.Value:
                                {
                                    var mkStr = @"
coords::" + pcase.Name + @"* ASTToCoords::mk" + pcase.Name + "(const ast::" + pcase.Name + "* ast, clang::ASTContext* c" + (pcase.ValueCount > 0 ? "," +
                    Peirce.Join(",", Enumerable.Range(0, pcase.ValueCount), v => pcase.ValueType + " operand" + v) : "") + @"){
    coords::" + pcase.Name + @"* coord = new coords::" + pcase.Name + @"(" + Peirce.Join(",", Enumerable.Range(0, pcase.ValueCount), v => " operand" + v) + @");
    ast::" + pcase.Name + "* unconst_ast = const_cast<ast::" + pcase.Name + @"*>(ast);
    //bad Clang! bad!
    /*if (auto dc = clang::dyn_cast<clang::NamedDecl>(unconst_ast)){
        clang::NamedDecl* unconst_dc = const_cast<clang::NamedDecl*>(dc);
        setASTState(coord, unconst_dc, c);
        overrideDecl2Coords(dc, coord);     // Use Clang canonical addresses? 
        overrideCoords2Decl(coord, dc);     // Use Clang canonical addresses?
    }
    if (auto dc = clang::dyn_cast<clang::Stmt>(unconst_ast)){
        clang::Stmt* unconst_dc = const_cast<clang::Stmt*>(dc);
        setASTState(coord, unconst_dc, c);
        overrideStmt2Coords(dc, coord);     // Use Clang canonical addresses? 
        overrideCoords2Stmt(coord, dc);     // Use Clang canonical addresses?  
    }
    return coord;
}
";
                                    file += mkStr;
                                    break;
                                }*/

                        }
                    }
                }
            }

            file += @"
//using namespace std;
void ASTToCoords::overrideStmt2Coords(const clang::Stmt *s, coords::Coords *c) {
    stmt_coords->insert(std::make_pair(s, c));
}



void ASTToCoords::overrideDecl2Coords(const clang::Decl *d, coords::Coords *c) {
    
    decl_coords->insert(std::make_pair(d, c));
}



void ASTToCoords::overrideCoords2Stmt(coords::Coords *c, const clang::Stmt *s) {
    
    coords_stmt->insert(std::make_pair(c, s));
}



void ASTToCoords::overrideCoords2Decl(coords::Coords *c, const clang::Decl *d) {
    
    coords_decl->insert(std::make_pair(c, d));
}
";


            this.CppFile = file;
        }

        public override void GenHeader()
        {
            var header = @"
#ifndef ASTTOCOORDS_H
#define ASTTOCOORDS_H

#include ""AST.h""
#include ""clang/AST/AST.h""
#include ""Coords.h""

#include <memory>

#include <iostream>

/*
This relational class maps Clang AST nodes to code coordinates
in our ontology. We want a single base type for all coordinates. 
Clang does not have a single base type for all AST nodes. This is
a special case of the broader reality that we will want to have
code coordinates for diverse types of code elements. So we will
need multiple function types, from various code element types to
our uniform (base class for) code coordinates. Coords subclasses
add specialized state and behavior corresponding to their concrete
code element types.

Note: At present, the only kind of Clang AST nodes that we need
are Stmt nodes. Stmt is a deep base class for Clang AST nodes,
including clang::Expr, clang::DeclRefExpr, clang:MemberCallExpr,
and so forth. So the current class is overbuilt in a sense; but
we design it as we have to show the intended path to generality.

To use this class, apply the mk methods to Clang AST nodes of 
the appropriate types. These methods create Coord objects of the
corresponding types, wrape the AST nodes in the Coords objects,
update the relational map, and return the constructed Coords. 
Client code is responsible for deleting (TBD).

Also note that Vector_Lit doesn't have a sub-expression.
*/

namespace ast2coords {

/*
When generating interpretation, we know subtypes of vector expressions
(literal, variable, function application), and so do not need and should
not use a generic putter. So here there are no superclass mkVecExpr or
Vector mk oprations. 
*/

class ASTToCoords
{
public:

    ASTToCoords();

    void setASTState(coords::Coords* coords, clang::Stmt* stmt, clang::ASTContext* c);
    void setASTState(coords::Coords* coords, clang::Decl* decl, clang::ASTContext* c);

";
            var file = header;

            foreach(var prod in ParsePeirce.Instance.Grammar.Productions)
            {
                foreach(var pcase in prod.Cases)
                {

                    if (pcase.CaseType == Grammar.CaseType.Passthrough || pcase.CaseType == Grammar.CaseType.Inherits)
                        continue;
                    else if (prod.ProductionType == Grammar.ProductionType.Single || prod.ProductionType == Grammar.ProductionType.CaptureSingle)
                    {

                        int i = 0;
                        var mkStr = "\tcoords::" + prod.Name + "* mk" + prod.Name + "(const ast::" + prod.Name + "* ast, clang::ASTContext* c" + (pcase.Productions.Count > 0 ? "," +
                            string.Join(",", pcase.Productions.Select(p_ => "coords::" + p_.Name + "* operand" + ++i)) : "") +(prod.HasValueContainer() ? "," +
                        Peirce.Join(",", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => "std::shared_ptr<" + prod.GetPriorityValueContainer().ValueType + "> value" + v) : "") + ");";
                        file += "\n" + mkStr + "\n";
                    }
                    else
                    {

                        switch (pcase.CaseType)
                        {
                            case Grammar.CaseType.Ident:
                                {
                                    break;
                                }
                            case Grammar.CaseType.Pure:
                                /*{
                                    int i = 0;
                                    var mkStr = "\tcoords::" + pcase.Name + "* mk" + pcase.Name + "(clang::ASTContext* c" + (pcase.Productions.Count > 0 ? "," +
                                        string.Join(",", pcase.Productions.Select(p_ => "coords::" + p_.Name + "* operand" + ++i)) : "") + ");";
                                    file += "\n" + mkStr + "\n";
                                    break;

                                }*/
                            case Grammar.CaseType.Hidden:
                            case Grammar.CaseType.Op:
                                {
                                    int i = 0;
                                    var mkStr = "\tcoords::" + pcase.Name + "* mk" + pcase.Name + "(const ast::" + pcase.Name + "* ast, clang::ASTContext* c" + (pcase.Productions.Count > 0 ? "," +
                                        string.Join(",", pcase.Productions.Select(p_ => "coords::" + p_.Name + "* operand" + ++i)) : "") + (prod.HasValueContainer() ? "," +
                        Peirce.Join(",", Enumerable.Range(0, prod.GetPriorityValueContainer().ValueCount), v => "std::shared_ptr<" + prod.GetPriorityValueContainer().ValueType + "> value" + v) : "") + ");";
                                    file += "\n" + mkStr + "\n";
                                    break;
                                }
                            case Grammar.CaseType.ArrayOp:
                                {
                                    int i = 0;
                                    var mkStr = "\tcoords::" + pcase.Name + "* mk" + pcase.Name + "(const ast::" + pcase.Name + "* ast, clang::ASTContext* c, std::vector<coords::" + pcase.Productions[0].Name + @"*> operands );";
                                    file += "\n" + mkStr + "\n";
                                    break;

                                }
                            /*case Grammar.CaseType.Value:
                                {
                                    int i = 0;
                                    var mkStr = "\tcoords::" + pcase.Name + "* mk" + pcase.Name + "(const ast::" + pcase.Name + "* ast, clang::ASTContext* c" + (pcase.ValueCount > 0 ? "," +
                        Peirce.Join(",", Enumerable.Range(0, pcase.ValueCount), v => pcase.ValueType + " operand" + v) : "") + ");";
                                    file += "\n" + mkStr + "\n";
                                    break;
                                }*/

                        }
                    }
                }
            }


            var footer = @"
    // TODO -- Have these routines return more specific subclass objects
    coords::Coords *getStmtCoords(const clang::Stmt *s) {
        return stmt_coords->find(s)->second;
    }

    coords::Coords *getDeclCoords(const clang::Decl *d) {
        return decl_coords->find(d)->second;
    }


	bool existsStmtCoords(const clang::Stmt* s) {
		return stmt_coords->find(s) != stmt_coords->end();
	}

	bool existsDeclCoords(const clang::Decl *d) {
		return decl_coords->find(d) != decl_coords->end();
	}



    /*
    !!!! I NEED THESE BADLY. MOVING TO PUBLIC !!!!
    */
    std::unordered_map<const clang::Stmt *, coords::Coords *> *stmt_coords;
    std::unordered_map<const clang::Decl *, coords::Coords *> *decl_coords;
    std::unordered_map<coords::Coords *,const clang::Stmt *> *coords_stmt;
    std::unordered_map<coords::Coords *,const clang::Decl *> *coords_decl;

 private:
    void overrideStmt2Coords(const clang::Stmt *s, coords::Coords *c);
    void overrideDecl2Coords(const clang::Decl*, coords::Coords *c);
    void overrideCoords2Stmt(coords::Coords *c, const clang::Stmt *s);
    void overrideCoords2Decl(coords::Coords *c, const clang::Decl *d);
    /*
    std::unordered_map<const clang::Stmt *, coords::Coords *> stmt_coords;
    std::unordered_map<const clang::Decl *, coords::Coords *> decl_coords;
    std::unordered_map<coords::Coords *,const clang::Stmt *> coords_stmt;
    std::unordered_map<coords::Coords *,const clang::Decl *> coords_decl;
    */
    
};
";
            footer += @"

} // namespace

#endif


";
            file += footer;

            this.HeaderFile = file;
        }
    }
}
