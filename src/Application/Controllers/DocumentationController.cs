using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace KidsTown.Application.Controllers;

[ApiController]
[Route(template: "[controller]")]
public class DocumentationController : ControllerBase
{
    [HttpGet]
    [Produces(contentType: "application/json")]
    public async Task<IImmutableList<DocumentationElement>> GetDocumentation()
    {
        await Task.CompletedTask;

        return ImmutableList.Create(
            new DocumentationElement(
                ElementId: 1,
                PreviousElementId: 0,
                Title: new Title(Text: "Stationstypen", Size: 3),
                Paragraphs: ImmutableList.Create(item: new Paragraph(
                    ParagraphId: 4,
                    PreviousParagraphId: 0,
                    Text:
                    "Das App bietet verschiedene Typen von Station. Wir brauchen die 'Self' Station und die 'Manned' Station.\nBei 'Self' Stationen können Eltern die Label für ihre Kinder selber drucken. 'Manned' Stationen müssen aus Datenschutzgründen immer von Mitarbeitern betreut werden.",
                    Icon: null))),
            new DocumentationElement(
                ElementId: 2,
                PreviousElementId: 1,
                Title: new Title(Text: "Self Station Ablauf", Size: 4),
                Paragraphs: ImmutableList<Paragraph>.Empty),
            new DocumentationElement(
                ElementId: 3,
                PreviousElementId: 2,
                Title: new Title(Text: "Startseite", Size: 5),
                ImageUrl: "self_start.png",
                Paragraphs: ImmutableList.Create(new Paragraph(
                        ParagraphId: 1,
                        PreviousParagraphId: 0,
                        Text:
                        "Paul Muster möchte seine vier Kinder einchecken, also gibt er dafür seine Handynummer ein und tippt auf 'Search!'.",
                        Icon: ParagraphIcon.Action),
                    new Paragraph(
                        ParagraphId: 2,
                        PreviousParagraphId: 1,
                        Text:
                        "Oder Paul Muster hat bereits einen BarCode für sich registriert und tippt deshalb auf den BarCode und hält seinen BarCode in die Kamera.",
                        Icon: ParagraphIcon.Action),
                    new Paragraph(
                        ParagraphId: 3,
                        PreviousParagraphId: 2,
                        Text: "Blub blub blub",
                        Icon: ParagraphIcon.Action),
                    new Paragraph(
                        ParagraphId: 4,
                        PreviousParagraphId: 3,
                        Text:
                        "Das ist die Startseite. Hier kann man über eine beliebige Telefonnummer, die in diesem Haushalt erfasst ist, ober über einen hinterlegten BarCode zum Haushalt gelangen. Telefonnummern können nur über Manned Stationen hinzugefügt oder bearbeitet werden.",
                        Icon: ParagraphIcon.Info))));
    }
}

public record DocumentationElement(
    int ElementId,
    int PreviousElementId,
    Title Title,
    ImmutableList<Paragraph> Paragraphs,
    string? ImageUrl = default);

public record Title(string Text = "", int Size = 5);

public record Paragraph(int ParagraphId, int PreviousParagraphId, string Text, ParagraphIcon? Icon);

public enum ParagraphIcon
{
    Action,
    Info,
    Warning
}