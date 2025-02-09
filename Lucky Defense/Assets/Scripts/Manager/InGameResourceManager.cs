using TMPro;
using UnityEngine;

public class InGameResourceManager : MonoBehaviour
{
    [SerializeField] private InGameUIManager inGameUIManager;
    [SerializeField] private TMP_Text currCoinInButtonText;
    
    private const int InitialCoin = 240;
    private int currentCoin;

    private const int InitialGem = 0;
    private int currentGem;

    private const int InitialHeroSummonCoinCost = 20;
    public int CurrentHeroSummonCoinCost { get; private set; }
    private const int NextHeroSummonCoinCostOffset = 2;

    public int InitialRareSummonGemCost => 1;
    public int InitialHeroicSummonGemCost => 1;
    public int InitialLegendarySummonGemCost => 2;

    private void Awake()
    {
        currentCoin = InitialCoin;
        
        currentGem = InitialGem;
        
        CurrentHeroSummonCoinCost = InitialHeroSummonCoinCost;
        
        currCoinInButtonText.SetText(InitialHeroSummonCoinCost.ToString());
        inGameUIManager.SetCurrCoinText(currentCoin);
        inGameUIManager.SetCurrGemText(currentGem);
    }

#if UNITY_STANDALONE || UNITY_EDITOR

    public void OnClickShowMeTheMoney()
    {
        AddCoin(450000);
        AddGem(4500);
    }
    
#endif
    
    public void AddCoin(int coinAmount)
    {
        currentCoin += coinAmount;
        
        inGameUIManager.SetCurrCoinText(currentCoin);
    }

    public bool TryUseCoin(int coinAmount)
    {
        if(currentCoin - coinAmount < 0)
            return false;
        
        currentCoin -= coinAmount;
        inGameUIManager.SetCurrCoinText(currentCoin);

        return true;
    }
    
    public void AddGem(int gemAmount)
    {
        currentGem += gemAmount;
        
        inGameUIManager.SetCurrGemText(currentGem);
    }

    public bool TryUseGem(int gemAmount)
    {
        if(currentGem - gemAmount < 0)
            return false;
        
        currentGem -= gemAmount;
        inGameUIManager.SetCurrGemText(currentGem);

        return true;
    }

    public void AddHeroSummonCoinCost()
    {
        CurrentHeroSummonCoinCost += NextHeroSummonCoinCostOffset;
    }
}