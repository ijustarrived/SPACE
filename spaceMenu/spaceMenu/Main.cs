using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;

namespace newSpace2_5
{

    /* To do and notes: 23/nov/2014
     * 
     * agregar una pantalla cool para el gameOver. Si quiero que la musica pegue con el screen dim, 
     * lo que tengo que hacer es que le resto mucho menos a la intensidad del negro y se tarda mas
     * 
     */
    public class Main : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D[] imgs = new Texture2D[11], // tiene todas las imagenes 
            mapImages,
            enemyImages,
            hudImages;

        Boolean killed, // tiene el valor de enemyKilled de spaceEnemies
            killed2;

        Rectangle[] enemyRect; // tiene todos los rectangulos de los enemigo que estan en la clase de spaceEnemies

        SoundEffect btnSound, // tiene el click sound
            shotSound,
            enemyDead,
            playerExplosion,
            screenCrackSound;

        SpriteFont sf;

        Song menuSong,
            gameOverSong,
            inGameSong;

        float colorTranspIntensity = 0.0f;

        #region objetos de cada clase
        Menu m = new Menu(); // objeto de menu

        Map mp = new Map();

        HUD hd = new HUD();

        SpaceEnemies se = new SpaceEnemies();

        #endregion


        public Main()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.IsFullScreen = true;

            Content.RootDirectory = "Content";

            // Frame rate is 30 fps by default for Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Extend battery life under lock.
            InactiveSleepTime = TimeSpan.FromSeconds(1);
        }

        protected override void Initialize()
        {
            mapImages = new Texture2D[1];

            hd.InitAccel();

            hudImages = new Texture2D[10];

            enemyImages = new Texture2D[3];

            hd.getMenu(m);

            hd.GetSpaceEnemies(se);

            m.setHud(hd);

            se.SetHd(hd);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            #region menu images

            imgs[7] = Content.Load<Texture2D>(@"pictures/arcadeBtn");

            imgs[10] = Content.Load<Texture2D>(@"pictures/campBtn");

            imgs[2] = Content.Load<Texture2D>(@"pictures/volClosed");

            imgs[3] = Content.Load<Texture2D>(@"pictures/volOpen");

            imgs[4] = Content.Load<Texture2D>(@"pictures/menuBtn");

            imgs[5] = Content.Load<Texture2D>(@"pictures/missionSel");

            imgs[6] = Content.Load<Texture2D>(@"pictures/optionBtn");

            imgs[0] = Content.Load<Texture2D>(@"pictures/menuBG");

            imgs[8] = Content.Load<Texture2D>(@"pictures/off");

            imgs[9] = Content.Load<Texture2D>(@"pictures/on");

            imgs[1] = Content.Load<Texture2D>(@"pictures/optBG");

            m.getImgs(imgs, spriteBatch);

            #endregion

            #region map images

            //ingame background
            mapImages[0] = Content.Load<Texture2D>(@"MapImgs/bigNeb");

            mp.getMapImgs(mapImages);

            #endregion

            #region hud images

            hudImages[0] = Content.Load<Texture2D>(@"HUDpics/frame");

            hudImages[1] = Content.Load<Texture2D>(@"HUDpics/cracked");

            hudImages[2] = Content.Load<Texture2D>(@"HUDpics/hpTopLayer");

            hudImages[3] = Content.Load<Texture2D>(@"HUDpics/hpBotLayer");

            hudImages[4] = Content.Load<Texture2D>(@"HUDpics/Pause");

            hudImages[5] = Content.Load<Texture2D>(@"HUDpics/Crosshair");

            hudImages[6] = Content.Load<Texture2D>(@"HUDpics/shotImg");

            hudImages[7] = Content.Load<Texture2D>(@"HUDpics/Pause_screen");

            hudImages[8] = Content.Load<Texture2D>(@"HUDpics/return");

            hudImages[9] = Content.Load<Texture2D>(@"HUDpics/Resume");

            hd.getHUDImg(hudImages, imgs);

            #endregion

            #region enemy images

            enemyImages[0] = Content.Load<Texture2D>(@"EnemyPics/meteor");

            enemyImages[1] = Content.Load<Texture2D>(@"EnemyPics/meteor2");

            enemyImages[2] = Content.Load<Texture2D>(@"EnemyPics/Explosion");

            se.SetImgs(enemyImages);

            #endregion

            #region audio

            #region fx

            btnSound = Content.Load<SoundEffect>(@"audio-fx/btnSound");

            screenCrackSound = Content.Load<SoundEffect>(@"audio-fx/screenHit");

            enemyDead = Content.Load<SoundEffect>(@"audio-fx/enemyKilled");            

            shotSound = Content.Load<SoundEffect>(@"audio-fx/Shoot3");

            playerExplosion = Content.Load<SoundEffect>(@"audio-fx/playerExplosion");

            hd.GetFxs(screenCrackSound, playerExplosion);

            se.SetFx(enemyDead);

            m.getSound(btnSound, shotSound);

            #endregion

            #region music

            menuSong = Content.Load<Song>(@"audio-fx/backToSpace");

            inGameSong = Content.Load<Song>(@"audio-fx/spaceSurv");

            gameOverSong = Content.Load<Song>(@"audio-fx/gameOver");

            m.getSongs(menuSong, inGameSong, gameOverSong);

            #endregion

            #endregion

            sf = Content.Load<SpriteFont>("font");

            m.getsf(sf);

            MediaPlayer.IsRepeating = true;

            hd.AssingImgsCenterOrigin();

            hd.assignImgsToRec();

            se.randomizeCoor();

            se.initRectangles();
        }

        protected override void Update(GameTime gameTime)
        {
            m.startMusic();

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                if (m.setExit())
                {
                    this.Exit();
                }

                m.backBtn();                
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            #region stuff to run when switches to ingame mode

            //only makes enemies move closer
            if (m.setIsGameRunning())
            {
                hd.GetIsSoundOn();

                se.CalcMultTimer(se.GetMultTimer(), se.GetIsMultActive()); 

                se.enemiesGetCloser(spriteBatch, hd);

                enemyRect = se.GetEnemyRecs();

                killed = se.GetEnemyKilled();

                se.SetScrnValue(hd.setScreenValue());

                hd.getScreenValue(se.GetScreenVal());

                killed2 = se.GetEnemyKilled2();

                hd.getScreenValue(se.GetScreenVal());

                se.SetScrnValue(hd.setScreenValue());
            }

            #endregion

            m.playSections(se, mp, killed, killed2, mapImages);

            #region gameover black screen thing

            //lo tengo en el main por que habian veces que algunas imgs se ponian oscuras y otras no por que estaban encima del layer

            if (hd.setPlayerDied())
            {
                spriteBatch.Draw(mapImages[0], Vector2.Zero, Color.Black * colorTranspIntensity);                

                if (colorTranspIntensity < 0.50f)
                {
                    colorTranspIntensity += 0.01f;
                }

                else
                {
                    m.displayScoreboard(se.GetPts(), se.GetMiss(), hd.setPlayerDied());
                }
            }

            #endregion

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
