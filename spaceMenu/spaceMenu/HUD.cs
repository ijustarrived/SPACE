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

namespace newSpace2_5
{
    /*Notes, stuff to do and ideas 15/dec/2014
     *
     * El vol en el pause screen, debe ser igual que en el option. Change that when I can.
     * 
     * Si pierdes y hay un enemy in range se queda running en warning mode, no se si dejarlo asi.
     *  
     * Check si puedo hacer que el laser valla mas rapido y empieze donde se supone. 
     * 
     * Maybe poner adds en el pause screen
     * 
     */

    //brega todo lo que tiene que ver con el HUD
    class HUD
    {
        #region class variables

        private Menu men;

        private SpaceEnemies sE;

        private SoundEffect screenCrackSound,
            playerExplosionSound;

        private Texture2D[] hudImgs = new Texture2D[10], // todas las imagenes
            menuImgs = new Texture2D[3]; // tiene las imagenes del volumen

        private Rectangle[] imgRec = new Rectangle[11]; //rectangulos del top y bot layer del hp, enemy hp y laser shot

        private Rectangle crossHColiBox; // crosshair collision box es un rect small encima del rect del crosshair y con eso es que se verifica colision

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
            mousymouse;

        private float menuNum = 0, // tiene el valor del current section
            screenValue = 1; // la misma variable como trans pero para el screen para qu haga el mismo efecto

        #region bools

        private bool red = true, // flag par que cambie a rojo todo
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
            hpBarLowLayerLength = 391, // valor que vas a estar cambiando con cada hit del player para asi hacer el hp que baje
            hpBarTopLayerLength = 360,
            points = 0,
            missedShots = 0,
            ptsMult = 0; // tiene el multiplo del point multiplier

        #endregion

        private Color hdColor = Color.White, // se usa para quitarle valor en el gameOver animation. Para que todo valla de rojo a negro
            ptsFontColor = Color.Navy; // tiene el color del font de los points        

        #endregion

        //coje valores del main y los pasa a la clase
        public void GetFxs(SoundEffect fx, SoundEffect fx2)
        {
            screenCrackSound = fx;

            playerExplosionSound = fx2;
        }

        //coje el valor de menu y lo pasa a la variable de hd
        public void GetIsSoundOn()
        {
            isSoundOn = men.SetIsSoundOn();
        }

        //play cualquier fx que se necesite repetir
        public void PlaySound(ref bool onceFlag, SoundEffect fx)
        {
            if (isSoundOn && (!onceFlag))
            {
                onceFlag = true;

                fx.Play();
            }
        }

        //play cualquier fx
        public void PlaySound( SoundEffect fx)
        {
            if (isSoundOn)
            {
                fx.Play();
            }
        }

      public void getMenu(Menu m)
      {
          men = m;
      }

      public void GetSpaceEnemies(SpaceEnemies s)
      {
          sE = s;
      }

        //para usar en map para poder dejarle saber que empieze a blacken el backgeound. Part of the gameOver animation
        public Boolean setPlayerDied()
        {
            return playerDied;
        }

        // asigna el valor cuando halla hecho ResetInGame de menu
        public void getPlayerDied(bool isDead)
        {
            playerDied = isDead;
        }

        //para que se usen en el scoreboard
        public int setPoints()
        {
            return points;
        }

        //para que se usen en el scoreboard
        public int setMissedShots()
        {
            return missedShots;
        }

        //pone todas las imagenes en hudImgs para que se usen en la clase
        public void getHUDImg(Texture2D[] imgList, Texture2D[] menImgs)
        {
            for (int i = 0; imgList.Length > i; i++)
            {
                hudImgs[i] = imgList[i];
            }

            // volumen open
            menuImgs[0] = menImgs[3];

             // off
            menuImgs[1] = menImgs[8];

             // on
            menuImgs[2] = menImgs[9];
        }

        // regresa la imagen de laser para que se pueda dibujar en el menu
        public Texture2D setLaserImg()
        {
            return hudImgs[6];
        }

        //asigna el current section number
        public void getMenuNum(float num)
        {
            menuNum = num;
        }

        //regresa cuando section cambia 4
        public float setMenuNum()
        {
            return menuNum;
        }

        // initializa el accelerometro
        public void InitAccel()
        {
            acc = new Accelerometer();

            acc.ReadingChanged += new EventHandler<AccelerometerReadingEventArgs>(AccelerometerReadingChanged);
        }

        //lee la data del acelerometro
        public void AccelerometerReadingChanged(object sender, AccelerometerReadingEventArgs e)
        {
            accelReadings.X = -(float)e.Y;
            accelReadings.Y = -(float)e.X;
            accelReadings.Z = -(float)e.Z;
        }

        //le dice al acelerometro que empieze a leer. Al momento siempre esta leyendo
        public void AccelStart()
        {
            if(!(accelActive))
            {
                acc.Start();

                accelActive = true;
            }
        }

        //le dice al acelerometro que pare de leer. Al momento siempre esta leyendo
        public void AccelStop()
        {
            if (accelActive)
            {
                acc.Stop();

                accelActive = false;
            }
        }

        //le sube y baja las coordenadas del crosshair
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

                        imgRec[4].Y +=  Convert.ToInt32(accelReadings.Y * 40);

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

        // para enviar el valor a spaceenemy
        public float setScreenValue()
        {
            return screenValue;
        }

        //set player hit
        public bool setPHit()
        {
            return pHit;
        }

        // para recoger el valor de spaceenemy
        public void getScreenValue(float value)
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
            imgRec[3] = new Rectangle(365, 417, 70, 70);

            // full crosshair box
            imgRec[4] = new Rectangle(Convert.ToInt32(crosshairPos.X), Convert.ToInt32(crosshairPos.Y), hudImgs[5].Width - 70, hudImgs[5].Height - 70);

            // laser right
            imgRec[5] = new Rectangle(Convert.ToInt32(initialLaserPosition.X), Convert.ToInt32(initialLaserPosition.Y),
                hudImgs[6].Width - 250, hudImgs[6].Height - 250);            

            /* crosshair small box. How this works es que hay un small rect en el medio del crosshair, pero doesn't bound el crosshair y eso es lo que verifica si
             colisiona con los meteoros */
            crossHColiBox = new Rectangle(Convert.ToInt32(crosshairPos.X), Convert.ToInt32(crosshairPos.Y), hudImgs[5].Width - 120, hudImgs[5].Height - 120);

            //pause screen
            imgRec[6] = new Rectangle(110, 70, hudImgs[7].Width - 680, hudImgs[7].Height - 450);

            //return to menu img
            imgRec[7] = new Rectangle(250, 300, hudImgs[8].Width - 320, hudImgs[8].Height - 40);

            //resume to game img
            imgRec[8] = new Rectangle(290, 130, hudImgs[9].Width - 140, hudImgs[8].Height- 40);

            //on Img
            imgRec[9] = new Rectangle(400, 220, menuImgs[1].Width, menuImgs[1].Height);

            //off Img
            imgRec[10] = new Rectangle(470, 220, menuImgs[2].Width, menuImgs[2].Height);
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

        // verifica si presiono resume en el pause screen
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

        // verifica si presiono el pause pegado al frame
        public void checkIfReturnImgWasPressed(ref Boolean hasPaused, ref bool inGameRunning, Rectangle mouseRect)
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

        //verifica si return en el pause screen
        public void checkIfReturnTxtWasPressed(Rectangle mouseRect)
        {
            if (mouseRect.Intersects(imgRec[7]))
            {
                men.ResetInGame();
            }
        }

        // hace el calulo de mover el laser shot
        public void UserShootsALaser( ref Boolean hasPaused, SoundEffect shot, Boolean isSoundOn, ref bool inGameRunning)
        {
            previous_State = mousymouse;

                mousymouse = Mouse.GetState();

                Rectangle clickedArea = new Rectangle(mousymouse.X, mousymouse.Y, 30, 30);

                if (mousymouse.LeftButton == ButtonState.Pressed && previous_State.LeftButton == ButtonState.Released)
                {
                    if (!playerDied)
                    {
                        checkIfReturnImgWasPressed(ref hasPaused, ref inGameRunning, clickedArea);

                        checkIfReturnTxtWasPressed(clickedArea);

                        CheckIfResumeTxtWasPressed(ref hasPaused, ref inGameRunning, clickedArea);

                        //pon el vol en el pause screen si fue presionado el pause
                        if (hasPaused)
                        {
                            men.volColli(imgRec[10], imgRec[9], clickedArea);
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

        // regresa la posicion del laser para que se pueda usar cuando se dibuja en el menu
        public Rectangle setLaserRect()
        {
            return imgRec[2];
        }

        public Rectangle setLaserRect2()
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
        private void ChangeHudColors(Color hd, Color font)
        {
            hdColor = hd;

            ptsFontColor = font;
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

            // no puede ser else por que ncesito que haga los 2
            if (hpBarLowLayerLength == 0f)
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

        public void playScreenCrackAnimation(SpriteBatch sp, bool hit)
        {
            if (hit)
            {
                if (!playerDied)
                {
                    PlaySound(ref playCrackedScreenOnce, screenCrackSound);

                    pHit = hit;

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
                    ChangeHudColors(Color.Red, Color.White);

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

                    ChangeHudColors(Color.White, Color.Navy);

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
                ChangeHudColors(Color.White, Color.Navy);

                displayHD(sp, sf);
            }
        }

        //regresa el state para dejar saber que el inGame mode sigue corriendo 
        public Boolean setIsInGameRunning()
        {
            return isInGameRunning;
        }

        // asigna el state del InGameRunning de menu
        public void getIsInGameRunning(bool isRunning)
        {
            isInGameRunning = isRunning;
        }

        // asigna el state de pause que viene de menu
        public void getHasInGamePaused(bool hasPaused)
        {
            isInGamePaused = hasPaused;
        }

        //display HUD normal
        public void displayHD(SpriteBatch sp, SpriteFont sf)
        {
                sp.Draw(hudImgs[0], Vector2.Zero, hdColor);

                sp.Draw(hudImgs[3], imgRec[1], null, hdColor, 0, centerImgsOrigin[1], SpriteEffects.None, 0);

                sp.Draw(hudImgs[2], imgRec[0], null, hdColor, 0, centerImgsOrigin[0], SpriteEffects.None, 0);

                sp.Draw(hudImgs[4], imgRec[3], hdColor);

            //pts
                sp.DrawString(sf, sE.GetPts().ToString(), new Vector2(530, 423), ptsFontColor);

                if (sE.GetIsMultActive())
                {
                    sp.DrawString(sf, sE.GetMultMultiplo().ToString() + "X", new Vector2(150, 423), Color.MediumBlue);
                }
        }

        //display crosshair. Lo tengo aparte para qu siempre apareca y no se afectado por el warning mode
        public void displayCrosshair(SpriteBatch sp, SpriteFont sf)
        {
            sp.Draw(hudImgs[5], imgRec[4], null, Color.White, 0, centerImgsOrigin[2], SpriteEffects.None, 0);           
        }

        //displays pause screen si se presiona pause
        public void DisplayPauseScrn(SpriteBatch sp)
        {
            if (isInGamePaused)
            {
                //screen
                sp.Draw(hudImgs[7], imgRec[6], Color.White);

                //return
                sp.Draw(hudImgs[8], imgRec[7], Color.White);

                //resume
                sp.Draw(hudImgs[9], imgRec[8], Color.White);

                //VOLUMEN open
                sp.Draw(menuImgs[0], new Vector2(270, 220), Color.White);
                
                //on
                sp.Draw(menuImgs[1], imgRec[9], Color.White);

                //off
                sp.Draw(menuImgs[2], imgRec[10], Color.White);
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
                playWarningMode(sp,k,sf);
            }

            else if (anchoActual < (int)widthTillImpact)
            {
                displayHD(sp, sf);

                DisplayPauseScrn(sp);
            }
        }
    }
}