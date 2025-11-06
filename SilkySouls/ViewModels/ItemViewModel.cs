using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        private bool _autoSpawnEnabled;
        private Item _selectedAutoSpawnWeapon;

        private string _preSearchCategory;
        private bool _isSearchActive;
        private readonly ObservableCollection<Item> _searchResultsCollection = new ObservableCollection<Item>();

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
            SelectedMassSpawnCategory = Categories.FirstOrDefault().Name;
            SelectedAutoSpawnWeapon = _itemsByCategory["Weapons"].FirstOrDefault();
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
                if (!SetProperty(ref _selectedCategory, value) || value == null) return;
                if (_selectedCategory == null) return;
                if (_isSearchActive)
                {
                    IsSearchActive = false;
                    _searchText = string.Empty;
                    OnPropertyChanged(nameof(SearchText));
                    _preSearchCategory = null;
                }

                Items = _itemsByCategory[_selectedCategory.Name];
                SelectedItem = Items.FirstOrDefault();
                SelectedMassSpawnCategory = SelectedCategory.Name;
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

        public bool IsSearchActive
        {
            get => _isSearchActive;
            private set => SetProperty(ref _isSearchActive, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (!SetProperty(ref _searchText, value))
                {
                    return;
                }

                if (string.IsNullOrEmpty(value))
                {
                    _isSearchActive = false;

                    if (_preSearchCategory != null)
                    {
                        _selectedCategory.Name = _preSearchCategory;
                        Items = _itemsByCategory[_selectedCategory.Name];
                        SelectedItem = Items.FirstOrDefault();
                        _preSearchCategory = null;
                    }
                }
                else
                {
                    if (!_isSearchActive)
                    {
                        _preSearchCategory = SelectedCategory.Name;
                        _isSearchActive = true;
                    }

                    ApplyFilter();
                }
            }
        }

        private void ApplyFilter()
        {
            _searchResultsCollection.Clear();
            var searchTextLower = SearchText.ToLower();

            foreach (var category in _itemsByCategory)
            {
                foreach (var item in category.Value)
                {
                    if (item.Name.ToLower().Contains(searchTextLower))
                    {
                        item.CategoryName = category.Key;
                        _searchResultsCollection.Add(item);
                    }
                }
            }

            Items = _searchResultsCollection;
        }

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                if (_selectedItem == null) return;
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
            int categoryId;
            if (_isSearchActive && _selectedItem.CategoryName != null)
            {
                var correctCategory = Categories.FirstOrDefault(c => c.Name == _selectedItem.CategoryName);
                categoryId = correctCategory?.Id ?? _selectedCategory.Id;
            }
            else
            {
                categoryId = _selectedCategory.Id;
            }
            
            if (CanInfuse)
            {
                itemId += _infusionTypes[SelectedInfusionType].Offset;
            }

            if (CanUpgrade && SelectedUpgrade > 0)
            {
                if (SelectedItem.UpgradeType == UpgradeType.PyromancyFlame ||
                    SelectedItem.UpgradeType == UpgradeType.PyromancyAscended)
                {
                    itemId += SelectedUpgrade * 100;
                }
                else
                {
                    itemId += SelectedUpgrade;
                }
            }

            _itemService.ItemSpawn(
                itemId,
                categoryId,
                SelectedQuantity);
        }

        public bool AutoSpawnEnabled
        {
            get => _autoSpawnEnabled;
            set => SetProperty(ref _autoSpawnEnabled, value);
        }

        public Item SelectedAutoSpawnWeapon
        {
            get => _selectedAutoSpawnWeapon;
            set => SetProperty(ref _selectedAutoSpawnWeapon, value);
        }

        public ObservableCollection<Item> WeaponList => new ObservableCollection<Item>(_itemsByCategory["Weapons"]);

        public void DisableButtons()
        {
            AreOptionsEnabled = false;
        }

        public void TryEnableActiveOptions()
        {
            AreOptionsEnabled = true;
        }

        public void TrySpawnWeaponPref()
        {
            if (AutoSpawnEnabled && SelectedAutoSpawnWeapon != null)
            {
                int itemId = SelectedAutoSpawnWeapon.Id;

                _itemService.ItemSpawn(
                    itemId,
                    0x00000000,
                    1);
            }
        }

        private string _selectedMassSpawnCategory;

        public string SelectedMassSpawnCategory
        {
            get => _selectedMassSpawnCategory;
            set => SetProperty(ref _selectedMassSpawnCategory, value);
        }

        public void MassSpawn()
        {
            Task.Run(() =>
            {
                if (SelectedMassSpawnCategory == "Weapons")
                {
                    foreach (var weapon in _itemsByCategory[SelectedMassSpawnCategory])
                    {
                        int itemId = weapon.Id;
                        _itemService.ItemSpawn(itemId, 0x00000000, 1);
                        Thread.Sleep(10);
                    }
                }
                else
                {
                    foreach (var item in _itemsByCategory[SelectedMassSpawnCategory])
                    {
                        int itemId = item.Id;
                        ItemCategory category = Categories.First(c => c.Name == SelectedMassSpawnCategory);
                        _itemService.ItemSpawn(itemId, category.Id, item.StackSize);
                        Thread.Sleep(10);
                    }
                }
            });
        }
    }
}