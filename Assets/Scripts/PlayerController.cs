using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour {

	[SerializeField] private float jumpSpeed,yAcceleration, minTiltSmooth, maxTiltSmooth, hoverDistance, hoverSpeed,grayScaleSpeed;
	[SerializeField] private GameObject[] models;
	private bool start;
	private float timer, tiltSmooth, y;
	private Rigidbody2D playerRigid;
	private Quaternion downRotation, upRotation;

	private Animator _animator;
	private Renderer _renderer;
	private bool _isDead;
	private static readonly int Grayscale = Shader.PropertyToID("_GrayscaleAmount");

	private float _yVelocity;
	private float _currentAcceleration;

	private void Awake()
	{
		// ensure animator and renderer not null
		//reset color
		SelectRandomModel();
		_renderer.material.SetFloat(Grayscale,0);
	}

	void Start () {
		
		
		
		tiltSmooth = maxTiltSmooth;
		playerRigid = GetComponent<Rigidbody2D> ();
		downRotation = Quaternion.Euler (0, 0, -90);
		upRotation = Quaternion.Euler (0, 0, 35);
	}

	void Update () {
		if (!start) {
			// Hover the player before starting the game
			timer += Time.deltaTime;
			y = hoverDistance * Mathf.Sin (hoverSpeed * timer);
			transform.localPosition = new Vector3 (0, y, 0);
		} else {
			// Rotate downward while falling
			transform.rotation = Quaternion.Lerp (transform.rotation, downRotation, tiltSmooth * Time.deltaTime);
		}
		// Limit the rotation that can occur to the player
		transform.rotation = new Quaternion (transform.rotation.x, transform.rotation.y, Mathf.Clamp (transform.rotation.z, downRotation.z, upRotation.z), transform.rotation.w);
	}

	void LateUpdate () {
		if (GameManager.Instance.GameState ()) {
			if (Input.GetMouseButtonDown (0)) {
				if(!start){
					// This code checks the first tap. After first tap the tutorial image is removed and game starts
					start = true;
					GameManager.Instance.GetReady ();
					_animator.speed = 2;
				}

				_currentAcceleration = yAcceleration;
				tiltSmooth = minTiltSmooth;
				transform.rotation = upRotation;
				_yVelocity = jumpSpeed;
				SoundManager.Instance.PlayTheAudio("Flap");
			}
		}
		if (playerRigid.velocity.y < -1f) {
			// Increase gravity so that downward motion is faster than upward motion
			tiltSmooth = maxTiltSmooth;
			_currentAcceleration = yAcceleration * 2;
		}

		_yVelocity += _currentAcceleration * Time.deltaTime;
		transform.position += _yVelocity * Time.deltaTime * Vector3.up;
	}

	void OnTriggerEnter2D (Collider2D col) {
		if (col.transform.CompareTag ("Score")) {
			Destroy (col.gameObject);
			GameManager.Instance.UpdateScore ();
		} else if (col.transform.CompareTag ("Obstacle")) {
			// Destroy the Obstacles after they reach a certain area on the screen
			foreach (Transform child in col.transform.parent.transform) {
				child.gameObject.GetComponent<BoxCollider2D> ().enabled = false;
			}
			KillPlayer ();
		}
	}

	void OnCollisionEnter2D (Collision2D col) {
		if (col.transform.CompareTag ("Ground"))
		{
			_currentAcceleration = 0;
			_yVelocity = 0;
			KillPlayer ();
			transform.rotation = downRotation;
		}
	}

	public void KillPlayer () {
		GameManager.Instance.EndGame ();
		_yVelocity = 0;
		// Stop the flapping animation
		_animator.enabled = false;
		if (!_isDead)
		{
			// fix duplicated
			_isDead = true;
			StartCoroutine(IEGrayScaleUp());
		}
		
	}

	
	private void SelectRandomModel()
	{
		var rand = Random.Range(0, models.Length);
		foreach (var model in models)
		{
			model.SetActive(false);
		}
		models[rand].SetActive(true);
		_animator = GetComponentInChildren<Animator>(false);
		_renderer = GetComponentInChildren<Renderer>(false);
	}

	private IEnumerator IEGrayScaleUp()
	{
		float grayScaleVolume = 0;
		while (grayScaleVolume < 1)
		{
			grayScaleVolume += Time.deltaTime * grayScaleSpeed;
			_renderer.material.SetFloat(Grayscale,grayScaleVolume);
			yield return new WaitForEndOfFrame();
		}
	}

}