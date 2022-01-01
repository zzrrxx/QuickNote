using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace QuickNote {

  public sealed partial class MainPage : Page {
    public MainPage() {
      this.InitializeComponent();
    }

    private void btnAdd_Click(object sender, RoutedEventArgs e) {
      Frame.Navigate(typeof(NewNotePage));
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
    }

    private void textBoxSearch_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {
      if (args.ChosenSuggestion != null) {
        Note note = args.ChosenSuggestion as Note;
        sender.Text = note.Name;
        markdown.Text = note.Content;
      } else {
        Note note = ((List<Note>)sender.ItemsSource)[0];
        sender.Text = note.Name;
        markdown.Text = note.Content;
      }
    }
  }
}
