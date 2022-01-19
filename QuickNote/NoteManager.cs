using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace QuickNote {
  internal class Note {
    private long     m_Id       = -1;
    private string   m_Name     = "";
    private string[] m_KeyWords = new string[0];
    private string   m_Content  = "";
    
    public Note(string name, string[] keywords, string content) {
      m_Name = name;
      m_KeyWords = keywords;
      m_Content = content;
    }
    public Note(long id, string name, string[] keywords, string content) {
      m_Id = id;
      m_Name = name;
      m_KeyWords = keywords;
      m_Content = content;
    }

    public long Id {
      get { return m_Id; }
    }
    public string Name {
      get { return m_Name;  }
      set { m_Name = value; }
    }
    public string[] Keywords {
      get { return m_KeyWords;  }
      set { m_KeyWords = value; }
    }
    public string Content {
      get { return m_Content;  }
      set { m_Content = value; }
    }

    public override string ToString() {
      return m_Name;
    }

  }
  internal class NoteManager {
    private const string NOTE_TABLE  = "Note";
    private const string IMAGE_TABLE = "Image";
    private const string DB_FILE     = "QuicNote.db";

    private SQLiteConnection m_DBConn   = null;

    public static NoteManager Instance = new NoteManager();

    public int MaxSearchResult = 10;

    private NoteManager() {
      DBOpenConnection();
      DBCreateNoteTable();
    }

    public List<Note> Search(string key) {
      List<Note> notes = new List<Note>();
      string query = "SELECT * from " + NOTE_TABLE + " where name LIKE '%" + key + "%' OR keywords LIKE '%" + key + "%'";
      if (this.MaxSearchResult > 0) {
        query += " LIMIT " + this.MaxSearchResult;  
      }
      SQLiteDataReader reader = DBExecuteQuery(query);
      while (reader.Read()) {
        long   id       = long.Parse(reader["id"].ToString());
        string name     = reader["name"].ToString();
        string keywords = reader["keywords"].ToString();
        string content  = reader["content"].ToString();

        notes.Add(new Note(
          id,
          name,
          keywords.Split(",", StringSplitOptions.RemoveEmptyEntries),
          content
        ));  
      }
      return notes;
    }
    public void AddNote(Note note) {
      DBBackup();
      string query = "INSERT INTO " + NOTE_TABLE + " VALUES (NULL," 
        + "'" + DBEscapeText(note.Name) + "',"
        + "'" + string.Join(",", note.Keywords) + "',"
        + "'" + DBEscapeText(note.Content) + "'"
        + ")";
      SQLiteDataReader reader = DBExecuteQuery(query);
      reader.Close();
    }
    public void UpdateNote(Note note) {
      DBBackup();
      string query = "UPDATE " + NOTE_TABLE + " SET "
        + "name='"     + DBEscapeText(note.Name) + "', "
        + "keywords='" + DBEscapeText(string.Join(",", note.Keywords)) + "',"
        + "content='"  + DBEscapeText(note.Content) + "' "
        + "WHERE id="  + note.Id;
      SQLiteDataReader reader = DBExecuteQuery(query);
      reader.Close();
    }
    public void DeleteNote(Note note) {
      DBBackup();
      string query = "DELETE FROM " + NOTE_TABLE + " WHERE id=" + note.Id;
      SQLiteDataReader reader = DBExecuteQuery(query);
      reader.Close();
    }

    public byte[] SearchImage(long id) {
      // https://stackoverflow.com/questions/625029/how-do-i-store-and-retrieve-a-blob-from-sqlite
      string query = "SELECT content from " + IMAGE_TABLE + " where id=" + id;

      byte[] imgData = new byte[0];
      SQLiteDataReader reader = DBExecuteQuery(query);
      if (reader.Read() && reader["content"] != null && !Convert.IsDBNull(reader["content"])) {
        imgData = (byte[]) reader["content"];
      }
      reader.Close();
      return imgData;
    }
    public long AddImage(byte[] imgData) {
      DBBackup();
      SQLiteCommand command = m_DBConn.CreateCommand();
      command.CommandText = "INSERT INTO " + IMAGE_TABLE + " VALUES (NULL, @img)";
      command.Parameters.Add("@img", DbType.Binary).Value = imgData;
      SQLiteDataReader reader = DBExecuteCommand(command);
      reader.Close();
      reader = DBExecuteQuery("SELECT * FROM " + IMAGE_TABLE + " ORDER BY id DESC LIMIT 1;");
      reader.Read();
      long id = long.Parse(reader["id"].ToString());
      reader.Close();  
      return id;
    }
    public void DeleteImage(long id) {
      DBBackup();
      string query = "DELETE FROM " + IMAGE_TABLE + " WHERE id=" + id;
      SQLiteDataReader reader = DBExecuteQuery(query);
      reader.Close();
    }

    private async void DBBackup() {
      StorageFolder localFolder = ApplicationData.Current.LocalFolder;
      StorageFile dbFile = await localFolder.GetFileAsync(DB_FILE);
      StorageFile backupDB = await localFolder.CreateFileAsync(DB_FILE + ".bak", CreationCollisionOption.ReplaceExisting);
      await dbFile.CopyAndReplaceAsync(backupDB);
    }
    private void DBOpenConnection() {
      StorageFolder localFolder = ApplicationData.Current.LocalFolder;
      string dbFile = localFolder.Path + "\\" + DB_FILE;
      m_DBConn = new SQLiteConnection();
      SQLiteConnectionStringBuilder connStr = new SQLiteConnectionStringBuilder();
      connStr.DataSource = dbFile;
      m_DBConn.ConnectionString = connStr.ToString();
      m_DBConn.Open();
    }
    private void DBCloseConnection() {
      m_DBConn.Close();  
    }
    private void DBCreateNoteTable() {
      string query = "CREATE TABLE IF NOT EXISTS " + NOTE_TABLE + "( " 
        + "id INTEGER PRIMARY KEY AUTOINCREMENT,"
        + "name TEXT NOT NULL UNIQUE,"
        + "keywords TEXT NOT NULL,"
        + "content TEXT NOT NULL"
        + ")";

      SQLiteDataReader reader = DBExecuteQuery(query);
      reader.Close();

      query = "CREATE TABLE IF NOT EXISTS " + IMAGE_TABLE + "( " 
        + "id INTEGER PRIMARY KEY AUTOINCREMENT,"
        + "content BLOB NOT NULL"
        + ")";
      reader = DBExecuteQuery(query);
      reader.Close();
    }

    private SQLiteDataReader DBExecuteQuery(string query) {
      SQLiteCommand cmd = m_DBConn.CreateCommand();
      cmd.CommandText = query;
      return DBExecuteCommand(cmd);
    }
    private SQLiteDataReader DBExecuteCommand(SQLiteCommand command) {
      return command.ExecuteReader();  
    }

    private string DBEscapeText(string value) {
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < value.Length; i++) {
        if (value[i] == '\'') {
          sb.Append("''");  
        } else {
          sb.Append(value[i]);
        }
      }
      return sb.ToString();
    }

  }

}
