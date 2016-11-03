using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

    Vector2 crt_dir; // (1, 0), (-1, 0), (0, 1), (0, -1)
    Vector2 next_dir;
    Vector2 pos;
    Vector2 turn_point;
    bool is_hyde;

    Vector2 RIGHT, LEFT, DOWN, UP;

    const float jekyll_speed = 10.0f;
    const float hyde_speed = jekyll_speed * 2;

    float speed;

    Sprite jekyllSprite;
    Sprite hydeSprite;

	// Use this for initialization
	void Start () {
	    RIGHT = new Vector2(1, 0);
        LEFT = new Vector2(-1, 0);
        DOWN = new Vector2(0, -1);
        UP = new Vector2(0, 1);

        crt_dir = new Vector2(0, 0);

        speed = jekyll_speed;

	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            crt_dir = UP;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            crt_dir = DOWN;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            crt_dir = LEFT;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            crt_dir = RIGHT;
        }

        Vector2 next_pos = (Vector2)transform.position + Time.deltaTime * speed * crt_dir;

        transform.position = next_pos;
	}

}
