using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DocScopes : Item
{
    public Projectile ignoreWalls;
    MeshRenderer modelRenderer;

    protected override void Awake() {
        base.Awake();
        modelRenderer = transform.Find("DocScopes").GetComponent<MeshRenderer>();
        modelRenderer.enabled = false;
    }

    public override IEnumerator UseItem() {
        if (Time.time > itemCooldownTime) {
            modelRenderer.enabled = true;
            itemController.IsUsingItem(true);
            AudioClip prevPlaying = null;
            if (AudioManager.instance != null) {
                prevPlaying = AudioManager.instance.GetPlaying();
                AudioManager.instance.PlayMusic(MusicManager.instance.gillette, 0.5f);
            }
            Gun equippedGun = GameObject.FindWithTag("Player").GetComponent<GunController>().GetEquipped();
            Projectile prevProjectile = equippedGun.projectile;
            equippedGun.projectile = ignoreWalls;
            yield return new WaitForSeconds(itemDuration);
            equippedGun.projectile = prevProjectile;
            itemController.IsUsingItem(false);
            if (AudioManager.instance != null) {
                AudioManager.instance.PlayMusic(prevPlaying, 0.5f);
            }
            itemCooldownTime = Time.time + itemCooldown;
            modelRenderer.enabled = false;
        }
    }
}
