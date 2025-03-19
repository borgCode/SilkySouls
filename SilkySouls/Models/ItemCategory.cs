using System.Collections.Generic;

namespace SilkySouls.Models
{
    public class ItemCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ItemCategory(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}