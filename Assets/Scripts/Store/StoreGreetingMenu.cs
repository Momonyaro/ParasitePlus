using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreGreetingMenu : MonoBehaviour
{
    public Image background;
    public AnimationCurve lerpCurve;
    public GameObject buyBtn;
    public GameObject sellBtn;
    public GameObject wallet;
    public TextMeshProUGUI walletText;

    //Replace $ with money amount
    private const string WalletText = "WALLET: $|SEK";

    private void Awake()
    {
        background.fillAmount = 0;
        buyBtn.SetActive(false);
        sellBtn.SetActive(false);
        wallet.SetActive(false);
    }

    public void GreetPlayer()
    {
        StartCoroutine(IEGreetings());
    }

    private IEnumerator IEGreetings()
    {
        StoreManager manager = FindObjectOfType<StoreManager>();
        wallet.SetActive(true);
        WriteWalletText(manager.currentSlim.wallet);

        float timer = 0;
        float maxTimer = lerpCurve.keys[lerpCurve.keys.Length - 1].time;

        while (timer < maxTimer)
        {
            background.fillAmount = lerpCurve.Evaluate(timer);

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        background.fillAmount = lerpCurve.Evaluate(maxTimer);

        buyBtn.SetActive(true);
        sellBtn.SetActive(true);

        yield break;
    }

    private void WriteWalletText(int money)
    {
        walletText.text = WalletText.Replace("$", money.ToString());
    }
}
