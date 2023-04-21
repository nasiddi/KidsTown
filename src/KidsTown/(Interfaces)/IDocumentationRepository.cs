using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;

namespace KidsTown.KidsTown;

public interface IDocumentationRepository
{
    Task<UpdateResult> UpdateDocumentation(IImmutableSet<DocumentationElement> elements);
    Task<IImmutableSet<DocumentationElement>> LoadDocumentation(Section section);
    Task<IImmutableSet<string>> GetAllImageFileIds();
}

public enum UpdateResult
{
    Success,
    HasChanged,
    Failed
}