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

namespace newSpace2_5
{
    /* Notes, ideas and problems: 15/dec/2014
     * 
     * Si le disparo al dust de un enemigo no me cuenta como miss. It should miss. No se como arreglarlo. 
     * 
     * Verificar que hay veces que si chocan 2 a la vez, rapido, cracked screen solo pasa una vez.
     * 
     * se pueden: push each other si uno esta encima del otro
     * 
     */

    //se manipulan las imagenes de los enemigos y se le agrega un chispo de animacion al destruirse
    class SpaceEnemies
    {
        #region variables

        private Color bigGuyColor = Color.White;

        private HUD hd;

        private SoundEffect enemyExplodeSound;

        private Texture2D[] Imgs = new Texture2D[3];//array de imagenes de enemigos

        #region rectangles

        private Rectangle[] imgsRec = new Rectangle[3], // tiene todos los rectangulos de cada enemigo
                            explosionImgsRec = new Rectangle[3]; // tiene la misma imagen de explosion, 
                                                                 // pero cada una esta linked a un solo enemigo. Ej: enemy[1] = explosion[1]

        #endregion

        #region ints

        private int[] allEnemyCoorX = new int[] { 80, 100, 200, 400, 600 },//tiene todas las coordenadas en X de los enemigos
                      allEnemyCoorY = new int[] { 80, 100, 210, 280, 350 };//tiene todas las coordenadas en Y de los enemigos

        private int bigIncreaseDimesions = 0, // le deja saber cada cuanto le va a sumar al ancho y largo del enemigo
                    smallIncreaseDimesions = 0,
                    enemyKilledCounter = 0, // te deja saber cuantas veces a matado a ese enemigo para luego ir mas rapido cada vez y quitar mas hp
                    bigEnemyDmg = 31, // cuanto quita enemy 1 30 es el default
                    smallEnemyDmg = 16, // default es 15
                    bigMaxSpeed = 5, //valor que se usa para ver cuando es que se le sube el size a la imagenes. View enemiesGetCloser for example
                    smallMaxSpeed = 5,
                    bigAddToSize = 1, // valor que se le suma al ancho
                    smallAddToSize = 1, // valor que se le suma al ancho
                    bigEnemyHp = 3, //  tiene cuanta vida tiene el enemy mas grande
                    multTimer = 110, // timer que te deja saber que el multiplier esta activo
                    ptsMult = 0, // tiene el multiplo del point multiplier
                    pts = 0, // points
                    misShots = 0; // amount of missed shots

        #endregion

        private float transEnemy = 1, // valor que se multiplica al color para volver la imagen invisible
                      transEnemy2 = 1,
                      screenVal = 0.0f; // tiene el valor de screenValue de HUD 

        private Vector2[] enemyCoor = new Vector2[2],//array con coordenadas para usar para los enemigos
                          centerImgsOrigin = new Vector2[3]; // tiene las coodenadas del origen en el centro de cada imagen

        #region bools

        private bool isBigKilled = false, // flag que te deja saber si el enemigo grande esta muerto para empezar la animacion de muerte
                     isSmallKilled = false,
                     playBigEnemyExplodeOnce = false, // flag que te deja saber que solo puedes play el fx una vez 
                     playSmallEnemyExplodeOnce = false,
                     runLines = false, // es un flag que te deja saber que solo se puede correr algo una sola vez 
                     runLines2 = false,
                     isMultActive = false, // flag que te deja saber que se prende el multiplier
                     playerHit = false; // flag que te deja saber si le dieron al player

        #endregion

        #endregion

        // pasa el objeto de HUD 
        public void SetHd(HUD h)
        {
            hd = h;
        }

        // asigna el valor del fx
        public void SetFx(SoundEffect fx)
        {
            enemyExplodeSound = fx;
        }

        // regresa cantida de enemigos killed, para saber si mueve el bg
        public int GetEnemyCounter()
        {
            return enemyKilledCounter;
        }

        // se usa para calcular el total final en el main
        public int GetPts()
        {
            return pts;
        }

        public void SetPts(int points)
        {
            pts = points;
        }

        // se usa para calcular el total final en el main
        public int GetMiss()
        {
            return misShots;
        }

        //pone todas las imagenes en enemyImgs para que se usen en la clase
        public void SetImgs(Texture2D[] imgList)
        {
            for (int i = 0; imgList.Length > i; i++)
            {
                Imgs[i] = imgList[i];
            }
        }

        //Para recoger el valor de HUD
        public void SetScrnValue(float value)
        {
            screenVal = value;
        }

        //asigna el flag si el player esta hit
        public void SetPlayerHit(bool hit)
        {
            playerHit = hit;
        }

        //regresa los rectangulos de cada enemigo par aque se puedan usar en la validacion de distancia. checkDistanceFromScreen
        public Rectangle[] getEnemyImgRects()
        {
            return imgsRec;
        }

        // para enviar el valor a HUD
        public float GetScreenVal()
        {
            return screenVal;
        }

        //para enviar el valor a HUD
        public bool GetEnemyKilled()
        {
            return isBigKilled;
        }

        public bool GetEnemyKilled2()
        {
            return isSmallKilled;
        }

        // asigna el flag para poder saber si le dieron al player para play el screencrack animation
        public bool GetPlayerHit()
        {
            return playerHit;
        }

        //reset la posision, el valor de trasparencia y el size
        public void resetBigGuy()
        {
            imgsRec[0].Width = 0;
            imgsRec[0].Height = 0;

            randomizeCoor();

            imgsRec[0].X = (int)enemyCoor[0].X;
            imgsRec[0].Y = (int)enemyCoor[0].Y;

            transEnemy = 1;

            bigEnemyHp = 3;
        }

        public void resetSmallGuy()
        {
            imgsRec[1].Width = 0;
            imgsRec[1].Height = 0;

            randomizeCoor();

            imgsRec[1].X = (int)enemyCoor[1].X;
            imgsRec[1].Y = (int)enemyCoor[1].Y;

            transEnemy2 = 1;
        }

        /* Method documentation
         * 
         * se llama al comenzar el juego para evitar que cuando se regrese al menu y le de play otravez las variables 
        de distancia se queden con el mismo valor
         * 
         */
        public void fullReset()
        {
            resetBigGuy();

            resetSmallGuy();

            bigMaxSpeed = 5;

            smallMaxSpeed = 5;

            bigAddToSize = 1;

            smallAddToSize = 1;
        }

        //regresa el array de rect para verificar si algun enemifo esta serca del screen
        public Rectangle[] GetEnemyRecs()
        {
            return imgsRec;
        }

        // sale la imagen del dust y fades slowly
        public void playEnemyDeathAnimation(SpriteBatch sp)
        {
            #region big guy

            if (isBigKilled)
            {
                if (!playerHit)
                {
                    hd.PlaySound(ref playBigEnemyExplodeOnce, enemyExplodeSound);
                }

                if (transEnemy > 0)
                {
                    //sp.Draw(Imgs[2], imgsRec[0], null, Color.White * transEnemy, 0, centerImgsOrigin[2], SpriteEffects.None, 0);

                    sp.Draw(Imgs[2], imgsRec[0], Color.White * transEnemy);

                    transEnemy -= 0.015f;
                }

                else
                {
                    playBigEnemyExplodeOnce = false;

                    isBigKilled = false;

                    resetBigGuy();
                }
            }

            #endregion

            #region small guy

            if (isSmallKilled)
            {
                if (!playerHit)
                {
                    hd.PlaySound(ref playSmallEnemyExplodeOnce, enemyExplodeSound);
                }

                if (transEnemy2 > 0)
                {
                    //sp.Draw(Imgs[2], imgsRec[1], null, Color.White * transEnemy2, 0, centerImgsOrigin[2], SpriteEffects.None, 0);
                    //color * trans lo hace invisible

                    sp.Draw(Imgs[2], imgsRec[1], Color.White * transEnemy2);

                    transEnemy2 -= 0.015f;
                }

                else
                {
                    playSmallEnemyExplodeOnce = false;

                    isSmallKilled = false;

                    resetSmallGuy();
                }
            }

            #endregion
        }

        //inicializa los rectangulos con las coordenadas de las imagenes
        public void initRectangles()
        {
            //use el length de enemyCoor por que la cantidad de coordenadas es la cantidad de enemigos.
            for (int i = 0; i < enemyCoor.Length; i++)
            {
                imgsRec[i] = new Rectangle((int)enemyCoor[i].X, (int)enemyCoor[i].Y, 0, 0);
            }
        }

        //randomiza las coordenadas de los enemigos y las pone en enemyCoor
        public void randomizeCoor()
        {
            Random rand = new Random(),
                rand2 = new Random();

            int randNum = 0;

            //randomly coje una coordenada de allEnemyCoorX & Y y las usa como coordenadas para el array de coordeneas para enemigos 
            for (int i = 0; i < enemyCoor.Length; i++)
            {
                randNum = rand.Next(0, 2);

                //le toca a poner un valor a las coordenadas en X de allEnemyCoorX y en Y que coja random entre 80 & 401
                if (randNum == 0)
                {
                    enemyCoor[i] = new Vector2(allEnemyCoorX[rand.Next(0, allEnemyCoorX.Length)], rand2.Next(80, 401));
                }

                else
                {
                    enemyCoor[i] = new Vector2(rand2.Next(80, 726), allEnemyCoorY[rand2.Next(0, allEnemyCoorY.Length)]);
                }
            }
        }

        //le suma uno al ancho y el largo para dar la impresion que se estan acercando, si le dan a player, brende el flag de hit
        /*
         * Si que valla mas rapido debo jugar con bajando y subiendo el valor de max speed o de addtowidth
         */

        //agrega el mas al size y le resta a cuan rapido se le agrega al size para qu sea mas rapido 
        public void BigGuyMovesFaster()
        {
            if (bigAddToSize < 8)
            {
                //maxspeed es el limite 
                if (bigMaxSpeed >= 2)
                {
                    bigMaxSpeed--;
                }

                else
                {
                    bigAddToSize++;

                    bigMaxSpeed = 4;
                }
            }

            else if (bigAddToSize < 25)
            {
                bigAddToSize++;
            }
        }

        public void SmallGuyMovesFaster()
        {
            if (smallAddToSize < 8)
            {
                if (smallMaxSpeed >= 2)
                {
                    smallMaxSpeed--;
                }

                else
                {
                    smallAddToSize++;

                    smallMaxSpeed = 4;
                }
            }

            else if (smallAddToSize < 25)
            {
                smallAddToSize++;
            }   
        }

        /*
         * regula cuan rapido crece cada imagen menor el # de increaseEnemy mas rapido crece, pero a mayor el width y el
         * height, tambien mas rapido crece.
         */
        public void enemiesGetCloser( SpriteBatch sp, HUD hd)
        {
            #region big guy

            if (imgsRec[0].Width < 300)
            {
                runLines = true;              

                if (bigIncreaseDimesions == bigMaxSpeed)
                {
                    imgsRec[0].Inflate(bigAddToSize, bigAddToSize);

                    //imgsRec[0].Width += bigAddToWidth;

                    //imgsRec[0].Height += bigAddToWidth;

                    bigIncreaseDimesions = 0;
                }

                else if (bigIncreaseDimesions < bigMaxSpeed)
                {
                    bigIncreaseDimesions += 1;
                }

                else
                {
                    bigIncreaseDimesions = 0;
                }
            }

            //player hit
            else
            {
                if (!isBigKilled)
                {
                    if (runLines)
                    {
                        playerHit = true;

                        isMultActive = false;

                        hd.calcPlayerHp(bigEnemyDmg, sp);

                        isBigKilled = true;

                        if (enemyKilledCounter < 2)
                        {
                            enemyKilledCounter++;
                        }

                        else
                        {
                            enemyKilledCounter = 0;
                        }

                        //default es 20
                        bigEnemyDmg += 20;

                        screenVal = 1;

                        runLines = false;

                        BigGuyMovesFaster();
                    }

                }
            }

            #endregion

            #region small guy

            if (imgsRec[1].Width < 200)
            {
                runLines2 = true;

                if (smallIncreaseDimesions == smallMaxSpeed)
                {
                    //imgsRec[1].Width += smallAddToSize;

                    //imgsRec[1].Height += smallAddToSize;

                    imgsRec[1].Inflate(smallAddToSize, smallAddToSize);

                    smallIncreaseDimesions = 0;
                }

                else if (smallIncreaseDimesions < smallMaxSpeed)
                {
                    smallIncreaseDimesions += 1;
                }

                else
                {
                    smallIncreaseDimesions = 0;
                }
            }

                //player hit
            else
            {
                // sino esta muerto el enemigo, entres al warning seccion
                if (!isSmallKilled)
                {
                    if (runLines2)
                    {
                        playerHit = true;

                        isMultActive = false;

                        hd.calcPlayerHp(smallEnemyDmg, sp);

                        isSmallKilled = true;

                        if (enemyKilledCounter < 2)
                        {
                            enemyKilledCounter++;
                        }

                        else
                        {
                            enemyKilledCounter = 0;
                        }

                        // default es 10
                        smallEnemyDmg += 10;

                        //regula el speed de acuerdo a la muerte de los enemigos

                        runLines2 = false;

                        SmallGuyMovesFaster();
                    }
                }
            }

            #endregion
        }

        //asigna el valor del metodo CalcMultTimer
        public void SetIsMultActive(bool isActive)
        {
            isMultActive = isActive;
        }

        //asigna el valor del metodo CalcMultTimer
        public void SetMultTimer(int timer)
        {
            multTimer = timer;
        }

        //asigna el valor del metodo CalcMultTimer
        public void SetMultMultiplo(int multiplo)
        {
            ptsMult = multiplo;
        }

        //para usar el valor en CalcMultMultiplo y CalcMultTimer
        public bool GetIsMultActive()
        {
            return isMultActive;
        }

        //para usar el valor en CalcMultMultiplo y CalcMultTimer
        public int GetMultTimer()
        {
            return multTimer;
        }

        // para que se use en el hud cuando el hud is displayed
        public int GetMultMultiplo()
        {
            return ptsMult;
        }

        // asigna default values
        public void ResetMult()
        {
            isMultActive = false;

            multTimer = 110;

            ptsMult = 0;
        }

        // calcula el multiplo de multiplier
        public void CalcMultMultiplo()
        {
            if (isMultActive)
            {
                //if (timer > 0)
                //{

                multTimer = 110;

                if (ptsMult < 60)
                {
                    ptsMult += 20;
                }
                //}
            }

            else
            {
                ResetMult();
            }
        }

        //caulcula el timer del multiplier
        public void CalcMultTimer(int timer, bool isActive)
        {
            if (isActive)
            {            
                if (timer > 0)
                {
                    timer--;

                    SetMultTimer(timer);

                    SetIsMultActive(true);
                }

                else
                {
                    timer = 110;

                    SetMultMultiplo(0);

                    SetMultTimer(timer);

                    SetIsMultActive(false);
                }
            }
        }

        // verifica colision de meteoro grande
        public void BigCollision(Rectangle bigEnemyRec, Rectangle smallEnemyRect, Rectangle crosshairRect, Rectangle pauseImgRect, Rectangle mouseRect)
        {
            if (crosshairRect.Intersects(bigEnemyRec))
            {
                if(!mouseRect.Intersects(pauseImgRect))
                {
                    if (bigEnemyHp > 0)
                    {
                        bigEnemyHp--;

                        bigGuyColor.B -= 50;

                        bigGuyColor.G -= 50;
                    }

                    else
                    {
                        pts += 10;

                        pts += ptsMult;

                        bigGuyColor = Color.White;

                        isBigKilled = true;

                        isMultActive = true;

                        CalcMultMultiplo();

                        //default es 20
                        bigEnemyDmg += 20;

                        BigGuyMovesFaster();
                    }
                }
            }

            else
            {
                // por alguna rason dice que si colisiona con el pequeño con todo y eso que no se esta dibujando.
                //Para eso esta este si no esta muerto, verifica eso, si esta muerto, vete directo al else.
               
                    //si dispara no choca con el grande y tampoco con el small one
                if (!(crosshairRect.Intersects(smallEnemyRect)))
                {
                    misShots++;

                    ResetMult();
                }
                

                //else
                //{
                //    if (isSmallKilled)
                //    {
                //        misShots++;

                //        ResetMult();
                //    }
                //}
            }
        }

        //verifica colision de enemigo pequeño
        public void SmallCollision(Rectangle smallEnemyRec, Rectangle bigEnemyRect, Rectangle crosshairRect, Rectangle pauseImgRect, Rectangle mouseRect)
        {
            if (crosshairRect.Intersects(smallEnemyRec))
            {
                if (!mouseRect.Intersects(pauseImgRect))
                {
                    pts += 5;

                    pts += ptsMult;

                    isMultActive = true;

                    CalcMultMultiplo();

                    // default es 10
                    smallEnemyDmg += 10;

                    isSmallKilled = true;

                    SmallGuyMovesFaster();
                }
            }

            else
            {                
                //si dispara no choca con el grande y tampoco con el small one
                if (!(crosshairRect.Intersects(bigEnemyRect)))
                {
                    misShots++;

                    ResetMult();
                }
                

                //else
                //{
                //    if (isBigKilled)
                //    {
                //        misShots++;

                //        ResetMult();
                //    }
                    
                //}
            }
        }

        //verifica si hay colision con algun enemigo 
        public void CheckForEnemyCollision( Rectangle[] enemyRect, Rectangle crosshiarRect, Rectangle pauseImgRect, Rectangle mouseRect)
        {
            if (!isSmallKilled)
            {
                SmallCollision(enemyRect[1], enemyRect[0], crosshiarRect, pauseImgRect, mouseRect);
            }

            if (!isBigKilled)
            {
                BigCollision(enemyRect[0], enemyRect[1], crosshiarRect, pauseImgRect, mouseRect);
            }
        }

        //desplega las imagenes de los enemigos y death animation
        public void displayImgs(SpriteBatch sp, SpriteFont sf)
        {
            sp.DrawString(sf, "multiplo: " + ptsMult.ToString(), new Vector2(100, 100), Color.Yellow);

            sp.DrawString(sf, "timer: " + multTimer.ToString(), new Vector2(100, 200), Color.Yellow);

            sp.DrawString(sf, "isMultiActive: " + isMultActive.ToString(), new Vector2(100, 300), Color.Yellow);

            //display big meteor
            if (!isBigKilled)
            {
                //sp.Draw(Imgs[0], imgsRec[0], null, Color.White, 0, centerImgsOrigin[0], SpriteEffects.None, 0);

                sp.Draw(Imgs[0], imgsRec[0], bigGuyColor);
            }

            else
            {
                playEnemyDeathAnimation(sp);
            }

            //display small guy
            if (!isSmallKilled)
            {
                //sp.Draw(Imgs[1], imgsRec[1], null, Color.White, 0, centerImgsOrigin[1], SpriteEffects.None, 0);

                sp.Draw(Imgs[1], imgsRec[1], null, Color.White);
            }

            else
            {
                playEnemyDeathAnimation(sp);
            }
        }
    }
}
