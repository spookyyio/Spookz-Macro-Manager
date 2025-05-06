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
using System.IO;
using System.Xml.Serialization;
using System.Security.Cryptography.X509Certificates;

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

        public override string ToString()
        {
            if (Action == "Press Key" && !string.IsNullOrEmpty(Key))
            {
                return $"{Action} - {Key}";
            }
            else if (Action == "Type" && !string.IsNullOrEmpty(Text))
            {
                return $"{Action} - {Text}";
            }
            return Action;
        }

    }

    public class Macro
    {
        public string Name { get; set; }
        [XmlElement("MacroKey")]
        public string MacroKey {  get; set; }
        public List<MacroStep> Steps { get; set; } = new List<MacroStep>();
    }
    public partial class CreateMacro : Window
    {
        public event Action MacroSaved;
        public CreateMacro()
        {
            InitializeComponent();
            StepsList.ItemsSource = CurrentSteps;
         
        }

        private void TypeBlock_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void action_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (action.SelectedItem is ComboBoxItem selectedItem && selectedItem.Content != null)
            {
                string selectedAction = selectedItem.Content.ToString()!;
                TypeBlock.IsEnabled = selectedAction == "Type";
                keyblock.IsEnabled = selectedAction == "Press Key";
            }
            else
            {
                TypeBlock.IsEnabled = false;
                keyblock.IsEnabled = false;
            }
        }

        private void StepsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MacroName_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Text = string.Empty;
            tb.GotFocus -= MacroName_GotFocus;
        }
        private void MacroName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Step_DEL_Click(object sender, RoutedEventArgs e)
        {
            if (StepsList.SelectedIndex >= 0 && StepsList.SelectedIndex < CurrentSteps.Count)
            {
                CurrentSteps.RemoveAt(StepsList.SelectedIndex);
                StepsList.Items.Refresh();
            }
        }

        private void Movestep_DN_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = StepsList.SelectedIndex;

            if (selectedIndex >= 0 && selectedIndex < CurrentSteps.Count - 1)
            {
                var temp = CurrentSteps[selectedIndex];
                CurrentSteps[selectedIndex] = CurrentSteps[selectedIndex + 1];
                CurrentSteps[selectedIndex + 1] = temp;

                StepsList.Items.Refresh();
                StepsList.SelectedIndex = selectedIndex + 1;
            }
        }

        private void Movestep_UP_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = StepsList.SelectedIndex;
            if (selectedIndex > 0 && selectedIndex < CurrentSteps.Count)
            {
                var temp = CurrentSteps[selectedIndex];
                CurrentSteps[selectedIndex] = CurrentSteps[selectedIndex - 1];
                CurrentSteps[selectedIndex - 1] = temp;

                StepsList.Items.Refresh();
                StepsList.SelectedIndex = selectedIndex - 1;
            }
        }

        private void Save_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Define the path to the macros.xml file in the application's base directory  
                string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "macros.xml");

                // Log the file path for debugging  
                MessageBox.Show($"Saving to: {filePath}", "Debug", MessageBoxButton.OK, MessageBoxImage.Information);

                // Create a new Macro object  
                var macro = new Macro
                {
                    Name = MacroName.Text,
                    MacroKey = macrokeyblock.Text, // Set the MacroKey property
                    Steps = CurrentSteps,
                };

                // Optionally include the macroKey in the Name for display purposes
                if (!string.IsNullOrEmpty(macro.MacroKey))
                {
                    macro.Name += $" (Key: {macro.MacroKey})";
                }

                // Serialize the macro to XML  
                XmlSerializer serializer = new XmlSerializer(typeof(List<Macro>));
                List<Macro> macros;

                // Check if the file exists  
                if (File.Exists(filePath))
                {
                    // Read existing macros  
                    using (FileStream fs = new FileStream(filePath, FileMode.Open))
                    {
                        macros = (List<Macro>)serializer.Deserialize(fs) ?? new List<Macro>();
                    }
                }
                else
                {
                    // Initialize a new list if the file doesn't exist  
                    macros = new List<Macro>();
                }

                // Add the new macro  
                macros.Add(macro);

                // Write the updated list back to the file  
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    serializer.Serialize(fs, macros);
                }

                // Notify the user and close the window  
                MessageBox.Show("Macro saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                MacroSaved?.Invoke();
                this.Close();
            }
            catch (Exception ex)
            {
                // Log the exception for debugging  
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                Action = action.Text,
                Key = keyblock.IsEnabled ? keyblock.Text : null,
                Text = TypeBlock.IsEnabled ? TypeBlock.Text : null
            };

            CurrentSteps.Add(step);
            StepsList.Items.Refresh();
        }

        private void keyblock_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void macrokeyblock_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            string macrokeyText = "";

            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
                macrokeyText += "Ctrl+";
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
                macrokeyText += "Shift+";
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
                macrokeyText += "Alt+";

            Key key = (e.Key == Key.System) ? e.SystemKey : e.Key;

            if (key >= Key.D0 && key <= Key.D9)
            {
                macrokeyText += (key - Key.D0).ToString();
            }
            else
            {
                macrokeyText += key.ToString();
            }

            macrokeyblock.Text = macrokeyText;
        }

        private void keyblock_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Check if the key is a modifier or a special key
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                e.Key == Key.LeftShift || e.Key == Key.RightShift ||
                e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
                e.Key == Key.System)
            {
                e.Handled = true; // Prevent default behavior for modifiers
            }
            else
            {
                string keyText = "";

                // Add modifiers to the key text
                if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
                    keyText += "Ctrl+";
                if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
                    keyText += "Shift+";
                if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
                    keyText += "Alt+";

                // Get the actual key pressed
                Key key = (e.Key == Key.System) ? e.SystemKey : e.Key;

                // Handle numeric keys
                if (key >= Key.D0 && key <= Key.D9)
                {
                    keyText += (key - Key.D0).ToString();
                }
                else
                {
                    keyText += key.ToString();
                }

                // Update the TextBox with the key combination
                keyblock.Text = keyText;

                e.Handled = true; // Prevent default behavior for this key
            }
        }

        public void TypeBlock_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Text = string.Empty;
            tb.GotFocus -= TypeBlock_GotFocus;
        }

        private void macrokeyblock_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }

}
