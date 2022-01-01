using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace QuickNote {
  internal class Utils {

    public static async void ShowYesMessageBox(string message) {
      MessageDialog dialog = new MessageDialog(message);
      dialog.Commands.Clear();
      dialog.Commands.Add(new UICommand { Label = "Yes", Id = 0 });
      await dialog.ShowAsync();
    }
    public static async Task<bool> ShowYesNoMessageBox(string message) {
      MessageDialog dialog = new MessageDialog(message);
      dialog.Commands.Clear();
      dialog.Commands.Add(new UICommand { Label = "Yes", Id = 0 });
      dialog.Commands.Add(new UICommand { Label = "No", Id = 1 });
      var res = await dialog.ShowAsync();

      return (int)res.Id == 0;
    }


  }
}
