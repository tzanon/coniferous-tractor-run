using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
	public bool debugMode = false;
	public Level level;

	private ChaserControls controls;
	private InputAction moveAction;

	private Rigidbody2D rb;

	public float speed = 6.0f;
	private Vector2 movement;

	private int _numCollectibles = 0;

	public int NumCollectibles { get => _numCollectibles; set => _numCollectibles = value; }

	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();

		controls = new ChaserControls();
		moveAction = controls.ForestChaser.Move;

		moveAction.performed += ctx => movement = ctx.ReadValue<Vector2>().normalized;
		moveAction.canceled += ctx => movement = Vector2.zero;
	}

	private void OnEnable()
	{
		controls.ForestChaser.Enable();
	}

	private void OnDisable()
	{
		controls.ForestChaser.Disable();
	}

	private void FixedUpdate()
	{
		if (movement != Vector2.zero)
		{
			rb.MovePosition(rb.position + speed * movement * Time.fixedDeltaTime);
		}
	}

	private void OnTriggerEnter2D(Collider2D coll)
	{
		if (coll.CompareTag("Collectible"))
		{
			if (debugMode)
				Debug.Log("picked up an apple!");

			_numCollectibles++;

			level.DeleteCollectible(coll.GetComponent<Collectible>());
		}
	}

}
