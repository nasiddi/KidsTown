using System.Collections.Immutable;

namespace KidsTown.KidsTown.Models
{
    public class DocumentationEntry
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public IImmutableList<EntryParagraph> Paragraphs { get; set; } = ImmutableList<EntryParagraph>.Empty;
        public string Title { get; set; } = string.Empty;
    }
}