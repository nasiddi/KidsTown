using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;

namespace KidsTown.KidsTown
{
    public interface IDocumentationRepository
    {
        Task<IImmutableList<DocumentationElement>> GetDocumentation();
    }
}