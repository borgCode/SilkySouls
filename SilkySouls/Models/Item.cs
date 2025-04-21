namespace SilkySouls.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public UpgradeType UpgradeType { get; set; }
        public int StackSize { get; set; }
        public string CategoryName { get; set; }
        
        public Item(int id, int stackSize, UpgradeType upgradeType, string name)
        {
            Id = id;
            Name = name;
            StackSize = stackSize;
            UpgradeType = upgradeType;
        }

    }
}