using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    class Player
    {
        public Vector2 crt_dir; // (1, 0), (-1, 0), (0, 1), (0, -1)
        public Vector2 next_dir;
        public Vector2 pos;
        public Vector2 turn_point;
        public bool is_hyde;
        public float speed;
    }

    Player player = new Player();

    Vector2 RIGHT, LEFT, DOWN, UP, NONE;

    const float jekyll_speed = 5.0f;
    const float hyde_speed = jekyll_speed * 2;

    Sprite jekyllSprite;
    Sprite hydeSprite;

    public WorldGeneration gen;

    const int WALL = 1;
    const int PATH = 0;

    float eps = 0.00001f;

	// Use this for initialization
	void Start () {
	    RIGHT = new Vector2(1, 0);
        LEFT = new Vector2(-1, 0);
        DOWN = new Vector2(0, -1);
        UP = new Vector2(0, 1);
        NONE = new Vector2(0, 0);

        player.crt_dir = NONE;
        player.next_dir = NONE;

        player.speed = jekyll_speed;

	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            player.next_dir = UP;
            if (player.crt_dir == DOWN)
            {
                player.crt_dir = UP;
            }

            update_turning_point(player);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            player.next_dir = DOWN;
            if (player.crt_dir == UP)
            {
                player.crt_dir = DOWN;
            }

            update_turning_point(player);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            player.next_dir = LEFT;
            if (player.crt_dir == RIGHT)
            {
                player.crt_dir = LEFT;
            }

            update_turning_point(player);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            player.next_dir = RIGHT;
            if (player.crt_dir == LEFT)
            {
                player.crt_dir = RIGHT;
            }

            update_turning_point(player);
        }

        if (player.crt_dir == NONE)
        {
            player.crt_dir = player.next_dir;
            update_turning_point(player);
        }

        if (player.crt_dir == UP && player.pos.y >= player.turn_point.y || player.crt_dir == DOWN && player.pos.y <= player.turn_point.y
            || player.crt_dir == RIGHT && player.pos.x >= player.turn_point.x || player.crt_dir == LEFT && player.pos.x <= player.turn_point.x)
        {
            player.pos = player.turn_point;
            player.crt_dir = player.next_dir;
            update_turning_point(player);
        }

        player.pos = player.pos + Time.deltaTime * player.speed * player.crt_dir;

        transform.position = player.pos * gen.getTileSize();
	}

    int get_next_idx(float pos, float dir)
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

    void update_turning_point(Player player)
    {
        int[][] labyrinth = gen.getLabyrinth();

        int startX = get_next_idx(player.pos.x, player.crt_dir.x);
        int startY = get_next_idx(player.pos.y, player.crt_dir.y);

        if (player.crt_dir == UP)
        {
            for (int j = startY; j < labyrinth.GetLength(0); j++)
            {
				if (labyrinth [j] [startX] == WALL) {
					player.turn_point = new Vector2 (startX, j - 1);
					break;
                }
                else if (player.next_dir.x != 0)
                {
                    int tryX = startX + (int)player.next_dir.x;
                    if (labyrinth[j][tryX] == PATH)
                    {
                        player.turn_point = new Vector2(startX, j);
                        break;
                    }
                }
            }
        }
        if (player.crt_dir == DOWN)
        {
            for (int j = startY; j >= 0; j--)
            {
                if (labyrinth[j][startX] == WALL)
                {
                    player.turn_point = new Vector2(startX, j + 1);
                    break;
                }
                else if (player.next_dir.x != 0)
                {
                    int tryX = startX + (int)player.next_dir.x;
                    if (labyrinth[j][tryX] == PATH)
                    {
                        player.turn_point = new Vector2(startX, j);
                        break;
                    }
                }
            }
        }
        if (player.crt_dir == RIGHT)
        {
			int len = labyrinth[0].Length;
			for (int i = startX; i < len; i++)
            {
                if (labyrinth[startY][i] == WALL)
                {
                    player.turn_point = new Vector2(i - 1, startY);
                    break;
                }
                else if (player.next_dir.y != 0)
                {
                    int tryY = startY + (int)player.next_dir.y;
                    if (labyrinth[tryY][i] == PATH)
                    {
                        player.turn_point = new Vector2(i, startY);
                        break;
                    }
                }
            }
        }
        if (player.crt_dir == LEFT)
        {
            for (int i = startX; i >= 0; i--)
            {
                if (labyrinth[startY][i] == WALL)
                {
                    player.turn_point = new Vector2(i + 1, startY);
                    break;
                }
                else if (player.next_dir.y != 0)
                {
                    int tryY = startY + (int)player.next_dir.y;
                    if (labyrinth[tryY][i] == PATH)
                    {
                        player.turn_point = new Vector2(i, startY);
                        break;
                    }
                }
            }

        }

        if (player.crt_dir == player.next_dir)
            player.next_dir = NONE;
    }

    public void placeOnTile(int x, int y)
    {
        float tileSize = gen.getTileSize();
		int nl = gen.getLabyrinth().Length;

        player.pos = new Vector2(x, nl - y - 1);
        transform.position = new Vector3(x * tileSize, (nl - y - 1) * tileSize, 0);
    }

}
