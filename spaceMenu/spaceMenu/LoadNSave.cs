using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO.IsolatedStorage;
using System.IO;

namespace newSpace3_2
{
    class LoadNSave
    {
        // loads high score from HighScore file OnActivated
        public void LoadFile(Menu men)
        {
            using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (file.FileExists("HighScore"))
                {
                    using (BinaryReader reader = new BinaryReader(file.OpenFile("HighScore", FileMode.Open)))
                    {
                        men.SetHighScore(reader.ReadInt32());
                    }
                }
            }
        }


        // saves high score to HighScore file OnExiting
        public void SafeFile(Menu men)
        {
            using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (BinaryWriter writer = new BinaryWriter(file.CreateFile("HighScore")))
                {
                    writer.Write(men.GetHighScore());
                }
            }
        }
    }
}
