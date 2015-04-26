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
using System.Windows;

namespace newSpace3_4
{
    /* Notes, ideas and problems: 15/mar/2015
     * 
     * Si chocan 2 a la vez un enemy suena y otro no. Pero si mueres, los enemigos siguen spawning con el
     * scoreboard displayed(Needs fixing. I think I got it.).
     * 
     */

    /// <summary>
    /// Maneja todo lo que tiene que ver con el menu
    /// </summary>
    class Menu 
    {
        #region class Variables

        private HUD hd;

        private SpaceEnemies se;

        private Texture2D[] imgs = new Texture2D[15]; // tiene todas las imgs

        private Rectangle[] MenuImgsRec = new Rectangle[15]; // tiene todos los rec de todas la imagenes

        private Rectangle mouseRect = new Rectangle(); // tiene el rectangulo del mouse

        private SoundEffect btnPressed,// sonido de boton pressed
            shotFired; // sonido del tiro

        private Song menuSong, //cancion del menu
            gameOverSong, // cancion de game over
            inGameSong; // cancion dentro del juego

        private SpriteFont sf;

        Vector2 pos = new Vector2(
               GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2,
               (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width) / 2);

        Color col = Color.White;

        int grandTotal = 0, // guarda el resultado de points menos missedShots 
            splashScreenTimer = 0, // controls when the slash screen starts and when it ends
            highScore; // name says it

        private float menuNumber = 0.0f, //te deja saber en que parte del menu estas. 0 = main menu, 1 = start arcade, 2 = campaing, 3 = option y
            // 4 = start campaing mission
            lastMenuNum = 0; // tiene el ultimo menu num para ser usado en vol coli del menu

        private bool hasSongStarted = false, // te deja saber si la cancion empezo. Si le das play mas de una vez la musca se escucha multiples veces 
            //en layers.
            hasSplashScreenSongStarted = false,
            hasGameOverSongStarted = false, // te deja saber si la cancion empezo para que no de play mas de una vez
            isInGameRunning = false, // te deja saber si el inGame mode empezo
            hasInGamePaused = false, // te deja saber si el juego fue pausado es solo para la seccion del laser
            exit = false, // le daje saber si exit
            soundOn = true, // te deja sebr si el sound esta on
            doOnce = true; // es un flag para que le deje saber que debe hacer set a menuNum solo una vez

       private SpriteBatch sp;

       private MouseState oldState, // viejo click
            newState; // click nuevo

        #endregion

       public int GetSplashScreenTimer()
       {
           return splashScreenTimer;
       }

        //play splash screen animation
       public void PlaySplashScreen(Texture2D logo, Song splashScreenSong)
       {  
           Vector2 logoCentralizedOrigin = new Vector2((float)logo.Height / 2, (float)logo.Width / 2);

           if (splashScreenTimer < 200)
           {
               if (!hasSplashScreenSongStarted)
               {
                   hasSplashScreenSongStarted = true;

                   MediaPlayer.Play(splashScreenSong);
               }

               sp.Draw(logo, new Rectangle(0, 0, logo.Width + 103, logo.Height + 2), Color.White);

               oldState = Mouse.GetState();

               //if not pressed keep running splash screen, else skip to main menu 
               if (oldState.LeftButton != ButtonState.Pressed)
                   splashScreenTimer++;

               else
                   splashScreenTimer = 200;
           }
       }

       public void SetHud(HUD h)
       {
           hd = h;
       }

       public void SetSpaceEnemies(SpaceEnemies spE)
       {
           se = spE;
       }

        // regresa el flag para que sea usado en el hud
       public bool SetIsSoundOn()
       {
           return soundOn;
       }

        // el valor se asigna cuando se presionan los btns en pause screen
       public void SetIsSoundOn(bool isSoundOn)
       {
           soundOn = isSoundOn;
       }

       //asigna imagenes a los rectangulos
        private void SetRects()
        {
            for (int i = 0; i < MenuImgsRec.Length; i++)
            {
                MenuImgsRec[i] = new Rectangle(0,0,imgs[i].Width, imgs[i].Height);
            }
        }

        // calcula los missed shits
        private double CalcAcurracy(int p, int mi)
        {
            //try
            //{
            //    min = Math.Abs(mi + p);

            //    min = p / min;

            //    min *= 100;

            //    min = Math.Round(min, 2);
            //}

            //catch
            //{
            //    min = 0;
            //}

            double avg = 0.0;

            try
            {
               avg = Math.Abs(mi + p);

                avg = p / avg;

                avg *= 100;

                avg = Math.Round(avg, 2);
            }

            catch
            {
                avg = 0;
            }

            return avg;
        }
        
       /// <summary>
       ///  Verifica si el final score es mas alto que el high score y si lo es lo asigna
       /// </summary>
        public void CalcHighScore()
        {
            if (grandTotal > highScore)
            {
                highScore = grandTotal;
            }
        }

        //para cuando el juego se haga load
        public void SetHighScore(int score)
        {
            highScore = score;
        }

        // para cuando se haga save
        public int GetHighScore()
        {
            return highScore;
        }

        public void BeforeArcadeStarts()
        {
            //para que haga lo del screen crack animation bien y que no lo haga en lugar donde no se supone que este
            hd.SetPlayerDied(false);

            //reset hp bars
            hd.reset();

            //sp.Draw(img, MenuImgsRec[13], Color.LightGray);

            hd.SetScreenValue(1);

            hd.ChangeHudColors(Color.White);

            hasGameOverSongStarted = false;

            isInGameRunning = true;

            hasInGamePaused = false;

            hd.SetIsInGameRunning(isInGameRunning);

            hasSongStarted = false;

            menuNumber = 1;

            se.SetPts(0);

            se.fullReset();

            hd.updatePlayerHp();

            playBtnSound();

            hd.AccelStart();
        }

        //retry btn when pressed on the game over screen
        public void GameOverRetryBtn(Texture2D img, Rectangle rect)
        {
            oldState = Mouse.GetState();

             assignMouseStuff(newState);

            if (mouseRect.Intersects(rect))
            {
                sp.Draw(img, rect, Color.White);

                if (oldState.LeftButton == ButtonState.Pressed && newState.LeftButton == ButtonState.Pressed)
                {
                    sp.Draw(img, rect, Color.LightGray);
                }

                else if (oldState.LeftButton == ButtonState.Released && newState.LeftButton == ButtonState.Pressed)
                {
                    sp.Draw(img, rect, Color.LightGray);

                    #region shit to assign and run when arcade is pressed

                    ////para que haga lo del screen crack animation bien y que no lo haga en lugar donde no se supone que este
                    //hd.getPlayerDied(false);

                    ////reset hp bars
                    //hd.reset();

                    //sp.Draw(img, MenuImgsRec[13], Color.LightGray);

                    //hd.getScreenValue(1);

                    //hd.ChangeHudColors(Color.White, Color.Navy);

                    //hasGameOverSongStarted = false;

                    //isInGameRunning = true;

                    //hasInGamePaused = false;

                    //hd.getIsInGameRunning(isInGameRunning);

                    //hasSongStarted = false;

                    //menuNumber = 1;

                    //se.SetPts(0);

                    //se.fullReset();                    

                    //hd.updatePlayerHp();

                    //playBtnSound();

                    //hd.AccelStart();

                    #endregion

                    BeforeArcadeStarts();
                }
            }

            else
            {
                sp.Draw(img, rect, Color.White);
            }

            newState = oldState;        
        }

        //action del main menu btn
        public void GameOverMainMenuBtn()
        {
            oldState = Mouse.GetState();

            assignMouseStuff(newState);

            if (mouseRect.Intersects(MenuImgsRec[12]))
            {
                sp.Draw(imgs[12], MenuImgsRec[12], Color.White);

                if (oldState.LeftButton == ButtonState.Pressed && newState.LeftButton == ButtonState.Pressed)
                {
                    sp.Draw(imgs[12], MenuImgsRec[12], Color.LightGray);
                }

                else if (oldState.LeftButton == ButtonState.Released && newState.LeftButton == ButtonState.Pressed)
                {
                    //backBtn();

                    BackToMainMenu();
                }
            }

            else
            {
                sp.Draw(imgs[12], MenuImgsRec[12], Color.White);
            }

            //newState = oldState; 
        }

        // darkens bg and displays scoreboard
        public void displayScoreboard(int points, int misses, bool isPlayerDead)
        {
            PlayGameOverSong();

            double avgMissed = Convert.ToDouble(misses); // tiene la cantidad de missed shots

            if (menuNumber < 2 && isPlayerDead == true)
            {
                #region board
                //board
                sp.Draw(imgs[11], new Rectangle(130, 16, imgs[11].Width + 180, imgs[11].Height + 100), Color.White);

                //menu btn
                sp.Draw(imgs[12], MenuImgsRec[12], Color.White);

                GameOverMainMenuBtn();

                //retry btn
                sp.Draw(imgs[13], MenuImgsRec[13], Color.White);

                GameOverRetryBtn(imgs[13], MenuImgsRec[13]);

                #endregion

                sp.DrawString(sf, highScore.ToString(), new Vector2(240, 190), Color.White);


                sp.DrawString(sf, points.ToString(), new Vector2(430, 110), Color.White);

                //if (points > misses)
                //{
                    avgMissed = CalcAcurracy(points, misses);
                //}

                sp.DrawString(sf, Math.Abs(avgMissed).ToString() + "%", new Vector2(430, 190), Color.White);

                grandTotal = Convert.ToInt32(Math.Abs(points * avgMissed));

                CalcHighScore();

                sp.DrawString(sf, grandTotal.ToString(), new Vector2(430, 270), Color.White);
            }
        }

        // asigna las imagenes a imgs
        public void SetImgs(Texture2D[] pic, SpriteBatch s)
        {
            for (int i = 0; i < pic.Length; i++)
            {
                imgs[i] = pic[i];
            }

            sp = s;

            SetRects();

            placeBtns();
        }

        //darle valor a las coordenadas de los btn
        private void placeBtns()
        {
            #region volOpen btn

            MenuImgsRec[3].X = 50;
            MenuImgsRec[3].Y = 50;

            #endregion

            #region option btn

            MenuImgsRec[6].X = 540;
            MenuImgsRec[6].Y = 40;

            #endregion

            #region arcade btn

            MenuImgsRec[7].X = 300;
            MenuImgsRec[7].Y = 390;
            MenuImgsRec[7].Width = 220;
            MenuImgsRec[7].Height = 30;

            #endregion

            #region off btn

            MenuImgsRec[8].X = 250;
            MenuImgsRec[8].Y = 50;

            #endregion

            #region on btn

            MenuImgsRec[9].X = 180;
            MenuImgsRec[9].Y = 50;

            #endregion

            #region campaing btn

            MenuImgsRec[10].X = 530;
            MenuImgsRec[10].Y = 390;
            MenuImgsRec[10].Width = 220;
            MenuImgsRec[10].Height = 30;

            #endregion

            #region main menu btn

            MenuImgsRec[12].X = 417;
            MenuImgsRec[12].Y = 323;
            MenuImgsRec[12].Width = imgs[12].Width + 95;
            MenuImgsRec[12].Height = imgs[12].Height + 5;

            #endregion

            #region retry btn

            MenuImgsRec[13].X = 136;
            MenuImgsRec[13].Y = 324;
            MenuImgsRec[13].Width = imgs[12].Width + 98;
            MenuImgsRec[13].Height = imgs[12].Height + 4;

            #endregion
        }

        //asigna cancion a menuSong
        public void SetSongs(Song s, Song s2, Song s3)
        {
            menuSong = s;

            inGameSong = s2;

            gameOverSong = s3;
        }

        // plays btn sound
        public void playBtnSound()
        {
            if (soundOn)
            {
                btnPressed.Play();
            }
        }

        public void SetMenNum(int num)
        {
            menuNumber = num;
        }

        //asigna sonido a btnPressed
        public void SetSound(SoundEffect s, SoundEffect s2)
        {
            btnPressed = s;

            shotFired = s2;
        }

        //starts music
        public void startMusic()
        {
            // si ya empezo no la empiezes otravez
            if (!hasSongStarted)
            {
                hasSongStarted = true;

                if (soundOn)
                {
                    if (menuNumber == 0)
                    {
                        MediaPlayer.Play(menuSong);
                    }

                    else if(menuNumber == 1 && (!hd.GetPlayerDied()))
                    {
                        MediaPlayer.Play(inGameSong);
                    }
                }

                else
                {
                    MediaPlayer.Pause();
                }
            }
        }

        public void PlayGameOverSong()
        {
            if (!hasGameOverSongStarted)
            {
                hasGameOverSongStarted = true;

                if (soundOn)
                {
                    MediaPlayer.IsRepeating = false;

                    MediaPlayer.Play(gameOverSong);
                }
            }
        }

        //display main menu background
        public void displayMenuBg()
        {
            if (menuNumber == 0)
            {
                //sp.Draw(imgs[0], Vector2.Zero, Color.White);

                sp.Draw(imgs[0], new Rectangle(0, 0, imgs[0].Width + 40, imgs[0].Height) , Color.White);
            }

            else if(menuNumber == 2 || menuNumber == 3 || menuNumber == 4)
            {
                sp.Draw(imgs[1], Vector2.Zero, Color.White);
            }
        }

        //regresa exit
        public bool GetExit()
        {
            return exit;
        }

        //reset todo lo que se usa en el inGame en esta area
        public void BackToMainMenu()
        {
            // guarda ultimo menu
            lastMenuNum = menuNumber;

            // cambia menu num a main menu num
            menuNumber = 0;

            //para que empieze la cancion
            hasSongStarted = false;

            // ingame mode stopped
            isInGameRunning = false;

            hd.SetHasInGamePaused(false);

            // no cierres el juego
            exit = false;

            //para que haga lo del screen crack animation bien y que no lo haga en lugar donde no se supone que este
            hd.SetPlayerDied(false);

            //para que no mueva los enemigos
            hd.SetIsInGameRunning(isInGameRunning);

            //pare el accelerometro
            hd.AccelStop();

            playBtnSound();

            //para que empieze el screen cracked animation de donde es y no lo haga donde no se supone que sea
            hd.SetScreenValue(0);

            //reset hp bars
            hd.reset();

            //reset esos 2 rects
            hd.ResetResumeNReturnRects();

            MediaPlayer.IsRepeating = true;
        }

        //cuando el btn de back del phone is pressed
        public void backBtn()
        {
            if (menuNumber == 0)
            {
                exit = true ;
            }

            else if (menuNumber < 2)
            {
                //lastMenuNum = menuNumber;

                //BackToMainMenu();  
                if (!hd.GetPlayerDied())
                {
                    SetIsGameRunning(hd.GetIsInGameRunning());

                    hd.PauseGame(ref hasInGamePaused, ref isInGameRunning);
                }
            }

            else
            {
                playBtnSound();

                lastMenuNum = menuNumber;

                menuNumber = 0;
            }
        }

        // asigna coordenadas y size del mouse a su rect
        private void assignMouseStuff(MouseState n)
        {
            mouseRect.X = n.X;
            mouseRect.Y = n.Y;
            mouseRect.Width = 20;
            mouseRect.Height = 20; 
        }

        //check si hay colision con el arcade btn
        private void arcadeColli()
        {
            oldState = Mouse.GetState();

            assignMouseStuff(newState);

            if (mouseRect.Intersects(MenuImgsRec[7]))
            {
                sp.Draw(imgs[7], MenuImgsRec[7], Color.White);

                if (oldState.LeftButton == ButtonState.Pressed && newState.LeftButton == ButtonState.Pressed)
                {
                    sp.Draw(imgs[7], MenuImgsRec[7], Color.LightBlue);
                }

                else if (oldState.LeftButton == ButtonState.Released && newState.LeftButton == ButtonState.Pressed)
                {
                    #region shit to assign and run when arcade is pressed

                    sp.Draw(imgs[7], MenuImgsRec[7], Color.LightBlue);

                    hd.SetScreenValue(1);

                    hd.ChangeHudColors(Color.White);

                    hasGameOverSongStarted = false;

                    isInGameRunning = true;

                    hasInGamePaused = false;

                    hd.SetIsInGameRunning(isInGameRunning);

                    hasSongStarted = false;

                    menuNumber = 1;

                    se.SetPts(0);

                    se.fullReset();                    

                    hd.updatePlayerHp();

                    playBtnSound();

                    hd.AccelStart();

                    #endregion

                    //BeforeArcadeStarts(
                }
            }

            else
            {
                sp.Draw(imgs[7], MenuImgsRec[7], Color.White);
            }

            newState = oldState;
        }

        //check si hay colision con el campaing btn
        private void campColli()
        {
            assignMouseStuff(newState);

            if (mouseRect.Intersects(MenuImgsRec[10]))
                {
                    sp.Draw(imgs[10], MenuImgsRec[10], Color.White);

                    if (oldState.LeftButton == ButtonState.Pressed && newState.LeftButton == ButtonState.Pressed)
                    {
                        sp.Draw(imgs[10], MenuImgsRec[10], Color.LightBlue);
                    }

                    else if(oldState.LeftButton == ButtonState.Released && newState.LeftButton == ButtonState.Pressed)
                    {
                        sp.Draw(imgs[10], MenuImgsRec[10], Color.LightBlue);

                        menuNumber = 2;

                        playBtnSound();
                    }
                }

            else
            {
                sp.Draw(imgs[10], MenuImgsRec[10], Color.White);
            }
        }

        //check si hay colision con el option btn
        private void optColli()
        {
            assignMouseStuff(newState);

            if (mouseRect.Intersects(MenuImgsRec[6]))
            {
                sp.Draw(imgs[6], MenuImgsRec[6], Color.White);

                if (oldState.LeftButton == ButtonState.Pressed && newState.LeftButton == ButtonState.Pressed)
                {
                    sp.Draw(imgs[6], MenuImgsRec[6], Color.LightBlue);
                }

                else if (oldState.LeftButton == ButtonState.Released && newState.LeftButton == ButtonState.Pressed)
                {
                    sp.Draw(imgs[6], MenuImgsRec[6], Color.LightBlue);

                    menuNumber = 3;

                    playBtnSound();
                }
            }

            else
            {
                sp.Draw(imgs[6], MenuImgsRec[6], Color.White);
            }

            //newState = oldState;
        }

        //assign text to spriteFont
        public void SetSf(SpriteFont s)
        {
            sf = s;
        }

        //check si hay colision con los btn del volumen
        private void volColli()
        {
            //cada vez que se entra a un menu se necesita empezar con esto para empezar con un nuevo state
            oldState = Mouse.GetState();

            assignMouseStuff(newState);     

            #region On

            if (mouseRect.Intersects(MenuImgsRec[9]))
            {
                if (oldState.LeftButton == ButtonState.Pressed && newState.LeftButton == ButtonState.Pressed)
                {
                    sp.Draw(imgs[9], MenuImgsRec[9], Color.LightBlue);
                }

                else if (oldState.LeftButton == ButtonState.Released && newState.LeftButton == ButtonState.Pressed)
                {
                    sp.Draw(imgs[9], MenuImgsRec[9], Color.LightBlue);

                    soundOn = true;

                    // si el ultimo menu num fue ingame mode do this
                    if (lastMenuNum == 1)
                    {
                        MediaPlayer.Play(menuSong);

                        lastMenuNum = menuNumber;
                    }

                    else
                    {
                        MediaPlayer.Resume();
                    }

                    playBtnSound();
                }

                else
                {
                    if (soundOn)
                    {
                        sp.Draw(imgs[9], MenuImgsRec[9], Color.LightBlue);
                    }

                    else
                    {
                        sp.Draw(imgs[9], MenuImgsRec[9], Color.White);
                    }
                }
            }

            else
            {
                if (soundOn)
                {
                    sp.Draw(imgs[9], MenuImgsRec[9], Color.LightBlue);
                }

                else
                {
                    sp.Draw(imgs[9], MenuImgsRec[9], Color.White);
                }
            }

            #endregion

            #region off

            if (mouseRect.Intersects(MenuImgsRec[8]))
            {
                if (oldState.LeftButton == ButtonState.Pressed && newState.LeftButton == ButtonState.Pressed)
                {
                    sp.Draw(imgs[8], MenuImgsRec[8], Color.LightBlue);
                }

                else if (oldState.LeftButton == ButtonState.Released && newState.LeftButton == ButtonState.Pressed)
                {
                    sp.Draw(imgs[8], MenuImgsRec[8], Color.LightBlue);

                    playBtnSound();

                    soundOn = false;

                    MediaPlayer.Pause();
                }

                else
                {
                    if (!soundOn)
                    {
                        sp.Draw(imgs[8], MenuImgsRec[8], Color.LightBlue);
                    }

                    else
                    {
                        sp.Draw(imgs[8], MenuImgsRec[8], Color.White);
                    }
                }
            }

            else
            {
                if (!soundOn)
                {
                    sp.Draw(imgs[8], MenuImgsRec[8], Color.LightBlue);
                }

                else
                {
                    sp.Draw(imgs[8], MenuImgsRec[8], Color.White);
                }
            }

            #endregion

            sp.Draw(imgs[3], MenuImgsRec[3], Color.White);

            newState = oldState;
        }

        //check si hay colision con los btn del volumen en pause screen
        public void volColli(Rectangle on, Rectangle off, Rectangle mousRect)
        {
            #region On

            if (mousRect.Intersects(on))
            {
                soundOn = true;

                MediaPlayer.Resume();

                playBtnSound();
            }

            #endregion

            #region off

            if (mousRect.Intersects(off))
            {
                playBtnSound();

                soundOn = false;

                MediaPlayer.Pause();                
            }
            #endregion
        }

        //te deja saber si corre toda la mierda del main para que ingame functions funcionen.
        public bool GetIsGameRunning()
        {
            return isInGameRunning;
        }

        //verifica si el juego paro por otro lado, player lost o simply exited
        public void SetIsGameRunning(Boolean isRunning)
        {
            isInGameRunning = isRunning;
        }

        //corre todos los methods de cada section de acuerdo al menuNumber
        public void playSections(SpaceEnemies se , Map mp, bool killedBig, bool killedSmall, bool killedSmallGray, Texture2D[] nebImg)
        {            
            #region main menu

            if (menuNumber == 0)
            {
                displayMenuBg();

                arcadeColli();

                //campColli();

                //optColli();
            }

            #endregion

            #region ingame section

            else if (menuNumber == 1)
            {
                if (doOnce)
                {
                    doOnce = false;

                    hd.SetMenuNum(menuNumber);
                }

                hd.PlayHowToPlay();                

                if (hd.GetIsNot1stTimePlaying())
                {
                    menuNumber = hd.GetMenuNum();

                    mp.displayInGameBg(sp);

                    //se.CheckPositionsAfterRand();

                    se.displayImgs(sp, sf);

                    hd.playScreenCrackAnimation(sp, se.GetPlayerHit());

                    hd.UserShootsALaser(ref hasInGamePaused, shotFired, soundOn, ref isInGameRunning);

                    // draw laser
                    sp.Draw(hd.GetLaserImg(), hd.GetLaserRect(), null, Color.White, 0, new Vector2(hd.GetLaserRect().Width / 2, hd.GetLaserRect().Height / 2),
                        SpriteEffects.None, 0);

                    sp.Draw(hd.GetLaserImg(), hd.GetLaserRect2(), null, Color.White, 0, new Vector2(hd.GetLaserRect().Width / 2, hd.GetLaserRect().Height / 2),
                        SpriteEffects.None, 0);

                    hd.MoveCrosshair();

                    hd.displayCrosshair(sp, sf);

                    se.SetPlayerHit(hd.GetPHit());


                    #region small guy
                    //esta heredando imgRect para evitar null refrence error
                    hd.checkDistanceFromScreen(200, se.GetEnemyImgRects()[1].Width, sp, sf, killedSmall);

                    SetIsGameRunning(hd.GetIsInGameRunning());

                    #endregion

                    #region big guy

                    hd.checkDistanceFromScreen(300, se.GetEnemyImgRects()[0].Width, sp, sf, killedBig);

                    SetIsGameRunning(hd.GetIsInGameRunning());

                    #endregion

                    #region gray small

                    hd.checkDistanceFromScreen(100, se.GetEnemyImgRects()[2].Width, sp, sf, killedSmallGray);

                    SetIsGameRunning(hd.GetIsInGameRunning());

                    #endregion

                    //sp.DrawString(sf, isInGameRunning.ToString(), new Vector2(100, 100), Color.Red);
                }

                //sp.DrawString(sf, " Not 1st time" + hd.GetIsNot1stTimePlaying().ToString(), new Vector2(200, 200), Color.Red);
            }

            #endregion

            #region mission section

            else if (menuNumber == 2)
            {
                displayMenuBg();
            }

            #endregion

            #region option section

            else if (menuNumber == 3)
            {
                displayMenuBg();

                volColli();
            }

            #endregion                      

            #region campaign section

            else
            {
                //start campaing mission
            }

            #endregion
        }
    }    
}