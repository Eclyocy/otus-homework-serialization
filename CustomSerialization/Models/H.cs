namespace CustomSerialization.Models
{
    /// <summary>
    /// Test class with fields and properties of complex types.
    /// </summary>
    public record H
    {
        public F complexField;

        public G ComplexProperty { get; protected set; }

        public H(F complexField, G complexProperty)
        {
            this.complexField = complexField;
            this.ComplexProperty = complexProperty;
        }
    }
}
