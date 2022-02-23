using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;

namespace KidsTown.KidsTown
{
    public interface IDocumentationService
    {
        Task<IImmutableList<DocumentationElement>> GetDocumentation();
    }
}