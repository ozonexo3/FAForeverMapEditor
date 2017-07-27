using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenericPopup : MonoBehaviour {

	public static GenericPopup Current;

	public GameObject Pivot;
	public CanvasGroup Cg;
	public Text TitleText;
	public Text DescriptionText;
	public GameObject CancelBtn;
	public GameObject NoBtn;
	public GameObject YesBtn;

	public Text CancelText;
	public Text NoText;
	public Text YesText;


	public const string key_yes = "Yes";
	public const string key_no = "No";
	public const string key_cancel = "Cancel";

	public enum PopupTypes{
		OneButton, TwoButton, TriButton
	}

	public class Popup
	{
		public PopupTypes PopupType;
		public string Title;
		public string Description;
		public string Yes;
		public string No;
		public string Cancel;

		public System.Action CancelAction;
		public System.Action NoAction;
		public System.Action YesAction;

	}

	private void Awake()
	{
		Current = this;
	}


	static List<Popup> PopupBufor = new List<Popup>();

	public static void RemoveAll()
	{
		PopupBufor = new List<Popup>();
		Current.PopupDisplayed = false;
		Current.Pivot.SetActive(false);
	}

	public static void ShowPopup(PopupTypes PopupType, string Title, string Description, string Yes, System.Action YesAction, string No = "", System.Action NoAction = null, string Cancel = "", System.Action CancelAction = null)
	{
		Popup NewPopup = new Popup();
		NewPopup.PopupType = PopupType;
		NewPopup.Title = Title;
		NewPopup.Description = Description;
		NewPopup.Yes = Yes;
		NewPopup.No = No;
		NewPopup.Cancel = Cancel;

		NewPopup.YesAction = YesAction;
		NewPopup.NoAction = NoAction;
		NewPopup.CancelAction = CancelAction;

		PopupBufor.Add(NewPopup);
		Current.StartPopup();
	}

	void StartPopup()
	{
		if (!PopupDisplayed && PopupBufor.Count > 0)
		{
			PopupDisplayed = true;
			ShowPupup();
		}
	}
	bool PopupDisplayed;
	void ShowPupup()
	{
		if (PopupBufor[0].PopupType == PopupTypes.OneButton)
		{
			YesBtn.SetActive(true);
			NoBtn.SetActive(false);
			CancelBtn.SetActive(false);
		}
		else if (PopupBufor[0].PopupType == PopupTypes.TwoButton)
		{
			YesBtn.SetActive(true);
			NoBtn.SetActive(true);
			CancelBtn.SetActive(false);
		}
		else if (PopupBufor[0].PopupType == PopupTypes.TriButton)
		{
			YesBtn.SetActive(true);
			NoBtn.SetActive(true);
			CancelBtn.SetActive(true);
		}

		TitleText.text = PopupBufor[0].Title;
		DescriptionText.text = PopupBufor[0].Description;

		YesText.text = PopupBufor[0].Yes;
		NoText.text = PopupBufor[0].No;
		CancelText.text = PopupBufor[0].Cancel;
		Pivot.SetActive(true);
		Debug.Log("Show popup");
	}

	void HidePopup()
	{
		Debug.Log("Hide popup");

		PopupDisplayed = false;
		Pivot.SetActive(false);
		PopupBufor.RemoveAt(0);
		StartPopup();
	}


	public void PressYes()
	{
		PopupBufor[0].YesAction();
		HidePopup();
	}

	public void PressNo()
	{
		PopupBufor[0].NoAction();
		HidePopup();
	}

	public void PressCancel()
	{
		PopupBufor[0].CancelAction();
		HidePopup();
	}



}
