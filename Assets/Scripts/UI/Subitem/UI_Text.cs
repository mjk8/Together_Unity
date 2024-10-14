using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

public class UI_Text : MonoBehaviour
{
    /// <Summary>
    /// Local String Reference를 입력하면 locale에 맞게 변환
    /// </Summary>
    public async Task<string> GetLocaleString(string description)
    {
        AsyncOperationHandle<StringTable> handle = LocalizationSettings.StringDatabase.GetTableAsync("StringTable");
        await handle.Task;  // Await the completion of the async operation
        StringTable table = handle.Result;
        return table.GetEntry(description).GetLocalizedString();
    }
    
    public void SetString(string description)
    {
        if (description == null)
        {
            gameObject.GetComponent<TMP_Text>().text = "";
            return;
        }

        GetLocaleString(description);
        GetComponent<TMP_Text>().text = description;
    }

}
