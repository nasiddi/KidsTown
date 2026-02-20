using System.Collections.Immutable;

namespace KidsTown.Models;

public record DocumentationElement(
    int Id,
    int PreviousId,
    int SectionId,
    Title Title,
    ImmutableList<Paragraph> Paragraphs,
    ImmutableList<Image> Images,
    DateTime UpdateDate);

public record Image(string FileId, int Id, int PreviousId);

public record Title(string Text = "", int Size = 5);

public record Paragraph(int Id, int PreviousId, string Text, ParagraphIcon? Icon);

public enum ParagraphIcon
{
    Action = 1,
    Info = 2,
    Warning = 3
}

public enum Section
{
    LabelStation = 1,
    ScanStation = 2
}