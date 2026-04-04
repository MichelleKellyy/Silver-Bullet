using UnityEngine;

public class ElectricitySounds : MonoBehaviour
{
    public AudioSource[] zaps;
    public float amount = 0.5f;
    public float length = 0.25f;

    public ParticleSystem ps;
    private ParticleSystem.MinMaxCurve rateCurve;

    private float zapProgress;

    void Start()
    {
        rateCurve = ps.emission.rateOverTime;
    }

    private float time;
    void Update()
    {
        time += Time.deltaTime;

        if (time <= length)
        {
            float t = Mathf.Repeat(ps.time / ps.main.duration, 1f);
            float graphValue = rateCurve.Evaluate(t);

            float soundsPerSecond = graphValue * amount;
            zapProgress += soundsPerSecond * Time.deltaTime;

            while (zapProgress >= 1f)
            {
                zaps[Random.Range(0, zaps.Length)].Play();
                zapProgress -= 1f;
            }
        }
    }
}