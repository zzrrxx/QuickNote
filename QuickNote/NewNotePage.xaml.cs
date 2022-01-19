using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QuickNote
{
  /// <summary>
  /// An empty page that can be used on its own or navigated to within a Frame.
  /// </summary>
  public sealed partial class NewNotePage : Page {

    private Note m_Note = null;

    public NewNotePage() {
      this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e) {
      m_Note = (Note)e.Parameter;
      if (m_Note == null) return;
      try {
        textBoxName.Text = m_Note.Name;
        textBoxKeywords.Text = string.Join(",", m_Note.Keywords);
        textBoxContent.Text = m_Note.Content;
        markdownBlock.Text = m_Note.Content;
      } catch (Exception ex) {
        
      }
    }

    private async void btnCancel_Click(object sender, RoutedEventArgs e) {

      bool chooseYes = await Utils.ShowYesNoMessageBox("Do you want to cancel editing the current note?");
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
      try {

        if (m_Note == null) {
          Note note = new Note(name, keywordsArr, content);
          NoteManager.Instance.AddNote(note);
        } else {
          m_Note.Name = name;
          m_Note.Keywords = keywordsArr;
          m_Note.Content = content;
          NoteManager.Instance.UpdateNote(m_Note);
        }

      } catch (Exception ex) {
        Utils.ShowYesMessageBox("Failed to save the note: " + ex.Message);
        return;
      }

      Frame.Navigate(typeof(MainPage));
    }

    private void textBoxContent_TextChanged(object sender, TextChangedEventArgs e) {
      try {
        markdownBlock.Text = textBoxContent.Text;
      } catch (Exception ex) {
        
      }
    }

    private async void btnInsertImage_Click(object sender, RoutedEventArgs e) {
      try {
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");

        Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
        if (file != null) {
          byte[] imgData = null;
          using (Stream stream = await file.OpenStreamForReadAsync()) {
            using (var memoryStream = new MemoryStream()) {
              stream.CopyTo(memoryStream);
              imgData = memoryStream.ToArray();
            }
          }

          long id = NoteManager.Instance.AddImage(imgData);
          textBoxContent.Text += "\r\n![" + id + "](/assert/images/" +id + ")";
        }
        
      } catch (Exception ex) {
      }
    }

    private async void markdownBlock_ImageResolving(object sender, Microsoft.Toolkit.Uwp.UI.Controls.ImageResolvingEventArgs e) {
      try {
        string url = e.Url.Trim();
        int pos = url.LastIndexOf("/");
        long id = long.Parse(url.Substring(pos + 1));
        byte[] imgData = NoteManager.Instance.SearchImage(id);

        BitmapImage biSource = new BitmapImage();
        using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream()) {
          _ = stream.WriteAsync(imgData.AsBuffer());
          stream.Seek(0);
          biSource.SetSource(stream);
        }

        e.Image = biSource;
        e.Handled = true;
      } catch (Exception ex) {
        
      }
    }
  }
}
