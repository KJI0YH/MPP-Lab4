using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Core.Tests
{
    [TestFixture]
    public class TestsGenerator_Tests
    {
        private readonly TestsGenerator _generator = new();

        [Test]
        public void ClassesCountTest()
        {
            var classTests = _generator.Generate(ProgramText1 + ProgramText2 + ProgramText3);

            var parsedClass = CSharpSyntaxTree.ParseText(classTests.Content).GetCompilationUnitRoot();
            Assert.That(parsedClass.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList(), Has.Count.EqualTo(3));
        }

        [Test]
        public void UsingsTest()
        {
            var classTests = _generator.Generate(ProgramText1 + ProgramText2 + ProgramText3);

            var parsedClass = CSharpSyntaxTree.ParseText(classTests.Content).GetCompilationUnitRoot();
            var usings = parsedClass.DescendantNodes().OfType<UsingDirectiveSyntax>().ToList();

            string controlUsings = @"
                System
                System.Collections.Generic
                System.Linq
                System.Text
                NUnit.Framework
            ";

            string namespaceUsings = @"
                HelloWorld
                WantSleep
                One.AnotherOne.AndAnotherOne
            ";

            foreach (var @using in usings)
            {
                Assert.That(controlUsings.Contains(@using.Name.ToString()) || namespaceUsings.Contains(@using.Name.ToString()));
            }
        }

        [Test]
        public void MethodsTest()
        {
            var classTests = _generator.Generate(ProgramText1 + ProgramText2 + ProgramText3);

            var parsedClass = CSharpSyntaxTree.ParseText(classTests.Content).GetCompilationUnitRoot();
            Assert.That(parsedClass.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList(), Has.Count.EqualTo(7));
        }

        [Test]
        public void OverloadMethodsTest()
        {
            var classTests = _generator.Generate(ProgramText3);

            var parsedClass = CSharpSyntaxTree.ParseText(classTests.Content).GetCompilationUnitRoot();
            var methods = parsedClass.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
            Assert.That(methods, Has.Count.EqualTo(3));
            Assert.Multiple(() =>
            {
                Assert.That(methods[0].Identifier.Text, Is.EqualTo("AndAnother_0_Test"));
                Assert.That(methods[1].Identifier.Text, Is.EqualTo("AndAnother_1_Test"));
            });
        }

        private const string ProgramText1 = @"
        namespace HelloWorld
        {
            public class Program
            {                                
                public static void Main(string[] args)
                {
                    Console.WriteLine(""Hello, World!"");
                }
                public int GetRandom()
                {
                    return 42;
                }
            }
        }";

        private const string ProgramText2 = @"
        namespace WantSleep
        {            
            public class Sleep
            {   
                public static void Main(string[] args)
                {
                    Console.WriteLine(""Hello, World!"");
                }
                public void SomeSleep()
                {
                    Thread.Sleep(10000000);
                }
                private void Bed(string name)
                {
                    return name;
                }
            }
        }";

        private const string ProgramText3 = @"
        namespace One.AnotherOne.AndAnotherOne
        {
            public static class One
            {
                public void Another(double x)
                {
                    return 42;
                }
                public int AndAnother(int x)
                {
                    return 42;
                }
                public int AndAnother(long x)
                {
                    return 42000000000;
                }
            }
        }";
    }
}