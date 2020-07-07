using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WineGlass : Item
{
    List<Projectile> projectileList = new List<Projectile>();

    public override IEnumerator UseItem() {
        if (Time.time > itemCooldownTime) {
            itemController.IsUsingItem(true);
            AudioClip prevPlaying = null;
            if (AudioManager.instance != null) {
                prevPlaying = AudioManager.instance.GetPlaying();
                AudioManager.instance.PlayMusic(MusicManager.instance.wineGlass, 0.1f);
            }

            Gun equippedGun = GameObject.FindWithTag("Player").GetComponent<GunController>().GetEquipped();
            equippedGun.OnShoot += AddFreezeProjectile;

            Projectile[] allProjectiles = FindObjectsOfType<Projectile>();
            foreach (Projectile projectile in allProjectiles) {
                projectile.enabled = false;
                if (projectile.gameObject.GetComponent<Rigidbody>() != null) {
                    projectile.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                }
            }

            GameObject[] allAnimators = GameObject.FindGameObjectsWithTag("EnemyAnimator");
            foreach (GameObject animator in allAnimators) {
                animator.GetComponent<Animator>().enabled = false;
            }

            GameObject[] allParticleSystems = GameObject.FindGameObjectsWithTag("EnemyEffect");
            foreach (GameObject particleSystem in allParticleSystems) {
                particleSystem.GetComponent<ParticleSystem>().Pause();
            }

            Enemy[] allEnemies = FindObjectsOfType<Enemy>();
            

            float itemTimer = 0;
            while (itemTimer < itemDuration) {
                allEnemies = FindObjectsOfType<Enemy>();
                foreach (Enemy enemy in allEnemies) {
                    enemy.enabled = false;
                    enemy.agent.enabled = false;
                    enemy.StopAllCoroutines();
                }
                itemTimer += Time.deltaTime;
                yield return null;
            }

            equippedGun.OnShoot -= AddFreezeProjectile;
            foreach (Projectile projectile in projectileList) {
                if (projectile != null) {
                    projectile.enabled = true;
                    if (projectile.gameObject.GetComponent<Rigidbody>() != null) {
                        projectile.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                    }
                }
            }
            foreach (Projectile projectile in allProjectiles) {
                if (projectile != null) {
                    projectile.enabled = true;
                    if (projectile.gameObject.GetComponent<Rigidbody>() != null) {
                        projectile.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                    }
                }
            }
            projectileList.Clear();

            foreach (GameObject particleSystem in allParticleSystems) {
                if (particleSystem != null) {
                    particleSystem.GetComponent<ParticleSystem>().Play();
                }
            }

            foreach (GameObject animator in allAnimators) {
                if (animator != null) {
                    animator.GetComponent<Animator>().enabled = false;
                }
            }

            foreach (Enemy enemy in allEnemies) {
                if (enemy != null) {
                    enemy.SetNextAttackTime();
                    enemy.enabled = true;
                    enemy.agent.enabled = true;
                    enemy.RestartPath();
                }
            }
            if (AudioManager.instance != null) {
                AudioManager.instance.PlayMusic(prevPlaying, 0.5f);
            }
            itemController.IsUsingItem(false);
            itemCooldownTime = Time.time + itemCooldown;
        }
    }

    public void AddFreezeProjectile(Projectile projectile) {
        projectile.enabled = false;
        if (projectile.gameObject.GetComponent<Rigidbody>() != null) {
            projectile.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
        projectileList.Add(projectile);
    }
}
