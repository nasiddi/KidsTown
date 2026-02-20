using System.Collections.Immutable;
using KidsTown.Models;

namespace KidsTown;

public interface IDocumentationService
{
    Task<IImmutableSet<DocumentationElement>> GetDocumentation(Section section);
    Task<UpdateResult> UpdateDocumentation(IImmutableSet<DocumentationElement> documentation);
    Task<string> SaveImage(Stream stream, string fileName);
    Task CleanupImages();
}