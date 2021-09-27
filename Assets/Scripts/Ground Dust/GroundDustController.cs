using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDustController : MonoBehaviour {
    public DeleteAfterTimer timerDelete;
    public ParticleSystem[] particles;

    public void SetLifetime(float newLifetime) {
        timerDelete.liveTime = newLifetime;
        foreach (ParticleSystem particleSystem in particles) {
            ParticleSystem.MainModule main = particleSystem.main;
            main.startLifetime = newLifetime;
        }
    }

    public void SetStartAlpha(float alpha) {
        foreach (ParticleSystem particleSystem in particles) {
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
            colorOverLifetime.color.gradient.alphaKeys[0].alpha = alpha;
        }
    }
}
