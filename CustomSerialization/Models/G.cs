namespace CustomSerialization.Models
{
    /// <summary>
    /// Test class with fields and properties of different primitive types.
    /// </summary>
    public record G
    {
        public int testField;

        public string TestProperty { get; protected set; }

        public G(int field, string property)
        {
            testField = field;
            TestProperty = property;
        }
    }
}
