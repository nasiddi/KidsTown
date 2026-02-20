using System.Collections.Immutable;
using Database.EfCore;
using KidsTown;
using KidsTown.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Database;

public class DocumentationRepository(IServiceScopeFactory serviceScopeFactory) : IDocumentationRepository
{
    public async Task<IImmutableSet<string>> GetAllImageFileIds()
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);
        var images = await db.DocImages.ToListAsync();
        return images.Select(e => e.FileId).ToImmutableHashSet();
    }

    public async Task<IImmutableSet<DocumentationElement>> LoadDocumentation(Section section)
    {
        await using var db = CommonRepository.GetDatabase(serviceScopeFactory);

        var elements = await db.Set<DocElement>()
            .Where(e => e.SectionId == (int) section)
            .Include(e => e.DocImages)
            .Include(e => e.DocParagraphs)
            .Include(e => e.DocTitle)
            .ToListAsync();

        return elements.Select(MapDocumentationElement).ToImmutableHashSet();
    }

    public async Task<UpdateResult> UpdateDocumentation(IImmutableSet<DocumentationElement> allElements)
    {
        var elements = allElements.Where(
                e => e.Title.Text.Length > 0
                    || e.Images.Count > 0
                    || e.Paragraphs.Any(p => p.Text.Length > 0))
            .ToImmutableList();

        await using var context = CommonRepository.GetDatabase(serviceScopeFactory);
        var existingDocElements = await context.DocElements.ToListAsync();
        var updateDate = DateTime.UtcNow;

        foreach (var docElement in elements.Select(MapDocElement))
        {
            var existingDocElement = existingDocElements.SingleOrDefault(e => e.Id == docElement.Id);

            if (existingDocElement == null)
            {
                context.DocElements.Add(docElement);
            }
            else
            {
                if (docElement.UpdateDate != DateTime.MinValue && docElement.UpdateDate != existingDocElement.UpdateDate)
                {
                    return UpdateResult.HasChanged;
                }

                docElement.UpdateDate = updateDate;
                context.Entry(existingDocElement).CurrentValues.SetValues(docElement);
                existingDocElements.Remove(existingDocElement);
            }
        }

        var elementsToDelete = existingDocElements.ToImmutableList();

        var imagesToDelete = await context.DocImages
            .Where(e => elementsToDelete.Select(d => d.Id).Contains(e.ElementId))
            .ToListAsync();

        var paragraphsToDelete = await context.DocParagraphs
            .Where(e => elementsToDelete.Select(d => d.Id).Contains(e.ElementId))
            .ToListAsync();

        var titlesToDelete = await context.DocTitles
            .Where(e => elementsToDelete.Select(d => d.Id).Contains(e.ElementId))
            .ToListAsync();

        var existingDocImages = await context.DocImages.ToListAsync();

        foreach (var docImage in elements.SelectMany(e => e.Images.Select(i => MapDocImage(i, e.Id))))
        {
            var existingDocImage = existingDocImages.SingleOrDefault(di => di.Id == docImage.Id);

            if (existingDocImage == null)
            {
                context.DocImages.Add(docImage);
            }
            else
            {
                context.Entry(existingDocImage).CurrentValues.SetValues(docImage);
                existingDocImages.Remove(existingDocImage);
            }
        }

        imagesToDelete.AddRange(existingDocImages);
        var existingDocParagraphs = await context.DocParagraphs.ToListAsync();

        foreach (var docParagraph in elements.SelectMany(
                     e => e.Paragraphs
                         .Where(p => p.Text.Length > 0)
                         .Select(i => MapDocParagraph(i, e.Id))))
        {
            var existingDocParagraph = existingDocParagraphs.SingleOrDefault(dp => dp.Id == docParagraph.Id);

            if (existingDocParagraph == null)
            {
                context.DocParagraphs.Add(docParagraph);
            }
            else
            {
                context.Entry(existingDocParagraph).CurrentValues.SetValues(docParagraph);
                existingDocParagraphs.Remove(existingDocParagraph);
            }
        }

        paragraphsToDelete.AddRange(existingDocParagraphs);

        var existingDocTitles = await context.DocTitles.ToListAsync();

        foreach (var docTitle in elements.Select(e => MapDocTitle(e.Title, e.Id)).Where(e => e is not null))
        {
            var existingDocTitle = existingDocTitles.SingleOrDefault(dp => dp.ElementId == docTitle!.ElementId);

            if (existingDocTitle == null)
            {
                context.DocTitles.Add(docTitle!);
            }
            else
            {
                existingDocTitle.Content = docTitle!.Content;
                existingDocTitle.Size = docTitle.Size;
                existingDocTitles.Remove(existingDocTitle);
            }
        }

        titlesToDelete.AddRange(existingDocTitles);

        elementsToDelete.ForEach(e => e.PreviousId = null);
        paragraphsToDelete.ForEach(e => e.PreviousId = null);
        imagesToDelete.ForEach(e => e.PreviousId = null);
        await context.SaveChangesAsync();

        context.RemoveRange(imagesToDelete);
        context.RemoveRange(paragraphsToDelete);
        context.RemoveRange(titlesToDelete);
        context.RemoveRange(elementsToDelete);
        await context.SaveChangesAsync();

        var docElements = await context.DocElements
            .Where(e => elements.Select(d => d.Id).Contains(e.Id))
            .ToListAsync();

        foreach (var docElement in docElements)
        {
            var previousId = elements.Single(e => e.Id == docElement.Id).PreviousId;
            docElement.PreviousId = previousId == 0 ? null : previousId;
        }

        var images = elements.SelectMany(d => d.Images).ToImmutableList();

        var docImages = await context.DocImages
            .Where(e => images.Select(i => i.Id).Contains(e.Id))
            .ToListAsync();

        foreach (var docImage in docImages)
        {
            var previousId = images.Single(e => e.Id == docImage.Id).PreviousId;
            docImage.PreviousId = previousId == 0 ? null : previousId;
        }

        var paragraphs = elements.SelectMany(d => d.Paragraphs).ToImmutableList();

        var docParagraphs = await context.DocParagraphs
            .Where(e => paragraphs.Select(d => d.Id).Contains(e.Id))
            .ToListAsync();

        foreach (var docParagraph in docParagraphs)
        {
            var previousId = paragraphs.Single(e => e.Id == docParagraph.Id).PreviousId;
            docParagraph.PreviousId = previousId == 0 ? null : previousId;
        }

        await context.SaveChangesAsync();

        return UpdateResult.Success;
    }

    private static DocumentationElement MapDocumentationElement(DocElement element)
    {
        return new DocumentationElement(
            element.Id,
            element.PreviousId ?? 0,
            element.SectionId,
            MapTitle(element.DocTitle),
            element.DocParagraphs.Select(MapParagraph).ToImmutableList(),
            element.DocImages.Select(MapImage).ToImmutableList(),
            element.UpdateDate);
    }

    private static Image MapImage(DocImage image)
    {
        return new Image(
            image.FileId,
            image.Id,
            image.PreviousId ?? 0);
    }

    private static Paragraph MapParagraph(DocParagraph paragraph)
    {
        return new Paragraph(
            paragraph.Id,
            paragraph.PreviousId ?? 0,
            paragraph.Content,
            (ParagraphIcon?) paragraph.ParagraphIconId);
    }

    private static Title MapTitle(DocTitle? title)
    {
        return title is null
            ? new Title()
            : new Title(title.Content, title.Size);
    }

    private static DocElement MapDocElement(DocumentationElement element)
    {
        return new DocElement
        {
            Id = element.Id,
            PreviousId = null,
            SectionId = element.SectionId,
            UpdateDate = element.UpdateDate
        };
    }

    private static DocImage MapDocImage(Image image, int elementId)
    {
        return new DocImage
        {
            Id = image.Id,
            PreviousId = null,
            ElementId = elementId,
            FileId = image.FileId
        };
    }

    private static DocParagraph MapDocParagraph(Paragraph paragraph, int elementId)
    {
        return new DocParagraph
        {
            Id = paragraph.Id,
            PreviousId = null,
            ElementId = elementId,
            Content = paragraph.Text.Trim(),
            ParagraphIconId = (int?) paragraph.Icon
        };
    }

    private static DocTitle? MapDocTitle(Title title, int elementId)
    {
        return title.Text.Length > 0
            ? new DocTitle
            {
                Content = title.Text.Trim(),
                Size = title.Size,
                ElementId = elementId
            }
            : null;
    }
}