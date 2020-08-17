using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeirceGen.Generators
{
    public class GenAST //: GenBase
    {
        public GenAST(int preventdefault)
        {

            GenHeader();
            if (!Directory.Exists(@"/peirce/PeirceGen/symlinkme"))
                Directory.CreateDirectory(@"/peirce/PeirceGen/symlinkme");
            System.IO.File.WriteAllText(this.GetHeaderLoc(), this.HeaderFile);
        }

        public string HeaderFile { get; set; }

        public void GenHeader()
        {
            var header = @"
#ifndef AST_H
#define AST_H

#include ""clang/AST/AST.h""
//#include ""clang/AST/ASTConsumer.h""
//#include ""clang/AST/Expr.h""
//#include ""clang/AST/Stmt.h""


namespace ast{

using RealScalar = double;

";          var file = header;

            var grammar = ParsePeirce.Instance.Grammar;

            foreach (var prod in grammar.Productions)
            {
                switch (prod.ProductionType)
                {
                    case Grammar.ProductionType.Single:
                    case Grammar.ProductionType.CaptureSingle: //{ break;  }
                        {
                            if (prod.IsFuncDeclare || prod.Cases[0].IsFuncDeclare)
                            {
                                file += "\n";
                                file += "using " + prod.Name + " = const clang::FunctionDecl;";

                            }
                            else if (prod.IsTranslationDeclare || prod.Cases[0].IsTranslationDeclare)
                            {
                                file += "\n";
                                file += "using " + prod.Name + " = const clang::TranslationUnitDecl;";

                            }
                            else if (prod.IsVarDeclare || prod.Cases[0].IsVarDeclare)
                            {
                                file += "\n";
                                file += "using " + prod.Name + " = const clang::VarDecl;";


                            }
                            else //(!(prod.Passthrough is Grammar.Production))
                            {
                                file += "\n";
                                file += "using " + prod.Name + " = const clang::Stmt;";
                            }
                            break;
                        }
                    default:{

                            if (prod.IsFuncDeclare)
                            {
                                file += "\n";
                                file += "using " + prod.Name + " = const clang::FunctionDecl;";

                            }
                            else if(prod.IsTranslationDeclare)
                            {
                                file += "\n";
                                file += "using " + prod.Name + " = const clang::TranslationUnitDecl;";

                            }
                            else if(prod.IsVarDeclare)
                            {
                                file += "\n";
                                file += "using " + prod.Name + " = const clang::VarDecl;";


                            }
                            else //(!(prod.Passthrough is Grammar.Production))
                            {
                                file += "\n";
                                file += "using " + prod.Name + " = const clang::Stmt;";
                            }
                            break;
                        }
                }

                foreach (var pcase in prod.Cases)
                {
                    if (pcase.CaseType == Grammar.CaseType.Passthrough || pcase.CaseType == Grammar.CaseType.Inherits)
                        continue;
                    switch (prod.ProductionType)
                    {
                        case Grammar.ProductionType.Single:
                        case Grammar.ProductionType.CaptureSingle:
                            {
                                break;
                                if (pcase.CaseType == Grammar.CaseType.Ident)
                                {
                                    if(pcase.IsFuncDeclare)
                                    {


                                        file += "\n";
                                        file += "using " + prod.Name + " = const clang::FunctionDecl;";
                                    }
                                    else if(pcase.IsTranslationDeclare)
                                    {


                                        file += "\n";
                                        file += "using " + prod.Name + " = const clang::TranslationUnitDecl;";
                                    }
                                    else if(pcase.IsVarDeclare)
                                    {


                                        file += "\n";
                                        file += "using " + prod.Name + " = const clang::VarDecl;";
                                    }
                                }break;
                            }
                        default:
                            {
                                if (pcase.CaseType == Grammar.CaseType.Ident || pcase.IsVarDeclare)
                                {

                                    file += "\n";
                                    file += "using " + prod.Name + "_" + pcase.Name + " = const clang::VarDecl;";
                                    break;
                                }
                                else if(pcase.IsFuncDeclare)
                                {

                                    file += "\n";
                                    file += "using " + pcase.Name + " = const clang::FunctionDecl;";
                                    break;

                                }
                                else if (pcase.IsTranslationDeclare)
                                {

                                    file += "\n";
                                    file += "using " + pcase.Name + " = const clang::TranslationUnitDecl;";
                                    break;

                                }
                                else
                                {

                                    file += "\n";
                                    file += "using " + pcase.Name + " = const clang::Stmt;";
                                    break;
                                }
                            }
                    }
                }
            }

            var footer = @"

} // namespace

#endif


";          file += footer;

            this.HeaderFile = file;
        }

        public string GetCPPLoc()
        {
            throw new NotImplementedException();
        }

        public string GetHeaderLoc()
        {
            return "/peirce/PeirceGen/symlinkme/AST.h";
        }
    }
}
