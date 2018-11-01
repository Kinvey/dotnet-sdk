using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Kinvey.TestApp.Shared.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class BasePage : ContentPage
	{
		public BasePage()
		{
			InitializeComponent();
		}

	    protected async Task DisplayMessage(string title, string message)
	    {
	        await DisplayAlert(title, message, "OK");
        }
	}
}