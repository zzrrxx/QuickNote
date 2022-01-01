using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QuickNote
{
  /// <summary>
  /// An empty page that can be used on its own or navigated to within a Frame.
  /// </summary>
  public sealed partial class NewNotePage : Page {
    public NewNotePage() {
      this.InitializeComponent();
    }

    private async void btnCancel_Click(object sender, RoutedEventArgs e) {

      bool chooseYes = await Utils.ShowYesNoMessageBox("Do you want to discard the current note?");
      if (!chooseYes) { return; }

      Frame.Navigate(typeof(MainPage));
    }

    private void btnAdd_Click(object sender, RoutedEventArgs e) {
      string name = textBoxName.Text.Trim();
      string keywords = textBoxKeywords.Text.Trim();
      string content = textBoxContent.Text;

      if (name.Length == 0) {
        Utils.ShowYesMessageBox("Name cannot be empty");
        return;
      }
      if (keywords.Length == 0) {
        Utils.ShowYesMessageBox("Keywords cannot be empty");
        return;
      }
      if (content.Length == 0) {
        Utils.ShowYesMessageBox("Keywords cannot be empty");
        return;
      }

      string[] keywordsArr = keywords.Split(",", StringSplitOptions.RemoveEmptyEntries);
      Note note = new Note(name, keywordsArr, content);
      try {
        NoteManager.Instance.AddNote(note);
      } catch (Exception ex) {
        Utils.ShowYesMessageBox("Failed to add the new note: " + ex.Message);
        return;
      }

      Frame.Navigate(typeof(MainPage));
    }

    private void textBoxContent_TextChanged(object sender, TextChangedEventArgs e) {
      markdownBlock.Text = textBoxContent.Text;
    }
  }
}
