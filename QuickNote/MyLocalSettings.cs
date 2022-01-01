using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace QuickNote {
  internal class MyLocalSettings {
    private ApplicationDataContainer m_LocalSettings = null;


    public static MyLocalSettings Instance = new MyLocalSettings();

    private MyLocalSettings() {
      m_LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;  
    }

    public Size InitialMainWindowSize {
      get {
        if (m_LocalSettings.Values["InitialMainWindowSize"] == null) {
          return new Size(480, 800); // default initial main window size
        }
        string sizeStr = m_LocalSettings.Values["InitialMainWindowSize"] as string;
        string[] parts = sizeStr.Split("_", StringSplitOptions.RemoveEmptyEntries);
        return new Size(int.Parse(parts[0]), int.Parse(parts[1]));
      }
      set {
        m_LocalSettings.Values["InitialMainWindowSize"] = (int)value.Width + "_" + (int)value.Height;
      }
    }

  }
}
