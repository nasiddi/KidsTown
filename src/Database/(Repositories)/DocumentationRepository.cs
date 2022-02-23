using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.KidsTown;
using KidsTown.KidsTown.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DocumentationElement = KidsTown.Database.EfCore.DocumentationElement;
using DocumentationEntry = KidsTown.Database.EfCore.DocumentationEntry;
using DocumentationTitle = KidsTown.KidsTown.Models.DocumentationTitle;

namespace KidsTown.Database
{
    public class DocumentationRepository : IDocumentationRepository
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DocumentationRepository(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<IImmutableList<KidsTown.Models.DocumentationElement>> GetDocumentation()
        {
            await using var db = CommonRepository.GetDatabase(serviceScopeFactory: _serviceScopeFactory);

            var documentationElements = await db.DocumentationElements
                .Include(e => e.DocumentationTitles)
                .Include(e => e.DocumentationEntries)
                .ThenInclude(e => e.DocumentEntryParagraphs).ToListAsync();

            return documentationElements.Select(MapDocumentationElements).ToImmutableList();
        }

        private KidsTown.Models.DocumentationElement MapDocumentationElements(DocumentationElement element)
        {
            var title = TryMapTitle(element.DocumentationTitles.SingleOrDefault());
            var entry = TryMapEntry(element.DocumentationEntries.SingleOrDefault());

            return new KidsTown.Models.DocumentationElement
            {
                Id = element.Id,
                Position = element.Position,
                TabId = element.DocumentationTabId,
                IsTitle = element.DocumentationTitles.SingleOrDefault() != null,
                Title = title,
                Entry = entry
            };
        }

        private static KidsTown.Models.DocumentationEntry? TryMapEntry(DocumentationEntry? entry)
        {
            if (entry == null)
            {
                return null;
            }

            return new KidsTown.Models.DocumentationEntry
            {
                Id = entry.Id,
                FileName = entry.FileName,
                Title = entry.Title,
                Paragraphs = entry.DocumentEntryParagraphs.Select(p => new EntryParagraph
                {
                    Id = p.Id,
                    Position = p.Position,
                    Icon = p.Icon,
                    Text = p.Text
                }).ToImmutableList()
            };

        }

        private static DocumentationTitle? TryMapTitle(EfCore.DocumentationTitle? title)
        {
            if (title == null)
            {
                return null;
            }

            return new DocumentationTitle
            {
                Id = title.Id,
                Size = title.Size,
                Text = title.Text
            };
        }
    }
}