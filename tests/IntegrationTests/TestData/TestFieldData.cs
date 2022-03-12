using KidsTown.Shared;

namespace KidsTown.IntegrationTests.TestData;

public class TestFieldData
{
    public readonly PeopleFieldId FieldDefinitionId;
    public readonly long FieldOptionId;
    public readonly string? Value;

    public TestFieldData(PeopleFieldId fieldDefinitionId, long fieldOptionId, string? value)
    {
        FieldDefinitionId = fieldDefinitionId;
        FieldOptionId = fieldOptionId;
        Value = value;
    }
}