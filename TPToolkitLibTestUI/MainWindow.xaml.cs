using Microsoft.Win32;
using System.Globalization;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WF = System.Windows.Forms;
using TPToolkitLib;

namespace TPToolkitLibUITest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            InitializeComponent();
            
        }

        // X mdb to 1 3d file
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Multiselect = true,
                DefaultExt = ".mdb",
                Filter = "Mesh (.mdb)|*.mdb",
                Title = "Select meshes",
            };
            var sfd = new SaveFileDialog
            {
                DefaultExt = "glb",
                Filter = "GL Transmission Format Binary |*.glb|Object file |*.obj",
            };
            if (ofd.ShowDialog() == true && sfd.ShowDialog() == true)
            {
                if (sfd.FilterIndex == 1) // glb
                {
                    MdbTool.XMdbTo1Glb(ofd.FileNames, sfd.FileName, "C:\\Users\\User0\\Documents\\Desktop\\TreasurePlanet\\TP_Game_ORIGINAL\\BD_Textures", true);
                }
                else // obj
                {
                    MdbTool.XMdbTo1Obj(ofd.FileNames, sfd.FileName, "C:\\Users\\User0\\Documents\\Desktop\\TreasurePlanet\\TP_Game_ORIGINAL\\BD_Textures", true);
                }
            }
        }

        // X mdb to X obj
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Multiselect = true,
                DefaultExt = ".mdb",
                Filter = "Mesh (.mdb)|*.mdb",
                Title = "Select meshes",
            };
            var sfbd = new WF.FolderBrowserDialog()
            {
                Description = "Select the TPGame folder",
                ShowNewFolderButton = false,
            };
            if (ofd.ShowDialog() == true && sfbd.ShowDialog() == WF.DialogResult.OK)
            {
                MdbTool.XMdbToXObj(ofd.FileNames, sfbd.SelectedPath, "C:\\Users\\User0\\Documents\\Desktop\\TreasurePlanet\\TP_Game_ORIGINAL\\BD_Textures", true);
            }
        }

        // X mdb to X glb
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Multiselect = true,
                DefaultExt = ".mdb",
                Filter = "Mesh (.mdb)|*.mdb",
                Title = "Select meshes",
            };
            var sfbd = new WF.FolderBrowserDialog()
            {
                Description = "Select a folder to export",
                ShowNewFolderButton = false,
            };
            if (ofd.ShowDialog() == true && sfbd.ShowDialog() == WF.DialogResult.OK)
            {
                MdbTool.XMdbToXGlb(ofd.FileNames, sfbd.SelectedPath, "C:\\Users\\User0\\Documents\\Desktop\\TreasurePlanet\\TP_Game_ORIGINAL\\BD_Textures", true);
            }
        }

        // X obj to X mdb
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Multiselect = true,
                DefaultExt = ".obj",
                Filter = "GL Transmission Format Binary |*.glb|Object file |*.obj",
                Title = "Select objs",
            };
            var sfbd = new WF.FolderBrowserDialog()
            {
                Description = "Select a folder to export",
                ShowNewFolderButton = false,
            };
            if (ofd.ShowDialog() == true && sfbd.ShowDialog() == WF.DialogResult.OK)
            {
                if (ofd.FilterIndex == 0) // glb
                {
                    MdbTool.XGlbToXMdb(ofd.FileNames, sfbd.SelectedPath);
                }
                else if (ofd.FilterIndex == 1) // obj
                {
                    MdbTool.XObjToXMdb(ofd.FileNames, sfbd.SelectedPath);
                }
            }
        }
    }
}