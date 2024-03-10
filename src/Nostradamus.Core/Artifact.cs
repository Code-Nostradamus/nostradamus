namespace Nostradamus.Core;

public class Artifact
{
    public Artifact(string classFullName, string content, int startLine, int endLine)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(endLine);
        ArgumentOutOfRangeException.ThrowIfNegative(startLine);
        if (endLine < startLine)
        {
            throw new ArgumentOutOfRangeException(nameof(endLine), "End line must be greater than or equal to start line.");
        }

        ClassFullName = classFullName ?? throw new ArgumentNullException(nameof(classFullName));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        StartLine = startLine;
        EndLine = endLine;
    }

    public string ClassFullName { get; }

    public string Content { get; }

    public int StartLine { get; }

    public int EndLine { get; }

    public string ClassName => ClassFullName.Split('.').Last();

    public override string ToString()
    {
        return $"Artifact(ClassFullName='{ClassFullName}', Content='{Content}', StartLine={StartLine}, EndLine={EndLine})";
    }
}
