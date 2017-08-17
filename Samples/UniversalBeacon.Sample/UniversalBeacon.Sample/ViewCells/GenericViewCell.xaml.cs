using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace UniversalBeacon.Sample.ViewCells
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class GenericViewCell : ViewCell
	{
		public GenericViewCell ()
		{
			InitializeComponent ();
		}
	}
}