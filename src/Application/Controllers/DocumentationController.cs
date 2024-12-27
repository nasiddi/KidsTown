using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using KidsTown.KidsTown;
using KidsTown.KidsTown.Models;
using Microsoft.AspNetCore.Mvc;

namespace KidsTown.Application.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentationController(IDocumentationService documentationService) : ControllerBase
{
    [HttpGet]
    [Produces("application/json")]
    [Route("{sectionId}")]
    public async Task<IImmutableSet<DocumentationElement>> GetDocumentation([FromRoute] int sectionId)
    {
        return await documentationService.GetDocumentation((Section) sectionId);
    }

    [HttpPost]
    [AuthenticateUser]
    [Produces("application/json")]
    public async Task<IActionResult> SaveDocumentation([FromBody] IImmutableSet<DocumentationElement> documentationElements)
    {
        var result = await documentationService.UpdateDocumentation(documentationElements);

        try
        {
            switch (result)
            {
                case UpdateResult.Success:
                    return Ok();
                case UpdateResult.HasChanged:
                    return Conflict();
                case UpdateResult.Failed:
                    return Problem();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        finally
        {
            Response.OnCompleted(
                async () =>
                {
                    if (result == UpdateResult.Success)
                    {
                        await documentationService.CleanupImages();
                    }
                });
        }
    }

    [HttpPost]
    [AuthenticateUser]
    [Produces("application/json")]
    [Route("{elementId}/image-upload")]
    public async Task<ImmutableList<string>> SaveImages(int elementId, [FromQuery] int? previousImageId = default)
    {
        var files = Request.Form.Files.ToList();

        var fileIds = new List<string>();

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                var uploadedFileId = await documentationService.SaveImage(file.OpenReadStream(), file.FileName);
                fileIds.Add(uploadedFileId);
            }
        }

        return fileIds.ToImmutableList();
    }
}