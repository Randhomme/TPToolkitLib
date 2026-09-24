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
using WF = System.Windows.Forms;
using TPToolkitLib;
using TPToolkitLib.MeshScene.Classes;
using System.IO;

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
            //SelectTPGameFolder();
        }

        private void SelectTPGameFolder()
        {
            var ofbd = new WF.FolderBrowserDialog()
            {
                Description = "Select TPGame folder",
                ShowNewFolderButton = false,
            };
            if (ofbd.ShowDialog() == WF.DialogResult.OK)
            {
                TPGameTool.LoadTPGameFolder(ofbd.SelectedPath);
            }
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
                if (ofd.FilterIndex == 1) // glb
                {
                    MdbTool.XGlbToXMdb(ofd.FileNames, sfbd.SelectedPath);
                }
                else if (ofd.FilterIndex == 2) // obj
                {
                    MdbTool.XObjToXMdb(ofd.FileNames, sfbd.SelectedPath);
                }
            }
        }

        // Open msb
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Multiselect = true,
                DefaultExt = ".obj",
                Filter = "Mesh scene |*.msb",
                Title = "Select msb",
            };
            if (ofd.ShowDialog() == true)
            {
                var msbScene = MsbTool.MeshSceneFromMsbs(ofd.FileNames);
            }
        }

        // Import msb
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Multiselect = true,
                DefaultExt = ".obj",
                Filter = "Mesh scene |*.msb",
                Title = "Select msb",
            };
            if (ofd.ShowDialog() == true)
            {
                var msbScene = new MsbScene();
                MsbTool.ImportMsbSceneFromMsbs(msbScene, ofd.FileNames);
            }
        }

        // Open and Export msb
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Multiselect = true,
                DefaultExt = ".obj",
                Filter = "Mesh scene |*.msb",
                Title = "Select msb",
            };
            var sfd = new SaveFileDialog
            {
                DefaultExt = "msb",
                Filter = "Mesh scene |*.msb",
            };
            if (ofd.ShowDialog() == true && sfd.ShowDialog() == true)
            {
                var msbScene = MsbTool.MeshSceneFromMsbs(ofd.FileNames);
                MsbTool.MeshSceneToMsb(msbScene, sfd.FileName);
            }
        }

        // Open wot
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog()
            {
                Multiselect = true,
                DefaultExt = ".wot",
                Filter = "World object |*.wot",
                Title = "Select wot",
            };
            if (ofd.ShowDialog() == true)
            {
                var wot = WotTool.WorldObjectFromWot(ofd.FileName, false);
            }
        }

        // Open wot folder
        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            var ofbd = new WF.FolderBrowserDialog()
            {
                Description = "Select the wot folder",
                ShowNewFolderButton = false,
            };
            if(ofbd.ShowDialog() == WF.DialogResult.OK)
            {
                var files = Directory.GetFiles(ofbd.SelectedPath, "*.wot");
                for (int i = 0; i < files.Length; i++)
                {
                    var file = files[i];
                    var wot = WotTool.WorldObjectFromWot(file, false);
                }
            }
        }
    }
}