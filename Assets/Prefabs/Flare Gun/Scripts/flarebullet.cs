using UnityEngine;
using System.Collections;

public class flarebullet : MonoBehaviour {
			

	private Light flarelight;
	private AudioSource flaresound;
	private ParticleSystemRenderer smokepParSystem;
	private bool myCoroutine;
	private float smooth = 2.4f;
	public 	float flareTimer = 9;
	public AudioClip flareBurningSound;


	// Use this for initialization
	void Start () {

		StartCoroutine("flareLightoff");
		
		GetComponent<AudioSource>().PlayOneShot(flareBurningSound);
		flarelight = GetComponent<Light>();
		flaresound = GetComponent<AudioSource>();
		smokepParSystem = GetComponent<ParticleSystemRenderer>();

		
		Destroy(gameObject,flareTimer + 1f);
		
	
	}
	
	// Update is called once per frame
	void Update () {

		
		if (myCoroutine == true)
			
		{
			flarelight.intensity = Random.Range(2f,6.0f);
			
		}else
			
		{
			flarelight.intensity =  Mathf.Lerp(flarelight.intensity,0f,Time.deltaTime * smooth);
			flarelight.range =  Mathf.Lerp(flarelight.range,0f,Time.deltaTime * smooth);			
			flaresound.volume = Mathf.Lerp(flaresound.volume,0f,Time.deltaTime * smooth);
			smokepParSystem.maxParticleSize = Mathf.Lerp(smokepParSystem.maxParticleSize,0f,Time.deltaTime * 5);


		}

			
	}
	
	IEnumerator flareLightoff()
	{
		myCoroutine = true;
		yield return new WaitForSeconds(flareTimer);
		myCoroutine = false;

	}
}
