using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObstacleSpawner : MonoBehaviour {

	[SerializeField] private float waitTime;
	[SerializeField] private GameObject obstaclePrefab;
	[SerializeField] private float easyMaxY;
	[SerializeField] private float hardMinY;
	[SerializeField] private float hardMaxY;
	private Queue<LevelPattern> _levelPatternQueue = new Queue<LevelPattern>();
	private LevelPattern _lastLevelPattern;
	private float _lastSpawnY;
	private float tempTime;

	private void Awake()
	{
		_levelPatternQueue.Enqueue(LevelPattern.Easy);
		_levelPatternQueue.Enqueue(LevelPattern.Easy);
		_levelPatternQueue.Enqueue(LevelPattern.Easy);
		_levelPatternQueue.Enqueue(LevelPattern.Hard);
		_levelPatternQueue.Enqueue(LevelPattern.Hard);
	}

	void Start(){
		tempTime = waitTime - Time.deltaTime;
	}

	void LateUpdate () {
		if(GameManager.Instance.GameState()){
			tempTime += Time.deltaTime;
			if(tempTime > waitTime){
				// Wait for some time, create an obstacle, then set wait time to 0 and start again
				tempTime = 0;
				_lastLevelPattern = _levelPatternQueue.Dequeue();
				_levelPatternQueue.Enqueue(_lastLevelPattern);

				float rand = 0;
				if (_lastLevelPattern == LevelPattern.Easy)
				{
					rand = Random.Range(-easyMaxY/2, easyMaxY/2);
					_lastSpawnY += rand;
				}
				else
				{
					// rand never less than zero
					rand = Random.Range(hardMinY, hardMaxY);
					if (_lastSpawnY > (MAX_PIPE_Y + MIN_PIPE_Y) / 2)
					{
						_lastSpawnY -= rand;
					}
					else
					{
						_lastSpawnY += rand;
					}
				}

				_lastSpawnY = Mathf.Clamp(_lastSpawnY,MIN_PIPE_Y, MAX_PIPE_Y);
				GameObject pipeClone = Instantiate(obstaclePrefab, transform.position, transform.rotation);
				pipeClone.transform.position += _lastSpawnY * Vector3.up;
			}
		}
	}

	void OnTriggerEnter2D(Collider2D col){
		if(col.gameObject.transform.parent != null){
			Destroy(col.gameObject.transform.parent.gameObject);
		}else{
			Destroy(col.gameObject);
		}
	}

	private const float MIN_PIPE_Y = -2.5f;
	private const float MAX_PIPE_Y = 1.5f;

	private enum LevelPattern
	{
		Easy,
		Hard
	}

}
