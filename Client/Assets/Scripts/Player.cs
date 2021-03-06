using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Player
{
	public Vector2		crt_dir; // (1, 0), (-1, 0), (0, 1), (0, -1)
	public Vector2		next_dir;
	public Vector2		pos;
	public Vector2		turn_point;

	public int			id;
	public float		speed;
	public bool			is_hyde;
	public string		name;
	public int			points;

	public GameObject		obj;
	public SpriteRenderer	renderer;
	public MoveAnimation	moveAnimation;

	public static Vector2 RIGHT = new Vector2(1, 0);
	public static Vector2 LEFT	= new Vector2(-1, 0);
	public static Vector2 DOWN	= new Vector2(0, -1);
	public static Vector2 UP	= new Vector2(0, 1);
	public static Vector2 NONE	= new Vector2(0, 0);

	public const float JEKYLL_SPEED = 3.0f;
	public const float HYDE_SPEED = JEKYLL_SPEED * 2;

	public const float JEKYLL_SPEED_ANIM = 0.05f;
	public const float HYDE_SPEED_ANIM = 0.01f;

	public static string HYDE_SPRITE_SHEET;
	public string spriteSheet;

	public static int[][] labyrinth;
	public static float TILE_SIZE;

    public Text scoreLabel = null;

	const int WALL = 1;
	const int PATH = 0;


	public Player()
	{
		crt_dir = NONE;
		next_dir = NONE;
		speed = JEKYLL_SPEED;
		is_hyde = false;
	}

	public void createGameObject()
	{
		obj = new GameObject("Player" + name + id);
		renderer = obj.AddComponent<SpriteRenderer>();
		moveAnimation = obj.AddComponent<MoveAnimation>();
		if (!is_hyde)
		{
			speed = JEKYLL_SPEED;
			moveAnimation.LoadAnimations(spriteSheet);
			moveAnimation.SetSpeed(JEKYLL_SPEED_ANIM);
		}
		else
		{
			speed = HYDE_SPEED;
			moveAnimation.LoadAnimations(HYDE_SPRITE_SHEET);
			moveAnimation.SetSpeed(HYDE_SPEED_ANIM);
		}


		moveAnimation.ChangeAnimation(MoveAnimation.AnimationType.IDLE_FRONT);
		obj.transform.localScale = new Vector3(3.9f, 3.9f, 1);

		//Debug.Log("=============> " + jekyllSprite.textureRect.width / jekyllSprite.pixelsPerUnit);
		renderer.sortingLayerName = "Player";
	}

	public void ChangeBehavior()
	{
		if (is_hyde)
		{
			// schimba in jekyll
			is_hyde = false;
			speed = JEKYLL_SPEED;
			moveAnimation.LoadAnimations(spriteSheet);
			moveAnimation.SetSpeed(JEKYLL_SPEED_ANIM);
		}
		else
		{
			// schimba in hyde
			is_hyde = true;
			speed = HYDE_SPEED;
			moveAnimation.LoadAnimations(HYDE_SPRITE_SHEET);
			moveAnimation.SetSpeed(HYDE_SPEED_ANIM);
		}

		ChangeAnimation();
	}

	public void Update(float dt)
	{
		if (crt_dir == NONE)
		{
			crt_dir = next_dir;
			update_turning_point();

			ChangeAnimation();
		}

		if (crt_dir == UP		&& pos.y >= turn_point.y ||
			crt_dir == DOWN		&& pos.y <= turn_point.y ||
			crt_dir == RIGHT	&& pos.x >= turn_point.x ||
			crt_dir == LEFT		&& pos.x <= turn_point.x)
		{
			pos = turn_point;
			crt_dir = next_dir;
			update_turning_point();

			ChangeAnimation();
		}

		pos += dt * speed * crt_dir;

		obj.transform.position = pos * TILE_SIZE;
	}

	public void ChangeAnimation()
	{
		if (crt_dir == UP)
			moveAnimation.ChangeAnimation(MoveAnimation.AnimationType.WALK_BACK);
		else if (crt_dir == LEFT)
			moveAnimation.ChangeAnimation(MoveAnimation.AnimationType.WALK_LEFT);
		else if (crt_dir == DOWN)
			moveAnimation.ChangeAnimation(MoveAnimation.AnimationType.WALK_FRONT);
		else if (crt_dir == RIGHT)
			moveAnimation.ChangeAnimation(MoveAnimation.AnimationType.WALK_RIGHT);
		else
			moveAnimation.ChangeAnimation(MoveAnimation.AnimationType.TURN_FRONT);
	}

	private void update_turning_point()
	{
		int startX = get_next_idx(pos.x, crt_dir.x);
		int startY = get_next_idx(pos.y, crt_dir.y);

		if (crt_dir == UP)
		{
			for (int j = startY; j < labyrinth.GetLength(0); j++)
			{
				if (labyrinth[j][startX] == WALL)
				{
					turn_point = new Vector2(startX, j - 1);
					break;
				}
				else if (next_dir.x != 0)
				{
					int tryX = startX + (int)next_dir.x;
					if (labyrinth[j][tryX] == PATH)
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
				if (labyrinth[j][startX] == WALL)
				{
					turn_point = new Vector2(startX, j + 1);
					break;
				}
				else if (next_dir.x != 0)
				{
					int tryX = startX + (int)next_dir.x;
					if (labyrinth[j][tryX] == PATH)
					{
						turn_point = new Vector2(startX, j);
						break;
					}
				}
			}
		}
		if (crt_dir == RIGHT)
		{
			int len = labyrinth[0].Length;
			for (int i = startX; i < len; i++)
			{
				if (labyrinth[startY][i] == WALL)
				{
					turn_point = new Vector2(i - 1, startY);
					break;
				}
				else if (next_dir.y != 0)
				{
					int tryY = startY + (int)next_dir.y;
					if (labyrinth[tryY][i] == PATH)
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
				if (labyrinth[startY][i] == WALL)
				{
					turn_point = new Vector2(i + 1, startY);
					break;
				}
				else if (next_dir.y != 0)
				{
					int tryY = startY + (int)next_dir.y;
					if (labyrinth[tryY][i] == PATH)
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
			return (int)Mathf.Floor(pos);
		}
		else if (dir > 0)
		{
			return (int)Mathf.Ceil(pos);
		}
		else return (int)pos;
	}

	public void RemoveFromLabyrinth()
	{
		UnityEngine.Object.Destroy(obj);
	}

	public void Respown(int x, int y)
	{
		pos = new Vector2(x, y);
		crt_dir = NONE;
		next_dir = NONE;

		obj.transform.position = pos * TILE_SIZE;
	}
}
