namespace Nostradamus.Parsers.Roslyn.Tests;

public class CodeParserTests // TODO: local functions handling test, interface handling test
{
    [Fact]
    public void Finding_artifacts_within_a_namespace_returns_correct_artifacts()
    {
        // Arrange
        var parser = new CodeParser();
        var fileContent = @"
            namespace TestNamespace {
                class TestClass {
                    void TestMethod() {}
                }
            }";

        // Act
        var artifacts = parser.FindArtifacts(fileContent);

        // Assert
        artifacts.Should().ContainSingle();
        artifacts[0].ClassFullName.Should().Be("TestClass");
        artifacts[0].Content.Should().Contain("TestMethod");
    }

    [Fact]
    public void Finding_artifacts_without_a_namespace_returns_correct_artifacts()
    {
        // Arrange
        var parser = new CodeParser();
        var fileContent = @"
            class TestClass {
                void TestMethod() {}
            }";

        // Act
        var artifacts = parser.FindArtifacts(fileContent);

        // Assert
        artifacts.Should().ContainSingle();
        artifacts[0].ClassFullName.Should().Be("TestClass");
        artifacts[0].Content.Should().Contain("TestMethod");
    }

    [Fact]
    public void Finding_artifacts_in_empty_file_returns_no_artifacts()
    {
        // Arrange
        var parser = new CodeParser();
        var fileContent = "";

        // Act
        var artifacts = parser.FindArtifacts(fileContent);

        // Assert
        artifacts.Should().BeEmpty();
    }

    [Fact]
    public void Finding_artifacts_throws_ArgumentNullException_for_null_input()
    {
        // Arrange
        var parser = new CodeParser();

        // Act
        Action act = () => parser.FindArtifacts(null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public async Task Finding_artifacts_within_a_file_scoped_namespace_returns_correct_artifacts()
    {
        // Arrange
        var parser = new CodeParser();
        var fileContent =
            await File.ReadAllTextAsync(
                "../../../../assets/code/eshop/OrderingIntegrationEventService.cs");

        // Act
        var artifacts = parser.FindArtifacts(fileContent);

        // Assert
        artifacts.Should().HaveCountGreaterOrEqualTo(2);
    }

    [Fact]
    public void
        Parsing_file_with_a_nested_class_returns_artifacts_for_both_outer_and_nested_class_members()
    {
        // Arrange
        var parser = new CodeParser();
        var fileContent = @"
public class OuterClass
{
    public void OuterMethod() {}

    public class NestedClass
    {
        public void NestedMethod() {}
    }
}";

        // Act
        var artifacts = parser.FindArtifacts(fileContent);

        // Assert
        artifacts.Should().HaveCount(2,
            because: "we expect one artifact for the outer method and one for the nested method");
        artifacts.Should()
            .ContainSingle(a => a.ClassName == "OuterClass" && a.Content.Contains("OuterMethod"),
                because: "the outer class method should be recognized as an artifact");
        artifacts.Should()
            .ContainSingle(a => a.ClassName == "NestedClass" && a.Content.Contains("NestedMethod"),
                because: "the nested class method should also be recognized as an artifact");
    }

    [Fact]
    public void Parsing_file_with_multiple_levels_of_nested_classes_correctly_returns_artifacts_for_all_levels()
    {
        // Arrange
        var parser = new CodeParser();
        var fileContent = @"
public class LevelOne
{
    public class LevelTwo
    {
        public class LevelThree
        {
            public void LevelThreeMethod() {}
        }
    }
}";

        // Act
        var artifacts = parser.FindArtifacts(fileContent);

        // Assert
        artifacts.Should()
            .ContainSingle(
                because: "there should be a single artifact from the deepest nested class method");
        artifacts[0].ClassName.Should().Be("LevelThree",
            because: "the artifact should originate from the deepest level of nesting");
        artifacts[0].Content.Should().Contain("LevelThreeMethod",
            because: "it should capture the method in the deepest nested class");
    }
}
