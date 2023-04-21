using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;

namespace KidsTown.KidsTown;

public interface IDocumentationService
{
    Task<IImmutableSet<DocumentationElement>> GetDocumentation(Section section);
    Task<UpdateResult> UpdateDocumentation(IImmutableSet<DocumentationElement> documentation);
    Task<string> SaveImage(Stream stream, string fileName);
    Task CleanupImages();
}