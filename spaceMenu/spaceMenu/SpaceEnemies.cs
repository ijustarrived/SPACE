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
    /* Notes, ideas and problems: 4/jan/2014
     * 
     * Verificar que si el fx del screen crack plays y matas un enemigo mientras ese sta playing, el fx del enemy deaths no suena.
     * En otras ocaciones el screen crack fx plays y despues el enemy death fx plays sin haber matado uno
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

        private Texture2D[] Imgs = new Texture2D[9];//array de imagenes de enemigos

        #region rectangles

        private Rectangle[] imgsRec = new Rectangle[3], // tiene todos los rectangulos de cada enemigo
                            explosionImgsRec = new Rectangle[4]; // tiene la misma imagen de explosion, 
                                                                 // pero cada una esta linked a un solo enemigo. Ej: enemy[1] = explosion[1]

        #endregion

        #region ints

        private int[] allEnemyCoorX = new int[] { 80, 100, 200, 400, 600 },//tiene todas las coordenadas en X de los enemigos
                      allEnemyCoorY = new int[] { 80, 100, 210, 280, 350 };//tiene todas las coordenadas en Y de los enemigos

        private int bigIncreaseDimesions = 0, // le deja saber cada cuanto le va a sumar al ancho y largo del enemigo
                    smallIncreaseDimesions = 0,
                    greySmallIncreaseDimesions = 0,

                    enemyKilledCounter = 0, // te deja saber cuantas veces a matado a ese enemigo para luego ir mas rapido cada vez y quitar mas hp

                    bigEnemyDmg = 16, // cuanto quita enemy 1 30 es el default
                    smallEnemyDmg = 11, // default es 15
                    greySmallEnemyDmg = 6,

                    smallGrayAniTimer = 0, // se usa para regular cuanto dura cada animation
                    smallGrayAniPhase = 1, // te deja saber cual animation le toca

                    bigMaxSpeed = 5, //valor que se usa para ver cuando es que se le sube el size a la imagenes. View enemiesGetCloser for example
                    smallMaxSpeed = 5,
                    greySmallMaxSpeed = 5,

                    bigAddToSize = 1, // valor que se le suma al ancho
                    smallAddToSize = 1, // valor que se le suma al ancho
                    greySmallAddToSize = 1,

                    bigEnemyHp = 3, //  tiene cuanta vida tiene el enemy mas grande
                    multTimer = 110, // timer que te deja saber que el multiplier esta activo
                    ptsMult = 0, // tiene el multiplo del point multiplier
                    pts = 0, // points
                    misShots = 0; // amount of missed shots

        #endregion

        private float transBigEnemy = 1, // valor que se multiplica al color para volver la imagen invisible
                      transSmallEnemy = 1,
                      transSmallGrayEnemy = 1,
                      screenVal = 0.0f; // tiene el valor de screenValue de HUD 

        private Vector2[] enemyCoor = new Vector2[3],//array con coordenadas para usar para los enemigos
                          centerImgsOrigin = new Vector2[3]; // tiene las coodenadas del origen en el centro de cada imagen

        #region bools

        private bool isBigKilled = false, // flag que te deja saber si el enemigo grande esta muerto para empezar la animacion de muerte
                     isSmallKilled = false,
                     isGreySmallKilled = false,

                     isBigEnemyExplodePlaying = false, // flag que te deja saber que solo puedes play el fx una vez 
                     isSmallEnemyExplodePlaying = false,
                     isSmallGrayEnemyExplodePlaying = false,

                     didBigChunkRun = false, // es un flag que te deja saber que solo se puede correr ese chunk una vez
                     didSmallChunkRun = false,
                     didGreySmallChunkRun = false,

                     isGraySmallAniRunning = true, // flag que te deja saber que los nuevos animation terminaron y se puede hacer reset al enemy

                     isSmallGrayFxPlaying = false, //  flag que te deja saber si el fx esta corriendo para que no lo corra otravez 

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
        public bool GetBigEnemyKilled()
        {
            return isBigKilled;
        }

        public bool GetSmallEnemyKilled()
        {
            return isSmallKilled;
        }

        public bool GetSmallGrayKilled()
        {
            return isGreySmallKilled;
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

            transBigEnemy = 1;

            bigEnemyHp = 3;

            bigGuyColor.G = 255;
            bigGuyColor.B = 255;
        }

        public void resetSmallGuy()
        {
            imgsRec[1].Width = 0;
            imgsRec[1].Height = 0;

            randomizeCoor();

            imgsRec[1].X = (int)enemyCoor[1].X;
            imgsRec[1].Y = (int)enemyCoor[1].Y;

            transSmallEnemy = 1;
        }

        public void ResetSmallGray()
        {
            imgsRec[2].Width = 0;
            imgsRec[2].Height = 0;

            randomizeCoor();

            imgsRec[2].X = (int)enemyCoor[2].X;
            imgsRec[2].Y = (int)enemyCoor[2].Y;

            transSmallGrayEnemy = 1;

            isSmallGrayFxPlaying = false;
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

            ResetSmallGray();

            bigMaxSpeed = 5;

            smallMaxSpeed = 5;

            greySmallMaxSpeed = 5;

            bigAddToSize = 1;

            smallAddToSize = 1;

            greySmallAddToSize = 1;

            bigEnemyHp = 3;
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
                    hd.PlaySound(ref isBigEnemyExplodePlaying, enemyExplodeSound);
                }

                if (transBigEnemy > 0)
                {
                    //sp.Draw(Imgs[2], imgsRec[0], null, Color.White * transEnemy, 0, centerImgsOrigin[2], SpriteEffects.None, 0);

                    sp.Draw(Imgs[2], imgsRec[0], Color.White * transBigEnemy);

                    transBigEnemy -= 0.015f;
                }

                else
                {
                    isBigEnemyExplodePlaying = false;

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
                    hd.PlaySound(ref isSmallEnemyExplodePlaying, enemyExplodeSound);
                }

                if (transSmallEnemy > 0)
                {
                    //sp.Draw(Imgs[2], imgsRec[1], null, Color.White * transEnemy2, 0, centerImgsOrigin[2], SpriteEffects.None, 0);
                    //"color * trans" lo hace invisible

                    sp.Draw(Imgs[2], imgsRec[1], Color.White * transSmallEnemy);

                    transSmallEnemy -= 0.015f;
                }

                else
                {
                    isSmallEnemyExplodePlaying = false;

                    isSmallKilled = false;

                    resetSmallGuy();
                }
            }

            #endregion

            #region small gray

            if (isGreySmallKilled)
            {
                if ((!playerHit) && (!isSmallGrayFxPlaying))
                {
                    isSmallGrayFxPlaying = true;

                    hd.PlaySound(ref isSmallGrayEnemyExplodePlaying, enemyExplodeSound);
                }

                if (isGraySmallAniRunning)
                {
                    #region p1

                    if (smallGrayAniTimer < 7 && smallGrayAniPhase == 1)
                    {
                        sp.Draw(Imgs[4], imgsRec[2], Color.White);

                        smallGrayAniTimer++;
                    }

                    else if (smallGrayAniTimer >= 7 && smallGrayAniPhase == 1)
                    {
                        smallGrayAniTimer++;

                        smallGrayAniPhase++;
                    }

                    #endregion

                    #region p2

                    if (smallGrayAniTimer < 14 && smallGrayAniPhase == 2)
                    {
                        sp.Draw(Imgs[5], imgsRec[2], Color.White);

                        smallGrayAniTimer++;
                    }

                    else if (smallGrayAniTimer >= 14 && smallGrayAniPhase == 2)
                    {
                        smallGrayAniTimer++;

                        smallGrayAniPhase++;
                    }

                    #endregion

                    #region p3

                    //if (smallGrayAniTimer < 30 && smallGrayAniPhase == 3)
                    //{
                    //    sp.Draw(Imgs[6], imgsRec[2], Color.White);

                    //    smallGrayAniTimer++;
                    //}

                    else if (transSmallGrayEnemy > 0)
                    {
                        //sp.Draw(Imgs[2], imgsRec[1], null, Color.White * transEnemy2, 0, centerImgsOrigin[2], SpriteEffects.None, 0);
                        //"color * trans" lo hace invisible

                        sp.Draw(Imgs[6], imgsRec[2], Color.White * transSmallGrayEnemy);

                        transSmallGrayEnemy -= 0.015f;
                    }

                    else if (transSmallGrayEnemy < 0)
                    {
                        smallGrayAniTimer = 0;

                        smallGrayAniPhase = 1;

                        isGraySmallAniRunning = false;

                        isSmallGrayEnemyExplodePlaying = false;

                        isGreySmallKilled = false;

                        ResetSmallGray();
                    }

                    //else if (smallGrayAniTimer >= 30 && smallGrayAniPhase == 3) 
                    //{
                    //    smallGrayAniTimer = 0;

                    //    smallGrayAniPhase = 1;

                    //    isGraySmallAniRunning = false;
                    //}

                    #endregion
                }

                //else
                //{
                //    smallGrayAniTimer = 0;

                //    smallGrayAniPhase = 1;

                //    isGraySmallAniRunning = false;

                //    playSmallEnemyExplodeOnce = false;

                //    isGreySmallKilled = false;

                //    ResetSmallGray();
                //}
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

        public void GreySmallGuyMovesFaster()
        {
            if (greySmallAddToSize < 8)
            {
                if (greySmallMaxSpeed >= 2)
                {
                    greySmallMaxSpeed--;
                }

                else
                {
                    greySmallAddToSize++;

                    greySmallMaxSpeed = 4;
                }
            }

            else if (greySmallAddToSize < 25)
            {
                greySmallAddToSize++;
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
                didBigChunkRun = true;              

                if (bigIncreaseDimesions == bigMaxSpeed)
                {
                    imgsRec[0].Inflate(bigAddToSize, bigAddToSize);

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
                    if (didBigChunkRun)
                    {
                        MissedAShot();

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

                        didBigChunkRun = false;

                        BigGuyMovesFaster();
                    }

                }
            }

            #endregion

            #region small guy

            if (imgsRec[1].Width < 200)
            {
                didSmallChunkRun = true;

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
                    if (didSmallChunkRun)
                    {
                        MissedAShot();

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

                        didSmallChunkRun = false;

                        SmallGuyMovesFaster();
                    }
                }
            }

            #endregion

            #region greyman small

            if (imgsRec[2].Width < 100)
            {
                didGreySmallChunkRun = true;

                if (greySmallIncreaseDimesions == greySmallMaxSpeed)
                {
                    imgsRec[2].Inflate(greySmallAddToSize, greySmallAddToSize);

                    greySmallIncreaseDimesions = 0;
                }

                else if (greySmallIncreaseDimesions < greySmallMaxSpeed)
                {
                    greySmallIncreaseDimesions += 1;
                }

                else
                {
                    greySmallIncreaseDimesions = 0;
                }
            }

            else
            {
                // sino esta muerto el enemigo, entres al warning seccion
                if (!isGreySmallKilled)
                {
                    if (didGreySmallChunkRun)
                    {
                        MissedAShot();

                        playerHit = true;

                        isGraySmallAniRunning = true;

                        isMultActive = false;

                        hd.calcPlayerHp(greySmallEnemyDmg, sp);

                        isGreySmallKilled = true;

                        if (enemyKilledCounter < 2)
                        {
                            enemyKilledCounter++;
                        }

                        else
                        {
                            enemyKilledCounter = 0;
                        }

                        // default es 10
                        greySmallEnemyDmg += 5;

                        //regula el speed de acuerdo a la muerte de los enemigos

                        didGreySmallChunkRun = false;

                        GreySmallGuyMovesFaster();
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

                if (ptsMult < 81)
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

        // called everytime you miss
        public void MissedAShot()
        {
            misShots++;

            ResetMult();
        }

        // verifica colision de meteoro grande
        public void BigCollision(Rectangle bigEnemyRec, Rectangle grayEnemyRect, Rectangle smallEnemyRect, Rectangle crosshairRect, Rectangle pauseImgRect,
                                 Rectangle mouseRect)
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
                        pts += 15;

                        pts += ptsMult;

                        bigGuyColor = Color.White;

                        isBigKilled = true;

                        isMultActive = true;

                        CalcMultMultiplo();

                        bigEnemyDmg += 15;

                        BigGuyMovesFaster();
                    }
                }
            }

            else
            {
                // por alguna rason dice que si colisiona con el pequeño con todo y eso que no se esta dibujando.
                //Para eso esta este si no esta muerto, verifica eso, si esta muerto, vete directo al else.
               
                    //si dispara no choca con el grande y tampoco con el small one
                if ((!(crosshairRect.Intersects(smallEnemyRect))) && (!(crosshairRect.Intersects(grayEnemyRect))))
                {
                    MissedAShot();
                }

                //if (!(crosshairRect.Intersects(grayEnemyRect)))
                //{
                //    MissedAShot();
                //}
            }
        }

        //verifica colision de enemigo pequeño
        public void SmallCollision(Rectangle smallEnemyRec, Rectangle grayEnemyRect, Rectangle bigEnemyRect, Rectangle crosshairRect, Rectangle pauseImgRect, 
                                   Rectangle mouseRect)
        {
            if (crosshairRect.Intersects(smallEnemyRec))
            {
                if (!mouseRect.Intersects(pauseImgRect))
                {
                    pts += 10;

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
                if ((!(crosshairRect.Intersects(bigEnemyRect))) && (!(crosshairRect.Intersects(grayEnemyRect))))
                {
                    MissedAShot();
                }

                //if (!(crosshairRect.Intersects(grayEnemyRect)))
                //{
                //    MissedAShot();
                //}
            }
        }

        //verifica colision de enemigo pequeño griz
        public void SmallGrayColli(Rectangle grayEnemyRect, Rectangle smallEnemyRec, Rectangle bigEnemyRect, Rectangle crossRect, Rectangle pauseImgRect, 
                                   Rectangle mouseRect)
        {
            if (crossRect.Intersects(grayEnemyRect))
            {
                if (!mouseRect.Intersects(pauseImgRect))
                {
                    pts += 5;

                    pts += ptsMult;

                    isMultActive = true;

                    CalcMultMultiplo();

                    greySmallEnemyDmg += 5;

                    isGreySmallKilled = true;

                    isGraySmallAniRunning = true;

                    GreySmallGuyMovesFaster();
                }
            }

            else
            {
                //si dispara no choca con el grande y tampoco con el small one
                if ((!(crossRect.Intersects(bigEnemyRect))) && (!(crossRect.Intersects(smallEnemyRec))))
                {
                    MissedAShot();
                }

                //if (!(crossRect.Intersects(smallEnemyRec)))
                //{
                //    MissedAShot();
                //}
            }
        }

        //verifica si hay colision con algun enemigo 
        public void CheckForEnemyCollision( Rectangle[] enemyRect, Rectangle crosshiarRect, Rectangle pauseImgRect, Rectangle mouseRect)
        {
            if (!isSmallKilled)
            {
                SmallCollision(enemyRect[1], enemyRect[2], enemyRect[0], crosshiarRect, pauseImgRect, mouseRect);
            }

            if (!isBigKilled)
            {
                BigCollision(enemyRect[0], enemyRect[2], enemyRect[1], crosshiarRect, pauseImgRect, mouseRect);
            }

            if (!isGreySmallKilled)
            {
                SmallGrayColli(enemyRect[2], enemyRect[1], enemyRect[0], crosshiarRect, pauseImgRect, mouseRect);
            }
        }

        //desplega las imagenes de los enemigos y death animation
        public void displayImgs(SpriteBatch sp, SpriteFont sf)
        {
            //sp.DrawString(sf, "smallGrayAniPhase: " + isGreySmallKilled.ToString(), new Vector2(100, 100), Color.Yellow);

            //sp.DrawString(sf, "transSmallGrayEnemy: " + transSmallGrayEnemy.ToString(), new Vector2(100, 200), Color.Yellow);

            //sp.DrawString(sf, "smallGrayAniTimer: " + smallGrayAniTimer.ToString(), new Vector2(100, 300), Color.Yellow);
            
            #region display big meteor

            if (!isBigKilled)
            {
                //sp.Draw(Imgs[0], imgsRec[0], null, Color.White, 0, centerImgsOrigin[0], SpriteEffects.None, 0);

                sp.Draw(Imgs[0], imgsRec[0], bigGuyColor);
            }

            else
            {
                playEnemyDeathAnimation(sp);
            }

            #endregion

            #region display small guy

            if (!isSmallKilled)
            {
                //sp.Draw(Imgs[1], imgsRec[1], null, Color.White, 0, centerImgsOrigin[1], SpriteEffects.None, 0);

                sp.Draw(Imgs[1], imgsRec[1], null, Color.White);
            }

            else
            {
                playEnemyDeathAnimation(sp);
            }

            #endregion

            #region display grey small

            if (!isGreySmallKilled)
            {
                //sp.Draw(Imgs[1], imgsRec[1], null, Color.White, 0, centerImgsOrigin[1], SpriteEffects.None, 0);

                sp.Draw(Imgs[3], imgsRec[2], null, Color.White);
            }

            else
            {
                playEnemyDeathAnimation(sp);
            }

            #endregion
        }
    }
}
