using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using DSRForge.ViewModels;
using Xceed.Wpf.Toolkit;

namespace DSRForge.Views
{
    /// <summary>
    /// Interaction logic for PlayerTab.xaml
    /// </summary>
    public partial class PlayerTab
    {
        private readonly PlayerViewModel _playerViewModel;
        
        private readonly Dictionary<string, Action<int?>> _attributeSetters;

        public PlayerTab(PlayerViewModel playerViewModel)
        {
            InitializeComponent();
            _playerViewModel = playerViewModel;
            DataContext = _playerViewModel;
            
            _attributeSetters = new Dictionary<string, Action<int?>>
            {
                { "VitalityUpDown", _playerViewModel.SetVitality },
                { "AttunementUpDown", _playerViewModel.SetAttunement },
                { "EnduranceUpDown", _playerViewModel.SetEndurance },
                { "StrengthUpDown", _playerViewModel.SetStrength },
                { "DexterityUpDown", _playerViewModel.SetDexterity },
                { "ResistanceUpDown", _playerViewModel.SetResistance },
                { "IntelligenceUpDown", _playerViewModel.SetIntelligence },
                { "FaithUpDown", _playerViewModel.SetFaith },
                { "HumanityUpDown", _playerViewModel.SetHumanity },
                { "SoulsUpDown", _playerViewModel.SetSouls },
                { "NewGameUpDown", _playerViewModel.SetNewGame },
                
            };
        }

        private void SetRtsrClick(object sender, RoutedEventArgs e)
        {
            _playerViewModel.SetHp(1);
        }


        private void HealthUpDown_GotFocus(object sender, RoutedEventArgs e)
        {
            _playerViewModel.PauseUpdates();
        }

        private void HealthUpDown_LostFocus(object sender, RoutedEventArgs e)
        {
            if (HealthUpDown.Value.HasValue)
            {
                _playerViewModel.SetHp(HealthUpDown.Value);
            }

            _playerViewModel.ResumeUpdates();
        }

        private void HealthUpDown_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                if (HealthUpDown.Value.HasValue)
                {
                    _playerViewModel.SetHp(HealthUpDown.Value);
                }

                Focus();

                e.Handled = true;
            }
        }

        private void SavePos1_Click(object sender, RoutedEventArgs e)
        {
            _playerViewModel.SavePos(0);
        }

        private void SavePos2_Click(object sender, RoutedEventArgs e)
        {
            _playerViewModel.SavePos(1);
        }

        private void RestorePos1_Click(object sender, RoutedEventArgs e)
        {
            _playerViewModel.RestorePos(0);
        }

        private void RestorePos2_Click(object sender, RoutedEventArgs e)
        {
            _playerViewModel.RestorePos(1);
        }

        private void Restore_Spell_Click(object sender, RoutedEventArgs e)
        {
            _playerViewModel.RestoreSpellCasts();
        }

        private void AttributeUpDown_LostFocus(object sender, RoutedEventArgs e)
        {
            var upDown = sender as IntegerUpDown;
            if (upDown?.Value.HasValue == true && _attributeSetters.TryGetValue(upDown.Name, out var setter))
            {
                setter(upDown.Value);
            }
        }

        private void AttributeUpDown_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Return) return;
    
            var upDown = sender as IntegerUpDown;
            if (upDown?.Value.HasValue == true && _attributeSetters.TryGetValue(upDown.Name, out var setter))
            {
                setter(upDown.Value);
            }
            Focus();
            e.Handled = true;
        }
        
        private void SpeedUpDown_LostFocus(object sender, RoutedEventArgs e)
        {
            var upDown = sender as DoubleUpDown;
            if (upDown?.Value.HasValue == true)
            {
                _playerViewModel.SetSpeed((float?)upDown.Value);
            }
        }

        private void SpeedUpDown_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Return) return;

            var upDown = sender as DoubleUpDown;
            if (upDown?.Value.HasValue == true)
            {
                _playerViewModel.SetSpeed((float?)upDown.Value);
            }
            Focus();
            e.Handled = true;
        }
         
    }
}