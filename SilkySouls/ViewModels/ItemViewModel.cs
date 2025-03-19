using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using SilkySouls.Models;
using SilkySouls.Services;
using SilkySouls.Utilities;

namespace SilkySouls.ViewModels
{
    public class ItemViewModel : BaseViewModel
    {
        private readonly ItemService _itemService;
        private ItemCategory _selectedCategory;
        private Item _selectedItem;
        private int _selectedQuantity = 1;
        private int _selectedUpgrade;
        private string _selectedInfusionType = "Normal";
        private int _maxUpgradeLevel = 15;
        private bool _quantityEnabled;
        private int _maxQuantity;
        private bool _canUpgrade;
        private bool _canInfuse;

        private bool _areOptionsEnabled;

        private string _searchText = string.Empty;

        private readonly Dictionary<string, ObservableCollection<Item>> _itemsByCategory;
        private readonly Dictionary<string, InfusionType> _infusionTypes = new Dictionary<string, InfusionType>();

        private ObservableCollection<ItemCategory> _categories;
        private ObservableCollection<Item> _items;
        private ObservableCollection<string> _availableInfusions;


        public ItemViewModel(ItemService itemService)
        {
            _itemService = itemService;

            _categories = new ObservableCollection<ItemCategory>();
            _items = new ObservableCollection<Item>();
            _availableInfusions = new ObservableCollection<string>();
            _itemsByCategory = new Dictionary<string, ObservableCollection<Item>>();

            LoadData();
            InitInfusions();
        }

        private void InitInfusions()
        {
            _infusionTypes.Add("Normal", new InfusionType(0, 15, false));
            _infusionTypes.Add("Chaos", new InfusionType(900, 5, true));
            _infusionTypes.Add("Crystal", new InfusionType(100, 5, false));
            _infusionTypes.Add("Divine", new InfusionType(600, 10, false));
            _infusionTypes.Add("Enchanted", new InfusionType(500, 5, true));
            _infusionTypes.Add("Fire", new InfusionType(800, 10, false));
            _infusionTypes.Add("Lightning", new InfusionType(200, 5, false));
            _infusionTypes.Add("Magic", new InfusionType(400, 10, false));
            _infusionTypes.Add("Occult", new InfusionType(700, 5, true));
            _infusionTypes.Add("Raw", new InfusionType(300, 5, true));
        }


        private void LoadData()
        {
            Categories.Add(new ItemCategory(0x00000000, "Ammo"));
            Categories.Add(new ItemCategory(0x10000000, "Armor"));
            Categories.Add(new ItemCategory(0x40000000, "Consumables"));
            Categories.Add(new ItemCategory(0x40000000, "Infinite Use Items"));
            Categories.Add(new ItemCategory(0x40000000, "Key Items"));
            Categories.Add(new ItemCategory(0x20000000, "Rings"));
            Categories.Add(new ItemCategory(0x40000000, "Spells"));
            Categories.Add(new ItemCategory(0x40000000, "Upgrade Materials"));
            Categories.Add(new ItemCategory(0x00000000, "Weapons"));

            _itemsByCategory.Add("Ammo", new ObservableCollection<Item>(DataLoader.GetItemList("Ammo")));
            _itemsByCategory.Add("Armor", new ObservableCollection<Item>(DataLoader.GetItemList("Armor")));
            _itemsByCategory.Add("Consumables", new ObservableCollection<Item>(DataLoader.GetItemList("Consumables")));
            _itemsByCategory.Add("Infinite Use Items",
                new ObservableCollection<Item>(DataLoader.GetItemList("InfiniteUseItems")));
            _itemsByCategory.Add("Key Items", new ObservableCollection<Item>(DataLoader.GetItemList("KeyItems")));
            _itemsByCategory.Add("Rings", new ObservableCollection<Item>(DataLoader.GetItemList("Rings")));
            _itemsByCategory.Add("Spells", new ObservableCollection<Item>(DataLoader.GetItemList("Spells")));
            _itemsByCategory.Add("Upgrade Materials",
                new ObservableCollection<Item>(DataLoader.GetItemList("UpgradeMaterials")));
            _itemsByCategory.Add("Weapons", new ObservableCollection<Item>(DataLoader.GetItemList("Weapons")));

            SelectedCategory = Categories.FirstOrDefault();
        }

        public bool AreOptionsEnabled
        {
            get => _areOptionsEnabled;
            set => SetProperty(ref _areOptionsEnabled, value);
        }

        public ObservableCollection<ItemCategory> Categories
        {
            get => _categories;
            private set => SetProperty(ref _categories, value);
        }

        public ObservableCollection<Item> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public ObservableCollection<string> AvailableInfusions
        {
            get => _availableInfusions;
            private set => SetProperty(ref _availableInfusions, value);
        }


        public ItemCategory SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value) && value != null)
                {
                    if (_selectedCategory != null)
                    {
                        Items = _itemsByCategory[_selectedCategory.Name];
                        SelectedItem = Items.FirstOrDefault();

                        if (!string.IsNullOrEmpty(SearchText))
                        {
                            ApplyFilter();
                        }
                    }
                }
            }
        }

        public bool CanUpgrade
        {
            get => _canUpgrade;
            private set => SetProperty(ref _canUpgrade, value);
        }

        public bool CanInfuse
        {
            get => _canInfuse;
            private set => SetProperty(ref _canInfuse, value);
        }

        public int MaxUpgradeLevel
        {
            get => _maxUpgradeLevel;
            private set => SetProperty(ref _maxUpgradeLevel, value);
        }

        public bool QuantityEnabled
        {
            get => _quantityEnabled;
            private set => SetProperty(ref _quantityEnabled, value);
        }

        public int MaxQuantity
        {
            get => _maxQuantity;
            private set => SetProperty(ref _maxQuantity, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilter();
                }
            }
        }

        private void ApplyFilter()
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(Items);

            if (string.IsNullOrEmpty(SearchText))
            {
                view.Filter = null;
            }
            else
            {
                view.Filter = item => ((Item)item).Name.ToLower().Contains(SearchText.ToLower());
            }
        }

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                if (_selectedItem != null)
                {
                    if (_selectedCategory.Name == "Spells")
                    {
                        QuantityEnabled = true;
                        MaxQuantity = _selectedItem.StackSize;
                        SelectedQuantity = 1;
                    }
                    else if (_selectedItem.StackSize > 1)
                    {
                        QuantityEnabled = true;
                        MaxQuantity = _selectedItem.StackSize;
                        SelectedQuantity = _selectedItem.StackSize;
                    }
                    else
                    {
                        QuantityEnabled = false;
                        MaxQuantity = 1;
                        SelectedQuantity = 1;
                    }

                    var newInfusions = new ObservableCollection<string>();

                    switch (_selectedItem.UpgradeType)
                    {
                        case UpgradeType.None:
                            CanUpgrade = false;
                            CanInfuse = false;
                            SelectedUpgrade = 0;
                            SelectedInfusionType = "Normal";
                            break;
                        case UpgradeType.Special:
                            CanUpgrade = true;
                            CanInfuse = false;
                            SelectedInfusionType = "Normal";
                            MaxUpgradeLevel = 5;
                            break;
                        case UpgradeType.Infusable:
                            CanUpgrade = true;
                            CanInfuse = true;
                            foreach (var key in _infusionTypes.Keys)
                            {
                                newInfusions.Add(key);
                            }

                            AvailableInfusions = newInfusions;
                            if (!newInfusions.Contains(SelectedInfusionType))
                                SelectedInfusionType = "Normal";

                            MaxUpgradeLevel = _infusionTypes[SelectedInfusionType].MaxUpgrade;
                            break;
                        case UpgradeType.InfusableLimited:
                            CanUpgrade = true;
                            CanInfuse = true;
                            foreach (var kvp in _infusionTypes)
                            {
                                if (kvp.Value.Limited) continue;
                                newInfusions.Add(kvp.Key);
                            }

                            AvailableInfusions = newInfusions;

                            if (!newInfusions.Contains(SelectedInfusionType))
                                SelectedInfusionType = "Normal";

                            MaxUpgradeLevel = _infusionTypes[SelectedInfusionType].MaxUpgrade;
                            break;
                        case UpgradeType.PyromancyFlame:
                            CanUpgrade = true;
                            CanInfuse = false;
                            SelectedInfusionType = "Normal";
                            MaxUpgradeLevel = 15;
                            break;
                        case UpgradeType.PyromancyAscended:
                            CanUpgrade = true;
                            CanInfuse = false;
                            SelectedInfusionType = "Normal";
                            MaxUpgradeLevel = 5;
                            break;
                    }
                }
            }
        }

        public int SelectedQuantity
        {
            get => _selectedQuantity;
            set
            {
                int clampedValue = Math.Max(1, Math.Min(value, MaxQuantity));
                SetProperty(ref _selectedQuantity, clampedValue);
            }
        }

        public int SelectedUpgrade
        {
            get => _selectedUpgrade;
            set => SetProperty(ref _selectedUpgrade, Math.Max(0, Math.Min(value, MaxUpgradeLevel)));
        }

        public string SelectedInfusionType
        {
            get => _selectedInfusionType;
            set
            {
                if (SetProperty(ref _selectedInfusionType, value) && value != null)
                {
                    MaxUpgradeLevel = _infusionTypes[value].MaxUpgrade;

                    if (SelectedUpgrade > MaxUpgradeLevel)
                        SelectedUpgrade = MaxUpgradeLevel;
                }
            }
        }

        public void SpawnItem()
        {
            if (_selectedItem == null) return;


            int itemId = _selectedItem.Id;
            if (CanInfuse)
            {
                itemId += _infusionTypes[SelectedInfusionType].Offset;
            }

            if (CanUpgrade)
            {
                itemId += SelectedUpgrade;
            }

            _itemService.ItemSpawn(
                itemId,
                SelectedCategory.Id,
                SelectedQuantity);
        }

        public void DisableButtons()
        {
            AreOptionsEnabled = false;
            _itemService.UninstallHook();
        }

        public void TryEnableActiveOptions()
        {
            AreOptionsEnabled = true;
        }
    }
}