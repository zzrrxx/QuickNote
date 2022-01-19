using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace QuickNote {

  public sealed partial class MainPage : Page {

    private Note m_CurNote = null;

    public MainPage() {
      this.InitializeComponent();

      ApplicationView.PreferredLaunchViewSize = MyLocalSettings.Instance.InitialMainWindowSize;
      ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
    }

    private void btnAdd_Click(object sender, RoutedEventArgs e) {
      Frame.Navigate(typeof(NewNotePage));
    }
    private void btnEdit_Click(object sender, RoutedEventArgs e) {
      if (m_CurNote == null) { return;}
      Frame.Navigate (typeof(NewNotePage), m_CurNote);
    }
    private async void btnDelete_Click(object sender, RoutedEventArgs e) {
      if (m_CurNote == null) { return; }

      bool delete = await Utils.ShowYesNoMessageBox("Do you want to delete the node: '" + m_CurNote.Name + "'?");
      if (delete) {
        try {
          NoteManager.Instance.DeleteNote(m_CurNote);
          m_CurNote = null;
          markdown.Text = "";
        } catch (Exception ex) {
          Utils.ShowYesMessageBox("Failed to delete the note: " + ex.Message);  
        }
      }
    }

    private void textBoxSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) {
      // Only get results when it was a user typing,
      // otherwise assume the value got filled in by TextMemberPath
      // or the handler for SuggestionChosen.
      if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput) {
        //Set the ItemsSource to be your filtered dataset
        string search = sender.Text.Trim();
        if (search.Length > 0) {
          sender.ItemsSource = NoteManager.Instance.Search(search);
        } else {
          sender.ItemsSource = new List<Note>();  
        }
      }
    }

    private void textBoxSearch_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args) {
      Note note = args.SelectedItem as Note;
      sender.Text = note.Name;
      markdown.Text = note.Content;
      m_CurNote = note;
    }

    private void textBoxSearch_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {
      m_CurNote = null;
      if (args.ChosenSuggestion != null) {
        m_CurNote = args.ChosenSuggestion as Note;
      } else {
        List<Note> notes = (List<Note>)sender.ItemsSource;
        if (notes == null || notes.Count == 0) return;
        m_CurNote = notes.First();
      }
      sender.Text = m_CurNote.Name;
      markdown.Text = m_CurNote.Content;
    }

    private void mainPage_SizeChanged(object sender, SizeChangedEventArgs e) {
      
      if (e.PreviousSize.Width == 0 && e.PreviousSize.Height == 0) return;

      MyLocalSettings.Instance.InitialMainWindowSize = e.NewSize;
    }

    private void markdown_ImageResolving(object sender, Microsoft.Toolkit.Uwp.UI.Controls.ImageResolvingEventArgs e) {
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
