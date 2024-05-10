/*  Created by:
 *  Project: Brick Breaker
 *  Date:
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using BrickBreaker.Screens;

namespace BrickBreaker
{
    public partial class GameScreen : UserControl
    {
        #region global values
        //image variables
        Image A = Properties.Resources.blocks_A1;
        Image B = Properties.Resources.blocks_B1;
        Image C = Properties.Resources.blocks_C1;
        Image D = Properties.Resources.blocks_D1;
        Image E = Properties.Resources.blocks_E1;
        Image Empty = Properties.Resources.blocks_empty1;
        Image rcCarTop = Properties.Resources.RC_top1;
        Image rcCarLeft = Properties.Resources.RC_top_left;
        Image rcCarRight = Properties.Resources.RC_top_right;
        Image ballig = Properties.Resources.toy_story_ball_down1;
        //powerup images
        Image gravityimage = Properties.Resources.Gravity;
        Image healthimage = Properties.Resources.Health;
        Image multiballimage = Properties.Resources.multiball;
        Image extendedpadleimage = Properties.Resources.extended_paddle;
        Image breakthroughimage = Properties.Resources.breakthrough___Copy;

        public static int width;
        public static int height;
        public int colVal = 255;
        //player1 button control keys - DO NOT CHANGE
        Boolean leftArrowDown, rightArrowDown, spaceDown;

        // Game values
        int lives, counter, extraSpeed, counterInterval;

        // Paddle and Ball objects
        Paddle paddle;
        Ball ball;

        public static List<Ball> balls = new List<Ball>();

        // lists for powerups
        List<Powers> powerList = new List<Powers>();

        Stopwatch breakTimer = new Stopwatch();
        Stopwatch gravityTimer = new Stopwatch();
        Stopwatch extendTimer = new Stopwatch();

        public static bool breakthroughBool;
        public static bool gravityBool;
        public static bool extendBool;
        public static bool startBool;
        public static bool rcCarBool;

        public static string level;

        // list of all blocks for current level
        List<Block> blocks = new List<Block>();

        // Brushes
        SolidBrush grayBrush = new SolidBrush(Color.Gray);
        SolidBrush ballBrush = new SolidBrush(Color.White);
        SolidBrush redBrush = new SolidBrush(Color.Red);
        SolidBrush yellowBrush = new SolidBrush(Color.Yellow);
        SolidBrush cyanBrush = new SolidBrush(Color.Cyan);
        SolidBrush greenBrush = new SolidBrush(Color.Green);
        SolidBrush purpleBrush = new SolidBrush(Color.Purple);

        //placeholder brushes for testing powerups
        SolidBrush breakThrough = new SolidBrush(Color.White);
        SolidBrush multiBall = new SolidBrush(Color.Blue);
        SolidBrush gravity = new SolidBrush(Color.Purple);
        SolidBrush extendPaddle = new SolidBrush(Color.Yellow);
        SolidBrush health = new SolidBrush(Color.Red);

        //declare random
        public static Random r = new Random();
        #endregion

        Rectangle rc_car = new Rectangle();

        Pen outlineBrush = new Pen(Color.FromArgb(100, 255, 0, 0));

        Stopwatch ballWatch = new Stopwatch();

        public GameScreen()
        {
            InitializeComponent();
            OnStart();
        }

        public void OnStart()
        {
            height = this.Height;
            width = this.Width;
            // make height and width variables


            //set life counter
            lives = 3;
            counter = extraSpeed = 0;

            List<Label> labels = new List<Label>();

            //set all button presses to false.
            leftArrowDown = rightArrowDown = spaceDown = false;

            //clear blocks list
            blocks.Clear();

            // setup starting paddle values and create paddle object

            int paddleWidth = 80;
            int paddleHeight = 105;
            int paddleX = ((this.Width / 2) - (paddleWidth / 2));
            int paddleY = (this.Height - paddleHeight) - 70;
            int paddleSpeed = 7;
            paddle = new Paddle(paddleX, paddleY, paddleWidth, paddleHeight, paddleSpeed, Color.White);

            // setup starting ball values
            int ballX = this.Width / 2 - 10;
            int ballY = this.Height - paddle.height - 80;

            // Creates a new ball
            int xSpeed = 8;
            int ySpeed = 8;
            int ballSize = 20;

            ball = new Ball(ballX, ballY, xSpeed, ySpeed, ballSize);
            balls.Add(ball);

            XmlReader reader = XmlReader.Create($"Resources/{level}.xml");

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Text)
                {
                    Block b;
                    int x = Convert.ToInt32(reader.ReadString());
                    reader.ReadToFollowing("y");
                    int y = Convert.ToInt32(reader.ReadString());
                    reader.ReadToFollowing("hp");
                    int hp = Convert.ToInt32(reader.ReadString());
                    reader.ReadToFollowing("colour");
                    string color = reader.ReadString();
                    b = new Block(x, y, hp);

                    blocks.Add(b);
                }
            }

            reader.Close();

            rc_car.X = paddle.x;
            rc_car.Y = paddle.y;
            rc_car.Width = paddle.width;
            rc_car.Height = paddle.height;

            counterInterval = 1500;

            // start the game engine loop
            gameTimer.Enabled = true;
        }

        private void GameScreen_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            //player 1 button presses
            switch (e.KeyCode)
            {
                case Keys.Left:
                    leftArrowDown = true;
                    break;
                case Keys.Right:
                    rightArrowDown = true;
                    break;
                case Keys.Space:
                    spaceDown = true;
                    break;
                case Keys.Escape:
                    Application.Exit();
                    break;
                default:
                    break;
            }
        }

        private void GameScreen_KeyUp(object sender, KeyEventArgs e)
        {
            //player 1 button releases
            switch (e.KeyCode)
            {
                case Keys.Left:
                    leftArrowDown = false;
                    break;
                case Keys.Right:
                    rightArrowDown = false;
                    break;
                case Keys.Space:
                    spaceDown = false;
                    break;
                default:
                    break;
            }
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            //test code to slow game down (will be removed for final game)
            if (spaceDown)
            {
                gameTimer.Interval = 100;
            }
            else
            {
                gameTimer.Interval = 1;
            }

            // Move the paddle
            if (leftArrowDown && paddle.x > 0)
            {
                paddle.Move("left");
            }
            if (rightArrowDown && paddle.x < (this.Width - paddle.width))
            {
                paddle.Move("right");
            }

            // Move ball
            foreach (Ball b in balls)
            {
                b.Move();
            }

            // Check for collision with top and side walls
            foreach (Ball b in balls)
            {
                b.WallCollision(this);
            }


            // Check for ball hitting bottom of screen
            for (int i = 0; i < balls.Count; i++)
            {
                if (balls[i].BottomCollision(this))
                {
                    balls.RemoveAt(i);

                    if (balls.Count == 0)
                    {
                        //delete active powerups
                        pressStartLabel.Visible = true;
                        gravityBool = false;
                        breakthroughBool = false;
                        extendBool = false;
                        gravityTimer.Reset();
                        breakTimer.Reset();
                        extendTimer.Reset();

                        lives--;

                        // Moves the ball back to origin
                        ball.xSpeed = 0;
                        ball.ySpeed = 0;
                        balls.Add(ball);
                        balls[i].x = ((paddle.x - (ball.size / 2)) + (paddle.width / 2));
                        balls[i].y = (this.Height - paddle.height) - 105;
                    }

                    if (lives == 0)
                    {
                        gameTimer.Enabled = false;
                        OnEnd();
                    }
                }
            }

            // Check for collision of ball with paddle, (incl. paddle movement)
            foreach (Ball b in balls)
            {
                for (int i = 0; i < balls.Count; i++)
                {
                    if (balls[i].xSpeed == 0 && balls[i].ySpeed == 0)
                    {
                        //do nothing
                    }
                    else
                    {
                        b.PaddleCollision(paddle, extraSpeed);
                    }
                }
            }

            // Check if ball has collided with any blocks
            foreach (Block b in blocks)
            {
                bool keepLooking = true;

                for (int i = 0; i < balls.Count; i++)
                {
                    if (balls[i].BlockCollision(b))
                    {
                        //do not collide if ball has not been launched
                        if (balls[i].xSpeed == 0 && balls[i].ySpeed == 0)
                        {
                            break;
                        }

                        b.hp--;

                        // if block hp = 0
                        if (b.hp == 0)
                        {
                            //random chance to spawn a powerup
                            if (r.Next(1, 3) == 1)
                            {
                                Powers power = new Powers(b.x + (b.width / 2), b.y + (b.height / 2), "");
                                powerList.Add(power);
                            }
                            keepLooking = false;
                            blocks.Remove(b);

                        }

                        if (blocks.Count == 0)
                        {
                            gameTimer.Enabled = false;
                            OnEnd();
                        }

                        break;
                    }

                }
                // break out of loop
                if (keepLooking == false)
                {
                    break;
                }

            }

            // Powers
            foreach (Powers p in powerList)
            {
                //move each powerBall
                if (ball.xSpeed != 0 && ball.ySpeed != 0)
                {
                    p.Move();
                }

                //check for paddle collision to see if the player deserves powerup
                if (p.Collision(paddle))
                {
                    //determine what kind of powerup it is
                    switch (p.type)
                    {
                        case "Breakthrough":
                            //unstoppable ball for duration of time
                            if (breakTimer.IsRunning == true)
                            {
                                breakTimer.Restart();
                            }
                            else
                            {
                                breakTimer.Start();
                                breakthroughBool = true;
                                ballBrush.Color = Color.LightBlue;
                            }
                            break;
                        case "Gravity":
                            //arc balls back upwards
                            if (gravityTimer.IsRunning == true)
                            {
                                gravityTimer.Restart();
                            }
                            else
                            {
                                gravityTimer.Start();
                                gravityBool = true;
                                ballBrush.Color = Color.LightPink;
                            }
                            break;
                        case "Health":
                            //grants the player an extra life, capped at 5 lives
                            if (lives < 5)
                            {
                                lives++;
                            }
                            break;
                        case "MultiBall":
                            //creates a new ball foreach ball currently on screen
                            int ballCount = balls.Count;
                            if (ballCount >= 2)
                            {
                                ballCount = 1;
                            }
                            for (int i = 0; i < ballCount; i++)
                            {
                                Ball newBall = new Ball(balls[i].x, balls[i].y, balls[i].xSpeed * -1, balls[i].ySpeed, ball.size);
                                balls.Add(newBall);
                            }

                            break;
                        case "ExtendPaddle":
                            //extends paddle
                            if (extendTimer.IsRunning == true)
                            {
                                extendTimer.Restart();
                            }
                            else
                            {
                                extendTimer.Start();
                                extendBool = true;

                                paddle.width += 40;
                                paddle.x -= 20;
                                rc_car.X = paddle.x;
                                rc_car.Width = paddle.width;
                            }
                            break;
                    }
                    //delete the powerBall
                    powerList.Remove(p);
                    break;
                }
                // if powerBall goes offscreen, delete the ball
                if (p.y > this.Height + 50)
                {
                    powerList.Remove(p);
                    break;
                }
                // remove powerBall if it is in pause-mode and reachable by the paddle
                else if (ball.xSpeed == 0 && ball.ySpeed == 0 && p.y > paddle.y - 30)
                {
                    powerList.Remove(p);
                    break;
                }

            }

            //check if duration has run out for each powerup

            //breakThrough
            if (4 < Convert.ToDouble(breakTimer.ElapsedMilliseconds / 1000))
            {
                breakTimer.Reset();
                ballBrush.Color = Color.White;
                breakthroughBool = false;
            }

            //extend powerup
            if (10 < Convert.ToDouble(extendTimer.ElapsedMilliseconds / 1000))
            {
                extendTimer.Reset();
                paddle.width -= 40;
                paddle.x += 20;
                rc_car.X = paddle.x;
                rc_car.Width = paddle.width;
                extendBool = false;
            }

            //gravity powerup
            if (7 < Convert.ToDouble(gravityTimer.ElapsedMilliseconds / 1000))
            {
                gravityTimer.Reset();
                gravityBool = false;
                ballBrush.Color = Color.White;
            }

            //ball animations
            ballWatch.Start();
            if (ballWatch.ElapsedMilliseconds >= 200)
            {
                ballig = Properties.Resources.toy_story_ball_down1;
            }
            if (ballWatch.ElapsedMilliseconds >= 400)
            {
                ballig = Properties.Resources.toy_story_ball_right1;
            }
            if (ballWatch.ElapsedMilliseconds >= 600)
            {
                ballig = Properties.Resources.toy_story_ball_up1;
            }
            if (ballWatch.ElapsedMilliseconds >= 1000)
            {
                ballig = Properties.Resources.toy_story_ball_left1;
                ballWatch.Restart();
            }

            // keep ball above paddle before release
            if (spaceDown == true && ball.xSpeed == 0)
            {
                pressStartLabel.Visible = false;
                //determine which way the ball travels
                if (leftArrowDown == true)
                {
                    //release ball with more left force
                    ball.xSpeed = -8;
                    ball.ySpeed = -8;
                }
                else if (rightArrowDown == true)
                {
                    //release ball with more right force
                    ball.xSpeed = 8;
                    ball.ySpeed = -8;
                }
                else
                {
                    //release ball with more upward force
                    ball.xSpeed = r.Next(3, 6);
                    if (r.Next(1, 3) == 1)
                    {
                        ball.xSpeed *= -1;
                    }
                    ball.ySpeed = r.Next(-8, -5);
                }
            }
            // center ball to paddle at pause
            if (ball.xSpeed == 0 && ball.ySpeed == 0)
            {
                ball.x = ((paddle.x - (ball.size / 2)) + (paddle.width / 2));
                ball.y = (this.Height - paddle.height) - 115;
            }

            //speeding up the ball every 5 seconds
            counter++;
            if (counter % 5000 == 0)
            {
                extraSpeed++;
            }

            if (counter % counterInterval == 0)
            {
                foreach (Block b in blocks)
                {
                    for (int i = 0; i < balls.Count; i++)
                    {
                        if (balls[i].xSpeed == 0 && balls[i].ySpeed == 0)
                        {
                            // do nothing
                        }
                        else
                        {
                            //slowly make blocks fall

                            b.y += 10;

                            if (b.y >= paddle.y)
                            {
                                gameTimer.Enabled = false;
                                OnEnd();
                                break;
                            }
                        }
                    }
                }
            }

            //redraws the screen
            Refresh();
        }

        public void OnEnd()
        {
            breakTimer.Reset();
            gravityTimer.Reset();
            extendTimer.Reset();
            gravityBool = false;
            breakthroughBool = false;
            extendBool = false;

            // Goes to the game over screen
            if (blocks.Count != 0)
            {
                Form form = this.FindForm();
                EndScreen ps = new EndScreen();
                form.Controls.Add(ps);
                ps.Location = new Point((form.Width - ps.Width) / 2, (form.Height - ps.Height) / 2);
                form.Controls.Remove(this);
            }
            else
            {
                Form form = this.FindForm();
                MenuScreen mm = new MenuScreen();
                form.Controls.Add(mm);
                mm.Location = new Point((form.Width - mm.Width) / 2, (form.Height - mm.Height) / 2);
                form.Controls.Remove(this);
            }
        }

        public void GameScreen_Paint(object sender, PaintEventArgs e)
        {
            // Draw paddle (rcCar)
            //draw paddle outline
            //e.Graphics.DrawRectangle(outlineBrush, paddle.x, paddle.y, paddle.width, paddle.height);
            if (leftArrowDown == true)
            {
                if (extendBool == true)
                {
                    paddle.width = 105 + 40;
                }
                else
                {
                    paddle.width = 105;
                }
                paddle.height = 80;
                e.Graphics.DrawImage(rcCarRight, paddle.x, paddle.y, paddle.width, paddle.height);
            }
            else if (rightArrowDown == true)
            {
                if (extendBool == true)
                {
                    paddle.width = 105 + 40;
                }
                else
                {
                    paddle.width = 105;
                }
                paddle.height = 80;
                e.Graphics.DrawImage(rcCarLeft, paddle.x, paddle.y, paddle.width, paddle.height);
            }
            else
            {
                paddle.height = 105 + 40;
                paddle.width = 80;
                e.Graphics.DrawImage(rcCarTop, paddle.x, paddle.y, paddle.width, paddle.height);
            }

            // Draws blocks
            foreach (Block b in blocks)
            {
                if (b.hp == 1)
                {
                    e.Graphics.DrawImage(A, b.x, b.y);
                }
                else if (b.hp == 2)
                {
                    e.Graphics.DrawImage(B, b.x, b.y);
                }
                else if (b.hp == 3)
                {
                    e.Graphics.DrawImage(C, b.x, b.y);
                }
                else if (b.hp == 4)
                {
                    e.Graphics.DrawImage(D, b.x, b.y);
                }
                else if (b.hp == 5)
                {
                    e.Graphics.DrawImage(E, b.x, b.y);
                }
                else if (b.hp > 5)
                {
                    e.Graphics.DrawImage(Empty, b.x, b.y);
                }
                else
                {
                    e.Graphics.FillRectangle(new SolidBrush(Color.Gray), b.x, b.y, b.width, b.height);
                }
            }


            // Draws powerups
            foreach (Powers p in powerList)
            {
                switch (p.type)
                {
                    case "Breakthrough":
                        e.Graphics.DrawImage(breakthroughimage, p.x, p.y, Powers.powerupSize, Powers.powerupSize);
                        break;
                    case "Gravity":
                        e.Graphics.DrawImage(gravityimage, p.x, p.y, Powers.powerupSize, Powers.powerupSize);
                        break;
                    case "Health":
                        e.Graphics.DrawImage(healthimage, p.x, p.y, Powers.powerupSize, Powers.powerupSize);
                        break;
                    case "MultiBall":
                        e.Graphics.DrawImage(multiballimage, p.x, p.y, Powers.powerupSize, Powers.powerupSize);
                        break;
                    case "ExtendPaddle":
                        e.Graphics.DrawImage(extendedpadleimage, p.x, p.y, Powers.powerupSize, Powers.powerupSize);
                        break;
                }
            }

            // Draws balls
            foreach (Ball b in balls)
            {
                e.Graphics.DrawImage(ballig, b.x, Convert.ToInt32(b.y));
            }

            //Draw hearts
            int xVal = 10;
            for (int i = 0; i < lives; i++)
            {
                e.Graphics.DrawImage(ballig, xVal, this.Height - 40, 20, 20);
                xVal += 25;
            }

        }
    }
}