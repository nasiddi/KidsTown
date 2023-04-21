using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using Google.Apis.Upload;
using KidsTown.KidsTown.Models;

namespace KidsTown.KidsTown;

public class DocumentationService : IDocumentationService
{
    private readonly IDocumentationRepository _documentationRepository;
    private readonly DriveService _driveService;

    private readonly string DriveFolderId = "1apAfvOmA3VTbRs10qHvbpAG2KxDTXz_t";


    public DocumentationService(IDocumentationRepository documentationRepository, DriveService driveService)
    {
        _documentationRepository = documentationRepository;
        _driveService = driveService;
    }

    public async Task<IImmutableSet<DocumentationElement>> GetDocumentation(Section section)
    {
        return await _documentationRepository.LoadDocumentation(section);
    }

    public async Task<UpdateResult> UpdateDocumentation(IImmutableSet<DocumentationElement> documentation)
    {
        return await _documentationRepository.UpdateDocumentation(documentation);
    }

    public async Task<string> SaveImage(Stream stream, string fileName)
    {
        var extension = Path.GetExtension(fileName);

        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = $"{Guid.NewGuid().ToString()}{extension}",
            Parents = new List<string> {DriveFolderId}
        };

        var request = _driveService.Files.Create(fileMetadata, stream, $"image/{extension.Substring(1)}");
        request.Fields = "*";
        var results = await request.UploadAsync();

        if (results.Status == UploadStatus.Failed)
        {
            Console.WriteLine($"Error uploading file: {results.Exception.Message}");
        }

        var uploadedFileId = request.ResponseBody?.Id!;
        return uploadedFileId;
    }

    public async Task CleanupImages()
    {
        var imageFileIds = await _documentationRepository.GetAllImageFileIds();
        var listRequest = _driveService.Files.List();
        listRequest.Q = $"'{DriveFolderId}' in parents";
        var fileList = await listRequest.ExecuteAsync();
        
        foreach (var file in fileList.Files)
        {
            if (imageFileIds.Contains(file.Id))
            {
                continue;
            }

            await _driveService.Files.Delete(file.Id).ExecuteAsync();
        }
    }
}