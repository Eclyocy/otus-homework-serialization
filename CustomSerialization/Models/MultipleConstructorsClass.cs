namespace CustomSerialization.Models
{
    /// <summary>
    /// Test class with multiple constructors.
    /// </summary>
    public record MultipleConstructorsClass
    {
        public double testField;

        public string? TestProperty { get; protected set; }

        public MultipleConstructorsClass()
        {
        }

        public MultipleConstructorsClass(double field)
        {
            testField = field;
        }

        public MultipleConstructorsClass(string? property)
        {
            TestProperty = property;
        }

        public MultipleConstructorsClass(
            double field,
            string? property)
        {
            testField = field;
            TestProperty = property;
        }
    }
}
