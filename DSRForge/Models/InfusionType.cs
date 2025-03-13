namespace DSRForge.Models
{
    public class InfusionType
    {

        public readonly int Offset;
        public readonly int MaxUpgrade;
        public readonly bool Limited;

        public InfusionType(int offset, int maxUpgrade, bool limited)
        {
            Offset = offset;
            MaxUpgrade = maxUpgrade;
            Limited = limited;
        }
    }
}