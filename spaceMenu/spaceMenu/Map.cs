using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace newSpace3
{
    /*Notes, ideas and problems: 23/nov/2014 
     * 
     *desplega el background como aqui todo es en layers y no quiero el los enemigos salgan encima del HUD, pues cree otra clase 
     *para que bregue lo del HUD.
     *
    */

    class Map
    {
        private Texture2D[] mapImages = new Texture2D[6]; // todas las imagenes del mapa

        private Vector2 bgLocation = new Vector2(0, 0); //coodenadas del bg

       private Rectangle bgRect = new Rectangle(); // es para poder rotar el bg

       private float rotationNum = 0; // tiene el numero de rotacion


        //pone todas las imagenes en mapImages para que se usen en la clase
        public void getMapImgs(Texture2D[] imgList)
        {
            for (int i = 0; imgList.Length > i; i++)
            {
                mapImages[i] = imgList[i];
            }
        }

        //display background
        public void displayInGameBg(SpriteBatch sp)
        {
            bgRect.Width = mapImages[0].Width;

            bgRect.Height = mapImages[0].Height;

            bgRect.X = 400;

            bgRect.Y = 250;

            rotationNum += 0.0003f;

            sp.Draw(mapImages[0],bgRect, null, Color.White, rotationNum, new Vector2(mapImages[0].Width / 2,  mapImages[0].Height / 2), SpriteEffects.None, 0f);
        }
    }
}
