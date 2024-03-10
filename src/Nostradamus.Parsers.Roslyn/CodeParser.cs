using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nostradamus.Core;

public class CodeParser
{
    private readonly CSharpParseOptions _options;

    public CodeParser()
    {
        _options = new CSharpParseOptions(LanguageVersion.LatestMajor);
    }

    public List<Artifact> FindArtifacts(string fileContent)
    {
        ArgumentNullException.ThrowIfNull(fileContent);
        var tree = CSharpSyntaxTree.ParseText(fileContent, _options);
        var root = (CompilationUnitSyntax)tree.GetRoot();
        var artifacts = new List<Artifact>();

        foreach (var member in root.Members)
        {
            artifacts.AddRange(ProcessMember(member));
        }

        return artifacts;
    }

    private IEnumerable<Artifact> ProcessMember(SyntaxNode member)
    {
        switch (member)
        {
            case BaseNamespaceDeclarationSyntax namespaceDeclaration:
                foreach (var namespaceMember in namespaceDeclaration.Members)
                {
                    foreach (var artifact in ProcessMember(namespaceMember))
                    {
                        yield return artifact;
                    }
                }
                break;
            case ClassDeclarationSyntax classDeclaration:
                foreach (var artifact in ProcessClass(classDeclaration))
                {
                    yield return artifact;
                }
                break;
        }
    }

    private IEnumerable<Artifact> ProcessClass(ClassDeclarationSyntax classDeclaration)
    {
        foreach (var member in classDeclaration.Members)
        {
            if (member is MethodDeclarationSyntax methodDeclaration)
            {
                var artifact = CreateArtifactFromClassMember(methodDeclaration, classDeclaration.Identifier.Text);
                yield return artifact;
            }
            else if (member is ConstructorDeclarationSyntax constructorDeclaration)
            {
                var artifact = CreateArtifactFromClassMember(constructorDeclaration, classDeclaration.Identifier.Text);
                yield return artifact;
            }
            else if (member is ClassDeclarationSyntax nestedClassDeclaration)
            {
                foreach (var artifact in ProcessClass(nestedClassDeclaration))
                {
                    yield return artifact;
                }
            }
        }
    }

    private Artifact CreateArtifactFromClassMember(BaseMethodDeclarationSyntax member, string classFullName)
    {
        var startLine = member.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        var endLine = member.GetLocation().GetLineSpan().EndLinePosition.Line + 1;
        var content = member.ToFullString().Trim();
        return new Artifact(classFullName, content, startLine, endLine);
    }
}
