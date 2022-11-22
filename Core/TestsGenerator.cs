﻿namespace Core
{
    public class TestsGenerator
    {
        public TestInfo[] Generate(string source)
        {
            CompilationUnitSyntax root = CSharpSyntaxTree.ParseText(source).GetCompilationUnitRoot();

            // Generate usings
            var sourceNamespaces = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>();
            var sourceUsings = root.DescendantNodes().OfType<UsingDirectiveSyntax>();
            var usings = new SyntaxList<UsingDirectiveSyntax>(sourceUsings)
                .Add(UsingDirective(ParseName("System")))
                .Add(UsingDirective(ParseName("System.Collections.Generic")))
                .Add(UsingDirective(ParseName("System.Linq")))
                .Add(UsingDirective(ParseName("System.Text")))
                .Add(UsingDirective(ParseName("NUnit.Framework")))
                .AddRange(sourceNamespaces.Select(GetClassUsing));

            // Get source classes
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .Where(@class => @class.Modifiers.Any(SyntaxKind.PublicKeyword)).ToList();

            // Generate test classes
            var members = classes.Select(CreateTestClass).ToArray();

            TestInfo[] tests = new TestInfo[classes.Count];
            for (int i = 0; i < classes.Count; i++)
            {
                // Generate unit
                var unit = CompilationUnit().
                    WithUsings(usings)
                    .AddMembers(members[i]);

                tests[i] = new TestInfo(classes[i].Identifier.Text + "_Test", unit.NormalizeWhitespace().ToFullString());
            }

            return tests;
        }

        // Generate using from namespace
        private UsingDirectiveSyntax GetClassUsing(NamespaceDeclarationSyntax namespaceDeclaration)
        {
            return UsingDirective(namespaceDeclaration.Name);
        }

        private MemberDeclarationSyntax CreateTestClass(ClassDeclarationSyntax classDeclaration)
        {
            // Generate attributes
            var attribute = SingletonList(AttributeList(SingletonSeparatedList(
                Attribute(IdentifierName("TestFixture")))));

            // Generate namespace 
            var @namespace = NamespaceDeclaration(IdentifierName("Tests"));
            var ns = classDeclaration.Parent as NamespaceDeclarationSyntax;
            if (ns != null)
            {
                @namespace = NamespaceDeclaration(QualifiedName(ns.Name, IdentifierName("Tests")));
            }

            // Generate modifiers
            var modifiers = TokenList(Token(SyntaxKind.PublicKeyword));

            // Generate class methods
            var methods = CreateTestMethods(classDeclaration);

            // Generate class
            var testClass = ClassDeclaration(classDeclaration.Identifier.Text + "_Tests")
                .WithAttributeLists(attribute)
                .WithModifiers(modifiers)
                .AddMembers(methods);

            // Add test class to namespace
            @namespace = @namespace.AddMembers(testClass);

            return @namespace;
        }

        private MemberDeclarationSyntax[] CreateTestMethods(SyntaxNode syntaxNode)
        {
            // Generate attributes
            var attribute = SingletonList(AttributeList(SingletonSeparatedList(
                Attribute(IdentifierName("Test")))));

            // Generate return type
            var returnType = PredefinedType(Token(SyntaxKind.VoidKeyword));

            // Generate body
            var body = Block(ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression, IdentifierName(Identifier("Assert")), IdentifierName("Fail")))
                .WithArgumentList(ArgumentList(SingletonSeparatedList(
                    Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("autogenerated"))))))));

            var result = new List<MemberDeclarationSyntax>();
            var methods = syntaxNode.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .Where(method => method.Modifiers.Any(SyntaxKind.PublicKeyword)).ToList();

            methods.Sort(
                (method1, method2) => string.Compare(method1.Identifier.Text, method2.Identifier.Text, StringComparison.Ordinal));

            int IdIndex = -1;
            for (int i = 0; i < methods.Count; i++)
            {
                // Generate methods identificator
                if (i != methods.Count - 1 && methods[i].Identifier.Text == methods[i + 1].Identifier.Text)
                {
                    IdIndex = 0;
                }
                else if (i != 0 && methods[i].Identifier.Text == methods[i - 1].Identifier.Text)
                {
                    IdIndex++;
                }
                else
                {
                    IdIndex = -1;
                }
                string identificator = methods[i].Identifier.Text + (IdIndex == -1 ? "" : "_" + IdIndex.ToString()) + "_Test";

                // Generate methods
                result.Add(MethodDeclaration(returnType, identificator)
                    .WithAttributeLists(attribute)
                    .WithModifiers(methods[i].Modifiers)
                    .WithBody(body));
            }

            return result.ToArray();
        }
    }
}
