using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace TCPServer
{
	public class Player
	{
		public Vector2	crt_dir;	// (1, 0), (-1, 0), (0, 1), (0, -1)
		public Vector2	next_dir;
		public Vector2	pos;
		public Vector2	turn_point;

		public int		id;
		public float	speed;
		public bool		is_hyde;
		public string	name;

		public static int[,] labyrinth;

		public const int WALL = 0;
		public const int PATH = 1;

		public static Vector2 RIGHT	= new Vector2(1, 0);
		public static Vector2 LEFT	= new Vector2(-1, 0);
		public static Vector2 DOWN	= new Vector2(0, -1);
		public static Vector2 UP	= new Vector2(0, 1);
		public static Vector2 NONE	= new Vector2(0, 0);

		public const float JEKYLL_SPEED = 5.0f;
		public const float HYDE_SPEED = JEKYLL_SPEED * 2;


		public Player()
		{
			crt_dir = NONE;
			next_dir = NONE;
			speed = JEKYLL_SPEED;
		}

		public void Update(float dt)
		{
			if (crt_dir == NONE)
			{
				crt_dir = next_dir;
				update_turning_point();
			}

			if (crt_dir == UP		&& pos.Y >= turn_point.Y ||
				crt_dir == DOWN		&& pos.Y <= turn_point.Y ||
				crt_dir == RIGHT	&& pos.X >= turn_point.X ||
				crt_dir == LEFT		&& pos.X <= turn_point.X)
			{
				pos = turn_point;
				crt_dir = next_dir;
				update_turning_point();
			}

			pos += dt * speed * crt_dir;
		}

		public void ChangeDirection(string dir)
		{
			switch (dir)
			{
				case "UP":
					next_dir = UP;
					if (crt_dir == DOWN)
					{
						crt_dir = UP;
					}
					break;


				case "DOWN":
					next_dir = DOWN;
					if (crt_dir == UP)
					{
						crt_dir = DOWN;
					}
					break;


				case "LEFT":
					next_dir = LEFT;
					if (crt_dir == RIGHT)
					{
						crt_dir = LEFT;
					}
					break;


				case "RIGHT":
					next_dir = RIGHT;
					if (crt_dir == LEFT)
					{
						crt_dir = RIGHT;
					}
					break;

				default:
					return;
			}
			
			update_turning_point();
		}

		public void update_turning_point()
		{
			int startX = get_next_idx(pos.X, crt_dir.X);
			int startY = get_next_idx(pos.Y, crt_dir.Y);

			if (crt_dir == UP)
			{
				for (int j = startY; j < labyrinth.GetLength(0); j++)
				{
					if (labyrinth[j, startX] == WALL)
					{
						turn_point = new Vector2(startX, j - 1);
						break;
					}
					else if (next_dir.X != 0)
					{
						int tryX = startX + (int)next_dir.X;
						if (labyrinth[j, tryX] == PATH)
						{
							turn_point = new Vector2(startX, j);
							break;
						}
					}
				}
			}

			if (crt_dir == DOWN)
			{
				for (int j = startY; j >= 0; j--)
				{
					if (labyrinth[j, startX] == WALL)
					{
						turn_point = new Vector2(startX, j + 1);
						break;
					}
					else if (next_dir.X != 0)
					{
						int tryX = startX + (int)next_dir.X;
						if (labyrinth[j, tryX] == PATH)
						{
							turn_point = new Vector2(startX, j);
							break;
						}
					}
				}
			}

			if (crt_dir == RIGHT)
			{
				int len = labyrinth.GetLength(1);
				for (int i = startX; i < len; i++)
				{
					if (labyrinth[startY, i] == WALL)
					{
						turn_point = new Vector2(i - 1, startY);
						break;
					}
					else if (next_dir.Y != 0)
					{
						int tryY = startY + (int)next_dir.Y;
						if (labyrinth[tryY, i] == PATH)
						{
							turn_point = new Vector2(i, startY);
							break;
						}
					}
				}
			}

			if (crt_dir == LEFT)
			{
				for (int i = startX; i >= 0; i--)
				{
					if (labyrinth[startY, i] == WALL)
					{
						turn_point = new Vector2(i + 1, startY);
						break;
					}
					else if (next_dir.Y != 0)
					{
						int tryY = startY + (int)next_dir.Y;
						if (labyrinth[tryY, i] == PATH)
						{
							turn_point = new Vector2(i, startY);
							break;
						}
					}
				}

			}

			if (crt_dir == next_dir)
				next_dir = NONE;
		}

		private int get_next_idx(float pos, float dir)
		{
			if (dir < 0)
			{
				return (int)Math.Floor(pos);
			}
			else if (dir > 0)
			{
				return (int)Math.Ceiling(pos);
			}
			else return (int)pos;
		}
	}
}
