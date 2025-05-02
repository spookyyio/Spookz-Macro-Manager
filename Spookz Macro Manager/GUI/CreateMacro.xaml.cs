using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GUI
{
    /// <summary>
    /// Interaction logic for CreateMacro.xaml
    /// </summary>
    /// 

    public class MacroStep
    {
        public string Action { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }

        public override string ToString() => $"{Action} - {Key ?? Text}";

    }

    public class Macro
    {
        public string Name { get; set; }
        public List<MacroStep> Steps { get; set; } = new List<MacroStep>();
    }
    public partial class CreateMacro : Window
    {
        public CreateMacro()
        {
            InitializeComponent();
        }

        private void TypeBlock_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void action_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        private void StepsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MacroName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Step_DEL_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Movestep_DN_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Movestep_UP_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Save_btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cancelbtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public List<MacroStep> CurrentSteps { get; set; } = new();

        private void addstepbtn_Click(object sender, RoutedEventArgs e)
        {
            var step = new MacroStep
            {
                Action = action.Text
            };
            CurrentSteps.Add(step);
            StepsList.Items.Refresh();
        }
    }

}
