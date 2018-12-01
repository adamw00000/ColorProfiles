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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ColorProfiles
{
    /// <summary>
    /// Interaction logic for ColorProfilePresenter.xaml
    /// </summary>
    public partial class ColorProfilePresenter: UserControl
    {
        public static DependencyProperty SaveCommandProperty = 
            DependencyProperty.Register(nameof(SaveCommand), 
                typeof(ICommand),
                typeof(ColorProfilePresenter));
        
        public ICommand SaveCommand
        {
            get
            {
                MessageBox.Show("bbb");
                return (ICommand)GetValue(SaveCommandProperty);
            }

            set
            {
                MessageBox.Show("ccc");
                SetValue(SaveCommandProperty, value);
            }
        }

        public ColorProfilePresenter()
        {
            InitializeComponent();
        }
    }
}
