using System;
using System.CodeDom;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.XPath;

namespace BrickBreaker
{
    public class Ball
    {
        public int x, xSpeed, size;
        public double ySpeed, y;
        public Color colour;

        public static Random rand = new Random();

        public Ball(int _x, double _y, int _xSpeed, double _ySpeed, int _ballSize)
        {
            x = _x;
            y = _y;
            xSpeed = _xSpeed;
            ySpeed = _ySpeed;
            size = _ballSize;

        }

        public void Move()
        {
            x += xSpeed;
            y += ySpeed;

            //ramping up the speed when gravity powerup is active
            if (GameScreen.gravityBool == true)
            {
                ySpeed -= .1;
            }

            //try to remove the possibility of the ball being stuck at the top of the screen
            for (int i = 0; i < GameScreen.balls.Count; i++)
            {
                if (Math.Abs(GameScreen.balls[i].ySpeed) <= 1 && GameScreen.balls[i].y < 100)
                {
                    GameScreen.balls[i].ySpeed = GameScreen.r.Next(-5, -2);
                }
            }
        }
        public bool BlockCollision(Block b)
        {
            //creating temporary rectangles
            Rectangle blockRecLeft = new Rectangle(b.x - 1, b.y, 2, b.height);
            Rectangle blockRecRight = new Rectangle(b.x + b.width - 1, b.y, 2, b.height);
            Rectangle blockRecMiddleBottom = new Rectangle(b.x + 2, b.y + b.height, b.width - 4, 1);
            Rectangle blockRecMiddleTop = new Rectangle(b.x + 2, b.y - 1, b.width - 4, 1);
            Rectangle ballRec = new Rectangle(x, Convert.ToInt32(y), size, size);

            //checking for intersection with each part of the blocks
            if ((ballRec.IntersectsWith(blockRecMiddleBottom) || ballRec.IntersectsWith(blockRecMiddleTop)) && (x > b.x - size + 1 || x < b.x + b.width + size - 1) /* && (y < b.y - size + 1 || y > b.y + b.height + size - 1) */)
            {
                //flipping the velocityy of the ball only if the breakthrough powerup is not active
                if (GameScreen.breakthroughBool == false)
                {
                    ySpeed *= -1;
                }

                return true;
            }
            else if ((ballRec.IntersectsWith(blockRecLeft) || ballRec.IntersectsWith(blockRecRight)) && (y > b.y - size + 1 || y < b.y + b.height + size - 1) /* && (x < b.x - size || x > b.x + b.width + size) */)
            {
                //flipping the velocityy of the ball only if the breakthrough powerup is not active
                if (GameScreen.breakthroughBool == false)
                {
                    xSpeed *= -1;
                }

                return true;
            }

            //returning false if no intersection
            return false;
        }

        public void PaddleCollision(Paddle p, int extraSpeed)
        {
            //creating temporary rectangles
            Rectangle blockRecLeft = new Rectangle(p.x - 2, p.y + 2, 4, p.height - 4);
            Rectangle blockRecRight = new Rectangle(p.x + p.width - 2, p.y - 2, 4, p.height - 4);
            Rectangle blockRecMiddleBottom = new Rectangle(p.x + 2, p.y + p.height, p.width - 4, 1);
            Rectangle blockRecMiddleTop = new Rectangle(p.x + 2, p.y - 1, p.width - 4, 1);
            Rectangle ballRec = new Rectangle(x, Convert.ToInt32(y), size, size);
            Rectangle paddleRec = new Rectangle(p.x, p.y, p.width, p.height);

            int collisonPoint;
            bool checkCollison = false;

            //checking for intersection between the ball and top of the block
            if ((ballRec.IntersectsWith(blockRecMiddleBottom) || ballRec.IntersectsWith(blockRecMiddleTop)) && (x > p.x - size + 1 || x < p.x + p.width + size - 1) /* && (y < p.y - size + 1 || y > p.y + p.height + size - 1) */)
            {
                //resetting the position of the ball to the top of the paddle and checking where it hit the paddle
                collisonPoint = x - p.x;

                //changing the x and y relative to where it hits the paddle (closer to the edge means shallower angle)
                if (collisonPoint < 5 || collisonPoint > 60)
                {
                    if (xSpeed < 0)
                    {
                        xSpeed = -5 - extraSpeed;
                        ySpeed = -2 - extraSpeed;
                    }
                    else
                    {
                        xSpeed = 5 + extraSpeed;
                        ySpeed = -2 - extraSpeed;
                    }
                }
                else if (collisonPoint > 5 && collisonPoint < 50)
                {
                    if (xSpeed < 0)
                    {
                        xSpeed = -4 - extraSpeed;
                        ySpeed = -3 - extraSpeed;
                    }
                    else
                    {
                        xSpeed = 4 + extraSpeed;
                        ySpeed = -3 - extraSpeed;
                    }
                }
            }

            if (x < p.x + 1 && x > p.x + p.width - 1 && y > p.y && y < p.y + p.width)
            {
                checkCollison = true;
            }

            //checking if the ball intersects with the sides of the paddle
            if (((ballRec.IntersectsWith(blockRecLeft) || ballRec.IntersectsWith(blockRecRight)) /*&& (y > p.y - size + 1 || y < p.y + p.height + size - 1))*/ || checkCollison/* && (x < p.x - size || x > p.x + p.width + size) */))
            {
                xSpeed *= -1;
            }

            if (ballRec.IntersectsWith(paddleRec))
            {
                x = p.x + (p.width / 2);
                y = p.y - size;
            }
        }

        public void WallCollision(UserControl UC)
        {
            // Collision with left wall
            if (x <= 0)
            {
                x = 0;
                xSpeed *= -1;
            }
            // Collision with right wall
            if (x >= (UC.Width - size))
            {
                x = UC.Width - size;
                xSpeed *= -1;
            }
            // Collision with top wall
            if (y <= 2)
            {
                //y = 0;
                ySpeed *= -1;
            }
        }

        public bool BottomCollision(UserControl UC)
        {
            Boolean didCollide = false;

            if (y >= UC.Height)
            {
                didCollide = true;
            }

            return didCollide;
        }

    }
}