using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : SingletonMonobehaviour<VFXManager>
{
    private WaitForSeconds twoSeconds;
    [SerializeField] private GameObject reapingPrefab = null;
    [SerializeField] private GameObject deciduousLeavesFallingPrefab = null;

    protected override void Awake()
    {
        base.Awake();

        twoSeconds = new WaitForSeconds(2f);
    }


    private void OnDisable()
    {
        EventHandler.HarvestActionEffectEvent -= displayHarvestActionEffect;
    }

    private void OnEnable()
    {
        EventHandler.HarvestActionEffectEvent += displayHarvestActionEffect;
    }

    private IEnumerator DisableHarvestActionEffect(GameObject effectGameObject, WaitForSeconds secondsToWait)
    {
        yield return secondsToWait;
        effectGameObject.SetActive(false);
    }

    private void displayHarvestActionEffect(Vector3 effectPosition, HarvestActionEffect harvestActionEffect)
    {
        switch (harvestActionEffect)
        {
            case HarvestActionEffect.deciduousLeavesFalling:
                GameObject deciduousLeaveFalling = PoolManager.Instance.ReuseObject(deciduousLeavesFallingPrefab, effectPosition, Quaternion.identity);
                deciduousLeaveFalling.SetActive(true);
                StartCoroutine(DisableHarvestActionEffect(deciduousLeaveFalling, twoSeconds));
                break;

            case HarvestActionEffect.reaping:
                GameObject reaping = PoolManager.Instance.ReuseObject(reapingPrefab, effectPosition, Quaternion.identity);
                reaping.SetActive(true);
                StartCoroutine(DisableHarvestActionEffect(reaping, twoSeconds));
                break;

            case HarvestActionEffect.none:
                break;

            default:
                break;
        }
    }

}
