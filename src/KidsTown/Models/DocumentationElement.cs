namespace KidsTown.KidsTown.Models
{
    public class DocumentationElement
    {
        public int Id { get; set; }
        public int Position { get; set; }
        public int TabId { get; set; }
        public bool IsTitle { get; set; }
        public DocumentationTitle? Title { get; set; }
        public DocumentationEntry? Entry { get; set; }
    }
}