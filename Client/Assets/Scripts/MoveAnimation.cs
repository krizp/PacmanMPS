using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MoveAnimation : MonoBehaviour
{
	public enum AnimationType
	{
		IDLE_BACK,
		IDLE_LEFT,
		IDLE_FRONT,
		IDLE_RIGHT,

		WALK_BACK,
		WALK_LEFT,
		WALK_FRONT,
		WALK_RIGHT,

		TURN_FRONT,

		NONE
	}

	List<Sprite> idleBack, idleLeft, idleFront, idleRight;
	List<Sprite> walkBack, walkLeft, walkFront, walkRight;

	List<Sprite> crtAnim;
	AnimationType crtAnimType;

	SpriteRenderer spriteRenderer;

	bool loadedAnimations;
	float speed, savedSpeed;
	float frame;
	int ind;

	const float TURN_SPEED = 0.05f;

	void Awake()
	{
		loadedAnimations = false;
	}
	
	// Use this for initialization
	void Start ()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		
		crtAnimType = AnimationType.NONE;
		
		frame = 0;
		ind = 0;
	}

	// Update is called once per frame
	void Update ()
	{
		if (!loadedAnimations)
			return;


		frame += Time.deltaTime;

		if (frame >= speed)
		{
			frame = 0;

			ind++;
			if ( ind >= crtAnim.Count )
			{
				ind = 0;

				if ( crtAnimType == AnimationType.TURN_FRONT )
				{
					ChangeAnimation(AnimationType.IDLE_FRONT);
					speed = savedSpeed;
				}
			}

			spriteRenderer.sprite = crtAnim[ind];
		}
	}

	public void SetSpeed(float speed)
	{
		this.speed = speed;
		savedSpeed = speed;
	}

	public void LoadAnimations(string spriteSheet)
	{
		//Object[] sprites = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(spriteSheet);
		Object[] sprites = Resources.LoadAll<Sprite>(spriteSheet);
		Debug.Log(sprites.Length);

		idleBack = new List<Sprite>();
		idleBack.Add((Sprite)sprites[60]);

		idleLeft = new List<Sprite>();
		idleLeft.Add((Sprite)sprites[69]);

		idleFront = new List<Sprite>();
		idleFront.Add((Sprite)sprites[78]);

		idleRight = new List<Sprite>();
		idleRight.Add((Sprite)sprites[87]);


		walkBack = new List<Sprite>();
		for (int i = 61; i < 69; i++)
		{
			walkBack.Add((Sprite)sprites[i]);
		}

		walkLeft = new List<Sprite>();
		for (int i = 70; i < 78; i++)
		{
			walkLeft.Add((Sprite)sprites[i]);
		}

		walkFront = new List<Sprite>();
		for (int i = 79; i < 87; i++)
		{
			walkFront.Add((Sprite)sprites[i]);
		}

		walkRight = new List<Sprite>();
		for (int i = 88; i < 96; i++)
		{
			walkRight.Add((Sprite)sprites[i]);
		}

		loadedAnimations = true;
		crtAnimType = AnimationType.NONE;
	}

	public void ChangeAnimation(AnimationType type)
	{
		if (type == crtAnimType)
			return;

		ind = 0;
		frame = speed;

		switch (type)
		{
			case AnimationType.IDLE_BACK:
				crtAnim = idleBack;
				crtAnimType = AnimationType.IDLE_BACK;
				break;

			case AnimationType.IDLE_LEFT:
				crtAnim = idleLeft;
				crtAnimType = AnimationType.IDLE_LEFT;
				break;

			case AnimationType.IDLE_FRONT:
				crtAnim = idleFront;
				crtAnimType = AnimationType.IDLE_FRONT;
				break;

			case AnimationType.IDLE_RIGHT:
				crtAnim = idleRight;
				crtAnimType = AnimationType.IDLE_RIGHT;
				break;

			case AnimationType.WALK_BACK:
				crtAnim = walkBack;
				crtAnimType = AnimationType.WALK_BACK;
				break;

			case AnimationType.WALK_LEFT:
				crtAnim = walkLeft;
				crtAnimType = AnimationType.WALK_LEFT;
				break;

			case AnimationType.WALK_FRONT:
				crtAnim = walkFront;
				crtAnimType = AnimationType.WALK_FRONT;
				break;

			case AnimationType.WALK_RIGHT:
				crtAnim = walkRight;
				crtAnimType = AnimationType.WALK_RIGHT;
				break;

			case AnimationType.TURN_FRONT:
				if (crtAnimType == AnimationType.IDLE_FRONT)
					break;
				
				crtAnim = TurnFront();
				crtAnimType = AnimationType.TURN_FRONT;
				speed = TURN_SPEED;
				break;
		}
	}
		
	List<Sprite> TurnFront()
	{
		List<Sprite> turnSprites = new List<Sprite>();

		switch (crtAnimType)
		{
			case AnimationType.WALK_BACK:
				turnSprites.Add(idleBack[0]);

				int choice = Random.Range(0, 2);
				if (choice == 0)
				{
					turnSprites.Add(walkLeft[0]);
					turnSprites.Add(walkLeft[2]);
				}
				else
				{
					turnSprites.Add(walkRight[0]);
					turnSprites.Add(walkRight[2]);
				}

				turnSprites.Add(idleFront[0]);
				break;

			case AnimationType.WALK_LEFT:
				turnSprites.Add(walkLeft[2]);
				turnSprites.Add(idleFront[0]);
				break;

			case AnimationType.WALK_RIGHT:
				turnSprites.Add(walkRight[2]);
				turnSprites.Add(idleFront[0]);
				break;

			default:
				turnSprites.Add(idleFront[0]);
				break;
		}

		return turnSprites;
	}
}
