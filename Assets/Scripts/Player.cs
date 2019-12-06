using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
	private int numApples;
	
	void Start()
	{
		
	}
	
	void Update()
	{
		
	}
	
	public void Move(InputAction.CallbackContext context)
	{
		Vector2 val = context.ReadValue<Vector2>();
		
	}
}
