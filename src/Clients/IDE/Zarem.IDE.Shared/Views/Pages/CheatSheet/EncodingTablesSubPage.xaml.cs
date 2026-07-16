// Avishai Dernis 2026

using Microsoft.UI.Xaml.Controls;
using System.Linq;
using Zarem.Assembler.Models.Tables;
using Zarem.IDE.ViewModels.Pages.CheatSheet;
using Zarem.Mips.Models.Instructions.Enums.Registers;

namespace Zarem.IDE.Views.Pages.CheatSheet
{
    public sealed partial class EncodingTablesSubPage : UserControl
    {
        public EncodingTablesSubPage()
        {
            this.InitializeComponent();
        }

        public EncodingTablesViewModel? ViewModel { get; set; }

        private void UserControl_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (ViewModel is null)
                return;

            // This is stupid, but whatever for now
            RegisterEncodingTable.CellData = ViewModel.GPRegisters.Select(x => (object)x).ToArray();
        }

        private static string GetRegisterString(MipsGpRegister reg, MipsRegisterSet set) => RegisterTable<MipsGpRegister, MipsRegisterSet>.GetRegisterString(reg, set);
    }
}
