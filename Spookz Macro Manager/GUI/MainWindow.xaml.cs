using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq.Expressions;
using Gma.System.MouseKeyHook;
using WindowsInput;
using WindowsInput.Native;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {
        private IKeyboardMouseEvents _globalHook;
        private List<Macro> _macros;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly InputSimulator _inputSimulator = new InputSimulator();

        public MainWindow()
        {
            InitializeComponent();
            PopulateMacroList();
            LoadMacros();
            SetupGlobalKeyHook();
        }

        private void LoadMacros()
        {
            string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "macros.xml");
            if (File.Exists(filePath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Macro>));
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    _macros = (List<Macro>)serializer.Deserialize(fs) ?? new List<Macro>();
                }
            }
            else
            {
                _macros = new List<Macro>();
            }
        }

        private void SetupGlobalKeyHook()
        {
            _globalHook = Hook.GlobalEvents();
            _globalHook.KeyDown += GlobalHook_KeyDown;
        }


        private void GlobalHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            string pressedKey = "";

            // Add modifiers to the key text
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
                pressedKey += "Ctrl+";
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
                pressedKey += "Shift+";
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
                pressedKey += "Alt+";

            // Extract the primary key and normalize numeric keys (e.g., D3 -> 3)
            string keyData = e.KeyCode.ToString();

            // Handle special case for 'Packet' key
            if (e.KeyCode == System.Windows.Forms.Keys.Packet)
            {
                Logger.Warn("Packet key detected. Ignoring this key press.");
                return; // Ignore the 'Packet' key and stop further processing
            }

            if (keyData.StartsWith("D") && keyData.Length == 2 && char.IsDigit(keyData[1]))
            {
                keyData = keyData[1].ToString(); // Strip the 'D' prefix
            }

            // Append the normalized key to the pressedKey string
            pressedKey += keyData;

            // Log the pressed key for debugging
            Logger.Info($"Normalized key pressed: {pressedKey}");

            // Check if the pressed key matches any MacroKey
            var matchingMacro = _macros.FirstOrDefault(m => m.MacroKey == pressedKey);
            if (matchingMacro != null)
            {
                // Log the macro trigger
                Logger.Info($"Macro triggered: {matchingMacro.Name}");

                ExecuteMacro(matchingMacro);
            }
            else
            {
                Logger.Info($"No macro matched for key: {pressedKey}");
            }
        }

        private void ExecuteMacro(Macro macro)
        {
            Logger.Info($"Executing macro: {macro.Name}");

            foreach (var step in macro.Steps)
            {
                if (step.Action == "Press Key" && !string.IsNullOrEmpty(step.Key))
                {
                    Logger.Info($"Simulating key press: {step.Key}");
                    SimulateKeyPress(step.Key);
                }
                else if (step.Action == "Type" && !string.IsNullOrEmpty(step.Text))
                {
                    Logger.Info($"Simulating typing: {step.Text}");
                    SimulateTyping(step.Text);
                }
            }
        }

        private void SimulateKeyPress(string key)
        {
            try
            {
                // Map the key string to a VirtualKeyCode
                if (Enum.TryParse(typeof(VirtualKeyCode), key, true, out var virtualKeyCode))
                {
                    Logger.Info($"Attempting to simulate key press: {key} (VirtualKeyCode: {virtualKeyCode})");

                    // Simulate the key press
                    _inputSimulator.Keyboard.KeyPress((VirtualKeyCode)virtualKeyCode);
                    Logger.Info($"Simulated key press: {key}");
                }
                else
                {
                    Logger.Warn($"Invalid key: {key}. Could not simulate key press.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error simulating key press for key: {key}. Exception: {ex.Message}");
            }
        }

        private void SimulateTyping(string text)
        {
            try
            {
                Logger.Info($"Attempting to simulate typing: {text}");

                // Simulate typing the text
                _inputSimulator.Keyboard.TextEntry(text);
                Logger.Info($"Simulated typing: {text}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error simulating typing for text: {text}. Exception: {ex.Message}");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _globalHook.KeyDown -= GlobalHook_KeyDown;
            _globalHook.Dispose();
        }
        
        private void PopulateMacroList()
        {
            // Define the path to the macros.xml file
            string filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "macros.xml");

            // Initialize an empty list of macros
            List<Macro> macros = new List<Macro>();

            // Check if the file exists
            if (File.Exists(filePath))
            {
                try
                {
                    // Deserialize the macros from the XML file
                    XmlSerializer serializer = new XmlSerializer(typeof(List<Macro>));
                    using (FileStream fs = new FileStream(filePath, FileMode.Open))
                    {
                        macros = (List<Macro>)serializer.Deserialize(fs) ?? new List<Macro>();
                    }
                }
                catch (Exception ex)
                {
                    // Log or display an error if deserialization fails
                    MessageBox.Show($"Failed to load macros: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // Bind the list of macros to the macrolist control
            macrolist.ItemsSource = macros;
        }

        private void addmacro_Click(object sender, RoutedEventArgs e)
        {
            CreateMacro createMacro = new CreateMacro();
            createMacro.MacroSaved += PopulateMacroList;
            createMacro.ShowDialog();
        }

        private void macrolist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void deletemacro_Click(object sender, RoutedEventArgs e)
        {

        }

        private void editmacro_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}