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
            //Coords = null;//remove warning
            Coords.GetType();
            var Interp = new Generators.GenInterp();
            //Interp = null;
            Interp.GetType();
            var Domain = new Generators.GenDomain();
            //Domain = null;
            Domain.GetType();
            var ASTToCoords = new Generators.GenASTToCoords();
            //ASTToCoords = null;
            ASTToCoords.GetType();
            var CoordsToDomain = new Generators.GenCoordsToDomain();
            //CoordsToDomain = null;
            CoordsToDomain.GetType();
            var CoordsToInterp = new Generators.GenCoordsToInterp();
            // CoordsToInterp = null;
            CoordsToInterp.GetType();
            var InterpToDomain = new Generators.GenInterpToDomain();
            // InterpToDomain = null;
            InterpToDomain.GetType();
            var Interpretation = new Generators.GenInterpretation();
            // Interpretation = null;
            Interpretation.GetType();
            var Oracle = new Generators.GenOracle();
            //Oracle = null;
            Oracle.GetType();
            var AST = new Generators.GenAST(42);
            // AST = null;
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
