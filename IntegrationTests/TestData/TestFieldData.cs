namespace IntegrationTests.TestData
{
    public class TestFieldData
    {
        public readonly TestFieldIds FieldDefinitionId;
        public readonly long FieldOptionId;
        public readonly string? Value;

        public TestFieldData(TestFieldIds fieldDefinitionId, long fieldOptionId, string? value)
        {
            FieldDefinitionId = fieldDefinitionId;
            FieldOptionId = fieldOptionId;
            Value = value;
        }
    }
}