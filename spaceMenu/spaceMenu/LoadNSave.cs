using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO.IsolatedStorage;
using System.IO;

namespace newSpace3_4

{ /*Notes, ideas and problems 4/apr/15
   * 
   * 
   */
    /// <summary>
    /// Brega con save y load
    /// </summary>
    class LoadNSave
    {
        /// <summary>
        /// Loads high score from Saves file OnActivated
        /// </summary>
        /// <param name="men">Object</param>
        /// <param name="hd">Object</param>
        public void LoadFile(Menu men, HUD hd)
        {
            using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (file.FileExists("Saves"))
                {
                    using (BinaryReader reader = new BinaryReader(file.OpenFile("Saves", FileMode.Open)))
                    {
                        //High score
                        men.SetHighScore(reader.ReadInt32());

                        // 1st time playing
                        hd.SetIsNot1stTimePlaying(reader.ReadBoolean());
                    }
                }
            }
        }


        /// <summary>
        /// Saves high score to Saves file OnExiting
        /// </summary>
        /// <param name="men">Object</param>
        /// <param name="hd">Object</param>
        public void SafeFile(Menu men, HUD hd)
        {
            using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (BinaryWriter writer = new BinaryWriter(file.CreateFile("Saves")))
                {
                    //High score
                    writer.Write(men.GetHighScore());

                    //1st time playing
                    writer.Write(hd.GetIsNot1stTimePlaying());
                }
            }
        }
    }
}
