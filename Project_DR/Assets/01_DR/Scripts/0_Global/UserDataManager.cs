using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ClearDatas
{
    public ClearData[] clearData;
}
[System.Serializable]

public class ClearData
{
    public string clearMBTI;
    public string clearDate;
}


public class UserDataManager : MonoBehaviour
{
    // DB에서 가져온 유저의 데이터를 관리하는 클래스
    #region 싱글톤 패턴
    public static UserDataManager Instance
    {
        get
        {
            // 만약 싱글톤 변수에 아직 오브젝트가 할당되지 않았다면
            if (m_Instance == null)
            {
                // 씬에서 GameManager 오브젝트를 찾아 할당
                m_Instance = FindObjectOfType<UserDataManager>();
            }
            // 싱글톤 오브젝트를 반환
            return m_Instance;
        }
    }
    private static UserDataManager m_Instance; // 싱글톤이 할당될 static 변수    
    #endregion

    #region 유저 데이터
 
    [Header("User Data")]
    public string PlayerID;

    [Header("PC Data")]
    public float HP;                  // 플레이어 체력
    public float Exp;                 // 플레이어 현재 경험치
    public float Gold;                // 플레이어 현재 골드
    public float ExpIncrease;         // 플레이어 경험치 증가량
    public float GoldIncrease;        // 플레이어 골드 증가량

    [Header("Weapon Data")]

    public float WeaponAtk;          // 공격력
    public float WeaponCriRate;      // 치명타 확률
    public float WeaponCriDamage;    // 치명타 증가율
    public float WeaponAtkRate;      // 공격 속도

    [Header("Skill Data")]
    public int TeraLv;        // 테라드릴 레벨
    public int GrinderLv;     // 드릴연마 레벨
    public int CrashLv;       // 드릴분쇄 레벨
    public int LandingLv;     // 드릴랜딩 레벨

    [Header("Quest Data")]
    public string QuestMain;           // 현재 퀘스트

    [Header("Clear Data")]
    public int ClearCount;         // 클리어 횟수
    public string ClearData;       // Json을 담을 직렬화된 클리어 데이터
    public ClearDatas ClearDatas;  // 클리어 데이터 모음
    #endregion


    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        PlayerDataManager.Update(true);

    }
    public void GetDataToDB()
    {

        PlayerID = PlayerDataManager.PlayerID; 
        HP = PlayerDataManager.HP;
        Gold = PlayerDataManager.Gold;
        Exp = PlayerDataManager.Exp;
        GoldIncrease = PlayerDataManager.GoldIncrease;
        ExpIncrease = PlayerDataManager.ExpIncrease;

        WeaponAtk = PlayerDataManager.WeaponAtk;
        WeaponCriRate = PlayerDataManager.WeaponCriRate;
        WeaponCriDamage = PlayerDataManager.WeaponCriDamage;
        WeaponAtkRate = PlayerDataManager.WeaponAtkRate;

        TeraLv = PlayerDataManager.SkillLevel1;
        GrinderLv = PlayerDataManager.SkillLevel2;
        CrashLv = PlayerDataManager.SkillLevel3;
        LandingLv = PlayerDataManager.SkillLevel4;

        QuestMain = PlayerDataManager.QuestMain;
        ClearCount = PlayerDataManager.ClearCount;

        ClearData = JsonUtility.ToJson(PlayerDataManager.ClearMBTIValue);
        ClearDatas = JsonUtility.FromJson<ClearDatas>(ClearData);
    }

    
    // 클리어 시간을 가져오는 함수
    public string GetCurrentDate()
    {
        return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
    }

}