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

namespace newSpace3_4
{
    /* Notes, ideas and problems: 8/mar/2015
     * 
     * When gameover, rarely, pasa que lo enemies siguen moviendo, no se supone que pase(BUG)
     * 
     */

    /// <summary>
    /// Se manipulan las imagenes de los enemigos y se le agrega un chispo de animacion al destruirse
    /// </summary>
    class SpaceEnemies
    {
        #region variables

        private Color bigGuyColor = Color.White, // tiene el color del big y small guy por que son los unicos que cambian con hits
            smallGuyColor = Color.White;

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
                    smallEnemyHp = 1,

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

        /// <summary>
        /// Pasa el objeto de HUD 
        /// </summary>
        /// <param name="h"></param>
        public void SetHd(HUD h)
        {
            hd = h;
        }

        /// <summary>
        /// Asigna el valor del fx
        /// </summary>
        /// <param name="fx"></param>
        public void SetFx(SoundEffect fx)
        {
            enemyExplodeSound = fx;
        }

        /// <summary>
        /// Se usa para calcular el total final en el main
        /// </summary>
        /// <returns>Pts acomulados</returns>
        public int GetPts()
        {
            return pts;
        }

        public void SetPts(int points)
        {
            pts = points;
        }

        /// <summary>
        ///  Se usa para calcular el total final en el main
        /// </summary>
        /// <returns>Missed shots</returns>
        public int GetMiss()
        {
            return misShots;
        }

        /// <summary>
        /// Pone todas las imagenes en enemyImgs para que se usen en la clase
        /// </summary>
        /// <param name="imgList">EnemyImg array</param>
        public void SetImgs(Texture2D[] imgList)
        {
            for (int i = 0; imgList.Length > i; i++)
            {
                Imgs[i] = imgList[i];
            }
        }

        /// <summary>
        /// Para recoger el valor de HUD
        /// </summary>
        /// <param name="value"></param>
        public void SetScrnValue(float value)
        {
            screenVal = value;
        }

        /// <summary>
        /// Asigna el flag si el player esta hit
        /// </summary>
        /// <param name="hit">Player hit boolean</param>
        public void SetPlayerHit(bool hit)
        {
            playerHit = hit;
        }

        /// <summary>
        /// Para que se puedan usar en la validacion de distancia. checkDistanceFromScreen
        /// </summary>
        /// <returns>Los rectangulos de cada enemigo</returns>
        public Rectangle[] GetEnemyImgRects()
        {
            return imgsRec;
        }

        /// <summary>
        ///  Para enviar el valor a HUD
        /// </summary>
        /// <returns>Value para multiplicar con color</returns>
        public float GetScreenVal()
        {
            return screenVal;
        }

        /// <summary>
        /// Para enviar el valor a HUD
        /// </summary>
        /// <returns>Boolean que te deja saber si murio</returns>
        public bool GetBigEnemyKilled()
        {
            return isBigKilled;
        }

        /// <summary>
        /// Para enviar el valor a HUD
        /// </summary>
        /// <returns>Boolean que te deja saber si murio</returns>
        public bool GetSmallEnemyKilled()
        {
            return isSmallKilled;
        }

        /// <summary>
        /// Para enviar el valor a HUD
        /// </summary>
        /// <returns>Boolean que te deja saber si murio</returns>
        public bool GetSmallGrayKilled()
        {
            return isGreySmallKilled;
        }

        /// <summary>
        ///Para poder saber si le dieron al player para play el screencrack animation
        /// </summary>
        /// <returns>Player hit boolean</returns>
        public bool GetPlayerHit()
        {
            return playerHit;
        }

        /// <summary>
        /// Reset la posicion, el valor de trasparencia y el size
        /// </summary>
        public void resetBigGuy()
        {
            imgsRec[0].Width = 0;
            imgsRec[0].Height = 0;

            randomizeCoor();

            imgsRec[0].X = (int)enemyCoor[0].X;
            imgsRec[0].Y = (int)enemyCoor[0].Y;

            transBigEnemy = 1;

            bigEnemyHp = 3;

            bigGuyColor = Color.White;

            //bigGuyColor.G = 255;
            //bigGuyColor.B = 255;
        }

        /// <summary>
        /// Reset la posicion, el valor de trasparencia y el size
        /// </summary>
        public void resetSmallGuy()
        {
            imgsRec[1].Width = 0;
            imgsRec[1].Height = 0;

            randomizeCoor();

            imgsRec[1].X = (int)enemyCoor[1].X;
            imgsRec[1].Y = (int)enemyCoor[1].Y;

            transSmallEnemy = 1;

            smallEnemyHp = 1;

            smallGuyColor = Color.White;

            //smallguyColor.G = 255;

            //smallguyColor.B = 255;
        }
        /// <summary>
        /// Reset la posicion, el valor de trasparencia y el size
        /// </summary>
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

        /// <summary>
        /// Se llama al comenzar el juego para evitar que cuando se regrese al menu y le de play, otravez, las variables
        /// de distancia se queden con el mismo valor.
        /// </summary>
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

            smallEnemyHp = 1;
        }

        /// <summary>
        /// Para verificar si algun enemigo esta serca del screen
        /// </summary>
        /// <returns>Regresa el array de rect de enemigo</returns>
        public Rectangle[] GetEnemyRecs()
        {
            return imgsRec;
        }

        /// <summary>
        /// Sale la imagen del dust y fades slowly
        /// </summary>
        /// <param name="sp">Objeto</param>
        public void playEnemyDeathAnimation(SpriteBatch sp)
        {
            #region big guy

            if (isBigKilled)
            {
                //if (!playerHit)
                //{
                //    hd.PlaySound(ref isBigEnemyExplodePlaying, enemyExplodeSound);
                //}

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
                //if (!playerHit)
                //{
                //    hd.PlaySound(ref isSmallEnemyExplodePlaying, enemyExplodeSound);
                //}

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
                //if ((!playerHit) && (!isSmallGrayFxPlaying))
                //{
                //    isSmallGrayFxPlaying = true;

                //    hd.PlaySound(ref isSmallGrayEnemyExplodePlaying, enemyExplodeSound);
                //}

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

        /// <summary>
        /// Inicializa los rectangulos con las coordenadas de las imagenes
        /// </summary>
        public void initRectangles()
        {
            //use el length de enemyCoor por que la cantidad de coordenadas es la cantidad de enemigos.
            for (int i = 0; i < enemyCoor.Length; i++)
            {
                imgsRec[i] = new Rectangle((int)enemyCoor[i].X, (int)enemyCoor[i].Y, 0, 0);
            }
        }

        /// <summary>
        /// Randomiza las coordenadas de los enemigos y las pone en enemyCoor
        /// </summary>
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
        
        /* Explicacion de movimiento de enemigos
         * Le suma uno al ancho y el largo para dar la impresion que se estan acercando, si le dan a player, brende el flag de hit.
         *
         * Si quiero que valla mas rapido debo bajar y subir el valor de max speed o de addtowidth
         */

        /// <summary>
        /// Agrega mas al size y le resta a cuan rapido se le agrega al size para que sea mas rapido 
        /// </summary>
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

        /// <summary>
        /// Agrega mas al size y le resta a cuan rapido se le agrega al size para que sea mas rapido 
        /// </summary>
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

        /// <summary>
        /// Agrega mas al size y le resta a cuan rapido se le agrega al size para que sea mas rapido 
        /// </summary>
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

        /// <summary>
        /// Regula cuan rapido crece cada imagen. Al ser menor el # de increaseEnemy mas rapido crece, pero a mayor el width y el
        /// height, tambien mas rapido crece.
        /// </summary>
        /// <param name="sp">Objeto</param>
        /// <param name="hd">Objeto</param>
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

        /// <summary>
        /// Asigna el valor del activacion de multiplicador del metodo CalcMultTimer
        /// </summary>
        /// <param name="isActive">Boolean de multiplier activo</param>
        public void SetIsMultActive(bool isActive)
        {
            isMultActive = isActive;
        }

        /// <summary>
        /// Asigna el valor del metodo CalcMultTimer
        /// </summary>
        /// <param name="timer">Valor de timer</param>
        public void SetMultTimer(int timer)
        {
            multTimer = timer;
        }

        /// <summary>
        /// Asigna el valor del multiplo para el metodo CalcMultTimer
        /// </summary>
        /// <param name="multiplo">Valor numerico</param>
        public void SetMultMultiplo(int multiplo)
        {
            ptsMult = multiplo;
        }

        /// <summary>
        /// Para usar el valor en CalcMultMultiplo y CalcMultTimer
        /// </summary>
        /// <returns>Boolean para activacion de multiplo</returns>
        public bool GetIsMultActive()
        {
            return isMultActive;
        }

        /// <summary>
        /// Para usar el valor en CalcMultMultiplo y CalcMultTimer
        /// </summary>
        /// <returns>Valor numerico del timer</returns>
        public int GetMultTimer()
        {
            return multTimer;
        }

        /// <summary>
        /// Para que se use en el hud cuando se necesite desplegar
        /// </summary>
        /// <returns>Valor numerico del multiplo</returns>
        public int GetMultMultiplo()
        {
            return ptsMult;
        }

        /// <summary>
        /// Asigna default values
        /// </summary>
        public void ResetMult()
        {
            isMultActive = false;

            multTimer = 110;

            ptsMult = 0;
        }

        /// <summary>
        /// Calcula el multiplo de multiplier
        /// </summary>
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

        /// <summary>
        /// Calcula el timer del multiplier
        /// </summary>
        /// <param name="timer">Valor numerico del timer</param>
        /// <param name="isActive">Booleano de activacion de multiplo</param>
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

        /// <summary>
        /// Adds to missedShots y resets el multiplier.
        /// Called everytime you miss
        /// </summary>
        public void MissedAShot()
        {
            misShots++;

            ResetMult();
        }

        /// <summary>
        /// Verifica colision del grande
        /// </summary>
        /// <param name="bigEnemyRec">Rectangulo del grande</param>
        /// <param name="grayEnemyRect">Rectangulo del griz</param>
        /// <param name="smallEnemyRect">Rectangulo del small</param>
        /// <param name="crosshairRect">Rectangulo del crosshair</param>
        /// <param name="pauseImgRect">Rectangulo del imagen de pausa</param>
        /// <param name="mouseRect"></param>
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
                        hd.PlaySound(enemyExplodeSound);

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
                // por alguna rason, si colisiona con el pequeño, con todo y eso que no se esta dibujando.
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

        /// <summary>
        /// Verifica colision de enemigo pequeño
        /// </summary>
        /// <param name="smallEnemyRec"></param>
        /// <param name="grayEnemyRect"></param>
        /// <param name="bigEnemyRect"></param>
        /// <param name="crosshairRect"></param>
        /// <param name="pauseImgRect"></param>
        /// <param name="mouseRect"></param>
        public void SmallCollision(Rectangle smallEnemyRec, Rectangle grayEnemyRect, Rectangle bigEnemyRect, Rectangle crosshairRect, Rectangle pauseImgRect, 
                                   Rectangle mouseRect)
        {
            if (crosshairRect.Intersects(smallEnemyRec))
            {
                if (!mouseRect.Intersects(pauseImgRect))
                {
                    if (smallEnemyHp > 0)
                    {
                        smallEnemyHp--;

                        smallGuyColor.B -= 100;

                        smallGuyColor.G -= 100;
                    }
                    else
                    {
                        hd.PlaySound(enemyExplodeSound);

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

        /// <summary>
        /// Verifica colision de enemigo griz
        /// </summary>
        /// <param name="grayEnemyRect"></param>
        /// <param name="smallEnemyRec"></param>
        /// <param name="bigEnemyRect"></param>
        /// <param name="crossRect"></param>
        /// <param name="pauseImgRect"></param>
        /// <param name="mouseRect"></param>
        public void GrayCollision(Rectangle grayEnemyRect, Rectangle smallEnemyRec, Rectangle bigEnemyRect, Rectangle crossRect, Rectangle pauseImgRect, 
                                   Rectangle mouseRect)
        {
            if (crossRect.Intersects(grayEnemyRect))
            {
                if (!mouseRect.Intersects(pauseImgRect))
                {
                    hd.PlaySound(enemyExplodeSound);

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

        /// <summary>
        /// Verifica si hay colision con algun enemigo 
        /// </summary>
        /// <param name="enemyRect">Array de rectangulos de enemigos</param>
        /// <param name="crosshiarRect"></param>
        /// <param name="pauseImgRect"></param>
        /// <param name="mouseRect"></param>
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
                GrayCollision(enemyRect[2], enemyRect[1], enemyRect[0], crosshiarRect, pauseImgRect, mouseRect);
            }
        }

        /// <summary>
        /// Desplega las imagenes de los enemigos y death animation
        /// </summary>
        /// <param name="sp">Objeto</param>
        /// <param name="sf">Objeto</param>
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

                sp.Draw(Imgs[1], imgsRec[1], null, smallGuyColor);
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
