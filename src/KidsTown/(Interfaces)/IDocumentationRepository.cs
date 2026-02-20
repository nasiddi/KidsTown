using System.Collections.Immutable;
using KidsTown.Models;

namespace KidsTown;

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