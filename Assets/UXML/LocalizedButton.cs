using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;


[UxmlElement]
public partial class LocalizedButton : Button
{
    public static BindingId KeyProperty = nameof(Key);
    
    [UxmlAttribute]
    public string Key;
    
    public LocalizedButton()
    {
        this.schedule.Execute(() =>
        {
            if (!string.IsNullOrEmpty(Key))
            {
                string loc = LocalizationSettings.StringDatabase.GetLocalizedString(Key);
                if (!string.IsNullOrEmpty(loc))
                {
                    text = loc;
                    return;
                }

                text = Key;
            }
        });
    }
    
    public LocalizedButton(string key)
    {
        Key = key;
    }



}

