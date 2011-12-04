using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;

namespace codesomnia.CapidemaBarcodes.Views
{
    /// <summary>
    /// Interaction logic for BarcodeManagerView.xaml
    /// </summary>
    public partial class BarcodeManagerView : UserControl
    {
        public BarcodeManagerView()
        {
            InitializeComponent();

            ProductCode.GotKeyboardFocus += GotFocus;
            EanCode.GotKeyboardFocus += GotFocus;
            Quantity.GotKeyboardFocus += GotFocus;

            ProductCode.FocusOnEnter(EanCode);
            EanCode.FocusOnEnter(Quantity);
            Quantity.FocusOnEnter(AddBarcode);
            AddBarcode.FocusOnEnter(ProductCode);

            AddBarcode.KeyUp += (sender, args) =>
                                  {
                                      if (args.Key == Key.Return || args.Key == Key.Enter)
                                      {
                                          ProductCode.Focus();
                                          var peer = new ButtonAutomationPeer(sender as Button);

                                          var invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;

                                          invokeProv.Invoke();
                                      }
                                  };

        }

        void GotFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }
    }
}
