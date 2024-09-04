namespace CustomSerialization.Models
{
    /// <summary>
    /// Base test class as suggested in the task description.
    /// Five integer fields.
    /// </summary>
    public record F
    {
        public int i1, i2, i3, i4, i5;

        public F(int i1, int i2, int i3, int i4, int i5)
        {
            this.i1 = i1;
            this.i2 = i2;
            this.i3 = i3;
            this.i4 = i4;
            this.i5 = i5;
        }
    }
}
