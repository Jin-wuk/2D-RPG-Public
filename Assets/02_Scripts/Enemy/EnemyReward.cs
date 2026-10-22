using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyReward : MonoBehaviour
{
    [Header("보상 설정")]
    [SerializeField] private int expAmount = 10;


    public void GiveRewards()
    {

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            //경험치 지급
            var levelSystem = player.GetComponent<PlayerLevelUpSystem>();
            if (levelSystem != null)
            {
                levelSystem.AddExp(expAmount);
            }

            EnemyDrop dropSystem = GetComponent<EnemyDrop>();
            if(dropSystem != null)
            {
                dropSystem.GiveRewards();
            }
        }
    }

}
