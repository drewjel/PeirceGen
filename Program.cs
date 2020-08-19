using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeirceGen
{
    class Program
    {
        static void Main(string[] args)
        {
            var Peirce = ParsePeirce.Instance;

            //var tt = Peirce.Grammar.Productions.SelectMany(p => p.Cases.Where(c_ => c_.CaseType == Grammar.CaseType.Pure));

            var Coords = new Generators.GenCoords();
            Coords.GetType();
            //Coords = Coords;//remove warning
            var Interp = new Generators.GenInterp();
            Interp.GetType();
            //Interp = Interp;
            var Domain = new Generators.GenDomain();
            //Domain = Domain;
            Domain.GetType();
            var ASTToCoords = new Generators.GenASTToCoords();
            //ASTToCoords = null;
            ASTToCoords.GetType();
            var CoordsToDomain = new Generators.GenCoordsToDomain();
            //CoordsToDomain = CoordsToDomain;
            CoordsToDomain.GetType();
            var CoordsToInterp = new Generators.GenCoordsToInterp();
            //CoordsToInterp = CoordsToInterp;
            CoordsToInterp.GetType();
            var InterpToDomain = new Generators.GenInterpToDomain();
            //InterpToDomain = InterpToDomain;
            InterpToDomain.GetType();
            var Interpretation = new Generators.GenInterpretation();
            //Interpretation = Interpretation;
            Interpretation.GetType();
            var Oracle = new Generators.GenOracle();
            //Oracle = Oracle;
            Oracle.GetType();
            var AST = new Generators.GenAST(42);
            //AST = AST;
            AST.GetType();
            Peirce.MatcherProductions.ForEach(prod => new Generators.GenMatcher(prod));
            Generators.GenMatcher.GenTopLevelMatchers();



            /*
            GenInterp();
            GenDomain();
            GenCoords();
            GenASTToCoords();
            GenCoordsToDomain();
            GenCoordsToInterp();
            GenInterpToDomain();
            GenInterpretation();
            */
        }
    }
}
