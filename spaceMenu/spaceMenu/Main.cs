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

namespace newSpace3_4
{

    /* To do and notes: 27/feb/2014
     * 
     * 
     */
    public class Main : Microsoft.Xna.Framework.Game
    {
        #region variables

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D[] imgs = new Texture2D[15], // tiene todas las imagenes 
            mapImages,
            enemyImages,
            hudImages;

        Boolean bigKilled, // tiene el valor de enemyKilled de spaceEnemies
            graySmallKilled,
            smallKilled;

        Rectangle[] enemyRect; // tiene todos los rectangulos de los enemigo que estan en la clase de spaceEnemies

        SoundEffect btnSound, // tiene el click sound
            shotSound,
            enemyDead,
            playerExplosion,
            screenCrackSound;

        SpriteFont sf;

        Song menuSong, // Main menu song
            gameOverSong,
            splashScreenSong,
            inGameSong;

        float colorTranspIntensity = 0.0f;

        #region objetos de cada clase
        Menu men = new Menu(); // objeto de menu

        Map mp = new Map();

        HUD hd = new HUD();

        SpaceEnemies se = new SpaceEnemies();

        LoadNSave loadSave = new LoadNSave();

        #endregion

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

        protected override void OnActivated(object sender, EventArgs args)
        {
            loadSave.LoadFile(men, hd);

            base.OnActivated(sender, args);
        }

        protected override void Initialize()
        {
            mapImages = new Texture2D[1];

            hd.InitAccel();

            hudImages = new Texture2D[19];

            enemyImages = new Texture2D[9];

            hd.SetMenu(men);

            hd.SetSpaceEnemies(se);

            men.SetHud(hd);

            se.SetHd(hd);

            men.SetSpaceEnemies(se);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);            

            #region menu images

            imgs[0] = Content.Load<Texture2D>(@"pictures/menuBG");

            imgs[1] = Content.Load<Texture2D>(@"pictures/optBG");

            imgs[2] = Content.Load<Texture2D>(@"pictures/volClosed");

            imgs[3] = Content.Load<Texture2D>(@"pictures/volOpen");

            imgs[4] = Content.Load<Texture2D>(@"pictures/menuBtn");

            imgs[5] = Content.Load<Texture2D>(@"pictures/missionSel");

            imgs[6] = Content.Load<Texture2D>(@"pictures/optionBtn");

            imgs[7] = Content.Load<Texture2D>(@"pictures/arcadeBtn");

            imgs[8] = Content.Load<Texture2D>(@"pictures/off");

            imgs[9] = Content.Load<Texture2D>(@"pictures/on");

            imgs[10] = Content.Load<Texture2D>(@"pictures/campBtn");

            imgs[11] = Content.Load<Texture2D>(@"pictures/GameOver");

            imgs[12] = Content.Load<Texture2D>(@"pictures/mainMenuBtn");

            imgs[13] = Content.Load<Texture2D>(@"pictures/retryBtn");

            imgs[14] = Content.Load<Texture2D>(@"pictures/rRLogo");

            men.SetImgs(imgs, spriteBatch);

            #endregion

            #region map images

            //ingame background
            mapImages[0] = Content.Load<Texture2D>(@"MapImgs/bigNeb");

            mp.SetMapImgs(mapImages);

            #endregion

            #region hud images

            hudImages[0] = Content.Load<Texture2D>(@"HUDpics/frame");

            hudImages[1] = Content.Load<Texture2D>(@"HUDpics/cracked");

            hudImages[2] = Content.Load<Texture2D>(@"HUDpics/hpTopLayer");

            hudImages[3] = Content.Load<Texture2D>(@"HUDpics/hpBotLayer");

            hudImages[4] = Content.Load<Texture2D>(@"HUDpics/Pause");

            hudImages[5] = Content.Load<Texture2D>(@"HUDpics/Crosshair");

            hudImages[6] = Content.Load<Texture2D>(@"HUDpics/shotImg");

            hudImages[7] = Content.Load<Texture2D>(@"HUDpics/GamePaused");

            hudImages[8] = Content.Load<Texture2D>(@"HUDpics/return");

            hudImages[9] = Content.Load<Texture2D>(@"HUDpics/Resume");

            hudImages[10] = Content.Load<Texture2D>(@"HUDpics/X2");

            hudImages[11] = Content.Load<Texture2D>(@"HUDpics/X4");

            hudImages[12] = Content.Load<Texture2D>(@"HUDpics/X6");

            hudImages[13] = Content.Load<Texture2D>(@"HUDpics/X8");

            hudImages[14] = Content.Load<Texture2D>(@"HUDpics/ON");

            hudImages[15] = Content.Load<Texture2D>(@"HUDpics/OFF");

            hudImages[16] = Content.Load<Texture2D>(@"HUDpics/vOpen");

            hudImages[17] = Content.Load<Texture2D>(@"HUDpics/Retry");

            hudImages[18] = Content.Load<Texture2D>(@"HUDpics/howToPlay");

            hd.SetHUDImg(hudImages, imgs);

            hd.SetSpriteBatch(spriteBatch);

            #endregion

            #region enemy images

            enemyImages[0] = Content.Load<Texture2D>(@"EnemyPics/meteor");

            enemyImages[1] = Content.Load<Texture2D>(@"EnemyPics/meteor2");

            enemyImages[2] = Content.Load<Texture2D>(@"EnemyPics/Explosion");

            enemyImages[3] = Content.Load<Texture2D>(@"EnemyPics/greymanSmall");

            enemyImages[4] = Content.Load<Texture2D>(@"EnemyPics/greyMan3Explo1");

            enemyImages[5] = Content.Load<Texture2D>(@"EnemyPics/greyMan3Explo2");

            enemyImages[6] = Content.Load<Texture2D>(@"EnemyPics/greyMan3Explo3");

            se.SetImgs(enemyImages);

            #endregion

            #region audio

            #region fx

            btnSound = Content.Load<SoundEffect>(@"audio-fx/btnSound");

            screenCrackSound = Content.Load<SoundEffect>(@"audio-fx/screenHit");

            enemyDead = Content.Load<SoundEffect>(@"audio-fx/enemyKilled");            

            shotSound = Content.Load<SoundEffect>(@"audio-fx/Shoot3");

            playerExplosion = Content.Load<SoundEffect>(@"audio-fx/playerExplosion");

            hd.SetFxs(screenCrackSound, playerExplosion);

            se.SetFx(enemyDead);

            men.SetSound(btnSound, shotSound);

            #endregion

            #region music

            menuSong = Content.Load<Song>(@"audio-fx/backToSpace");

            inGameSong = Content.Load<Song>(@"audio-fx/spaceSurv");

            gameOverSong = Content.Load<Song>(@"audio-fx/gameOver");

            splashScreenSong = Content.Load<Song>(@"audio-fx/spashScreenAudio");

            men.SetSongs(menuSong, inGameSong, gameOverSong);

            #endregion

            #endregion

            sf = Content.Load<SpriteFont>("font");

            men.SetSf(sf);

            MediaPlayer.IsRepeating = true;

            hd.AssingImgsCenterOrigin();

            hd.assignImgsToRec();

            se.randomizeCoor();

            se.initRectangles();
        }

        protected override void Update(GameTime gameTime)
        {
            //men.startMusic();

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                if (men.GetExit())
                {
                    this.Exit();
                }

                men.backBtn();                
            }

            //se.CheckPositionsAfterRand();

            base.Update(gameTime);
        }

        /// <summary>
        /// Pasa el valor del screen a hd y a space enemies
        /// </summary>
        private void PlaceScrnValues()
        {
            se.SetScrnValue(hd.GetScreenValue());

            hd.SetScreenValue(se.GetScreenVal());
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(20, 20, 20));

            spriteBatch.Begin();

            men.PlaySplashScreen(imgs[14], splashScreenSong);

            if (men.GetSplashScreenTimer() > 199)
            {
                men.startMusic();

                #region stuff to run when switches to ingame mode

                //only makes enemies move closer
                if (men.GetIsGameRunning())
                {
                    //hd.PlayHowToPlay(hudImages[18]);

                    if (hd.GetIsNot1stTimePlaying())
                    {
                        hd.GetIsSoundOn();

                        se.CalcMultTimer(se.GetMultTimer(), se.GetIsMultActive());

                        se.enemiesGetCloser(spriteBatch, hd);

                        enemyRect = se.GetEnemyRecs();

                        bigKilled = se.GetBigEnemyKilled();

                        PlaceScrnValues();
                        smallKilled = se.GetSmallEnemyKilled();

                        PlaceScrnValues();

                        graySmallKilled = se.GetSmallGrayKilled();

                        PlaceScrnValues();
                    }
                }

                #endregion

                    men.playSections(se, mp, bigKilled, smallKilled, graySmallKilled, mapImages);


                    #region gameover black screen thing

                    //lo tengo en el main por que habian veces que algunas imgs se ponian oscuras y otras no por que estaban encima del layer

                    if (hd.GetPlayerDied())
                    {
                        spriteBatch.Draw(mapImages[0], Vector2.Zero, Color.Black * colorTranspIntensity);

                        if (colorTranspIntensity < 0.50f)
                        {
                            colorTranspIntensity += 0.01f;
                        }

                        else
                        {
                            men.displayScoreboard(se.GetPts(), se.GetMiss(), hd.GetPlayerDied());
                        }
                    }

                    #endregion                
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void OnDeactivated(object sender, EventArgs args)
        {
            loadSave.SafeFile(men, hd);

            base.OnDeactivated(sender, args);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            //loadSave.SafeFile(men, hd);

            base.OnExiting(sender, args);
        }
    }
}
