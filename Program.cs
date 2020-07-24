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

            var Coords = new Generators.GenCoords();
            var Interp = new Generators.GenInterp();
            var Domain = new Generators.GenDomain();
            var ASTToCoords = new Generators.GenASTToCoords();
            var CoordsToDomain = new Generators.GenCoordsToDomain();
            var CoordsToInterp = new Generators.GenCoordsToInterp();
            var InterpToDomain = new Generators.GenInterpToDomain();
            var Interpretation = new Generators.GenInterpretation();
            var Oracle = new Generators.GenOracle();
            var AST = new Generators.GenAST(42);


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
