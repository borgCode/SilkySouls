namespace SilkySouls.Models
{
    public class EquippedWeapon
    {
        public string DisplayName { get; set; }
        public int SlotOffset { get; set; }
    
        public EquippedWeapon(string displayName, int slotOffset)
        {
            DisplayName = displayName;
            SlotOffset = slotOffset;
        }
    }
}