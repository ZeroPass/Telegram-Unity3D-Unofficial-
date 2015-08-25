using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Net;

namespace TelegramMTProtoTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MTProto pMTProto = null;
        Authorization pAuthorization = null;
        public MainWindow()
        {
            InitializeComponent();
            pMTProto = new MTProto();
        }

        void SimpleHttp()
        {
            var url = "https://vesta.web.telegram.org/apiw_test1/"; // "http://149.154.167.40/apiw1";
            var data = "(auth.sendCode \"+38631392561\" 0 41190 \"c47d79fbf23aa077d6c01e328a69cb4a\" \"en\")";
            using (var wc = new WebClient())
            {
                var result = wc.UploadData(url, Encoding.ASCII.GetBytes(data));
            }  
        }
    }
}
