using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : Gun
{
    public override IEnumerator Ult() {
        //CHANGE TIME TO INCREASE RATE
        if (currentState != State.Ulting && Time.time > ultCooldownTime) {
            currentState = State.Ulting;
            float initialMSBetweenShots = msBetweenShots;
            float initialSpeed = bulletSpeed;
            float initialSpread = spread;
            msBetweenShots = 50;
            bulletSpeed = 75;
            spread = 25;
            float ultTimer = 0;
            unlimitedMag = true;
            while (ultTimer < ultDuration) {
                ultTimer += Time.deltaTime;
                yield return null;
            }
            bulletSpeed = initialSpeed;
            msBetweenShots = initialMSBetweenShots;
            spread = initialSpread;
            unlimitedMag = false;
            currentState = State.Normal;
            ultCooldownTime = Time.time + ultCooldown;
        }
        
    }
}
