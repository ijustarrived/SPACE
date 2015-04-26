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
using Microsoft.Devices.Sensors;

namespace newSpace3_4
{
    /*Notes, stuff to do and ideas 8/mar/2015
     * 
     * Raras veces el hud se desaparece(bug)
     * 
     */

    /// <summary>
    /// Brega todo lo que tiene que ver con el HUD
    /// </summary>
    class HUD
    {
        #region class variables

        private Menu men;

        private SpaceEnemies sE;

        private SpriteBatch sp;

        private SoundEffect screenCrackSound,
            playerExplosionSound;

        private Texture2D[] hudImgs = new Texture2D[19], // todas las imagenes
            menuImgs = new Texture2D[3]; // tiene las imagenes del volumen

        private Rectangle[] imgRec = new Rectangle[12]; //rectangulos del top y bot layer del hp, enemy hp y laser shot

        private Rectangle crossHColiBox, // crosshair collision box es un rect small encima del rect del crosshair y con eso es que se verifica colision
                    clickedArea;

        #region vectors

        private Vector2[] centerImgsOrigin = new Vector2[4]; // centro origen de las imagenes para que puedan ser centralizadas

        private Vector2 crosshairPos = new Vector2(400, 200), // coordenadas del crosshair
            initialLaserPosition = new Vector2(-60, 500), // posicion inicial del laser1
            initialLaserPosition2 = new Vector2(860, 500), // posicion inicial del laser2
            posFinalLaser = new Vector2(),
            diferenciaDist = new Vector2(), // tiene la diferencia en distancia entre el crosshair y el laser1 shot
            diferenciaDist2 = new Vector2(); // tiene la diferencia en distancia entre el crosshair y el laser2 shot

        private Vector3 accelReadings = new Vector3(); //guarda la data del acelerometro

        #endregion

        private Accelerometer acc;

        MouseState previous_State,
            newState,
            mousymouse;

        private float menuNum = 0, // tiene el valor del current section
            screenValue = 1; // la misma variable como trans pero para el screen para qu haga el mismo efecto

        #region bools

        private bool red = false, // flag par que cambie a rojo todo
            isNotFirstTimePlaying = false, // flag que te deja saber si deje subir el how to play 
            isSoundOn = true, // flag que te deja saber si el sounido esta prendido
            playCrackedScreenOnce = false, // flag que te deja saber que se de play una vez y no muchas veces   
            playPlayerExplosionOnce = false,
            pHit = false, // para enviar el valor a space enemy y darselo a playerHit
            accelActive = false, // flag que te deja saber si el accelerometro esta leyendo
            shotOnceflag = true, // flag para que solo suena el fx del laser una vez
            okToShot = false, // te deja saber que solo presiona la pantalla
            playerDied = false, // flag que te deja saber si el player murio
            laserReachedFinalPos = true, // on flag que te indica si el laser llego a la posicion final
            isInGamePaused = false, // te daje saber si el ingame mode fue pausado
            wasPausePressed = false, // flag para que evite que dispare when you unpause the game. Solo pasa con el pause img
            isInGameRunning = true; //  te deja saber si el ingame mode empezo

        #endregion

        #region ints

        private int counter = 0, // por cuanto va a estar todo rojo o blanco
            howToPlayTimer = 0, // controls when the howToPlay starts and when it ends
            hpBarLowLayerLength = 391, // valor que vas a estar cambiando con cada hit del player para asi hacer el hp que baje
            hpBarTopLayerLength = 360,
            points = 0,
            missedShots = 0,
            ptsMult = 0; // tiene el multiplo del point multiplier

        #endregion

        private Color hdColor = Color.White, // se usa para quitarle valor en el gameOver animation. Para que todo valla de rojo a negro
            ptsFontColor = Color.Navy; // tiene el color del font de los points     

        private MouseState oldState;

        #endregion

        /// <summary>
        /// coje valores del main y los pasa a la clase
        /// </summary>
        /// <param name="fx">screenCrack</param>
        /// <param name="fx2"> player explosion</param>
        public void SetFxs(SoundEffect fx, SoundEffect fx2)
        {
            screenCrackSound = fx;

            playerExplosionSound = fx2;
        }

        public void SetSpriteBatch(SpriteBatch sb)
        {
            sp = sb;
        }

        /// <summary>
        /// Is not por que el save me regresa false por default
        /// </summary>
        /// <param name="isNot1stTimePlaying"></param>
        public void SetIsNot1stTimePlaying(bool isNot1stTimePlaying)
        {
            isNotFirstTimePlaying = isNot1stTimePlaying;
        }

        public bool GetIsNot1stTimePlaying()
        {
            return isNotFirstTimePlaying;
        }

        /// <summary>
        /// Plays how to play img
        /// </summary>
        /// <param name="howToPlayImg">how to play image</param>
        public void PlayHowToPlay()
        {
            if (!isNotFirstTimePlaying)
            {
                Vector2 logoCentralizedOrigin = new Vector2((float)hudImgs[18].Height / 2, (float)hudImgs[18].Width / 2);

                if (howToPlayTimer < 130)
                {
                    sp.Draw(hudImgs[18], new Rectangle(0, 0, hudImgs[18].Width + 103, hudImgs[18].Height + 2), Color.White);

                    oldState = Mouse.GetState();

                    //if not pressed keep running how to play img, else skip to main menu 
                    if (oldState.LeftButton != ButtonState.Pressed)
                        howToPlayTimer++;

                    else
                    {
                        howToPlayTimer = 130;

                        isNotFirstTimePlaying = true;
                    }
                }

                else
                    isNotFirstTimePlaying = true;
            }
        }

        public int GetHowToPlayTimer()
        {
            return howToPlayTimer;
        }

        /// <summary>
        /// coje el valor de menu y lo pasa a la variable de hd
        /// </summary>
        public void GetIsSoundOn()
        {
            isSoundOn = men.SetIsSoundOn();
        }

        /// <summary>
        /// play cualquier fx que se necesite repetir
        /// </summary>
        /// <param name="isPlaying">Bool que te deja saber si la musica esta corriendo</param>
        /// <param name="fx"></param>
        public void PlaySound(ref bool isPlaying, SoundEffect fx)
        {
            if (isSoundOn && (!isPlaying))
            {
                isPlaying = true;

                fx.Play();
            }
        }

        /// <summary>
        /// por si  tienes que play una vez
        /// </summary>
        /// <param name="fx"></param>
        public void PlaySound(SoundEffect fx)
        {
            if (isSoundOn)
            {
                fx.Play();
            }
        }


        public void SetMenu(Menu m)
        {
            men = m;
        }

        public void SetSpaceEnemies(SpaceEnemies s)
        {
            sE = s;
        }

        /// <summary>
        /// para usar en map para poder dejarle saber que empieze a blacken el backgeound. Part of the gameOver animation
        /// </summary>
        /// <returns>Bool que te deja saber que el player murio</returns>
        public Boolean GetPlayerDied()
        {
            return playerDied;
        }

        /// <summary>
        /// asigna el valor cuando halla hecho ResetInGame de menu
        /// </summary>
        /// <param name="isDead">Bool qu te deja saber si el player ded</param>
        public void SetPlayerDied(bool isDead)
        {
            playerDied = isDead;
        }

        /// <summary>
        /// para que se usen en el scoreboard
        /// </summary>
        /// <returns></returns>
        public int GetPoints()
        {
            return points;
        }

        /// <summary>
        /// para que se usen en el scoreboard
        /// </summary>
        /// <returns></returns>
        public int GetMissedShots()
        {
            return missedShots;
        }

        /// <summary>
        /// pone todas las imagenes en hudImgs para que se usen en la clase
        /// </summary>
        /// <param name="imgs">hud imgs array</param>
        /// <param name="menImgs">menu imgs array</param>
        public void SetHUDImg(Texture2D[] imgs, Texture2D[] menImgs)
        {
            for (int i = 0; imgs.Length > i; i++)
            {
                hudImgs[i] = imgs[i];
            }

            // volumen open
            menuImgs[0] = menImgs[3];

            // off
            menuImgs[1] = menImgs[8];

            // on
            menuImgs[2] = menImgs[9];
        }

        /// <summary>
        /// regresa la imagen de laser para que se pueda dibujar en el menu
        /// </summary>
        /// <returns></returns>
        public Texture2D GetLaserImg()
        {
            return hudImgs[6];
        }

        /// <summary>
        /// asigna el current section number
        /// </summary>
        /// <param name="num">Menu section number</param>
        public void SetMenuNum(float num)
        {
            menuNum = num;
        }

        /// <summary>
        /// regresa cuando section cambia 4
        /// </summary>
        /// <returns></returns>
        public float GetMenuNum()
        {
            return menuNum;
        }

        /// <summary>
        ///  initializa el accelerometro
        /// </summary>
        public void InitAccel()
        {
            acc = new Accelerometer();

            acc.ReadingChanged += new EventHandler<AccelerometerReadingEventArgs>(AccelerometerReadingChanged);
        }

        /// <summary>
        /// lee la data del acelerometro
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AccelerometerReadingChanged(object sender, AccelerometerReadingEventArgs e)
        {
            accelReadings.X = -(float)e.Y;
            accelReadings.Y = -(float)e.X;
            accelReadings.Z = -(float)e.Z;
        }

        /// <summary>
        /// le dice al accelerometro que empieze a leer.
        /// </summary>
        public void AccelStart()
        {
            if (!(accelActive))
            {
                acc.Start();

                accelActive = true;
            }
        }

        /// <summary>
        /// le dice al acelerometro que pare de leer
        /// </summary>
        public void AccelStop()
        {
            if (accelActive)
            {
                acc.Stop();

                accelActive = false;
            }
        }

        /// <summary>
        /// le sube y baja las coordenadas del crosshair
        /// </summary>
        public void MoveCrosshair()
        {
            if (accelActive)
            {
                #region Y Movement

                if (accelReadings.Y > 0)
                {
                    if (imgRec[4].Y < 371)
                    {
                        crosshairPos.Y += Convert.ToInt32(accelReadings.Y * 40);

                        imgRec[4].Y += Convert.ToInt32(accelReadings.Y * 40);

                        crossHColiBox.Y += Convert.ToInt32(accelReadings.Y * 40);
                    }
                }

                else
                {
                    if (imgRec[4].Y > 70)
                    {
                        crosshairPos.Y += Convert.ToInt32(accelReadings.Y * 40);

                        imgRec[4].Y += Convert.ToInt32(accelReadings.Y * 40);

                        crossHColiBox.Y += Convert.ToInt32(accelReadings.Y * 40);
                    }
                }

                #endregion

                #region X Movement

                if (accelReadings.X > 0)
                {
                    if (imgRec[4].X < 710)
                    {
                        //crosshairPos para que el laser siga es posicion
                        crosshairPos.X += Convert.ToInt32(accelReadings.X * 40);

                        //para que el crosshair se mueva
                        imgRec[4].X += Convert.ToInt32(accelReadings.X * 40);

                        // para que el small crosshair siga el big one
                        crossHColiBox.X += Convert.ToInt32(accelReadings.X * 40);
                    }
                }

                else
                {
                    if (imgRec[4].X > 70)
                    {
                        crosshairPos.X += Convert.ToInt32(accelReadings.X * 40);

                        imgRec[4].X += Convert.ToInt32(accelReadings.X * 40);

                        crossHColiBox.X += Convert.ToInt32(accelReadings.X * 40);
                    }
                }

                #endregion
            }
        }

        /// <summary>
        ///  para enviar el valor a spaceenemy
        /// </summary>
        /// <returns></returns>
        public float GetScreenValue()
        {
            return screenValue;
        }

        /// <summary>
        /// set player hit
        /// </summary>
        /// <returns></returns>
        public bool GetPHit()
        {
            return pHit;
        }

        /// <summary>
        /// para recoger el valor de spaceenemy
        /// </summary>
        /// <param name="value"></param>
        public void SetScreenValue(float value)
        {
            screenValue = value;
        }

        //asigna los valores del top/bot layer del hp y el enemy hp
        public void assignImgsToRec()
        {
            //hp bar
            imgRec[0] = new Rectangle(400, 50, hpBarTopLayerLength, 15);

            //hp bar
            imgRec[1] = new Rectangle(400, 50, hpBarLowLayerLength, 20);

            //laser left
            imgRec[2] = new Rectangle(Convert.ToInt32(initialLaserPosition.X), Convert.ToInt32(initialLaserPosition.Y),
                hudImgs[6].Width - 250, hudImgs[6].Height - 250);

            //pause button
            imgRec[3] = new Rectangle(375, 427, 50, 50);

            // full crosshair box
            imgRec[4] = new Rectangle(Convert.ToInt32(crosshairPos.X), Convert.ToInt32(crosshairPos.Y), hudImgs[5].Width - 70, hudImgs[5].Height - 70);

            // laser right
            imgRec[5] = new Rectangle(Convert.ToInt32(initialLaserPosition.X), Convert.ToInt32(initialLaserPosition.Y),
                                        hudImgs[6].Width - 250, hudImgs[6].Height - 250);

            /* crosshair small box. How this works es que hay un small rect en el medio del crosshair, pero doesn't bound el crosshair y eso es lo que verifica si
             colisiona con los meteoros */
            crossHColiBox = new Rectangle(Convert.ToInt32(crosshairPos.X), Convert.ToInt32(crosshairPos.Y), hudImgs[5].Width - 120, hudImgs[5].Height - 120);

            //pause screen
            imgRec[6] = new Rectangle(120, 30, hudImgs[7].Width + 160, hudImgs[7].Height + 100);

            ////return to menu img
            //imgRec[7] = new Rectangle(250, 300, hudImgs[8].Width - 320, hudImgs[8].Height - 40);

            ////resume to game img
            //imgRec[8] = new Rectangle(290, 130, hudImgs[9].Width - 140, hudImgs[8].Height- 40);

            //on Img
            imgRec[9] = new Rectangle(517, 159, hudImgs[14].Width - 15, hudImgs[14].Height - 15);

            //off Img
            imgRec[10] = new Rectangle(572, 159, hudImgs[15].Width - 15, hudImgs[15].Height - 15);

            //Retry img
            imgRec[11] = new Rectangle(430, 230, hudImgs[17].Width - 10, hudImgs[17].Height - 10);
        }

        // update el hp de acuerdo al length del lower y top layer
        public void updatePlayerHp()
        {
            imgRec[1].Width = hpBarLowLayerLength;

            imgRec[0].Width = hpBarTopLayerLength;
        }

        //sets off all flags to pause game
        public void PauseGame(ref Boolean hasPaused, ref bool inGameRunning)
        {
            men.playBtnSound();

            hasPaused = true;

            isInGamePaused = true;

            isInGameRunning = false;

            inGameRunning = false;

            AccelStop();
        }

        public void UnpauseGame(ref Boolean hasPaused, ref bool inGameRunning)
        {
            men.playBtnSound();

            hasPaused = false;

            isInGamePaused = false;

            isInGameRunning = true;

            inGameRunning = true;

            AccelStart();
        }

        // verifica si presiono resume txt en el pause screen
        public void CheckIfResumeTxtWasPressed(ref Boolean hasPaused, ref bool inGameRunning, Rectangle mouseRect)
        {
            if (mouseRect.Intersects(imgRec[8]))
            {
                if (!hasPaused)
                {
                    PauseGame(ref hasPaused, ref inGameRunning);
                }
                else
                {
                    // you paused the game! you bastard!

                    UnpauseGame(ref hasPaused, ref inGameRunning);
                }
            }

            else
            {
                if (!hasPaused)
                {
                    okToShot = true;

                    //si llego al final pos, pues puedes hacer esto
                    if (laserReachedFinalPos)
                    {
                        posFinalLaser = crosshairPos;
                    }
                }

                else
                {
                    okToShot = false;
                }
            }
        }

        // verifica si presiono el resume img pegado al frame
        public void checkIfResumeImgWasPressed(ref Boolean hasPaused, ref bool inGameRunning, Rectangle mouseRect)
        {
            if (mouseRect.Intersects(imgRec[3]))
            {
                wasPausePressed = true;

                if (!hasPaused)
                {
                    PauseGame(ref hasPaused, ref inGameRunning);
                    // you unpaused the game!
                }
                else
                {
                    // you paused the game! you bastard!

                    UnpauseGame(ref hasPaused, ref inGameRunning);
                }
            }

            else
            {
                wasPausePressed = false;

                if (!hasPaused)
                {
                    okToShot = true;

                    //si llego al final pos, pues puedes hacer esto
                    if (laserReachedFinalPos)
                    {
                        posFinalLaser = crosshairPos;
                    }
                }

                else
                {
                    okToShot = false;
                }
            }
        }

        //verifica si presiono return en el pause screen
        public void checkIfReturnTxtWasPressed(Rectangle mouseRect)
        {
            if (mouseRect.Intersects(imgRec[7]))
            {
                men.BackToMainMenu();
            }
        }

        //main menu btn on pause screen
        public void MainMenuBtn(SpriteBatch sp)
        {
            previous_State = Mouse.GetState();

            //men.assignMouseStuff(newState);

            if (clickedArea.Intersects(imgRec[7]))
            {
                sp.Draw(hudImgs[8], imgRec[7], Color.White);

                if (previous_State.LeftButton == ButtonState.Pressed && newState.LeftButton == ButtonState.Pressed)
                {
                    sp.Draw(hudImgs[8], imgRec[7], Color.LightGray);
                }

                else if (previous_State.LeftButton == ButtonState.Released && newState.LeftButton == ButtonState.Pressed)
                {
                    //men.backBtn();

                    men.BackToMainMenu();
                }
            }

            else
            {
                sp.Draw(hudImgs[8], imgRec[7], Color.White);
            }

            //newState = previous_State; 
        }

        // hace el calulo de mover el laser shot
        public void UserShootsALaser(ref Boolean hasPaused, SoundEffect shot, Boolean isSoundOn, ref bool inGameRunning)
        {
            previous_State = mousymouse;

            mousymouse = Mouse.GetState();

            clickedArea = new Rectangle(mousymouse.X, mousymouse.Y, 30, 30);

            if (men.GetSplashScreenTimer() > 199)
            {
                if (mousymouse.LeftButton == ButtonState.Pressed && previous_State.LeftButton == ButtonState.Released)
                {
                    if (!playerDied)
                    {
                        checkIfResumeImgWasPressed(ref hasPaused, ref inGameRunning, clickedArea);

                        //checkIfReturnTxtWasPressed(clickedArea);

                        CheckIfResumeTxtWasPressed(ref hasPaused, ref inGameRunning, clickedArea);



                        //pon el vol en el pause screen si fue presionado el pause
                        if (hasPaused)
                        {
                            //men.volColli(imgRec[10], imgRec[9], clickedArea);
                        }

                        else
                        {
                            sE.CheckForEnemyCollision(sE.GetEnemyRecs(), crossHColiBox, imgRec[3], clickedArea);
                        }
                    }
                }

                #region calcula para donde va el laser

                if (okToShot && wasPausePressed == false)
                {
                    diferenciaDist = posFinalLaser - initialLaserPosition;

                    diferenciaDist2 = posFinalLaser - initialLaserPosition2;

                    int temp1 = Convert.ToInt32(initialLaserPosition.X),
                        temp2 = Convert.ToInt32(initialLaserPosition.Y);

                    #region laser

                    if (!((temp1).Equals(Convert.ToInt32(posFinalLaser.X))))
                    {
                        laserReachedFinalPos = false;

                        #region left laser

                        Vector2 newPos = new Vector2();

                        //mientras mas alto el multiplo mas rapido va y mas rapido puede presionar
                        newPos.X = diferenciaDist.X * 0.7f + initialLaserPosition.X;

                        newPos.Y = diferenciaDist.Y * 0.7f + initialLaserPosition.Y;

                        initialLaserPosition = newPos;

                        imgRec[2].X = Convert.ToInt32(initialLaserPosition.X);

                        imgRec[2].Y = Convert.ToInt32(initialLaserPosition.Y);

                        #endregion

                        #region right laser

                        Vector2 newPos2 = new Vector2();

                        //mientras mas alto el multiplo mas rapido va y mas rapido puede presionar
                        newPos2.X = diferenciaDist2.X * 0.7f + initialLaserPosition2.X;

                        newPos2.Y = diferenciaDist2.Y * 0.7f + initialLaserPosition2.Y;

                        initialLaserPosition2 = newPos2;

                        imgRec[5].X = Convert.ToInt32(initialLaserPosition2.X);

                        imgRec[5].Y = Convert.ToInt32(initialLaserPosition2.Y);

                        #endregion

                        if (isSoundOn)
                        {
                            if (shotOnceflag)
                            {
                                shot.Play();

                                shotOnceflag = false;
                            }
                        }
                    }

                    else
                    {
                        laserReachedFinalPos = true;

                        #region left laser

                        initialLaserPosition.X = -60;

                        initialLaserPosition.Y = 500;

                        imgRec[2].X = Convert.ToInt32(initialLaserPosition.X);

                        imgRec[2].Y = Convert.ToInt32(initialLaserPosition.Y);

                        #endregion

                        #region right laser

                        initialLaserPosition2.X = 860;

                        initialLaserPosition2.Y = 500;

                        imgRec[5].X = Convert.ToInt32(initialLaserPosition2.X);

                        imgRec[5].Y = Convert.ToInt32(initialLaserPosition2.Y);

                        #endregion

                        okToShot = false;

                        shotOnceflag = true;
                    }

                    #endregion

                }

                #endregion
            }
        }

        // regresa la posicion del laser para que se pueda usar cuando se dibuja en el menu
        public Rectangle GetLaserRect()
        {
            return imgRec[2];
        }

        public Rectangle GetLaserRect2()
        {
            return imgRec[5];
        }

        //asigna los default values a los necesarios
        public void reset()
        {
            //hp bar
            hpBarLowLayerLength = 391;

            //hp bar
            hpBarTopLayerLength = 360;

            //player not hit
            sE.SetPlayerHit(false);

            //player not hit
            pHit = false;
        }

        // cambia los colores al HUD
        public void ChangeHudColors(Color hd)
        {
            hdColor = hd;
        }

        //cuando el player le den esto va actualizar su vida de acuerdo al dmg del enemigo
        public void calcPlayerHp(int enemyDmg, SpriteBatch sp)
        {
            if (hpBarLowLayerLength > 0f)
            {
                hpBarLowLayerLength -= enemyDmg;

                hpBarTopLayerLength -= enemyDmg;

                //evita que el valor no sea menor a 0
                hpBarLowLayerLength = (int)MathHelper.Clamp((float)hpBarLowLayerLength, 0.0f, 391.0f);

                hpBarTopLayerLength = (int)MathHelper.Clamp((float)hpBarTopLayerLength, 0.0f, 360.0f);

                updatePlayerHp();
            }

            // no puede ser else por que necesito que haga los 2
            if (hpBarLowLayerLength < 1)
            {
                playerDied = true;
            }
        }

        // plays screen cracked and stops accelerometro
        public void gameOverAnimation(SpriteBatch sp)
        {
            if (screenValue > 0.50)
            {
                isInGameRunning = false;

                men.SetIsGameRunning(isInGameRunning);

                PlaySound(ref playPlayerExplosionOnce, playerExplosionSound);

                sp.Draw(hudImgs[1], Vector2.Zero, Color.White * screenValue);

                screenValue -= 0.015f;

                AccelStop();
            }

            else
            {
                playPlayerExplosionOnce = false;
            }
        }

        //coje las coordenadas del origen del centro de cada imagen y la pone en centerImgsOrigin
        public void AssingImgsCenterOrigin()
        {
            //hp layer
            centerImgsOrigin[0] = new Vector2((hudImgs[2].Width) / 2, (hudImgs[2].Height) / 2);

            //hp layer
            centerImgsOrigin[1] = new Vector2((hudImgs[3].Width) / 2, (hudImgs[3].Height) / 2);

            // crosshair
            centerImgsOrigin[2] = new Vector2((hudImgs[5].Width) / 2, (hudImgs[5].Height) / 2);

            centerImgsOrigin[2].X -= 22;

            centerImgsOrigin[2].Y -= 19;
        }

        //corre la animacion del cracked screen
        public void playScreenCrackAnimation(SpriteBatch sp, bool hit)
        {
            if (hit)
            {
                if (!playerDied)
                {
                    PlaySound(ref playCrackedScreenOnce, screenCrackSound);

                    pHit = hit;

                    // si el valor del screen es mayor a zero por se le resta
                    if (screenValue > 0)
                    {
                        sp.Draw(hudImgs[1], Vector2.Zero, Color.White * screenValue);

                        screenValue -= 0.015f;
                    }

                    else
                    {
                        playCrackedScreenOnce = false;

                        screenValue = 1;

                        pHit = false;
                    }
                }

                else
                {
                    pHit = true;

                    gameOverAnimation(sp);
                }
            }
        }

        //warning mode animation
        private void playWarningMode(SpriteBatch sp, bool kill, SpriteFont sf)
        {
            //si el enemy is not dead do this
            if (!kill)
            {
                if (red)
                {
                    ChangeHudColors(Color.Red);

                    if (counter < 20)
                    {
                        counter++;
                    }

                    else
                    {
                        counter = 0;

                        red = false;
                    }
                }

                else
                {
                    //displayHD(sp, sf);

                    ChangeHudColors(Color.White);

                    if (counter < 20)
                    {
                        counter++;
                    }

                    else
                    {
                        counter = 0;

                        red = true;
                    }
                }
            }

                //si el enemy is dead, do this
            else
            {
                ChangeHudColors(Color.White);

                displayHD(sp, sf);
            }
        }

        //regresa el state para dejar saber que el inGame mode sigue corriendo 
        public Boolean GetIsInGameRunning()
        {
            return isInGameRunning;
        }

        // asigna el state del InGameRunning de menu
        public void SetIsInGameRunning(bool isRunning)
        {
            isInGameRunning = isRunning;
        }

        // asigna el state de pause que viene de menu
        public void SetHasInGamePaused(bool hasPaused)
        {
            isInGamePaused = hasPaused;
        }

        //display multiplier imgs
        public void DisplayMultiplier(SpriteBatch sp)
        {
            if (sE.GetIsMultActive())
            {
                if (sE.GetMultMultiplo() == 20)
                {
                    sp.Draw(hudImgs[10], new Vector2(150, 370), Color.White);
                }

                else if (sE.GetMultMultiplo() == 40)
                {
                    sp.Draw(hudImgs[11], new Vector2(150, 370), Color.White);
                }

                else if (sE.GetMultMultiplo() == 60)
                {
                    sp.Draw(hudImgs[12], new Vector2(150, 370), Color.LightYellow);
                }

                else if (sE.GetMultMultiplo() > 79)
                {
                    sp.Draw(hudImgs[13], new Vector2(150, 370), Color.Gold);
                }

                //sp.DrawString(sf, sE.GetMultMultiplo().ToString() + "X", new Vector2(150, 423), Color.MediumBlue);
            }
        }

        //display HUD normal
        public void displayHD(SpriteBatch sp, SpriteFont sf)
        {
            if (isNotFirstTimePlaying)
            {
                sp.Draw(hudImgs[0], new Rectangle(0, 0, hudImgs[0].Width - 10, hudImgs[0].Height), hdColor);

                sp.Draw(hudImgs[3], imgRec[1], null, hdColor, 0, centerImgsOrigin[1], SpriteEffects.None, 0);

                sp.Draw(hudImgs[2], imgRec[0], null, hdColor, 0, centerImgsOrigin[0], SpriteEffects.None, 0);

                sp.Draw(hudImgs[4], imgRec[3], hdColor);

                //High score
                if (isInGameRunning)
                    sp.DrawString(sf, men.GetHighScore().ToString(), new Vector2(370, 10), Color.LightGray);

                //pts
                sp.DrawString(sf, sE.GetPts().ToString(), new Vector2(580, 435), Color.LightGray);

                DisplayMultiplier(sp);

                //sp.DrawString(sf, "red: " + red.ToString(), new Vector2(100, 200), Color.Yellow);
            }
        }

        //display crosshair. Lo tengo aparte para qu siempre apareca y no se afectado por el warning mode
        public void displayCrosshair(SpriteBatch sp, SpriteFont sf)
        {
            sp.Draw(hudImgs[5], imgRec[4], null, Color.White, 0, centerImgsOrigin[2], SpriteEffects.None, 0);
        }

        /*
         * Por alguna razon podias collide con todo y eso con no estuvieran dibujados.
         * Se llama cada vez que empieze un nuevo juego.
         */
        public void ResetResumeNReturnRects()
        {
            imgRec[7] = new Rectangle();

            imgRec[8] = new Rectangle();
        }

        private void PauseVolBtns(SpriteBatch sp)
        {
            //cada vez que se entra a un menu se necesita empezar con esto para empezar con un nuevo state
            //previous_State = Mouse.GetState();

            //clickedArea = new Rectangle(previous_State.X, previous_State.Y, 30, 30);

            previous_State = Mouse.GetState();

            #region On

            if (clickedArea.Intersects(imgRec[9]))
            {
                if (previous_State.LeftButton == ButtonState.Pressed && newState.LeftButton == ButtonState.Pressed)
                {
                    sp.Draw(hudImgs[14], imgRec[9], Color.LightBlue);
                }

                else if (previous_State.LeftButton == ButtonState.Released && newState.LeftButton == ButtonState.Pressed)
                {
                    sp.Draw(hudImgs[14], imgRec[9], Color.LightBlue);

                    men.SetIsSoundOn(true);

                    //soundOn = true;

                    //// si el ultimo menu num fue ingame mode do this
                    //if (lastMenuNum == 1)
                    //{
                    //    MediaPlayer.Play(menuSong);

                    //    lastMenuNum = menuNumber;
                    //}

                    //else
                    //{
                    MediaPlayer.Resume();
                    //}

                    men.playBtnSound();
                }

                else
                {
                    if (men.SetIsSoundOn())
                    {
                        sp.Draw(hudImgs[14], imgRec[9], Color.LightBlue);
                    }

                    else
                    {
                        sp.Draw(hudImgs[14], imgRec[9], Color.White);
                    }
                }
            }

            else
            {
                if (men.SetIsSoundOn())
                {
                    sp.Draw(hudImgs[14], imgRec[9], Color.LightBlue);
                }

                else
                {
                    sp.Draw(hudImgs[14], imgRec[9], Color.White);
                }
            }

            //newState = previous_State;            

            #endregion

            #region off

            //previous_State = Mouse.GetState();

            //clickedArea = new Rectangle(previous_State.X, previous_State.Y, 30, 30);

            if (clickedArea.Intersects(imgRec[10]))
            {
                if (previous_State.LeftButton == ButtonState.Pressed && newState.LeftButton == ButtonState.Pressed)
                {
                    sp.Draw(hudImgs[15], imgRec[10], Color.LightBlue);
                }

                else if (previous_State.LeftButton == ButtonState.Released && newState.LeftButton == ButtonState.Pressed)
                {
                    sp.Draw(hudImgs[15], imgRec[10], Color.LightBlue);

                    men.playBtnSound();

                    men.SetIsSoundOn(false);

                    //soundOn = false;

                    MediaPlayer.Pause();
                }

                else
                {
                    if (!men.SetIsSoundOn())
                    {
                        sp.Draw(hudImgs[15], imgRec[10], Color.LightBlue);
                    }

                    else
                    {
                        sp.Draw(hudImgs[15], imgRec[10], Color.White);
                    }
                }
            }

            else
            {
                if (!men.SetIsSoundOn())
                {
                    sp.Draw(hudImgs[15], imgRec[10], Color.LightBlue);
                }

                else
                {
                    sp.Draw(hudImgs[15], imgRec[10], Color.White);
                }
            }

            #endregion

            //newState = previous_State;
        }

        private void PauseRetryBtn(SpriteBatch sp)
        {
            previous_State = Mouse.GetState();

            if (clickedArea.Intersects(imgRec[11]))
            {
                sp.Draw(hudImgs[17], imgRec[11], Color.White);

                if (previous_State.LeftButton == ButtonState.Pressed && newState.LeftButton == ButtonState.Pressed)
                {
                    sp.Draw(hudImgs[17], imgRec[11], Color.LightBlue);
                }

                else if (previous_State.LeftButton == ButtonState.Released && newState.LeftButton == ButtonState.Pressed)
                {
                    sp.Draw(hudImgs[17], imgRec[11], Color.LightBlue);

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

                    men.BeforeArcadeStarts();

                    UnpauseGame(ref isInGamePaused, ref isInGameRunning);
                }
            }

            else
            {
                sp.Draw(hudImgs[17], imgRec[11], Color.White);
            }
        }

        //displays pause screen si se presiona pause
        public void DisplayPauseScrn(SpriteBatch sp)
        {
            //previous_State = Mouse.GetState();

            //mousymouse = Mouse.GetState();

            //clickedArea = new Rectangle(previous_State.X, previous_State.Y, 30, 30);

            if (isInGamePaused)
            {
                //screen
                sp.Draw(hudImgs[7], imgRec[6], Color.White);

                //return to menu img
                imgRec[7] = new Rectangle(122, 340, hudImgs[8].Width + 155, hudImgs[8].Height + 10);

                //return
                //sp.Draw(hudImgs[8], imgRec[7], Color.White);

                MainMenuBtn(sp);

                //resume to game img
                imgRec[8] = new Rectangle(430, 85, hudImgs[9].Width - 30, hudImgs[9].Height - 10);

                //resume
                sp.Draw(hudImgs[9], imgRec[8], Color.White);

                //VOLUMEN open
                sp.Draw(hudImgs[16], new Rectangle(428, 153, hudImgs[16].Width + 42, hudImgs[16].Height + 16), Color.White);

                PauseVolBtns(sp);

                PauseRetryBtn(sp);

                newState = previous_State;

                //on
                //sp.Draw(menuImgs[1], imgRec[9], Color.White);

                ////off
                //sp.Draw(menuImgs[2], imgRec[10], Color.White);
            }
        }

        /*
         * verifica la distancia entre el screen y el enemigo. mAXaNCHO = ancho maximo indicado por imagen, anchoActual = al ancho que tiene al momento.
         * De acuerdo a cuan serca o lejos plays warning mode or displaytHD 
        */
        public void checkDistanceFromScreen(int maxAncho, int anchoActual, SpriteBatch sp, SpriteFont sf, bool k)
        {
            float divisor = 1.5f,// por cuanto se divide
                widthTillImpact; // la distancia entre enemigo y screen

            //distancia para que coque puede ser 100. Cada vez que de 100 el actualSize, pone el warning mode
            widthTillImpact = ((float)maxAncho) / divisor;

            if (anchoActual >= (int)widthTillImpact)
            {
                playWarningMode(sp, k, sf);
            }

            else if (anchoActual <= (int)widthTillImpact)
            {
                displayHD(sp, sf);

                DisplayPauseScrn(sp);
            }

            //sp.DrawString(sf, "kill: " + k.ToString(), new Vector2(100, 300), Color.Yellow);
        }
    }
}