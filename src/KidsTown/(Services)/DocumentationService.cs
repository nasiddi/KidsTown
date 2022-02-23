using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using KidsTown.KidsTown.Models;

namespace KidsTown.KidsTown
{
    public class DocumentationService : IDocumentationService
    {
        private readonly IDocumentationRepository _documentationRepository;

        public DocumentationService(IDocumentationRepository _documentationRepository)
        {
            this._documentationRepository = _documentationRepository;
        }
        public Task<IImmutableList<DocumentationElement>> GetDocumentation()
        {
            return _documentationRepository.GetDocumentation();
        }
    }
}